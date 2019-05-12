using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Arise.FileSyncer.Core.Components;
using Arise.FileSyncer.Core.Messages;

namespace Arise.FileSyncer.Core.FileSync
{
    internal class FileSender : IDisposable
    {
        private readonly SyncerConnection owner;

        private readonly QueueWorker<FileSendInfo> sender;
        private AutoResetEvent chunkRequestEvent;
        private volatile int chunkCount;

        public FileSender(SyncerConnection owner)
        {
            this.owner = owner;

            sender = new QueueWorker<FileSendInfo>(SendFileByChunks);
            chunkRequestEvent = new AutoResetEvent(false);
            chunkCount = owner.Owner.Settings.ChunkRequestCount;

            sender.Start();
        }

        public void AddFiles(IList<FileSendInfo> sendInfos)
        {
            long overallSize = 0;

            foreach (FileSendInfo sendInfo in sendInfos)
            {
                overallSize += sendInfo.File.Length;
                owner.Progress.AddMaximum(sendInfo.File.Length);
                sender.Enqueue(sendInfo, false);
            }

            owner.Send(new FileSizeMessage(overallSize));
            sender.Signal();
        }

        public void AddFile(FileSendInfo sendInfo)
        {
            owner.Send(new FileSizeMessage(sendInfo.File.Length));
            owner.Progress.AddMaximum(sendInfo.File.Length);
            sender.Enqueue(sendInfo, true);
        }

        public void ChunkRequest()
        {
            Interlocked.Increment(ref chunkCount);
            chunkRequestEvent?.Set();
        }

        public bool IsSendQueueEmpty()
        {
            return sender.IsEmpty;
        }

        private void SendFileByChunks(FileSendInfo sendInfo)
        {
            Log.Info($"Sending: {sendInfo.File.FullName}");

            //Generate a unique file ID
            Guid fileId = Guid.NewGuid();

            //Send start info
            owner.Send(new FileStartMessage()
            {
                FileId = fileId,
                ProfileId = sendInfo.ProfileId,
                ProfileKey = sendInfo.ProfileKey,
                RelativePath = sendInfo.RelativePath,
                FileSize = sendInfo.File.Length,
            });

            //Init vars
            long allBytesRead = 0;
            int bytesRead = 0;
            byte[] buffer = new byte[owner.Owner.Settings.BufferSize];
            bool hadError = false;


            string pathRelative = sendInfo.RelativePath;
            string pathRoot = sendInfo.RootPath;

            if (!string.Equals(Path.Combine(pathRoot, pathRelative), sendInfo.File.FullName, StringComparison.Ordinal))
            {
                pathRoot = string.Empty;
                pathRelative = sendInfo.File.FullName;
            }

            //Create file stream
            if (!Utility.FileCreateReadStream(sendInfo.ProfileId, pathRoot, pathRelative, out Stream fileStream))
            {
                hadError = true;
            }

            if (!hadError)
            {
                //Read from file stream
                while ((bytesRead = FileReadSafe(fileStream, ref buffer, ref hadError)) > 0)
                {
                    //Checks
                    if (chunkCount <= 0) chunkRequestEvent?.WaitOne();
                    if (sender.Exiting) hadError = true;
                    if (hadError) break;

                    //Send message and update progress
                    owner.Send(new FileDataMessage()
                    {
                        FileId = fileId,
                        Chunk = SubArray(buffer, 0, bytesRead),
                    });

                    Interlocked.Decrement(ref chunkCount);
                    allBytesRead += bytesRead;
                    owner.Progress.AddProgress(bytesRead);
                }
            }

            //Check for errors
            if (hadError)
            {
                Log.Warning($"{this}: Removed progress: File send error!");
                owner.Progress.RemoveProgress(allBytesRead);
                owner.Progress.RemoveMaximum(sendInfo.File.Length);
            }
            else
            {
                if (owner.Owner.Settings.Profiles.TryGetValue(sendInfo.ProfileId, out var profile))
                {
                    profile.UpdateLastSyncDate(owner.Owner, sendInfo.ProfileId);
                }
                else
                {
                    Log.Warning($"{this}: Profile does not exist: {sendInfo.ProfileId}");
                }
            }

            //Dispose the file stream
            fileStream?.Dispose();

            //Send end info
            owner.Send(new FileEndMessage()
            {
                FileId = fileId,
                Success = !hadError,
                LastWriteTime = sendInfo.File.LastWriteTime,
                CreationTime = sendInfo.File.CreationTime,
            });

            //Call finished callback
            sendInfo.Finished();
        }

        private static int FileReadSafe(Stream fileStream, ref byte[] buffer, ref bool hadError)
        {
            try { return fileStream.Read(buffer, 0, buffer.Length); }
            catch { hadError = true; return 0; }
        }

        private static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Buffer.BlockCopy(data, index, result, 0, length);
            return result;
        }

        //----------------------------------------------------------------
        #region Dispose
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    if (chunkRequestEvent != null)
                    {
                        chunkRequestEvent.Set();
                        chunkRequestEvent.Dispose();
                        chunkRequestEvent = null;
                    }

                    sender.Dispose();
                }
            }
        }
        #endregion
    }
}
