using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int mazeWidth = 4;
    [SerializeField] private float mazeHeight = 4;
    [SerializeField] private GameObject roomPrefab;
    private Stack<GameObject> callStack = new Stack<GameObject>();
    private Dictionary<Vector2Int, GameObject> cells = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> visited = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] private Vector2Int currentPos = new Vector2Int(0,0);

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
                cells[new Vector2Int(i, j)] = room.gameObject;
            }
        }

        MoveThroughMaze();
    }

    private void MoveThroughMaze()
    {
        List<GameObject> neighbors = new List<GameObject>();

        // East
        if ((currentPos.x + 1) < mazeWidth && TryGetVisitedGameObject(new Vector2Int(currentPos.x+1, currentPos.y)) == null) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x + 1, currentPos.y)]);
        }

        // West
        if ((currentPos.x - 1) > 0 && TryGetVisitedGameObject(new Vector2Int(currentPos.x - 1, currentPos.y)) == null) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x - 1, currentPos.y)]);
        }

        // North
        if ((currentPos.y + 1) < mazeHeight && TryGetVisitedGameObject(new Vector2Int(currentPos.x, currentPos.y + 1)) == null) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y + 1)]);
        }

        // South
        if ((currentPos.y - 1) > 0 && TryGetVisitedGameObject(new Vector2Int(currentPos.x, currentPos.y + 1)) == null) // GET RID OF VISITED STUFF, IT IS FOR LATER
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y + 1)]);
        }

        if (neighbors.Count == 0)
        {
            return;
        }
        else if (neighbors.Count == 1)
        {
            callStack.Push(neighbors.ElementAt(0));
        }
        else
        {
            int randomValue = UnityEngine.Random.Range(0, neighbors.Count);
            callStack.Push(neighbors.ElementAt(randomValue));
            visited[currentPos] = cells[currentPos];
        }
    }

    private GameObject TryGetVisitedGameObject(Vector2Int index)
    {
        if(visited.TryGetValue(index, out GameObject cell))
        {
            return cell;
        }
        return null;
    }
}
