using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AppManager : Singleton<AppManager>
{
    protected AppManager() { }

    public ScenarioManager ScenarioManager;
    public GUIManager GUIManager;
    public InputController InputController;
    public ResourcesManager ResourcesManager;

    void Awake()
    {
    }
}