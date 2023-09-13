using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public enum TileType : int
    {
        WATER = 0,
        LAND = 1,
        PORT = 2,
    }

    public enum TerrainType : int
    {
        DESERT = 0,
        BRICK = 1,
        ORE = 2,
        SHEEP = 3,
        WHEAT = 4,
        WOOD = 5,
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

    public class TerrainTile : BoardTile
    {
        public int diceNumber;
        public TerrainType terrainType;
        public TerrainTile(Vector3Int position, TerrainType terrainType, int diceNumber = 0)
        : base(position, TileType.LAND)
        {
            this.diceNumber = diceNumber;
            this.terrainType = terrainType;
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

    // UI Components
    [SerializeField] private Tilemap waterTilemap;
    [SerializeField] private Tilemap landTilemap;
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap docksTilemap;
    [SerializeField] private Tilemap portsTilemap;
    [SerializeField] private Tilemap numbersTilemap;
    [SerializeField] private SerializedDictionary<TileType, Tile> baseTilesDict;
    [SerializeField] private SerializedDictionary<TerrainType, Tile> terrainTilesDict;
    [SerializeField] private SerializedDictionary<PortType, Tile> portTilesDict;
    [SerializeField] private SerializedDictionary<int, Tile> numberTilesDict;

    // Board
    [SerializeField] private TextAsset boardVariantJsonAsset;
    private BoardVariant boardVariant;
    private List<BoardRule> boardRules;
    private HexGraph graph;

    private List<TerrainTile> terrainTiles;
    private List<PortTile> portTiles;
    private Dictionary<int, List<TerrainTile>> diceTiles;

    private void Start()
    {
        GenerateBoard();
        GenerateGraph();
        UpdateUI();
    }

    private void GenerateBoard()
    {
        boardVariant = BoardVariant.From(boardVariantJsonAsset);
        
        boardRules = new List<BoardRule>
        {
            new AdjacentNumbersRule(),
            new Adjacent68Rule()
        };

        terrainTiles = new List<TerrainTile>();
        portTiles = new List<PortTile>();
        diceTiles = new Dictionary<int, List<TerrainTile>>();

        List<TerrainType> terrainOrder = BoardVariant.Explode(boardVariant.terrainCounts, shuffle: false);
        List<PortType> portOrder = BoardVariant.Explode(boardVariant.portCounts, shuffle: false);
        List<int> diceOrder = BoardVariant.Explode(boardVariant.diceCounts, shuffle: false);

        int attempts = 0;
        do
        {
            attempts++;

            terrainTiles.Clear();
            portTiles.Clear();
            diceTiles.Clear();
            
            diceOrder.RemoveAll(dice => dice == 7);

            BoardVariant.Shuffle(diceOrder);
            BoardVariant.Shuffle(terrainOrder);
            BoardVariant.Shuffle(portOrder);

            // Add DESERT 7 rolls to maintain alignment
            List<int> desertIndices = Enumerable.Range(0, terrainOrder.Count).Where(i => terrainOrder[i].Equals(TerrainType.DESERT)).ToList();
            desertIndices.ForEach(index =>
            {
                diceOrder.Insert(index, 7);
            });

            // Add terrain tiles with dice rolls
            for (int i = 0; i < terrainOrder.Count; i++)
            {
                AddTerrainTile(boardVariant.landPositions[i], terrainOrder[i], diceOrder[i]);
            }

            // Add port tiles
            for (int i = 0; i < boardVariant.portPositions.Count; i++)
            {
                AddPortTile(boardVariant.portPositions[i], portOrder[i]);
            }
        } while (!Validate(this));
        Debug.Log("Generated valid board in " + attempts + " attempts!");
    }

    private void AddTerrainTile(Vector3Int position, TerrainType type, int diceNumber)
    {
        terrainTiles.Add(new TerrainTile(position, type, diceNumber));
        if (!diceTiles.ContainsKey(diceNumber))
        {
            diceTiles.Add(diceNumber, new List<TerrainTile>());
        }
        diceTiles[diceNumber].Add(terrainTiles.Last());
    }

    public void AddPortTile(Vector3Int position, PortType type)
    {
        portTiles.Add(new PortTile(position, type));
    }


    private void GenerateGraph()
    {
        List<HexGraph.HexPosition> tilePositions = new();

        foreach (TerrainTile tile in terrainTiles)
        {
            tilePositions.Add(new HexGraph.HexPosition(tile.position, terrainTilemap.CellToWorld(tile.position)));
        }

        graph = new HexGraph(tilePositions);

        
    }
    private void UpdateUI()
    {
        foreach (TerrainTile tile in terrainTiles)
        {
            landTilemap.SetTile(tile.position, baseTilesDict[TileType.LAND]);
            terrainTilemap.SetTile(tile.position, terrainTilesDict[tile.terrainType]);
            if (!tile.terrainType.Equals(TerrainType.DESERT))
            {
                numbersTilemap.SetTile(tile.position, numberTilesDict[tile.diceNumber]);
            }
        }
        foreach (PortTile tile in portTiles)
        {
            portsTilemap.SetTile(tile.position, portTilesDict[tile.portType]);
        }
    }

    public List<TerrainTile> GetTerrainTiles() => terrainTiles;
    public List<PortTile> GetPortTiles() => portTiles;
    public List<BoardRule> GetBoardRules() => boardRules;
    public Dictionary<int, List<TerrainTile>> GetDiceTiles() => diceTiles;

    public static bool Validate(Board board)
    {
        bool result = true;
        board.GetBoardRules().ForEach(rule =>
        {
            result &= rule.Validate(board);
        });
        return result;
    }
}
