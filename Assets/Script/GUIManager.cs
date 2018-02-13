using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public RectTransform canvas;

    public RectTransform scenarioOrigin;

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

    void Start () {
		
	}
	
	void Update () {
		
	}

    public void EnableScenarioUI()
    {
        scenarioName.rectTransform.parent.gameObject.SetActive(true);

        saveScenario.gameObject.SetActive(true);
        startTrack.gameObject.SetActive(true);
        stopTrack.gameObject.SetActive(true);
        addSoundblock.gameObject.SetActive(true);
        drawLink.gameObject.SetActive(true);
        importAudioFile.gameObject.SetActive(true);
    }

    public void ChangeScenarioName(string newName)
    {
        scenarioName.text = newName;
    }
}
