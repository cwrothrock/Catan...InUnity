using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class CatanManager : MonoBehaviour
{
    [SerializeField] private Tilemap tilemapResources;

    // Start is called before the first frame update
    void Start()
    {
        tilemapResources.CompressBounds();
        FindResourcePositions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FindResourcePositions()
    {
        for (int x = tilemapResources.cellBounds.xMin; x < tilemapResources.cellBounds.xMax; x++) {
            for (int y = tilemapResources.cellBounds.yMin; y < tilemapResources.cellBounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x,y,(int)tilemapResources.transform.position.z);
                if (tilemapResources.HasTile(pos)) {
                    Debug.Log(new Vector3(x,y,0));
                    Debug.Log(tilemapResources.GetSprite(pos));
                }
            }
        }
    }
}
