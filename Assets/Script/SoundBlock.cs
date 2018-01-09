using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
        data.Add(new Dropdown.OptionData(" "));

        foreach (AudioClip clip in AppManager.Instance.clips)
        {
            data.Add(new Dropdown.OptionData(clip.name));
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(data);
        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(); });
	}
	
	void Update()
    {
        if (link != null)
        {
            link.GetComponent<Transform>().position = front.transform.position;

            Vector3 To;
            if (nextBlock == null)
                To = Input.mousePosition;
            else
                To = nextBlock.rear.GetComponent<RectTransform>().position;

            Vector3 FromTo = To - link.GetComponent<RectTransform>().position;
            link.GetComponent<RectTransform>().sizeDelta = new Vector2(FromTo.magnitude, 5);
            link.GetComponent<RectTransform>().right = FromTo;
        }

        if (source != null && source.clip != null)
        {
            if (source.time >= source.clip.length - 1)
            {
                if (source.loop && ((link == null) || (!link.IsActive )) )
                {
                    source.time = 1;
                }
                else
                {
                    StopSound();
                    
                    if (nextBlock)
                    {
                        nextBlock.PlaySound();
                    }
                }
            }

            float ratio = (source.time - 1) / (source.clip.length - 2);
            slider.value = ratio;
        }
    }

    public void PlaySound()
    {
        if (source != null)
        {
            source.time = 1;
            source.Play();

            AppManager.Instance.SetActiveSoundBlock(this);
        }
    }

    public void StopSound()
    {
        if (source != null)
            source.Stop();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AppManager.Instance.IsDrawingLink)
        {
            AppManager.Instance.SoundblockClicked(this);
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

    public void DropdownValueChanged()
    {
        foreach (AudioClip clip in AppManager.Instance.clips)
        {
            if (clip.name == dropdown.options[dropdown.value].text)
            {
                if (source != null)
                {
                    source.clip = clip;

                    if (clip.name.Contains("Loop"))
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
