// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the bottom of this file.
// Learn more about F# at http://fsharp.org

namespace Azure.IoTHub.Examples.FSharp.CreateDeviceIdentity

open System
open System.Linq
open FSharp.Configuration
open Microsoft.Azure.Devices
open Microsoft.Azure.Devices.Common.Exceptions

type Config = YamlConfig<FilePath="../config/config.yaml">

module Main = 
    
    [<EntryPoint>]
    let main argv = 
        let config = Config()
        let configFilePath = "../../../config/config.yaml"

        config.Load(configFilePath)

        let connectionString = config.AzureIoTHubConfig.ConnectionString
        let deviceId = config.DeviceConfigs.First().DeviceId
    
        let printDeviceKey (device: Device) = printfn "Generated device key: %A" device.Authentication.SymmetricKey.PrimaryKey
    
        let registryManager = RegistryManager.CreateFromConnectionString(connectionString)
    
        let addDevice deviceId = 
            registryManager.AddDeviceAsync(new Device(deviceId))
    
        let getDevice deviceId =  
            registryManager.GetDeviceAsync(deviceId)
        
        let writeDeviceKey (device : Device) = 
            config.DeviceConfigs
            |> Seq.iter (fun dc -> 
                    if String.Compare(dc.DeviceId, device.Id) = 0 then
                        dc.Key <- device.Authentication.SymmetricKey.PrimaryKey
                )
            config.Save(configFilePath)
            device
     
        try 
            addDevice deviceId 
            |> Async.AwaitTask 
            |> Async.RunSynchronously 
            |> writeDeviceKey
            |> printDeviceKey
        with 
        | :? System.AggregateException as e ->
            e.InnerExceptions 
            |> Seq.iter (fun ex -> 
                if ex :? DeviceAlreadyExistsException then 
                    getDevice deviceId 
                    |> Async.AwaitTask 
                    |> Async.RunSynchronously 
                    |> writeDeviceKey
                    |> printDeviceKey
                )
    
    
        let console = Console.ReadLine()
    
        0 // return an integer exit code
