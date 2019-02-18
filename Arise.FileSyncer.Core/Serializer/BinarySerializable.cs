using System.IO;

namespace Arise.FileSyncer.Core.Serializer
{
    public interface IBinarySerializable
    {
        void Deserialize(Stream stream);
        void Serialize(Stream stream);
    }
}
