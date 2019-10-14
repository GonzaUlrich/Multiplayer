using System.IO;
using System.Collections;

public enum PacketType
{
    Ping,
    ConnectionRequest,
    ChallengeRequest,
    ChallengeResponse,
    Conected,
    HandShake,
    HandShake_OK,
    ACK,
    Error,
    User,
}

public abstract class NetworkPacket<P> : ISerializablePacket
{
    public ushort packetType { get; set; }
    public ushort UserType { get; set; }
    public P payload;

    public NetworkPacket(ushort type, bool isUserType)
    {
        if (isUserType)
        {
            this.packetType = (ushort)PacketType.User;
            this.UserType = type;
        }
        else
            this.packetType = type;
    }

    public virtual void Serialize(Stream stream)
    {
        OnSerialize(stream);
    }

    public virtual void Deserialize(Stream stream)
    {
        OnDeserialize(stream);
    }


    protected abstract void OnSerialize(Stream stream);
    protected abstract void OnDeserialize(Stream stream);
}

public abstract class ServerNetworkPacket<T> : NetworkPacket<T>
{
    public ServerNetworkPacket(PacketType type) : base((ushort)type, false)
    {
    }
}

public class ConnectionRequestPacket : ServerNetworkPacket<BitArray>
{
    public ConnectionRequestPacket() : base(PacketType.ConnectionRequest)
    {
    }

    protected override void OnDeserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        payload = new BitArray(32);
        BitArray recievedArray = new BitArray(32);
        for (int i = 0; i < 32; i ++)
        {
            payload.Set(i, br.ReadBoolean());
        }
    }

    protected override void OnSerialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);

        for (int i = 0; i < 32; i ++)
        {
            bw.Write(payload[i]);
        }
    }
}

public class ChallengeRequestPacket : ServerNetworkPacket<BitArray>
{
    public ChallengeRequestPacket() : base(PacketType.ChallengeRequest)
    {
    }

    protected override void OnDeserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        payload = new BitArray(32);
        BitArray recievedArray = new BitArray(32);
        for (int i = 0; i < 32; i ++)
        {
            payload.Set(i, br.ReadBoolean());
        }
    }

    protected override void OnSerialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);

        for (int i = 0; i < 32; i ++)
        {
            bw.Write(payload[i]);
        }
    }
}

public class ChallengeResponsePacket : ServerNetworkPacket<BitArray>
{
    public ChallengeResponsePacket() : base(PacketType.ChallengeResponse)
    {
    }

    protected override void OnDeserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        payload = new BitArray(32);
        BitArray recievedArray = new BitArray(32);
        for (int i = 0; i < 32; i ++)
        {
            payload.Set(i, br.ReadBoolean());
        }
    }

    protected override void OnSerialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);

        for (int i = 0; i < 32; i ++)
        {
            bw.Write(payload[i]);
        }
    }
}

public class ConectedPacket : ServerNetworkPacket<string>
{
    public ConectedPacket() : base(PacketType.Conected)
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
