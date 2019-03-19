using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Arise.FileSyncer.Core.Components;
using Arise.FileSyncer.Core.Helpers;
using Arise.FileSyncer.Core.Messages;

namespace Arise.FileSyncer.Core.FileSync
{
    internal class FileBuilder : IDisposable
    {
        private class BuilderFileInfo
        {
            public Guid ProfileId { get; private set; }
            public Guid RemoteDeviceId { get; private set; }
            public string RelativePath { get; private set; }
            public string RootPath { get; private set; }
            public long FileSize { get; private set; }
            public long WrittenSize { get; set; }

            public BuilderFileInfo(FileStartMessage message, string rootPath)
            {
                ProfileId = message.ProfileId;
                RemoteDeviceId = message.RemoteDeviceId;
                RelativePath = PathHelper.GetCorrect(message.RelativePath, false);
                RootPath = rootPath;
                FileSize = message.FileSize;
                WrittenSize = 0;
            }
        }

        public const string TemporaryFileExtension = ".synctmp";

        private SyncerPeer owner;

        private QueueWorker<FileMessageBase> builder;
        private ConcurrentDictionary<Guid, BuilderFileInfo> fileInfos;

        private Stream writerStream = null;
        private Guid WriterFileId
        {
            get
            {
                lock (_writerFileIdLock)
                {
                    return _writerFileId;
                }
            }

            set
            {
                lock (_writerFileIdLock)
                {
                    _writerFileId = value;
                }
            }
        }

        private Guid _writerFileId = Guid.Empty;
        private readonly object _writerFileIdLock = new object();
        private bool disposed = false;

        public FileBuilder(SyncerPeer owner)
        {
            this.owner = owner;

            builder = new QueueWorker<FileMessageBase>(FileBuilderTask);
            fileInfos = new ConcurrentDictionary<Guid, BuilderFileInfo>();

            owner.ConnectionRemoved += Owner_ConnectionRemoved;

            builder.Start();
        }

        public void MessageToQueue(FileMessageBase message)
        {
            builder.Enqueue(message, true);
        }

        public bool IsBuildQueueEmpty()
        {
            return builder.IsEmpty;
        }

        private void ResetStream()
        {
            writerStream?.Dispose();
            writerStream = null;
            WriterFileId = Guid.Empty;
        }

        private void Owner_ConnectionRemoved(object sender, ConnectionRemovedEventArgs e)
        {
            List<Guid> markedForRemove = new List<Guid>();

            foreach (KeyValuePair<Guid, BuilderFileInfo> fileInfo in fileInfos)
            {
                if (fileInfo.Value.RemoteDeviceId == e.Id)
                {
                    if (WriterFileId == fileInfo.Key)
                    {
                        Log.Verbose($"{this}: Disposing writer after disconnect.");
                        ResetStream();
                    }

                    markedForRemove.Add(fileInfo.Key);
                }
            }

            foreach (Guid key in markedForRemove)
            {
                fileInfos.TryRemove(key, out var _);
            }
        }

        private void FileBuilderTask(FileMessageBase message)
        {
            switch (message.MessageType)
            {
                case NetMessageType.FileData:
                    Case_FileDataChunk((FileDataMessage)message);
                    break;
                case NetMessageType.FileStart:
                    Case_FileInfoStart((FileStartMessage)message);
                    break;
                case NetMessageType.FileEnd:
                    Case_FileInfoEnd((FileEndMessage)message);
                    break;
                default:
                    Log.Error($"{this}: Invalid NetMessageType");
                    break;
            }
        }

        private void Case_FileDataChunk(FileDataMessage message)
        {
            if (fileInfos.TryGetValue(message.FileId, out BuilderFileInfo fileInfo))
            {
                bool error = false;

                if (WriterFileId != message.FileId)
                {
                    writerStream?.Dispose();
                    WriterFileId = message.FileId;
                    if (!Utility.FileCreateWriteStream(fileInfo.ProfileId, fileInfo.RootPath, fileInfo.RelativePath + TemporaryFileExtension, out writerStream))
                    {
                        error = true;
                    }
                }

                if (!error)
                {
                    try
                    {
                        writerStream.Write(message.Chunk, 0, message.Chunk.Length);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"{this}: exception when writing. MSG: " + ex.Message);

                        ResetStream();

                        error = true;
                    }
                }

                if (owner.TryGetConnection(fileInfo.RemoteDeviceId, out ISyncerConnection iConnection))
                {
                    SyncerConnection connection = (SyncerConnection)iConnection;
                    connection.Send(new FileChunkRequestMessage());

                    if (!error)
                    {
                        fileInfo.WrittenSize += message.Chunk.Length;
                        connection.Progress.AddProgress(message.Chunk.Length);
                    }
                }
            }
        }

        private void Case_FileInfoStart(FileStartMessage message)
        {
            if (!owner.Settings.Profiles.TryGetValue(message.ProfileId, out var profile))
            {
                Log.Warning($"{this}: Profile does not exist: {message.ProfileId}");
                return;
            }

            if (profile.AllowReceive && profile.Key == message.ProfileKey)
            {
                string relativePath = PathHelper.GetCorrect(message.RelativePath, false);
                string temporaryRelativePath = relativePath + TemporaryFileExtension;

                foreach (KeyValuePair<Guid, BuilderFileInfo> builderInfo in fileInfos)
                {
                    if (builderInfo.Value.ProfileId == message.ProfileId &&
                        builderInfo.Value.RelativePath.Equals(relativePath, StringComparison.Ordinal))
                    {
                        fileInfos.TryRemove(builderInfo.Key, out var _);
                        if (builderInfo.Key == WriterFileId) writerStream?.Dispose();
                        Log.Verbose($"{this}: FileInfo removed because received newer FileInfo with the same Path");
                        break;
                    }
                }

                if (Utility.FileDelete(message.ProfileId, profile.RootDirectory, temporaryRelativePath) &&
                    Utility.FileCreate(message.ProfileId, profile.RootDirectory, temporaryRelativePath))
                {
                    Log.Info("Receiving: " + relativePath);

                    if (!fileInfos.TryAdd(message.FileId, new BuilderFileInfo(message, profile.RootDirectory)))
                    {
                        Log.Error($"{this}: Failed to add BuilderFileInfo to fileInfos!");
                    }
                }
            }
            else
            {
                Log.Warning($"{this}: Message failed verification!");
                return;
            }
        }

        private void Case_FileInfoEnd(FileEndMessage message)
        {
            //If file info does not exist then return
            if (!fileInfos.TryGetValue(message.FileId, out BuilderFileInfo fileInfo))
            {
                Log.Warning($"{this}: Invalid file guid");
                return;
            }

            //Close and flush the writer stream if it's still open
            if (WriterFileId == message.FileId)
            {
                writerStream?.Flush();
                ResetStream();
            }

            //Get some data
            string pathReal = Path.Combine(fileInfo.RootPath, fileInfo.RelativePath);
            string relativeTempPath = fileInfo.RelativePath + TemporaryFileExtension;

            // Check success
            if (!message.Success)
            {
                Log.Info($"{this}: Remote error in file send");

                if (owner.TryGetConnection(fileInfo.RemoteDeviceId, out ISyncerConnection connection))
                {
                    connection.Progress.RemoveProgress(fileInfo.WrittenSize);
                    connection.Progress.RemoveMaximum(fileInfo.FileSize);
                }

                //Delete temp file
                Utility.FileDelete(fileInfo.ProfileId, fileInfo.RootPath, fileInfo.RelativePath + TemporaryFileExtension);
            }
            else
            {
                //Delete existing file at pathReal
                Utility.FileDelete(fileInfo.ProfileId, fileInfo.RootPath, fileInfo.RelativePath);

                //Rename from the temp file to the real file
                if (Utility.FileRename(fileInfo.ProfileId, fileInfo.RootPath, relativeTempPath, Path.GetFileName(fileInfo.RelativePath)))
                {
                    //If the rename was successful -> Set file times if it supports timestamps
                    if (!SyncerPeer.SupportTimestamp ||
                        Utility.FileSetTime(fileInfo.ProfileId, fileInfo.RootPath, fileInfo.RelativePath, message.LastWriteTime, message.CreationTime))
                    {
                        //If the info update was successful
                        if (owner.Settings.Profiles.TryGetValue(fileInfo.ProfileId, out var profile))
                        {
                            profile.UpdateLastSyncDate(owner, fileInfo.ProfileId);
                        }
                        else
                        {
                            Log.Warning($"{this}: Profile does not exist: {fileInfo.ProfileId}");
                        }
                    }
                    else Log.Warning($"{this}: Failed to set Time: {fileInfo.RelativePath}");
                }
                else
                {
                    //Remove temp file if move/rename failed
                    Utility.FileDelete(fileInfo.ProfileId, fileInfo.RootPath, fileInfo.RelativePath + TemporaryFileExtension);

                    Log.Warning($"{this}: Failed to rename: {fileInfo.RelativePath}");
                }
            }

            fileInfos.TryRemove(message.FileId, out var _);
        }

        //----------------------------------------------------------------
        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    builder.Dispose();

                    if (writerStream != null)
                    {
                        writerStream.Dispose();
                        writerStream = null;
                    }

                    owner.ConnectionRemoved -= Owner_ConnectionRemoved;
                }
            }
        }

        #endregion
    }
}
