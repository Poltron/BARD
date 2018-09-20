using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    private Button _deleteButton;
    private InputField _valueInputField;

    private ResourceViewDetails details;

    private int id;
    public int Id { get { return id; } }

    public void Initialize(ResourceViewDetails details, int id)
    {
        Initialize(details, id, 0);
    }

    public void Initialize(ResourceViewDetails details, int id, float value)
    {
        this.details = details;
        this.id = id;

        _deleteButton = GetComponentInChildren<Button>();
        _deleteButton.onClick.AddListener(OnDeleteButton);

        _valueInputField = GetComponentInChildren<InputField>();
        _valueInputField.text = value.ToString();
        _valueInputField.onValueChanged.AddListener(OnTransitionValueChanged);
    }

    public void OnDeleteButton()
    {
        details.DeleteTransition(id);
        GameObject.Destroy(gameObject);
    }

    public void OnTransitionValueChanged(string strValue)
    {
        float floatValue = float.Parse(strValue);
        details.TransitionValueChanged(id, floatValue);
    }
}
