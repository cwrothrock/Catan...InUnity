using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CatanManager : MonoBehaviour
{
    [SerializeField]
    private Tile desertTile;
    [SerializeField]
    private Tile brickTile;
    [SerializeField]
    private Tile oreTile;
    [SerializeField]
    private Tile sheepTile;
    [SerializeField]
    private Tile wheatTile;
    [SerializeField]
    private Tile woodTile;

    [SerializeField]
    private Tile dice02Tile;
    [SerializeField]
    private Tile dice03Tile;
    [SerializeField]
    private Tile dice04Tile;
    [SerializeField]
    private Tile dice05Tile;
    [SerializeField]
    private Tile dice06Tile;
    [SerializeField]
    private Tile dice07Tile;
    [SerializeField]
    private Tile dice08Tile;
    [SerializeField]
    private Tile dice09Tile;
    [SerializeField]
    private Tile dice10Tile;
    [SerializeField]
    private Tile dice11Tile;
    [SerializeField]
    private Tile dice12Tile;

    private static Tile[] resourceTiles;

    private static Tile[] numberTiles;

    private enum ResourceType : int
    {
        DESERT = 0,
        BRICK = 1,
        ORE = 2,
        SHEEP = 3,
        WHEAT = 4,
        WOOD = 5
    }

    private Board board;

    private class BoardTile
    {
        public int x;
        public int y;
        public ResourceType resourceType;
        public int diceNumber;

        public BoardTile(int x, int y, ResourceType resourceType, int diceNumber)
        {
            this.x = x;
            this.y = y;
            this.resourceType = resourceType;
            this.diceNumber = diceNumber;
        }
    }

    private class Board
    {
        private Tilemap resourceGrid;
        private Tilemap numberGrid;
        private List<BoardTile> boardTiles;

        public Board()
        {
            boardTiles = new();
            resourceGrid = GameObject.FindGameObjectWithTag("ResourceTiles").GetComponent<Tilemap>();
            numberGrid = GameObject.FindGameObjectWithTag("NumberTiles").GetComponent<Tilemap>();
        }

        public void GenerateBoard(int x_from, int x_to, int y_from, int y_to)
        {
            for (int x = x_from; x <= x_to; x++)
            {
                for (int y = y_from; y <= y_to; y++)
                {
                    BoardTile boardTile = new(x, y, ResourceType.WOOD, 6);
                    boardTiles.Add(boardTile);
                }
            }
        }

        public void PopulateBoard()
        {
            foreach (BoardTile boardTile in boardTiles)
            {
                resourceGrid.SetTile(new Vector3Int(boardTile.x, boardTile.y, 0), resourceTiles[(int)boardTile.resourceType]);
                numberGrid.SetTile(new Vector3Int(boardTile.x, boardTile.y, 0), numberTiles[boardTile.diceNumber]);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        resourceTiles = new Tile[] {
            desertTile,
            brickTile,
            oreTile,
            sheepTile,
            wheatTile,
            woodTile
        };

        numberTiles = new Tile[] {
            dice02Tile,
            dice03Tile,
            dice04Tile,
            dice05Tile,
            dice06Tile,
            dice07Tile,
            dice08Tile,
            dice09Tile,
            dice10Tile,
            dice11Tile,
            dice12Tile
        };

        board = new();
        board.GenerateBoard(-2, 2, -2, 2);
        board.PopulateBoard();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
