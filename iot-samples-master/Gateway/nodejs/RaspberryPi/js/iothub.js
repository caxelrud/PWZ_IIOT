'use strict';

var Protocol = require('azure-iot-device-amqp').Amqp;
var Client = require('azure-iot-device').Client;
var Message = require('azure-iot-device').Message;
var tcpPortUsed = require('tcp-port-used');


/// IotHub module
module.exports = 
{
    iothub_client: null,
    connected: false,
    startTime: new Date(),
    output: null,

    on_connect(err) {
        if(err) {
            console.error(`Could not connect to IoT Hub. Error: ${err.message}`);
        }
        else {
            this.connected = true;

            this.iothub_client.on('message', this.on_message.bind(this));
            this.iothub_client.on('error', this.on_error.bind(this));
            this.iothub_client.on('disconnect', this.on_disconnect.bind(this));
        }
    },

    on_message(msg){
        console.log('Id: ' + msg.messageId + ' Body: ' + msg.data);
        client.complete(msg, printResultFor('completed'));
    },

    on_error(err) {
        console.error(`Azure IoT Hub error: ${err.message}`);
    },

    on_disconnect() {
        console.log('Got disconnected from Azure IoT Hub.');
        this.connected = false;
    },

    create(messageBus, configuration) {
        this.messageBus = messageBus;
        this.configuration = configuration;

        tcpPortUsed.check(5671, '10.121.209.60')
        .then(function(inUse) {
            console.log('Port 5671 usage: '+inUse);
        }, function(err) {
            console.error('Error on check:', err.message);
        });


        if(this.configuration && this.configuration.connection_string) {
            
            var connectionString = this.configuration.connection_string;
            // open a connection to the IoT Hub
            this.iothub_client = Client.fromConnectionString(connectionString, Protocol);
            this.iothub_client.open(this.on_connect.bind(this));
            return true;
        }
        else {
            console.error('This module requires the connection string to be passed in via configuration.');
            return false;
        }
    },

    receive(message) {
        var buf = new Buffer(message.content);
        var data = buf.toString();
        var m = new Message(data);
        if (message.properties) {
            for (var prop in message.properties) {
                m.properties.add(prop, message.properties[prop]);
            }
        }
        // do we need to re-connect?
        if (!this.connected) {
            this.iothub_client.open(this.on_connect.bind(this));
        }

        // Send message to IoT HUb
        if(this.connected){
            this.iothub_client.sendEvent(m, err => {
                if (err) {
                    console.error(`An error occurred when sending message to Azure IoT Hub: ${err.toString()}`);
                }
            });
        } else {
            console.log('Message send failue: Connection closed');
        }
    },

    destroy() {
        console.log('iothub_writer.destroy');
        if(this.connected) {
            this.iothub_client.close(err => {
                if(err) {
                    console.error(`An error occurred when disconnecting from Azure IoT Hub: ${err.toString()}`);
                }
            });
        }
    }
}

