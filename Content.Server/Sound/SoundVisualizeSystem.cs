using Content.Shared.Coordinates;

namespace Content.Server.Sound;

public sealed class SoundVisualizeSystem : EntitySystem
{
    private const string SoundEffect = "EffectSound";

    public void PlaySoundEffect(EntityUid user)
    {
        SpawnAttachedTo(SoundEffect, user.ToCoordinates());
    }
}
