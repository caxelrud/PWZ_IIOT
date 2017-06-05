using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Azure.IoTHub.Examples.CSharp.Core;
using Microsoft.Azure.Devices.Common;

namespace SendCloudToDeviceMessages
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Sends a cloud to device message (async).
        /// </summary>
        /// <param name="serviceClient">The service client.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static async Task SendCloudToDeviceMessageAsync(ServiceClient serviceClient, string deviceId, string message)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(message)) {Ack = DeliveryAcknowledgement.Full};
            await serviceClient.SendAsync(deviceId, commandMessage);
        }

        /// <summary>
        /// Receives the message ACK from the device (async).
        /// </summary>
        /// <param name="serviceClient">The service client.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task ReceiveFeedbackAsync(ServiceClient serviceClient, CancellationToken token)
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            Console.WriteLine("Waiting for c2d ACK from service");
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Listenter Cancelled, Exiting ...");
                    break;
                }

                var result = await feedbackReceiver.ReceiveAsync(TimeSpan.FromSeconds(1));
                if (result == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received ACK: {0}", string.Join(", ", result.Records.Select(f => f.StatusCode)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(result);
            }
        }

        /// <summary>
        /// Receives the file upload notification (async).
        /// </summary>
        /// <param name="serviceClient">The service client.</param>
        /// <returns></returns>
        private static async Task ReceiveFileUploadNotificationAsync(ServiceClient serviceClient)
        {
            var notificationReceiver = serviceClient.GetFileNotificationReceiver();

            Console.WriteLine("Receiving file upload notification from service");
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync(TimeSpan.FromSeconds(1));
                if (fileUploadNotification == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received file upload noticiation: {0}", string.Join(", ", fileUploadNotification.BlobName));
                Console.ResetColor();

                await notificationReceiver.CompleteAsync(fileUploadNotification);
            }
        }

        private static async Task MessageSendTaskAsync(ServiceClient serviceClient, string deviceId, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Listenter Cancelled, Exiting ...");
                    break;
                }

                Console.WriteLine("Type a message and press <ENTER> to send a C2D message.");
                // niave impl - press ctrl-c twice or close window to exit
                var message = Console.ReadLine();

                if (!message.IsNullOrWhiteSpace())
                    await SendCloudToDeviceMessageAsync(serviceClient, deviceId, message);
            }
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

            Console.WriteLine("Send Cloud-to-Device message");
            var serviceClient = ServiceClient.CreateFromConnectionString(config.AzureIoTHubConfig.ConnectionString);

            // monitor for ACKs from cloud to device mesages
            var ackTask = ReceiveFeedbackAsync(serviceClient, cts.Token);

            // monitor for File Upload Notifications
            var fileNotificationTask = ReceiveFileUploadNotificationAsync(serviceClient);


            var sendTask = MessageSendTaskAsync(serviceClient, config.DeviceConfigs.First().DeviceId, cts.Token);

            Task.WaitAll(ackTask, fileNotificationTask, sendTask);
            
        }
    }
}




