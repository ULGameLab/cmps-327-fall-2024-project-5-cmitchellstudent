﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using MapGen;

public class Node
{
    public Node cameFrom = null; //parent node
    public double priority = 0; // F value
    public double costSoFar = 0; // G Value
    public Tile tile;

    public Node(Tile _tile, double _priority, Node _cameFrom, double _costSoFar)
    {
        cameFrom = _cameFrom;
        priority = _priority; 
        costSoFar = _costSoFar;
        tile = _tile;
    }
}

public class PathFinder
{
    List<Node> TODOList = new List<Node>();
    List<Node> DoneList = new List<Node>();
    Tile goalTile; 


    // This is the constructor
    public PathFinder()
    {
        goalTile = null;
    }

    // DONE: Find the path based on A-Star Algorithm
    public Queue<Tile> FindPathAStar(Tile start, Tile goal)
    {
        TODOList = new List<Node>();
        DoneList = new List<Node>();
        List<Tile> DoneTiles = new List<Tile>();

        TODOList.Add(new Node(start, 0, null, 0));
        goalTile = goal;

        while (TODOList.Count > 0)
        {
            TODOList.Sort((x, y) => (x.priority.CompareTo(y.priority))); // This will keep the TODO List sorted based on the F cost
            Node current = TODOList[0];
            DoneList.Add(current);
            DoneTiles.Add(current.tile);
            TODOList.RemoveAt(0);

            if (current.tile == goal)
            {
                return RetracePath(current);  // Returns the Path if goal is reached
            }
            
            // for each neighboring tile calculate the costs
            // You just need to fill code inside this foreach only
            
            foreach (Tile nextTile in current.tile.Adjacents)
            {
                if (DoneTiles.Contains(nextTile))
                {
                    continue;
                }
                int xDistance = Math.Abs(start.indexX - nextTile.indexX);
                int yDistance = Math.Abs(start.indexY - nextTile.indexY);
                int remaining = Math.Abs(xDistance - yDistance);
                double gCost = 14 * Math.Min(xDistance, yDistance) + 10 * remaining;
                double hCost = HeuristicsDistance(nextTile, goal);
                TODOList.Add(new Node(nextTile, gCost+hCost, current, gCost));
            }
        }
        return new Queue<Tile>(); // Returns an empty Path if no path is found
    }

    // TODO: Find the path based on A-Star Algorithm
    // In this case avoid a path passing near an enemy tile
    // BONUS TASK (Required the for Honors Contract Students)
    public Queue<Tile> FindPathAStarEvadeEnemy(Tile start, Tile goal, List<Enemy> enemyList)
    {
        TODOList = new List<Node>();
        DoneList = new List<Node>();
        List<Tile> DoneTiles = new List<Tile>();

        TODOList.Add(new Node(start, 0, null, 0));
        goalTile = goal;

        while (TODOList.Count > 0)
        {
            TODOList.Sort((x, y) => (x.priority.CompareTo(y.priority))); // This will keep the TODO List sorted
            Node current = TODOList[0];
            DoneList.Add(current);
            DoneTiles.Add(current.tile);
            TODOList.RemoveAt(0);

            if (current.tile == goal)
            {
                return RetracePath(current);  // Returns the Path if goal is reached
            }

            // for each neighboring tile calculate the costs
            // You just need to fill code inside this foreach only
            // Just increase the F cost of the enemy tile and the tiles around it by a certain amount (say 30)
            foreach (Tile nextTile in current.tile.Adjacents)
            {
                if (DoneTiles.Contains(nextTile))
                {
                    continue;
                }
                double enemyCost = 0;
                foreach (var enemy in enemyList)
                {
                    if (nextTile == enemy.currentTile)
                    {
                        enemyCost = 100;
                    }

                    if (enemy.currentTile.Adjacents.Contains(nextTile))
                    {
                        enemyCost = 80;
                    }
                }
                int xDistance = Math.Abs(start.indexX - nextTile.indexX);
                int yDistance = Math.Abs(start.indexY - nextTile.indexY);
                int remaining = Math.Abs(xDistance - yDistance);
                double gCost = 14 * Math.Min(xDistance, yDistance) + 10 * remaining;
                double hCost = HeuristicsDistance(nextTile, goal);
                
                TODOList.Add(new Node(nextTile, gCost+hCost+enemyCost, current, gCost));
            }
        }
        return new Queue<Tile>(); // Returns an empty Path
    }

    // Manhattan Distance with horizontal/vertical cost of 10
    double HeuristicsDistance(Tile currentTile, Tile goalTile)
    {
        int xdist = Math.Abs(goalTile.indexX - currentTile.indexX);
        int ydist = Math.Abs(goalTile.indexY - currentTile.indexY);
        // Assuming cost to move horizontally and vertically is 10
        //return manhattan distance
        return (xdist * 10 + ydist * 10);
    }

    // Retrace path from a given Node back to the start Node
    Queue<Tile> RetracePath(Node node)
    {
        List<Tile> tileList = new List<Tile>();
        Node nodeIterator = node;
        while (nodeIterator.cameFrom != null)
        {
            tileList.Insert(0, nodeIterator.tile);
            nodeIterator = nodeIterator.cameFrom;
        }
        return new Queue<Tile>(tileList);
    }

    // Generate a Random Path. Used for enemies
    public Queue<Tile> RandomPath(Tile start, int stepNumber)
    {
        List<Tile> tileList = new List<Tile>();
        Tile currentTile = start;
        for (int i = 0; i < stepNumber; i++)
        {
            Tile nextTile;
            //find random adjacent tile different from last one if there's more than one choice
            if (currentTile.Adjacents.Count < 0)
            {
                break;
            }
            else if (currentTile.Adjacents.Count == 1)
            {
                nextTile = currentTile.Adjacents[0];
            }
            else
            {
                nextTile = null;
                List<Tile> adjacentList = new List<Tile>(currentTile.Adjacents);
                ShuffleTiles<Tile>(adjacentList);
                if (tileList.Count <= 0) nextTile = adjacentList[0];
                else
                {
                    foreach (Tile tile in adjacentList)
                    {
                        if (tile != tileList[tileList.Count - 1])
                        {
                            nextTile = tile;
                            break;
                        }
                    }
                }
            }
            tileList.Add(currentTile);
            currentTile = nextTile;
        }
        return new Queue<Tile>(tileList);
    }

    private void ShuffleTiles<T>(List<T> list)
    {
        // Knuth shuffle algorithm :: 
        // courtesy of Wikipedia :) -> https://forum.unity.com/threads/randomize-array-in-c.86871/
        for (int t = 0; t < list.Count; t++)
        {
            T tmp = list[t];
            int r = UnityEngine.Random.Range(t, list.Count);
            list[t] = list[r];
            list[r] = tmp;
        }
    }
}
