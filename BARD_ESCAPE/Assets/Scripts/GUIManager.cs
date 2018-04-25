using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GUIManager : MonoBehaviour
{
    [SerializeField]
    ScenarioManager scenarioManager;

    [SerializeField]
    SoundHandler soundHandler;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private RectTransform PlayScenarioMenu;

    [SerializeField]
    private RectTransform MainMenu;

    [SerializeField]
    private RectTransform HomeMenu;

    [SerializeField]
    private RectTransform LoadScenarioMenu;

    [SerializeField]
    private Text ScenarioName;

    [SerializeField]
    private Button Scenario1Button;

    [SerializeField]
    private Button Scenario2Button;

    [SerializeField]
    private Button Scenario3Button;

    public void GoToLoadScenarioMenu()
    {
        UpdateScenarioAvailableList();

        PlayScenarioMenu.gameObject.SetActive(false);
        MainMenu.gameObject.SetActive(true);
        LoadScenarioMenu.gameObject.SetActive(true);
        HomeMenu.gameObject.SetActive(false);
    }

    public void UpdateScenarioAvailableList()
    {
        Scenario1Button.gameObject.SetActive(false);
        Scenario2Button.gameObject.SetActive(false);
        Scenario3Button.gameObject.SetActive(false);

        if (!Directory.Exists("scenarios"))
        {
            Directory.CreateDirectory("scenarios");
            return;
        }

        string[] files = Directory.GetFiles("scenarios");

        int nb = 0;
        foreach (var file in files)
        {
            if (file.EndsWith(".bard"))
            {
                switch (nb)
                {
                    case 0:
                        Scenario1Button.GetComponentInChildren<Text>().text = file;
                        Scenario1Button.gameObject.SetActive(true);
                        break;
                    case 1:
                        Scenario2Button.GetComponentInChildren<Text>().text = file;
                        Scenario2Button.gameObject.SetActive(true);
                        break;
                    case 2:
                        Scenario3Button.GetComponentInChildren<Text>().text = file;
                        Scenario3Button.gameObject.SetActive(true);
                        break;
                    case 3:
                        Debug.LogError("Can't handle more than 3 scenarios");
                        break;
                }

                nb++;
            }
        }
    }

    public void LoadScenarioButton1()
    {
        scenarioManager.LoadScenario(Scenario1Button.GetComponentInChildren<Text>().text);
        ChangeScenarioName(Scenario1Button.GetComponentInChildren<Text>().text);
    }

    public void LoadScenarioButton2()
    {
        scenarioManager.LoadScenario(Scenario2Button.GetComponentInChildren<Text>().text);
        ChangeScenarioName(Scenario2Button.GetComponentInChildren<Text>().text);
    }

    public void LoadScenarioButton3()
    {
        scenarioManager.LoadScenario(Scenario3Button.GetComponentInChildren<Text>().text);
        ChangeScenarioName(Scenario3Button.GetComponentInChildren<Text>().text);
    }

    public void GoToMainMenu()
    {
        PlayScenarioMenu.gameObject.SetActive(false);
        MainMenu.gameObject.SetActive(true);
        LoadScenarioMenu.gameObject.SetActive(false);
        HomeMenu.gameObject.SetActive(true);
    }

    public void GoToScenarioPlaying()
    {
        PlayScenarioMenu.gameObject.SetActive(true);
        MainMenu.gameObject.SetActive(false);
        LoadScenarioMenu.gameObject.SetActive(false);
        HomeMenu.gameObject.SetActive(false);
    }

    public void ResetScenario()
    {
        soundHandler.ResetScenario();
    }

    public void PlayButton()
    {
        soundHandler.PlayScenario();
    }

    public void PauseButton()
    {
        soundHandler.PauseScenario();
    }

    public void StopButton()
    {
        soundHandler.StopScenario();
    }

    public void ChangeScenarioName(string text)
    {
        ScenarioName.text = text;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
