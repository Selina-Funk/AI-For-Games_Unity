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
                if (i == 0)
                {
                    room.GetComponent<Room>().ChangeWallActivation(Room.Directions.WEST, true);
                }
                if (j == (mazeHeight - 1))
                {
                    room.GetComponent<Room>().ChangeWallActivation(Room.Directions.SOUTH, true);
                }
                cells[new Vector2Int(i, j)] = room.gameObject;
            }
        }

        callStack.Push(cells[currentPos]);

        MoveThroughMaze();
    }

    private void Update()
    {
        foreach (var cell in callStack)
        {
            if (cell == callStack.Peek())
            {
                continue;
            }
            cell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.purple;
        }
    }

    private void MoveThroughMaze()
    {
        GameObject current = callStack.Peek();

        while (callStack.Count != 0)
        {
            if (visited.ContainsKey(current.GetComponent<Room>().GetPosition()))
            {
                current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
                callStack.Pop();
            }
            else
            {
                current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
                currentPos = current.GetComponent<Room>().GetPosition();
            }

            List<GameObject> neighbors = new List<GameObject>();

            // East
            if ((currentPos.x + 1) < mazeWidth && TryGetVisitedGameObject(new Vector2Int(currentPos.x + 1, currentPos.y)) == null) // GET RID OF VISITED STUFF, IT IS FOR LATER
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
                GameObject nextCell = neighbors[0];
                callStack.Push(neighbors[0]);

                if (nextCell.GetComponent<Room>().GetPosition().x - cells[currentPos].GetComponent<Room>().GetPosition().x == 1)
                {
                    cells[currentPos].GetComponent<Room>().ChangeWallActivation(Room.Directions.EAST, false);
                }
                else if (nextCell.GetComponent<Room>().GetPosition().x - cells[currentPos].GetComponent<Room>().GetPosition().x == 1)
                {
                    nextCell.GetComponent<Room>().ChangeWallActivation(Room.Directions.EAST, false);
                }
                else if (nextCell.GetComponent<Room>().GetPosition().y - cells[currentPos].GetComponent<Room>().GetPosition().y == 1)
                {
                    nextCell.GetComponent<Room>().ChangeWallActivation(Room.Directions.NORTH, false);
                }
                else if (nextCell.GetComponent<Room>().GetPosition().y - cells[currentPos].GetComponent<Room>().GetPosition().y == -1)
                {
                    cells[currentPos].GetComponent<Room>().ChangeWallActivation(Room.Directions.NORTH, false);
                }
            }
            else
            {
                int randomValue = UnityEngine.Random.Range(0, neighbors.Count);
                GameObject nextCell = neighbors[randomValue];
                callStack.Push(nextCell);
                visited[currentPos] = cells[currentPos];

                if (nextCell.GetComponent<Room>().GetPosition().x - cells[currentPos].GetComponent<Room>().GetPosition().x == 1)
                {
                    cells[currentPos].GetComponent<Room>().ChangeWallActivation(Room.Directions.EAST, false);
                }
                else if (nextCell.GetComponent<Room>().GetPosition().x - cells[currentPos].GetComponent<Room>().GetPosition().x == 1)
                {
                    nextCell.GetComponent<Room>().ChangeWallActivation(Room.Directions.EAST, false);
                }
                else if (nextCell.GetComponent<Room>().GetPosition().y - cells[currentPos].GetComponent<Room>().GetPosition().y == 1)
                {
                    nextCell.GetComponent<Room>().ChangeWallActivation(Room.Directions.NORTH, false);
                }
                else if (nextCell.GetComponent<Room>().GetPosition().y - cells[currentPos].GetComponent<Room>().GetPosition().y == -1)
                {
                    cells[currentPos].GetComponent<Room>().ChangeWallActivation(Room.Directions.NORTH, false);
                }
            }

            new WaitForSeconds(1.0f);
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
