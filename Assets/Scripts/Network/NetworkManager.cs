using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get; private set;
    }

    private uint nextObjectId = 0;

    private UdpConnection connection;

    public bool StartServer(int port)
    {
        try
        {
            isServer = true;
            this.port = port;
            connection = new UdpConnection(port, this);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }

        return true;
    }

    public bool StartClient(IPAddress ip, int port)
    {
        try
        {
            isServer = false;

            this.port = port;
            this.ipAddress = ip;
            
            connection = new UdpConnection(ip, port, this);
            
            ConnectionManager.Instance.AddClient(new IPEndPoint(ip, port));
            ConnectionManager.Instance.ConnectToServer();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }

        return true;
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        ConnectionManager.Instance.OnReceiveData(data, ip);
    }

    public void SendToServer(byte[] data)
    {
        Debug.Log("Sending to server");
        connection.Send(data);
    }

    public void Send(byte[] data, IPEndPoint ip)
    {
        Debug.Log("Sending to client");
        connection.Send(data, ip);
    }

    public uint GetNewObjectId()
    {
        uint newObjectId = nextObjectId;
        nextObjectId++;
        return newObjectId;
    }

    void Update()
    {
        if (connection != null)
            connection.FlushReceiveData();
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
