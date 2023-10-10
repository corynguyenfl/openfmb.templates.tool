// SPDX-FileCopyrightText: 2021 Open Energy Solutions Inc

using Google.Protobuf.Reflection;
using System.Reflection;

namespace OpenFMB.Templates.Tool
{
    public static class ProfileRegistry
    {
        public static readonly Dictionary<string, MessageDescriptor> Profiles = new();
        public static readonly Dictionary<string, string> ProfileDeviceTagMap = new();

        public static readonly List<Module> Modules = new();

        static ProfileRegistry()
        {
            var assembly = Assembly.Load("OpenFMB.Models");
            var namespaces = assembly.GetTypes()
                         .Select(t => t.Namespace)
                         .Distinct();

            foreach (var ns in namespaces)
            {
                var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal));

                foreach (var t in types)
                {
                    if (t.Name.EndsWith("ReadingProfile")
                        || t.Name.EndsWith("EventProfile")
                        || t.Name.EndsWith("StatusProfile")
                        || t.Name.EndsWith("ControlProfile")
                        || t.Name.EndsWith("ScheduleProfile")
                        || t.Name.EndsWith("AvailabilityProfile")
                        || t.Name.EndsWith("RequestProfile")
                        || t.Name.EndsWith("CapabilityOverrideProfile")
                        || t.Name.EndsWith("CapabilityProfile"))
                    {
                        var prop = t.GetProperty("Descriptor");
                        if (prop != null)
                        {
                            var descriptor = prop.GetValue(null) as MessageDescriptor;
                            Profiles.Add(t.Name, descriptor);
                            var module = descriptor.FullName.Split('.')[0];

                            var m = Modules.FirstOrDefault(x => x.Name == module);
                            if (m == null)
                            {
                                m = new Module()
                                {
                                    Name = module
                                };
                                Modules.Add(m);
                            }
                            m.Profiles.Add(t.Name);

                            if (!ProfileDeviceTagMap.TryGetValue(module, out string tag))
                            {
                                foreach (var f in descriptor.Fields.InFieldNumberOrder())
                                {
                                    if (f.FieldType == FieldType.Message)
                                    {
                                        var mrid = f.MessageType.FindFieldByName("mRID");
                                        if (mrid == null)
                                        {
                                            SetDeviceTagForProfile(module, f);
                                        }
                                        else
                                        {
                                            if (f.Name != "identifiedObject")
                                            {
                                                ProfileDeviceTagMap[module] = f.Name;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Modules.Sort((x, y) => x.Name.CompareTo(y.Name));
        }

        private static void SetDeviceTagForProfile(string module, FieldDescriptor fieldDescriptor)
        {
            foreach (var c in fieldDescriptor.MessageType.Fields.InFieldNumberOrder())
            {
                if (c.FieldType == FieldType.Message)
                {
                    var mrid = c.MessageType.FindFieldByName("mRID");

                    if (mrid == null)
                    {
                        SetDeviceTagForProfile(module, c);
                    }
                    else
                    {
                        if (c.Name != "identifiedObject")
                        {
                            ProfileDeviceTagMap[module] = fieldDescriptor.Name;
                            break;
                        }
                    }
                }
            }
        }

        public static string GetProfileFullName(string profileName)
        {
            if (Profiles.TryGetValue(profileName, out MessageDescriptor descriptor))
            {
                return descriptor.ClrType.FullName;
            }
            else
            {
                throw new ArgumentException($"'{profileName}' is not a valid profile name.");
            }
        }

        public static string GetDeviceTagForProfile(string profileName)
        {
            string deviceName = null;
            if (Profiles.TryGetValue(profileName, out MessageDescriptor descriptor))
            {
                var module = descriptor.FullName.Split('.')[0];
                ProfileDeviceTagMap.TryGetValue(module, out deviceName);
            }

            return deviceName;
        }

        public static string GetDeviceTagForModule(string module)
        {
            ProfileDeviceTagMap.TryGetValue(module, out string tag);
            return tag;
        }

        public static Module GetModuleByProfileName(string profileName)
        {
            foreach (var m in Modules)
            {
                foreach (var p in m.Profiles)
                {
                    if (p.ToLower() == profileName.ToLower())
                    {
                        return m;
                    }
                }
            }
            return null;
        }

        public static bool IsControlProfile(string profileName)
        {
            return profileName.EndsWith("ControlProfile");
        }
    }

    public class Module
    {
        public string Name { get; set; }
        public List<string> Profiles { get; } = new List<string>();
    }
}
