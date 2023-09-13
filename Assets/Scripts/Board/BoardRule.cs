using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
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
            valid &= !HexGraph.HasNeighbors(positions);
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
        return !HexGraph.HasNeighbors(positions);
    }
}