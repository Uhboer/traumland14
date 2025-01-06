using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.Popups;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Chat.Commands
{
    [AnyCommand]
    internal sealed class SuicideCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public string Command => "suicide";

        public string Description => Loc.GetString("suicide-command-description");

        public string Help => Loc.GetString("suicide-command-help-text");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is not { } player)
            {
                shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
                return;
            }

            if (player.Status != SessionStatus.InGame || player.AttachedEntity == null)
                return;

            var minds = _entityManager.System<SharedMindSystem>();

            // This check also proves mind not-null for at the end when the mob is ghosted.
            if (!minds.TryGetMind(player, out var mindId, out var mindComp) ||
                mindComp.OwnedEntity is not { Valid: true } victim)
            {
                shell.WriteLine(Loc.GetString("suicide-command-no-mind"));
                return;
            }

            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            var random = IoCManager.Resolve<IRobustRandom>();
            var audioSystem = EntitySystem.Get<SharedAudioSystem>();

            if (protoMan.TryIndex<SoundCollectionPrototype>("SuicideTrying", out var proto))
            {
                var sound = new SoundPathSpecifier(proto.PickFiles[random.Next(proto.PickFiles.Count)]);
                var filter = Filter.Empty();
                filter.AddPlayer(player);
                audioSystem.PlayEntity(sound, filter, (EntityUid) player.AttachedEntity, false, AudioParams.Default.WithVolume(-0.5f));
                return;
            }

            /*
        var coordinates = new EntityCoordinates(ourXform.MapUid.Value, args.WorldPoint);
        var volume = MathF.Min(10f, 1f * MathF.Pow(jungleDiff, 0.5f) - 5f);
        var audioParams = AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(volume);

        _audio.PlayPvs(_shuttleImpactSound, coordinates, audioParams);
            */

            /*
            var gameTicker = EntitySystem.Get<GameTicker>();
            var suicideSystem = EntitySystem.Get<SuicideSystem>();
            if (suicideSystem.Suicide(victim))
            {
                var deniedMessage = Loc.GetString("suicide-command-denied");
                shell.WriteLine(deniedMessage);
                _entityManager.System<PopupSystem>()
                    .PopupEntity(deniedMessage, victim, victim);
                return;
            }

            if (suicideSystem.Suicide(victim))
                return;

            shell.WriteLine(Loc.GetString("ghost-command-denied"));
            */
        }
    }
}
