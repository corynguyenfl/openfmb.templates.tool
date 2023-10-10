using YamlDotNet.RepresentationModel;

namespace OpenFMB.Templates.Tools.Configurations
{
    public class FileInformation
    {
        public static YamlNode ToYaml()
        {
            var node = new YamlMappingNode
            {
                { "id", "openfmb-adapter-main" },
                { "edition", "2.1" },
                { "version", "2.1.0.0" },
                { "plugin", "" }
            };

            return node;
        }
    }
}
