using Content.Shared.PlayDice;
using Robust.Client.GameObjects;

namespace Content.Client.PlayDice;

public sealed class PlayDiceSystem : SharedPlayDiceSystem
{
    protected override void UpdateVisuals(EntityUid uid, DiceComponent? die = null)
    {
        if (!Resolve(uid, ref die) || !TryComp(uid, out SpriteComponent? sprite))
            return;

        // TODO maybe just move each diue to its own RSI?
        var state = sprite.LayerGetState(0).Name;
        if (state == null)
            return;

        var prefix = state.Substring(0, state.IndexOf('_'));
        sprite.LayerSetState(0, $"{prefix}_{die.CurrentValue}");
    }
}
