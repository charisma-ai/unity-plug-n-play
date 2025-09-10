using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTapContinuePrompt : MonoBehaviour
{
    [SerializeField] private GameObject _tapContinueText;

    public bool IsPromptActive { get; private set; }

    private void Start()
    {
        SetPromptActive(false);
    }
    
    public void SetPromptActive(bool active)
    {
        _tapContinueText.SetActive(active);
        IsPromptActive = active;
    }
}
