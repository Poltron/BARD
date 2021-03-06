﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Xml.Serialization;
using System.IO;

public class SoundBlock : MonoBehaviour
     , IPointerClickHandler
     , IDragHandler
     , IPointerEnterHandler
     , IPointerExitHandler
{

    [HideInInspector]
    public AudioSource source;

    public SoundBlock nextBlock;

    public Link link;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Dropdown dropdown;

    public Image rear;

    public Image front;

    public int soundblockId;

    private bool initialized;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void UpdateSoundlist()
    {
        List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
        data.Add(new Dropdown.OptionData(" "));

        foreach (Resource resource in AppManager.Instance.ResourcesManager.Resources)
        {
            data.Add(new Dropdown.OptionData(resource.Name));
        }
        
        dropdown.ClearOptions();
        dropdown.AddOptions(data);

        if (!initialized)
        {
            dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(); });
            initialized = true;
        }
    }

    void Start()
    {
    }
	
	void Update()
    {
        if (link != null)
        {
            link.GetComponent<Transform>().position = front.transform.position;

            Vector3 To;
            float div = 1.0f;
            if (nextBlock == null)
            {
                To = Input.mousePosition;
            }
            else
            {
                To = nextBlock.rear.GetComponent<RectTransform>().position;
            }

            Vector3 FromTo = To - front.transform.position;

            link.GetComponent<RectTransform>().sizeDelta = new Vector2(FromTo.magnitude / div, 5); // 0.64f c'est le scale du canvas scaler.... wtf mate
            link.GetComponent<RectTransform>().right = FromTo;
        }

        if (source != null && source.clip != null)
        {
            if (source.time >= source.clip.length - 0.5f)
            {
                if (source.loop && ((link == null) || !AppManager.Instance.GUIManager.ScenarioView.goToNextSound))
                {
                    source.time = 0;
                }
                else
                {
                    AppManager.Instance.GUIManager.ScenarioView.goToNextSound = false;

                    StopSound();
                    
                    if (nextBlock)
                    {
                        nextBlock.PlaySound();
                    }
                }
            }

            float ratio = source.time / source.clip.length;
            slider.value = ratio;
        }
    }

    public void PlaySound()
    {
        if (source != null)
        {
            source.time = 0;
            source.Play();

            AppManager.Instance.ScenarioManager.SetActiveSoundBlock(this);
        }
    }

    public void StopSound()
    {
        if (source != null)
            source.Stop();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AppManager.Instance.InputController.IsDrawingLink)
        {
            AppManager.Instance.InputController.SoundblockClicked(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //GetComponent<Image>().color = new Color(240.0f / 255.0f, 240.0f / 255.0f, 240.0f / 255.0f, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //GetComponent<Image>().color = Color.white;
    }

    public void SetClip(string name)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == name)
                dropdown.value = i;
        }
    }

    public void DropdownValueChanged()
    {
        foreach (Resource resource in AppManager.Instance.ResourcesManager.Resources)
        {
            if (resource.Name == dropdown.options[dropdown.value].text)
            {
                if (source != null)
                {
                    source.clip = resource.Clip;

                    if (source.clip.name.Contains("Loop"))
                    {
                        source.loop = true;

                        if (link != null)
                            link.IsActive = false;
                    }
                    else
                    {
                        source.loop = false;

                        if (link != null)
                            link.IsActive = true;

                    }
                }

                break;
            }
        }
    }
}
