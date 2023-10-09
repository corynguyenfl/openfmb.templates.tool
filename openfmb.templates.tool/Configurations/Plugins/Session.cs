using YamlDotNet.RepresentationModel;

namespace openfmb.templates.tool.Configurations.Plugins
{
    public class Session
    {
        public const string LocalPathKey = "local-path";
        public const string SessionNameKey = "session-name";
        public const string PathKey = "path";
        public const string OverridesKey = "overrides";
        public const string KeyKey = "key";
        public const string ValueKey = "value";

        public string Name { get; set; } = "Session";

        public string PluginName { get; private set; }

        public string Path { get; set; } = "";

        public Session(string plugin)
        {
            PluginName = plugin;
        }

        public YamlMappingNode ToYaml()
        {
            var ss = new YamlMappingNode
            {
                { PathKey, Path },
                { LocalPathKey, "" },
                { SessionNameKey, Name }
            };
            var overrides = new YamlSequenceNode();
            ss.Add(OverridesKey, overrides);

            return ss;
        }
    }
}
