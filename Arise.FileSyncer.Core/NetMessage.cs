using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Arise.FileSyncer.Core.Serializer;

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
        private static readonly Dictionary<NetMessageType, Type> messageTypes;

        static NetMessageFactory()
        {
            messageTypes = new Dictionary<NetMessageType, Type>();
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            foreach (Type classType in currentAssembly.GetTypes())
            {
                if (typeof(NetMessage).IsAssignableFrom(classType) && !classType.IsAbstract)
                {
                    NetMessage netMessage = CreateClass(classType);
                    messageTypes.Add(netMessage.MessageType, classType);
                }
            }
        }

        public static NetMessage Create(NetMessageType messageType)
        {
            return CreateClass(GetClassType(messageType));
        }

        private static Type GetClassType(NetMessageType messageType)
        {
            if (messageTypes.TryGetValue(messageType, out Type classType)) return classType;
            else throw new Exception("NetMessage: No class found for messageType");
        }

        private static NetMessage CreateClass(Type messageClassType)
        {
            return (NetMessage)Activator.CreateInstance(messageClassType);
        }
    }
}
