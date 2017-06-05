'use strict';
const BATCH_SIZE = 2
const SEPERATOR = '|'

module.exports = {
    messageBus: null,
    configuration: null,
    messageBuffers: null,

    create: function (messageBus, configuration) {
        this.messageBus = messageBus;
        this.configuration = configuration;
        this.messageBuffers = new Array();
        return true;
    },

    receive: function (message) {
        var messageContent = message.content;
        var sensorType = message.properties.sensorType

        // if we have a buffer for this sensor then add the message
        // to the queue.
        if(sensorType in this.messageBuffers){
            this.messageBuffers[sensorType].push(messageContent);
        } 
        // else create a new queue and push the message onto it.
        else {
            this.messageBuffers[sensorType] = new Array();
            this.messageBuffers[sensorType].push(messageContent);
        }

        if(this.messageBuffers[sensorType].length >= BATCH_SIZE){
            var stringified = this.messageBuffers[sensorType].map(function(data){
                let buf = Buffer.from(message.content);
                let str = buf.toString();
                return str;
            })
            
            // join the received messages together on designated seperator.
            var batch = stringified.join(SEPERATOR);

            this.messageBus.publish({
                properties: {
                    'sensorType': message.properties.deviceType,
                    'name' : message.properties.deviceName,
                    'id' : message.properties.deviceId
                },
                content: new Uint8Array(Buffer.from(batch))
            });

            // reset buffer array to 0
            this.messageBuffers[sensorType].length = 0;
        }
    },

    destroy: function () {
        if(this.messageBuffers){
            this.messageBuffers.length = 0;
        }
        console.log('batcher.destroy');
    }
};