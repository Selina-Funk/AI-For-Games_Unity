using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int mazeWidth = 4;
    [SerializeField] private int mazeHeight = 4;
    [SerializeField] private GameObject roomPrefab;
    private Stack<GameObject> callStack = new Stack<GameObject>();
    private Dictionary<Vector2Int, GameObject> cells = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> visited = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] private Vector2Int currentPos = new Vector2Int(0,0);

    private List<GameObject> randomPrimList = new List<GameObject>();

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

        StartCoroutine(GoThroughMaze(0.3f));
        //StartCoroutine(RandomPrimMaze(0.5f));
    }

    private void MoveThroughMaze()
    {
        if (callStack.Count <= 0)
        {
            callStack.Push(cells[currentPos]);
        }
        GameObject current = callStack.Peek();

        current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        currentPos = current.GetComponent<Room>().GetPosition();

        List<GameObject> neighbors = new List<GameObject>();

        // East
        if ((currentPos.x + 1) < mazeWidth && TryGetVisitedGameObject(new Vector2Int(currentPos.x + 1, currentPos.y)) == null)
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x + 1, currentPos.y)]);
        }

        // West
        if ((currentPos.x - 1) >= 0 && TryGetVisitedGameObject(new Vector2Int(currentPos.x - 1, currentPos.y)) == null)
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x - 1, currentPos.y)]);
        }

        // North
        if ((currentPos.y + 1) < mazeHeight && TryGetVisitedGameObject(new Vector2Int(currentPos.x, currentPos.y + 1)) == null)
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y + 1)]);
        }

        // South
        if ((currentPos.y - 1) >= 0 && TryGetVisitedGameObject(new Vector2Int(currentPos.x, currentPos.y - 1)) == null)
        {
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y - 1)]);
        }

        if (neighbors.Count == 0)
        {
            GameObject obj = callStack.Pop();
            obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
            obj = callStack.Peek();
            obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
            visited[currentPos] = cells[currentPos];
            return;
        }
        else if (neighbors.Count == 1)
        {
            GameObject nextCell = neighbors[0];
            callStack.Push(neighbors[0]);
            visited[currentPos] = cells[currentPos];

            RemoveWall(nextCell, current);

            current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.purple;
            nextCell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            int randomValue = UnityEngine.Random.Range(0, neighbors.Count);
            GameObject nextCell = neighbors[randomValue];
            callStack.Push(nextCell);
            visited[currentPos] = cells[currentPos];

            RemoveWall(nextCell, current);

            current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.purple;
            nextCell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void RandomPrim()
    {
        
        if (randomPrimList.Count <= 0)
        {
            currentPos = new Vector2Int(mazeWidth / 2, mazeHeight / 2);
            randomPrimList.Add(cells[currentPos]);
        }

        int randomObj = UnityEngine.Random.Range(0, randomPrimList.Count);
        Room chosenObj = randomPrimList[randomObj].GetComponent<Room>();
        currentPos = chosenObj.GetPosition();

        // Add East Object
        if ((chosenObj.GetPosition().x + 1) < mazeWidth && !randomPrimList.Contains(cells[new Vector2Int(chosenObj.GetPosition().x + 1, chosenObj.GetPosition().y)]) && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x + 1, chosenObj.GetPosition().y)) == null)
        {
            cells[new Vector2Int(chosenObj.GetPosition().x + 1, chosenObj.GetPosition().y)].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.blue;
            randomPrimList.Add(cells[new Vector2Int(chosenObj.GetPosition().x + 1, chosenObj.GetPosition().y)]);
        }

        // Add West Object
        if ((chosenObj.GetPosition().x - 1) >= 0 && !randomPrimList.Contains(cells[new Vector2Int(chosenObj.GetPosition().x - 1, chosenObj.GetPosition().y)]) && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x - 1, chosenObj.GetPosition().y)) == null)
        {
            cells[new Vector2Int(chosenObj.GetPosition().x - 1, chosenObj.GetPosition().y)].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.blue;
            randomPrimList.Add(cells[new Vector2Int(chosenObj.GetPosition().x - 1, chosenObj.GetPosition().y)]);
        }

        // Add North Object
        if ((chosenObj.GetPosition().y + 1) < mazeHeight && !randomPrimList.Contains(cells[new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y + 1)]) && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y + 1)) == null)
        {
            cells[new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y + 1)].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.blue;
            randomPrimList.Add(cells[new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y + 1)]);
        }

        // Add South Object
        if ((chosenObj.GetPosition().y - 1) >= 0 && !randomPrimList.Contains(cells[new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y -1)]) && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y - 1)) == null)
        {
            cells[new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y - 1)].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.blue;
            randomPrimList.Add(cells[new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y - 1)]);
        }

        // Deactivate East Wall
        if (TryGetVisitedGameObject(chosenObj.GetPosition()) == null && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x + 1, chosenObj.GetPosition().y)) != null)
        {
            chosenObj.GetWallByDir(Room.Directions.EAST).SetActive(false);
        }
        // Deactivate West Wall
        else if (TryGetVisitedGameObject(chosenObj.GetPosition()) == null && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x - 1, chosenObj.GetPosition().y)) != null)
        {
            cells[new Vector2Int(chosenObj.GetPosition().x - 1, chosenObj.GetPosition().y)].GetComponent<Room>().GetWallByDir(Room.Directions.EAST).SetActive(false);
        }
        // Deactivate North Wall
        else if (TryGetVisitedGameObject(chosenObj.GetPosition()) == null && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y - 1)) != null)
        {
            chosenObj.GetWallByDir(Room.Directions.NORTH).SetActive(false);
        }
        // Deactivate South Wall
        else if (TryGetVisitedGameObject(chosenObj.GetPosition()) == null && TryGetVisitedGameObject(new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y + 1)) != null)
        {
            cells[new Vector2Int(chosenObj.GetPosition().x, chosenObj.GetPosition().y + 1)].GetComponent<Room>().GetWallByDir(Room.Directions.NORTH).SetActive(false);
        }

        visited[currentPos] = chosenObj.gameObject;
        chosenObj.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
        randomPrimList.Remove(cells[currentPos]);
    }

    private void RemoveWall(GameObject nextCell, GameObject current)
    {
        if (nextCell.GetComponent<Room>().GetPosition().x - current.GetComponent<Room>().GetPosition().x == 1)
        {
            current.GetComponent<Room>().ChangeWallActivation(Room.Directions.EAST, false);
        }
        else if (nextCell.GetComponent<Room>().GetPosition().x - current.GetComponent<Room>().GetPosition().x == -1)
        {
            nextCell.GetComponent<Room>().ChangeWallActivation(Room.Directions.EAST, false);
        }
        else if (nextCell.GetComponent<Room>().GetPosition().y - current.GetComponent<Room>().GetPosition().y == 1)
        {
            nextCell.GetComponent<Room>().ChangeWallActivation(Room.Directions.NORTH, false);
        }
        else if (nextCell.GetComponent<Room>().GetPosition().y - current.GetComponent<Room>().GetPosition().y == -1)
        {
            current.GetComponent<Room>().ChangeWallActivation(Room.Directions.NORTH, false);
        }
    }

    private IEnumerator GoThroughMaze(float duration)
    {
        MoveThroughMaze();

        if (callStack.Count > 0)
        {
            yield return new WaitForSeconds(duration);
            StartCoroutine(GoThroughMaze(duration));
        }
        yield return null;
    }

    private IEnumerator RandomPrimMaze(float duration)
    {
        RandomPrim();

        if (randomPrimList.Count > 0)
        {
            yield return new WaitForSeconds(duration);
            StartCoroutine(RandomPrimMaze(duration));
        }

        yield return null;
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
