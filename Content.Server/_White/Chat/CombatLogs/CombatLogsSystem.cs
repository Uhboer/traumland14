using Content.Server._White.Notice;
using Content.Server.Chat.Managers;
using Content.Server.IdentityManagement;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Player;


namespace Content.Server._White.Chat.CombatLogs;

public sealed class CombatLogsSystem : EntitySystem
{
    //[Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IdentitySystem _identitySystem = default!;
    [Dependency] private readonly NoticeSystem _notice = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageableComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<ProjectileComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<DamageableComponent, HitScanReflectAttemptEvent>(OnHitscan);
    }

    public void OnAttacked(EntityUid entity, DamageableComponent comp, AttackedEvent args)
    {
        if (!TryComp<MobStateComponent>(entity, out var playermobstate) || playermobstate.CurrentState == MobState.Dead)
            return;

        // Get attacker's identity or entity metadata name if there is none
        string attacker;

        if (HasComp<IdentityComponent>(args.User))
        {
            var representation = _identitySystem.GetIdentityRepresentation(args.User);
            if (representation is not null)
                attacker = _identitySystem.GetIdentityName(args.User, representation);
            else
                attacker = Name(args.User);
        }
        else
        {
            attacker = Name(args.User);
        }

        // Get weapon's name
        string weapon = Name(args.Used);
        if (!TryComp<MeleeWeaponComponent>(args.Used, out var weaponComponent))
            return;

        var targetPart = args.TargetPart.ToString();
        if (targetPart is null)
            targetPart = string.Empty;

        if (playermobstate.CurrentState == MobState.Critical) // Cant see shit in crit
        {
            LogMessage(Loc.GetString("combatlogs-crit-hits", ("bodyPart", targetPart)));
            return;
        }

        bool unarmed = weapon == Name(args.User);

        if (args.User == entity)
        {
            if (unarmed)
            {
                LogMessage(Loc.GetString("combatlogs-unarmed-hit-self", ("bodyPart", targetPart)));
                return;
            }
            LogMessage(Loc.GetString("combatlogs-hit-self", ("bodyPart", targetPart), ("weapon", weapon)));
            return;
        }

        if (unarmed)
        {
            LogMessage(Loc.GetString("combatlogs-unarmed-hit", ("attacker", attacker), ("bodyPart", targetPart)));
            return;
        }
        LogMessage(Loc.GetString("combatlogs-hit", ("attacker", attacker), ("bodyPart", targetPart), ("weapon", weapon)));
        return;

        void LogMessage(string message)
        {
            LogToChat(message, entity);
        }
    }

    public void OnProjectileHit(EntityUid entity, ProjectileComponent comp, ProjectileHitEvent args)
    {
        if (!TryComp<MobStateComponent>(args.Target, out var playermobstate) || playermobstate.CurrentState == MobState.Dead)
            return;

        var targetPart = args.TargetPart.ToString();
        if (targetPart is null)
            targetPart = string.Empty;

        string projectile = MetaData(entity).EntityName;
        string message = Loc.GetString("combatlogs-projectile-hit", ("projectile", projectile), ("bodyPart", targetPart));

        if (playermobstate.CurrentState == MobState.Critical) // Cant see shit in crit
            message = Loc.GetString("combatlogs-crit-hits", ("bodyPart", targetPart));

        LogToChat(message, args.Target);
    }

    public void OnHitscan(EntityUid entity, DamageableComponent comp, HitScanReflectAttemptEvent args)
    {
        if (!TryComp<MobStateComponent>(entity, out var playermobstate) || playermobstate.CurrentState == MobState.Dead)
            return;

        var targetPart = args.TargetPart.ToString();
        if (targetPart is null)
            targetPart = string.Empty;

        string message = args.Reflected
            ? Loc.GetString("combatlogs-hitscan-reflect")
            : Loc.GetString("combatlogs-hitscan-hit", ("bodyPart", targetPart));

        LogToChat(message, args.Target);
    }

    public void LogToChat(string message, EntityUid? actor)
    {
        if (actor is null)
            return;

        _notice.SendNoticeMessage(actor.Value,
            message,
            PopupType.SmallCaution);
    }
}
