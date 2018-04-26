using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResourceViewEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Color normalColor;

    [SerializeField]
    private Color hoverColor;

    int resourceID;

	void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void SetResource(int id)
    {
        resourceID = id;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AppManager.Instance.GUIManager.ViewResourceEntry(resourceID);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = normalColor;
    }

}
