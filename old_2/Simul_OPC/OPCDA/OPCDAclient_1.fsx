

#I @"C:\Users\inter\OneDrive\Projects(Comp)\Dev_2017\IOT_2017\Simul_OPC\packages\OPC"

#r "OpcNetApi.dll"
#r "OpcNetApi.Com.dll"

open System
open Opc
open OpcCom


//5.2      Opc.IDiscovery
//5.2.1       EnumerateHosts Method
let se= new OpcCom.ServerEnumerator()

//5.2.2       GetAvailableServers Method
// Show Servers--------------------------------------------------
let servers=se.GetAvailableServers(Opc.Specification.COM_DA_30)
for i in servers do
    printfn "(%s) " i.Name;; 
(*(Kepware.KEPServerEX.V5)
(OPCLabs.KitServer)*)
//let vtq1 = easyDAClient.ReadItem("", "OPCLabs.KitServer.2","Simulation.Random")

//5.1      Opc.Factory
let mFactory = new OpcCom.Factory()
let mURL = new Opc.URL("opcda://localhost/OPCLabs.KitServer.2")
//let mURL = new Opc.URL("opcda://localhost/Kepware.KEPServerEX.V5")
//let mURL = new Opc.URL("opcda://localhost/ICONICS.SimulatorOPCDA")
//let mURL = new Opc.URL("opcda://localhost/Matrikon.OPC.Simulator.1");;

//5.3      Opc.Server
//5.3.1       Constructor
//5.3.2 Properties
//5.3.3 Duplicate Method
//5.3.4       Connect Method
//5.3.5       Disconnect Method

//5.4 Opc.Da.Server
//5.4.1 Constructor
let mserver = new Opc.Da.Server(mFactory, mURL)

//5.4.2 Properties
mserver;;

//5.4.3 Connect Method
let mCredentials = new System.Net.NetworkCredential()
let mConnectData = new Opc.ConnectData(mCredentials);
mserver.Connect(mURL, mConnectData)

// 4.1 Opc.IServer Interface
//4.1.3 GetSupportedLocale Method
    //let l=mserver.GetSupportedLocales
//4.1.2 SetLocale Method
    //mserver.SetLocale("en-US") 
//4.1.1 GetLocale Method
    //mserver.GetLocale()
    //mserver.GetErrorText
//4.7 Opc.Da.IServer Interface
//4.7.2 SetResultsFilters Method
mserver.SetResultFilters(0x09) //Minimal
//4.7.1 GetResultsFilters Method
mserver.GetResultFilters()

mserver.Name
mserver.GetType()

//4.7.3 GetStatus Method
let Sstat=mserver.GetStatus()
Sstat;;

//4.7.5 Read Method
    //4.3 Opc>Da.Item Class
let someItems : Opc.Da.Item array = Array.zeroCreate 3
someItems.[0] <- new Opc.Da.Item()
someItems.[0].ItemName <- "Channel1.Device1.Tag1"; 
someItems.[0].ClientHandle <- 0
someItems.[1] <- new Opc.Da.Item()
someItems.[1].ItemName <- "Channel1.Device1.Tag1"; 
someItems.[1].ClientHandle <- 1
someItems.[2] <- new Opc.Da.Item()
someItems.[2].ItemName <- "Simulation Examples.Functions.Sine1" 
someItems.[2].ClientHandle <- 2

let someItems1 : Opc.Da.Item array = Array.zeroCreate 1

someItems1.[0] <- new Opc.Da.Item()
someItems1.[0].ItemName <- "Simulation.Random"; 
someItems1.[0].ClientHandle <- 0


let r1=mserver.Read(someItems1)
let r2=mserver.Read([|someItems.[2]|]);;

r1

let AsyncOPCDARead(items:Opc.Da.Item[])=
    let dele=new Func<Opc.Da.Item[],Opc.Da.ItemValueResult[]>(mserver.Read)
    Async.FromBeginEnd(items,dele.BeginInvoke,dele.EndInvoke)

type Opc.Da.Server with
    member this.AsyncOPCDARead(items:Opc.Da.Item[])=
        let dele=new Func<Opc.Da.Item[],Opc.Da.ItemValueResult[]>(this.Read)
        Async.FromBeginEnd(items,dele.BeginInvoke,dele.EndInvoke)


async {
        let! r1=AsyncOPCDARead(someItems)
        printfn "%A" r1.[0].Value
      } |> Async.Start

async {
        let! r1=mserver.AsyncOPCDARead(someItems)
        printfn "%A" r1.[0].Value
      } |> Async.Start


//4.7.5 Write Method
let v1=new Opc.Da.ItemValue(ItemName="TAG_0000",Value=10.0)
let v2=new Opc.Da.ItemValue(ItemName="TAG_0001",Value=11.0)

mserver.Write([|v1;v2|])
mserver.Read([|someItems.[0];someItems.[1]|])

//4.7.6 CreateSubscription (Group)
let g1State = new Opc.Da.SubscriptionState()
g1State.Name<-"Group1"
g1State.ClientHandle<-1
g1State;;
let g1 = mserver.CreateSubscription(g1State)

printfn "Number of groups:%i" mserver.Subscriptions.Count
seq { for i in mserver.Subscriptions -> i.Name }|>Seq.iter (printfn "%s") 

//4.7.8 Browse Method
    //4.5 Opc.Da.BrowseElement Class
//4.7.9 Browse Next Method

//4.7.10  GetProperties Method
    //4.2 Opc.ItemIdentifier Class
let i0=Opc.ItemIdentifier("TAG_0000")
let i1=Opc.ItemIdentifier("TAG_0001")
let r=mserver.GetProperties([|i0;i1|],null,true)
printfn "DataType:%A" r.[0].[0].Value
printfn "Value: %A" r.[0].[1].Value
printfn "Quality: %A"  r.[0].[2].Value
printfn "Timestamp: %A"  r.[0].[3].Value;;

//4.8 Opc.Da.ISubscription
//4.8.1 DataChanged Event
//4.8.2 GetResultsFilter Method
//4.8.3 SetResultsFilter Method

//4.8.4 GetState Method
let r2=g1.GetState()
r2;;
let r2a=mserver.Subscriptions.Item(0).GetState()
r2a;;

//4.8.5 ModifyState Method

//4.8.6 AddItems Method
let r3=g1.AddItems(someItems)
let g1SH=Seq.toList(seq { for i in r3 -> i.ServerHandle })
seq { for i in r3 -> i.ResultID }|>Seq.iter (printfn "%A");;

//4.8.7 ModifyItems Method
//4.8.8 RemoveItems Method

//4.8.9 Read Method
let someItems_1 : Opc.Da.Item array = Array.zeroCreate 2
someItems_1.[0] <- new Opc.Da.Item()
someItems_1.[0].ServerHandle <- g1SH.[0]
someItems_1.[1] <- new Opc.Da.Item()
someItems_1.[1].ServerHandle <- g1SH.[1]

let r4=g1.Read(someItems_1)
//mserver.Subscriptions.Item(0).Read(someItems_1)

//Opc.Da.ItemProperty
printfn "Value: %A" r4.[0].Value
printfn "Quality: %A"  r4.[0].Quality
printfn "Timestamp: %A"  r4.[0].Timestamp;;

printfn "Value: %A" r4.[1].Value
printfn "Quality: %A"  r4.[1].Quality
printfn "Timestamp: %A"  r4.[1].Timestamp;;

//4.8.10  Write Method
    //4.4 Opc.Da.ItemValue Class
let someValues_1 : Opc.Da.ItemValue array = Array.zeroCreate 2

someValues_1.[0]<-new Opc.Da.ItemValue(ItemName="TAG_0000",Value=20.0)
someValues_1.[0].ServerHandle <- g1SH.[0]
someValues_1.[1]<-new Opc.Da.ItemValue(ItemName="TAG_0001",Value=21.0)
someValues_1.[1].ServerHandle <- g1SH.[1]

let r5=g1.Write(someValues_1)
seq { for i in r5 -> i.ResultID }|>Seq.iter (printfn "%A");;

//4.8.11 BeginReadMethod




//let dele1 = new Func<Opc.Da.Item[],Opc.Da.ItemValueResult[]>(fun (x:Opc.Da.Item[]) -> Opc.Da.ISubscription)

//Async.FromBeginEnd(someValues_1,Opc.Da.ReadAsyncDelegate.BeginInvoke,Opc.Da.ReadAsyncDelegate.EndInvoke)

//4.8.12 BeginWriteMethod
//4.8.13 CancelMethod
//4.8.14 Refresh Method
//4.8.15 SetEnable Method
//4.8.16 GetEnable Method

//5         Client API
//5.1.2 System Type Property
//5.1.3 UseRemoting Property
//5.1.4 CreateInstance Method
//5.4.5 CreateSubscription Method

//5.5 Opc.Da.Subscription
//5.5.1 Constructor
//5.5.2 Properties


//4.7.7 CancelSubscription
mserver.CancelSubscription(g1)

//5.4.3 Disconnect Method
mserver.Disconnect();;








//mserver.Subscriptions.Item(0).Read([|items.[0];items.[1]|])

//mserver.Subscriptions.Item(0).ServerHandle





//seq { for i in group1. -> i.ResultID }|>Seq.iter (printfn "%A")

(*
    // write items
    Opc.Da.ItemValue[] writeValues = new Opc.Da.ItemValue[3];
    writeValues[0] = new Opc.Da.ItemValue();
    writeValues[1] = new Opc.Da.ItemValue();
    writeValues[2] = new Opc.Da.ItemValue();
    writeValues[0].ServerHandle = group.Items[0].ServerHandle;
    writeValues[0].Value = 0;
    writeValues[1].ServerHandle = group.Items[1].ServerHandle;
    writeValues[1].Value = 0;
    writeValues[2].ServerHandle = group.Items[2].ServerHandle;
    writeValues[2].Value = 0;
    Opc.IRequest req;
    group.Write(writeValues, 321, new Opc.Da.WriteCompleteEventHandler(WriteCompleteCallback), out req);

    // and now read the items again
    group.Read(group.Items, 123, new Opc.Da.ReadCompleteEventHandler(ReadCompleteCallback), out req);
    Console.ReadLine();
}

//-------------------------------------------------------------------------------------------------
static void WriteCompleteCallback(object clientHandle, Opc.IdentifiedResult[] results)
{
    Console.WriteLine("Write completed");
    foreach (Opc.IdentifiedResult writeResult in results)
    {
        Console.WriteLine("\t{0} write result: {1}", writeResult.ItemName, writeResult.ResultID);
    }
    Console.WriteLine();
}
//-------------------------------------------------------------------------------------------------
static void ReadCompleteCallback(object clientHandle, Opc.Da.ItemValueResult[] results)
{
    Console.WriteLine("Read completed");
    foreach (Opc.Da.ItemValueResult readResult in results)
    {
        Console.WriteLine("\t{0}\tval:{1}", readResult.ItemName, readResult.Value);
    }
    Console.WriteLine();
}
//-------------------------------------------------------------------------------------------------

*)
