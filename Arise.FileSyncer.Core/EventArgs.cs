using System;
using Arise.FileSyncer.Core.Messages;

namespace Arise.FileSyncer.Core
{
    public class ConnectionEventArgs : EventArgs
    {
        public Guid Id { get; init; }
    }

    public class ConnectionVerifiedEventArgs : ConnectionEventArgs
    {
        public string Name { get; init; }
    }

    public class ProfileEventArgs : EventArgs
    {
        public Guid Id { get; init; }
        public SyncProfile Profile { get; init; }
    }

    public class ProfileErrorEventArgs : ProfileEventArgs
    {
        public SyncProfileError Error { get; init; }
    }

    public class ProfileReceivedEventArgs : EventArgs
    {
        public Guid RemoteId { get; }

        // Profile Data
        public Guid Id { get; }
        public Guid Key { get; }
        public string Name { get; }
        public DateTime CreationDate { get; }
        public bool SkipHidden { get; }

        internal ProfileReceivedEventArgs(Guid remoteId, ProfileShareMessage message)
        {
            RemoteId = remoteId;
            Id = message.Id;
            Key = message.Key;
            Name = message.Name;
            CreationDate = message.CreationDate;
            SkipHidden = message.SkipHidden;
        }
    }

    public class PairingRequestEventArgs : EventArgs
    {
        public string DisplayName { get; }
        public Guid RemoteDeviceId { get; }
        public Action<bool> ResultCallback { get; }

        public PairingRequestEventArgs(string displayName, Guid remoteDeviceId, Action<bool> resultCallback)
        {
            DisplayName = displayName;
            RemoteDeviceId = remoteDeviceId;
            ResultCallback = resultCallback;
        }
    }

    public class NewPairAddedEventArgs : EventArgs
    {
        public Guid RemoteDeviceId { get; }

        public NewPairAddedEventArgs(Guid remoteDeviceId)
        {
            RemoteDeviceId = remoteDeviceId;
        }
    }

    public class FileBuiltEventArgs : EventArgs
    {
        public Guid ProfileId { get; }
        public string RootPath { get; }
        public string RelativePath { get; }

        public FileBuiltEventArgs(Guid profileId, string rootPath, string relativePath)
        {
            ProfileId = profileId;
            RootPath = rootPath;
            RelativePath = relativePath;
        }
    }
}
