using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceViewDetails : MonoBehaviour
{

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

    private int activeResourceId;

	void Start()
    {
        activeResourceId = -1;
        content.gameObject.SetActive(false);
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

        Debug.Log("Set Details : " + res.Name + "/" + res.Id);
    }

    public void ChangeBPM(string newValue)
    {
        if (newValue == "")
            return;

        int res = int.Parse(newValue);
        if (res >= 60 && res <= 240)
        {
            AppManager.Instance.ResourcesManager.GetResource(activeResourceId).BPM = res;
        }
    }

    public void ChangeBPB(string newValue)
    {
        if (newValue == "")
            return;

        int res = int.Parse(newValue);
        if (res == 3 || res == 4)
        {
            AppManager.Instance.ResourcesManager.GetResource(activeResourceId).BPB = res;
        }

    }
}
