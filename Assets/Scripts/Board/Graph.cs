using UnityEngine;
using System.Collections.Generic;

public class Graph
{
    public enum Building
    {
        Settlement,
        City,
    }

    public class Node { }

    public class VertexNode : Node
    {
        public List<EdgeNode> edges = new();
        public Building building;
        public string owner;

        public VertexNode() { }

        public void AddEdge(EdgeNode edge)
        {
            edges.Add(edge);
        }
    }

    public class EdgeNode : Node
    {
        public List<VertexNode> vertices = new();
        public bool hasRoad = false;
        public string owner;

        public EdgeNode() { }

        public void AddVertex(VertexNode vertex)
        {
            vertices.Add(vertex);
        }
    }

    public class TileNode : Node
    {
        public List<VertexNode> vertices = new();
        public bool hasRobber = false;

        public TileNode() { }

        public void AddVertex(VertexNode vertex)
        {
            vertices.Add(vertex);
        }
    }

    public List<VertexNode> vertices = new();
    public List<EdgeNode> edges = new();
    public Dictionary<Vector3Int, TileNode> tiles = new();

    public Graph() { }

    public void AddVertex(VertexNode vertex)
    {
        vertices.Add(vertex);
    }

    public void AddEdge(EdgeNode edge)
    {
        edges.Add(edge);
    }

    public void AddTile(Vector3Int position, TileNode tile)
    {
        tiles[position] = tile;
    }

    public TileNode GetTileAt(Vector3Int position)
    {
        return tiles[position];
    }
}