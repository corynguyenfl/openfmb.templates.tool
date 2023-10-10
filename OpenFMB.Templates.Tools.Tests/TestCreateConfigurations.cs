using OpenFMB.Templates.Tools.Configurations;

namespace OpenFMB.Templates.Tools.Tests
{
    [TestClass]
    public class TestCreateConfigurations
    {
        [TestMethod]
        public void TestGenerateAdapterConfigurations1()
        {
            var config = new Parameters
            {
                DevicePort = 20001,
                DeviceMrids = new List<string>() { Guid.NewGuid().ToString().ToLower() }
            };

            var f = "templates/template1.yaml";
            var yaml = File.ReadAllText(f);

            var tuple = ConfigGenerator.GenerateAdapterConfigurations(yaml, config);

            Assert.IsTrue(tuple.Item1.IndexOf("dnp3-master:") > 0);
        }

        [TestMethod]
        public void TestGenerateAdapterConfigurations2()
        {
            var config = new Parameters
            {
                DevicePort = 502,
                DeviceMrids = new List<string>() { Guid.NewGuid().ToString().ToLower() }
            };

            var f = "templates/template2.yaml";
            var yaml = File.ReadAllText(f);

            var tuple = ConfigGenerator.GenerateAdapterConfigurations(yaml, config);

            Assert.IsTrue(tuple.Item1.IndexOf("modbus-master:") > 0);
        }

        [TestMethod]
        public void TestGenerateAdapterConfigurations3()
        {
            var config = new Parameters
            {
                DevicePort = 20001,
                DeviceMrids = new List<string>() { Guid.NewGuid().ToString().ToLower() }
            };

            var f = "templates/template3.yaml";
            var yaml = File.ReadAllText(f);

            var tuple = ConfigGenerator.GenerateAdapterConfigurations(yaml, config);

            Assert.IsTrue(tuple.Item1.IndexOf("dnp3-master:") > 0);
        }
    }
}