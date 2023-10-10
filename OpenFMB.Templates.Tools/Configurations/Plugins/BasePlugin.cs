using YamlDotNet.RepresentationModel;

namespace OpenFMB.Templates.Tools.Configurations.Plugins
{
    public abstract class BasePlugin
    {
        public abstract string SessionTagName { get; }
        public abstract string Name { get; }

        public List<Session> Sessions { get; } = new List<Session>();

        public bool Enabled { get; set; }
        public int ThreadPoolSize { get; set; } = 1;

        public YamlNode ToYaml()
        {
            var node = new YamlMappingNode(
                new YamlScalarNode("enabled"), new YamlScalarNode(Enabled.ToString().ToLower()),
                new YamlScalarNode("thread-pool-size"), new YamlScalarNode(ThreadPoolSize.ToString()));

            var masters = new YamlSequenceNode();
            node.Add(SessionTagName, masters);

            foreach (var session in Sessions)
            {
                var yaml = session.ToYaml();
                masters.Add(yaml);
            }

            return node;
        }
    }
}
