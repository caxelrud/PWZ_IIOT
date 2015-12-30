// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.Threading.Tasks

open Microsoft.Azure.Devices
open Microsoft.Azure.Devices.Common.Exceptions

module Async =
    let AwaitVoidTask : (Task -> Async<unit>) =
        Async.AwaitIAsyncResult >> Async.Ignore

let mutable registryManager:RegistryManager=null
let connectionString = "HostName=PWZIOTHUB1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=JR8bAe3jN5n3ZqDpHenA+F0QoeiIN6Jg9ooHKM3nXmE="


let AddDeviceAsync (deviceId:string)= async {        
        let mutable device:Device=null
        try
            let! d = Async.AwaitTask(registryManager.AddDeviceAsync(new Device(deviceId)))
            device<-d
            ()
        with
        | :? DeviceAlreadyExistsException ->
            let! d = Async.AwaitTask(registryManager.GetDeviceAsync(deviceId))
            device<-d
            ()
        return device
    }

[<EntryPoint>]
let main argv = 
    registryManager <-RegistryManager.CreateFromConnectionString(connectionString)

    let device=Async.RunSynchronously (AddDeviceAsync "Device2")
    printfn "Generated device key: %A"  device.Authentication.SymmetricKey.PrimaryKey
    Console.ReadLine() |>ignore
    0 
