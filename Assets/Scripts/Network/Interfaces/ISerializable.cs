using System.IO;

public interface ISerializablePacket
{
    ushort UserType { get; set; }
    ushort packetType { get; set; }

    void Serialize(Stream stream);
    void Deserialize(Stream stream);
}