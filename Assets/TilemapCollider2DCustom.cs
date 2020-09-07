using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.Tilemaps;
using System;
using System.CodeDom.Compiler;
using JetBrains.Annotations;
using UnityEditor.UIElements;

public class TilemapCollider2DCustom : MonoBehaviour{
    Tilemap tilemap;

    void Start() {
        tilemap = GetComponent<Tilemap>();
        List <Vector2[]> edges= generateColliderPoints(colliderMap(tilemap), tilemap.cellBounds.min);

        if (edges.Count == 0) Debug.Log("Generated no Edge Collider");
        if (edges.Count == 1) Debug.Log("Generated " + edges.Count + " Edge Collider");
        if (edges.Count>1) Debug.Log("Generated "+edges.Count+" Edge Colliders");

        foreach (Vector2[] points in edges) {

            EdgeCollider2D collider = (EdgeCollider2D)gameObject.AddComponent(typeof(EdgeCollider2D));
            collider.points = points;

        }
    }
    void Update() {

    }


    bool[][] colliderMap(Tilemap tilemap) {
        
        TileBase[] allTiles = tilemap.GetTilesBlock(tilemap.cellBounds);
        bool[][] map = new bool[tilemap.cellBounds.size.x * 2 + 2][];
        for (int i = 0; i < map.Length; i++) {
            map[i] = new bool[tilemap.cellBounds.size.y * 2 + 2];
        }

        for (int x = 0; x < tilemap.cellBounds.size.x; x++) {
            
            for (int y = 0; y < tilemap.cellBounds.size.y; y++) {
                TileBase tile = allTiles[x + y * tilemap.cellBounds.size.x];

                if (tile != null) {
                    map[x * 2 + 1][y * 2] = !map[x * 2 + 1][y * 2];
                    map[x * 2][y * 2 + 1] = !map[x * 2][y * 2 + 1];
                    map[x * 2 + 2][y * 2 + 1] = !map[x * 2 + 2][y * 2 + 1];
                    map[x * 2 + 1][y * 2 + 2] = !map[x * 2 + 1][y * 2 + 2];
                }
            }
        }
        return map;
    }


    List<Vector2[]> generateColliderPoints(bool[][] map,Vector3Int offset) {
        /*String[] m=new String[map[0].Length];
        for (int x = 0; x < map.Length; x++) {
            for (int y = 0; y < map[x].Length; y++) {
                if (map[x][y]) m[y] += "x";
                if (!map[x][y]) m[y] += ".";

            }
        }
        String outM="";
        foreach (String s in m) {
            outM += s + Environment.NewLine;
        }
        Debug.Log(outM);
        */

        List<Vector2[]> pointsList = new List<Vector2[]>();

        for (int x=0;x<map.Length;x++) {
            for (int y = 0; y < map[x].Length; y++) {
                if (xOr(x%2==0,y%2==0)) {
                    if (map[x][y]) {

                        Vector2[] points = new Vector2[0];
                        Vector2 startPoint = new Vector2(x, y);

                        points = traceLine(map, startPoint, 1, startPoint).ToArray();

                        for (int i = 0; i < points.Length; i++) {
                            points[i] /= 2;
                            points[i] += new Vector2(offset.x, offset.y);
                        }

                        pointsList.Add(points);

                        Debug.Log("New area found ... It had a boundarylength of "+points.Length);

                    }
                }
            }
        }
        /*foreach (Vector2[] vArray in pointsList) {
            Debug.Log("-----------------");
            foreach (Vector2 v in vArray) {
                Debug.Log(v);
            }
        }*/
        return pointsList;
    }

    bool xOr(bool b1, bool b2) {
        return b1 != b2;
    }

    List<Vector2> traceLine(bool[][] map, Vector2 currentPos, int direction,Vector2 firstPoint) {

        /*String[] m = new String[map[0].Length];
        for (int x = 0; x < map.Length; x++) {
            for (int y = 0; y < map[x].Length; y++) {
                if (map[x][y]&&x==(int)currentPos.x && y == (int)currentPos.y) m[y] += "o";
                if (map[x][y]&&!(x == (int)currentPos.x && y == (int)currentPos.y)) m[y] += "x";
                if (!map[x][y]) m[y] += ".";

            }
        }
        String outM = "";
        foreach (String s in m) {
            outM += s + Environment.NewLine;
        }

        Debug.Log(outM);
        Debug.Log("direction: "+direction);
         */
        if (currentPos == firstPoint) {
            if (currentPos.x % 2 == 0) {
                firstPoint += new Vector2(0, direction);
            } else if (currentPos.y % 2 == 0) {
                firstPoint += new Vector2(direction, 0);
            } else {
                Debug.LogWarning("There might be a Mistake here! The code is not expected to get here.");
            }
        }



        if (currentPos.x % 2 == 0) {
            //Debug.Log("vertical");
            for (int i = 0; i < 3; i++) {
                try {
                // i = 0  =>          x - 1                   y + direction                   [left]
                // i = 1  =>          x                       y + direction*2                 [up/down]
                // i = 2  =>          x + 1                   y + direction                   [right]
                    if (map[(int)currentPos.x - 1 + i][(int)currentPos.y + direction * (1 + i % 2)]) {
                        if (map[(int)currentPos.x][(int)currentPos.y] != true) { Debug.LogWarning("There might be a Mistake here! This position is expected to be true."); }
                        map[(int)currentPos.x][(int)currentPos.y] = false;
                        //Debug.Log("i = " + i + ";          (" + ((int)currentPos.x) + " | " + ((int)currentPos.y) + ")   =>  (" + ((int)currentPos.x + direction * (1 + i % 2)) + " | " + ((int)currentPos.y - 1 + i) + ")");

                        //  i = 0     =>                                          x - 1                       y + direction                             -1
                        //  i = 1     =>                                          x                           y + direction*2                           direction
                        //  i = 2     =>                                          x + 1                       y + direction                             1
                        List<Vector2> points = traceLine(map, new Vector2((int)currentPos.x - 1 + i, (int)currentPos.y + direction * (1 + i % 2)), i - 1 + direction * i % 2, firstPoint);
                        points.Add(currentPos + new Vector2(0,direction));
                        return points;

                    }
                } catch (IndexOutOfRangeException e) { }
            }
        } else if (currentPos.y % 2 == 0) {
            //Debug.Log("horizontal");
            for (int i = 0; i < 3; i++) {
                try {
                // i = 0  =>          x + direction                           y - 1           [up]
                // i = 1  =>          x + direction*2                         y               [left/right]
                // i = 2  =>          x + direction                           y + 1           [down]
                    if (map[(int)currentPos.x + direction * (1 + i % 2)][(int)currentPos.y - 1 + i]) {
                        if (map[(int)currentPos.x][(int)currentPos.y] != true) { Debug.LogWarning("There might be a Mistake here! This position is expected to be true."); }
                        map[(int)currentPos.x][(int)currentPos.y] = false;
                        //Debug.Log("i = " + i + ";          (" + ((int)currentPos.x) + " | " + ((int)currentPos.y) + ")   =>  (" + ((int)currentPos.x + direction * (1 + i % 2)) + " | " + ((int)currentPos.y - 1 + i) + ")");

                        //  i = 0     =>                                          x + direction                   y - 1                                 -1
                        //  i = 1     =>                                          x + direction*2                 y                                     direction
                        //  i = 2     =>                                          x + direction                   y + 1                                 1
                        List<Vector2> points = traceLine(map, new Vector2((int)currentPos.x + direction * (1 + i % 2), (int)currentPos.y - 1 + i), i - 1 + direction * i % 2, firstPoint);
                        points.Add(currentPos + new Vector2(direction,0));
                        return points;

                    }
                }catch (IndexOutOfRangeException e) { }
        }
        }


        if (map[(int)currentPos.x][(int)currentPos.y] != true) { Debug.LogWarning("There might be a Mistake here! This position is expected to be true."); }
        map[(int)currentPos.x][(int)currentPos.y] = false;

        List<Vector2> newPointsList = new List<Vector2>();
        newPointsList.Add(firstPoint);
        if (currentPos.x % 2 == 0) {
            newPointsList.Add(currentPos + new Vector2(0, direction));
        } else if (currentPos.y % 2 == 0){
            newPointsList.Add(currentPos + new Vector2(direction, 0));
        }else {
            Debug.LogWarning("There might be a Mistake here! The code is not expected to get here.");
        }

        return newPointsList;
    }


}
