using openfmb.templates.tool.Configurations.Plugins;
using YamlDotNet.RepresentationModel;

namespace openfmb.templates.tool.Configurations
{
    public class PluginsSection
    {
        public const string Dnp3Master = "dnp3-master";
        public const string ModbusMaster = "modbus-master";
        public const string Log = "log";
        public const string Mqtt = "mqtt";
        public const string Nats = "nats";
        public const string TimescaleDB = "timescaledb";

        public string Name => "plugins";

        public readonly IList<IPlugin> Plugins = new List<IPlugin>();

        public YamlNode ToYaml()
        {
            var node = new YamlMappingNode();

            foreach (var p in Plugins)
            {
                node.Add(p.Name, p.ToYaml());
            }

            return node;
        }
    }
}
