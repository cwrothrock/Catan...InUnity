using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

public class Graph
{
    // Enums
    // ========================================

    public enum Building
    {
        None,
        Settlement,
        City,
    }

    public enum VertexDirection
    {
        North,
        Northeast,
        Southeast,
        South,
        Southwest,
        Northwest,
    }

    public enum TileDirection
    {
        Northwest,
        Northeast,
        East,
        Southeast,
        Southwest,
        West,
    }

    // Node Classes
    // ========================================

    public class Node
    {
        private Vector3 worldPosition;

        public Node(Vector3 worldPosition) { this.worldPosition = worldPosition; }

        public void SetWorldPosition(Vector3 worldPosition) { this.worldPosition = worldPosition; }

        public Vector3 GetWorldPosition() => worldPosition;
    }

    public class VertexNode : Node
    {
        private List<EdgeNode> edges = new();
        private Building building = Building.None;
        private string owner;

        public VertexNode(Vector3 worldPosition) : base(worldPosition) { }

        public void AddEdge(EdgeNode edge) { edges.Add(edge); }

        public void SetBuilding(Building building) { this.building = building; }

        public void SetOwner(string owner) { this.owner = owner; }

        public List<EdgeNode> GetEdges() => edges;

        public Building GetBuilding() => building;

        public string GetOwner() => owner;
    }

    public class EdgeNode : Node
    {
        private List<VertexNode> vertices = new();
        private string owner;

        public EdgeNode(Vector3 worldPosition) : base(worldPosition) { }

        public void AddVertex(VertexNode vertex) { vertices.Add(vertex); }

        public void SetOwner(string owner) { this.owner = owner; }

        public List<VertexNode> GetVertices() => vertices;

        public string GetOwner() => owner;
    }

    public class TileNode : Node
    {
        private Dictionary<VertexDirection, VertexNode> vertices = new();
        private bool hasRobber = false;

        public TileNode(Vector3 worldPosition) : base(worldPosition) { }

        public void AddVertex(VertexDirection direction, VertexNode vertex) { vertices[direction] = vertex; }

        public void SetHasRobber(bool hasRobber) { this.hasRobber = hasRobber; }

        public bool HasRobber() => hasRobber;

        public bool HasVertex(VertexDirection direction) => vertices.ContainsKey(direction);

        public VertexNode GetVertex(VertexDirection direction) => vertices[direction];

        public Dictionary<VertexDirection, VertexNode> GetVertices() => vertices;
    }

    private List<VertexNode> vertices = new();
    private List<EdgeNode> edges = new();
    private Dictionary<Vector3Int, TileNode> tiles = new();

    public Graph() { }


    // Getters and Setters
    // ========================================

    public void AddVertex(VertexNode vertex) { vertices.Add(vertex); }

    public void AddEdge(EdgeNode edge) { edges.Add(edge); }

    public void AddTile(Vector3Int position, TileNode tile) { tiles[position] = tile; }

    public bool HasTile(Vector3Int position) => tiles.ContainsKey(position);

    public TileNode GetTile(Vector3Int position) => tiles[position];

    public List<VertexNode> GetVertices() => vertices;

    public List<EdgeNode> GetEdges() => edges;

    public List<TileNode> GetTiles() => tiles.Values.ToList();


    // Helper functions and variables
    // ========================================

    // A Dictionary mapping of the form A => List<(B, C)>
    // For a neighboring tile at A, the local vertex B is the same as the vertex at C of the neighboring tile
    public static Dictionary<TileDirection, List<(VertexDirection, VertexDirection)>> VertexMappings = new() {
            { TileDirection.Northwest, new() { ( VertexDirection.Northwest, VertexDirection.South ),
                                            ( VertexDirection.North, VertexDirection.Southeast ) } },
            { TileDirection.Northeast, new() { ( VertexDirection.North, VertexDirection.Southwest ),
                                            ( VertexDirection.Northeast, VertexDirection.South ), } },
            { TileDirection.West, new() { ( VertexDirection.Northwest, VertexDirection.Northeast ),
                                        ( VertexDirection.Southwest, VertexDirection.Southeast ), } },
            { TileDirection.East, new() { ( VertexDirection.Northeast, VertexDirection.Northwest ),
                                        ( VertexDirection.Southeast, VertexDirection.Southwest ), } },
            { TileDirection.Southwest, new() { ( VertexDirection.Southwest, VertexDirection.North ),
                                            ( VertexDirection.South, VertexDirection.Northeast ), } },
            { TileDirection.Southeast, new() { ( VertexDirection.Southeast, VertexDirection.North ),
                                            ( VertexDirection.South, VertexDirection.Northwest ), } },
        };

    // A Dictionary mapping of the form A => List<(B, C)>
    // The vertex at A for a tile corresponds to the vertex at C of the tile at B
    public static Dictionary<VertexDirection, List<(TileDirection, VertexDirection)>> TileNeighborsOfVertexDirection = new()
    {
        { VertexDirection.North, new() { ( TileDirection.Northwest, VertexDirection.Southeast ),
                                         ( TileDirection.Northeast, VertexDirection.Southwest ), } },
        { VertexDirection.Northwest, new() { ( TileDirection.Northwest, VertexDirection.South ),
                                             ( TileDirection.West, VertexDirection.Northwest ), } },
        { VertexDirection.Northeast, new() { ( TileDirection.Northeast, VertexDirection.South ),
                                             ( TileDirection.East, VertexDirection.Northwest ), } },
        { VertexDirection.Southwest, new() { ( TileDirection.West, VertexDirection.Southeast ),
                                             ( TileDirection.Southwest, VertexDirection.North ), } },
        { VertexDirection.Southeast, new() { ( TileDirection.East, VertexDirection.Southwest ),
                                             ( TileDirection.Southeast, VertexDirection.North ), } },
        { VertexDirection.South, new() { ( TileDirection.Southwest, VertexDirection.Northeast ),
                                         ( TileDirection.Southeast, VertexDirection.Northwest ), } },
    };

    public static Dictionary<VertexDirection, List<VertexDirection>> GetVertexDirectionNeighbors = new()
    {
        { VertexDirection.North, new() { VertexDirection.Northwest, VertexDirection.Northeast } },
        { VertexDirection.Northeast, new() { VertexDirection.North, VertexDirection.Southeast } },
        { VertexDirection.Southeast, new() { VertexDirection.Northeast, VertexDirection.South } },
        { VertexDirection.South, new() { VertexDirection.Southeast, VertexDirection.Southwest } },
        { VertexDirection.Southwest, new() { VertexDirection.South, VertexDirection.Northwest } },
        { VertexDirection.Northwest, new() { VertexDirection.Southwest, VertexDirection.North } },
    };

    public static Dictionary<VertexDirection, Vector3> GetVertexOffsetVector = new()
    {
        { VertexDirection.North, new Vector3(0, 1f / Mathf.Sqrt(3), 0) },
        { VertexDirection.Northeast, new Vector3(1f / 2, 1f / (2 * Mathf.Sqrt(3)), 0) },
        { VertexDirection.Southeast, new Vector3(1f / 2, -1f / (2 * Mathf.Sqrt(3)), 0) },
        { VertexDirection.South, new Vector3(0, -1f / Mathf.Sqrt(3), 0) },
        { VertexDirection.Northwest, new Vector3(-1f / 2, 1f / (2 * Mathf.Sqrt(3)), 0) },
        { VertexDirection.Southwest, new Vector3(-1f / 2, -1f / (2 * Mathf.Sqrt(3)), 0) },
    };

    // Determine if two tile locations are neighbors ona tilemap
    public static bool IsNeighbor(Vector3Int v, Vector3Int w)
    {
        // Return false if either x or y difference is greater than 1 (i.e. more than one tile away)
        if (Mathf.Abs(v.x - w.x) > 1 || Mathf.Abs(v.y - w.y) > 1)
        {
            return false;
        }
        // x value difference is either 0 or 1. If y values are the same, they are on the same row, and therefore neighbors
        if (v.y == w.y)
        {
            return true;
        }
        if (v.y % 2 == 0)
        {
            // v is in even row
            return w.x <= v.x;
        }
        else
        {
            // v is in odd row
            return w.x >= v.x;
        }
    }

    // Return list of neighboring tile locations
    public static Dictionary<TileDirection, Vector3Int> GetNeighbors(Vector3Int v)
    {
        if (v.y % 2 == 0)
        {
            // v is in an even row
            return new Dictionary<TileDirection, Vector3Int> {
                // Row above
                { TileDirection.Northwest, new Vector3Int(v.x - 1, v.y + 1, v.z) },
                { TileDirection.Northeast, new Vector3Int(v.x, v.y + 1, v.z) },
                // Same row
                { TileDirection.West, new Vector3Int(v.x - 1, v.y, v.z) },
                { TileDirection.East, new Vector3Int(v.x + 1, v.y, v.z) },
                // Row below
                { TileDirection.Southwest, new Vector3Int(v.x - 1, v.y - 1, v.z) },
                { TileDirection.Southeast, new Vector3Int(v.x, v.y - 1, v.z) },
            };
        }
        else
        {
            // v is in an odd row
            return new Dictionary<TileDirection, Vector3Int> {
                // Row above
                { TileDirection.Northwest, new Vector3Int(v.x, v.y + 1, v.z) },
                { TileDirection.Northeast, new Vector3Int(v.x + 1, v.y + 1, v.z) },
                // Same row
                { TileDirection.West, new Vector3Int(v.x - 1, v.y, v.z) },
                { TileDirection.East, new Vector3Int(v.x + 1, v.y, v.z) },
                // Row below
                { TileDirection.Southwest, new Vector3Int(v.x, v.y - 1, v.z) },
                { TileDirection.Southeast, new Vector3Int(v.x + 1, v.y - 1, v.z) },
            };
        }
    }

    public void GenerateGraph(List<(Vector3Int cell, Vector3 world)> tilePositions)
    {
        foreach ((Vector3Int cellPosition, Vector3 worldPosition) in tilePositions)
        {
            TileNode tileNode = new(worldPosition);
            Debug.Log("Created TileNode at: " + worldPosition);

            Dictionary<TileDirection, Vector3Int> neighbors = GetNeighbors(cellPosition);

            // Set vertices if nieghboring tile already exists
            foreach (TileDirection tileDirection in Enum.GetValues(typeof(TileDirection)))
            {
                if (HasTile(neighbors[tileDirection]))
                {
                    TileNode neighbor = GetTile(neighbors[tileDirection]);
                    foreach (var (here, there) in VertexMappings[tileDirection])
                    {
                        tileNode.AddVertex(here, neighbor.GetVertex(there));
                    }
                }
            }
            // else, create new vertex
            foreach (VertexDirection vertexDirection in Enum.GetValues(typeof(VertexDirection)))
            {
                if (!tileNode.HasVertex(vertexDirection))
                {
                    Vector3 vertexWorldPosition = worldPosition + GetVertexOffsetVector[vertexDirection];
                    VertexNode vertexNode = new(vertexWorldPosition);
                    Debug.Log("Created " + vertexDirection + " VertexNode at: " + vertexWorldPosition);

                    foreach (VertexDirection neighborVertexDirection in GetVertexDirectionNeighbors[vertexDirection])
                    {
                        if (tileNode.HasVertex(neighborVertexDirection))
                        {
                            VertexNode neighborVertexNode = tileNode.GetVertex(neighborVertexDirection);

                            Vector3 edgeWorldPosition = (vertexNode.GetWorldPosition() + neighborVertexNode.GetWorldPosition()) / 2;
                            EdgeNode edgeNode = new(edgeWorldPosition);
                            Debug.Log("Created EdgeNode at: " + edgeWorldPosition);

                            edgeNode.AddVertex(vertexNode);
                            edgeNode.AddVertex(neighborVertexNode);

                            vertexNode.AddEdge(edgeNode);
                            neighborVertexNode.AddEdge(edgeNode);

                            AddEdge(edgeNode);
                        }
                    }
                    tileNode.AddVertex(vertexDirection, vertexNode);
                    AddVertex(vertexNode);
                }
            }

            AddTile(cellPosition, tileNode);
        }

        Debug.Log("[GenerateGraph] # TileNodes: " + GetTiles().Count);
        Debug.Log("[GenerateGraph] # VertexNodes: " + GetVertices().Count);
        Debug.Log("[GenerateGraph] # EdgeNodes: " + GetEdges().Count);
    }
}