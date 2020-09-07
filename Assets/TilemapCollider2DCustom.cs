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
        //gets the tilemap attatched to this gameobject
        tilemap = GetComponent<Tilemap>();

        //  gets an list of Vector2[] from the function "generateColliderPoints" which needs a pre-processed tilemap (converted to bool matrix) given by "colliderMap(tilemap)" as well as the tilemaps cellbounds to translate the tilemap positions to worldpositions. 
        //  Each Vector2[] will be converted to its own edge collider  (in lines 28-32)
        List<Vector2[]> edges= generateColliderPoints(colliderMap(tilemap), tilemap.cellBounds.min);

        // gives the user feedback how many colliders will be created
        if (edges.Count == 0) Debug.Log("Generated no edge collider");
        if (edges.Count == 1) Debug.Log("Generated 1 edge collider");
        if (edges.Count>1) Debug.Log("Generated "+edges.Count+" edge colliders");


        // Takes the given list of Vector2[] and transfers each Vector2[] into an edgeColliders
        foreach (Vector2[] points in edges) {
            EdgeCollider2D collider = (EdgeCollider2D)gameObject.AddComponent(typeof(EdgeCollider2D));
            collider.points = points;
        }


    }


    // the function which pre-processes the tilemap converting it into an bool[][] (Matrix) which i will rever to as a map.    bool[x][y] is true if there is a tile at the position (x|y)
    bool[][] colliderMap(Tilemap tilemap) {
        // converts the Tilemap into an array containing its TileBases
        TileBase[] allTiles = tilemap.GetTilesBlock(tilemap.cellBounds);

        // initiallises the map (bool[][]) with the correct size
        bool[][] map = new bool[tilemap.cellBounds.size.x * 2 + 2][];
        for (int i = 0; i < map.Length; i++) {
            map[i] = new bool[tilemap.cellBounds.size.y * 2 + 2];
        }

        /*loops through all of the TileBases (and finds all of the edges of the structure)
         * Example:
         *
         *  Input:                              Output:                             Meaning: (you can see the old numbers as well as the 1s outlined)
         *      0 1 1 0                             0 0 0 1 0 1 0 0 0 0                     -   -         
         *      0 1 1 1                             0 0 1 0 0 0 1 0 0 0                 0 | 1   1 | 0    
         *      0 1 1 1                             0 0 0 0 0 0 0 1 0 0                          
         *                                          0 0 1 0 0 0 0 1 1 0                 0 | 1   1   1 |   
         *                                          0 0 0 0 0 0 0 0 0 0                          
         *                                          0 0 1 0 0 0 0 0 1 0                 0 | 1   1   1 |   
         *                                          0 0 0 1 0 1 0 1 0 0                     -   -   -        
         *                                          0 0 0 0 0 0 0 0 0 0                     
        */

        for (int x = 0; x < tilemap.cellBounds.size.x; x++) {
            for (int y = 0; y < tilemap.cellBounds.size.y; y++) {
                //gets the tile at the tilemaps position [x][y]         !!! Do not confuse this with the tiles worldposition. This is only its position inside the tilemap !!!
                TileBase tile = allTiles[x + y * tilemap.cellBounds.size.x];

                // This is the algorithme doing what was discribed above (converting matrix of tiles to matrix of bounds)
                if (tile != null) {
                    map[x * 2 + 1][y * 2] = !map[x * 2 + 1][y * 2];
                    map[x * 2][y * 2 + 1] = !map[x * 2][y * 2 + 1];
                    map[x * 2 + 2][y * 2 + 1] = !map[x * 2 + 2][y * 2 + 1];
                    map[x * 2 + 1][y * 2 + 2] = !map[x * 2 + 1][y * 2 + 2];
                }

            }
        }

        //returns the map of boarders
        return map;
    }


    // The function generating the actual collider points from a map of boarders (bool[][]) 
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
        
        // creates the list in which all of the Vector[] will be stored
        List<Vector2[]> pointsList = new List<Vector2[]>();

        // loops through every (spot of the map) bool in the map (bool[][]) 
        for (int x=0;x<map.Length;x++) {
            for (int y = 0; y < map[x].Length; y++) {
                //through the way of outputting the map in the way described at line 50 boarders will only be able to be on every second spot (chess-board-pattern)
                // this checks if the point (x|y) is in a second spot (you could imagen it as only the white fields of the chess board)
                if (xOr(x%2==0,y%2==0)) {
                    // if there is a boarder at (x|y)
                    if (map[x][y]) {

                        Vector2[] points = new Vector2[0];
                        Vector2 startPoint = new Vector2(x, y);

                        // gets a point list and converts it to a point[] (could be imagend as a line) for one Collider from "traceLine" (which takes in the map it operates on as well as the point from which it starts tracking down the others and a starting direction)
                        points = traceLine(map, startPoint, 1, startPoint).ToArray();

                        // this applies a transformation to the points translating them from their map to world cordinates
                        for (int i = 0; i < points.Length; i++) {
                            points[i] /= 2;
                            points[i] += new Vector2(offset.x, offset.y);
                        }

                        // adds the point [] (line) to the list of Vector2[] which will later on give this point[] to a edge collider
                        pointsList.Add(points);

                        // gives the user feedback that a arrea with the boundary size -points.Length- has been found
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

        // returns the list of Vector2[]
        return pointsList;
    }

    bool xOr(bool b1, bool b2) {
        return b1 != b2;
    }


    // !!! This function calls itself; each iteration adding one point to the List !!!
    // it takes in a map as well as a point on the map and searches for others; which then again search for more... Once it finds no more it returns the Vector2s in a list of all of the points found
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


        // if this is the first point it corrects the first points possision 
        if (currentPos == firstPoint) {
            if (currentPos.x % 2 == 0) {
                firstPoint += new Vector2(0, direction);
            } else if (currentPos.y % 2 == 0) {
                firstPoint += new Vector2(direction, 0);
            } else {
                Debug.LogWarning("There might be a Mistake here! The code is not expected to get here.");
            }
        }


        // this first part (lines 186-210) is accessed if this is looking at a vertical line
        if (currentPos.x % 2 == 0) {
            //Debug.Log("vertical");
            // this searches if there is another line in the given direction (1=down ; 2=up)
            for (int i = 0; i < 3; i++) {
                try {
                // i = 0  =>          x - 1                   y + direction                   [left]
                // i = 1  =>          x                       y + direction*2                 [up/down]
                // i = 2  =>          x + 1                   y + direction                   [right]
                    if (map[(int)currentPos.x - 1 + i][(int)currentPos.y + direction * (1 + i % 2)]) {
                        // if another line in the given direction was found this will be executed; calling another instance of this funcion for the found line. Otherwise this will create a new Vector2 list 

                        if (map[(int)currentPos.x][(int)currentPos.y] != true) { Debug.LogWarning("There might be a Mistake here! This position is expected to be true."); }
                        map[(int)currentPos.x][(int)currentPos.y] = false;
                        //Debug.Log("i = " + i + ";          (" + ((int)currentPos.x) + " | " + ((int)currentPos.y) + ")   =>  (" + ((int)currentPos.x + direction * (1 + i % 2)) + " | " + ((int)currentPos.y - 1 + i) + ")");

                        //  i = 0     =>                                          x - 1                       y + direction                             -1
                        //  i = 1     =>                                          x                           y + direction*2                           direction
                        //  i = 2     =>                                          x + 1                       y + direction                             1
                        List<Vector2> points = traceLine(map, new Vector2((int)currentPos.x - 1 + i, (int)currentPos.y + direction * (1 + i % 2)), i - 1 + direction * i % 2, firstPoint);
                        // this point is added to the Vector2 list of the called instance
                        points.Add(currentPos + new Vector2(0,direction));

                        //this passes the list up to the function which called this (probably another instance of this)
                        return points;

                    }
                } catch (IndexOutOfRangeException e) { }
            }


        // this second part is accessed if this is looking at a horizontal line. It is the exact same code but instead of searching up or down it searches right or left of it selfe
        } else if (currentPos.y % 2 == 0) {
            //Debug.Log("horizontal");
            for (int i = 0; i < 3; i++) {
                try {
                // i = 0  =>          x + direction                           y - 1           [up]
                // i = 1  =>          x + direction*2                         y               [left/right]
                // i = 2  =>          x + direction                           y + 1           [down]
                    if (map[(int)currentPos.x + direction * (1 + i % 2)][(int)currentPos.y - 1 + i]) {
                        // if another line in the given direction was found this will be executed; calling another instance of this funcion for the found line. Otherwise this will create a new Vector2 list 
                        
                        if (map[(int)currentPos.x][(int)currentPos.y] != true) { Debug.LogWarning("There might be a Mistake here! This position is expected to be true."); }
                        map[(int)currentPos.x][(int)currentPos.y] = false;
                        //Debug.Log("i = " + i + ";          (" + ((int)currentPos.x) + " | " + ((int)currentPos.y) + ")   =>  (" + ((int)currentPos.x + direction * (1 + i % 2)) + " | " + ((int)currentPos.y - 1 + i) + ")");

                        //  i = 0     =>                                          x + direction                   y - 1                                 -1
                        //  i = 1     =>                                          x + direction*2                 y                                     direction
                        //  i = 2     =>                                          x + direction                   y + 1                                 1
                        List<Vector2> points = traceLine(map, new Vector2((int)currentPos.x + direction * (1 + i % 2), (int)currentPos.y - 1 + i), i - 1 + direction * i % 2, firstPoint);
                        // this point is added to the Vector2 list of the called instance
                        points.Add(currentPos + new Vector2(direction,0));

                        //this passes the list up to the function which called this (probably another instance of this)
                        return points;

                    }
                }catch (IndexOutOfRangeException e) { }
            }
        }

        // the following code is executed if there was no next line found

        // this is a check if something weired is happening. (might be extremly helpful in the case of trying to run all of this multithreaded)
        if (map[(int)currentPos.x][(int)currentPos.y] != true) { Debug.LogWarning("There might be a Mistake here! This position is expected to be true."); }
        map[(int)currentPos.x][(int)currentPos.y] = false;

        // a new list of Vector2 is created (which will be passed on to the function which called this)
        List<Vector2> newPointsList = new List<Vector2>();



        // this adds the line wich is currently looked at to the list
        
        // this adds the first point
        newPointsList.Add(firstPoint);
        // this adds this point
        if (currentPos.x % 2 == 0) {
            newPointsList.Add(currentPos + new Vector2(0, direction));
        } else if (currentPos.y % 2 == 0){
            newPointsList.Add(currentPos + new Vector2(direction, 0));
        }else {
            // this is a check if this point left the "chessboard-pattern"
            Debug.LogWarning("There might be a Mistake here! The code is not expected to get here.");
        }
        
        // return the newly created list
        return newPointsList;
    }


}
