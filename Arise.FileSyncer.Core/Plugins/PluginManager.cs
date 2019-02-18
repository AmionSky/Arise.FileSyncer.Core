using System.Collections.Generic;

namespace Arise.FileSyncer.Plugins
{
    public class PluginManager
    {
        private readonly Dictionary<string, Plugin> plugins;

        public PluginManager()
        {
            plugins = new Dictionary<string, Plugin>();
        }

        /// <summary>
        /// Adds a new plugin.
        /// </summary>
        /// <returns>Successful</returns>
        public bool Add(Plugin plugin)
        {
            if (!plugins.ContainsKey(plugin.Name))
            {
                plugins.Add(plugin.Name, plugin);
                return true;
            }

            return false;
        }

        public bool TryGet(string pluginName, out Plugin plugin)
        {
            return plugins.TryGetValue(pluginName, out plugin);
        }

        public IList<Plugin> GetByFeatures(PluginFeatures features)
        {
            List<Plugin> selected = new List<Plugin>();

            foreach (KeyValuePair<string, Plugin> kv in plugins)
            {
                if (kv.Value.Features.HasFlag(features))
                {
                    selected.Add(kv.Value);
                }
            }

            return selected;
        }
    }
}
