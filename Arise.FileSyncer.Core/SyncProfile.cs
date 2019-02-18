using System;
using System.IO;
using System.Threading;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Serializer;

namespace Arise.FileSyncer.Core
{
    public class SyncProfile : IBinarySerializable
    {
        /// <summary>
        /// Verification key
        /// </summary>
        public Guid Key { get; private set; }

        public string Name { get; private set; }
        public bool Activated { get; private set; }

        public bool AllowSend { get; private set; }
        public bool AllowReceive { get; private set; }
        public bool AllowDelete { get; private set; }

        private long _lastSyncDate = 0;
        public DateTime LastSyncDate
        {
            get => new DateTime(Interlocked.Read(ref _lastSyncDate));
            set => Interlocked.Exchange(ref _lastSyncDate, value.Ticks);
        }

        public DateTime CreationDate { get; private set; }

        public bool SkipHidden { get; private set; }
        public string RootDirectory { get; private set; }

        public string Plugin { get; private set; }

        public SyncProfile() { }

        private SyncProfile(Creator creator)
        {
            Key = creator.Key;

            Name = creator.Name;
            Activated = creator.Activated;

            AllowSend = creator.AllowSend;
            AllowReceive = creator.AllowReceive;
            AllowDelete = creator.AllowDelete;

            LastSyncDate = creator.LastSyncDate;
            CreationDate = creator.CreationDate;

            SkipHidden = creator.SkipHidden;
            RootDirectory = creator.RootDirectory;

            Plugin = creator.Plugin;
        }

        public static implicit operator SyncProfile(Creator creator)
        {
            return new SyncProfile(creator);
        }

        internal bool GenerateState(out FileSystemItem[] state)
        {
            return DirectoryTreeQuery.GenerateTree(out state, RootDirectory, SkipHidden, FileBuilder.TemporaryFileExtension);
        }

        public void Deserialize(Stream stream)
        {
            Key = stream.ReadGuid();

            Name = stream.ReadString();
            Activated = stream.ReadBoolean();

            AllowSend = stream.ReadBoolean();
            AllowReceive = stream.ReadBoolean();
            AllowDelete = stream.ReadBoolean();

            LastSyncDate = stream.ReadDateTime();
            CreationDate = stream.ReadDateTime();

            SkipHidden = stream.ReadBoolean();
            RootDirectory = stream.ReadString();

            Plugin = stream.ReadString();
        }

        public void Serialize(Stream stream)
        {
            stream.Write(Key);

            stream.Write(Name);
            stream.Write(Activated);

            stream.Write(AllowSend);
            stream.Write(AllowReceive);
            stream.Write(AllowDelete);

            stream.Write(LastSyncDate);
            stream.Write(CreationDate);

            stream.Write(SkipHidden);
            stream.Write(RootDirectory);

            stream.Write(Plugin);
        }

        public class Creator
        {
            /// <summary>
            /// Verification key
            /// </summary>
            public Guid Key { get; set; }

            public string Name { get; set; }
            public bool Activated { get; set; }

            public bool AllowSend { get; set; }
            public bool AllowReceive { get; set; }
            public bool AllowDelete { get; set; }

            public DateTime LastSyncDate { get; set; }
            public DateTime CreationDate { get; set; }

            public bool SkipHidden { get; set; }
            public string RootDirectory { get; set; }

            public string Plugin { get; set; }

            public Creator()
            {
                Key = Guid.Empty;

                Name = string.Empty;
                Activated = true;

                AllowSend = false;
                AllowReceive = false;
                AllowDelete = false;

                LastSyncDate = DateTime.Now;
                CreationDate = DateTime.Now;

                SkipHidden = true;
                RootDirectory = string.Empty;

                Plugin = string.Empty;
            }

            public Creator(SyncProfile profile)
            {
                Key = profile.Key;

                Name = profile.Name;
                Activated = profile.Activated;

                AllowSend = profile.AllowSend;
                AllowReceive = profile.AllowReceive;
                AllowDelete = profile.AllowDelete;

                LastSyncDate = profile.LastSyncDate;
                CreationDate = profile.CreationDate;

                SkipHidden = profile.SkipHidden;
                RootDirectory = profile.RootDirectory;

                Plugin = profile.Plugin;
            }
        }
    }
}
