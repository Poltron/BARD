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
    private ScenarioView scenarioView;
    public ScenarioView ScenarioView { get { return scenarioView; } }

    [SerializeField]
    private Canvas resourceView;
    [SerializeField]
    private RectTransform resourceListContent;
    [SerializeField]
    private GameObject resourceListEntryPrefab;

    private List<RectTransform> resourceListEntries;

    [SerializeField]
    private SoundwaveDrawer resourceWavePanel;

    [SerializeField]
    private ResourceViewDetails resourceDetailsPanel;
    public ResourceViewDetails ResourceViewDetails { get { return resourceDetailsPanel; } }

    [SerializeField]
    private GameObject viewBar;

    private int activeResourceViewId;

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

    [SerializeField]
    private LinkDetailsView linkDetailsView;

    void Awake()
    {
        resourceListEntries = new List<RectTransform>();
        activeResourceViewId = -1;
    }

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

    public void AddResourceViewListEntry(Resource resource)
    {
        GameObject go = GameObject.Instantiate(resourceListEntryPrefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(resourceListContent, false);

        go.GetComponent<ResourceViewEntry>().SetResource(resource.Id);
        go.GetComponentInChildren<Text>().text = resource.Name;

        resourceListEntries.Add(go.GetComponent<RectTransform>());
    }

    public void ViewResourceEntry(int resourceId)
    {
        if (activeResourceViewId == resourceId)
            return;

        activeResourceViewId = resourceId;

        Resource res = AppManager.Instance.ResourcesManager.GetResource(resourceId);

        resourceDetailsPanel.SetDetails(resourceId);
        resourceWavePanel.Draw(res.Clip);
    }
}
