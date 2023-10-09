namespace openfmb.templates.tool.Configurations.Plugins
{
    public class Dnp3MasterPlugin : BasePlugin, IPlugin
    {
        public override string Name => PluginsSection.Dnp3Master;

        public override string SessionTagName => "masters";
    }
}
