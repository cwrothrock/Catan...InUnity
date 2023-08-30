using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

public class Graph
{
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

    public static List<VertexDirection> GetVertexDirectionNeighbors(VertexDirection vertexDirection)
    {
        return vertexDirection switch
        {
            VertexDirection.North => new() { VertexDirection.Northwest, VertexDirection.Northeast },
            VertexDirection.Northeast => new() { VertexDirection.North, VertexDirection.Southeast },
            VertexDirection.Southeast => new() { VertexDirection.Northeast, VertexDirection.South },
            VertexDirection.South => new() { VertexDirection.Southeast, VertexDirection.Southwest },
            VertexDirection.Southwest => new() { VertexDirection.South, VertexDirection.Northwest },
            VertexDirection.Northwest => new() { VertexDirection.Southwest, VertexDirection.North },
            _ => new() { },
        };
    }

    public static Vector3 GetVertexOffsetVector(VertexDirection vertexDirection)
    {
        return vertexDirection switch
        {
            VertexDirection.North => new Vector3(0, 1f / Mathf.Sqrt(3), 0),
            VertexDirection.Northeast => new Vector3(1f / 2, 1f / (2 * Mathf.Sqrt(3)), 0),
            VertexDirection.Southeast => new Vector3(1f / 2, -1f / (2 * Mathf.Sqrt(3)), 0),
            VertexDirection.South => new Vector3(0, -1f / Mathf.Sqrt(3), 0),
            VertexDirection.Northwest => new Vector3(-1f / 2, 1f / (2 * Mathf.Sqrt(3)), 0),
            VertexDirection.Southwest => new Vector3(-1f / 2, -1f / (2 * Mathf.Sqrt(3)), 0),
            _ => Vector3.zero,
        };
    }

    public void AddVertex(VertexNode vertex) { vertices.Add(vertex); }

    public void AddEdge(EdgeNode edge) { edges.Add(edge); }

    public void AddTile(Vector3Int position, TileNode tile) { tiles[position] = tile; }

    public bool HasTile(Vector3Int position) => tiles.ContainsKey(position);

    public TileNode GetTile(Vector3Int position) => tiles[position];

    public List<VertexNode> GetVertices() => vertices;

    public List<EdgeNode> GetEdges() => edges;

    public List<TileNode> GetTiles() => tiles.Values.ToList();
}