using System;

namespace Arise.FileSyncer.Components
{
    internal class PairingSupport
    {
        public bool Accept { get; set; } = false;
        public DateTime GenTime { get; set; } = new DateTime(1, 1, 1); // 0001-01-01
    }
}
