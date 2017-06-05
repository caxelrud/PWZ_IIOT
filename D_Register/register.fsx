//register.fsx
//Azure IOT Hub

(*
References:
(1)
http://www.lucidmotions.net/2016/11/introduction-to-azure-iot-with-fsharp.html
https://github.com/WilliamBerryiii/Azure.IotHub.Examples.FSharp

(2)
https://github.com/hoetz/IoTHubFsharp     
*)

(*
nuget
Install-Package Fsharp.Configuration
Install Package Microsoft.Azure.Devices
Install-Package Microsoft.Azure.Devices.Client 
*)


#I @"C:\Users\inter\OneDrive\Projects(Comp)\Dev_2017\IOT_2017\packages\"

#r @"DotNetty.Buffers.0.4.4\lib\net45\DotNetty.Buffers.dll"
#r @"DotNetty.Common.0.4.4\lib\net45\DotNetty.Common.dll"
#r @"Microsoft.Extensions.DependencyInjection.Abstractions.1.1.0\lib\netstandard1.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll"
#r @"Microsoft.Extensions.Logging.1.1.1\lib\netstandard1.1\Microsoft.Extensions.Logging.dll"
#r @"Microsoft.Extensions.Logging.Abstractions.1.1.1\lib\netstandard1.1\Microsoft.Extensions.Logging.Abstractions.dll"
#r @"Microsoft.Win32.Primitives.4.3.0\lib\net46\Microsoft.Win32.Primitives.dll"


#r @"FSharp.Configuration.1.0.0\lib\net40\FSharp.Configuration.dll"
#r @"Microsoft.AspNet.WebApi.Client.5.2.3\lib\net45\System.Net.Http.Formatting.dll"
#r @"Microsoft.AspNet.WebApi.Core.5.2.3\lib\net45\System.Web.Http.dll"
#r @"Microsoft.Azure.Amqp.2.0.4\lib\net45\Microsoft.Azure.Amqp.dll"
#r @"Microsoft.Azure.Devices.1.2.5\lib\net451\Microsoft.Azure.Devices.dll"
#r @"Microsoft.Azure.Devices.Shared.1.0.10\lib\net45\Microsoft.Azure.Devices.Shared.dll"
#r @"Microsoft.Azure.Devices.Client.1.2.9\lib\net45\Microsoft.Azure.Devices.Client.dll"
#r @"Newtonsoft.Json.6.0.8\lib\net45\Newtonsoft.Json.dll"
#r @"PCLCrypto.2.0.147\lib\net45\PCLCrypto.dll"
#r @"PInvoke.BCrypt.0.3.2\lib\net40\PInvoke.BCrypt.dll"
#r @"PInvoke.Kernel32.0.3.2\lib\net40\PInvoke.Kernel32.dll"
#r @"PInvoke.NCrypt.0.3.2\lib\net40\PInvoke.NCrypt.dll"
//#r @"\PInvoke.Windows.Core.dll"
//#r @"\System.ValueTuple.dll"
#r @"Validation.2.2.8\lib\dotnet\Validation.dll"

(*
#r "System.Data"
#r "System.Data.DataSetExtensions"
#r "System.Net.Http"
#r "System.Xml"
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"

#r @"Microsoft.AspNet.WebApi.Client.5.2.3\lib\net45\System.Net.Http.Formatting.dll"
#r @"Microsoft.AspNet.WebApi.Core.5.2.3\lib\net45\System.Web.Http.dll"
*)

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


//




     


(*
     
*)


