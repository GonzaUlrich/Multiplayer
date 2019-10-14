using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ChatStart : MonoBehaviour
{
    public GameObject goChatScreen;
    public InputField iptPort;
    public InputField iptIP;
    public InputField iptName;

    public void StartServer()
    {
        try
        {
            NetworkManager.Instance.StartServer(int.Parse(iptPort.text));
            goChatScreen.SetActive(true);
            goChatScreen.GetComponent<ChatScreen>().username = "SERVER";
            gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError("No se pudo iniciar el server. ERROR: " + ex.Message);
        }
    }

    public void StartClient()
    {
        try
        {
            IPAddress ip;
            if (iptIP.text == "localhost")
                ip = LocalIPAddress();
            else
                ip = IPAddress.Parse(iptIP.text);
            
            NetworkManager.Instance.StartClient(ip, int.Parse(iptPort.text));
            goChatScreen.SetActive(true);
            goChatScreen.GetComponent<ChatScreen>().username = iptName.text;
            gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError("No se pudo iniciar el server. ERROR: " + ex.Message);
        }
    }

    IPAddress LocalIPAddress()
    {
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        return null;
    }
}
