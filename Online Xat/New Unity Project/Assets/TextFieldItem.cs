using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFieldItem : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI displayText;
    
    public void SetupText(string text)
    {
        displayText.text = text;
    }
}
