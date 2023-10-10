using openfmb.templates.tool.Configurations.Plugins;
using YamlDotNet.RepresentationModel;

namespace openfmb.templates.tool.Configurations
{
    public class MainConfiguration
    {
        public LoggingSection Logging { get; } = new LoggingSection();

        public PluginsSection Plugins { get; } = new PluginsSection();

        public MainConfiguration(Parameters parameters)
        {
            if (parameters.Protocol == Protocol.DNP3)
            {
                var plugin = new Dnp3MasterPlugin
                {
                    Enabled = true
                };
                plugin.Sessions.Add(new Session(PluginsSection.Dnp3Master)
                {
                    Name = "session1",
                    Path = parameters.TemplatePath,
                });
                Plugins.Plugins.Add(plugin);
            }
            else if (parameters.Protocol == Protocol.MODBUS)
            {
                var plugin = new ModbusMasterPlugin
                {
                    Enabled = true
                };
                plugin.Sessions.Add(new Session(PluginsSection.ModbusMaster)
                {
                    Name = "session1",
                    Path = parameters.TemplatePath,
                });
                Plugins.Plugins.Add(plugin);
            }

            if (parameters.TransportProtocol == TransportProtocol.NATS)
            {
                var natsPlugin = new NatsPlugin
                {
                    ConnectUrl = parameters.NatsUrl
                };
                natsPlugin.Security.JwtCredsFile = parameters.NatsJwtCredsFile;
                natsPlugin.Publishes.AddRange(parameters.PublishTopics);
                natsPlugin.Subscribes.AddRange(parameters.SubscribeTopics);
                Plugins.Plugins.Add(natsPlugin);
            }
            else if (parameters.TransportProtocol == TransportProtocol.MQTT)
            {
                var mqttPlugin = new MqttPlugin
                {
                    ConnectUrl = parameters.MqttUrl
                };
                mqttPlugin.Security.Username = parameters.MqttUsername;
                mqttPlugin.Security.Password = parameters.MqttPassword;
                mqttPlugin.Publishes.AddRange(parameters.PublishTopics);
                mqttPlugin.Subscribes.AddRange(parameters.SubscribeTopics);
                Plugins.Plugins.Add(mqttPlugin);
            }
        }

        private YamlStream GetYamlStream()
        {
            var stream = new YamlStream();
            var root = new YamlMappingNode();
            var doc = new YamlDocument(root);

            stream.Add(doc);

            // file info            
            root.Add("file", FileInformation.ToYaml());

            // Logging            
            root.Add(Logging.Name, Logging.ToYaml());

            // Plugins           
            root.Add(Plugins.Name, Plugins.ToYaml());

            return stream;
        }

        public string Generate()
        {
            var stream = GetYamlStream();
            using var writer = new StringWriter();
            stream.Save(writer, assignAnchors: false);

            // Replace
            var s = writer.ToString().Replace("\r\n", "\n");
            return s;
        }
    }
}
