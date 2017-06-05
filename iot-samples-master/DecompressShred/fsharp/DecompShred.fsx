#r "Microsoft.ServiceBus.dll"
#r "System.Runtime.Serialization.dll"

open System
open System.Text
open System.IO
open System.Diagnostics
open System.IO.Compression
open Microsoft.ServiceBus.Messaging
open System.Runtime.Serialization

let decompress (data : string) = 
    let gzipBuffer = Convert.FromBase64String(data)
    (   use memoryStream = new MemoryStream()
        let dataLength = BitConverter.ToInt32(gzipBuffer, 0)
        memoryStream.Write(gzipBuffer, 4, gzipBuffer.Length - 4)
        let buffer = Array.zeroCreate<byte> (int(dataLength))
        memoryStream.Position <- 0L
        (   use zip = new GZipStream(memoryStream, CompressionMode.Decompress)
            zip.Read(buffer, 0, buffer.Length) |> ignore)
        Encoding.UTF8.GetString(buffer)
    )

let eventHubName = "demo"
let connectionString = "Endpoint=sb://postshred.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=GilIgSl7Rk8wMvuL27m9MyQjN8H5DTJV5WdsVJ+dlSM=";

let Run (input: string, log: TraceWriter) = 
    let groupedData = decompress input
    let eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName)
    
    groupedData.Split([|"|"|], StringSplitOptions.RemoveEmptyEntries)
    |> Array.iter (fun data -> 
        eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(data)))
        log.Info(sprintf "F# Queue trigger function processed: '%s'" data)
        )