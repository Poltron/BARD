﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField]
    private GameObject LinkPrefab;

    public bool IsDrawingLink;

    private SoundBlock firstSoundblockClick;

    /*
     *      LINK DRAWING
     */

    public void EnterDrawLink()
    {
        IsDrawingLink = true;
    }

    public void DrawLink(SoundBlock soundBlock)
    {
        if (!firstSoundblockClick)
        {
            firstSoundblockClick = soundBlock;
            GameObject go = GameObject.Instantiate(LinkPrefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(firstSoundblockClick.transform);

            Link link = go.GetComponent<Link>();
            firstSoundblockClick.link = link;
            
            if (!firstSoundblockClick.source.loop)
                firstSoundblockClick.link.IsActive = true;
            else
                firstSoundblockClick.link.IsActive = false;

            link.previousBlock = firstSoundblockClick;
        }
        else if (firstSoundblockClick != soundBlock)
        {
            firstSoundblockClick.nextBlock = soundBlock;
            firstSoundblockClick.link.nextBlock = soundBlock;

            StopDrawLink();
        }
    }

    public void StopDrawLink()
    {
        IsDrawingLink = false;
        firstSoundblockClick = null;
    }

    public void SoundblockClicked(SoundBlock soundBlock)
    {
        if (IsDrawingLink)
        {
            DrawLink(soundBlock);
        }
    }
}
