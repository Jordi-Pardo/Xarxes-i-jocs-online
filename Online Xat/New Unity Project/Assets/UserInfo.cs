using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI userName;
    
    public void Setup(string name)
    {
        userName.text = name;
    }
    
}
