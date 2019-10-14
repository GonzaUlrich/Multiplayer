using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;

public class PacketsManager : Singleton<PacketsManager>, IReceiveData
{
    private Dictionary<uint, System.Action<ushort, Stream, IPEndPoint>> onPacketReceived = new Dictionary<uint, System.Action<ushort, Stream, IPEndPoint>>();
    private uint currentPacketId = 0;

    protected override void Initialize()
    {
        base.Initialize();
        ConnectionManager.Instance.onReceiveEvent += OnReceiveData;
    }

    public void AddListener(uint ownerId, System.Action<ushort, Stream, IPEndPoint> callback)
    {
        if (!onPacketReceived.ContainsKey(ownerId))
            onPacketReceived.Add(ownerId, callback);
    }

    public void RemoveListener(uint ownerId)
    {
        if (onPacketReceived.ContainsKey(ownerId))
            onPacketReceived.Remove(ownerId);
    }

    public void SendPacket(ISerializablePacket packet, uint objectId, bool reliable = false)
    {
        byte[] bytes = Serialize(packet, objectId);

        Debug.Log($"Enviando " + bytes.Length + " Bytes.");

        if (NetworkManager.Instance.isServer)
            Broadcast(bytes);
        else
            NetworkManager.Instance.SendToServer(bytes);
    }

    public void SendPacket(ISerializablePacket packet, IPEndPoint iPEndPoint, uint objectId, bool reliable = false)
    {
        if (NetworkManager.Instance.isServer)
        {
            byte[] bytes = Serialize(packet, objectId);

            Debug.Log($"Enviando " + bytes.Length + " Bytes.");

            NetworkManager.Instance.Send(bytes, iPEndPoint);
        }
    }

    public void Broadcast(byte[] data)
    {
        List<IPEndPoint> iPEndPoints = ConnectionManager.Instance.GetIPEndPoints();

        foreach (IPEndPoint ip in iPEndPoints)
        {
            NetworkManager.Instance.Send(data, ip);
        }
    }

    private byte[] Serialize(ISerializablePacket packet, uint objectId)
    {
        PacketHeader header = new PacketHeader();
        MemoryStream stream = new MemoryStream();

        header.id = currentPacketId;
        header.senderId = ConnectionManager.Instance.clientId;
        header.objectId = objectId;
        header.packetType = (ushort)packet.packetType;   
        header.Serialize(stream);

        if (packet.packetType == (ushort)PacketType.User)
        {
            UserTypeHeader userHeader = new UserTypeHeader();
            userHeader.packetType = (ushort)packet.UserType;
            userHeader.Serialize(stream);
        } 

        packet.Serialize(stream);

        stream.Close();

        return stream.ToArray();
    }

    public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
    {
        Debug.Log($"Recibiendo: " + data);

        PacketHeader header = new PacketHeader();
        MemoryStream stream = new MemoryStream(data);

        header.Deserialize(stream);
        
        if (header.packetType == (ushort)PacketType.User)
        {
            UserTypeHeader userHeader = new UserTypeHeader();
            userHeader.Deserialize(stream);

            InvokeCallback(header.objectId, userHeader.packetType, stream, null);
        }
        else
            InvokeCallback(header.objectId, header.packetType, stream, ipEndpoint);

        stream.Close();
    }

    void InvokeCallback(uint objectId, ushort type, Stream stream, IPEndPoint ipEndpoint)
    {
        if (onPacketReceived.ContainsKey(objectId))
            onPacketReceived[objectId].Invoke(type, stream, ipEndpoint);
    }
}
