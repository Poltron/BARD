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
    private RectTransform resourceListContent;
    [SerializeField]
    private GameObject resourceListEntryPrefab;

    private List<RectTransform> resourceListEntries;

    [SerializeField]
    private RectTransform resourceWavePanel;

    [SerializeField]
    private RectTransform resourceDetailsPanel;

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

    void Awake()
    {
        resourceListEntries = new List<RectTransform>();
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
        go.transform.SetParent(resourceListContent);
        go.transform.localPosition = new Vector3( 15, -15 + resourceListEntries.Count * -40, 0);

        go.GetComponent<ResourceViewEntry>().SetResource(resource.Id);
        go.GetComponentInChildren<Text>().text = resource.Name;

        resourceListEntries.Add(go.GetComponent<RectTransform>());
    }

    public void ViewResourceEntry(int resourceId)
    {
        Resource res = AppManager.Instance.ResourcesManager.GetResource(resourceId);
        resourceDetailsPanel.GetComponentInChildren<Text>().text = "Name : " + res.Name + "            " + "Id : " + res.Id + "\n\n" + "Length : " + res.Clip.length + " sec" + "            " + "Frequency : " + res.Clip.frequency + "            " + "Channels : " + res.Clip.channels;
        resourceWavePanel.GetComponent<SoundwaveDrawer>().Draw(res.Clip);
    }
}
