using UnityEngine;
using UnityEngine.Tilemaps;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

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

    private List<BoardTile> boardTiles = new List<BoardTile>();

    private Graph graph;

    private void Start()
    {
        // docksTilemap.CompressBounds();
        // landTilemap.CompressBounds();

        boardVariant = BoardVariant.From(boardVariantJsonAsset);
        // boardVariant.portPositions = new();
        // boardVariant.landPositions = new();

        GenerateBoard();
    }

    private void GenerateBoard()
    {
        List<TileType> terrainOrder = BoardVariant.Explode(boardVariant.terrainCounts, shuffle: true);
        List<PortType> portOrder = BoardVariant.Explode(boardVariant.portCounts, shuffle: true);
        List<int> diceOrder = BoardVariant.Explode(boardVariant.diceCounts, shuffle: true);

        foreach (Vector3Int pos in boardVariant.landPositions)
        {
            landTilemap.SetTile(pos, terrainTilesDict[TileType.LAND]);

            TerrainTile terrainTile = new TerrainTile(pos, terrainOrder[0]);
            terrainOrder.RemoveAt(0);
            terrainTilemap.SetTile(pos, terrainTilesDict[terrainTile.tileType]);

            if (!terrainTile.tileType.Equals(TileType.DESERT))
            {
                terrainTile.diceNumber = diceOrder[0];
                diceOrder.RemoveAt(0);
                numbersTilemap.SetTile(pos, numberTilesDict[terrainTile.diceNumber]);
            }

            boardTiles.Add(terrainTile);
            // boardVariant.landPositions.Add(pos);
        }

        foreach (Vector3Int pos in boardVariant.portPositions)
        {
            PortTile portTile = new PortTile(pos, portOrder[0]);
            portOrder.RemoveAt(0);
            portsTilemap.SetTile(pos, portTilesDict[portTile.portType]);

            boardTiles.Add(portTile);
            // boardVariant.portPositions.Add(pos);
        }
    }
}
