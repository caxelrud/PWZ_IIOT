 #r @"..\packages\FSharp.Configuration.0.6.2\lib\net40\FSharp.Configuration.dll" 
 #r @"FSharp.Core.dll" 
 #r @"..\packages\Microsoft.Azure.Amqp.1.1.5\lib\net451\Microsoft.Azure.Amqp.dll" 
 #r @"..\packages\Microsoft.Azure.Devices.1.0.14\lib\net451\Microsoft.Azure.Devices.dll" 
 #r @"..\packages\Mono.Security.3.2.3.0\lib\net45\Mono.Security.dll" 
 #r @"mscorlib.dll" 
 #r @"Newtonsoft.Json.dll" 
 #r @"..\packages\PCLCrypto.2.0.147\lib\portable-net45+win+wpa81+wp80+MonoAndroid10+xamarinios10+MonoTouch10\PCLCrypto.dll" 
 #r @"..\packages\PInvoke.BCrypt.0.3.90\lib\net40\PInvoke.BCrypt.dll" 
 #r @"..\packages\PInvoke.Kernel32.0.3.90\lib\net40\PInvoke.Kernel32.dll" 
 #r @"..\packages\PInvoke.NCrypt.0.3.90\lib\net40\PInvoke.NCrypt.dll" 
 #r @"..\packages\PInvoke.Windows.Core.0.3.90\lib\portable-net45+win+wpa81+MonoAndroid10+xamarinios10+MonoTouch10\PInvoke.Windows.Core.dll" 
 #r @"..\packages\FSharp.Configuration.0.6.2\lib\net40\SharpYaml.dll" 
 #r @"System.dll" 
 #r @"System.Core.dll" 
 #r @"System.Net.Http.dll" 
 #r @"..\packages\Microsoft.AspNet.WebApi.Client.5.2.3\lib\net45\System.Net.Http.Formatting.dll" 
 #r @"System.Numerics.dll" 
 #r @"System.Runtime.Serialization.dll" 
 #r @"..\packages\Microsoft.AspNet.WebApi.Core.5.2.3\lib\net45\System.Web.Http.dll" 
 #r @"..\packages\Validation.2.3.7\lib\dotnet\Validation.dll"

open System
open FSharp.Configuration
open Microsoft.Azure.Devices
open Microsoft.Azure.Devices.Common.Exceptions

[<Literal>]
let c = __SOURCE_DIRECTORY__ + "../../SharedConfig/fsharp/config.yaml"

type Config = YamlConfig<FilePath=c, ReadOnly=true>

let config = Config()
let connectionString = config.AzureIoTHub.ConnectionString

let registryManager = RegistryManager.CreateFromConnectionString(connectionString)

let jobs = registryManager.GetJobsAsync() |> Async.AwaitTask |> Async.RunSynchronously
