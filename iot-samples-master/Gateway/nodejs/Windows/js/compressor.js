'use strict';

const zlib = require('zlib');

module.exports = {
    messageBus: null,
    configuration: null,

    create: function (messageBus, configuration) {
        this.messageBus = messageBus;
        this.configuration = configuration;
        return true;
    },

    receive: function (message) {
        var data = Buffer.from(message.content).toString();
        var self = this;
        zlib.gzip(data, function(err, compressed){
            if(!err){ 
                // add message data.
                var target = Buffer.alloc(compressed.length + 4)
                compressed.copy(target, 4, 0, compressed.length)

                // add header data
                var compressedLength = compressed.length.toString()
                var compLen = Buffer.from(compressedLength);
                compLen.copy(target, 0, 0, 4)

                // send message
                message.content = new Uint8Array(Buffer.from(target.toString('base64')));
                self.messageBus.publish(message);
                console.log('Forwarded batched and compressed message to azure');
            }else{
                throw new Error('Error running gzip compression for data blob: %s', data);
            }
            
        });
        
    },

    destroy: function () {
        console.log('compressor.destroy');
    }
};