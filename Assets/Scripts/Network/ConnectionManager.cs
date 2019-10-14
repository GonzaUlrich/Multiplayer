using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.IO;

public struct Client
{
    public float lastMsgTimeStamp;
    public uint id;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, uint id, float timeStamp)
    {
        this.lastMsgTimeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
    }
}

public struct NewClient
{
    public float lastMsgTimeStamp;
    public uint id;
    public IPEndPoint ipEndPoint;
    public BitArray clientDirt;
    public BitArray serverDirt;

    public NewClient(IPEndPoint ipEndPoint, uint id, float timeStamp, BitArray clientDirt, BitArray serverDirt)
    {
        this.lastMsgTimeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.clientDirt = clientDirt;
        this.serverDirt = serverDirt;
    }
}

public class ConnectionManager : MonoBehaviourSingleton<ConnectionManager>, IReceiveData
{
    public int timeOut = 30;

    public uint clientId
    {
        get; private set;
    }

    public uint newClientId
    {
        get; private set;
    }

    private readonly Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
    private readonly Dictionary<IPEndPoint, uint> ipToId = new Dictionary<IPEndPoint, uint>();

    
    private readonly Dictionary<uint, NewClient> newClients = new Dictionary<uint, NewClient>();
    private readonly Dictionary<IPEndPoint, uint> newIpToId = new Dictionary<IPEndPoint, uint>();

    public Action<byte[], IPEndPoint> onReceiveEvent;

    private bool isServer;
    private uint objectId;

    private bool onConectionRequest = false;
    private bool onChallengeResponse = false;
    private bool connected = false;
    
    private BitArray clientDirt;
    private BitArray serverDirt;
    private BitArray challengeResolved;

    private void Update() 
    {
        
        if (newClients.Count > 0)
        {
            using (var iterator = newClients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    SendChallengeRequest(iterator.Current.Value.ipEndPoint, iterator.Current.Value.serverDirt);
                }
            }
        }
        else
        {
            if (onConectionRequest)
                SendConnectionRequest();
            
            if (onChallengeResponse)
                SendChallengeResponse();
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

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        if (onReceiveEvent != null)
            onReceiveEvent.Invoke(data, ip);
    }

    public void AddClient(IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Adding client: " + ip.Address);

            uint id = clientId;
            ipToId[ip] = clientId;

            clients.Add(clientId, new Client(ip, id, Time.realtimeSinceStartup));

            clientId++;
        }
    }

    public void AddNewClient(IPEndPoint ip, BitArray clientDirt)
    {
        if (!newIpToId.ContainsKey(ip) && !ipToId.ContainsKey(ip))
        {
            Debug.Log("Adding new client: " + ip.Address);

            uint id = newClientId;
            newIpToId[ip] = newClientId;

            BitArray newServerDirt = new BitArray(32);
            for (int i  = 0; i < newServerDirt.Length; i++)
                newServerDirt.Set(i, (UnityEngine.Random.Range(0, 1) == 0));

            newClients.Add(newClientId, new NewClient(ip, id, Time.realtimeSinceStartup, clientDirt, newServerDirt));

            newClientId++;
        }
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    void RemoveNewClient(IPEndPoint ip)
    {
        if (newIpToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            newClients.Remove(newIpToId[ip]);
            newIpToId.Remove(ip);
        }
    }

    void OnRecieve(ushort type, Stream stream, IPEndPoint ip)
    {
        switch(type)
        {
            case (ushort)PacketType.ConnectionRequest:
            {
                ConnectionRequestPacket packet = new ConnectionRequestPacket();
                packet.Deserialize(stream);
                AddNewClient(ip, packet.payload);
                break;
            }
            
            case (ushort)PacketType.ChallengeRequest:
            {
                onConectionRequest = false;
                ChallengeRequestPacket packet = new ChallengeRequestPacket();
                packet.Deserialize(stream);
                serverDirt = packet.payload;

                DoChallenge();
                break;
            }
            
            case (ushort)PacketType.ChallengeResponse:
            {
                ChallengeResponsePacket packet = new ChallengeResponsePacket();
                packet.Deserialize(stream);
                CheckChallengeRecieved(ip, packet.payload);
                break;
            }
                
            
            case (ushort)PacketType.Conected:
                OnConected();
                break;
        }
    }

    void SendConnectionRequest() 
    {   
        Debug.Log("Enviando Connection Request");
        ConnectionRequestPacket packet = new ConnectionRequestPacket();
        packet.payload = clientDirt;
        PacketsManager.Instance.SendPacket(packet, objectId);
    }

    void SendChallengeRequest(IPEndPoint ip, BitArray serverDirt) 
    {   
        Debug.Log("Enviando ChallengeRequest");
        ChallengeRequestPacket packet = new ChallengeRequestPacket();
        packet.payload = serverDirt;
        PacketsManager.Instance.SendPacket(packet, ip, objectId);
    }

    void SendChallengeResponse() 
    {
        Debug.Log("Enviando Challenge Response");
        ChallengeResponsePacket packet = new ChallengeResponsePacket();
        packet.payload = challengeResolved;
        PacketsManager.Instance.SendPacket(packet, objectId);
    }

    void SendConected(IPEndPoint ip) 
    {
        Debug.Log("Enviando Conected");
        AddClient(ip);

        ConectedPacket packet = new ConectedPacket();
        packet.payload = "test";
        PacketsManager.Instance.SendPacket(packet, ip, objectId);
    }

    void OnConected()
    {
        Debug.Log("Conected to Server");
        onChallengeResponse = false;
        connected = true;
    }

    void DoChallenge()
    {
        challengeResolved = serverDirt;
        challengeResolved.Xor(clientDirt);
        onChallengeResponse = true;
    }
    
    void CheckChallengeRecieved(IPEndPoint ip, BitArray answerRecieved)
    {
        if (newIpToId.ContainsKey(ip))
        {
            BitArray answer = newClients[newIpToId[ip]].serverDirt;
            answer.Xor(newClients[newIpToId[ip]].clientDirt);

            for (int i  = 0; i < answer.Length; i++)
            {
                if (answer[i] != answerRecieved[i])
                    return;
            }

            onChallengeResponse = false;
            RemoveNewClient(ip);
            SendConected(ip);
        }
    }

    public void ConnectToServer()
    {
        if (!connected && !onChallengeResponse)
        {
            onConectionRequest = true;

            clientDirt = new BitArray(32);
            for (int i  = 0; i < 32; i++)
                clientDirt.Set(i, (UnityEngine.Random.Range(0, 2) == 0));
            
        }
    }

    public List<IPEndPoint> GetIPEndPoints()
    {
        List<IPEndPoint> iPEndPoints = new List<IPEndPoint>();
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iPEndPoints.Add(iterator.Current.Value.ipEndPoint);
            }
        }

        return iPEndPoints;
    }
}
