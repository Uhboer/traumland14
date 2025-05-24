using System.Linq;
using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Client.UserInterface.Systems.Inventory.Controls;
using Content.Client.Viewport;
using Content.Shared.CCVar;
using Content.Shared.Maps;
using Content.Shared.Movement.Components;
using Content.Shared.Tag;
using MathNet.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._Finster.Lookup;

public class HUDLookupLabel : HUDLabel
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    public int TextPositionX { get; set; } = 240;
    public LookupAlignment Alignment { get; set; } = LookupAlignment.Top;

    public HUDLookupLabel()
    {
        IoCManager.InjectDependencies(this);

        _cfg.OnValueChanged(CCVars.ShowLookupHint, (toggle) =>
        {
            Visible = toggle;
        }, true);
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var bounds = args.ContentSize;

        Text = string.Empty;

        // First - try to find focused HUD controls, instead find tiles or entity in the world
        EntityUid? hoveredEnt = null;
        var hudBoundsArgs = _vpUIManager.TryFindHUDControl();
        if (hudBoundsArgs is not null && (hudBoundsArgs.Value.IsFocused || hudBoundsArgs.Value.InBounds))
            FindInHUD(hudBoundsArgs);
        else
            hoveredEnt = FindInWorld();

        // Normalaize text
        Text = Text.ToUpper();

        // TODO: Players name color
        var textColor = Color.Gainsboro.WithAlpha(0.25f);
        if (hoveredEnt is not null)
        {
            if (_entManager.TryGetComponent<MobMoverComponent>(hoveredEnt.Value, out var _))
            {
                textColor = Color.Gainsboro.WithAlpha(0.65f);
            }
            else if (_entManager.TryGetComponent<TagComponent>(hoveredEnt.Value, out var tagComp) && tagComp != null)
            {
                var found = tagComp.Tags.FirstOrDefault(x => x == "Wall");
                if (found == default)
                    textColor = Color.Gainsboro.WithAlpha(0.5f);
            }
        }
        Color = textColor;

        // Im lazy. I don't it would be too difficult to change it.
        var targetX = TextPositionX;
        var targetY = Scale - (Scale / 2);
        if (Alignment == LookupAlignment.Bottom)
            targetY = 480 - Scale - (Scale / 2);

        var dimensions = handle.GetDimensions(Font!, Text, 1f);
        TextPosition = new Vector2(targetX, targetY) - new Vector2(dimensions.X / 2, 0);

        base.Draw(args);
    }

    private void FindInHUD(HUDBoundsCheckArgs? args)
    {
        if (args is null)
            return;

        var control = args.Value.FocusedControl;
        if (control is null)
            return;

        switch (control)
        {
            case HUDAlertControl alertControl:
                if (alertControl.Name is not null)
                {
                    Text = Loc.GetString(alertControl.Name);
                    return;
                }
                break;
            case HUDSlotControl slotControl:
                if (slotControl.Entity is not null &&
                    _entManager.TryGetComponent<MetaDataComponent>(slotControl.Entity, out var metaData))
                {
                    Text = Loc.GetString(metaData.EntityName);
                    return;
                }

                Text = Loc.GetString(slotControl.HoverName);
                return;
                break;
            case IHUDDescription descControl:
                Text = descControl.Description;
                return;
        }

        // TODO: Maybe need make something like "FocusName"?
        if (control.Name is not null)
            Text = control.Name;
    }

    private EntityUid? FindInWorld()
    {
        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.ScreenToMap(mouseScreenPos);

        var examineSys = _entManager.System<ExamineSystem>();
        var mapSys = _entManager.System<SharedMapSystem>();

        EntityCoordinates mouseGridPos;
        TileRef? tile = null;

        if (_player.LocalEntity is null)
            return null;
        if (!_entManager.TryGetComponent<TransformComponent>(_player.LocalEntity, out var xformComp))
            return null;

        if (mousePos.MapId == MapId.Nullspace || mousePos.MapId != xformComp.MapID)
            return null;

        var mapUid = _mapManager.GetMapEntityId(xformComp.MapID);
        //var nodePos = _maps.WorldToTile(mapUid, grid, mousePos.Position);

        if (!examineSys.CanExamine(_player.LocalEntity.Value, mousePos))
            return null;

        if (mousePos != MapCoordinates.Nullspace)
        {
            if (_mapManager.TryFindGridAt(mousePos, out var mouseGridUid, out var mouseGrid))
            {
                mouseGridPos = mapSys.MapToGrid(mouseGridUid, mousePos);
                tile = mapSys.GetTileRef(mouseGridUid, mouseGrid, mouseGridPos);
            }
            else
            {
                mouseGridPos = new EntityCoordinates(mapUid, mousePos.Position);
                tile = null;
            }
        }

        var currentState = _stateManager.CurrentState;
        if (currentState is not GameplayStateBase screen)
            return null;

        var entityToClick = screen.GetClickedEntity(mousePos);

        if (entityToClick is not null &&
            _entManager.TryGetComponent<MetaDataComponent>(entityToClick, out var metaComp))
        {
            //if (_examine.CanExamine(_player.LocalEntity.Value, entityToClick.Value))
            Text = metaComp.EntityName;
        }
        else if (tile is not null)
        {
            var tileDef = (ContentTileDefinition) _tileDefManager[tile.Value.Tile.TypeId];
            if (tileDef.ID != ContentTileDefinition.SpaceID)
                Text = $"{Loc.GetString(tileDef.Name)}";
        }

        return entityToClick;
    }
}

public enum LookupAlignment
{
    Top,
    Bottom
}
