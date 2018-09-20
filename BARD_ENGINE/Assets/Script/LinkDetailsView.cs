using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkDetailsView : MonoBehaviour
{
    [SerializeField]
    private Text fromBlock;

    [SerializeField]
    private Dropdown linkTypeDropdown;

    [SerializeField]
    private Text toBlock;

    private Link activeLink;

    void Start ()
    {
        linkTypeDropdown.ClearOptions();

        string[] labels = Enum.GetNames(typeof(LinkType));
        List<string> options = new List<string>(labels);
        linkTypeDropdown.AddOptions(options);
	}
	
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetLink(Link activeLink)
    {
        this.activeLink = activeLink;

        fromBlock.text = activeLink.previousBlock.soundblockId.ToString();
        linkTypeDropdown.value = (int)activeLink.linkType;
        toBlock.text = activeLink.nextBlock.soundblockId.ToString();
    }

    public void LinkValueChanged(int value)
    {
        activeLink.linkType = (LinkType)value;
    }
}
