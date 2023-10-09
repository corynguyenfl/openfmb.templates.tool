using openfmb.templates.tool.Configurations.Templates;

namespace openfmb.templates.tool.Configurations
{
    public class ConfigGenerator
    {
        /// <summary>
        /// Generate the main configuration and the template for the adapter.  
        /// First item in tuple is main configuration, second one is template.
        /// </summary>
        /// <param name="rawTemplateContent"></param>
        /// <param name="parameters"></param>
        /// <returns>Tuple object</returns>
        public static Tuple<string, string> GenerateAdapterConfigurations(
            string rawTemplateContent,
            Parameters parameters
        )
        {
            var template = TemplatesHelper.GenerateTemplate(rawTemplateContent, parameters);

            var config = new MainConfiguration(parameters);

            var mainConfig = config.Generate();

            return Tuple.Create(mainConfig, template);
        }
    }
}
