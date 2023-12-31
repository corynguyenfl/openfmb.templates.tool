﻿using YamlDotNet.RepresentationModel;

namespace OpenFMB.Templates.Tools.Configurations.Plugins
{
    public class NatsPlugin : IPlugin
    {
        public bool Enabled { get; set; }

        public string Name => PluginsSection.Nats;

        public int MaxQueuedMessages { get; set; } = 100;

        public string ConnectUrl { get; set; } = "nats://localhost:4222";

        public int ConnectRetrySeconds { get; set; } = 5;

        public NatsSecurity Security { get; } = new NatsSecurity();

        public List<ITopic> Publishes { get; } = new List<ITopic>();

        public List<ITopic> Subscribes { get; } = new List<ITopic>();

        public YamlNode ToYaml()
        {
            var node = new YamlMappingNode
            {
                { "enabled", Enabled.ToString().ToLower() },
                { "max-queued-messages", MaxQueuedMessages.ToString() },
                { "connect-url", ConnectUrl },
                { "connect-retry-seconds", ConnectRetrySeconds.ToString() },

                {
                    "security",
                    new YamlMappingNode(
                        new YamlScalarNode("security-type"), new YamlScalarNode(Security.SecurityType.ToString().ToLower()),
                        new YamlScalarNode("ca-trusted-cert-file"), new YamlScalarNode(Security.CertFile),
                        new YamlScalarNode("client-private-key-file"), new YamlScalarNode(Security.ClientKey),
                        new YamlScalarNode("client-cert-chain-file"), new YamlScalarNode(Security.ClientCert),
                        new YamlScalarNode("jwt-creds-file"), new YamlScalarNode(Security.JwtCredsFile))
                }
            };

            var publish = new YamlSequenceNode();
            node.Add("publish", publish);

            foreach (var p in Publishes)
            {
                publish.Add(new YamlMappingNode(
                    new YamlScalarNode("profile"), new YamlScalarNode(p.Profile),
                    new YamlScalarNode("subject"), new YamlScalarNode(p.Subject)));
            }

            var subscribe = new YamlSequenceNode();
            node.Add("subscribe", subscribe);

            foreach (var p in Subscribes)
            {
                subscribe.Add(new YamlMappingNode(
                    new YamlScalarNode("profile"), new YamlScalarNode(p.Profile),
                    new YamlScalarNode("subject"), new YamlScalarNode(p.Subject)));
            }

            return node;
        }
    }

    public class NatsSecurity
    {
        public SecurityType SecurityType { get; set; } = SecurityType.tls_mutual_auth;

        public string CertFile { get; set; } = "cert.pem";

        public string ClientKey { get; set; } = "client_key.pem";

        public string ClientCert { get; set; } = "client_cert.pem";

        public string JwtCredsFile { get; set; } = "";
    }
}
