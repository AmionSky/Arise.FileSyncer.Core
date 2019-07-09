using System;

namespace Arise.FileSyncer.Core
{
    public class ConnectionAddedEventArgs
    {
        public Guid Id { get; }

        public ConnectionAddedEventArgs(Guid id)
        {
            Id = id;
        }
    }

    public class ConnectionVerifiedEventArgs
    {
        public Guid Id { get; }
        public string Name { get; }

        public ConnectionVerifiedEventArgs(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class ConnectionRemovedEventArgs
    {
        public Guid Id { get; }

        public ConnectionRemovedEventArgs(Guid id)
        {
            Id = id;
        }
    }

    public class ProfileEventArgs : EventArgs
    {
        public Guid Id { get; }
        public SyncProfile Profile { get; }

        public ProfileEventArgs(Guid id, SyncProfile profile)
        {
            Id = id;
            Profile = profile;
        }
    }

    public class ProfileErrorEventArgs : ProfileEventArgs
    {
        public SyncProfileError Error { get; }

        public ProfileErrorEventArgs(Guid id, SyncProfile profile, SyncProfileError error) : base(id, profile)
        {
            Error = error;
        }
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

        internal ProfileReceivedEventArgs(Guid remoteId, Messages.ProfileShareMessage msg)
        {
            RemoteId = remoteId;
            Id = msg.Id;
            Key = msg.Key;
            Name = msg.Name;
            CreationDate = msg.CreationDate;
            SkipHidden = msg.SkipHidden;
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
