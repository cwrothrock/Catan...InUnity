using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

// Class for Point-Top Hexagonal grid with 6 vertices and edges for each hexagon added to the graph.
public class HexGraph
{
    // Enums
    // ========================================

    // Directions from center of hexagon
    public enum VertexDirection
    {
        North,
        Northeast,
        Southeast,
        South,
        Southwest,
        Northwest,
    }

    public enum EdgeDirection
    {
        Northwest,
        Northeast,
        East,
        Southeast,
        Southwest,
        West,
    }

    // Structs
    // ========================================

    // Position of Hexagonal Tile on 2D grid
    public struct HexPosition
    {
        public Vector3Int gridPosition;
        public Vector3 worldPosition;
        public HexPosition(Vector3Int gridPosition, Vector3 worldPosition)
        {
            this.worldPosition = worldPosition;
            this.gridPosition = gridPosition;
        }
    }

    // Node Classes
    // ========================================

    // Base class for every graph node
    public class Node
    {
        private Vector3 worldPosition;

        public Node(Vector3 worldPosition)
        {
            this.worldPosition = worldPosition;
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            this.worldPosition = worldPosition;
        }

        public Vector3 GetWorldPosition() => worldPosition;
    }

    // Vertex node for each of the 6 hex vertices. Can be shared between 3 Hexes and 3 Edges.
    public class Vertex : Node
    {
        private List<Edge> edges = new();

        public Vertex(Vector3 worldPosition) : base(worldPosition) { }

        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
        }

        public List<Edge> GetEdges() => edges;
    }

    // Edge node for each of the 6 hexagon edges. Can be shared between 2 Hexes.
    public class Edge : Node
    {
        private List<Vertex> vertices = new();

        public Edge(Vector3 worldPosition) : base(worldPosition) { }

        public void AddVertex(Vertex vertex)
        {
            vertices.Add(vertex);
        }

        public List<Vertex> GetVertices() => vertices;
    }

    // Hex node for each hexagon structuring the graph. 
    public class Hex : Node
    {
        private HexPosition position;
        private Dictionary<VertexDirection, Vertex> vertices = new();

        public Hex(HexPosition position) : base(position.worldPosition)
        {
            this.position = position;
        }

        public void AddVertex(VertexDirection direction, Vertex vertex)
        {
            vertices[direction] = vertex;
        }

        public bool HasVertex(VertexDirection direction)
        {
            return vertices.ContainsKey(direction);
        }

        public Vertex GetVertex(VertexDirection direction) => vertices[direction];

        public Dictionary<VertexDirection, Vertex> GetVertices() => vertices;

        public HexPosition GetHexPosition() => position;
    }

    // Class Definition
    // ========================================
    private List<Vertex> vertices = new();
    private List<Edge> edges = new();
    private Dictionary<Vector3Int, Hex> hexes = new();

    public HexGraph(List<HexPosition> hexPositions)
    {
        Reset();
        AddHexes(hexPositions);

        Debug.Log("[HexGraph] # Hexes: " + hexes.Count);
        Debug.Log("[HexGraph] # Vertices: " + vertices.Count);
        Debug.Log("[HexGraph] # Edges: " + edges.Count);
    }

    public void AddHexes(List<HexPosition> hexPositions)
    {
        hexPositions.ForEach(hexPosition => AddHex(hexPosition));
    }

    public void AddHex(HexPosition hexPosition)
    {
        Hex hex = new Hex(hexPosition);

        Dictionary<EdgeDirection, Vector3Int> hexNeighbors = GetNeighbors(hexPosition.gridPosition);

        // Set vertices if nieghboring tile already exists
        foreach (EdgeDirection edgeDirection in Enum.GetValues(typeof(EdgeDirection)))
        {
            if (HasHex(hexNeighbors[edgeDirection]))
            {
                Hex neighbor = GetHex(hexNeighbors[edgeDirection]);
                foreach (var (localVertex, neighborVertex) in EdgeDirectionToSharedVertices[edgeDirection])
                {
                    hex.AddVertex(localVertex, neighbor.GetVertex(neighborVertex));
                }
            }
        }
        // Fill in remaining vertices
        foreach (VertexDirection vertexDirection in Enum.GetValues(typeof(VertexDirection)))
        {
            if (!hex.HasVertex(vertexDirection))
            {
                Vector3 vertexWorldPosition = hexPosition.worldPosition + VertexDirectionToWorldPositionOffset[vertexDirection];
                Vertex vertex = new(vertexWorldPosition);

                foreach (VertexDirection neighborVertexDirection in VertexDirectionToNeighboringDirections[vertexDirection])
                {
                    if (hex.HasVertex(neighborVertexDirection))
                    {
                        Vertex neighborVertex = hex.GetVertex(neighborVertexDirection);

                        Vector3 edgeWorldPosition = (vertex.GetWorldPosition() + neighborVertex.GetWorldPosition()) / 2;
                        Edge edge = new(edgeWorldPosition);

                        edge.AddVertex(vertex);
                        edge.AddVertex(neighborVertex);

                        vertex.AddEdge(edge);
                        neighborVertex.AddEdge(edge);

                        AddEdge(edge);
                    }
                }
                hex.AddVertex(vertexDirection, vertex);
                AddVertex(vertex);
            }
        }

        AddHex(hexPosition.gridPosition, hex);
    }

    public void Reset()
    {
        vertices.Clear();
        edges.Clear();
        hexes.Clear();
    }

    // Getters and Setters
    // ========================================

    public void AddVertex(Vertex vertex)
    {
        vertices.Add(vertex);
    }

    public void AddEdge(Edge edge)
    {
        edges.Add(edge);
    }

    public void AddHex(Vector3Int position, Hex hex)
    {
        hexes[position] = hex;
    }

    public bool HasHex(Vector3Int position)
    {
        return hexes.ContainsKey(position);
    }

    public Hex GetHex(Vector3Int position)
    {
        if (HasHex(position))
        {
            return hexes[position];
        }
        return null;
    }

    public List<Vertex> GetVertices() => vertices;

    public List<Edge> GetEdges() => edges;

    public Dictionary<Vector3Int, Hex> GetHexes() => hexes;

    // Helper functions and variables
    // ========================================

    // A Dictionary mapping of the form A => List<(B, C)>
    // For a neighboring tile at A, the local vertex B is the same as the vertex at C of the neighboring tile
    public static Dictionary<EdgeDirection, List<(VertexDirection, VertexDirection)>> EdgeDirectionToSharedVertices = new() {
            { EdgeDirection.Northwest, new() { ( VertexDirection.Northwest, VertexDirection.South ),
                                            ( VertexDirection.North, VertexDirection.Southeast ) } },
            { EdgeDirection.Northeast, new() { ( VertexDirection.North, VertexDirection.Southwest ),
                                            ( VertexDirection.Northeast, VertexDirection.South ), } },
            { EdgeDirection.West, new() { ( VertexDirection.Northwest, VertexDirection.Northeast ),
                                        ( VertexDirection.Southwest, VertexDirection.Southeast ), } },
            { EdgeDirection.East, new() { ( VertexDirection.Northeast, VertexDirection.Northwest ),
                                        ( VertexDirection.Southeast, VertexDirection.Southwest ), } },
            { EdgeDirection.Southwest, new() { ( VertexDirection.Southwest, VertexDirection.North ),
                                            ( VertexDirection.South, VertexDirection.Northeast ), } },
            { EdgeDirection.Southeast, new() { ( VertexDirection.Southeast, VertexDirection.North ),
                                            ( VertexDirection.South, VertexDirection.Northwest ), } },
        };

    // A Dictionary mapping of the form A => List<(B, C)>
    // The vertex at A for a tile corresponds to the vertex at C of the tile at B
    public static Dictionary<VertexDirection, List<(EdgeDirection, VertexDirection)>> VertexDirectionToSharedVertices = new()
    {
        { VertexDirection.North, new() { ( EdgeDirection.Northwest, VertexDirection.Southeast ),
                                         ( EdgeDirection.Northeast, VertexDirection.Southwest ), } },
        { VertexDirection.Northwest, new() { ( EdgeDirection.Northwest, VertexDirection.South ),
                                             ( EdgeDirection.West, VertexDirection.Northwest ), } },
        { VertexDirection.Northeast, new() { ( EdgeDirection.Northeast, VertexDirection.South ),
                                             ( EdgeDirection.East, VertexDirection.Northwest ), } },
        { VertexDirection.Southwest, new() { ( EdgeDirection.West, VertexDirection.Southeast ),
                                             ( EdgeDirection.Southwest, VertexDirection.North ), } },
        { VertexDirection.Southeast, new() { ( EdgeDirection.East, VertexDirection.Southwest ),
                                             ( EdgeDirection.Southeast, VertexDirection.North ), } },
        { VertexDirection.South, new() { ( EdgeDirection.Southwest, VertexDirection.Northeast ),
                                         ( EdgeDirection.Southeast, VertexDirection.Northwest ), } },
    };

    public static Dictionary<VertexDirection, List<VertexDirection>> VertexDirectionToNeighboringDirections = new()
    {
        { VertexDirection.North, new() { VertexDirection.Northwest, VertexDirection.Northeast } },
        { VertexDirection.Northeast, new() { VertexDirection.North, VertexDirection.Southeast } },
        { VertexDirection.Southeast, new() { VertexDirection.Northeast, VertexDirection.South } },
        { VertexDirection.South, new() { VertexDirection.Southeast, VertexDirection.Southwest } },
        { VertexDirection.Southwest, new() { VertexDirection.South, VertexDirection.Northwest } },
        { VertexDirection.Northwest, new() { VertexDirection.Southwest, VertexDirection.North } },
    };

    public static Dictionary<VertexDirection, Vector3> VertexDirectionToWorldPositionOffset = new()
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

    // If any pair of grid positions are neighbors
    public static bool HasNeighbors(List<Vector3Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (IsNeighbor(list[i], list[j]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Return list of neighboring tile locations
    public static Dictionary<EdgeDirection, Vector3Int> GetNeighbors(Vector3Int v)
    {
        if (v.y % 2 == 0)
        {
            // v is in an even row
            return new Dictionary<EdgeDirection, Vector3Int> {
                // Row above
                { EdgeDirection.Northwest, new Vector3Int(v.x - 1, v.y + 1, v.z) },
                { EdgeDirection.Northeast, new Vector3Int(v.x, v.y + 1, v.z) },
                // Same row
                { EdgeDirection.West, new Vector3Int(v.x - 1, v.y, v.z) },
                { EdgeDirection.East, new Vector3Int(v.x + 1, v.y, v.z) },
                // Row below
                { EdgeDirection.Southwest, new Vector3Int(v.x - 1, v.y - 1, v.z) },
                { EdgeDirection.Southeast, new Vector3Int(v.x, v.y - 1, v.z) },
            };
        }
        else
        {
            // v is in an odd row
            return new Dictionary<EdgeDirection, Vector3Int> {
                // Row above
                { EdgeDirection.Northwest, new Vector3Int(v.x, v.y + 1, v.z) },
                { EdgeDirection.Northeast, new Vector3Int(v.x + 1, v.y + 1, v.z) },
                // Same row
                { EdgeDirection.West, new Vector3Int(v.x - 1, v.y, v.z) },
                { EdgeDirection.East, new Vector3Int(v.x + 1, v.y, v.z) },
                // Row below
                { EdgeDirection.Southwest, new Vector3Int(v.x, v.y - 1, v.z) },
                { EdgeDirection.Southeast, new Vector3Int(v.x + 1, v.y - 1, v.z) },
            };
        }
    }

}