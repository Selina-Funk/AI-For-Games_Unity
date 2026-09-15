using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int mazeWidth = 4;
    [SerializeField] private float mazeHeight = 4;
    [SerializeField] private GameObject roomPrefab;
    private Stack<GameObject> callStack;
    private Dictionary<Vector2Int, GameObject> cells;
    private Dictionary<Vector2Int, GameObject> visited;
    private Vector2Int currentPos;

    private void Awake()
    {
        for (int i = 0; i < mazeWidth; i++)
        {
            for (int j = 0; j < mazeHeight; j++)
            {
                GameObject room = Instantiate(roomPrefab, new Vector2(i, -j), Quaternion.identity);
                room.name = $"Room_{i}_{j}";
                room.transform.SetParent(GameObject.Find("Maze").transform);
                room.GetComponent<Room>().SetPosition(new Vector2Int(i, j));
                cells.Add(new Vector2Int(i, j), room);
            }
        }
    }

    private void MoveThroughMaze()
    {
        if (currentPos.IsUnityNull())
        {
            currentPos = new Vector2Int(0, 0);
        }

        List<GameObject> neighbors = new List<GameObject>();

        // East
        if ((currentPos.x + 1) < mazeWidth && !visited[(new Vector2Int(currentPos.x + 1, currentPos.y))]) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x + 1, currentPos.y)]);
        }

        // West
        if ((currentPos.x - 1) < mazeWidth && !visited[(new Vector2Int(currentPos.x - 1, currentPos.y))]) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x - 1, currentPos.y)]);
        }

        // North
        if ((currentPos.y + 1) < mazeWidth && !visited[(new Vector2Int(currentPos.x, currentPos.y + 1))]) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y + 1)]);
        }

        // South
        if ((currentPos.y + 1) < mazeWidth && !visited[(new Vector2Int(currentPos.x, currentPos.y + 1))]) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y + 1)]);
        }

        int indexCount = 0;

        foreach (var neighbor in neighbors)
        {
            if (visited[neighbor.GetComponent<Room>().GetPosition()])
            {
                continue;
            }
        }
    }
}
