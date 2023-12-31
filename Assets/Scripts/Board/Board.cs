using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public abstract class BoardRule
    {
        public virtual bool Validate(Board board)
        {
            // Board configuration is valid
            return true;
        }
    }

    public class AdjacentNumbersRule : BoardRule
    {
        public override bool Validate(Board board)
        {
            bool valid = true;
            board.GetDiceTiles().Keys.ToList().ForEach(diceRoll =>
            {
                List<Vector3Int> positions = board.GetDiceTiles()[diceRoll].Select(tile => tile.position).ToList();
                valid &= !ContainsNeighbors(positions);
            });
            return valid;
        }
    }

    public class Adjacent68Rule : BoardRule
    {
        public override bool Validate(Board board)
        {
            List<Vector3Int> positions = new();
            positions.AddRange(board.GetDiceTiles()[6].Select(tile => tile.position));
            positions.AddRange(board.GetDiceTiles()[8].Select(tile => tile.position));
            return !ContainsNeighbors(positions);
        }
    }

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
    private List<BoardRule> boardRules;
    private List<TerrainTile> terrainTiles;
    private List<PortTile> portTiles;
    private Dictionary<int, List<TerrainTile>> diceTiles;
    private Graph graph;

    private void Start()
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

        graph = new Graph();

        GenerateBoard(boardVariant, boardRules);
        UpdateBoardUI();
        UpdateBoardGraph();
    }

    private void GenerateBoard(BoardVariant boardVariant, List<BoardRule> boardRules)
    {

        List<TileType> terrainOrder = BoardVariant.Explode(boardVariant.terrainCounts, shuffle: false);
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
            List<int> desertIndices = Enumerable.Range(0, terrainOrder.Count).Where(i => terrainOrder[i].Equals(TileType.DESERT)).ToList();
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

    private void AddTerrainTile(Vector3Int position, TileType type, int diceNumber)
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

    private void UpdateBoardGraph()
    {
        List<(Vector3Int, Vector3)> tilePositions = new();

        foreach (TerrainTile tile in terrainTiles)
        {
            tilePositions.Add((tile.position, terrainTilemap.CellToWorld(tile.position)));
        }

        graph.GenerateGraph(tilePositions);
    }

    public static bool ContainsNeighbors(List<Vector3Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (Graph.IsNeighbor(list[i], list[j]))
                {
                    return true;
                }
            }
        }
        return false;
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
