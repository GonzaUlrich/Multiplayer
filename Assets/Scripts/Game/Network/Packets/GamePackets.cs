using System.IO;

public enum UserPacketType
{
    Message,
}

public abstract class GameNetworkPacket<T> : NetworkPacket<T>
{
    public GameNetworkPacket(UserPacketType type) : base((ushort)type, true)
    {
    }
}

public class StringMessagePacket : GameNetworkPacket<string>
{
    public StringMessagePacket() : base(UserPacketType.Message)
    {
    }

    protected override void OnDeserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);
        payload = br.ReadString();
    }

    protected override void OnSerialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write(payload);
    }
}

public class ChatMessageMessagePacket : GameNetworkPacket<ChatMessage>
{
    public ChatMessageMessagePacket() : base (UserPacketType.Message)
    {
    }
    
    protected override void OnDeserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        payload = new ChatMessage();
        payload.time = br.ReadString();
        payload.color = br.ReadString();
        payload.name = br.ReadString();
        payload.message = br.ReadString();
    }

    protected override void OnSerialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        
        bw.Write(payload.time);
        bw.Write(payload.color);
        bw.Write(payload.name);
        bw.Write(payload.message);
    }
}