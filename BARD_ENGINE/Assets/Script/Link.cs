using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Link : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image head;
    [SerializeField]
    private Image body;
    
    public bool IsActive;

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
        if (!AppManager.Instance.InputController.IsDrawingLink)
            IsActive = !IsActive;
    }
}
