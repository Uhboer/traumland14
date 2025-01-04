using Content.Server.GameTicking;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.Ghost
{
    [AnyCommand]
    public sealed class Ghost : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public string Command => "ghost";
        public string Description => "Give up on life and become a ghost.";
        public string Help => "ghost";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine("You have no session, you can't ghost.");
                return;
            }

            if (player.Status != SessionStatus.InGame || player.AttachedEntity == null)
                return;

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
            var minds = _entities.System<SharedMindSystem>();
            if (!minds.TryGetMind(player, out var mindId, out var mind))
            {
                mindId = minds.CreateMind(player.UserId);
                mind = _entities.GetComponent<MindComponent>(mindId);
            }

            if (!EntitySystem.Get<GameTicker>().OnGhostAttempt(mindId, true, true, mind))
            {
                shell.WriteLine("You can't ghost right now.");
            }
            */
        }
    }
}
