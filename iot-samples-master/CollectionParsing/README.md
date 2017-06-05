# Collection parsing #


----------


## Background ##

In a typical Internet of Things scenario data when sent to cloud is often aggregated. Individual devices / sensors send data to a gateway that aggregates and sends it over to the cloud ingestion services at regular periods of interval.

In the rare cases, where one has built a custom own ingestion service it is generally where data is de-aggregated and stored as individual records or pushed for further processing into stream processing.

Specific to Azure, aggregated data is sent over to Iot Hub or Event hub. Then you have two options:


1. Use Azure Stream Analytics Cross Apply function to break the input message into multiple data points. You can find more details [here](http://https://msdn.microsoft.com/en-us/library/azure/dn706229.aspx "Azure Stream Analytics Cross Apply").
2. Build a custom service that reads data from an Iot Hub/Event hub and then parses the collection. 

You will use Option 2 when you don't really intend to use Stream analytics for stream processing. However, most people simply deploy option 1 only to implement the de-aggregation. As it provides scale, reliability and faster development.

## Generic Collection Parser ##
We have developed a generic collection parser that can parse a JSON collection into separate JSON objects based on a configuration specified. 

### Deploy as Azure functions ###

This parser can be deployed as a Nodejs Azure function, which is an Event hub trigger. Steps to create and deploy Azure functions can be found [here](https://azure.microsoft.com/en-in/documentation/articles/functions-create-first-azure-function/ "Azure Functions Getting Started"). 


> Note: You can use it to deploy an IoT hub trigger as well. Just use the Event Hub end point (found in the Overview blade of IoT Hub) and provide it as an Event hub input while creating Azure functions.

![](functionscollparser.png)

Once you had created the function you can add the code from [**index.js**](https://github.com/Microsoft/iot-samples/blob/develop/CollectionParsing/AzureFunctions/NodejsIotHubTrigger/index.js "index.js") file. Please refer to the Integrate tab to add the inputs and outputs, you will need to accordingly modify the input & output names in the code.

The function accepts a message from event hub, uses the config variable to determine the pattern and then splits the msg into separate JSON objects. These objects are then send one by one to the Eventhub as separate messages. Or stored in the Blob storage (as a single blob but with separate messages).

You can M the output behaviour based on your requirements.

### Console application ###

The parser is also available as a console nodejs app. You deploy it in any manner you like, in a VM as a cron job, or include it in an API. 


## Setting the config variable ##

The config variable stores the pattern of the collection. While most cases your collection might be a simple array, the parser can deal with multi-level arrays. Key is to provide the right config.

Following are some samples that could help you:

#### Simple root collection ####


    [{'e1':'v11','e2':'v21'},{'e1':'v12','e2':'v22'},{'e1':'v13','e2':'v23'}...]

    **var config = {splitby:''}**
    
#### Simple named root collection ####

    {'root':[{'e1':'v11','e2':'v21'},{'e1':'v12','e2':'v22'},{'e1':'v13','e2':'v23'}...]}
    
    **var config = {splitby:'root'}**
    
#### Multi-level collection ####

Refer to the [sample3.json](https://github.com/Microsoft/iot-samples/blob/develop/CollectionParsing/SampleCollections/sample3.json) . It consists of collections at various levels. You can split the collection by specifying the pattern in the following manner:



> Note: you can run the console application by un-commenting various config values that work on this sample3.json to test for yourself how it works.

    { "random1":"this is a randome field",
	"random2": "this is a random field 2",
	"root": [
		{"a1":{"a2": [{"e1":"v1","e2":"v2"},{"e1":"v11","e2":"v21"}]} },
		{"a1":{"a2": [{"e1":"v111","e2":"v2r2"},{"e1":"v11r2","e2":"v21r2"}]} },
		{"a1":{"a2": [{"e1":"v1111","e2":"v2r3"},{"e1":"v11r3","e2":"v21r3"}]} },
		{"a1":{"a2": [{"e1":"v11111","e2":"v2r4"},{"e1":"v11r4","e2":"v21r4"}]} }	
	   ]
    }
 
    var config = {splitby:'root[.]a1'}
    {"a2": [{"e1":"v1","e2":"v2"},{"e1":"v11","e2":"v21"}]}
    {"a2": [{"e1":"v111","e2":"v2r2"},{"e1":"v11r2","e2":"v21r2"}]}
    {"a2": [{"e1":"v1111","e2":"v2r3"},{"e1":"v11r3","e2":"v21r3"}]}
    {"a2": [{"e1":"v11111","e2":"v2r4"},{"e1":"v11r4","e2":"v21r4"}]}  

    var config = {splitby:'root[.]a1[.]a2'}

    {"e1":"v1","e2":"v2"}
    {"e1":"v11","e2":"v21"}
    {"e1":"v111","e2":"v2r2"}
    {"e1":"v11r2","e2":"v21r2"}
    {"e1":"v1111","e2":"v2r3"}
    {"e1":"v11r3","e2":"v21r3"}
    {"e1":"v11111","e2":"v2r4"}
    {"e1":"v11r4","e2":"v21r4"}


#### Work in Progress ####

1. We are working on the Python application similar to the nodejs console
2. Integrating with de-compression module. Since it could be another widely used pattern. Data is aggregated & then compressed and sent over the ingestion service; the module will de-compress and then de-aggregate data.

## Contact me ##
You can write to me at [shweta.gupta@microsoft.com](mailto:shweta.gupta@microsoft.com) in case of any questions. Also, please feel free to share your feedback, especially if you have collections that this parser does not cater to.
