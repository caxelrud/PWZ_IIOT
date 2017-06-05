using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Azure.IoTHub.Examples.CSharp.Core;


namespace ReadDeviceToCloudMessages
{
    /// <summary>
    /// Sample program to process device messages sent to Azure IoT Hub.
    /// </summary>
    public class Program
    {
        private static EventHubClient _eventHubClient;

        /// <summary>
        /// Receives the messages from device (async).
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns></returns>
        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = _eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                var eventData = await eventHubReceiver.ReceiveAsync(TimeSpan.FromSeconds(1));
                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
            }
        }

        public static void Main(string[] args)
        {
            var config = @"../../../config/config.yaml".GetIoTConfiguration();

            Console.WriteLine("Receive messages. Ctrl-C to exit.");
            _eventHubClient = EventHubClient.CreateFromConnectionString(
                config.AzureIoTHubConfig.ConnectionString, 
                config.AzureIoTHubConfig.IoTHubD2CEndpoint);

            var d2CPartitions = _eventHubClient.GetRuntimeInformation().PartitionIds;

            var cts = new CancellationTokenSource();

            // register cancellation request event.
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            Task.WaitAll(d2CPartitions.Select(partition => ReceiveMessagesFromDeviceAsync(partition, cts.Token)).ToArray());
        }
    }
}
