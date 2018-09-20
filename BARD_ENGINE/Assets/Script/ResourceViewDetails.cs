using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceViewDetails : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Text idText;

    [SerializeField]
    private Text lengthText;

    [SerializeField]
    private Text frequencyText;

    [SerializeField]
    private Text channelsText;

    [SerializeField]
    private InputField BPMField;

    [SerializeField]
    private InputField BPBField;

    [SerializeField]
    private InputField beginLoopField;

    [SerializeField]
    private InputField endLoopField;

    [SerializeField]
    private Button addTransitionButton;

    [SerializeField]
    private RectTransform transitionContent;

    [SerializeField]
    private GameObject transitionPrefab;

    private List<GameObject> transitionsView;

    private int activeResourceId;

    void Start()
    {
        activeResourceId = -1;
        content.gameObject.SetActive(false);
        transitionsView = new List<GameObject>();
    }
	
	void Update()
    {
		
	}

    public void SetDetails(int resourceId)
    {
        activeResourceId = resourceId;

        content.gameObject.SetActive(true);

        Resource res = AppManager.Instance.ResourcesManager.GetResource(resourceId);
        nameText.text = res.Name;
        idText.text = res.Id.ToString();
        lengthText.text = res.Clip.length.ToString() + "s";
        frequencyText.text = res.Clip.frequency + "Hz";
        channelsText.text = res.Clip.channels.ToString();
        BPMField.text = res.BPM.ToString();
        BPBField.text = res.BPB.ToString();
        beginLoopField.text = res.BeginLoop.ToString();
        endLoopField.text = res.EndLoop.ToString();

        ResetTransitionView();
        foreach (TransitionData transition in res.Transitions)
        {
            AddTransitionView(transition.id, transition.value);
        }

        Debug.Log("Set Details : " + res.Name + "/" + res.Id);
    }

    public void ChangeBPM(string newValue)
    {
        if (newValue == "")
            return;

        int intValue = int.Parse(newValue);
        if (intValue >= 60 && intValue <= 240)
        {
            AppManager.Instance.ResourcesManager.GetResource(activeResourceId).BPM = intValue;
            AppManager.Instance.ScenarioManager.UpdateAudioFile(activeResourceId);
        }
    }

    public void ChangeBPB(string newValue)
    {
        if (newValue == "")
            return;

        int intValue = int.Parse(newValue);
        if (intValue == 3 || intValue == 4)
        {
            AppManager.Instance.ResourcesManager.GetResource(activeResourceId).BPB = intValue;
            AppManager.Instance.ScenarioManager.UpdateAudioFile(activeResourceId);
        }

    }

    public void ChangeBeginLoop(string newValue)
    {
        if (newValue == "")
            return;

        float floatValue = float.Parse(newValue);
        AppManager.Instance.ResourcesManager.GetResource(activeResourceId).BeginLoop = floatValue;
        AppManager.Instance.ScenarioManager.UpdateAudioFile(activeResourceId);
    }

    public void ChangeEndLoop(string newValue)
    {
        if (newValue == "")
            return;

        float floatValue = float.Parse(newValue);
        AppManager.Instance.ResourcesManager.GetResource(activeResourceId).EndLoop = floatValue;
        AppManager.Instance.ScenarioManager.UpdateAudioFile(activeResourceId);
    }

    public void AddTransition()
    {
        Resource res = AppManager.Instance.ResourcesManager.GetResource(activeResourceId);

        TransitionData trData = new TransitionData(res.nextTransitionId, 0);
        res.Transitions.Add(trData);

        AddTransitionView(res.nextTransitionId, 0);

        res.nextTransitionId++;

        AppManager.Instance.ScenarioManager.UpdateAudioFile(activeResourceId);
    }

    public Transition AddTransitionView(int id, float value)
    {
        GameObject go = GameObject.Instantiate(transitionPrefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(transitionContent, false);

        transitionsView.Add(go);

        Transition tr = go.GetComponent<Transition>();
        tr.Initialize(this, id, value);

        return tr;
    }

    public void DeleteTransition(int toRemove)
    {
        Resource res = AppManager.Instance.ResourcesManager.GetResource(activeResourceId);
        TransitionData tr = res.Transitions.Find(x => x.id == toRemove);
        res.Transitions.Remove(tr);

        AppManager.Instance.ScenarioManager.UpdateAudioFile(activeResourceId);
    }

    public void TransitionValueChanged(int toEdit, float newValue)
    {
        Resource res = AppManager.Instance.ResourcesManager.GetResource(activeResourceId);
        TransitionData tr = res.Transitions.Find(x => x.id == toEdit);
        tr.value = newValue;

        AppManager.Instance.ScenarioManager.UpdateAudioFile(activeResourceId);
    }

    public void ResetTransitionView()
    {
        foreach (GameObject go in transitionsView)
        {
            GameObject.Destroy(go);
        }

        transitionsView.Clear();
    }
}
