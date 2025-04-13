using Content.KayMisaZlevels.Server.Systems;
using Content.KayMisaZlevels.Shared.Miscellaneous;
using Content.KayMisaZlevels.Shared.Systems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Power.Nodes;

[DataDefinition]
public sealed partial class CableVerticalNode : Node
{
    public override IEnumerable<Node> GetReachableNodes(TransformComponent xform,
            EntityQuery<NodeContainerComponent> nodeQuery,
            EntityQuery<TransformComponent> xformQuery,
            MapGridComponent? grid,
            IEntityManager entMan)
    {
        if (!xform.Anchored || grid == null)
            yield break;

        var gridIndex = grid.TileIndicesFor(xform.Coordinates);
        var nodeList = FindVerticalNodes(xform.Owner, nodeQuery, gridIndex, entMan);
        if (nodeList is null)
            yield break;

        // TODO: Make it works better, instead of that shit.
        var cardinalNodes = GetReachableNodesLikeCable(xform, nodeQuery, xformQuery, grid, entMan, gridIndex);
        if (cardinalNodes is not null)
        {
            foreach (var node in cardinalNodes)
            {
                nodeList.Add(node);
            }
        }

        foreach (var node in nodeList)
        {
            if (node != this)
                yield return node;
        }
    }

    // TODO: It should work from CableNode, but i dont have ant strength to refactor to use the original cabel code.
    // So, just copypaste and do the save like CableNode, but with vertical cables
    private IEnumerable<Node> GetReachableNodesLikeCable(TransformComponent xform,
            EntityQuery<NodeContainerComponent> nodeQuery,
            EntityQuery<TransformComponent> xformQuery,
            MapGridComponent grid,
            IEntityManager entMan,
            Vector2i gridIndex)
    {
        var terminalDirs = 0;
        List<(Direction, Node)> nodeDirs = new();

        foreach (var (dir, node) in NodeHelpers.GetCardinalNeighborNodes(nodeQuery, grid, gridIndex))
        {
            if (node is CableNode && node != this)
            {
                nodeDirs.Add((dir, node));
            }

            if (node is CableVerticalNode && node != this)
            {
                nodeDirs.Add((dir, node));
            }

            if (node is CableDeviceNode && dir == Direction.Invalid)
            {
                // device on same tile
                nodeDirs.Add((Direction.Invalid, node));
            }

            if (node is CableTerminalNode)
            {
                if (dir == Direction.Invalid)
                {
                    // On own tile, block direction it faces
                    terminalDirs |= 1 << (int) xformQuery.GetComponent(node.Owner).LocalRotation.GetCardinalDir();
                }
                else
                {
                    var terminalDir = xformQuery.GetComponent(node.Owner).LocalRotation.GetCardinalDir();
                    if (terminalDir.GetOpposite() == dir)
                    {
                        // Target tile has a terminal towards us, block the direction.
                        terminalDirs |= 1 << (int) dir;
                        break;
                    }
                }
            }
        }

        foreach (var (dir, node) in nodeDirs)
        {
            // If there is a wire terminal connecting across this direction, skip the node.
            if (dir != Direction.Invalid && (terminalDirs & (1 << (int) dir)) != 0)
                continue;

            yield return node;
        }
    }

    private List<Node>? FindVerticalNodes(
            EntityUid ent,
            EntityQuery<NodeContainerComponent> nodeQuery,
            Vector2i gridIndex,
            IEntityManager entMan)
    {
        List<Node> list = new List<Node>();
        var zPhysics = entMan.System<ZPhysicsSystem>();

        // Find nodes on bottom grid
        if (zPhysics.TryGetTileWithEntity(ent, ZDirection.Down, out var bTile, out var bGrid, out var bMap, recursive: false) &&
            bGrid is not null)
        {
            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, bGrid, gridIndex))
            {
                if (node is not null)
                    list.Add(node);
            }
        }

        // Find nodes on top grid
        if (zPhysics.TryGetTileWithEntity(ent, ZDirection.Up, out var tTile, out var tGrid, out var tMap, recursive: false) &&
            tGrid is not null)
        {
            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, tGrid, gridIndex))
            {
                if (node is not null)
                    list.Add(node);
            }
        }

        if (list.Count > 0)
            return list;
        else
            return null;
    }
}
