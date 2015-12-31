using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace CreateDeviceIdentity
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=PWZIOTHUB1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=****";
        private async static Task AddDeviceAsync()
        {
            string deviceId = "Device1";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
        private async static Task GetDevicesAsync()
        {
            IEnumerable<Device> r1;
            r1 = await registryManager.GetDevicesAsync(2);
            foreach (var i in r1) {
                Console.WriteLine("Devices: {0}", i.Id); }
        }

        static void Main(string[] args)
        {
            //
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            //Register
            //AddDeviceAsync().Wait();
            //Get
            GetDevicesAsync().Wait();

            Console.ReadLine();
        }
    }
}
