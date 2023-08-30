using System;
using System.Collections.Generic;
using System.Linq;
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

        public Vector3 GetWorldPosition() { return worldPosition; }

        public void SetWorldPosition(Vector3 worldPosition) { this.worldPosition = worldPosition; }
    }

    public class VertexNode : Node
    {
        private List<EdgeNode> edges = new();
        private Building building = Building.None;
        private string owner;

        public VertexNode() { }

        public void AddEdge(EdgeNode edge) { edges.Add(edge); }

        public List<EdgeNode> GetEdges() { return edges; }

        public Building GetBuilding() { return building; }

        public void SetBuilding(Building building) { this.building = building; }

        public string GetOwner() { return owner; }

        public void SetOwner(string owner) { this.owner = owner; }
    }

    public class EdgeNode : Node
    {
        private List<VertexNode> vertices = new();
        private string owner;

        public EdgeNode() { }

        public void AddVertex(VertexNode vertex) { vertices.Add(vertex); }

        public List<VertexNode> GetVertices() { return vertices; }

        public string GetOwner() { return owner; }

        public void SetOwner(string owner) { this.owner = owner; }
    }

    public class TileNode : Node
    {
        private Dictionary<VertexDirection, VertexNode> vertices = new();
        private bool hasRobber = false;

        public TileNode() { }

        public void AddVertex(VertexDirection direction, VertexNode vertex) { vertices[direction] = vertex; }

        public Dictionary<VertexDirection, VertexNode> GetVertices() { return vertices; }

        public VertexNode GetVertex(VertexDirection direction) { return vertices[direction]; }

        public bool HasVertex(VertexDirection direction) { return vertices.ContainsKey(direction); }

        public bool HasRobber() { return hasRobber; }

        public void SetHasRobber(bool hasRobber) { this.hasRobber = hasRobber; }
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

    public void AddVertex(VertexNode vertex) { vertices.Add(vertex); }

    public void AddEdge(EdgeNode edge) { edges.Add(edge); }

    public void AddTile(Vector3Int position, TileNode tile) { tiles[position] = tile; }

    public bool HasTile(Vector3Int position) { return tiles.ContainsKey(position); }

    public TileNode GetTile(Vector3Int position) { return tiles[position]; }

    public List<VertexNode> GetVertices() { return vertices; }

    public List<EdgeNode> GetEdges() { return edges; }

    public List<TileNode> GetTiles() { return tiles.Values.ToList(); }
}