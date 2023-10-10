using openfmb.templates.tool.Configurations;

namespace openfmb.templates.tool
{
    class Program
    {
        static void Main()
        {
            TestGenerateConfigs();
        }

        static void TestGenerateConfigs()
        {
            var config = new Parameters
            {
                DevicePort = 12345,
                DeviceMrids = new List<string>() { Guid.NewGuid().ToString().ToLower() }
            };

            foreach (var f in Directory.GetFiles("templates", "*.yaml"))
            {
                var yaml = File.ReadAllText(f);

                var tuple = ConfigGenerator.GenerateAdapterConfigurations(yaml, config);

                Console.WriteLine(tuple.Item1);
                Console.WriteLine();
                //Console.WriteLine(tuple.Item2);

            }
        }
    }
}