using System;
using System.Collections.Generic;
using System.IO;
using Arise.FileSyncer.Core.Messages;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core
{
    internal enum NetMessageType : byte
    {
        None = 0,

        VerificationData = 1,
        VerificationResponse = 2,
        SyncInitialization = 3,

        FileStart = 4,
        FileEnd = 5,
        FileData = 6,
        FileChunkRequest = 7,
        FileSize = 8,

        CreateDirectories = 9,

        ProfileShare = 10,

        PairingRequest = 11,
        PairingResponse = 12,

        SyncInitFinished = 13,
        SyncProfile = 14,

        IsAlive = 15,

        DeleteFiles = 16,
        DeleteDirectories = 17,
    }

    internal abstract class NetMessage : IBinarySerializable
    {
        public abstract NetMessageType MessageType { get; }

        public abstract void Process(SyncerConnection con);

        public abstract void Deserialize(Stream stream);
        public abstract void Serialize(Stream stream);
    }

    internal static class NetMessageFactory
    {
        public static NetMessage Create(NetMessageType messageType)
        {
            return messageType switch
            {
                NetMessageType.None => throw new Exception("NetMessage: Can't create 'None' message"),
                NetMessageType.VerificationData => new VerificationDataMessage(),
                NetMessageType.VerificationResponse => new VerificationResponseMessage(),
                NetMessageType.SyncInitialization => new SyncInitializationMessage(),
                NetMessageType.FileStart => new FileStartMessage(),
                NetMessageType.FileEnd => new FileEndMessage(),
                NetMessageType.FileData => new FileDataMessage(),
                NetMessageType.FileChunkRequest => new FileChunkRequestMessage(),
                NetMessageType.FileSize => new FileSizeMessage(),
                NetMessageType.CreateDirectories => new CreateDirectoriesMessage(),
                NetMessageType.ProfileShare => new ProfileShareMessage(),
                NetMessageType.PairingRequest => new PairingRequestMessage(),
                NetMessageType.PairingResponse => new PairingResponseMessage(),
                NetMessageType.SyncInitFinished => new SyncInitFinishedMessage(),
                NetMessageType.SyncProfile => new SyncProfileMessage(),
                NetMessageType.IsAlive => new IsAliveMessage(),
                NetMessageType.DeleteFiles => new DeleteFilesMessage(),
                NetMessageType.DeleteDirectories => new DeleteDirectoriesMessage(),
                _ => throw new Exception("NetMessage: No class found for messageType"),
            };
        }
    }
}
