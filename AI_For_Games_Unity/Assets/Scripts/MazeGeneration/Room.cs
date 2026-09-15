using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum Directions
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    };

    [SerializeField] private GameObject northWall;
    [SerializeField] private GameObject eastWall;
    [SerializeField] private GameObject southWall;
    [SerializeField] private GameObject westWall;

    public Dictionary<Directions, GameObject> walls = new Dictionary<Directions, GameObject>();

    private Vector2Int position;

    //public bool visited = false;

    //public Vector2Int position;

    private void Start()
    {
        walls[Directions.NORTH] = northWall;
        walls[Directions.EAST] = eastWall;
        walls[Directions.SOUTH] = southWall;
        walls[Directions.WEST] = westWall;
    }

    public Vector2Int GetPosition()
    {
        return position;
    }

    public void SetPosition(Vector2Int pos)
    {
        position = pos;
    }
}
