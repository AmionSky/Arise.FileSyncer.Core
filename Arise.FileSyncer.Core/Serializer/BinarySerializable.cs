using System.IO;

namespace Arise.FileSyncer.Serializer
{
    public interface IBinarySerializable
    {
        void Deserialize(Stream stream);
        void Serialize(Stream stream);
    }
}
