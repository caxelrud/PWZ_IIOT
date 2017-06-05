open System
open System.Linq
open System.Text
open System.Threading
open System.Threading.Tasks
open FSharp.Configuration
open Microsoft.Azure.Devices
open Microsoft.Azure.Devices.Common

type Config = YamlConfig<FilePath="../config/config.yaml", ReadOnly=true>

[<EntryPoint>]
let main argv = 
    let config = Config()
    config.Load("../../../config/config.yaml")
    let connectionString = config.AzureIoTHubConfig.ConnectionString
    let deviceId = config.DeviceConfigs.First().DeviceId

    let serviceClient = ServiceClient.CreateFromConnectionString(config.AzureIoTHubConfig.ConnectionString);

    let cts = new CancellationTokenSource()

    let sendMessageToDevice (serviceClient:ServiceClient) (deviceId:string) (tokenSource:CancellationTokenSource) = 
        let rec task () = 
            let token = tokenSource.Token
            async {
                printfn "Type `exit` to terminate"
                let message = Console.ReadLine()  
                match message with 
                | "exit" -> 
                    tokenSource.Cancel()
                    return ()
                | _ -> 
                    if token.IsCancellationRequested then return ()
                    let commandMessage = new Message(Encoding.ASCII.GetBytes(message))
                    serviceClient.SendAsync(deviceId, commandMessage) |> Async.AwaitTask |> ignore
                    return! task ()
                }
        task ()

    let receiveFeedbackFromDevice (serviceClient:ServiceClient) (token:CancellationToken)  = 
        let feedbackReceiver = serviceClient.GetFeedbackReceiver()
        let rec task () = 
            async{
                if token.IsCancellationRequested then return ()
                let! ack = feedbackReceiver.ReceiveAsync(TimeSpan.FromSeconds(1.)) |> Async.AwaitTask
                match ack with
                | null -> return! task ()
                | _ -> 
                    printfn "%s" (ack.Records |> Seq.map (fun x -> x.StatusCode.ToString()) |> Seq.fold (+) "")
                    feedbackReceiver.CompleteAsync(ack) |> ignore
                    return! task ()
            }
        task ()

    let receiveFileNotification (token:CancellationToken) = 
        let rec task () = 
            async{
                if token.IsCancellationRequested then return ()
                return! task ()
            }
        task ()
    
    [sendMessageToDevice serviceClient deviceId cts; receiveFeedbackFromDevice serviceClient cts.Token; receiveFileNotification cts.Token]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    Console.ReadLine() |> ignore

    0 // return an integer exit code

