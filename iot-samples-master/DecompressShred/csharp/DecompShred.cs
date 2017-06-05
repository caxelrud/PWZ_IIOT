#r ".\Microsoft.ServiceBus.dll"

using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.ServiceBus.Messaging;
using System.Runtime.Serialization;

public static string eventHubName = "<event-hub-name>";
public static string eventHubConnectionString = "<connection-string>";

public static void SendToEventHub(EventHubClient client, string eventData)
{
    client.Send(new EventData(Encoding.UTF8.GetBytes(eventData)));
}

public static string DecompressMessage(string compressedMessage)
{
    // this string compression code is lifted & modified from 
    // http://madskristensen.net/post/Compress-and-decompress-strings-in-C
    var gzBuffer = Convert.FromBase64String(compressedMessage);
    using (MemoryStream ms = new MemoryStream())
    {
        var msgLength = BitConverter.ToInt32(gzBuffer, 0);
        ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

        var buffer =new byte[msgLength];

        ms.Position = 0;
        using (var zip = new GZipStream(ms, CompressionMode.Decompress))
        {
            zip.Read(buffer, 0, buffer.Length);
        }

        return Encoding.UTF8.GetString(buffer);
    }   
}

public static string[] Shred(string batch)
{
    return batch.Split((new char[] {'|'}), StringSplitOptions.RemoveEmptyEntries);
}

public static void Run(string input, TraceWriter log)
{
    var eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, eventHubName);
    
    var decompressedData = DecompressMessage(input);
    var dataCollection = Shred(decompressedData);
    foreach(var message in dataCollection)
    {
        SendToEventHub(eventHubClient, message);
        log.Info($"C# Event Hub trigger function processed a message: {message}");
    }
}