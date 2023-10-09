namespace openfmb.templates.tool.Configurations.Plugins
{
    public class ModbusMasterPlugin : BasePlugin, IPlugin
    {
        public override string Name => PluginsSection.ModbusMaster;

        public override string SessionTagName => "sessions";

    }
}
