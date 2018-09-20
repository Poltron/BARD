using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum LinkType
{
    Cut = 0,
    CrossFade,
    DemiCrossFade
}

public class Link : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image head;
    [SerializeField]
    private Image body;
    
    public bool IsActive;

    public SoundBlock previousBlock;
    public LinkType linkType;
    public SoundBlock nextBlock;

    void Update()
    {
		if (IsActive)
        {
            head.color = Color.red;
            body.color = Color.red;
        }
        else
        {
            head.color = Color.black;
            body.color = Color.black;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AppManager.Instance.GUIManager.ScenarioView.SetActiveLink(this);
    }
}
