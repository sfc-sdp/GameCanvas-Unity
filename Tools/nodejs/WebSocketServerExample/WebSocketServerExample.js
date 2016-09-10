"use strict";
const ws = require('ws');

function main()
{
    let wss = new ws.Server({port: 3000});

    wss.broadcast = function (data)
    {
        for (let client of this.clients)
        {
            client.send(data);
        }
    };

    wss.on('connection', function (client) {
        client.on('message', function (message) {
            console.log(message);
            wss.broadcast(message);
        });
    });
}

main();
