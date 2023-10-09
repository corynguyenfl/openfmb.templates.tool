using YamlDotNet.RepresentationModel;

namespace openfmb.templates.tool.Configurations
{
    public class LoggingSection
    {
        public string Name => "logging";
        public string LoggerName { get; set; } = "application";

        public bool RotatingFileEnable { get; set; } = true;

        public string RotatingFilePath { get; set; } = "adapter.log";

        public int RotatingFileMaxSize { get; set; } = 1048576;

        public int RotatingFileMaxFiles { get; set; } = 3;

        public bool ConsoleEnable { get; set; } = true;

        public YamlNode ToYaml()
        {
            var node = new YamlMappingNode
            {
                { "logger-name", LoggerName },
                { "console", new YamlMappingNode(new YamlScalarNode("enabled"), new YamlScalarNode(ConsoleEnable.ToString().ToLower())) },

                {
                    "rotating-file",
                    new YamlMappingNode(
                        new YamlScalarNode("enabled"), new YamlScalarNode(RotatingFileEnable.ToString().ToLower()),
                        new YamlScalarNode("path"), new YamlScalarNode(RotatingFilePath),
                        new YamlScalarNode("max-size"), new YamlScalarNode(RotatingFileMaxSize.ToString()),
                        new YamlScalarNode("max-files"), new YamlScalarNode(RotatingFileMaxFiles.ToString()))
                }
            };

            return node;
        }
    }
}
