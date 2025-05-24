using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Content.Shared.CCVar;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Client.Changelog
{
    public sealed partial class ChangelogManager : IPostInjectInit
    {
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly IResourceManager _resource = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IConfigurationManager _configManager = default!;

        private const string SawmillName = "changelog";
        public const string MainChangelogName = "Changelog";

        private ISawmill _sawmill = default!;

        public bool NewChangelogEntries { get; private set; }
        public DateTime LastReadTime { get; private set; }
        public DateTime MaxTime { get; private set; }

        public string BuildNumber { get; private set; } = string.Empty;
        public string BuildCommit { get; private set; } = string.Empty;

        public event Action? NewChangelogEntriesChanged;

        /// <summary>
        ///     Ran when the user opens ("read") the changelog,
        ///     stores the new ID to disk and clears <see cref="NewChangelogEntries"/>.
        /// </summary>
        /// <remarks>
        ///     <see cref="MaxTime"/> is NOT cleared
        ///     since that's used in the changelog menu to show the "since you last read" bar.
        /// </remarks>
        public void SaveNewReadId()
        {
            NewChangelogEntries = false;
            NewChangelogEntriesChanged?.Invoke();

            using var sw =
                _resource.UserData.OpenWriteText(
                    new($"/changelog_last_seen_{_configManager.GetCVar(CCVars.ServerId)}_datetime"));

            sw.Write(MaxTime.ToString("O"));
        }

        public async void Initialize()
        {
            // Open changelog purely to compare to the last viewed date.
            var changelogs = await LoadChangelog();
            UpdateChangelogs(changelogs);
        }

        private void UpdateChangelogs(List<Changelog> changelogs)
        {
            if (changelogs.Count == 0)
            {
                return;
            }

            var mainChangelogs = changelogs.Where(c => c.Name == MainChangelogName).ToArray();
            if (mainChangelogs.Length == 0)
            {
                _sawmill.Error($"No changelog file found in Resources/Changelog with name {MainChangelogName}");
                return;
            }

            var changelog = changelogs[0];
            if (mainChangelogs.Length > 1)
            {
                _sawmill.Error($"More than one file found in Resource/Changelog with name {MainChangelogName}");
            }

            if (changelog.Entries.Count == 0)
            {
                return;
            }

            MaxTime = changelog.Entries.Max(c => c.Time);

            var path = new ResPath($"/changelog_last_seen_{_configManager.GetCVar(CCVars.ServerId)}_datetime");
            if (_resource.UserData.TryReadAllText(path, out var lastReadTimeText))
            {
                if (Regex.IsMatch(lastReadTimeText,
                        @"^([\+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24\:?00)([\.,]\d+(?!:))?)?(\17[0-5]\d([\.,]\d+)?)?([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$"))
                {
                    LastReadTime = DateTime.ParseExact(lastReadTimeText, "O", CultureInfo.InvariantCulture);
                }
            }

            NewChangelogEntries = LastReadTime < MaxTime;

            NewChangelogEntriesChanged?.Invoke();
        }

        public Task<List<Changelog>> LoadChangelog()
        {
            return Task.Run(() =>
            {
                var changelogs = new List<Changelog>();
                var directory = new ResPath("/Changelog");
                foreach (var file in _resource.ContentFindFiles(new ResPath("/Changelog/")))
                {
                    if (file.Directory != directory || file.Extension != "yml")
                        continue;

                    var yamlData = _resource.ContentFileReadYaml(file);

                    if (yamlData.Documents.Count == 0)
                        continue;

                    var node = yamlData.Documents[0].RootNode.ToDataNodeCast<MappingDataNode>();
                    var changelog = _serialization.Read<Changelog>(node, notNullableOverride: true);
                    if (string.IsNullOrWhiteSpace(changelog.Name))
                        changelog.Name = file.FilenameWithoutExtension;

                    changelogs.Add(changelog);
                }

                changelogs.Sort((a, b) => a.Order.CompareTo(b.Order));
                return changelogs;
            });
        }

        /// <summary>
        ///     Tries to return a human-readable version number from the build.json file
        /// </summary>
        public string GetClientVersion()
        {
            if (BuildNumber == string.Empty)
            {
                var buildInfo = GetBuildInfo();
                if (buildInfo is null)
                {
                    BuildNumber = "fent";
                }
                else
                {
                    var days = new int[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 303, 334, 365 };
                    BuildNumber = (365 * (buildInfo.Value.Year - 2020) - 31 + days[buildInfo.Value.Month] + buildInfo.Value.Day).ToString();
                    BuildCommit = buildInfo.Value.Commit;
                }
            }

            if (string.IsNullOrEmpty(BuildNumber) || string.IsNullOrEmpty(BuildCommit))
                return Loc.GetString("changelog-version-unknown");

            return Loc.GetString("changelog-build-info",
                ("build", BuildNumber),
                ("version", BuildCommit));
        }

        public BuildInfo? GetBuildInfo()
        {
            // Parses /manifest.yml for game-specific settings that cannot be exclusively set up by content code.
            if (!_resource.TryContentFileRead("/buildInfo.yml", out var stream))
                return null;

            var yamlStream = new YamlStream();
            using (stream)
            {
                using var streamReader = new StreamReader(stream, EncodingHelpers.UTF8);
                yamlStream.Load(streamReader);
            }

            if (yamlStream.Documents.Count == 0)
                return null;

            if (yamlStream.Documents.Count != 1 || yamlStream.Documents[0].RootNode is not YamlMappingNode mapping)
            {
                return null;
            }

            int year = 2020;
            if (mapping.TryGetNode("year", out var yearNode))
                year = yearNode.AsInt();

            int month = 1;
            if (mapping.TryGetNode("month", out var monthNode))
                month = monthNode.AsInt();

            int day = 1;
            if (mapping.TryGetNode("day", out var dayNode))
                day = dayNode.AsInt();

            string commit = "";
            if (mapping.TryGetNode("commit", out var commitNode))
                commit = commitNode.AsString();

            return new BuildInfo(year, month, day, commit);
        }

        public void PostInject()
        {
            _sawmill = _logManager.GetSawmill(SawmillName);
        }

        [DataDefinition]
        public sealed partial class Changelog
        {
            /// <summary>
            ///     The name to use for this changelog.
            ///     If left unspecified, the name of the file is used instead.
            ///     Used during localization to find the user-displayed name of this changelog.
            /// </summary>
            [DataField("Name")]
            public string Name = string.Empty;

            /// <summary>
            ///     The individual entries in this changelog.
            ///     These are not kept around in memory in the changelog manager.
            /// </summary>
            [DataField("Entries")]
            public List<ChangelogEntry> Entries = new();

            /// <summary>
            ///     Whether or not this changelog will be displayed as a tab to non-admins.
            ///     These are still loaded by all clients, but not shown if they aren't an admin,
            ///     as they do not contain sensitive data and are publicly visible on GitHub.
            /// </summary>
            [DataField("AdminOnly")]
            public bool AdminOnly;

            /// <summary>
            ///     Used when ordering the changelog tabs for the user to see.
            ///     Larger numbers are displayed later, smaller numbers are displayed earlier.
            /// </summary>
            [DataField("Order")]
            public int Order;
        }

        [DataDefinition]
        public sealed partial class ChangelogEntry
        {
            [DataField("id")]
            public int Id { get; private set; }

            [DataField("author")]
            public string Author { get; private set; } = "";

            [DataField]
            public DateTime Time { get; private set; }

            [DataField("changes")]
            public List<ChangelogChange> Changes { get; private set; } = default!;
        }

        [DataDefinition]
        public sealed partial class ChangelogChange
        {
            [DataField("type")]
            public ChangelogLineType Type { get; private set; }

            [DataField("message")]
            public string Message { get; private set; } = "";
        }

        public enum ChangelogLineType
        {
            Add,
            Remove,
            Fix,
            Tweak,
        }
    }

    public struct BuildInfo
    {
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public string Commit { get; }

        public BuildInfo(int year, int month, int day, string commit)
        {
            Year = year;
            Month = month;
            Day = day;
            Commit = commit;
        }
    }
}
