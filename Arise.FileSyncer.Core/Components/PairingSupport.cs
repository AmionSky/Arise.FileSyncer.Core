using System;

namespace Arise.FileSyncer.Core.Components
{
    internal class PairingSupport
    {
        public bool Accept { get; set; } = false;
        public DateTime GenTime { get; set; } = new DateTime(0, DateTimeKind.Utc);
    }
}
