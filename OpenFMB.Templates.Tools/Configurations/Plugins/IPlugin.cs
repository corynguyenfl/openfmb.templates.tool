using YamlDotNet.RepresentationModel;

namespace OpenFMB.Templates.Tools.Configurations.Plugins
{
    public interface IPlugin : IYamlNode
    {
        bool Enabled { get; set; }
    }

    public interface IYamlNode
    {
        string Name { get; }

        YamlNode ToYaml();
    }

    public enum SecurityType
    {
        none,
        tls_server_auth,
        tls_mutual_auth
    }

    public enum AuthenticationType
    {
        none,
        password,
        certificate
    }

    public interface ITopic
    {
        string Profile { get; set; }
        string Subject { get; set; }
    }

    public class Publish : ITopic
    {
        public string Profile { get; set; } = "ENTER_PROFILE_NAME";

        public string Subject { get; set; } = "*";
    }

    public class Subscribe : ITopic
    {
        public string Profile { get; set; } = "ENTER_PROFILE_NAME";

        public string Subject { get; set; } = "*";
    }
}
