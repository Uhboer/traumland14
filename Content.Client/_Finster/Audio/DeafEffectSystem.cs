using System.Linq;
using Content.Client.GameTicking.Managers;
using Content.Client.Lobby;
using Content.Shared._ERRORGATE.Hearing;
using Content.Shared._Finster.Audio;
using Content.Shared.Audio.Events;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Linguini.Bundle.Errors;
using Robust.Client;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Finster.Audio;

// Part of ContentAudioSystem that is responsible for lobby music playing/stopping and round-end sound-effect.
public sealed partial class DeafEffectSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<AudioComponent> ent, ref ComponentInit args)
    {
        if (ApplyDeaf(ent, ref args))
            return;
    }

    public bool ApplyDeaf(Entity<AudioComponent> ent, ref ComponentInit args)
    {
        if (TryComp<DeafComponent>(_player.LocalEntity, out var comp) && comp.BlockSounds &&
            TryComp<TransformComponent>(ent, out var xformComp) && xformComp.MapUid is not null)
        {
            _audio.SetVolume(ent, -20f, ent.Comp);
            return true;
        }

        return false;
    }
}
