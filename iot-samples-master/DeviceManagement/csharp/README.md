# iot-samples/DeviceManagement/C#
This folder contains Azure IoT Device Management samples written in C#

## Quick Start
To get started exploring the C# language samples for Device Management: 
* Open the [Visual Studio solution](/DeviceManagement/csharp/Azure.IoTHub.Examples.CSharp.DeviceManagement.sln)
* Run a Nuget Package Restore 
* Copy `config/config.default.yaml` to `config/config.yaml` \* ... please see [Configuration](#configuration) below for details\* 
* Open `config/config.yaml` and enter: 
        * The IoT Hub HostName
        * The IoT Hub Connection string
        * A unique DeviceId & Nickname
* Set the CreateDeviceIdentity.csproj as the start-up project.
* Run the solution. 
* Confirm that the `Key` setting for the configured device is now populated in the `config/config.yaml` file.

## Getting Started 
This C# solution includes a number of samples that demonstrate the basic Device Management functionality of the Azure 
IoT Hub.  Each of the [solution/simulations](#solutions/simulations) listed below will walk you though a happy path 
usage of IoT Hub and its associated SDKs.  

#### Package Restore
Begin by opening the C# [Visual Studio solution](/DeviceManagement/csharp/Azure.IoTHub.Examples.CSharp.DeviceManagement.sln) and
kickoff a Nuget Restore.
![Restore Nuget Packages][rnp]

#### Configuration
Each project in the solution leverages a shared configuration file `config/config.yaml`, which you will notice is missing from 
the `config` folder.  We've excluded the configuration file used at runtime to help prevent accidental pushes of secure keys 
to source control, so you'll need to copy `config.default.yaml` to `config.yaml` before entering configuration values. 

Keep in mind that the [config.yaml](/DeviceManagement/csharp/config.default.yaml) file is serialized and deserialized from an object
model that is included in the [`Azure.IoTHub.Examples.CSharp.Core`](/DeviceManagement/csharp/Core/README.md) project and may be read and
written to from any number of projects in this solution.  \

At the bare minimum, you will need to add configuration information for the IoT Hub, including host name and connection strings and 
create a unique DeviceId to tests with.  Please follow these handy guides for setting up an Azure Resource Group and IoT Hub:  
* [setting up a resource group](https://azure.microsoft.com/en-us/documentation/articles/resource-group-portal/)
* [create an IoT Hub](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/#create-an-iot-hub)

With the resource group and IoT Hub created:
* copy the primary connection string into the `config.yaml->ConnectionString` field.  
![Hub Connection String][cstring]
* copy the hub host name into the `config.yaml->Hostname` field.
![Hub Hostname][hostname]

Lastly, set a unique `DeviceId` in the `config.yaml` file and proceed to the [Create a Device Identity](#create-a-device-identity)
project.

### Solutions/Simulations
* [Create a Device Identity](#create-a-device-identity)
* [Delete a Device Identity](#delete-a-device-identity)
* [Push Data to Azure IoT Hub](#push-data-to-azure-iot-hub)
* [Read Data Pushed to Azure IoT Hub](#read-data-pushed-to-azure-iot-hub)
* [Send Notification from Azure Iot Hub to Device](#send-notification-from-azure-iot-hub-to-device)
* [Send Command to Device to Upload File](#send-command-to-device-to-upload-file)
* [Pushing Different Data Types](#pushing-different-data-types)

### Create a Device Identity
[view project code](/DeviceManagement/csharp/CreateDeviceIdentity/Program.cs)

This example utilizes the connection string and device id of the `config.yaml` file to configure
a device in your Azure IoT Hub.  After running the project with the debugger, the device's PrimaryKey
field in the `config.yaml` file will be updated for use in other simulations.  A list of properties
that can be configured for your test device can be found on the [Iot Hub Guide for Developers](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-devguide/#device-identity-registry)

### Delete a Device Identity
[view project code](/DeviceManagement/csharp/DeleteDevice/Program.cs)

This example performs the following steps:
1. Create a device identity in your Azure IoT Hub if it does not already exist based on the `config.yaml` file.
2. Display a list of configured devices (should be at least 1 device after step 1).
3. Remove the device configured in the `config.yaml` file.
4. Display a list of configured devices (if there are any).
5. Add the device configured in teh `config.yaml` file back into your Azure IoT Hub.

See the ``public static async Task RemoveDevice(Device device)`` method in `Program.cs` for the [RegistryManager](https://msdn.microsoft.com/en-us/library/microsoft.azure.devices.registrymanager.aspx#Anchor_3)
call to remove configured devices.

### Push Data to Azure IoT Hub
[view code](/DeviceManagement/csharp/DeviceSimulator/Program.cs)

This example is a simulated IoT device that posts semi-randomized wind measurements to the configured
Azure IoT Hub.  This project is used in most of the following simulations as the data pump and has
several features to facilite the testing of different data schema.


### Read Data Pushed to Azure IoT Hub
[view code](/DeviceManagement/csharp/ReadDeviceToCoudMessages/Program.cs)


In this example we use two projects,`DeviceSimulator` and `ReadDeviceToCoudMessages` to initiate traffic
to the Azure IoT Hub and then read the pushed data back.  This simulation might represent a streaming aggregation
or monitoring applciation for connected devices.  To run this simulation you will need to make sure that the device
configured in the `config.yaml` file, has been added to the Azure IoT Hub using the [Create a Device Identity](#create-a-device-identity)
project and the solution has been rebuilt if the `config.yaml` was updated.

Once the simulator device has been added to your Azure IoT Hub, you will want to set both the `DeviceSimulator`
and the `ReadDeviceToCoudMessages` projects as Startup Projects, [instructions here](https://msdn.microsoft.com/en-us/library/ms165413.aspx).
Running the debugger will start both projects and you should see two console windows popup displaying data
flowing to and being read from your Azure IoT Hub.

### Send Notification from Azure Iot Hub to Device
[view code](/DeviceManagement/csharp/SendCloudToDeviceMessages/Program.cs)

In this example we use two projects, the `DeviceSimulator` and the `SendCloudToDeviceMessages` to initiate traffic
from the Azure IoT Hub to the device, having the device then *ACK* the received commands.  This simulation represents a naive [SCADA](https://en.wikipedia.org/wiki/SCADA)
implementation for remote control of devices. To run this simulation you will need to make sure that the device
configured in the `config.yaml` file, has been added to the Azure IoT Hub using the [Create a Device Identity](#create-a-device-identity)
project and the solution has been rebuilt if the `config.yaml` was updated.

Once the simulator device has been added to your Azure IoT Hub, you will want to set both the `DeviceSimulator`
and the `SendCloudToDeviceMessages` projects as Startup Projects, [instructions here](https://msdn.microsoft.com/en-us/library/ms165413.aspx).
Running the debugger will start both projects and you should see two console windows displaying data
flowing to the Azure Iot Hub from the device simulator, and a prompt to send control messages to the device
from the `SendCloudToDeviceMessages` console.

*Suggestion*: play with sending multiple messages quickly and see how messages are pulled from the hub byt the controler.

### Send Command to Device to Upload File
[view code](/DeviceManagement/csharp/SendCloudToDeviceMessages/Program.cs)

In this example we will use two projects, the `DeviceSimulator` and the `SendCloudToDeviceMessages` to explore
uploading of bulk observation data from our simulated device.  The simulation might represent the transfer of data collected
during a network outage or perhaps aggregate statistics collected by the device.  To run this simulation you will need to make sure that:
* the device configured in the `config.yaml` file, has been added to the Azure IoT Hub using the [Create a Device Identity](#create-a-device-identity)
project
* A [configured storage account](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-manage-through-portal/#file-upload)
has been attached to your Azure IoT Hub for use with your Azure IoT Hub and, create or select a default storage container.
* Turn ON *'Receive Notification for uploaded files'* under the Azure IoT Hub blade.
* The IoT Hub Storage Conatiner name has been added to the `config.yaml`
* the solution has been rebuilt, if the `config.yaml` was updated.

With the simulator device registered and blob storage attached to the hub, set both the `DeviceSimulator`
and the `SendCloudToDeviceMessages` projects as Startup Projects, [instructions here](https://msdn.microsoft.com/en-us/library/ms165413.aspx).
Running the debugger will start both projects and you should see two console windows displaying data
flowing to the Azure Iot Hub from the device simulator, and a prompt to send control messages to the device
from the `SendCloudToDeviceMessages` console.  In the `SendCloudToDeviceMessages` terminal type the phrase
`upload bulk data` and press `Enter` to request that the device upload a file to blob storage.  Once the file
has uploaded the `SendCloudToDeviceMessages` will display a receipt notification.

### Pushing Different Data Type
[view code](/DeviceManagement/csharp/DeviceSimulator/Program.cs)

The `DeviceSimulator` project includes several demo data generation functions for use in exporing other Azure products
such as [Azure Stream Analytics](https://azure.microsoft.com/en-us/services/stream-analytics/).  To utilize these functions 
open the project's `Program.cs` file, find the `Data Generator Functions` section and uncomment the examples to send 
JSON or CSV data form the simulated device to the IoT Hub.

## License
This project is licensed under the [MIT License](/LICENSE.txt)


[rnp]: /DeviceManagement/csharp/assets/rnp.png "Restore Nuget Packages"
[cstring]: /DeviceManagement/csharp/assets/cstring.png "Connection String"
[hostname]: /DeviceManagement/csharp/assets/hostname.png "Hostname"