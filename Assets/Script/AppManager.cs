using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AppManager : Singleton<AppManager>
{
    protected AppManager() { }

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private GraphicRaycaster raycaster;

    [SerializeField]
    private GameObject SoundBlockPrefab;

    [SerializeField]
    private GameObject LinkPrefab;

    [SerializeField]
    SoundBlock firstBlock;

    public SoundBlock activeSoundBlock;

    public bool IsDrawingLink;

    private SoundBlock firstSoundblockClick;

    public List<SoundBlock> blocks;
    public AudioClip[] clips;

    void Start ()
    {
        blocks = new List<SoundBlock>();
    }
	
	void Update ()
    {
    }

    public void StartSoundTrack()
    {
        firstBlock.PlaySound();
        activeSoundBlock = firstBlock;
    }

    public void SetActiveSoundBlock(SoundBlock active)
    {
        activeSoundBlock = active;
    }

    public void StopSoundTrack()
    {
        activeSoundBlock.StopSound();
    }

    public void SpawnSoundBlock()
    {
        GameObject blockGO = GameObject.Instantiate(SoundBlockPrefab, Vector3.zero, Quaternion.identity);
        blockGO.transform.SetParent(canvas.transform);
        SoundBlock soundBlock = blockGO.GetComponent<SoundBlock>();

        if (blocks.Count == 0)
        {
            firstBlock = soundBlock;
            firstBlock.GetComponent<Image>().color = Color.green;
            firstBlock.rear.gameObject.SetActive(false);
        }

        blocks.Add(soundBlock);
    }

    public void DrawSoundBlockLink()
    {
        IsDrawingLink = true;
    }

    public void SoundblockClicked(SoundBlock soundBlock)
    {
        if (IsDrawingLink)
        {
            if (!firstSoundblockClick)
            {
                firstSoundblockClick = soundBlock;
                GameObject go = GameObject.Instantiate(LinkPrefab, Vector3.zero, Quaternion.identity);
                go.transform.SetParent(firstSoundblockClick.transform);
                firstSoundblockClick.link = go.GetComponent<Link>();

                if (!firstSoundblockClick.source.loop)
                    firstSoundblockClick.link.IsActive = true;
                else
                    firstSoundblockClick.link.IsActive = false;

            }
            else if (firstSoundblockClick != soundBlock)
            {
                firstSoundblockClick.nextBlock = soundBlock;

                firstSoundblockClick = null;

                IsDrawingLink = false;
            }
        }
    }
}
