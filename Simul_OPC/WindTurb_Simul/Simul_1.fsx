//Simul_1.fsx
//http://madskristensen.net/post/Compress-and-decompress-strings-in-C
//check out http://gis.stackexchange.com/a/68275 for where this calc originated.

#I @"C:\Users\inter\OneDrive\Projects(Comp)\Dev_2017\IOT_2017\Simul_OPC\packages"
#I @"C:\Users\inter\OneDrive\Projects(Comp)\Dev_2017\IOT_2017\Simul_OPC\packages\QuickOPC"

#r "System.Device.dll"

//OPC
#r "OpcLabs.BaseLib.dll"
#r "OpcLabs.EasyOpcClassic.dll"
#r "OpcLabs.EasyOpcUA.dll"

// Classic OPC

open OpcLabs.EasyOpc
open OpcLabs.EasyOpc.DataAccess
open OpcLabs.EasyOpc.DataAccess.AddressSpace
open OpcLabs.EasyOpc.DataAccess.OperationModel //CA


open System 
open System.Device.Location


type telemetryDataPoint = {
    location : GeoCoordinate 
    deviceId : string 
    windSpeed : float 
    obsTime : DateTime
    }

let rand = Random()

let getRandomGeoCoordinate seed (lat : float) (long : float) (radius : float) : GeoCoordinate = 
    // check out http://gis.stackexchange.com/a/68275 for where this calc originated.
    //let rand = Random()//Random(seed)
    let u = rand.NextDouble()
    let v = rand.NextDouble()
    let w = radius / 111000. * Math.Sqrt(u)
    let t = 2. * Math.PI * v
    let x = (w * Math.Cos(t)) / Math.Cos(lat)
    let y = w * Math.Sin(t)
    GeoCoordinate(y+lat, x+long) 

let nycSites = Array.init 10 (fun index -> getRandomGeoCoordinate index 47.643417 -122.126083 (20000.))
let avgWindSpeed = 10.
let deviceId="Device1"

let windSpeedMessage location index =             
    let telemetryReading = { 
        deviceId = sprintf "%s-%i" deviceId index
        windSpeed = (avgWindSpeed + rand.NextDouble() * 4. - 2.) 
        location = location
        obsTime = DateTime.UtcNow
        }
    telemetryReading



let r=nycSites |> Array.mapi (fun idx site -> windSpeedMessage site idx)
let d=r.[0].deviceId
let w=r.[0].windSpeed
let l=r.[0].location
let t=r.[0].obsTime
r.Length
//DateTime.FromOADate Method



//6.1.1.1 Reading from OPC Classic Items
//A single item
let easyDAClient = new EasyDAClient()

//Multiple items-Kepware
let p1a=ServerDescriptor("", "Kepware.KEPServerEX.V5")

let p2a=[|DAItemDescriptor("Channel1.Device1.D1-Date"); 
          DAItemDescriptor("Channel1.Device1.D1-Lat-1");
          DAItemDescriptor("Channel1.Device1.D1-Log-1");
          DAItemDescriptor("Channel1.Device1.D1-WS-1"); |]
let vtqResults2 = easyDAClient.ReadMultipleItems(p1a,p2a)
(*val vtqResults2 : OperationModel.DAVtqResult [] =
  [|Success; 12/30/1899 12:00:00 AM {System.DateTime} @2017-06-03T03:53:44.120; GoodNonspecific (192);
    Success; 0 {System.Double} @2017-06-03T03:53:44.120; GoodNonspecific (192);
    Success; 0 {System.Int32} @2017-06-03T03:53:44.120; GoodNonspecific (192);
    Success; 0 {System.Int32} @2017-06-03T03:53:44.120; GoodNonspecific (192)|]  *)

// This example shows how to write a value into a single item.
easyDAClient.WriteItemValue("", "Kepware.KEPServerEX.V5","Channel1.Device1.D1-Lat-1", 1.25)
let vtq5 = easyDAClient.ReadItem("", "Kepware.KEPServerEX.V5","Channel1.Device1.D1-Lat-1")
vtq5.Value

// Perform the OPC write

let windSpeed () =             
        (avgWindSpeed + rand.NextDouble() * 4. - 2.) 


let Sites = Array.init 10 (fun index -> getRandomGeoCoordinate index 47.643417 -122.126083 (20000.))
//Sites.[0].Latitude
let Speed = Array.init 10 (fun _ -> windSpeed())
let t1=["Channel1.Device1.D1-Lat-1";"Channel1.Device1.D1-Lat-2";"Channel1.Device1.D1-Lat-3"
        "Channel1.Device1.D1-Lat-4";"Channel1.Device1.D1-Lat-5";"Channel1.Device1.D1-Lat-6"
        "Channel1.Device1.D1-Lat-7";"Channel1.Device1.D1-Lat-8";"Channel1.Device1.D1-Lat-9";"Channel1.Device1.D1-Lat-10"
]
let t2=["Channel1.Device1.D1-Log-1";"Channel1.Device1.D1-Log-2";"Channel1.Device1.D1-Log-3"
        "Channel1.Device1.D1-Log-4";"Channel1.Device1.D1-Log-5";"Channel1.Device1.D1-Log-6"
        "Channel1.Device1.D1-Log-7";"Channel1.Device1.D1-Log-8";"Channel1.Device1.D1-Log-9";"Channel1.Device1.D1-Log-10"
]
let t3=["Channel1.Device1.D1-WS-1";"Channel1.Device1.D1-WS-2";"Channel1.Device1.D1-WS-3"
        "Channel1.Device1.D1-WS-4";"Channel1.Device1.D1-WS-5";"Channel1.Device1.D1-Ws-6"
        "Channel1.Device1.D1-WS-7";"Channel1.Device1.D1-WS-8";"Channel1.Device1.D1-Ws-9";"Channel1.Device1.D1-WS-10"
]

//let a1=DAItemValueArguments( ServerDescriptor("Kepware.KEPServerEX.V5"),DAItemDescriptor("Channel1.Device1.D1-Lat-1"),3.1)
let A1=[|for i in [0..9] -> 
            DAItemValueArguments(ServerDescriptor("Kepware.KEPServerEX.V5"),DAItemDescriptor(t1.[i]),Sites.[i].Latitude)
            //DAItemValueArguments("","Kepware.KEPServerEX.V5",t1.[i],Sites.[i].Latitude)
            |]
let A2=[|for i in [0..9] -> 
            DAItemValueArguments(ServerDescriptor("Kepware.KEPServerEX.V5"),DAItemDescriptor(t2.[i]),Sites.[i].Longitude)
            |]
let A3=[|for i in [0..9] -> 
            DAItemValueArguments(ServerDescriptor("Kepware.KEPServerEX.V5"),DAItemDescriptor(t3.[i]),Speed.[i])
            |]

//===========================================================

open System.Threading;

//Non-blocking Periodic Solution
let createTimer timeInterval=
    let timer = new System.Timers.Timer(float timeInterval)
    timer.AutoReset <- true
    let observable = timer.Elapsed  
    timer.Start()
    (timer,observable)


//Create & Start
//Check top Thread
printfn "Top Thread %d" Thread.CurrentThread.ManagedThreadId

let timer10s,obs10s = createTimer 10000

let timer3min,obs3min = createTimer (3*60000)

//Every 3 min
//Change location


let fun3min _=
            printfn "Every 3 min-tick %A" DateTime.Now
            let Sites = Array.init 10 (fun index -> getRandomGeoCoordinate index 47.643417 -122.126083 (20000.))
            let A1=[|for i in [0..9] -> 
                        DAItemValueArguments(ServerDescriptor("Kepware.KEPServerEX.V5"),DAItemDescriptor(t1.[i]),Sites.[i].Latitude)
                        |]
            let A2=[|for i in [0..9] -> 
                        DAItemValueArguments(ServerDescriptor("Kepware.KEPServerEX.V5"),DAItemDescriptor(t2.[i]),Sites.[i].Longitude)
                        |]

            let operationResults1 = easyDAClient.WriteMultipleItemValues(A1)
            let operationResults2 = easyDAClient.WriteMultipleItemValues(A2)
            ()
obs3min |> Observable.subscribe (fun3min)

//Every 10 secs
//Change Wind Speed
let fun10sec _=
            printfn "Every 10 sec-tick %A" DateTime.Now
            let Speed = Array.init 10 (fun _ -> windSpeed())

            let A3=[|for i in [0..9] -> 
                        DAItemValueArguments(ServerDescriptor("Kepware.KEPServerEX.V5"),DAItemDescriptor(t3.[i]),Speed.[i])
                        |]
            let operationResults1 = easyDAClient.WriteMultipleItemValues(A3)
            ()
obs10s |> Observable.subscribe (fun10sec)

//Stop
timer10s.Dispose()
timer3min.Dispose()

(*
easyDAClient.WriteItemValue("", "Kepware.KEPServerEX.V5","Channel1.Device1.D1-Lat-1", Sites.[0].Latitude)
easyDAClient.WriteItemValue("", "Kepware.KEPServerEX.V5","Channel1.Device1.D1-Lat-2", Sites.[1].Latitude)
easyDAClient.WriteItemValue("", "Kepware.KEPServerEX.V5","Channel1.Device1.D1-Lat-3", Sites.[2].Latitude)
*)
