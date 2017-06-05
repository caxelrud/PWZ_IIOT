//npm install express
//npm install body-parser

'use strict';
var express = require('express');
var bodyParser = require('body-parser')

/// Sensor module
module.exports = {
    messageBus: null,
    configuration: null,
    server: null,
    devices: {},

    create: function (messageBus, configuration) {
        this.messageBus = messageBus;
        this.configuration = configuration;

        var app = express();
        app.use(bodyParser.json())

        // GET /
        app.get("/", function(req, res){
            res.send('Welcome to our field gateway batch & compression demo.');
        });

        // POST /messages
        app.post("/messages", function(req, res){
            if (!req.body) return res.sendStatus(400)
            
            // mock protocol translation 
            var deviceId = req.body.deviceId
            var deviceName = req.body.deviceName
            var deviceType = req.body.deviceType
            var payload = JSON.stringify(req.body.data)

            console.log("Received data from device.");

            // publish message to gateway bus
            messageBus.publish({
                properties: {
                    'sensorType': deviceType,
                    'name' : deviceName,
                    'id' : deviceId
                },
                content: new Uint8Array(Buffer.from(payload))
            });

            res.sendStatus(200)
        });
        
        // 404
        app.use(function(req, res, next) {
            res.status(404).send('Resource not Found.');
        });

        // 500
        app.use(function(err, req, res, next) {
            console.error(err.stack);
            res.status(500).send('Internal Server Error.');
        });

        // service startup callback
        function startup(port){
            console.log("Started server on port %s", port)
        }
        var port = this.configuration.port

        // Start
        this.server = app.listen(port, startup(port));

        return true;
    },
    
    receive: function(message) {
    },

    destroy: function() {
        console.log("Attempting to shutdown server gracefully.");
        this.server.close(function() {
            console.log("Closed out remaining connections.");
        });
        
        console.log('receiver.destroy');
    }
};
