using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class MazeUI : MonoBehaviour
{
    [SerializeField] private Slider widthSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider timeStepSlider;
    [SerializeField] private TMP_Dropdown algorithmDropdown;

    [SerializeField] private TextMeshProUGUI widthNumberText;
    [SerializeField] private TextMeshProUGUI heightNumberText;
    [SerializeField] private TextMeshProUGUI timeStepNumberText;

    private MazeGenerator mazeGenerator;


    private bool isRunning = false;

    private void Awake()
    {
        mazeGenerator = GameObject.Find("MazeGenerator").GetComponent<MazeGenerator>();
        widthNumberText.text = mazeGenerator.GetMazeWidth().ToString();
        heightNumberText.text = mazeGenerator.GetMazeHeight().ToString();
        timeStepNumberText.text = mazeGenerator.GetTimeStep().ToString();
    }

    public void SetWidthNumber()
    {
        if (mazeGenerator.GetRunningAlgorithm() != null) mazeGenerator.StopRunningMaze();
        isRunning = false;
        widthNumberText.text = widthSlider.value.ToString();
        mazeGenerator.SetMazeWidth((int)widthSlider.value);

        mazeGenerator.RemakeMaze();
    }

    public void SetHeightNumber()
    {
        if (mazeGenerator.GetRunningAlgorithm() != null) mazeGenerator.StopRunningMaze();
        isRunning = false;
        heightNumberText.text = heightSlider.value.ToString();
        mazeGenerator.SetMazeHeight((int)heightSlider.value);

        mazeGenerator.RemakeMaze();
    }

    public void SetTimeStep()
    {
        mazeGenerator.SetTimeStep(timeStepSlider.value);
        timeStepNumberText.text = timeStepSlider.value.ToString("F2");
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
    }

    public void ResetGeneration()
    {
        mazeGenerator.StopRunningMaze();
        mazeGenerator.GetVisited().Clear();
        mazeGenerator.RemakeMaze();
        isRunning = false;
        //mazeGenerator.FireMaze();
    }
}
