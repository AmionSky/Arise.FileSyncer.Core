using System;
using System.Threading.Tasks;

namespace Arise.FileSyncer.Plugins
{
    [Flags]
    public enum PluginFeatures : short
    {
        None = 0,
        ModifySendData = 0b0000_0000_0000_0001,
    };

    public abstract partial class Plugin
    {
        public abstract string Name { get; }
        public abstract string DisplayName { get; }
        public abstract PluginFeatures Features { get; }

        public virtual void Initialize() { }
        public virtual void CleanUp() { }

        public Task<MSD_OUT> ModifySendDataAsync(MSD_IN data)
        {
            return Task.Run(() => ModifySendData(data));
        }

        protected virtual MSD_OUT ModifySendData(MSD_IN data)
        {
            throw new NotImplementedException();
        }
    }
}
