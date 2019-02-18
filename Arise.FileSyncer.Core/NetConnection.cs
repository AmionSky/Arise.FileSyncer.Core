using System;
using System.IO;

namespace Arise.FileSyncer.Core
{
    public interface INetConnection : IDisposable
    {
        /// <summary>
        /// Same as remote's DeviceId.
        /// </summary>
        Guid Id { get; }

        Stream SenderStream { get; }
        Stream ReceiverStream { get; }
    }
}
