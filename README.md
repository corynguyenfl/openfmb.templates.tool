# OpenFMB Configuration Generator

## Overview

A simple tool to manipulate pre-mapped template file and generate:
- Adapter main configuration
- Device specific template

    The following `Parameters` are used to manipulate the pre-mapped template and generate adapter main configuration:
    
    ```csharp
        public class Parameters
        {
            public string DeviceIp { get; set; } = "127.0.0.1";
            public int DevicePort { get; set; } = 20000;
            public List<string> DeviceNames { get; set; } = new List<string>() { "MyDevice" };
            public List<string> DeviceMrids { get; set; } = new List<string>() { Guid.Empty.ToString() };
            public int MasterAddress { get; set; } = 1;
            public int OutstationAddress { get; set; } = 1;

            public Protocol Protocol { get; set; } = Protocol.Unknown;
            public TransportProtocol TransportProtocol { get; set; } = TransportProtocol.NATS;

            public string NatsUrl { get; set; } = "nats://localhost:4222";
            public string NatsJwtCredsFile { get; set; } = "jwt.creds";

            public string MqttUrl { get; set; } = "tcp://localhost:1883";
            public string MqttUsername { get; set; } = "username";
            public string MqttPassword { get; set; } = "password";

            public List<ITopic> PublishTopics { get; set; } = new List<ITopic>();
            public List<ITopic> SubscribeTopics { get; set; } = new List<ITopic>();

            public string TemplatePath { get; set; } = "/openfmb/template.yaml";        
        }
    ```

## Installation

1. NuGet Package:  Install teh library via NuGet Package Manager using the following command (TODO):

    ```mathematica
    Install-Package OpenFMB.Templates.Tools
    ```
2. Manual Download:  You can manually download the library DLL from the GitHub releases page and reference it in your project

## Usage

Here is a simple example of how to use the OpenFMB Template Tools:

```csharp
    using OpenFMB.Templates.Tools.Configurations;

    class Program
    {
        static void Main(string[] args) 
        {
            var config = new Parameters
            {
                DevicePort = 20000,
                DeviceMrids = new List<string>() { Guid.NewGuid().ToString().ToLower() }
            };

            // Pre-mapped template file
            var f = "templates/template1.yaml";
            var yaml = File.ReadAllText(f);

            var tuple = ConfigGenerator.GenerateAdapterConfigurations(yaml, config);
        }
    }
```