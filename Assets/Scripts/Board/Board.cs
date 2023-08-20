using UnityEngine;
using UnityEngine.Tilemaps;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public class Board : MonoBehaviour
{
    public enum TileType : int
    {
        WATER = 0,
        DESERT = 1,
        BRICK = 2,
        ORE = 3,
        SHEEP = 4,
        WHEAT = 5,
        WOOD = 6,
        PORT = 7,
        LAND = 8,
    }

    public enum PortType : int
    {
        // 4:1
        ANY4 = 0,
        // 3:1
        ANY3 = 1,
        // 2:1
        BRICK = 2,
        ORE = 3,
        SHEEP = 4,
        WHEAT = 5,
        WOOD = 6,
    }

    public abstract class BoardTile
    {
        public Vector3Int position;
        public TileType tileType;

        public BoardTile(Vector3Int position, TileType tileType = TileType.WATER)
        {
            this.position = position;
            this.tileType = tileType;
        }
    }

    public class PortTile : BoardTile
    {
        public PortType portType;
        public PortTile(Vector3Int position, PortType portType = PortType.ANY4)
        : base(position, TileType.PORT)
        {
            this.portType = portType;
        }
    }

    public class TerrainTile : BoardTile
    {
        public int diceNumber;
        public TerrainTile(Vector3Int position, TileType tileType, int diceNumber = 0)
        : base(position, tileType)
        {
            this.diceNumber = diceNumber;
        }
    }

    [SerializeField] private Tilemap landTilemap;
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap docksTilemap;
    [SerializeField] private Tilemap portsTilemap;
    [SerializeField] private Tilemap numbersTilemap;

    [SerializeField] private SerializedDictionary<TileType, Tile> terrainTilesDict;
    [SerializeField] private SerializedDictionary<PortType, Tile> portTilesDict;
    [SerializeField] private SerializedDictionary<int, Tile> numberTilesDict;

    [SerializeField] private TextAsset boardVariantJsonAsset;
    private BoardVariant boardVariant;

    private List<TerrainTile> terrainTiles = new List<TerrainTile>();
    private List<PortTile> portTiles = new List<PortTile>();

    private Graph graph;

    private void Start()
    {
        boardVariant = BoardVariant.From(boardVariantJsonAsset);
        // boardVariant.portPositions = new();
        // boardVariant.landPositions = new();

        GenerateBoard();
        UpdateBoardUI();
    }

    private void GenerateBoard()
    {
        List<TileType> terrainOrder = BoardVariant.Explode(boardVariant.terrainCounts, shuffle: false);
        List<int> diceOrder = BoardVariant.Explode(boardVariant.diceCounts, shuffle: false);

        // Place 6/8s first
        int sixes = diceOrder.RemoveAll(x => x == 6);
        int eights = diceOrder.RemoveAll(x => x == 8);
        int count = sixes + eights;
        List<Vector3Int> candidatePositions;
        do {
            BoardVariant.Shuffle(boardVariant.landPositions);
            candidatePositions = boardVariant.landPositions.Take(count).ToList();
        } while (HasNeighbors(candidatePositions));
        
        List<int> diceTemp = Enumerable.Repeat(6, sixes).ToList();
        diceTemp.AddRange(Enumerable.Repeat(8, eights));
        for (int i = 0; i < count; i++)
        {
            if (terrainOrder[i].Equals(TileType.DESERT))
            {
                AddTerrainTile(candidatePositions[i], terrainOrder[i], 7);
            } else 
            {
                AddTerrainTile(candidatePositions[i], terrainOrder[i], diceTemp[i]);
                diceOrder.RemoveAt(0);
            }
        }

        // Place remaining numbers
        for (int i = count; i < terrainOrder.Count; i++)
        {
            if (terrainOrder[i].Equals(TileType.DESERT))
            {
                AddTerrainTile(boardVariant.landPositions[i], terrainOrder[i], 7);
            } else 
            {
                AddTerrainTile(boardVariant.landPositions[i], terrainOrder[i], diceOrder[0]);
                diceOrder.RemoveAt(0);
            }
        }

        // Place ports
        List<PortType> portOrder = BoardVariant.Explode(boardVariant.portCounts, shuffle: true);
        foreach (Vector3Int pos in boardVariant.portPositions)
        {
            AddPortTile(pos, portOrder[0]);
            portOrder.RemoveAt(0);
        }
    }

    private bool HasNeighbors(List<Vector3Int> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = i+1; j < positions.Count; j++)
            {
                // if (IsNeighbor(positions[i], positions[j]))
                // {
                //     return true;
                // }
            }
        }
        return false;
    }

    private void AddTerrainTile(Vector3Int position, TileType type, int diceNumber)
    {
        terrainTiles.Add(new TerrainTile(position, type, diceNumber));
    }

    public void AddPortTile(Vector3Int position, PortType type)
    {
        portTiles.Add(new PortTile(position, type));
    }

    private void UpdateBoardUI()
    {
        foreach (TerrainTile tile in terrainTiles)
        {
            landTilemap.SetTile(tile.position, terrainTilesDict[TileType.LAND]);
            terrainTilemap.SetTile(tile.position, terrainTilesDict[tile.tileType]);
            if (!tile.tileType.Equals(TileType.DESERT))
            {
                numbersTilemap.SetTile(tile.position, numberTilesDict[tile.diceNumber]);
            }
        }
        foreach (PortTile tile in portTiles)
        {
            portsTilemap.SetTile(tile.position, portTilesDict[tile.portType]);
        }
    }
}
