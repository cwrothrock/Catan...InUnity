using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Graph
{
    public enum Building
    {
        None,
        Settlement,
        City,
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

        public void SetOwner(String owner) { this.owner = owner; }
    }

    public class TileNode : Node
    {
        private List<VertexNode> vertices = new();
        private bool hasRobber = false;

        public TileNode() { }

        public void AddVertex(VertexNode vertex) { vertices.Add(vertex); }

        public List<VertexNode> GetVertices() { return vertices; }

        public bool HasRobber() { return hasRobber; }

        public void SetHasRobber(bool hasRobber) { this.hasRobber = hasRobber; }
    }

    private List<VertexNode> vertices = new();
    private List<EdgeNode> edges = new();
    private Dictionary<Vector3Int, TileNode> tiles = new();

    public Graph() { }

    public void AddVertex(VertexNode vertex) { vertices.Add(vertex); }

    public void AddEdge(EdgeNode edge) { edges.Add(edge); }

    public void AddTile(Vector3Int position, TileNode tile) { tiles[position] = tile; }

    public TileNode GetTileAt(Vector3Int position) { return tiles[position]; }

    public List<VertexNode> GetVertices() { return vertices; }

    public List<EdgeNode> GetEdges() { return edges; }

    public List<TileNode> GetTiles() { return tiles.Values.ToList(); }
}