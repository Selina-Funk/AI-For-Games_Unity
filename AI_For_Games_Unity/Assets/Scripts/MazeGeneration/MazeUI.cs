using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class MazeUI : MonoBehaviour
{
    [SerializeField] private Slider widthSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private TMP_Dropdown algorithmDropdown;

    [SerializeField] private TextMeshProUGUI widthNumberText;
    [SerializeField] private TextMeshProUGUI heightNumberText;

    private MazeGenerator mazeGenerator;


    private bool isRunning = false;

    private void Awake()
    {
        mazeGenerator = GameObject.Find("MazeGenerator").GetComponent<MazeGenerator>();
        widthNumberText.text = mazeGenerator.GetMazeWidth().ToString();
        heightNumberText.text = mazeGenerator.GetMazeHeight().ToString();
    }

    public void SetWidthNumber()
    {
        if (mazeGenerator.GetRunningAlgorithm() != null) StopCoroutine(mazeGenerator.GetRunningAlgorithm());
        isRunning = false;
        widthNumberText.text = widthSlider.value.ToString();
        mazeGenerator.SetMazeWidth((int)widthSlider.value);

        mazeGenerator.RemakeMaze();
    }

    public void SetHeightNumber()
    {
        if (mazeGenerator.GetRunningAlgorithm() != null) StopCoroutine(mazeGenerator.GetRunningAlgorithm());
        isRunning = false;
        heightNumberText.text = heightSlider.value.ToString();
        mazeGenerator.SetMazeHeight((int)heightSlider.value);

        mazeGenerator.RemakeMaze();
    }

    public void ChangeMazeAlgo()
    {
        if (algorithmDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "DFS")
        {
            mazeGenerator.SetMazeAlgorithm(MazeGenerator.MazeType.DFS);
        }
        else if (algorithmDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Random Prim")
        {
            mazeGenerator.SetMazeAlgorithm(MazeGenerator.MazeType.RANDOMPRIM);
        }
        else if (algorithmDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Hunt And Kill")
        {
            mazeGenerator.SetMazeAlgorithm(MazeGenerator.MazeType.HUNTANDKILL);
        }
    }

    public void GenerateMaze()
    {
        if (!isRunning)
        {
            mazeGenerator.FireMaze();
            isRunning = true;
        }
        //else
        //{
        //    StopCoroutine(mazeGenerator.GetRunningAlgorithm());
        //    mazeGenerator.GetVisited().Clear();
        //    mazeGenerator.RemakeMaze();
        //    mazeGenerator.FireMaze();
        //}
    }

    public void ResetGeneration()
    {
        StopCoroutine(mazeGenerator.GetRunningAlgorithm());
        mazeGenerator.GetVisited().Clear();
        mazeGenerator.RemakeMaze();
        mazeGenerator.FireMaze();
    }
}
