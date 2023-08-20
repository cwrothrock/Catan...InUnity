using UnityEngine;
using UnityEngine.Tilemaps;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using Unity.Netcode;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.UIElements;

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
            board.GetDiceTiles().Keys.ToList().ForEach(diceRoll => {
                List<Vector3Int> positions = board.GetDiceTiles()[diceRoll].Select(tile => tile.position).ToList();
                valid &= ContainsNeighbors(positions);
            });
            return valid;
        }
    }

    public class Adjacent68Rule : BoardRule
    {
        public override bool Validate(Board board)
        {
            List<Vector3Int> positions = new();
            positions.Concat(board.GetDiceTiles()[6].Select(tile => tile.position).ToList());
            positions.Concat(board.GetDiceTiles()[8].Select(tile => tile.position).ToList());
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

    # region Inspector Elements
    [SerializeField] private Tilemap landTilemap;
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap docksTilemap;
    [SerializeField] private Tilemap portsTilemap;
    [SerializeField] private Tilemap numbersTilemap;

    [SerializeField] private SerializedDictionary<TileType, Tile> terrainTilesDict;
    [SerializeField] private SerializedDictionary<PortType, Tile> portTilesDict;
    [SerializeField] private SerializedDictionary<int, Tile> numberTilesDict;
    [SerializeField] private SerializedDictionary<bool, string> boardRulesDict;

    [SerializeField] private TextAsset boardVariantJsonAsset;
    # endregion

    private BoardVariant boardVariant;
    private List<BoardRule> boardRules;
    private List<TerrainTile> terrainTiles;
    private List<PortTile> portTiles;
    private Graph graph;
    private Dictionary<int, List<TerrainTile>> diceTiles;

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
    }

    private void GenerateBoard(BoardVariant boardVariant, List<BoardRule> boardRules)
    {

        List<TileType> terrainOrder = BoardVariant.Explode(boardVariant.terrainCounts, shuffle: false);
        List<PortType> portOrder = BoardVariant.Explode(boardVariant.portCounts, shuffle: false);
        List<int> diceOrder = BoardVariant.Explode(boardVariant.diceCounts, shuffle: false);

        do {
            terrainTiles.Clear();
            portTiles.Clear();
            diceTiles.Clear();

            BoardVariant.Shuffle(diceOrder);
            BoardVariant.Shuffle(terrainOrder);
            BoardVariant.Shuffle(portOrder);

            // Add DESERT 7 rolls to maintain alignment
            List<int> desertIndices = Enumerable.Range(0, terrainOrder.Count).Where(i => terrainOrder[i].Equals(TileType.DESERT)).ToList();
            desertIndices.ForEach(index => {
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
        } while (false);
        // TODO: change this to while(!Validate(this))
        Validate(this);
    }

    private void AddTerrainTile(Vector3Int position, TileType type, int diceNumber)
    {
        terrainTiles.Add(new TerrainTile(position, type, diceNumber));
        if (!diceTiles.ContainsKey(diceNumber)) {
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

    public static bool ContainsNeighbors(List<Vector3Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i+1; j < list.Count; j++)
            {
                if (IsNeighbor(list[i], list[j])) 
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
        board.GetBoardRules().ForEach(rule => {
            result &= rule.Validate(board);
        });
        return result;
    }
}
