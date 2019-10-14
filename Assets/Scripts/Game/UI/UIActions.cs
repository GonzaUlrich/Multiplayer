using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Drawing;

public class UIActions : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void Enter()
    {
        GameObject.Find("ChatScreen").GetComponent<ChatScreen>().SendMessage();
    }
}
