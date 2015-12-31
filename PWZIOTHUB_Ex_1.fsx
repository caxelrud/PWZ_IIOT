//PWZIOTHUB_Ex_1

(* Part 1
//From https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/
*)
//Create Device Identity

#I @"C:\Project(Comp)\Dev_2015\PWEIOT_2015"

#r @".\lib\Microsoft.Azure.Amqp.dll"
#r @".\lib\Microsoft.Azure.Devices.dll"
#r @".\lib\Newtonsoft.json.dll"
#r @"System.Data"
#r @"System.Data.DataSetExtensions"
#r @"System.Net.Http"
#r @".\lib\System.Net.Http.Formatting.dll"
#r @"System.Runtime.Serialization"
#r @".\lib\System.Web.Http.dll"
#r @"System.Xml"
#r @"System.Xml.Linq"

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
let connectionString = "HostName=PWZIOTHUB1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=*******"

let AddDeviceAsync= async {        
    let deviceId = "Device2"
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
    printfn "Generated device key: %A"  device.Authentication.SymmetricKey.PrimaryKey
    }

registryManager <-RegistryManager.CreateFromConnectionString(connectionString)

Async.RunSynchronously AddDeviceAsync
(*System.AggregateException: One or more errors occurred. ---> System.MissingMethodException: Method not found: 'Void Newtonsoft.Json.Serialization.DefaultContractResolver.set_IgnoreSerializableAttribute(Boolean)'.
   at System.Net.Http.Formatting.JsonContractResolver..ctor(MediaTypeFormatter formatter)
*)

//GetDeviceAsync---------------
let t1= async {
        let! r1=Async.AwaitTask(registryManager.GetDevicesAsync(maxCount=2))
        return r1
              } 
Async.RunSynchronously t1
        
//-----------------------------------------------------------------------------
(*
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

static RegistryManager registryManager;
static string connectionString = "{iothub connection string}";

private async static Task AddDeviceAsync()
{
    string deviceId = "myFirstDevice";
    Device device;
    try
    {
        device = await registryManager.AddDeviceAsync(new Device(deviceId));
    }
    catch (DeviceAlreadyExistsException)
    {
        device = await registryManager.GetDeviceAsync(deviceId);
    }
    Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
}

registryManager = RegistryManager.CreateFromConnectionString(connectionString);
AddDeviceAsync().Wait();
Console.ReadLine();
*)


//===================================================
(*
//Example 0:
module Async =
    let AwaitVoidTask : (Task -> Async<unit>) =
        Async.AwaitIAsyncResult >> Async.Ignore

let books = localdb.GetCollection<BsonDocument>("books")
let book1=BsonDocument().Add(BsonE("author", "Ernest Hemingway")).Add(BsonE("title", "For Whom the Bell Tolls"))
let t2= books.InsertOneAsync(book1)
let wf2 = async {do! t2 |> Async.AwaitVoidTask}
Async.RunSynchronously wf2

let wf3 = async {
    let t3= books.Find(book1).FirstAsync()
    let! result = Async.AwaitTask(t3)
    return result
    }
let r1=Async.RunSynchronously wf3
r1.ToJson();;

let wf4 = async {
    let t4=localdb.DropCollectionAsync("Queue1")
    let t5=localdb.DropCollectionAsync("Queue1Seq")
    do! t4 |> Async.AwaitVoidTask
    do! t5 |> Async.AwaitVoidTask
    }
Async.RunSynchronously wf4



//Example 1
//let cancellationTokenSource = ref None
let mutable (cancellationTokenSource:CancellationTokenSource option) = None
let start()  = 
        let cts = new CancellationTokenSource()
        let token = cts.Token
        let config = { defaultConfig with cancellationToken = token}
        //let config =  { defaultConfig with bindings = [ HttpBinding.mk HTTP IPAddress.Loopback 3000us ] }
        startWebServerAsync config app
        |> snd
        |> Async.StartAsTask 
        |> ignore
        //cancellationTokenSource := Some cts
        cancellationTokenSource <- Some cts
        true

let stop()= 
        match cancellationTokenSource with
        | Some cts -> cts.Cancel()
        | None -> ()
        true
//To Start
start();;
//To Stop
stop();;//cancellation is not working!

//Example 2
let getSource() =
    async {
        let request = WebRequest.Create("https://raw.githubusercontent.com/fsprojects/FSharp.Control.Reactive/master/src/Observable.fs")
        let! response = request.AsyncGetResponse()
        use stream = response.GetResponseStream()
        use reader = new StreamReader(stream)
        return! Async.AwaitTask(reader.ReadToEndAsync())
    } |> Async.RunSynchronously

//Example 3
let timer1= new System.Timers.Timer(30000.0) 
timer1.AutoReset<-true
timer1.Start()
let timefun2()=
    printfn "Start"
    printfn "timefun1-Thread %d" Thread.CurrentThread.ManagedThreadId
    timer1.Elapsed |>Observable.subscribe (fun _ ->printfn "tick %A" DateTime.Now) |>ignore  //local observable
    ()
let taskBody2=new Action(timefun2)
let task2=Task.Factory.StartNew(taskBody2)
//wait
task2.Dispose();;//Didn't work
//wait
timer1.Stop();;
timer1.Start();;
//wait
timer1.Stop();;

//Example 4
module TaskHelper

open System.Threading.Tasks
// Source code from: http://theburningmonk.com/2012/10/f-helper-functions-to-convert-between-asyncunit-and-task/

[<AutoOpen>]
module Async =
    let inline awaitPlainTask (task: Task) = 
        // rethrow exception from preceding task if it fauled
        let continuation (t : Task) : unit =
            match t.IsFaulted with
            | true -> raise t.Exception
            | arg -> ()
        task.ContinueWith continuation |> Async.AwaitTask
 
    let inline startAsPlainTask (work : Async<unit>) = Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)


*)


