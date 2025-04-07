using Content.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Content.Shared.Movement.Systems;
using System.Numerics;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Shared.Movement.Components;
using Content.Shared.Standing;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Components;
using JetBrains.FormatRipper.Elf;
using Content.Shared._Shitmed.Targeting;
using Content.Server.Stunnable;
using Content.Shared.Damage;

namespace Content.Server.Jumping;

public sealed class JumpingSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwSys = default!;
    [Dependency] private readonly PhysicsSystem _phys = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damSystem = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    public override void Initialize()
    {
        base.Initialize();

        /*CommandBinds.Builder
            .Bind(
        ContentKeyFunctions.AltActivateItemInWorld,
        new PointerInputCmdHandler(AltClick)).Register<JumpSystem>();
        */
        SubscribeLocalEvent<JumpingComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<JumpingComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(EntityUid uid, JumpingComponent component, ref StartCollideEvent args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var phys) || !args.OtherFixture.Hard)
            return;

        if (phys.BodyStatus == BodyStatus.InAir)
        {
            DamageSpecifier damage = component.BaseCollideDamage;
            _stun.TryParalyze(uid, TimeSpan.FromSeconds(3), true);
            _damSystem.TryChangeDamage(uid, damage * component.DamageModifier, ignoreResistances: true, targetPart: TargetBodyPart.Head);
            _damSystem.TryChangeDamage(uid, damage * component.DamageModifier, ignoreResistances: true, targetPart: TargetBodyPart.Torso);
            _physics.SetLinearVelocity(uid, new Vector2(0, 0));

            // TODO: Stun time should be calculated by mass contests
            _stun.TryParalyze(args.OtherEntity, TimeSpan.FromSeconds(3), true);
            _damSystem.TryChangeDamage(args.OtherEntity, damage * component.DamageModifier, ignoreResistances: true, targetPart: TargetBodyPart.Head);
            _damSystem.TryChangeDamage(args.OtherEntity, damage * component.DamageModifier, ignoreResistances: true, targetPart: TargetBodyPart.Torso);
        }
    }

    public bool TryJump(EntityUid uid, EntityCoordinates coords, JumpingComponent? jumpComp = null)
    {
        if (Deleted(uid) ||
            !Resolve(uid, ref jumpComp))
            return false;

        var userTransf = Transform(uid);
        if ((userTransf.WorldPosition - coords.Position).Length() > jumpComp.JumpRange)
            return false;

        if ((jumpComp.LastJump != null && _gameTiming.CurTime - jumpComp.LastJump < jumpComp.JumpCooldown)
            || (TryComp<InputMoverComponent>(uid, out var inputMoverComp) && !inputMoverComp.CanMove)
            || (TryComp<StandingStateComponent>(uid, out var standingComp) && standingComp.CurrentState != StandingState.Standing))
            return false;

        //if (_rand.Prob(0.05f))
        //    jumpComp.IsFailed = true;
        _phys.SetLinearVelocity(uid, Vector2.Zero);
        _throwSys.TryThrow(uid, new EntityCoordinates(
                position: SharedMoverController.SnapCoordinatesToTile(coords.Position),
                entityId: coords.EntityId),
                baseThrowSpeed: jumpComp.JumpSpeed);
        jumpComp.LastJump = _gameTiming.CurTime;

        return true;
    }

    /*
    public bool AltClick(ICommonSession? session, EntityCoordinates coords, EntityUid uid)
    {
        if (session == null || session.AttachedEntity == null || !TryComp<JumpingComponent>(session.AttachedEntity, out var jumpComp))
            return false;

        var userTransf = Transform(session.AttachedEntity.Value);
        if ((userTransf.WorldPosition - coords.Position).Length() > jumpComp.JumpRange)
        {
            return false;
        }
        if ((jumpComp.LastJump != null && _gameTiming.CurTime - jumpComp.LastJump < jumpComp.JumpCooldown)
            || (TryComp<InputMoverComponent>(session.AttachedEntity, out var inputMoverComp) && !inputMoverComp.CanMove)
            || (TryComp<StandingStateComponent>(session.AttachedEntity, out var standingComp) && standingComp.CurrentState != StandingState.Standing))
            return false;

        //if (_rand.Prob(0.05f))
        //    jumpComp.IsFailed = true;
        _phys.SetLinearVelocity(session.AttachedEntity.Value, Vector2.Zero);
        _throwSys.TryThrow(session.AttachedEntity.Value, new EntityCoordinates(position: SharedMoverController.SnapCoordinatesToTile(coords.Position), entityId: coords.EntityId), baseThrowSpeed: jumpComp.JumpSpeed);
        jumpComp.LastJump = _gameTiming.CurTime;
        return true;
    }
    */

    private void OnLand(EntityUid uid, JumpingComponent jumpComp, ref LandEvent args)
    {
        //jumpComp.IsFailed = false;
    }
}
