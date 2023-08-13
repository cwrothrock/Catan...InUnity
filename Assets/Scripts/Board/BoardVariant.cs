using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

[Serializable]
public class BoardVariant
{
    public Dictionary<int, int> diceCounts;
    public Dictionary<Board.PortType, int> portCounts;
    public Dictionary<Board.TileType, int> terrainCounts;
    public Dictionary<Board.TileType, int> cardCounts;
    public Dictionary<string, int> devCounts;
    public Dictionary<string, int> buildingCounts;

    public static BoardVariant From(string json)
    {
        return JsonConvert.DeserializeObject<BoardVariant>(json);
    }

    public static BoardVariant From(TextAsset textAsset)
    {
        return JsonConvert.DeserializeObject<BoardVariant>(textAsset.text);
    }

    public static List<T> Explode<T>(Dictionary<T,int> dict, bool shuffle = false)
    {
        List<T> values = new List<T>();
        foreach (KeyValuePair<T,int> kvp in dict)
        {
            values.AddRange(Enumerable.Repeat(kvp.Key, kvp.Value));
        }
        if (shuffle) 
        {
            Shuffle(values);
        }
        return values;
    }

    public static void Shuffle<T>(List<T> list)
    {
        var count = list.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}
