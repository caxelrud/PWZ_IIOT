# iot-samples/Gateway/NodeJS/Windows
This folder includes Azure IoT Gateway example projects for Windows deployments of a NodeJS Gateway. The 
included demo [device simulator](/Gateway/nodejs/device_simulator/simulator.js) and sample Gateway 
implement message batching and compression which can be used in concert with the [Decompression and Shredding Azure Function](/DecompressShred)
to create a complete End to End Azure IoT solution.

Please note that the binaries in the [/bin](/Gateway/nodejs/Windows/bin) folder are compiled for x86 
and may need to be recompiled run on your target architecture.

# Prerequisites
* x86 compatible architecture. 
* NodeJS & NPM installed

# Getting Started
* In the root of the project run `npm install`
* Open the [gateway.json](/Gateway/nodejs/Windows/gateway.json) file and update the connection string 
under the `iothub_writer` module to map to your IoT Hub connection string.  The connection string 
should include the registerd device id of the field gateway; i.e. `HostName=foo.bar.azure-device.net;DeviceId=foo;SharedAccessKey=bar`
* Open a terminal window in the `FieldGateway/nodejs` folder and run the following command: `.\bin\gateway.exe gateway.json`
* Start the device simulator by following the [Quick Start](/Gateway/nodejs/device_simulator/README.md#quick-start) section of the device_simulator README.

The device simulator will begin pushing messages at the field gateway, which will in turn, batch 10 messages, compress and forward them on to your IoT Hub.
 