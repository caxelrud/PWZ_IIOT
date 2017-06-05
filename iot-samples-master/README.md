# iot-samples
This repository includes samples &amp; components that can be incorporated and used in PoCs of IoT solutions for Azure.  The primary aim of this body of work is to encode community learnings and industry agnostic best practices around IoT in a friendly quick-start context.  Examples have been organized roughly by SDK or functionality, further by language, and are subject to change/reorganization as new content is added.  

## General Topics
* [Device Management](/DeviceManagement/README.md) - IoT examples for Device Managment including device to cloud messaging.
* [Collection Parsing](/CollectionParsing/README.md) - IoT examples for processing collections of data.
* [Decompression & Shreding](DecompressShred/README.md) - Azure Function examples for decompressing and shredding IoT Hub messages.
* [Gateway](Gateway/README.md) - IoT examples that leverage the Azure Gateway SDK. 

## End to End solutions
Though the array of possible solutions around IoT Hub is nearly infinite, the following list includes a few solid E2E solutions that will help you explore not only IoT Hub capabilities, but also an array of other Azure compatiblities. 

* Simulator Device |> Protocol Gateway |> IoT Hub |> Azure Stream Analytics
* Simulator Device |> IoT Hub |> Azure Stream Analytics |> (Blob Storage, PowerBI)
* Simulator Device |> IoT Hub |> Event Hub Reader Applicaiton (run business logic, respond with cloud2device message) |> IoT Hub |> Simulator Device 
* Simulator Device |> Gateway (batch & compress) |> IoT Hub |> Azure Function (decompress & shred) |> Event Hub |> Azure Stream Analytics (flatten data) |> PowerBI 
* Simulator Device (RaspberryPi) |> Field Gateway (RaspberryPi)(batch & compress) |> IoT Hub |> Logic App |> Iot Hub (cloud2device) |> Field Gateway(RaspberryPi) |> Receiving Device (RaspberryPi)

## Contributing
Please see the [Contributing Guidelines](CONTRIBUTING.md). 

## Primary Contributors
* @williamberryiii [William Berry](https://github.com/WilliamBerryiii)
* @shwetams [shwetams](https://github.com/shwetams)

## License
This project is licensed under the [MIT License](LICENSE.txt)
