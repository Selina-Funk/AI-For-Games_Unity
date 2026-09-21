using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MazeGenerator : MonoBehaviour
{
    [Header("General Maze Info")]
    [SerializeField] private int mazeWidth = 4;
    [SerializeField] private int mazeHeight = 4;
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Vector2Int currentPos = new Vector2Int(0, 0);
    private Dictionary<Vector2Int, GameObject> cells = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> visited = new Dictionary<Vector2Int, GameObject>();
    private MazeType mazeAlgorithm;
    private Coroutine runningAlgorithm;
    
    [Header("Recursive Backtracking")]
    private Stack<GameObject> callStack = new Stack<GameObject>();

    [Header("Random Prim")]
    private List<GameObject> randomPrimList = new List<GameObject>();

    [Header("Hunt and Kill")]
    Vector2Int startPos;
    private bool noNeibhors = false;
    Queue<GameObject> frontier = new Queue<GameObject>();

    public enum MazeType
    {
        DFS,
        RANDOMPRIM,
        HUNTANDKILL
    }

    private void Awake()
    {
        mazeAlgorithm = MazeType.DFS;

        InitMaze();

        //DFSSetUp();
        //StartCoroutine(RandomPrimMaze(0.2f));
        //StartCoroutine(HuntAndKillMaze(0.2f));
    }

    public void InitMaze()
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

        Camera camera = Camera.main;
        Room centerRoom = cells[new Vector2Int(mazeWidth / 2, mazeHeight / 2)].GetComponent<Room>();
        camera.transform.position = new Vector3(centerRoom.GetPosition().x, -centerRoom.GetPosition().y, camera.transform.position.z);
        
        float AspectRation = (float)Screen.width / (float)Screen.height;
        float horizontalSize = ((mazeWidth / AspectRation) / 2.0f) + 2;
        float verticalSize = ((mazeHeight / AspectRation) / 2.0f) + 4;
        camera.orthographicSize = Mathf.Max(horizontalSize, verticalSize) * 1.5f;

        frontier.Enqueue(cells[currentPos]);
        startPos = currentPos;
        cells[currentPos].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
    }

    public void RemakeMaze()
    {
        frontier.Clear();
        foreach(var child in cells.Values)
        {
            Destroy(child.gameObject);
        }
        cells = new Dictionary<Vector2Int, GameObject>();
        InitMaze();
    }

    private void DFSSetUp()
    {
        currentPos = new Vector2Int(0, 0);
        frontier.Enqueue(cells[currentPos]);
        startPos = currentPos;
        cells[currentPos].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        callStack.Push(cells[currentPos]);

        runningAlgorithm = StartCoroutine(RecursiveBacktrackMaze(0.1f));
    }

    private void RecursiveBacktracking()
    {
        visited[currentPos] = cells[currentPos];
        cells[currentPos].GetComponent<Room>().SetVisited(true);
        GameObject current = callStack.Peek();
        if (current == null) return;

        current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        currentPos = current.GetComponent<Room>().GetPosition();

        List<GameObject> neighbors = new List<GameObject>();

        // East
        if (cells.TryGetValue(new Vector2Int(currentPos.x + 1, currentPos.y), out GameObject neighbor))
        {
            if (!neighbor.GetComponent<Room>().GetVisited()) neighbors.Add(neighbor);
        }
        
        // West
        if (cells.TryGetValue(new Vector2Int(currentPos.x - 1, currentPos.y), out GameObject neighborW))
        {
            if (!neighborW.GetComponent<Room>().GetVisited()) neighbors.Add(neighborW);
        }
        
        // North
        if (cells.TryGetValue(new Vector2Int(currentPos.x, currentPos.y + 1), out GameObject neighborN))
        {
            if (!neighborN.GetComponent<Room>().GetVisited()) neighbors.Add(neighborN);
        }
        
        // South
        if (cells.TryGetValue(new Vector2Int(currentPos.x, currentPos.y - 1), out GameObject neighborS))
        {
            if (!neighborS.GetComponent<Room>().GetVisited()) neighbors.Add(neighborS);
        }

        if (neighbors.Count == 0)
        {
            GameObject obj = callStack.Pop();
            obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
            if (callStack.Count > 0)
            {
                obj = callStack.Peek();
                if (obj != null) obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
            }
            return;
        }
        else if (neighbors.Count == 1)
        {
            GameObject nextCell = neighbors[0];
            callStack.Push(neighbors[0]);
            RemoveWall(nextCell, current);

            current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.purple;
            nextCell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            int randomValue = UnityEngine.Random.Range(0, neighbors.Count);
            GameObject nextCell = neighbors[randomValue];
            callStack.Push(nextCell);
            
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

    public void HuntAndKill()
    {
        GameObject current = cells[currentPos];
        current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;

        List<GameObject> neighbors = new List<GameObject>();

        // East
        if ((currentPos.x + 1) < mazeWidth && TryGetVisitedGameObject(new Vector2Int(currentPos.x + 1, currentPos.y)) == null)
        {
            noNeibhors = false;
            neighbors.Add(cells[new Vector2Int(currentPos.x + 1, currentPos.y)]);
        }

        // West
        if ((currentPos.x - 1) >= 0 && TryGetVisitedGameObject(new Vector2Int(currentPos.x - 1, currentPos.y)) == null)
        {
            noNeibhors = false;
            neighbors.Add(cells[new Vector2Int(currentPos.x - 1, currentPos.y)]);
        }

        // North
        if ((currentPos.y + 1) < mazeHeight && TryGetVisitedGameObject(new Vector2Int(currentPos.x, currentPos.y + 1)) == null)
        {
            noNeibhors = false;
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y + 1)]);
        }

        // South
        if ((currentPos.y - 1) >= 0 && TryGetVisitedGameObject(new Vector2Int(currentPos.x, currentPos.y - 1)) == null)
        {
            noNeibhors = false;
            neighbors.Add(cells[new Vector2Int(currentPos.x, currentPos.y - 1)]);
        }

        if (noNeibhors)
        {
            cells[currentPos].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
            if (startPos.x + 1 < mazeWidth)
            {
                startPos = new Vector2Int(startPos.x + 1, startPos.y);
                frontier.Enqueue(cells[startPos]);
            }
            else if (startPos.y + 1 < mazeHeight)
            {
                startPos = new Vector2Int(0, startPos.y + 1);
                frontier.Enqueue(cells[startPos]);
            }
            currentPos = startPos;
            GameObject obj = frontier.Dequeue();
            obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            visited[currentPos] = cells[currentPos];
            cells[currentPos].GetComponent<Room>().SetVisited(true);
        }

        if (neighbors.Count == 0)
        {
            cells[currentPos].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
            noNeibhors = true;
            currentPos = startPos;
            cells[currentPos].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;

        }
        else if (neighbors.Count == 1)
        {
            GameObject nextCell = neighbors[0];
            visited[currentPos] = cells[currentPos];

            if (TryGetVisitedGameObject(nextCell.GetComponent<Room>().GetPosition()) == null)
            {
                RemoveWall(nextCell, cells[currentPos]);
            }

            currentPos = nextCell.GetComponent<Room>().GetPosition();
            current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
            nextCell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            int randomCell = UnityEngine.Random.Range(0, neighbors.Count);
            GameObject nextCell = neighbors[randomCell];
            visited[currentPos] = cells[currentPos];
            if (TryGetVisitedGameObject(nextCell.GetComponent<Room>().GetPosition()) == null)
            {
                RemoveWall(nextCell, cells[currentPos]);
            }
            currentPos = nextCell.GetComponent<Room>().GetPosition();
            current.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
            nextCell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public void FireMaze()
    {
        switch(mazeAlgorithm)
        {
            case MazeType.DFS:
                DFSSetUp();
                return;
            case MazeType.RANDOMPRIM:
                runningAlgorithm = StartCoroutine(RandomPrimMaze(0.2f));
                return;
            case MazeType.HUNTANDKILL:
                runningAlgorithm = StartCoroutine(HuntAndKillMaze(0.2f));
                return;
            default:
                return;
        }
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

    private IEnumerator RecursiveBacktrackMaze(float duration)
    {
        RecursiveBacktracking();

        if (callStack.Count > 0)
        {
            yield return new WaitForSeconds(duration);
            StartCoroutine(RecursiveBacktrackMaze(duration));
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

    private IEnumerator HuntAndKillMaze(float duration)
    {
        HuntAndKill();

        if (frontier.Count > 0)
        {
            yield return new WaitForSeconds(duration);
            StartCoroutine(HuntAndKillMaze(duration));
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

    public MazeType GetMazeAlgorithm()
    {
        return mazeAlgorithm;
    }

    public void SetMazeAlgorithm(MazeType type)
    {
        mazeAlgorithm = type;
    }

    public void SetMazeWidth(int newWidth)
    {
        mazeWidth = newWidth;
    }

    public int GetMazeWidth()
    {
        return mazeWidth;
    }

    public void SetMazeHeight(int newHeight)
    {
        mazeHeight = newHeight;
    }

    public int GetMazeHeight()
    {
        return mazeHeight;
    }

    public Coroutine GetRunningAlgorithm()
    {
        return runningAlgorithm;
    }
}
