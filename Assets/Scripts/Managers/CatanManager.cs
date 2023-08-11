using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CatanManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemapResources;
    [SerializeField]
    private Tilemap tilemapNumbers;

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

        public BoardTile(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

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

        public Board(Tilemap resources, Tilemap numbers)
        {
            resourceGrid = resources;
            numberGrid = numbers;

            boardTiles = new();

            for (int x = resourceGrid.cellBounds.xMin; x < resourceGrid.cellBounds.xMax; x++)
            {
                for (int y = resourceGrid.cellBounds.yMin; y < resourceGrid.cellBounds.yMax; y++)
                {
                    // TODO: I think this only works for small boards. Figure out how to do this for abstract boards.
                    if (x == resourceGrid.cellBounds.xMin && Mathf.Abs(x) == Mathf.Abs(y))
                    {
                        continue;
                    }
                    if (x == resourceGrid.cellBounds.xMax - 1 && y != 0)
                    {
                        continue;
                    }

                    BoardTile boardTile = new(x, y);
                    boardTiles.Add(boardTile);
                }
            }
        }

        public void GenerateBoard()
        {
            foreach (BoardTile boardTile in boardTiles)
            {
                // TODO: Add board generation logic
                boardTile.resourceType = (ResourceType)Random.Range(0, 5);
                boardTile.diceNumber = Random.Range(2, 12);

                SetBoardTile(boardTile);
            }
        }

        public void SetBoardTile(BoardTile boardTile)
        {
            resourceGrid.SetTile(new Vector3Int(boardTile.x, boardTile.y, 0), resourceTiles[(int)boardTile.resourceType]);
            numberGrid.SetTile(new Vector3Int(boardTile.x, boardTile.y, 0), numberTiles[boardTile.diceNumber]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
