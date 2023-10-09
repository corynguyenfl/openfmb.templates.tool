using YamlDotNet.RepresentationModel;

namespace openfmb.templates.tool
{
    public static class YamlHelper
    {
        public static object GetValueByKey(this YamlMappingNode node, string key)
        {
            object obj = null;
            if (node.Children.ContainsKey(key))
            {
                obj = node[key];
            }
            return obj;
        }

        public static bool ContainsKey(this YamlMappingNode node, string key)
        {
            return node.Children.ContainsKey(key);
        }
    }
}
