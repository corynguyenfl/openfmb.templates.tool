using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace OpenFMB.Templates.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config();
            config.DevicePort = 12345;
            config.DeviceMrid = Guid.NewGuid().ToString();

            foreach (var f in Directory.GetFiles("templates", "*.yaml"))
            {
                _ = GetTemplateString(f, config);
            }
        }

        static string GetTemplateString(string filePath, Config config)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    var builder = new DeserializerBuilder().WithNodeTypeResolver(new ScalarNodeTypeResolver());
                    var des = builder.Build();
                    var dic = des.Deserialize(reader) as Dictionary<object, object>;

                    // Determine protocol
                    var protocol = Protocol.DNP3;
                    if (dic.TryGetValue("auto_polling", out object value))
                    {
                        protocol = Protocol.MODBUS;
                    }

                    UpdateProtocolConfiguration(dic, protocol, config);

                    var profiles = dic["profiles"] as List<object>;

                    foreach (Dictionary<object, object> p in profiles.Cast<Dictionary<object, object>>())
                    {
                        try
                        {
                            var jsonStr = JsonConvert.SerializeObject(p, new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });
                            var token = JsonConvert.DeserializeObject(jsonStr) as JToken;

                            string profileName = p["name"].ToString();

                            // Update mRID and name
                            var module = ProfileRegistry.GetModuleByProfileName(profileName);
                            var tag = ProfileRegistry.GetDeviceTagForModule(module.Name);
                            var mappings = p["mapping"] as Dictionary<object, object>;

                            if (tag.ToLower() == "conductingequipment")
                            {
                                var conductingEquipment = mappings["conductingEquipment"] as Dictionary<object, object>;
                                var mrid = conductingEquipment["mRID"] as Dictionary<object, object>;
                                mrid["value"] = config.DeviceMrid;

                                var nameObject = conductingEquipment["namedObject"] as Dictionary<object, object>;
                                var name = nameObject["name"] as Dictionary<object, object>;
                                var v = name["value"] as Dictionary<object, object>;
                                v["value"] = config.DeviceName;
                            }
                            else
                            {
                                var obj = mappings[tag] as Dictionary<object, object>;
                                var conductingEquipment = obj["conductingEquipment"] as Dictionary<object, object>;
                                var mrid = conductingEquipment["mRID"] as Dictionary<object, object>;
                                mrid["value"] = config.DeviceMrid;

                                var nameObject = conductingEquipment["namedObject"] as Dictionary<object, object>;
                                var name = nameObject["name"] as Dictionary<object, object>;
                                var v = name["value"] as Dictionary<object, object>;
                                v["value"] = config.DeviceName;
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    var stream = GetYamlStream(dic);

                    using (var writer = new StringWriter())
                    {
                        stream.Save(writer, assignAnchors: false);

                        // Replace
                        var s = writer.ToString().Replace("\r\n", "\n");
                        var output = Path.GetFileNameWithoutExtension(filePath) + "-output.yaml";
                        File.WriteAllText(output, s);
                        return s;
                    }
                }
            }
            else
            {
                Console.WriteLine($"The file path '{filePath}' referenced by the adapter does not exist.");
            }
            return string.Empty;
        }

        static YamlStream GetYamlStream(Dictionary<object, object> jsonDict)
        {
            var stream = new YamlStream
            {
                ToYamlDocument(jsonDict)
            };
            return stream;
        }

        static YamlDocument ToYamlDocument(Dictionary<object, object> jsonDict)
        {
            var root = new YamlMappingNode();
            var doc = new YamlDocument(root);

            // Hack to get around the fact that YamlDotNet works well with serializing ExpandoObjects
            var json = JsonConvert.SerializeObject(jsonDict);
            var dict = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            var yaml = new Serializer().Serialize(dict);

            var yamlMapping = new Deserializer().Deserialize(yaml, typeof(YamlMappingNode)) as YamlMappingNode;

            foreach (var kvp in yamlMapping)
            {
                root.Add(kvp.Key, kvp.Value);
            }

            return doc;
        }

        private static void UpdateProtocolConfiguration(
            Dictionary<object, object> dictionary,
            Protocol protocolType,
            Config config)
        {
            if (protocolType == Protocol.MODBUS)
            {
                // Device IP address
                dictionary["remote-ip"] = config.DeviceIp;
                // Device port
                dictionary["port"] = config.DevicePort;
                // Unit identifier
                dictionary["unit-identifier"] = config.OutstationAddress;
            }
            else
            {
                var channel = dictionary["channel"] as Dictionary<object, object>;
                // Device IP address
                channel["outstation-ip"] = config.DeviceIp;
                // Device port
                channel["port"] = config.DevicePort;

                var protocol = dictionary["protocol"] as Dictionary<object, object>;
                // Master address
                protocol["master-address"] = config.MasterAddress;
                // Outstation address
                protocol["outstation-address"] = config.OutstationAddress;
            }
        }

        class ScalarNodeTypeResolver : INodeTypeResolver
        {
            bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
            {
                if (currentType == typeof(object))
                {
                    if (nodeEvent is Scalar scalar)
                    {
                        if (scalar.Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            currentType = typeof(int);
                            return true;
                        }
                        if (Regex.IsMatch(scalar.Value, @"^(true|false)$", RegexOptions.IgnorePatternWhitespace))
                        {
                            currentType = typeof(bool);
                            return true;
                        }

                        if (Regex.IsMatch(scalar.Value, @"^-? ( 0 | [1-9] [0-9]* )$", RegexOptions.IgnorePatternWhitespace))
                        {
                            currentType = typeof(int);
                            return true;
                        }

                        if (Regex.IsMatch(scalar.Value, @"^-? ( 0 | [1-9] [0-9]* ) ( \. [0-9]* )? ( [eE] [-+]? [0-9]+ )?$", RegexOptions.IgnorePatternWhitespace))
                        {
                            currentType = typeof(float);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }

    class Config
    {
        public string DeviceIp { get; set; } = "127.0.0.1";
        public int DevicePort { get; set; } = 20000;
        public string DeviceName { get; set; } = "MyDevice";
        public string DeviceMrid { get; set; } = Guid.Empty.ToString();
        public int MasterAddress { get; set; } = 1;
        public int OutstationAddress { get; set; } = 1;
    }

    enum Protocol
    {
        MODBUS,
        DNP3
    }
}