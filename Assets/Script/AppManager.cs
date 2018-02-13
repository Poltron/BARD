using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AppManager : Singleton<AppManager>
{
    protected AppManager() { }

    [HideInInspector]
    public ScenarioManager ScenarioManager;
    [HideInInspector]
    public GUIManager GUIManager;
    [HideInInspector]
    public InputController InputController;

    void Awake()
    {
        ScenarioManager = GetComponent<ScenarioManager>();
        GUIManager = GetComponent<GUIManager>();
        InputController = GetComponent<InputController>();
    }
}