using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    static GameObject panel;
    static Text textPanel;
    static string messageList;


    void Start()
    {
        panel = transform.Find("Panel").gameObject;
        textPanel = transform.Find("Panel/Text").GetComponent<Text>();
    }

    
    public static void ShowMessage(string message)
    {
        if (panel == null)
        {
            Debug.Log("DebugPanel is not exist");
            return;
        }

        panel.SetActive(true);
        messageList += " -> " + message;
        textPanel.text = messageList;
    }


    public static void HideMessage()
    {
        if (panel == null)
        {
            Debug.Log("DebugPanel is not exist");
            return;
        }

        panel.SetActive(false);
        messageList = "";
    }
    


   
}
