using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioView : MonoBehaviour
{
    [SerializeField]
    private LinkDetailsView linkDetailsView;

    public Link activeLink;

    [SerializeField]
    private GameObject goingNextSoundPanel;

    public bool goToNextSound;

	void Update ()
    {
		if (isActiveAndEnabled && AppManager.Instance.ScenarioManager.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                goToNextSound = true;
            }
        }
        
        goingNextSoundPanel.SetActive(goToNextSound);
    }

    public void SetActiveLink(Link link)
    {
        if (!AppManager.Instance.InputController.IsDrawingLink)
        {
            link.IsActive = !link.IsActive;

            // si on active un link, si il y avait déjà un link actif on le désactive
            if (link.IsActive && (activeLink != null && activeLink.IsActive))
            {
                activeLink.IsActive = false;
            }

            activeLink = link;

            if (!AppManager.Instance.ScenarioManager.isPlaying)
            {
                if (link.IsActive)
                    ShowLinkDetails(link);
                else
                    HideLinkDetails();
            }
        }
    }

    public void ShowLinkDetails(Link link)
    {
        linkDetailsView.Show();
        linkDetailsView.SetLink(link);
    }

    public void HideLinkDetails()
    {
        linkDetailsView.Hide();
    }
}
