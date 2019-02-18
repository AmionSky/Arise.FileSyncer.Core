using System;
using System.IO;
using System.Threading.Tasks;
using Arise.FileSyncer.Components;

namespace Arise.FileSyncer
{
    internal class NetMessageHandler : IDisposable
    {
        public readonly INetConnection Connection;

        private readonly Action disconnect;

        private readonly QueueWorker<NetMessage> sender;
        private readonly QueueWorker<NetMessage> processor;

        private Task receiverTask = null;
        private bool exitReceiver = true;
        private bool disposed = false;

        public NetMessageHandler(INetConnection connection, Action<NetMessage> messageReceived, Action disconnect)
        {
            Connection = connection;
            this.disconnect = disconnect;

            sender = new QueueWorker<NetMessage>(SenderTask);
            processor = new QueueWorker<NetMessage>(messageReceived);
        }

        public void Start()
        {
            if (disposed)
            {
                Log.Error($"{this}: Tried to start a disposed message handler");
                return;
            }

            processor.Start();
            sender.Start();

            if (receiverTask == null)
            {
                exitReceiver = false;
                receiverTask = Task.Factory.StartNew(ReceiverWorker, TaskCreationOptions.LongRunning);
            }
        }

        public void Stop()
        {
            exitReceiver = true;
            processor.Stop();
            sender.Stop();
        }

        public void Send(NetMessage message)
        {
            sender.Enqueue(message, true);
        }

        public void SendAndDisconnect(NetMessage message)
        {
            try
            {
                // Stop sender worker
                sender.Stop();
                sender.Wait(TimeSpan.FromSeconds(3));

                // Send message synchronously
                SenderTask(message);
            }
            catch (Exception ex)
            {
                Log.Warning($"{this}: SendAndDisconnect failed: {ex}");
            }

            // Disconnect
            disconnect();
        }

        private void ReceiverWorker()
        {
            if (Connection.ReceiverStream is Stream stream)
            {
                while (!exitReceiver)
                {
                    try
                    {
                        int readByte = stream.ReadByte();
                        if (readByte == -1)
                        {
                            Log.Verbose($"{this}: Receiver EOS");
                            disconnect();
                            break;
                        };

                        NetMessageType messageType = (NetMessageType)readByte;
                        NetMessage message = NetMessageFactory.Create(messageType);

                        message.Deserialize(stream);
                        processor.Enqueue(message, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{this}: Receive Exception: " + ex.Message);
                        disconnect();
                        break;
                    }
                }
            }

            Log.Verbose($"{this}: Message Receiver Stopped");
            receiverTask = null;
        }

        private void SenderTask(NetMessage message)
        {
            try
            {
                Connection.SenderStream.WriteByte((byte)message.MessageType);
                message.Serialize(Connection.SenderStream);
                Connection.SenderStream.Flush();
            }
            catch (Exception ex)
            {
                Log.Error($"{this}: Send Exception: " + ex.Message);
                disconnect();
            }
        }

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
                    exitReceiver = true;

                    processor.Dispose();
                    sender.Dispose();

                    Connection.Dispose();
                }
            }
        }

        #endregion
    }
}
