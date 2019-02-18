using System;
using System.IO;
using Arise.FileSyncer.Core.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal abstract class FileMessageBase : NetMessage
    {
        public Guid FileId { get; set; }

        public override void Deserialize(Stream stream)
        {
            FileId = stream.ReadGuid();
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(FileId);
        }
    }
}
