"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
//Disable the send button until connection is established.
var sendBtn = document.getElementById("sendButton");
sendBtn.disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
    sendBtn.disabled = false;
    var connectionId = connection.connectionId;
}).catch(function (err) {
    return console.error(err.toString());
});
if (sendBtn != null){
    sendMessageFunction();
}

function sendMessageFunction() {
    document.getElementById("sendButton").addEventListener("click", function (event) {
        if (sendBtn != null) {
            var user = document.getElementById("userInput").value;
        }
        var message = document.getElementById("messageInput").value;
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });
}
