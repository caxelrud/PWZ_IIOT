using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.IoTHub.Examples.CSharp.Core;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;

namespace DeviceProperties
{
    class Program
    {
        public static async Task<Device> GetDeviceProperties(DeviceClient registryManager, string deviceId)
        {
            var props = await registryManager..GetDeviceAsync(deviceId);
            return props
        }

        static void Main(string[] args)
        {
            var config = @"../../../config/config.yaml".GetIoTConfiguration();

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            Console.WriteLine("Send Cloud-to-Device message\n");
            var registryManager = DeviceClient.CreateFromConnectionString(config.AzureIoTHubConfig.ConnectionString);
            
        }
    }
}
