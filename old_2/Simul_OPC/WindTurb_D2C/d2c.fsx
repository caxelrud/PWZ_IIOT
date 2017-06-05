//d2c.fsx

#I @"C:\Users\inter\OneDrive\Projects(Comp)\Dev_2017\IOT_2017\Simul_OPC\WindTurb_D2C\packages"

#r "DotNetty.Buffers.dll"
#r "DotNetty.Codecs.dll"
#r "DotNetty.Codecs.Mqtt.dll"
#r "DotNetty.Common.dll"
#r "DotNetty.Handlers.dll"
#r "DotNetty.Transport.dll"
#r "FSharp.Configuration.dll"
#r "FSharp.Core.dll"
#r "Microsoft.Azure.Amqp.dll"
#r "Microsoft.Azure.Devices.Client.dll"
#r "Microsoft.Azure.KeyVault.Core.dll"
#r "Microsoft.Data.Edm.dll"
#r "Microsoft.Data.OData.dll"
#r "Microsoft.Data.Services.Client.dll"
#r "Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.dll"
#r "Microsoft.WindowsAzure.Storage.dll"
#r "Mono.Security.dll"
#r "Newtonsoft.Json.dll"
#r "PCLCrypto.dll"
#r "PInvoke.BCrypt.dll"
#r "PInvoke.Kernel32.dll"
#r "PInvoke.NCrypt.dll"
#r "PInvoke.Windows.Core.dll"
#r "SharpYaml.dll"
#r "System.Net.Http.Formatting.dll"
#r "System.Spatial.dll"
#r "Validation.dll"
#r "System.Device"

open System 
open System.Linq
open System.Text
open System.IO
open System.IO.Compression
open System.Device.Location
open System.Threading
open FSharp.Configuration
open Microsoft.Azure.Devices.Client
open Newtonsoft.Json

type Config = YamlConfig<FilePath="config.yaml", ReadOnly=true>

type telemetryDataPoint = {
    location : GeoCoordinate 
    deviceId : string 
    windSpeed : float 
    obsTime : DateTime
    }

let config = Config()
config.Load("config.yaml")
let iotHubUri   = config.AzureIoTHubConfig.IoTHubUri
let deviceKey   = config.DeviceConfigs.First().Key
let deviceId    = config.DeviceConfigs.First().DeviceId

let deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey))
    
let avgWindSpeed = 10.
let rand = new Random()

let windSpeedMessage location index =                 
                let telemetryReading = { 
                    deviceId = sprintf "%s%i" deviceId index
                    windSpeed = (avgWindSpeed + rand.NextDouble() * 4. - 2.) 
                    location = location
                    obsTime = DateTime.UtcNow
                    }
                let json = JsonConvert.SerializeObject(telemetryReading)
                json
let getRandomGeoCoordinate seed (lat : float) (long : float) (radius : float) : GeoCoordinate = 
            // check out http://gis.stackexchange.com/a/68275 for where this calc originated.
            let rand = new Random(seed)
            let u = rand.NextDouble()
            let v = rand.NextDouble()
            let w = radius / 111000. * Math.Sqrt(u)
            let t = 2. * Math.PI * v
            let x = (w * Math.Cos(t)) / Math.Cos(lat)
            let y = w * Math.Sin(t)
            GeoCoordinate(y+lat, x+long) 
                   
           
 //http://madskristensen.net/post/Compress-and-decompress-strings-in-C

let compress (data : string) = 
            let buffer = Encoding.UTF8.GetBytes(data)
            let ms = new MemoryStream()
            (   use zip = new GZipStream(ms, CompressionMode.Compress, true)
                zip.Write(buffer, 0, buffer.Length) )
            ms.Position <- 0L
            let compressed = Array.zeroCreate<byte> (int(ms.Length))
            ms.Read(compressed, 0, compressed.Length) |> ignore
            let gzipBuffer = Array.zeroCreate<byte> (int(compressed.Length) + 4)
            Buffer.BlockCopy(compressed, 0, gzipBuffer, 4, compressed.Length)
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzipBuffer, 0, 4)
            Convert.ToBase64String gzipBuffer

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
            
                   
let dataSendTask (data : string) =
            async {
                let compressedData = compress data
                let message = new Message(Encoding.UTF8.GetBytes(compressedData))
                deviceClient.SendEventAsync(message) 
                        |> Async.AwaitTask 
                        |> ignore
                printfn "%O > Sending message %s" (DateTime.Now.ToString()) (decompress compressedData)
            
            }            
let dataReceiveTask (deviceClient : DeviceClient) = 
            let rec task (client : DeviceClient) = async {
                    let! message = client.ReceiveAsync() |> Async.AwaitTask
                    Console.ForegroundColor <- ConsoleColor.Yellow 
                    printfn "Cloud to device message received: %s" (message.GetBytes() |> Encoding.ASCII.GetString)
                    Console.ResetColor() 
                    return! (task client)
                }
            task deviceClient
            

let nycSites = Array.init 10 (fun index -> getRandomGeoCoordinate index 47.643417 -122.126083 (20000.))

// Start Cloud to Device Reader
//dataReceiveTask deviceClient |> Async.Start

let batchDataStreamTasks = 
            Seq.initInfinite ( fun x -> 
                String.concat "|" (nycSites |> Array.mapi (fun idx site -> windSpeedMessage site idx)
                )
            )

batchDataStreamTasks
        |> Seq.iter (fun x -> 
            dataSendTask x 
            |> Async.RunSynchronously
            Async.Sleep 60000 |> Async.RunSynchronously)
        
    

(*
open System
open Microsoft.Azure.Devices
open FSharp.Configuration

open System.Threading.Tasks

//==============================================================
//Create Device
//-------------
type Config = YamlConfig<FilePath="config.yaml">

let config = Config()
let connectionString = config.AzureIoTHubConfig.ConnectionString
let deviceId = config.DeviceConfigs.[0].DeviceId   //First().DeviceId

let printDeviceKey (device: Device) = printfn "Generated device key: %A" device.Authentication.SymmetricKey.PrimaryKey

let registryManager = RegistryManager.CreateFromConnectionString(connectionString)

let addDevice deviceId = 
        registryManager.AddDeviceAsync(new Device(deviceId))


let getDevice deviceId =  
        registryManager.GetDeviceAsync(deviceId)


//Method 1 (OK)
let rec unwrapExMessage (exn:Exception)=
        match exn.InnerException with
            | null -> exn.Message
            | _ -> unwrapExMessage exn.InnerException

let executeAsync deviceId=
    async {
            let! dev= addDevice deviceId 
                        |> Async.AwaitTask 
                        |> Async.Catch
            match dev with
                    | Choice1Of2 device -> printfn "Device %s registered OK" device.Authentication.SymmetricKey.PrimaryKey
                    | Choice2Of2 error ->   printfn "Error! %s" (unwrapExMessage error) 
           }

executeAsync deviceId |> Async.RunSynchronously

//Method 2 (Not returning? Not Working!)
let deviceId2="Device2"
try 
        addDevice deviceId2 
            |> Async.AwaitTask 
            |> Async.RunSynchronously 
            |> printDeviceKey
    with 
    | :? System.AggregateException as e ->
        e.InnerExceptions 
        |> Seq.iter (fun ex -> 
            if ex :? DeviceAlreadyExistsException then 
                getDevice deviceId |> Async.AwaitTask |> Async.RunSynchronously |> printDeviceKey
            )
//===============================================================================================
//Simulate Device
//---------------
open System.Text
open System.Threading
open Microsoft.Azure.Devices.Client
open Newtonsoft.Json

//type Config = YamlConfig<FilePath="../config/config.yaml", ReadOnly=true>
type TelemetryDataPoint = {
    deviceId : string 
    windSpeed : float 
    }

//let config = Config()
let iotHubUri   = config.AzureIoTHubConfig.IoTHubUri
let deviceKey   = config.DeviceConfigs.[0].Key
let deviceId    = config.DeviceConfigs.[0].DeviceId
let deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey))
let avgWindSpeed = 10.
let rand = new Random()

let windSpeedMessage = Seq.initInfinite (fun index -> 
        let telemetryReading = { deviceId = deviceId; windSpeed = (avgWindSpeed + rand.NextDouble() * 4. - 2.) }
        let json = JsonConvert.SerializeObject(telemetryReading)
        let bytes = Encoding.ASCII.GetBytes(json)
        index, new Message(bytes), json
        )

let cancellation = new CancellationTokenSource()
let dataSendTask = 
        async {
            
            windSpeedMessage |> Seq.iter (fun (index, message, json) -> 
                deviceClient.SendEventAsync(message) |> Async.AwaitIAsyncResult |> Async.Ignore |> ignore
                printfn "%O > Sending message %i: %s" (DateTime.Now.ToString()) index json
                Thread.Sleep 1000
                )
        } 
Async.Start (dataSendTask,cancellation.Token)//Async.RunSynchronously
//cancellation.Cancel  //to cancel
*)

