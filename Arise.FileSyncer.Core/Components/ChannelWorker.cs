using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Arise.FileSyncer.Core.Components
{
    public class ChannelWorker<T>
    {
        private readonly Channel<T> channel;
        private readonly Action<T> job;
        private readonly Task task;

        public ChannelWorker(bool singleWriter, Action<T> job)
        {
            this.job = job;

            channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
            {
                SingleWriter = singleWriter,
                SingleReader = true
            });
            
            task = Work();
        }

        public void Complete()
        {
            channel.Writer.TryComplete();
        }

        public bool Write(T item)
        {
            bool result = channel.Writer.TryWrite(item);
            if (!result)
            {
                Log.Warning($"ChannelWorker: Failed to write to Channel");
            }
            return result;
        }

        public void Wait(TimeSpan timeout)
        {
            task.Wait(timeout);
        }

        private async Task Work()
        {
            var reader = channel.Reader;

            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out T item))
                {
                    job(item);
                }
            }
        }
    }
}
