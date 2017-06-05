'use strict';

module.exports = {
    messageBus: null,
    configuration: null,

    create: function (messageBus, configuration) {
        this.messageBus = messageBus;
        this.configuration = configuration;
        return true;
    },

    receive: function (message) {
        var buf = new Buffer(message.content);
        var receive = buf.toString();
        console.log(`printer.receive - ` + receive);
    },

    destroy: function () {
        console.log('printer.destroy');
    }
};