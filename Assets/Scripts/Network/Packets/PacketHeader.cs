using System.IO;

public class PacketHeader : ISerializablePacket
{
    public uint senderId;
    public uint id;
    public uint objectId;
    public ushort packetType { get; set; }
    public ushort UserType { get; set; }

    public void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);

        bw.Write(id);
        bw.Write(senderId);
        bw.Write(objectId);
        bw.Write(packetType);
        bw.Write(UserType);

        OnSerialize(stream);
    }

    public void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        id = br.ReadUInt32();
        senderId = br.ReadUInt32();
        objectId = br.ReadUInt32();
        packetType = br.ReadUInt16();
        UserType = br.ReadUInt16();

        OnDeserialize(stream);
    }

    protected virtual void OnSerialize(Stream stream)
    {
    }

    protected virtual void OnDeserialize(Stream stream)
    {
    }
}

public class UserTypeHeader : ISerializablePacket
{
    public ushort packetType { get; set; }
    public ushort UserType { get; set; }

    public void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);

        bw.Write(packetType);

        OnSerialize(stream);
    }

    public void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        packetType = br.ReadUInt16();

        OnDeserialize(stream);
    }

    protected virtual void OnSerialize(Stream stream)
    {
    }

    protected virtual void OnDeserialize(Stream stream)
    {
    }
}
