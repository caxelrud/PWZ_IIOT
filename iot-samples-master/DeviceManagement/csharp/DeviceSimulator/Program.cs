using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

using Azure.IoTHub.Examples.CSharp.Core;

namespace DeviceSimulator
{

    public class TelemetryDataPoint
    {
        public string deviceId { get; set; }
        public DateTime obsTime { get; set; }
        public double windSpeed { get; set; }
        public GeoCoordinate location { get; set; }
    }

    /// <summary>
    /// Sample program that simulates an IoT device for use with Azure IoT Hub.
    /// </summary>
    public class Program
    {
        #region Data Generators

        /// <summary>
        /// Function to generate simple randomized device data as JSON string.
        /// </summary>
        private static readonly Func<string, string> GetFlatSampleDeviceData = deviceId =>
        {
            const double avgWindSpeed = 10; // m/s
            var rand = new Random();

            var currentWindSpeed = avgWindSpeed + rand.NextDouble()*4 - 2;

            var telemetryDataPoint = new TelemetryDataPoint
            {
                deviceId = deviceId,
                obsTime = DateTime.UtcNow,
                windSpeed = currentWindSpeed
            };
            return JsonConvert.SerializeObject(telemetryDataPoint);
        };

        /// <summary>
        /// Function to generate simple randomized device data in CSV format.
        /// </summary>
        private static readonly Func<string, string> GetCSVSampleDeviceData = deviceId =>
        {
            const string schema = "deviceId,obsTime,windSpeed";

            const double avgWindSpeed = 10; // m/s
            var rand = new Random();

            var currentWindSpeed = avgWindSpeed + rand.NextDouble()*4 - 2;


            var telemetryDataPoint = $"{deviceId},{DateTime.UtcNow},{currentWindSpeed}";
            return telemetryDataPoint;
        };

        /// <summary>
        /// Function to generate object graph of randomized device data .
        /// </summary>
        private static readonly Func<string, GeoCoordinate, string> GetJSONObjectGraphSampleDeviceData = 
            (deviceId, geoCoordinate) =>
        {
            const double avgWindSpeed = 10; // m/s
            var rand = new Random();

            var currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

            var telemetryDataPoint = new TelemetryDataPoint
            {
                deviceId = deviceId,
                obsTime = DateTime.UtcNow,
                windSpeed = currentWindSpeed,
                location = geoCoordinate
            };
            return JsonConvert.SerializeObject(telemetryDataPoint);
        };

        private static readonly Func<int, double, double, float, GeoCoordinate> GetRandomGeoCoordinate = 
            (seed, latitude, longitude, radius) =>

        {
            // check out http://gis.stackexchange.com/a/68275 for where this calc originated.
            var rand = new Random(seed);
            var u = rand.NextDouble();
            var v = rand.NextDouble();
            var w = radius/111000*Math.Sqrt(u);
            var t = 2*Math.PI*v;
            var x = (w*Math.Cos(t))/Math.Cos(latitude);
            var y = w*Math.Sin(t);
            return new GeoCoordinate(y + latitude, x + longitude);
        };

        /// <summary>
        /// Function to generate stream of randomized device data .
        /// </summary>
        private static IEnumerable<string> GetAggregateJSONDataStream(string deviceId)
        {
            const double avgWindSpeed = 10; // m/s
            const int locationCount = 10;

            var locations = new List<GeoCoordinate>();
            for (var i = 0; i < locationCount; i++)
            {
                locations.Add(GetRandomGeoCoordinate(i, 47.640568390488625, -122.1293731033802, 5000));
            }
            while (true)
            {
                var telemetryData = locations.Select((loc, index) =>
                {
                    var rand = new Random(DateTime.Now.Millisecond % (index+ 1));
                    var currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

                    return JsonConvert.SerializeObject(new TelemetryDataPoint
                    {
                        deviceId = deviceId + index,
                        obsTime = DateTime.UtcNow,
                        windSpeed = currentWindSpeed,
                        location = loc
                    });
                });

                yield return string.Join("|", telemetryData);
            }
        }
        #endregion Data Generators 

        /// <summary>
        /// Sends the device to cloud messages (async).
        /// </summary>
        /// <param name="deviceClient">The device client.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="dataGenerator">The data generator.</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns></returns>
        private static async Task SendDeviceToCloudMessagesAsync(
            DeviceClient deviceClient, 
            string deviceId, 
            Func<string, string> dataGenerator, 
            CancellationToken ct)
        {
            while (true)
            {
                if (ct.IsCancellationRequested) break;

                var messageString = dataGenerator(deviceId);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000, ct);
            }
        }


        /// <summary>
        /// Receives the cloud to device message (async).
        /// </summary>
        /// <param name="deviceClient">The device client.</param>
        /// <param name="iotStorageContainerName">Name of the iot storage container.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        private static async Task ReceiveC2DAsync(DeviceClient deviceClient, string iotStorageContainerName, CancellationToken ct)
        {
            Console.WriteLine("Receiving cloud to device messages from service");
            while (true)
            {
                if (ct.IsCancellationRequested) break;

                var receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                var messageText = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                switch (messageText.ToLowerInvariant())
                {
                    case "upload bulk data":
                        const string bulkDataUploadFilePath = @"MOCK_DATA.csv";
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Uploading MOCK_DATA");
                        Console.ResetColor();

                        var sw = Stopwatch.StartNew();

                        using (var mockData = new FileStream(bulkDataUploadFilePath, FileMode.Open))
                        {
                            await deviceClient.UploadToBlobAsync(iotStorageContainerName, mockData);
                        }

                        sw.Stop();

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("File uploaded in {0}ms", sw.ElapsedMilliseconds);
                        Console.ResetColor();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Received message: {0}", messageText);
                        Console.ResetColor();
                        break;
                }

                // this will remove the message from the queue.  
                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

        public static void Main(string[] args)
        {
            var config = @"../../../config/config.yaml".GetIoTConfiguration();
            var testDevices = config.DeviceConfigs;
            var azureConfig = config.AzureIoTHubConfig;

            var receiveTasks = new List<Task>();
            var sendTasks = new List<Task>();
            foreach (var testDevice in testDevices)
            {


                Console.WriteLine("Simulated device");
                var deviceClient = DeviceClient.Create(
                    azureConfig.IoTHubUri,
                    new DeviceAuthenticationWithRegistrySymmetricKey(testDevice.DeviceId, testDevice.Key));

                var cts = new CancellationTokenSource();

                // register cancellation request event.
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("Exiting ...");
                };


                /*  
                 *  Data Generator Functions
                 *  Select a message type by uncommenting an assignment below
                */
                // var dataGenerator = GetFlatSampleDeviceData;
                // var dataGenerator = GetCSVSampleDeviceData;
                // var dataGenerator = GetJSONObjectGraphSampleDeviceData;



                /* 
                 * kick off simple cloud data send.
                 */
                //var sendTask = SendDeviceToCloudMessagesAsync(deviceClient, testDevice.DeviceId, dataGenerator, cts.Token);



                /* 
                 * run batched & compressed data generator
                 */
                sendTasks.Add(Task.Run(() =>
                    GetAggregateJSONDataStream(testDevice.DeviceId)
                        .Compress()
                        .SendDeviceToCloudMessageAsync(deviceClient), cts.Token)

                );

                /* 
                 * kick off could data receive
                 */
                
                receiveTasks.Add(ReceiveC2DAsync(deviceClient, azureConfig.IoTHubStorageContainer, cts.Token));
            }
            var list = new List<Task>();
            list.AddRange(sendTasks);
            list.AddRange(receiveTasks);
            Task.WaitAll(list.ToArray());


            Console.ReadLine();
        }
    }

    public static class ExtensionMethods
    {
        public static IEnumerable<string> Compress(this IEnumerable<string> stream)
        {
            foreach (var message in stream)
            {
                // this string compression code is lifted & modified from 
                // http://madskristensen.net/post/Compress-and-decompress-strings-in-C
                var buffer = Encoding.UTF8.GetBytes(message);
                var ms = new MemoryStream();
                using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(buffer, 0, buffer.Length);
                }

                ms.Position = 0;
                var compressed = new byte[ms.Length];
                ms.Read(compressed, 0, compressed.Length);

                var gzBuffer = new byte[compressed.Length + 4];
                Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);

                yield return Convert.ToBase64String(gzBuffer);
            }
        }

        public static async Task SendDeviceToCloudMessageAsync(this IEnumerable<string> stream, DeviceClient deviceClient)
        {
            foreach (var data in stream)
            {
                var message = new Message(Encoding.ASCII.GetBytes(data));
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, data);
                await Task.Delay(5000);
            }
        }
    }
}
