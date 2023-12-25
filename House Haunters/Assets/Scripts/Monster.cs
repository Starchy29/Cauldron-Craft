using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : GridEntity
{
    [SerializeField] private MonsterName monsterType;

    public bool OnPlayerTeam { get; private set; }

    public MonsterType Stats { get; private set; }
    private int health;
    // statuses
    // shields

    void Start() {
        Stats = MonsterData.Instance.GetMonsterData(monsterType);
        health = Stats.Health;
    }

    void Update() {
        
    }

    // returns null if this monster cannot get to the tile with one movement
    public List<Vector2Int> FindPath(Vector2Int endTile) {
        LevelGrid level = LevelGrid.Instance;
        if(!level.GetTile(endTile).Walkable) {
            return null;
        }

        // set up data array for pathfinding
        PathData[,] distances = new PathData[LevelGrid.height, LevelGrid.width];
        for(int y = 0; y < LevelGrid.height; y++) {
            for(int x = 0; x < LevelGrid.width; x++) {
                distances[y, x] = new PathData();
            }
        }
        distances[Tile.y, Tile.x].travelDistance = 0;
        distances[Tile.y, Tile.x].distanceToEnd = Global.CalcTileDistance(Tile, endTile);

        // find the shortest path using A*
        List<Vector2Int> closedList = new List<Vector2Int>();
        List<Vector2Int> openList = new List<Vector2Int>() { Tile };
        while(openList.Count > 0) {
            // find the best option in the open list
            Vector2Int nextTile = openList.Min((Vector2Int tile) => { return distances[tile.y, tile.x].Estimate; });

            // check if this is the end
            if(nextTile == endTile) {
                if(distances[nextTile.y, nextTile.x].travelDistance > Stats.Speed) {
                    // invalid if too many steps
                    return null;
                }

                List<Vector2Int> path = new List<Vector2Int>();
                while(distances[nextTile.y, nextTile.x].previous != null) {
                    path.Add(nextTile);
                    nextTile = distances[nextTile.y, nextTile.x].previous.Value;
                }
                path.Reverse();
                return path;
            }

            openList.Remove(nextTile);
            closedList.Add(nextTile);

            // update the neighbors
            foreach(Vector2Int direction in Global.Cardinals) {
                Vector2Int neighbor = nextTile + direction;
                if(!level.IsInGrid(neighbor) || !level.GetTile(neighbor).Walkable) {
                    continue; // walls and pits are not navigable
                }

                bool inOpen = openList.Contains(neighbor);
                bool inClosed = closedList.Contains(neighbor);
                int discoveredDistance = distances[nextTile.y, nextTile.x].travelDistance + level.GetTile(neighbor).GetTravelCost(OnPlayerTeam);
                if(!inOpen && !inClosed) {
                    // found a new route
                    openList.Add(neighbor);
                    distances[neighbor.y, neighbor.x].previous = nextTile;
                    distances[neighbor.y, neighbor.x].travelDistance = discoveredDistance;
                    distances[neighbor.y, neighbor.x].distanceToEnd = Global.CalcTileDistance(neighbor, endTile);
                }
                else if(inOpen && discoveredDistance < distances[neighbor.y, neighbor.x].travelDistance) {
                    // found a shorter route
                    distances[neighbor.y, neighbor.x].previous = nextTile;
                    distances[neighbor.y, neighbor.x].travelDistance = discoveredDistance;
                }
            }
        }

        return null; // no valid path
    }

    private struct PathData {
        public Vector2Int? previous;
        public int travelDistance;
        public int distanceToEnd;

        public int Estimate { get { return travelDistance + distanceToEnd; } }
    }
}
