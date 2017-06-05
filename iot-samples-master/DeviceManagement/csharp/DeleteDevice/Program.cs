using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Azure.IoTHub.Examples.CSharp.Core;

namespace Azure.IotHub.Examples.Csharp.DeleteDevice
{
    /// <summary>
    /// Sample program to delete a device from Azure IoT Hub
    /// </summary>
    public class Program
    {
        private static RegistryManager _registryManager;

        /// <summary>
        /// Adds the device to the IoT Hub or returns an already configured device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        private static async Task<Device> AddDeviceAsync(string deviceId)
        {
            Device device;
            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId));
                Console.WriteLine($"Registered (Id:PrimaryKey): {device.Id}:{device.Authentication.SymmetricKey.PrimaryKey}.");
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceId);
                Console.WriteLine($"Device (Id:PrimaryKey): {device.Id}:{device.Authentication.SymmetricKey.PrimaryKey} already registered.");
            }
            return device;
        }

        /// <summary>
        /// Gets the list of configred devices in the Azure IoT Hub.
        /// </summary>
        /// <returns></returns>
        public static async Task GetDevices()
        {
            Console.WriteLine("Getting Configured Device List ...");
            var devices = await _registryManager.GetDevicesAsync(1000); 

            Console.WriteLine("Configured Device List: (ID:PrimaryKey)");
            devices.ToList().ForEach(x => Console.WriteLine($"\t {x.Id} : {x.Authentication.SymmetricKey.PrimaryKey}"));
        }

        /// <summary>
        /// Removes the supplied device from the Azure IoT Hub.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        public static async Task RemoveDevice(Device device)
        {
            await _registryManager.RemoveDeviceAsync(device);
        }

        
        public static void Main(string[] args)
        {
            var config = @"../../../config/config.yaml".GetIoTConfiguration();
            var testDevice = config.DeviceConfigs.First();
            var azureConfig = config.AzureIoTHubConfig;

            _registryManager = RegistryManager.CreateFromConnectionString(azureConfig.ConnectionString);

            // check that the device is configured, add if not
            var t1 = AddDeviceAsync(testDevice.DeviceId);
            t1.Wait();
            var device = t1.Result;

            //display list of devices.
            GetDevices().Wait();

            // Remove a device.
            Console.WriteLine($"Removing (Id:PrimaryKey): {testDevice.DeviceId}:{testDevice.Key}.");
            RemoveDevice(device).Wait();
            Console.WriteLine($"Removed (Id:PrimaryKey): {testDevice.DeviceId}:{testDevice.Key}.");

            // show device removed.
            GetDevices().Wait();

            // add device back to not interrupt 
            Console.WriteLine("Adding device back into registry.");
            t1 = AddDeviceAsync(testDevice.DeviceId);
            t1.Wait();

            Console.ReadLine();
        }
    }
}
