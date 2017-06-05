namespace Azure.IoTHub.Examples.FSharp.DeleteDevice

open System
open System.Linq
open FSharp.Configuration
open Microsoft.Azure.Devices
open Microsoft.Azure.Devices.Common.Exceptions

type Config = YamlConfig<FilePath="../config/config.yaml", ReadOnly=true>

module Main = 
    
    [<EntryPoint>]
    let main argv = 
        let config = Config()
        config.Load("../../../config/config.yaml")
        let connectionString = config.AzureIoTHubConfig.ConnectionString
        let deviceId = config.DeviceConfigs.First().DeviceId
    
        let printDeviceKey (device: Device) = printfn "Generated device key: %A" device.Authentication.SymmetricKey.PrimaryKey
    
        let registryManager = RegistryManager.CreateFromConnectionString(connectionString)
    
        let getDevice deviceId =  
            async {
                let! device = registryManager.GetDeviceAsync(deviceId) |> Async.AwaitTask
                return device
            }

        let addDevice deviceId = 
            async {
                printfn "Adding Device: '%s' to IoT Hub" deviceId
                let result = 
                    try 
                        registryManager.AddDeviceAsync(new Device(deviceId)) 
                        |> Async.AwaitTask 
                        |> Async.RunSynchronously
                    with 
                    | :? System.AggregateException as e ->
                        if (e.InnerExceptions
                            |> Seq.exists (fun ex -> ex :? DeviceAlreadyExistsException)) then
                            getDevice deviceId |> Async.RunSynchronously
                        else 
                            reraise()                                           

                return deviceId
            }
    
        let getDevices (deviceId : string) = 
            async {
                let! devices = registryManager.GetDevicesAsync(1000) |> Async.AwaitTask
                printfn "Listing currently configured devices ... " |> ignore
                devices |> Seq.iter (fun x -> printfn "\t %s : %s" x.Id x.Authentication.SymmetricKey.PrimaryKey)
                return deviceId
            } 

        let removeDevice (deviceId : string) = 
            async {
                printfn "Removing DeviceId: '%s'" deviceId |> ignore
                registryManager.RemoveDeviceAsync(deviceId) |> Async.AwaitTask |> ignore
                return deviceId
            } 



        addDevice deviceId |> Async.RunSynchronously
        |> getDevices |> Async.RunSynchronously
        |> removeDevice |> Async.RunSynchronously
        |> getDevices |> Async.RunSynchronously
        |> addDevice  |> Async.RunSynchronously
        |> ignore
         

        let console = Console.ReadLine()
    
        0 // return an integer exit code