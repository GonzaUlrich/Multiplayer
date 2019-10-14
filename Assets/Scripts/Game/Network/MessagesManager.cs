using UnityEngine;
using System;

public class MessagesManager : Singleton<MessagesManager>
{
    protected override void Initialize()
    {
        base.Initialize();
    }

    public void SendString(string message, uint objectId)
    {
        Debug.Log($"Sending: " + message);

        StringMessagePacket packet = new StringMessagePacket();

        packet.payload = message;

        PacketsManager.Instance.SendPacket(packet, objectId);
    }

    public void SendChatMessage(string message, string name, string time, string color, uint objectId)
    {
        Debug.Log($"Sending: " + message);

        ChatMessageMessagePacket packet = new ChatMessageMessagePacket();

        packet.payload = new ChatMessage();
        packet.payload.time = time;
        packet.payload.color = color;
        packet.payload.name = name;
        packet.payload.message = message;

        PacketsManager.Instance.SendPacket(packet, objectId);
    }
}
