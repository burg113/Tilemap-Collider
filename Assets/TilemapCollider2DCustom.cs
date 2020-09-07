using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.Tilemaps;
using System;
using System.CodeDom.Compiler;

public class TilemapCollider2DCustom : MonoBehaviour{
    Tilemap tilemap;

    void Start() {
        tilemap = GetComponent<Tilemap>();
        
    }
    void Update() {

    }


    bool[][] processedTilemap(Tilemap tilemap) {
        TileBase[] allTiles = tilemap.GetTilesBlock(tilemap.cellBounds);
        bool[][] map = new bool[tilemap.cellBounds.size.x][];

        for (int x = 0; x < tilemap.cellBounds.size.x; x++) {
            map[x] = new bool[tilemap.cellBounds.size.y];
            for (int y = 0; y < tilemap.cellBounds.size.y; y++) {
                TileBase tile = allTiles[x + y * tilemap.cellBounds.size.x];
                map[x][y] = tile != null;
            }
        }
        return map;
    }


    Collider generateCollider(bool[][] map) {


        return null;
    }


}
