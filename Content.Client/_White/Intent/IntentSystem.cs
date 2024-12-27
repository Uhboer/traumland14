using Content.Client.Hands.Systems;
using Content.Client.NPC.HTN;
using Content.Shared._White.Intent;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client._White.Intent;

public sealed class IntentSystem : SharedIntentSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public event Action<bool>? LocalPlayerIntentUpdated;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntentComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, IntentComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateHud(uid);
    }

    public override void Shutdown()
    {
        base.Shutdown();
    }

    public override void SetIntent(EntityUid entity, Shared._White.Intent.Intent intent = Shared._White.Intent.Intent.Help, IntentComponent? component = null)
    {
        base.SetIntent(entity, intent, component);
        UpdateHud(entity);
    }

    protected override bool IsNpc(EntityUid uid)
    {
        return HasComp<HTNComponent>(uid);
    }

    public bool HasIntents(EntityUid uid)
    {
        return HasComp<IntentComponent>(uid);
    }

    public void LocalToggleIntent(Shared._White.Intent.Intent intent)
    {
        var uid = _playerManager.LocalEntity;

        if (uid == null)
            return;

        if (!TryComp(uid, out IntentComponent? comp))
            return;

        RaiseNetworkEvent(new ToggleNetIntentEvent(intent));
    }

    private void UpdateHud(EntityUid entity)
    {
        if (entity != _playerManager.LocalEntity || !_timing.IsFirstTimePredicted)
            return;

        var inCombatMode = CanAttack();
        LocalPlayerIntentUpdated?.Invoke(inCombatMode);
    }

    public bool CanAttack()
    {
        return CanAttack(_playerManager.LocalEntity);
    }

    public Shared._White.Intent.Intent? GetIntent()
    {
        return GetIntent(_playerManager.LocalEntity);
    }
}
