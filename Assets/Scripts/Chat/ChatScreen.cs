using UnityEngine.UI;
using System.IO;
using System.Net;
using UnityEngine;
using System;
using TMPro;

public class ChatScreen : MonoBehaviour
{
    public TextMeshProUGUI txtChat;
    public InputField iptMessage;
    public string username;
    public string colorSelected = "black";

    private char[] separator = {'/', '$', '/'};
    private uint objectId;

    private void Update() 
    {
        if (Input.GetKeyUp(KeyCode.Return)|| Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
       
    }

    void OnEnable()
    {
        objectId = NetworkManager.Instance.GetNewObjectId();
        PacketsManager.Instance.AddListener(objectId, OnRecieve);
    }

    void OnDisable()
    {
        PacketsManager.Instance.RemoveListener(objectId);
    }
    
    void OnRecieve(ushort type, Stream stream, IPEndPoint ip)
    {
        try 
        {
            if (type == (ushort)UserPacketType.Message)
            {
                ChatMessageMessagePacket packet = new ChatMessageMessagePacket();
                packet.Deserialize(stream);

                Debug.Log("Recibido: " + packet.payload);

                if (NetworkManager.Instance.isServer)
                {
                    MessagesManager.Instance.SendChatMessage(packet.payload.message, packet.payload.name, packet.payload.time, packet.payload.color,  objectId);
                }

                if (packet.payload.name != username)
                    WriteInChat(packet.payload.message, packet.payload.name, packet.payload.color, packet.payload.time);
                
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void SendMessage()
    {
        if (iptMessage.text != string.Empty)
        {
            MessagesManager.Instance.SendChatMessage(iptMessage.text, username, DateTime.Now.ToString(), colorSelected, objectId);
            WriteInChat(iptMessage.text, username, colorSelected, DateTime.Now.ToString());
            iptMessage.text = string.Empty;
        }
    }

    void WriteInChat(string message, string name, string color, string time)
    {
        txtChat.text = txtChat.text + System.Environment.NewLine + "<color=\"" + color +  "\">[" + name + " - " + time +  "] " + message;
    }

    public void ColorChanged(string color)
    {
        this.colorSelected = color;
    }
}
