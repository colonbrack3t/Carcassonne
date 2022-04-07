using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    // Start is called before the first frame update
    public Tile_Side[] sides = new Tile_Side[4];
    public bool is_river, is_river_source, is_river_lake, is_river_bend, is_monk_place, is_road_end, is_castle, is_road, multiple_castles;
    public List<(MeepPositions position, Direction direction)> claimed = new List<(MeepPositions position, Direction direction)>();
    void Start()
    {
        is_river = Array.IndexOf(sides, Tile_Side.River) > -1;
        is_road = Array.IndexOf(sides, Tile_Side.Road) > -1;
        is_castle = Array.IndexOf(sides, Tile_Side.Castle) > -1;
    }

    // Update is called once per frame - VFX HERE?
    void Update()
    {

    }

    //rotates transform and updates tile sides
    public void Rotate()
    {

        transform.Rotate(0, 90, 0);
        Tile_Side last_item = sides[3];
        for (int i = 3; i > 0; i--)
        {
            sides[i] = sides[i - 1];
        }
        sides[0] = last_item;
    }
    public List<(MeepPositions, List<Direction>)> get_Meep_Locations()
    {
        List<(MeepPositions, List<Direction>)> output = new List<(MeepPositions, List<Direction>)>();
        if (is_castle)
        {
            if (multiple_castles)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (sides[i] == Tile_Side.Castle)
                        output.Add((MeepPositions.OnCastle, new List<Direction> { (Direction)i }));

                }

            }
            else
            {
                var dirs = new List<Direction>();
                for (int i = 0; i < 4; i++)
                    if (sides[i] == Tile_Side.Castle)
                        dirs.Add((Direction)i);
                output.Add((MeepPositions.OnCastle, dirs));
            }
        }
        if (is_road)
        {
            if (is_road_end)//similar to multiple castles
            {
                for (int i = 0; i < 4; i++)
                {
                    if (sides[i] == Tile_Side.Road)
                        output.Add((MeepPositions.OnRoad, new List<Direction> { (Direction)i }));

                }

            }
            else
            {
                var dirs = new List<Direction>();
                for (int i = 0; i < 4; i++)
                    if (sides[i] == Tile_Side.Road)
                        dirs.Add((Direction)i);
                output.Add((MeepPositions.OnRoad, dirs));
            }
        }
        if (is_monk_place)
        {
            output.Add((MeepPositions.OnTower, null));
        }
        return output;
    }
    public static bool SideMatchesPosition(Tile_Side side, MeepPositions pos)
    {
        return (side == Tile_Side.Road && pos == MeepPositions.OnRoad) ||
               (side == Tile_Side.Castle && pos == MeepPositions.OnCastle);
    }
    public static Direction InvertDirection(Direction d)
    {
        switch (d)
        {
            case Direction.North:
                return Direction.South;
            case Direction.East:
                return Direction.West;
            case Direction.South:
                return Direction.North;
            case Direction.West:
                return Direction.East;
            default :
                return d;
        }
    }
    public static (int x, int y) DirectionToIntPair(Direction d)
    {
        int x = 0, y = 0;
        switch (d)
        {
            case Direction.North:
                y++;
                break;
            case Direction.East:
                x++;
                break;
            case Direction.South:
                y--;
                break;
            case Direction.West:
                x--;
                break;
            
        }
        return (x, y);
    }
}



public enum Tile_Side
{
    Road, Field, River, Castle

}
public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    Centre = 4
}

public enum MeepPositions
{
    OnCastle,
    OnRoad,
    OnTower
}
