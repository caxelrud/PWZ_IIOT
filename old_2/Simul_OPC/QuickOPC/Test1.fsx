//OPClab - EasyOPC
//CA BUG: You need to be connected to the Internet to use the temporary license!

#r "OpcLabs.BaseLib.dll"
#r "OpcLabs.EasyOpcClassic.dll"
#r "OpcLabs.EasyOpcUA.dll"

//============================================
// Classic OPC

open System
open OpcLabs.EasyOpc
open OpcLabs.EasyOpc.DataAccess
open OpcLabs.EasyOpc.DataAccess.AddressSpace


//6.1.1.1 Reading from OPC Classic Items
//A single item
let easyDAClient = new EasyDAClient()
let vtq1 = easyDAClient.ReadItem("", "OPCLabs.KitServer.2","Simulation.Random")
vtq1;;
(*
 val it : DAVtq =
  0.00125125888851588 {System.Double} @2017-05-27T20:34:00.915; GoodNonspecific (192)
    {HasValue = true;
     Quality = GoodNonspecific (192);
     Timestamp = 5/27/2017 8:34:00 PM;
     TimestampLocal = 5/27/2017 3:34:00 PM;
     Value = 0.001251258889;
     ValueType = System.Double;}   
*)
let vtq2 = easyDAClient.ReadItem("", "OPCLabs.KitServer.2","Demo.Ramp")
vtq2;;

//Kepware
//"opcda://localhost/Kepware.KEPServerEX.V5","Channel1.Device1.Tag1"
let vtq3 = easyDAClient.ReadItem("", "Kepware.KEPServerEX.V5","Channel1.Device1.Tag1")
vtq3;;
(*
val it : DAVtq =
  36 {System.Int32} @2017-05-27T20:41:01.617; GoodNonspecific (192)
    {HasValue = true;
     Quality = GoodNonspecific (192);
     Timestamp = 5/27/2017 8:41:01 PM;
     TimestampLocal = 5/27/2017 3:41:01 PM;
     Value = 36;
     ValueType = System.Int32;}    
*)
let vtq4 = easyDAClient.ReadItem(
    new ServerDescriptor("", "Kepware.KEPServerEX.V5"),
    new DAItemDescriptor( "Channel1.Device1.Tag1")); //CA FIX

//Multiple items-OPCLabs
let p1=ServerDescriptor("", "OPCLabs.KitServer.2")
let p2=[|DAItemDescriptor("Simulation.Random"); 
        //DAItemDescriptor( "Trends.Ramp (1min)"); //CA BAD
        DAItemDescriptor( "Trends.Sine (1 min)"); 
        DAItemDescriptor("Simulation.Register_I4")|]
let vtqResults = easyDAClient.ReadMultipleItems(p1,p2)

//Multiple items-Kepware
let p1a=ServerDescriptor("", "Kepware.KEPServerEX.V5")

let p2a=[|DAItemDescriptor("Channel1.Device1.Tag1"); 
          DAItemDescriptor("Channel1.Device1.Tag2") |]
let vtqResults2 = easyDAClient.ReadMultipleItems(p1a,p2a)
(* val vtqResults2 : OperationModel.DAVtqResult [] =
  [|Success; 2646 {System.Int32} @2017-05-27T23:23:01.882; GoodNonspecific (192);
    Success; 0 {System.Int32} @2017-05-27T23:23:01.882; GoodNonspecific (192)|] *)

//Read parameters
//6.1.1.1.1 Reading just the value
//A single item
//6.1.1.2 Getting OPC Classic Property Values
//A single property
let p3=DAPropertyIds.Timestamp
let p4:DAPropertyId=null
let value = easyDAClient.GetPropertyValue("",
                                          "OPCLabs.KitServer.2", "Simulation.Random",
                                           DAPropertyId(DAPropertyIds.Timestamp)) 
(*val value : obj = 5/27/2017 7:29:45 PM*)

//Multiple properties
// This example shows how to obtain a data type of all OPC items under a branch.

open System.Linq
open OpcLabs.BaseLib.ComInterop
open OpcLabs.BaseLib.OperationModel
open OpcLabs.EasyOpc.DataAccess.AddressSpace

let serverDescriptor=ServerDescriptor("", "OPCLabs.KitServer.2")
let nodeDescriptor=DANodeDescriptor("")
let nodeElementCollection = easyDAClient.BrowseLeaves(serverDescriptor, DANodeDescriptor("Simulation"))
let q2=
        query { 
        for p in nodeElementCollection do
                where (not p.IsHint)
                select (DANodeDescriptor(p)) }
q2
|> Seq.iter (fun i -> printfn "Name: %s" i.ItemId)
(*Name: Simulation.AlternatingError (1 min)
Name: Simulation.Incrementing (1 min)
Name: Simulation.Ramp (10 s)
...
*)                
let nodeDescriptorArray=q2.ToArray()
nodeDescriptorArray.Count()
// Get the value of DataType property; it is a 16-bit signed integer
let valueResultArray =
        easyDAClient.GetMultiplePropertyValues(  serverDescriptor,
                                                nodeDescriptorArray, 
                                                DAPropertyDescriptor(DAPropertyId(DAPropertyIds.DataType)))
(*
val valueResultArray : ValueResult [] =
  [|Success; 3 {System.Int16}; Success; 3 {System.Int16};
    Success; 5 {System.Int16}; Success; 8200 {System.Int16};
    Success; 8195 {System.Int16}; Success; 8196 {System.Int16};
    ...|]
*)
// Check if there has been an error getting the property value
for i in valueResultArray do
    if not (isNull i.Exception) then
        printfn "%A" i.Exception.Message
    //let varType =  VarType(i.Value :?>int)  
    //printfn "%A" varType 

//6.1.2 Modifying Information
//6.1.2.1 Writing to OPC Classic Items
// This example shows how to write a value into a single item.
easyDAClient.WriteItemValue("", "OPCLabs.KitServer.2","Simulation.Register_I4", 12345)
let vtq5 = easyDAClient.ReadItem("", "OPCLabs.KitServer.2","Simulation.Register_I4")
(* val vtq5 : DAVtq =
  12345 {System.Int32} @2017-05-28T03:57:21.432; GoodNonspecific (192) *)

//6.1.2.1.1 Writing value, timestamp and quality
//A single item
//Multiple items

//6.1.3 Browsing for Information
//6.1.3.2 Browsing for OPC Classic Nodes (Branches and Leaves)
// This example shows how to recursively browse the nodes in the OPC address space.

let rec BrowseFromNode(client:EasyDAClient,serverDescriptor:ServerDescriptor,parentNodeDescriptor:DANodeDescriptor,branchCount:int byref,leafCount:int byref)=
    // Obtain all node elements under parentNodeDescriptor
    let browseParameters = new DABrowseParameters()    // no filtering whatsoever
    let nodeElementCollection = 
                    client.BrowseNodes(serverDescriptor, parentNodeDescriptor, browseParameters)
    // Remark: that BrowseNodes(...) may also throw OpcException; a production code should contain handling for 
    // it, here omitted for brevity.
    for nodeElement in nodeElementCollection do
        printfn "%A" nodeElement
        // If the node is a branch, browse recursively into it.
        if nodeElement.IsBranch then
            branchCount<-branchCount+1
            BrowseFromNode(client, serverDescriptor, DANodeDescriptor(nodeElement),&branchCount,&leafCount)
            ()
        else
            leafCount<-leafCount+1
            ()

let mutable _branchCount=0
let mutable _leafCount=0

BrowseFromNode(easyDAClient, serverDescriptor,nodeDescriptor,&_branchCount,&_leafCount);
_branchCount,_leafCount
(*val it : int * int = (236, 1619)*)

//6.1.3.3 Browsing for OPC Classic Access Paths
//6.1.3.4 Browsing for OPC Classic Properties

//6.1.4 Subscribing to Information
//6.1.4.1 Subscribing to OPC Classic Items
//A single item
//Multiple items
// This example shows how subscribe to changes of multiple items and display the value of the item with each change.

//using JetBrains.Annotations;
open System.Threading
open OpcLabs.EasyOpc.DataAccess.OperationModel //CA FIX

//let easyDAClient_ItemChanged( e:EasyDAItemChangedEventArgs)=    //sender:obj,
//    printfn "%A: %A" e.Arguments.ItemDescriptor.ItemId e.Vtq 

let f1 (sender:obj) (e:EasyDAItemChangedEventArgs) =    
    printfn "%A: %A" e.Arguments.ItemDescriptor.ItemId e.Vtq 


let d1=new EasyDAItemChangedEventHandler(f1) 

//easyDAClient.ItemChanged.Add(easyDAClient_ItemChanged)
easyDAClient.ItemChanged.AddHandler(d1)

easyDAClient.SubscribeMultipleItems(
        [|DAItemGroupArguments("", "OPCLabs.KitServer.2","Simulation.Random", 15000, null);
          DAItemGroupArguments("", "OPCLabs.KitServer.2","Trends.Ramp (1 min)", 15000, null);
          DAItemGroupArguments("", "OPCLabs.KitServer.2","Trends.Sine (1 min)", 15000, null);
          DAItemGroupArguments("", "OPCLabs.KitServer.2","Simulation.Register_I4", 15000, null)|] //CA Register_I4 bad
          )
printfn "Waiting for 10 seconds..."
//Thread.Sleep(45 * 1000)
easyDAClient.ItemChanged.RemoveHandler(d1)

//6.1.4.3 Changing Existing Subscription
//A single subscription
// This example shows how change the update rate of an existing subscription.
// Item changed event handler
let f2 (sender:obj) (e:EasyDAItemChangedEventArgs) =    
    printfn "%A" e.Vtq 
let d2=new EasyDAItemChangedEventHandler(f2) 
//easyDAClient.ItemChanged.Add(easyDAClient_ItemChanged)
easyDAClient.ItemChanged.AddHandler(d2)
//Subscribing
let handle = easyDAClient.SubscribeItem("", "OPCLabs.KitServer.2","Simulation.Random", 1000)
printfn "Waiting for 10 seconds..."
//Thread.Sleep(10 * 1000)
printfn "Changing subscription..."
easyDAClient.ChangeItemSubscription(handle, DAGroupParameters(100))
printfn "Waiting for 10 seconds..."
//Thread.Sleep(10 * 1000)
printfn "Unsubscribing..."
easyDAClient.UnsubscribeAllItems()
printfn "Waiting for 10 seconds..."
//Thread.Sleep(10 * 1000)

//Multiple subscriptions
// This example shows how subscribe to changes of multiple items, and unsubscribe from one of them.
easyDAClient.ItemChanged.AddHandler(d2)
let handleArray =easyDAClient.SubscribeMultipleItems(
                    [|DAItemGroupArguments("", "OPCLabs.KitServer.2","Simulation.Random", 15000, null);
                      DAItemGroupArguments("", "OPCLabs.KitServer.2","Trends.Ramp (1 min)", 15000, null);
                      DAItemGroupArguments("", "OPCLabs.KitServer.2","Trends.Sine (1 min)", 15000, null);
                      DAItemGroupArguments("", "OPCLabs.KitServer.2","Simulation.Register_I4", 15000, null)|] //CA Register_I4 bad
                    )
//Processing item changed events
//Thread.Sleep(30 * 1000);
//Unsubscribing from the first item
easyDAClient.UnsubscribeItem(handleArray.[0])
//Processing item changed events
easyDAClient.UnsubscribeAllItems()

//From multiple items

//From all items

//Implicit and explicit unsubscribe

//6.1.4.6 OPC Classic Item Changed Event or Callback
//6.1.4.8 Using Callback Methods Instead of Event Handlers
// This example shows how subscribe to changes of multiple items and display the value of the item with each change,
// using a callback method specified using lambda expression.
// Instantiate the client object
//Subscribing
// The callback is a lambda expression the displays the value

easyDAClient.SubscribeItem("", "OPCLabs.KitServer.2","Simulation.Random", 1000,
                    fun (sender:obj, eventArgs:EasyDAItemChangedEventArgs) ->
                        //Debug.Assert(eventArgs != null);
                        if not (isNull eventArgs.Exception) then
                            printfn "%A" (eventArgs.Exception.ToString())
                        else
                            //Debug.Assert(eventArgs.Vtq != null)
                            printfn "%A" (eventArgs.Vtq.ToString())
                    )
//Processing item changed events for some time ...
//Unsubscribing
easyDAClient.UnsubscribeAllItems()

//6.1.6 Setting Parameters
// This example shows how the OPC server can quickly be disconnected after writing a value into one of its OPC items. ??
easyDAClient.InstanceParameters.HoldPeriods.TopicWrite <- 100 // in milliseconds
easyDAClient.WriteItemValue("", "OPCLabs.KitServer.2","Simulation.Register_I4", 12345)

//6.2 Procedural Coding Model for OPC Classic A&E
//6.2.1 Obtaining Information
//6.2.1.1 Getting Condition State

// This example shows how to obtain current state information for the condition instance corresponding to a Source and
// certain ConditionName.

open OpcLabs.EasyOpc.AlarmsAndEvents

let easyAEClient = new EasyAEClient()
let conditionState = easyAEClient.GetConditionState("","OPCLabs.KitEventServer.2","Simulation.ConditionState1", "Simulated")
conditionState.ActiveSubcondition;;
conditionState.Enabled;;
conditionState.Active;;
conditionState.Acknowledged;;
conditionState.Quality;;

//6.2.2 Modifying Information
//6.2.2.1 Acknowledging a Condition

//7 Live Mapping Model
//7.1 Live Mapping Model for OPC Data
//7.1.1 Live Mapping Example
//Define the mapping
open OpcLabs.BaseLib.LiveMapping
open OpcLabs.EasyOpc.DataAccess.LiveMapping
open OpcLabs.EasyOpc.DataAccess.LiveMapping.Extensions
// The Boiler and its constituents are described in our application
// domain terms, the way we want to work with them.
// Attributes are used to describe the correspondence between our
// types and members, and OPC nodes.
// This is how the boiler looks in OPC address space:
// - Boiler #1
//      - CC1001 (CustomController)
//          - ControlOut
//          - Description
//          - Input1
//          - Input2
//          - Input3
//      - Drum1001 (BoilerDrum)
//          - LIX001 (LevelIndicator)
//              - Output
//      - FC1001 (FlowController)
//          - ControlOut
//          - Measurement
//          - SetPoint
//      - LC1001 (LevelController)
//          - ControlOut
//          - Measurement
//          - SetPoint
//      - Pipe1001 (BoilerInputPipe)
//          - FTX001 (FlowTransmitter)
//          - Output
//      - Pipe1002 (BoilerOutputPipe)
//          - FTX002 (FlowTransmitter)
//          - Output

[<DAType>]
type GenericActuator()=
    [<DANode; DAItem>]
    member val Input = 0.0 with get, set
    
[<DAType>]
type GenericSensor()= 
    // Meta-members are filled in by information collected during
    // mapping, and allow access to it later from your code.
    // Alternatively, you can derive your class from DAMappedNode,
    // which will bring in many meta-members automatically.
        // member defined with type declaration
    [<MetaMember("NodeDescriptor")>]
    member val NodeDescriptor:DANodeDescriptor=null with get, set
    [<DANode; DAItem(Operations = DAItemMappingOperations.ReadAndSubscribe)>] // no OPC writing
    member val Output=0.0 with get,set
    

[<DAType>]
type GenericController()=
    [<DANode; DAItem(Operations = DAItemMappingOperations.ReadAndSubscribe)>] // no OPC writing
    member val Measurement=0.0 with get,set 
    [<DANode; DAItem>]
    member val SetPoint=0.0 with get,set
    [<DANode; DAItem(Operations = DAItemMappingOperations.ReadAndSubscribe)>] // no OPC writing
    member val ControlOut=0.0 with get,set

[<DAType>]
type Valve()=
    inherit GenericActuator()

[<DAType>]
type FlowTransmitter()=
    inherit GenericSensor()

[<DAType>]
type LevelIndicator()=
    inherit GenericSensor()

[<DAType>]
[<CLIMutable>]
type CustomController={
    [<DANode; DAItem>]
    Input1:double;
    [<DANode; DAItem>]
    Input2:double; 
    [<DANode; DAItem>]
    Input3:double; 
    [<DANode; DAItem(Operations = DAItemMappingOperations.ReadAndSubscribe)>] // no OPC writing
    ControlOut:double; 
    [<DANode; DAItem>]
    Description:string 
    }

[<DAType>]
type FlowController()=
    inherit GenericController()

[<DAType>]
type LevelController()= 
    inherit GenericController()

[<DAType>]
[<CLIMutable>]
type BoilerOutputPipe= {
    // Specifying BrowsePath-s here only because we have named the
    // class members differently from OPC node names.
    [<DANode(BrowsePath = "FTX002")>]
    FlowTransmitter2:FlowTransmitter
    }


[<DAType>]
type BoilerDrum={
    // Specifying BrowsePath-s here only because we have named the
    // class members differently from OPC node names.
    [<DANode(BrowsePath = "LIX001")>]
     LevelIndicator:LevelIndicator
    }



(*
{
{
[DAType]
class BoilerInputPipe
{
// Specifying BrowsePath-s here only because we have named the
// class members differently from OPC node names.
[DANode(BrowsePath = "FTX001")]
public FlowTransmitter FlowTransmitter1 = new FlowTransmitter();
[DANode(BrowsePath = "ValveX001")]
public Valve Valve = new Valve();
}

// Specifying BrowsePath-s here only because we have named the
// class members differently from OPC node names.
[DANode(BrowsePath = "Pipe1001")]
public BoilerInputPipe InputPipe = new BoilerInputPipe();
[DANode(BrowsePath = "Drum1001")]
public BoilerDrum Drum = new BoilerDrum();
[DANode(BrowsePath = "Pipe1002")]
public BoilerOutputPipe OutputPipe = new BoilerOutputPipe();
[DANode(BrowsePath = "FC1001")]
public FlowController FlowController = new FlowController();
[DANode(BrowsePath = "LC1001")]
public LevelController LevelController = new LevelController();
[DANode(BrowsePath = "CC1001")]
public CustomController CustomController = new CustomController();
}


*)

//8 Live Binding Model
//8.1 Live Binding Model for OPC Data

//9 Reactive Programming Model
//10 User Interface



//11 EasyOPC Extensions for .NET
//11.1 Usage
//11.1.1 Generic Types
//11.2.1 Generic Types for OPC-DA
(*
DAVtq<T>: Holds typed data value (of type T), timestamp, and quality.
DAVtqResult<T>: Holds result of an operation in form of a typed DAVtq<T> (value, timestamp, quality).
DAItemValueArguments<T>: Arguments for an operation on an OPC item. Carries a value of type T.
DAItemVtqArguments<T>: Arguments for an operation on an OPC item. Carries a DAVtq<T> object.
EasyDAItemSubscriptionArguments<T>: Arguments for subscribing to an OPC item of type T.
EasyDAItemChangedEventArgs<T>: Data of an event or callback for a significant change in OPC item of type T.    
*)
//11.2.3 Extensions for OPC Properties
//11.2.3.1 Type-safe Access
// This example shows how to obtain a data type of an OPC item.
open OpcLabs.BaseLib.ComInterop
open OpcLabs.EasyOpc.DataAccess.Extensions
// Get the DataType property value, already converted to VarType
let varType = easyDAClient.GetDataTypePropertyValue("","OPCLabs.KitServer.2", "Simulation.Random")
(* val varType : VarType = R8 *)
//11.2.3.4 Alternate Access Methods
// This example shows how to obtain a dictionary of OPC property values for an OPC
//item, and extract property values.
// Get dictionary of property values, for all well-known properties
let propertyValueDictionary=easyDAClient.GetPropertyValueDictionary("","OPCLabs.KitServer.2", "Simulation.Random")
// Display some of the obtained property values
// The production code should also check for the .Exception first, before getting .Value
(*
     (305(LimitExceeded),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (306(Deadband),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (307(HighHighLimit),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (308(HighLimit),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (309(LowLimit),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (310(LowLowLimit),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (311(ChangeRateLimit),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (312(DeviationLimit),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.);
     (313(SoundFile),
      *** Failure -1073479165 (0xC0040203): The server does not recognize the passed property ID.)]    
*)
propertyValueDictionary.[DAPropertyId(DAPropertyIds.AccessRights)].Value;;
propertyValueDictionary.[DAPropertyId(DAPropertyIds.DataType)].Value;;
(*val it : obj = 5s*)
propertyValueDictionary.[DAPropertyId(DAPropertyIds.Timestamp)].Value;;
// This example shows how to obtain a structure containing property values for an
// OPC item, and display some property values.
// Get a structure containing values of all well-known properties
let itemPropertyRecord =easyDAClient.GetItemPropertyRecord("", "OPCLabs.KitServer.2","Simulation.Random")
itemPropertyRecord.AccessRights
itemPropertyRecord.DataType
itemPropertyRecord.Timestamp

(*
*)


//===============================================================
//OPC-UA
//===============================================================
//Test 1 (ok)
open OpcLabs.EasyOpc.UA
let client = new EasyUAClient()
#time
let value = 
    client.ReadValue(
        new UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"), 
        new UANodeDescriptor("nsu=http://test.org/UA/Data/;i=10853"))
#time

//OpcLabs.EasyOpc.UA.OperationModel.UAException: An OPC-UA operation failure with error code -1 (0xFFFFFFFF) occurred, originating from 'System'. The inner exception, of type 'System.ServiceModel.EndpointNotFoundException', contains details about the problem. ---> System.ServiceModel.EndpointNotFoundException: There was no endpoint listening at http://opcua.demo-this.com:51211/UA/SampleServer/discovery that could accept the message. This is often caused by an incorrect address or SOAP action. See InnerException, if present, for more details.
// ---> System.Net.WebException: The remote name could not be resolved: 'opcua.demo-this.com'

//Test 2 (ok)
open OpcLabs.EasyOpc.UA.OperationModel
open System
open System.Threading
let handle = 
        client.SubscribeDataChange(
            new UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"), // or "opc.tcp://opcua.demo-this.com:51210/UA/SampleServer"
            new UANodeDescriptor("nsu=http://test.org/UA/Data/;i=10853"),
            1000,
            new EasyUADataChangeNotificationEventHandler(
                fun sender eventArgs -> Console.WriteLine(eventArgs.AttributeData.Value:obj)))
            // Remark: Production code would check eventArgs.Exception before accessing eventArgs.AttributeData.
//wait...
client.UnsubscribeAllMonitoredItems()

//Test 3 (OK)
//Examples for OPC UA (Data)
// This example shows how to subscribe to changes of a single monitored item, pull events, and display each change.
open OpcLabs.EasyOpc.UA
open OpcLabs.EasyOpc.UA.OperationModel
open System

//namespace UADocExamples
//namespace _EasyUAClient
//PullDataChangeNotification
// Instantiate the client object
// In order to use event pull, you must set a non-zero queue capacity upfront.
let easyUAClient = new EasyUAClient(PullDataChangeNotificationQueueCapacity = 1000) 

printfn("Subscribing...")
easyUAClient.SubscribeDataChange(
                    UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),   
                    // or "opc.tcp://opcua.demo-this.com:51210/UA/SampleServer"
                    UANodeDescriptor("nsu=http://test.org/UA/Data/;i=10853"),
                    1000)

printfn("Processing data change events for 1 minute...");
//Environment.TickCount/1000/60/60 //hours
let endTick = Environment.TickCount + 60 * 1000

while (Environment.TickCount < endTick) do
    let eventArgs = easyUAClient.PullDataChangeNotification(2 * 1000)
    if not(isNull(eventArgs))  then //<> null)
        // Handle the notification event
        printfn "%A" eventArgs

//Test 4 (OK)
// This example shows how to read a range of values from an array.
open OpcLabs.EasyOpc.UA
open System
open OpcLabs.EasyOpc.UA.OperationModel
//namespace UADocExamples
//namespace _UAIndexRangeList
//Usage
let ReadValue()=
    // Instantiate the client object
    let easyUAClient = new EasyUAClient()
    // Obtain the value, indicating that just the elements 2 to 4 should be returned
    let value = easyUAClient.ReadValue(
                    UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                        UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10305"),
                        UAIndexRangeList.OneDimension(2, 4)))
                        // or "opc.tcp://opcua.demo-this.com:51210/UA/SampleServer"
    // Cast to typed array
    let arrayValue = value:?>int32[]
    // Display results
    for i in [0..2] do 
        printfn "arrayValue[%d]: {%d}" i  arrayValue.[i]

ReadValue();;

//Test 5 (OK)
//QuickOPC User's Guide page 122
(* This example shows how to read data (value, timestamps, and status code) of 3
attributes at once. In this example,
 we are reading a Value attribute of 3 different nodes, but the method can also be
used to read multiple attributes
 of the same node. *)

open OpcLabs.EasyOpc.UA
open System
open OpcLabs.EasyOpc.UA.OperationModel

let ReadMultiple()=
    let easyUAClient = new EasyUAClient()
    //easyUAClient.ReadMultiple
    // Obtain attribute data. By default, the Value attributes of the nodes will be read.
    let v1=[|UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10305"));
             UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10305"));
             UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10305"))|]

    let attributeDataResultArray = easyUAClient.ReadMultiple(v1)
    // Display results
    for attributeDataResult in attributeDataResultArray do
        printfn "AttributeData: %A" attributeDataResult.AttributeData
                              
ReadMultiple();;

//Test 6 ()
//QuickOPC User's Guide page 129
// This example shows how to read value of a single node, and display it.

open OpcLabs.EasyOpc.UA
open System
open OpcLabs.EasyOpc.UA.OperationModel

let ReadValue1()=
    let easyUAClient = new EasyUAClient()
    let v1=UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;i=10853"))
    let value = easyUAClient.ReadValue(v1)
    // Display results
    printfn "value: %A" value
                              
ReadValue1();;

//Test 7 (OK)---------------------------------------------------
//QuickOPC User's Guide page 132
(*
This example shows how to read the Value attributes of 3 different nodes at once.
Using the same method, it is also possible to read multiple attributes of the same node.    
*)
open OpcLabs.EasyOpc.UA
open System
open OpcLabs.EasyOpc.UA.OperationModel

let ReadMultipleValues()=
    let easyUAClient = new EasyUAClient()
    //easyUAClient.ReadMultiple
    // Obtain attribute data. By default, the Value attributes of the nodes will be read.
    let v1=[|UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10845"));
             UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10853"));
             UAReadArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10855"))|]

    let valueResultArray = easyUAClient.ReadMultipleValues(v1)
    // Display results
    for valueResult in valueResultArray do
        printfn "Value: %A" valueResult.Value
                              
ReadMultipleValues();;

//6.1.2 Modifying Information
//6.1.2.2 Writing Attributes of OPC UA Nodes

//Test (OK)-----------------------------------------------------
//QuickOPC User's Guide page 143
// This example shows how to write a value into a single node.
open System
open OpcLabs.EasyOpc.UA
let WriteValue()=
    let easyUAClient = new EasyUAClient()
    // Modify value of a node
    easyUAClient.WriteValue(
        UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"), 
        UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10221"),12345)

WriteValue();;

//Test(OK)-------------------------------------------------------
//Multiple nodes or attributes

// This example shows how to write values into 3 nodes at once.
//QuickOPC User's Guide page 146
open System
open OpcLabs.EasyOpc.UA
open OpcLabs.EasyOpc.UA.OperationModel

let WriteMultipleValues()=
    let easyUAClient = new EasyUAClient()
    let v1=[|UAWriteValueArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10221"),23456);
             UAWriteValueArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10226"),2.34567890);
             UAWriteValueArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10227"),"ABC")|]
    let valueResultArray = easyUAClient.WriteMultipleValues(v1)
    ()                              

WriteMultipleValues();;

//Test(OK)-------------------------------------------------------
//QuickOPC User's Guide page 147
// This example shows how to write values into 3 nodes at once, test for success of
//  each write and display the exception message in case of failure.
open System
open OpcLabs.EasyOpc.UA
open OpcLabs.EasyOpc.UA.OperationModel

let WriteMultipleValues1()=
    let easyUAClient = new EasyUAClient()
    let v1=[|UAWriteValueArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10221"),23456);
             UAWriteValueArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10226"),"This string
                                                    cannot be converted to Double");
             UAWriteValueArguments(UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;s=UnknownNode"),"ABC")|]
    let operationResultArray = easyUAClient.WriteMultipleValues(v1)
    for i in operationResultArray do // = 0; i < operationResultArray.Length; i++)
    if i.Succeeded then
        printfn "Result %A: success" i
    else
        printfn "Result %A: %A" i (i.Exception.GetBaseException().Message)

WriteMultipleValues1()
(*
Result Success: success
Result *** Failure -1 (0xFFFFFFFF): Input string was not in a correct format.: "Input string was not in a correct format.
+ Attempting to change an object of type 'System.String' to type 'System.Double'."
Result *** Failure -1 (0xFFFFFFFF): The OPC-UA server has returned a status for the DataType attribute that is not Good. The
returned status is 'BadNodeIdUnknown'.: "The OPC-UA server has returned a status for the DataType attribute that is not Good.
 The returned status is 'BadNodeIdUnknown'."
*)

//Data Type in OPC UA Write
//Writing value, timestamps and status code
//A single node and attribute

//Test(??)-------------------------------------------------------
//QuickOPC User's Guide page 153
// This example shows how to write data (a value, timestamps and status code) into a single attribute of a node.
open System
open OpcLabs.EasyOpc.UA
open OpcLabs.EasyOpc.UA.OperationModel

let Write()=
    let easyUAClient = new EasyUAClient()
    try
        // Modify data of a node's attribute
        easyUAClient.Write(
            UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"), 
            UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10221"),
            UAAttributeData(12345, UAStatusCode(UASeverity.GoodOrSuccess),DateTime.UtcNow))
        // Writing server timestamp is not supported by the sample server.
        // The UA Test Server does not support this, and therefore a failure
        //will occur.
    with
        | :? UAException as uaException -> 
            printfn "Failure: %A" (uaException.GetBaseException().Message)

Write()

//6.1.3 Browsing for Information
//6.1.3.5 Discovering OPC UA Servers
//6.1.3.5.1 OPC UA Local Discovery

//Test(OK)-------------------------------------------------------
//QuickOPC User's Guide page 167
// This example shows how to obtain application URLs of all OPC Unified Architecture
//servers on a given machine.
open System
open OpcLabs.EasyOpc.UA

let DiscoverServers()=
    let easyUAClient = new EasyUAClient()
    // Obtain collection of server elements
    let applicationElementCollection = easyUAClient.DiscoverServers("opcua.demo-this.com")
    // Display results
    for i in applicationElementCollection do
        printfn "applicationElementCollection[%A].ApplicationUriString: %A"
            i.DiscoveryUriString i.ApplicationUriString

DiscoverServers()
(*
applicationElementCollection["opc.tcp://opcua.demo-this.com:51210/UA/SampleServer"].ApplicationUriString: "urn:opcua.demo-thi
s.com:UA Sample Server"
applicationElementCollection["http://opcua.demo-this.com:51211/UA/SampleServer"].ApplicationUriString: "urn:opcua.demo-this.c
om:UA Sample Server"
applicationElementCollection["https://opcua.demo-this.com:51212/UA/SampleServer/"].ApplicationUriString: "urn:opcua.demo-this
.com:UA Sample Server"
applicationElementCollection["http://opcua.demo-this.com:62543/Quickstarts/AlarmConditionServer"].ApplicationUriString: "urn:
opcua.demo-this.com:Quickstart Alarm Condition Server"
applicationElementCollection["opc.tcp://opcua.demo-this.com:62544/Quickstarts/AlarmConditionServer"].ApplicationUriString: "u
rn:opcua.demo-this.com:Quickstart Alarm Condition Server"
*)

//6.1.3.5.2 OPC UA Global Discovery
//6.1.3.5.3 Generalized OPC UA Discovery
//6.1.3.6 Browsing for OPC UA Nodes

//Test(OK)-------------------------------------------------------
//QuickOPC User's Guide page 174
// This example shows how to obtain "data nodes" (objects, variables and properties)
// under the "Objects" node in the address
// space.

open System
open OpcLabs.EasyOpc.UA

let BrowseDataNodes()=
    let easyUAClient = new EasyUAClient()
    // Obtain data nodes under "Objects" node
    let nodeElementCollection = easyUAClient.BrowseDataNodes(
                                    UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"))
    // Display results
    for i in nodeElementCollection do
        printfn "nodeElement.NodeId: %A" i.NodeId
        printfn "nodeElement.DisplayName: %A" i.DisplayName

BrowseDataNodes()
(*
nodeElement.NodeId: Server
nodeElement.DisplayName: "Server"
nodeElement.NodeId: nsu=http://test.org/UA/Data/;ns=2;i=10157
nodeElement.DisplayName: "Data"
nodeElement.NodeId: nsu=http://samples.org/UA/memorybuffer;ns=7;i=1025
nodeElement.DisplayName: "MemoryBuffers"
nodeElement.NodeId: nsu=http://opcfoundation.org/UA/Boiler/;ns=4;i=1240
nodeElement.DisplayName: "Boilers"
*)

//6.1.4 Subscribing to Information
//6.1.4.1 Subscribing to OPC Classic Items
//6.1.4.2 Subscribing to OPC UA Monitored Items
//A single node and attribute


//Test(OK)-------------------------------------------------------
//QuickOPC User's Guide page 187
// This example shows how to subscribe to changes of a single monitored item and
//display the value of the item with each change.

open System
open OpcLabs.EasyOpc.UA
open OpcLabs.EasyOpc.UA.OperationModel

let SubscribeDataChange=
    async {
    // Instantiate the client object and hook events
    let easyUAClient = new EasyUAClient()
    let easyUAClient_DataChangeNotification(e:EasyUADataChangeNotificationEventArgs )=
        // Display value
        // Remark: Production code would check e.Exception before accessing e.AttributeData.Console.
        printfn "Value: %A" e.AttributeData.Value
    easyUAClient.DataChangeNotification.Add(easyUAClient_DataChangeNotification)
    printfn "Subscribing..."
    easyUAClient.SubscribeDataChange(
                UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"), 
            UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10853"), 1000) |>ignore
    printfn "Processing data change events for 20 seconds..."
    //System.Threading.Thread.Sleep(20 * 1000)
    do! Async.Sleep (20*1000)
    printfn "Unsubscribing..."
    easyUAClient.UnsubscribeAllMonitoredItems()
    printfn "Waiting for 5 seconds..."
    //System.Threading.Thread.Sleep(5 * 1000)
    do! Async.Sleep (5*1000)
    printfn "Done"
    }

Async.Start SubscribeDataChange

(*> Subscribing...
Processing data change events for 20 seconds...
Value: -3.10923192e-35f
Value: -2.33415454e-35f
Value: 2.06627727e-28f
Value: 1.0413831e-13f
Value: -2.68463333e+32f
Value: -3.95644746e-24f
Value: -2.60344807e+38f
Value: -3.5518296e+19f
Value: 3.84925266e-28f
Unsubscribing...
Waiting for 5 seconds...
Done*)

//Multiple nodes or attributes

//Test(??)-------------------------------------------------------
//QuickOPC User's Guide page 192
// This example shows how to subscribe to changes of multiple monitored items and
//display the value of the monitored item with each change.
open System
open OpcLabs.EasyOpc.UA
open OpcLabs.EasyOpc.UA.OperationModel

let SubscribeMultipleMonitoredItems=
    async {
    // Instantiate the client object and hook events
    let easyUAClient = new EasyUAClient()
    let easyUAClient_DataChangeNotification(e:EasyUADataChangeNotificationEventArgs )=
        // Display value
        // Remark: Production code would check e.Exception before accessing e.AttributeData.Console.
        printfn "Node:%A Value:%A"  e.Arguments.NodeDescriptor e.AttributeData.Value

    easyUAClient.DataChangeNotification.Add(easyUAClient_DataChangeNotification)
    printfn "Subscribing..."

    let v1=[|EasyUAMonitoredItemArguments(null,UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10845"));
             EasyUAMonitoredItemArguments(null,UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10853"));
             EasyUAMonitoredItemArguments(null,UAEndpointDescriptor("http://opcua.demo-this.com:51211/UA/SampleServer"),
                          UANodeDescriptor("nsu=http://test.org/UA/Data/;ns=2;i=10855"))|] 
    easyUAClient.SubscribeMultipleMonitoredItems(v1) |>ignore
    printfn "Processing data change events for 10 seconds..."
    //System.Threading.Thread.Sleep(10 * 1000)
    do! Async.Sleep (10*1000)
    printfn "Unsubscribing..."
    easyUAClient.UnsubscribeAllMonitoredItems()
    printfn "Waiting for 5 seconds..."
    //System.Threading.Thread.Sleep(5 * 1000)
    do! Async.Sleep (5*1000)
    printfn "Done"
    }

Async.Start SubscribeMultipleMonitoredItems

//6.1.4.3 Changing Existing Subscription
//A single subscription

//Test(??)-------------------------------------------------------
//QuickOPC User's Guide page 206
//// This example shows how change the sampling rate of multiple existing monitored item
//subscriptions. using OpcLabs.EasyOpc

//Test(??)-------------------------------------------------------
//QuickOPC User's Guide page 214
// This example shows how to unsubscribe from changes of multiple items.

//Test(??)-------------------------------------------------------
//QuickOPC User's Guide page 218
// This example shows how to unsubscribe from changes of all items.



