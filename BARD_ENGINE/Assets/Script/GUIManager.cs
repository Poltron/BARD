using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public RectTransform canvas;

    public RectTransform scenarioOrigin;

    [SerializeField]
    private Button scenarioViewButton;
    [SerializeField]
    private Button resourceViewButton;
    [SerializeField]
    private Canvas scenarioView;
    [SerializeField]
    private Canvas resourceView;

    [SerializeField]
    private GameObject viewBar;

    [SerializeField]
    private Text scenarioName;
    [SerializeField]
    private Button saveScenario;
    [SerializeField]
    private Button startTrack;
    [SerializeField]
    private Button stopTrack;
    [SerializeField]
    private Button addSoundblock;
    [SerializeField]
    private Button drawLink;
    [SerializeField]
    private Button importAudioFile;

    void Start()
    {
        ToggleScenarioUI(false);
    }
    
    public void ToggleScenarioUI(bool shown)
    {
        saveScenario.gameObject.SetActive(shown);

        viewBar.gameObject.SetActive(shown);

        ToggleScenarioView(shown);
        ToggleResourceView(false);
    }

    public void ChangeScenarioName(string newName)
    {
        scenarioName.text = newName;
    }

    public void ToggleScenarioView(bool shown)
    {
        scenarioView.enabled = shown;
        scenarioView.gameObject.SetActive(shown);
        
        scenarioViewButton.interactable = !shown;
    }

    public void ToggleResourceView(bool shown)
    {
        resourceView.enabled = shown;
        resourceView.gameObject.SetActive(shown);
        
        resourceViewButton.interactable = !shown;
    }

    public void ScenarioViewButtonClick()
    {
        ToggleScenarioView(true);
        ToggleResourceView(false);
    }

    public void ResourceViewButtonClick()
    {
        ToggleScenarioView(false);
        ToggleResourceView(true);
    }
}
