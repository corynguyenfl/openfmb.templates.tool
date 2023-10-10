using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenFMB.Templates.Tool;
using OpenFMB.Templates.Tools.Configurations.Plugins;
using System.Dynamic;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace OpenFMB.Templates.Tools.Configurations.Templates
{
    public class TemplatesHelper
    {
        public static string GenerateTemplateFromFile(string templateYamlFilePath, Parameters config)
        {
            var yaml = File.ReadAllText(templateYamlFilePath);
            return GenerateTemplate(yaml, config);
        }

        public static string GenerateTemplate(string yaml, Parameters config)
        {
            var dic = TemplateAsDictionary(yaml);
            return DoGenerateTemplate(dic, config);
        }

        public static Dictionary<object, object> TemplateAsDictionary(string yaml)
        {
            var builder = new DeserializerBuilder().WithNodeTypeResolver(new ScalarNodeTypeResolver());
            var des = builder.Build();
            return des.Deserialize(new StringReader(yaml)) as Dictionary<object, object>;
        }

        public static Protocol GetProtocol(Dictionary<object, object> dic)
        {
            // Determine protocol
            var protocol = Protocol.DNP3;
            if (dic.TryGetValue("auto_polling", out _))
            {
                protocol = Protocol.MODBUS;
            }

            return protocol;
        }

        static string DoGenerateTemplate(Dictionary<object, object> dic, Parameters config)
        {
            // Determine protocol
            config.Protocol = GetProtocol(dic);

            UpdateProtocolConfiguration(dic, config);

            var profiles = dic["profiles"] as List<object>;

            for (int i = 0; i < profiles.Count; i++)
            {
                var p = profiles[i] as Dictionary<object, object>;
                string profileName = p["name"].ToString();

                var deviceInformation = config.DeviceIdentifier(i, profileName);

                var deviceName = deviceInformation.Item1;
                var deviceMrid = deviceInformation.Item2;

                // Update mRID and name
                var module = ProfileRegistry.GetModuleByProfileName(profileName);
                var tag = ProfileRegistry.GetDeviceTagForModule(module.Name);
                var mappings = p["mapping"] as Dictionary<object, object>;

                if (tag.ToLower() == "conductingequipment")
                {
                    var conductingEquipment = mappings["conductingEquipment"] as Dictionary<object, object>;
                    var mrid = conductingEquipment["mRID"] as Dictionary<object, object>;
                    mrid["value"] = deviceMrid;

                    var nameObject = conductingEquipment["namedObject"] as Dictionary<object, object>;
                    var name = nameObject["name"] as Dictionary<object, object>;
                    var v = name["value"] as Dictionary<object, object>;
                    v["value"] = deviceName;
                }
                else
                {
                    var obj = mappings[tag] as Dictionary<object, object>;
                    var conductingEquipment = obj["conductingEquipment"] as Dictionary<object, object>;
                    var mrid = conductingEquipment["mRID"] as Dictionary<object, object>;
                    mrid["value"] = deviceMrid;

                    var nameObject = conductingEquipment["namedObject"] as Dictionary<object, object>;
                    var name = nameObject["name"] as Dictionary<object, object>;
                    var v = name["value"] as Dictionary<object, object>;
                    v["value"] = deviceName;
                }

                // Update subject
                if (ProfileRegistry.IsControlProfile(profileName))
                {
                    config.SubscribeTopics.Add(new Subscribe()
                    {
                        Profile = profileName,
                        Subject = deviceMrid
                    });
                }
                else
                {
                    config.PublishTopics.Add(new Publish()
                    {
                        Profile = profileName,
                        Subject = deviceMrid
                    });
                }
            }

            var stream = GetYamlStream(dic);

            using var writer = new StringWriter();
            stream.Save(writer, assignAnchors: false);

            // Replace
            var s = writer.ToString().Replace("\r\n", "\n");
            //File.WriteAllText("output.yaml", s);
            return s;
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
            Parameters parameters)
        {
            if (parameters.Protocol == Protocol.MODBUS)
            {
                // Device IP address
                dictionary["remote-ip"] = parameters.DeviceIp;
                // Device port
                dictionary["port"] = parameters.DevicePort;
                // Unit identifier
                dictionary["unit-identifier"] = parameters.OutstationAddress;
            }
            else
            {
                var channel = dictionary["channel"] as Dictionary<object, object>;
                // Device IP address
                channel["outstation-ip"] = parameters.DeviceIp;
                // Device port
                channel["port"] = parameters.DevicePort;

                var protocol = dictionary["protocol"] as Dictionary<object, object>;
                // Master address
                protocol["master-address"] = parameters.MasterAddress;
                // Outstation address
                protocol["outstation-address"] = parameters.OutstationAddress;
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
}
