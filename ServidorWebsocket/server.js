const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: 8080 });

console.log("Servidor de Chat iniciat en el port 8080");

wss.on('connection', function connection(ws) {
    console.log('Nou usuari conectat');

    ws.on('message', function incoming(message) {
    const msgString = message.toString();
    console.log('Missatge Rebut: ' + msgString);

    wss.clients.forEach(function each(client) {
        if (client.readyState === WebSocket.OPEN) {
        client.send(msgString);
        }
    });
    });

    ws.on('close', () => {
        console.log('Un usuari sha desconectat');
    });
});