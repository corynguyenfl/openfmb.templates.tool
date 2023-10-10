using OpenFMB.Templates.Tool;
using OpenFMB.Templates.Tools.Configurations.Plugins;

namespace OpenFMB.Templates.Tools
{
    public class Parameters
    {
        public string DeviceIp { get; set; } = "127.0.0.1";
        public int DevicePort { get; set; } = 20000;
        public List<string> DeviceNames { get; set; } = new List<string>() { "MyDevice" };
        public List<string> DeviceMrids { get; set; } = new List<string>() { Guid.Empty.ToString() };
        public int MasterAddress { get; set; } = 1;
        public int OutstationAddress { get; set; } = 1;

        public Protocol Protocol { get; set; } = Protocol.Unknown;
        public TransportProtocol TransportProtocol { get; set; } = TransportProtocol.NATS;

        public string NatsUrl { get; set; } = "nats://localhost:4222";
        public string NatsJwtCredsFile { get; set; } = "jwt.creds";

        public string MqttUrl { get; set; } = "tcp://localhost:1883";
        public string MqttUsername { get; set; } = "username";
        public string MqttPassword { get; set; } = "password";

        public List<ITopic> PublishTopics { get; set; } = new List<ITopic>();
        public List<ITopic> SubscribeTopics { get; set; } = new List<ITopic>();

        public string TemplatePath { get; set; } = "/openfmb/template.yaml";

        public Tuple<string, string> DeviceIdentifier(int index, string profileName)
        {
            string name = index < DeviceNames.Count ? DeviceNames[index] : DeviceNames[^1];
            string mrid = index < DeviceMrids.Count ? DeviceMrids[index] : DeviceMrids[^1];

            if (ProfileRegistry.IsControlProfile(profileName))
            {
                if (SubscribeTopics.FirstOrDefault(x => x.Profile == profileName && x.Subject == mrid) != null)
                {
                    mrid = Guid.NewGuid().ToString().ToLower();
                }
            }
            else
            {
                if (PublishTopics.FirstOrDefault(x => x.Profile == profileName && x.Subject == mrid) != null)
                {
                    mrid = Guid.NewGuid().ToString().ToLower();
                }
            }

            return Tuple.Create(name, mrid);
        }
    }

    public enum Protocol
    {
        Unknown,
        MODBUS,
        DNP3
    }

    public enum TransportProtocol
    {
        NATS,
        MQTT
    }
}
