var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


/*
    Connect to SignalR server
    sends connection id of the current user to the database since it is used to receive instant messages
*/
connection.start().then(function () {
    $.ajax({
        url: '/?handler=UpdateConnectionId',
        method: 'POST',
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        data: {
            ConnectionId: connection.connectionId
        }
    });
    //Once connected check messages send to the user
    checkRecentMessages();
}).catch(function (err) {
    return console.error(err.toString());
});


var blogNames = document.querySelectorAll(".blog-name");
var blogs = document.querySelectorAll(".chat-blog");
var statuses = document.querySelectorAll(".blog-status");
var chatDetails = document.querySelectorAll(".chat-details");

//Sending message to another person
connection.on("UpdateSentChat", function (chat) {
    var chatBlogs = document.querySelectorAll(".chat-blog");
    var statusTick = document.querySelectorAll(".check-status");
    chatBlogs.forEach((chatBlog, index) => {
        var blogName = chatBlog.querySelector(".blog-name");
        if (blogName.innerHTML == chat.senderName) {
            statusTick = chatBlog.querySelector(".check-status");

            if (statusTick == null) {
                statusTick = document.createElement("i");
                statusTick.classList.add("bi");
                statusTick.classList.add("check-status");

                var messageContext = chatBlog.querySelector(".message-context");
                messageContext.insertBefore(statusTick, messageContext.children[0]);
            } else {
                statusTick.classList.remove("bi-check2");
                statusTick.classList.remove("bi-check2-all");
                statusTick.classList.remove("read");
            }


            if (chat.status == "Sent") {
                statusTick.classList.add("bi-check2");
            }
            else if (chat.status == "Delivered" || chat.status == "Read") {
                statusTick.classList.add("bi-check2-all");
            }
            if (chat.status == "Read") {
                statusTick.classList.add("read");
            }
            
            var outgoings = document.querySelectorAll(".outgoing-messages");

            //Updating inside chat ticks
            outgoings.forEach(outgoing => {
                var statusTick = outgoing.children[0].children[1].children[0].children[0];
                if (chat.status == "Delivered" || chat.status == "Read") {
                    if (statusTick.classList.contains("bi-check2")) { // 1 tick
                        statusTick.classList.remove("bi-check2");
                        statusTick.classList.add("bi-check2-all");
                        if (chat.status == "Read") {
                            statusTick.classList.add("read");
                        }
                    } else {
                        if (!statusTick.classList.contains("read") && chat.status == "Read") {
                            statusTick.classList.add("read");
                        }
                    }
                }
            })
        }
    });
});

//Received message
connection.on("UpdateReceivedChat", function (chat) {
    blogNames.forEach((blogName, index) => {
        if (blogName.innerHTML == chat.sentBy) {

            $.ajax({
                url: '/?handler=PhoneNumber',
                method: 'GET',
            }).done(result => {
                userPhoneNumber = result;
                connection.invoke("UpdateSentMessage", userPhoneNumber, chat.sentBy, "Delivered").catch(function (err) {
                    return console.error(err.toString());
                });
            });
            document.querySelectorAll(".blog-message")[index].innerHTML = chat.context;
            var size = document.querySelectorAll(".blog-message")[index].scrollHeight;
            if (size > 17) {
                document.querySelectorAll(".blog-message")[index].innerHTML = chat.context.split(0, size / 9);
            }
            document.querySelectorAll(".blog-time-sent")[index].innerHTML = chat.time;
            if (chat.status == "Sent") {
                if (blogs[index].children[1].children[0].children.length == 1) {
                    var unreadMessagesContainer = document.createElement("div");
                    unreadMessagesContainer.classList.add("number-of-messages");
                    unreadMessagesContainer.innerHTML = 1;
                    chatDetails[index].firstElementChild.appendChild(unreadMessagesContainer);
                } else {
                    blogs[index].children[1].children[0].children[1].innerHTML = parseInt(blogs[index].children[1].children[0].children[1].innerHTML) + 1;
                }
            }

            if ((statuses.length >= index) && (statuses[index] != undefined && statuses[index] != "")) {
                statuses[index].innerHTML = "";
            }
        }
    });
    if (document.getElementById("name").innerHTML == chat.sentBy) {
        var outgoings = document.querySelectorAll(".outgoing-messages")
        if (outgoings.length >= 1) {
            outgoings[outgoings.length - 1].children[0].children[2].children[0].innerHTML = "";
        }
    }
});

//Structure chat history, putting received messages on the left and sent messages on the right
connection.on("UpdateChat", function (chat) {
    if (chat.type == "Received") {
        blogNames.forEach((blogName, index) => {
            if (blogName.innerHTML == chat.sentBy) {
                document.querySelectorAll(".blog-message")[index].innerHTML = chat.context;
                document.querySelectorAll(".blog-time-sent")[index].innerHTML = chat.time;
                if (chat.status == "Delivered") {
                    if (blogs[index].children[1].children[0].children.length == 1) {
                        var unreadMessagesContainer = document.createElement("div");
                        unreadMessagesContainer.classList.add("number-of-messages");
                        unreadMessagesContainer.innerHTML = 1;
                        chatDetails[index].firstElementChild.appendChild(unreadMessagesContainer);
                    } else {
                        blogs[index].children[1].children[0].children[1].innerHTML = parseInt(blogs[index].children[1].children[0].children[1].innerHTML) + 1;
                    }
                }
            }
        });
    } else {
        blogNames.forEach((blogName, index) => {
            if (blogName.innerHTML == chat.receivedBy) {
                document.querySelectorAll(".blog-message")[index].innerHTML = chat.context;
                document.querySelectorAll(".blog-time-sent")[index].innerHTML = chat.time;
                if ((statuses.length >= index) && (statuses[index] != undefined && statuses[index] != "")) {
                    statuses[index].innerHTML = chat.status;
                }
                var outgoings = document.querySelectorAll(".outgoing-messages")
                outgoings[outgoings.length - 1].children[0].children[2].children[0].innerHTML = statuses[index].innerHTML;
                outgoings[outgoings.length - 1].children[0].children[2].children[0].innerHTML = statuses[index].innerHTML;
            }
        });
    }
    var blogs = document.querySelectorAll(".chat-blog");
    let usernames = document.querySelectorAll(".name");


    blogs.forEach((blog, index) => blog.addEventListener("click", () => {
        if (blog.children[1].children[1].children.length == 2) {
            var unreadContainer = document.querySelector(".unread-messages-count-container");
            unreadContainer.remove();
            var userPhoneNumber = '';
            $.ajax({
                url: '/?handler=PhoneNumber',
                method: 'GET',
            }).done(result => {
                userPhoneNumber = result;
                connection.invoke("UpdateMessage", blog.children[1].children[0].children[0].innerHTML, userPhoneNumber, "Read", "Received").catch(function (err) {
                    return console.error(err.toString());
                });
            });
        }
        var chatContainer = document.querySelector(".chat");
        if (chatContainer.classList.contains("hidden")) {
            chatContainer.classList.remove("hidden");
            var messageInput = $("#message")[0];
            messageInput.focus();
        }
        var username = usernames[index].innerHTML;
        $.ajax({
            url: '/?handler=ConversationByUsername',
            method: 'GET',
            data: {
                contactUserName: username
            }
        }).done(result => {
            console.log(result)
            var json = JSON.parse(result);

            var nameTittle = document.querySelector("#name");
            var lastSeen = document.querySelector("#last-seen");
            nameTittle.innerHTML = json.ContactName;
            lastSeen.innerHTML = "Today at " + json.LastSeen;

            var messages = json.Messages;
            var date = '';
            var messageBodyContainer = document.querySelector("#message-body");
            var child = messageBodyContainer.lastElementChild;
            while (child) {
                messageBodyContainer.removeChild(child);
                child = messageBodyContainer.lastElementChild;
            }

            messages.forEach(message => {
                var csDate = message.Date;

                var dateContainer = document.createElement("div");
                dateContainer.classList.add("date");
                dateContainer.innerHTML = csDate;

                var dateLines = document.querySelectorAll(".date");
                var shouldAddDateLine = true;
                if (dateLines.length == 0) {
                    messageBodyContainer.append(dateContainer);
                } else {
                    dateLines.forEach(dateline => {
                        if (dateline.innerHTML == date)
                            shouldAddDateLine = false;
                    })
                    if (shouldAddDateLine) {
                        messageBodyContainer.append(dateContainer);
                    }
                }


                var conversationBodyContainer = document.createElement("div");
                conversationBodyContainer.classList.add("conversation-body");

                var nameContainer = document.createElement("div");
                nameContainer.classList.add("name");

                var messageContainer = document.createElement("div");
                messageContainer.classList.add("message");

                var messageInfoContainer = document.createElement("div");
                messageInfoContainer.classList.add("message-info");

                var statusContainer = document.createElement("div");
                statusContainer.classList.add("status");

                var timeSentContainer = document.createElement("div");
                timeSentContainer.classList.add("time-sent");

                nameContainer.innerHTML = message.Type == "Sent" ? "You" : json.ContactName;
                messageContainer.innerHTML = message.Context;
                statusContainer.innerHTML = message.Type == "Sent" ? message.Status : "";
                timeSentContainer.innerHTML = message.Time;

                if (message.Type == "Received") {
                    var incomingContainer = document.createElement("div");
                    incomingContainer.classList.add("incoming");

                    var incomingMessagesContainer = document.createElement("div");
                    incomingMessagesContainer.classList.add("incoming-messages");

                    var incomingMessageContentContainer = document.createElement("div");
                    incomingMessageContentContainer.classList.add("incoming-message-content");

                    messageBodyContainer.append(incomingContainer);
                    incomingContainer.append(conversationBodyContainer);
                    conversationBodyContainer.append(incomingMessagesContainer);
                    incomingMessagesContainer.append(incomingMessageContentContainer);
                    incomingMessageContentContainer.append(nameContainer, messageContainer, messageInfoContainer);

                } else {
                    var outgoingContainer = document.createElement("div");
                    outgoingContainer.classList.add("outgoing");

                    var outgoingMessagesContainer = document.createElement("div");
                    outgoingMessagesContainer.classList.add("outgoing-messages");

                    var outgoingMessageContentContainer = document.createElement("div");
                    outgoingMessageContentContainer.classList.add("outgoing-message-content")

                    messageBodyContainer.append(outgoingContainer);
                    outgoingContainer.append(conversationBodyContainer);
                    conversationBodyContainer.append(outgoingMessagesContainer);
                    outgoingMessagesContainer.append(outgoingMessageContentContainer);
                    outgoingMessageContentContainer.append(nameContainer, messageContainer, messageInfoContainer);
                }
                messageInfoContainer.append(statusContainer, timeSentContainer);

            })
            messageBodyContainer.scrollTo(0, messageBodyContainer.scrollHeight);
        })
    }))

});

/*
    This function will be triggered when someone has sent message to the user
*/

connection.on("ReceiveMessage", function (chat) {
    var chatContainer = document.querySelector(".chat");

    //check on recent messages if sent number exits else add it at top of container
    var blogs = document.querySelectorAll(".chat-blog");
    var conversationIndex = -1;
    blogs.forEach((blog, index) => {
        if (blog.children[1].children[0].children[0].innerHTML == chat.sentBy) {
            conversationIndex = index;
        }
    });
    /*
        When conversationIndex is equal to -1, it means it's new conversation
        We create chat block then put it at the top
    */
    if (conversationIndex == -1) {
        var chatsContainer = document.querySelector(".chats-container");
        var chatBlogContainer = document.createElement("div");
        chatBlogContainer.classList.add("chat-blog");

        var pictureContainer = document.createElement("div");
        pictureContainer.classList.add("picture");

        var profileAvatarContainer = document.createElement("div");
        profileAvatarContainer.classList.add("profile-avatar");
        var iElement = document.createElement("i");
        iElement.classList.add("bi");
        iElement.classList.add("bi-person");
        iElement.style.fontSize = "40px";

        // add picture to chat blog
        profileAvatarContainer.appendChild(iElement);
        pictureContainer.appendChild(profileAvatarContainer);
        //**************************** */

        var chatDetailsContainer = document.createElement("div");
        chatDetailsContainer.classList.add("chat-details");

        var messageTopInfo = document.createElement("div");
        messageTopInfo.classList.add("message-top-info");

        //Message top info (name of chat and time message is sent)
        var blogNameDiv = document.createElement("div");
        blogNameDiv.classList.add("name");
        blogNameDiv.classList.add("blog-name");
        blogNameDiv.innerHTML = chat.sentBy;

        var timeSentDiv = document.createElement("div");
        timeSentDiv.classList.add("time-sent");
        timeSentDiv.classList.add("blog-time-sent");
        timeSentDiv.innerHTML = chat.time;

        messageTopInfo.append(blogNameDiv, timeSentDiv);
        chatDetailsContainer.appendChild(messageTopInfo);
        //****************************************** */




        //Message body (text message and unread messages)
        var messageBodyDiv = document.createElement("div");
        messageBodyDiv.classList.add("message-body-container");

        var messageContextDiv = document.createElement("div");
        messageContextDiv.classList.add("message-context");


        var textMessageSpan = document.createElement("span");
        textMessageSpan.classList.add("text-message");
        textMessageSpan.innerHTML = chat.context;

        var unreadMessagesCountDiv = document.createElement("div");
        unreadMessagesCountDiv.classList.add("unread-messages-count-container");
        var numberOfMessagesDiv = document.createElement("div");
        numberOfMessagesDiv.classList.add("number-of-messages");
        
        numberOfMessagesDiv.innerHTML = 1;
        unreadMessagesCountDiv.appendChild(numberOfMessagesDiv);


        messageContextDiv.appendChild(textMessageSpan);
        messageBodyDiv.append(messageContextDiv, unreadMessagesCountDiv);
        chatDetailsContainer.appendChild(messageBodyDiv);
        //************************************************* */



        // Add elements to the ui
        if (chatsContainer.childElementCount == 0) {
            chatsContainer.appendChild(chatBlogContainer);
        } else {
            chatsContainer.insertBefore(chatBlogContainer, chatsContainer.children[0]);
        }
        chatBlogContainer.append(pictureContainer, chatDetailsContainer);


    } else {
        var chatsContainers = document.querySelectorAll(".chats-container");
        chatsContainers.forEach((container, index) => {
            var name = container.querySelector(".blog-name");
            if (name.innerHTML == chat.sentBy) {
                document.querySelectorAll(".text-message")[index].innerHTML = chat.context;
                document.querySelectorAll(".time-sent")[index].innerHTML = chat.time;
                document.querySelectorAll(".status")[index].innerHTML = "";

                // This is for when user is the 
                var statusTick = container.querySelector(".check-status");
                if (statusTick != null) {
                    statusTick.remove();
                }
                //add unread container
                if (!chatContainer.classList.contains("hidden")) { // there is open chat
                    var nameTittle = document.querySelector("#name");
                    if (nameTittle != null && nameTittle.innerHTML != chat.sentBy) { // if open chat is of the sender
                        var numberOfMessagesDiv = container.querySelector(".number-of-messages");

                        if (numberOfMessagesDiv == null) { // there are no unread messages
                            unreadMessageCountDiv = document.createElement("div");
                            unreadMessageCountDiv.classList.add("unread-message-count-container");
                            numberOfMessagesDiv = document.createElement("div");
                            numberOfMessagesDiv.classList.add("number-of-messages");
                            numberOfMessagesDiv.innerHTML = 1;
                            unreadMessageCountDiv.appendChild(numberOfMessagesDiv);

                            var messageBodyDiv = container.querySelector(".message-body-container");
                            messageBodyDiv.appendChild(unreadMessageCountDiv);
                        } else {
                            var numberOfMessagesDiv = container.querySelector(".number-of-messages");
                            var currentUnreadMessages = numberOfMessagesDiv.innerHTML;
                            numberOfMessagesDiv.innerHTML = currentUnreadMessages + 1;
                        }
                    }
                } else { // no chat open
                    var numberOfMessagesDiv = container.querySelector(".number-of-messages");

                    if (numberOfMessagesDiv == null) { // there are no unread messages
                        unreadMessageCountDiv = document.createElement("div");
                        unreadMessageCountDiv.classList.add("unread-message-count-container");
                        numberOfMessagesDiv = document.createElement("div");
                        numberOfMessagesDiv.classList.add("number-of-messages");
                        numberOfMessagesDiv.innerHTML = 1;
                        unreadMessageCountDiv.appendChild(numberOfMessagesDiv);

                        var messageBodyDiv = container.querySelector(".message-body-container");
                        messageBodyDiv.appendChild(unreadMessageCountDiv);
                    } else {
                        var numberOfMessagesDiv = container.querySelector(".number-of-messages");
                        var currentUnreadMessages = numberOfMessagesDiv.innerHTML;
                        numberOfMessagesDiv.innerHTML = currentUnreadMessages + 1;
                    }
                }

                var chatsContainer = document.querySelector(".chats-container");
                var currentChat = chatsContainer.children[index];
                chatsContainer.removeChild(currentChat);
                chatsContainer.insertBefore(currentChat, chatsContainer.children[0]);
            }
        })
    }

    //check if receiver was on our sender chat
    if (!chatContainer.classList.contains("hidden")) {
        var nameTittle = document.querySelector("#name");
        if (nameTittle != null && nameTittle.innerHTML == chat.sentBy) { // was on the chat
            var messageBodyContainer = document.querySelector("#message-body");

            var dateContainer = document.createElement("div");
            dateContainer.classList.add("date");
            dateContainer.innerHTML = "Today";

            var dateLines = document.querySelectorAll(".date");
            if (dateLines[dateLines.length - 1].innerHTML != "Today")
                messageBodyContainer.append(dateContainer);


            var conversationBodyContainer = document.createElement("div");
            conversationBodyContainer.classList.add("conversation-body");

            var messageContainer = document.createElement("div");
            messageContainer.classList.add("message");

            var messageStatusContainer = document.createElement("div");
            messageStatusContainer.classList.add("message-info");

            var statusContainer = document.createElement("div");
            statusContainer.classList.add("status");

            var timeSentContainer = document.createElement("div");
            timeSentContainer.classList.add("time-sent");

            messageContainer.innerHTML = chat.context;
            statusContainer.innerHTML = "";
            timeSentContainer.innerHTML = chat.time;

            var incomingContainer = document.createElement("div");
            incomingContainer.classList.add("incoming");

            var incomingMessagesContainer = document.createElement("div");
            incomingMessagesContainer.classList.add("incoming-messages");

            var incomingMessageContentContainer = document.createElement("div");
            incomingMessageContentContainer.classList.add("incoming-message-content");
            messageStatusContainer.append(statusContainer, timeSentContainer);

            messageBodyContainer.append(incomingContainer);
            incomingContainer.append(conversationBodyContainer);
            conversationBodyContainer.append(incomingMessagesContainer);
            incomingMessagesContainer.append(incomingMessageContentContainer);
            incomingMessageContentContainer.append(messageContainer, messageStatusContainer);

            //invoke update with read status
            var userPhoneNumber = '';
            $.ajax({
                url: '/?handler=PhoneNumber',
                method: 'GET',
            }).done(result => {
                userPhoneNumber = result;
                connection.invoke("UpdateSentMessageStatus", "Read", chat.sentBy, userPhoneNumber).catch(function (err) {
                    return console.error(err.toString());
                });
            });
        }
    } else {
        //invoke update with delivered status
        var userPhoneNumber = '';
        $.ajax({
            url: '/?handler=PhoneNumber',
            method: 'GET',
        }).done(result => {
            userPhoneNumber = result;
            connection.invoke("UpdateSentMessageStatus", "Delivered", chat.sentBy, userPhoneNumber).catch(function (err) {
                return console.error(err.toString());
            });
        });
    }
    var chatContainer = document.querySelector(".chat");
    if (!chatContainer.classList.contains("hidden")) {
        messageBodyContainer.scrollTo(0, messageBodyContainer.scrollHeight);
        var messageInput = $("#message")[0];
        messageInput.focus();
    }
    openChat();
});

/*
    Buttons to close modals
*/
var closeButtons = document.querySelectorAll("#close-btn");
var newContactModal = document.querySelector(".new-contact-modal");
var contactListModal= document.querySelector(".contact-list-modal");
closeButtons.forEach((closeButton, index) => {
    closeButton.addEventListener("click", () => {
        if (index == 0) {
            if (!newContactModal.classList.contains("hide")) {
                newContactModal.classList.add("hide");
            }
        } else {
            if (!contactListModal.classList.contains("hide")) {
                contactListModal.classList.add("hide");
            }
        }
    });
});

var createContactButtons = document.querySelectorAll("#create");
createContactButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        if (newContactModal.classList.contains("hide")) {
            newContactModal.classList.remove("hide")
        }
        if (!contactListModal.classList.contains("hide")) {
            contactListModal.classList.add("hide")
        }
    })
});

/*
    Show all contacts
*/

var newMessageButton = document.querySelector("#create-message");
newMessageButton.addEventListener("click", () => {
    //Send request and get data
    fetch('/?handler=Data', {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
        },
    })
        .then(response => response.json())
        .then(contacts => {
            var availableContactsContainer = document.querySelector(".available-contacts");
            var child = availableContactsContainer.lastElementChild;
            if (contacts.length > 0) {
                while (child) {
                    availableContactsContainer.removeChild(child);
                    child = availableContactsContainer.lastElementChild;
                }
            }
            contacts.forEach(contact => {
                var contactContainer = document.createElement("div");
                contactContainer.classList.add("contact-container");
                var detailsContainer = document.createElement("div");
                var usernameContainer = document.createElement("div");
                var phoneNumberContainer = document.createElement("div");
                var actionContainer = document.createElement("div");

                usernameContainer.classList.add("username");
                phoneNumberContainer.classList.add("phone-number");
                actionContainer.classList.add("action");

                usernameContainer.innerText = contact.UserName;
                phoneNumberContainer.innerText = contact.PhoneNumber;

                detailsContainer.append(usernameContainer, phoneNumberContainer);

                actionContainer.innerText = contact.IsMember ? "Message" : "Invite";
                contactContainer.appendChild(detailsContainer);
                contactContainer.appendChild(actionContainer);
                availableContactsContainer.appendChild(contactContainer);
            })
            //Hide new contact form modal if visible
            if (!newContactModal.classList.contains("hide")) {
                newContactModal.classList.add("hide")
            }
            //Show contact list modal if not visible
            if (contactListModal.classList.contains("hide")) {
                contactListModal.classList.remove("hide")
            }
            MessageFunction();
        })
});
openChat();
var phoneNumber;

/*
    Adds functionality for message button since it's not part of the ui at first
    Once chat is opened, it will pull latest chats if there's conversation already or open chat block with empty conversation
*/

function MessageFunction() {
    var actionButtons = document.querySelectorAll(".action");
    actionButtons.forEach((actionButton, index) => {
        if (actionButton.innerHTML.includes("Message")) {
            actionButton.addEventListener("click", () => {
                if (!contactListModal.classList.contains("hide")) {
                    contactListModal.classList.add("hide");
                }
                var chatContainer = document.querySelector(".chat");
                if (chatContainer.classList.contains("hidden")) {
                    chatContainer.classList.remove("hidden")
                }
                var username = document.querySelectorAll(".username")[index].innerHTML;
                $.ajax({
                    url: '/?handler=ConversationByUsername',
                    method: 'GET',
                    data: {
                        contactUserName: username
                    }
                }).done(result => {
                    console.log(result)
                    var json = JSON.parse(result);

                    var nameTittle = document.querySelector("#name");
                    var lastSeen = document.querySelector("#last-seen");
                    nameTittle.innerHTML = json.ContactName;
                    lastSeen.innerHTML = json.LastSeen;

                    var messages = json.Messages;
                    var date = '';
                    var messageBodyContainer = document.querySelector("#message-body");
                    var child = messageBodyContainer.lastElementChild;
                    while (child) {
                        messageBodyContainer.removeChild(child);
                        child = messageBodyContainer.lastElementChild;
                    }

                    messages.forEach(message => {
                        var today = new Date();
                        var yesterday = new Date(today);
                        yesterday.setDate(yesterday.getDate() - 1);
                        var csDate = message.Date.split("/");
                        var formatedDate = (csDate[1][0] == "0" ? csDate[1].replaceAll("0", "") : csDate[1]) + "/" + (csDate[2][0] == 0 ? csDate[1].replaceAll("0", "") : csDate[2]) + "/" + csDate[0];

                        if (formatedDate == today.toLocaleDateString()) {
                            date = "Today";
                        } else if (formatedDate == yesterday.toLocaleDateString()) {
                            date = "Yesterday";
                        }
                        else {
                            date = message.Date
                        }

                        var dateContainer = document.createElement("div");
                        dateContainer.classList.add("date");
                        dateContainer.innerHTML = date;

                        var dateLines = document.querySelectorAll(".date");
                        var shouldAddDateLine = true;
                        if (dateLines.length == 0) {
                            messageBodyContainer.append(dateContainer);
                        } else {
                            dateLines.forEach(dateline => {
                                if (dateline.innerHTML == date)
                                    shouldAddDateLine = false;
                            })
                            if (shouldAddDateLine) {
                                messageBodyContainer.append(dateContainer);
                            }
                        }


                        var conversationBodyContainer = document.createElement("div");
                        conversationBodyContainer.classList.add("conversation-body");

                        var nameContainer = document.createElement("div");
                        nameContainer.classList.add("name");

                        var messageContainer = document.createElement("div");
                        messageContainer.classList.add("message");

                        var messageInfoContainer = document.createElement("div");
                        messageInfoContainer.classList.add("message-info");

                        var statusContainer = document.createElement("div");
                        statusContainer.classList.add("status");

                        var timeSentContainer = document.createElement("div");
                        timeSentContainer.classList.add("time-sent");

                        nameContainer.innerHTML = message.Type == "Sent" ? "You" : json.ContactName;
                        messageContainer.innerHTML = message.Context;
                        statusContainer.innerHTML = message.Type == "Sent" ? message.Status : "";
                        timeSentContainer.innerHTML = message.Time;

                        if (message.Type == "Received") {
                            var incomingContainer = document.createElement("div");
                            incomingContainer.classList.add("incoming");

                            var incomingMessagesContainer = document.createElement("div");
                            incomingMessagesContainer.classList.add("incoming-messages");

                            var incomingMessageContentContainer = document.createElement("div");
                            incomingMessageContentContainer.classList.add("incoming-message-content");

                            messageBodyContainer.append(incomingContainer);
                            incomingContainer.append(conversationBodyContainer);
                            conversationBodyContainer.append(incomingMessagesContainer);
                            incomingMessagesContainer.append(incomingMessageContentContainer);
                            incomingMessageContentContainer.append(nameContainer, messageContainer, messageInfoContainer);

                        } else {
                            var outgoingContainer = document.createElement("div");
                            outgoingContainer.classList.add("outgoing");

                            var outgoingMessagesContainer = document.createElement("div");
                            outgoingMessagesContainer.classList.add("outgoing-messages");

                            var outgoingMessageContentContainer = document.createElement("div");
                            outgoingMessageContentContainer.classList.add("outgoing-message-content")

                            messageBodyContainer.append(outgoingContainer);
                            outgoingContainer.append(conversationBodyContainer);
                            conversationBodyContainer.append(outgoingMessagesContainer);
                            outgoingMessagesContainer.append(outgoingMessageContentContainer);
                            outgoingMessageContentContainer.append(nameContainer, messageContainer, messageInfoContainer);
                        }
                        messageInfoContainer.append(statusContainer, timeSentContainer);

                    })
                    messageBodyContainer.scrollTo(0, messageBodyContainer.scrollHeight);
                })
            })
        }
    })
}

/*
    Sending message to another contact
*/

var sendMessageButton = document.querySelector("#send-message");
if (sendMessageButton != null) {
    sendMessageButton.addEventListener("click", () => {
        var textMessage = document.querySelector("#message").value;
        document.querySelector("#message").value = "";

        // There 

        if (textMessage.length > 0) {

            var date = "Today";
            var messageBodyContainer = document.querySelector("#message-body");

            var dateContainer = document.createElement("div");
            dateContainer.classList.add("date");
            dateContainer.innerHTML = date;

            var dateLines = document.querySelectorAll(".date");
            var shouldAddDateLine = true;
            if (dateLines.length == 0) {
                messageBodyContainer.append(dateContainer);
            } else {
                dateLines.forEach(dateline => {
                    if (dateline.innerHTML == date)
                        shouldAddDateLine = false;
                })
                if (shouldAddDateLine) {
                    messageBodyContainer.append(dateContainer);
                }
            }


            var conversationBodyContainer = document.createElement("div");
            conversationBodyContainer.classList.add("conversation-body");

            var messageContainer = document.createElement("div");
            messageContainer.classList.add("message");

            var messageInfoContainer = document.createElement("div");
            messageInfoContainer.classList.add("message-info");

            var statusContainer = document.createElement("div");
            statusContainer.classList.add("status");

            var timeSentContainer = document.createElement("div");
            timeSentContainer.classList.add("time-sent");

            var statusTick = document.createElement("i");
            statusTick.classList.add("bi");
            statusTick.classList.add("bi-check2");
            statusTick.classList.add("check-status");

            messageContainer.innerHTML = textMessage;
            statusContainer.appendChild(statusTick);

            var today = new Date();
            timeSentContainer.innerHTML = today.getHours() + ":" + today.getMinutes();

            var outgoingContainer = document.createElement("div");
            outgoingContainer.classList.add("outgoing");

            var outgoingMessagesContainer = document.createElement("div");
            outgoingMessagesContainer.classList.add("outgoing-messages");

            var outgoingMessageContentContainer = document.createElement("div");
            outgoingMessageContentContainer.classList.add("outgoing-message-content")

            messageBodyContainer.append(outgoingContainer);
            outgoingContainer.append(conversationBodyContainer);
            conversationBodyContainer.append(outgoingMessagesContainer);
            outgoingMessagesContainer.append(outgoingMessageContentContainer);
            outgoingMessageContentContainer.append(messageContainer, messageInfoContainer);
            messageInfoContainer.append(statusContainer, timeSentContainer);
            var outgoings = document.querySelectorAll(".outgoing-messages");
            
            messageBodyContainer.scrollTo(0, messageBodyContainer.scrollHeight);

            var chatBlogs = document.querySelectorAll(".chat-blog");
            chatBlogs.forEach((chatBlog, index) => {
                var blogName = chatBlog.querySelector(".blog-name");
                if (blogName.innerHTML == document.getElementById("name").innerHTML) {
                    var newMessage = outgoings[outgoings.length - 1].children[0].children[0].innerHTML;
                    document.querySelectorAll(".text-message")[index].innerHTML = newMessage;
                    document.querySelectorAll(".blog-status")[index].innerHTML = "Sent";
                    document.querySelectorAll(".blog-time-sent")[index].innerHTML = timeSentContainer.innerHTML;
                    var messageContextDiv = chatBlog.querySelector(".message-context");

                    var statusTick = chatBlog.querySelector(".check-status");
                    if (statusTick == null) {
                        statusTick = document.createElement("i");
                        statusTick.classList.add("bi");
                        statusTick.classList.add("bi-check2");
                        statusTick.classList.add("check-status");

                        messageContextDiv.insertBefore(statusTick, messageContextDiv.children[0]);
                    } else {
                        statusTick.classList.remove("bi-check2-all");
                        statusTick.classList.remove("read");
                        statusTick.classList.add("bi-check2");
                    }
                }
            })
            /*
                Find position of the person user is sending message to from list of recent chats
                Purpose is to update the index and put it at the top to show it is the most recent message
            */
            var blogs = document.querySelectorAll(".chat-blog");
            var conversationIndex = -1;

            blogs.forEach((blog, index) => {
                if (blog.children[1].children[0].children[0].innerHTML == document.getElementById("name").innerHTML) {
                    conversationIndex = index;
                }
            });

            /*
                When conversationIndex is equal to -1, it means it's new conversation
                We create chat block then put it at the top
            */
            if (conversationIndex == -1) {
                var chatsContainer = document.querySelector(".chats-container");

                var chatBlogContainer = document.createElement("div");
                chatBlogContainer.classList.add("chat-blog")

                var pictureContainer = document.createElement("div");
                pictureContainer.classList.add("picture");

                var profileAvatarContainer = document.createElement("div");
                profileAvatarContainer.classList.add("profile-avatar");
                var iElement = document.createElement("i");
                iElement.classList.add("bi");
                iElement.classList.add("bi-person");
                iElement.style.fontSize = "40px";

                // add picture to chat blog
                profileAvatarContainer.appendChild(iElement);
                pictureContainer.appendChild(profileAvatarContainer);
                //**************************** */

                var chatDetailsContainer = document.createElement("div");
                chatDetailsContainer.classList.add("chat-details");

                var messageTopInfo = document.createElement("div");
                messageTopInfo.classList.add("message-top-info");

                //Message top info (name of chat and time message is sent)
                var blogNameDiv = document.createElement("div");
                blogNameDiv.classList.add("name");
                blogNameDiv.classList.add("blog-name");
                blogNameDiv.innerHTML = document.getElementById("name").innerHTML;

                var timeSentDiv = document.createElement("div");
                timeSentDiv.classList.add("time-sent");
                timeSentDiv.classList.add("blog-time-sent");
                timeSentDiv.innerHTML = timeSentContainer.innerHTML;

                messageTopInfo.append(blogNameDiv, timeSentDiv);
                chatDetailsContainer.appendChild(messageTopInfo)
                //****************************************** */




                //Message body (ticks, text message and unread messages)
                var messageBodyDiv = document.createElement("div");
                messageBodyDiv.classList.add("message-body-container");

                var messageContextDiv = document.createElement("div");
                messageContextDiv.classList.add("message-context");

                var oneCheckIcon = document.createElement("i");
                oneCheckIcon.classList.add("bi");
                oneCheckIcon.classList.add("bi-check2");
                oneCheckIcon.classList.add("check-status");

                var textMessageSpan = document.createElement("span");
                textMessageSpan.classList.add("text-message");
                textMessageSpan.innerHTML = textMessage;


                messageContextDiv.append(oneCheckIcon, textMessageSpan);
                messageBodyDiv.appendChild(messageContextDiv);
                chatDetailsContainer.appendChild(messageBodyDiv);
                //************************************************* */



                // Old message status, will be hidden
                var messageBottomStatusDiv = document.createElement("div");
                messageBottomStatusDiv.classList.add("message-bottom-status");

                var bottomStatusDiv = document.createElement("div");
                bottomStatusDiv.classList.add("status");
                bottomStatusDiv.classList.add("blog-status");
                bottomStatusDiv.innerHTML = "Sent";

                messageBottomStatusDiv.appendChild(bottomStatusDiv);
                chatDetailsContainer.appendChild(messageBottomStatusDiv);
                //***************************************************** */


                // Add elements to the ui
                if (chatsContainer.childElementCount == 0) {
                    chatsContainer.appendChild(chatBlogContainer);
                } else {
                    chatsContainer.insertBefore(chatBlogContainer, chatsContainer.children[0]);
                }
                chatBlogContainer.append(pictureContainer, chatDetailsContainer);
                openChat();
            }

            /*
                Chat block already exists, means we have conversation already
                We just update index and put it at the top
            */

            else {
                var blogNames = document.querySelectorAll(".blog-name");
                blogNames.forEach((name, index) => {
                    var nameContainer = document.querySelector("#name");
                    if (name.innerHTML == nameContainer.innerHTML) {
                        var chatsContainer = document.querySelector(".chats-container");
                        var currentChat = chatsContainer.children[index];
                        var statusTick = currentChat.querySelector(".check-status")
                        if (statusTick != null) {
                            if (statusTick.classList.contains("bi-check2-all")) {
                                statusTick.classList.remove("bi-check2-all");
                                if (statusTick.classList.contains("read")) {
                                    statusTick.classList.remove("read");
                                }
                                statusTick.classList.add("bi-check2");
                            }
                        } else {
                            statusTick = document.createElement("i");
                            statusTick.classList.add("bi");
                            statusTick.classList.add("bi-check2");
                            statusTick.classList.add("check-status");

                            var messageContext = chatsContainer.querySelector(".message-context");
                            messageContext.insertBefore(statusTick, messageContext.children[0]);
                        }

                        chatsContainer.removeChild(currentChat);
                        chatsContainer.insertBefore(currentChat, chatsContainer.children[0]);
                    }
                })
            }

            /*
                Get name of person user is sending message to then send message 
            */

            phoneNumber = document.querySelector("#name").innerHTML;
            var userPhoneNumber = '';
            $.ajax({
                url: '/?handler=PhoneNumber',
                method: 'GET',
            }).done(result => {
                userPhoneNumber = result;
                var message = new Message(phoneNumber, textMessage);

                connection.invoke("SendToReceiver", message, userPhoneNumber).catch(function (err) {
                    return console.error(err.toString());
                });
            });
        }
    });
}

/*
    Adding new contact to contact list
*/

var createAccountButton = document.querySelector(".submit-button");
createAccountButton.addEventListener("click", () => {
    var phoneNumber = document.querySelector("#phone").value;
    var username = document.querySelector("#username").value;
    $.ajax({
        url: '/?handler=Account',
        method: 'POST',
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        data: {
            PhoneNumber: phoneNumber,
            UserName: username
        }
    }).then(result => {
        var json = JSON.parse(result)
        if (json.outcome == "Success") {
            var closeButtons = document.querySelectorAll("#close-btn");
            closeButtons.forEach(button => button.click());
            alert("Account added successfully");
        } else {
            console.log("Failed: " + json);
        }
    })
});

/*
    Open Chat
    
    This function is for handling event when user clicks on chat to open conversation
*/
function openChat() {
    var blogs = document.querySelectorAll(".chat-blog");
    let usernames = document.querySelectorAll(".name");

    blogs.forEach((blog, index) => blog.addEventListener("click", () => {
        var unreadMessages = blog.querySelector(".number-of-messages");

        /*
            If message is unread and user clicks on it, send update to the other side to inform the text has been read
        */
        if (unreadMessages != null) { // unread message
            unreadMessages.remove();
            //update sent chat status
            var userPhoneNumber = '';

            $.ajax({
                url: '/?handler=PhoneNumber',
                method: 'GET',
            }).done(result => {
                userPhoneNumber = result;
                connection.invoke("UpdateSentMessageStatus", "Read", usernames[index].innerHTML, userPhoneNumber).catch(function (err) {
                    return console.error(err.toString());
                });
            });
        }

        // Reveals conversation container where chats will be displayed since it's hidden at first
        var chatContainer = document.querySelector(".chat");
        if (chatContainer.classList.contains("hidden")) {
            chatContainer.classList.remove("hidden")
            var messageInput = $("#message")[0];
            messageInput.focus();
        }

        /*
            Retrireve name of chat then pull conversation of that name
        */
        var username = usernames[index].innerHTML;
        $.ajax({
            url: '/?handler=ConversationByUsername',
            method: 'GET',
            data: {
                contactUserName: username
            }
        }).done(result => {

            /*
                This occurs when conversation has been pulled from the database
                It creates conversation ui
            */

            var json = JSON.parse(result);
            var nameTittle = document.querySelector("#name");
            var lastSeen = document.querySelector("#last-seen");
            nameTittle.innerHTML = json.ContactName;
            lastSeen.innerHTML = json.LastSeen != "Online" ? "Last seen " + json.LastSeen : json.LastSeen;

            var messages = json.Messages;
            var messageBodyContainer = document.querySelector("#message-body");
            var child = messageBodyContainer.lastElementChild;
            while (child) {
                messageBodyContainer.removeChild(child);
                child = messageBodyContainer.lastElementChild;
            }

            messages.forEach(message => {
                var csDate = message.Date;
                var dateContainer = document.createElement("div");
                dateContainer.classList.add("date");
                dateContainer.innerHTML = csDate;

                var dateLines = document.querySelectorAll(".date");
                var shouldAddDateLine = true;
                if (dateLines.length == 0) {
                    messageBodyContainer.append(dateContainer);
                } else {
                    dateLines.forEach(dateline => {
                        if (dateline.innerHTML == csDate)
                            shouldAddDateLine = false;
                    })
                    if (shouldAddDateLine) {
                        messageBodyContainer.append(dateContainer);
                    }
                }


                var conversationBodyContainer = document.createElement("div");
                conversationBodyContainer.classList.add("conversation-body");

                var messageContainer = document.createElement("div");
                messageContainer.classList.add("message");

                var messageInfoContainer = document.createElement("div");
                messageInfoContainer.classList.add("message-info");

                var statusContainer = document.createElement("div");
                statusContainer.classList.add("status");

                var timeSentContainer = document.createElement("div");
                timeSentContainer.classList.add("time-sent");

                messageContainer.innerHTML = message.Context;


                timeSentContainer.innerHTML = message.Time;

                if (message.Type == "Received") {
                    statusContainer.innerHTML = "";
                    var incomingContainer = document.createElement("div");
                    incomingContainer.classList.add("incoming");

                    var incomingMessagesContainer = document.createElement("div");
                    incomingMessagesContainer.classList.add("incoming-messages");

                    var incomingMessageContentContainer = document.createElement("div");
                    incomingMessageContentContainer.classList.add("incoming-message-content");

                    messageBodyContainer.append(incomingContainer);
                    incomingContainer.append(conversationBodyContainer);
                    conversationBodyContainer.append(incomingMessagesContainer);
                    incomingMessagesContainer.append(incomingMessageContentContainer);
                    messageInfoContainer.append(timeSentContainer);
                    incomingMessageContentContainer.append(messageContainer, messageInfoContainer);

                } else {
                    var statusTick = document.createElement("i");
                    statusTick.classList.add("bi");
                    if (message.Status == "Sent") {
                        statusTick.classList.add("bi-check2");
                    } else if (message.Status == "Delivered") {
                        statusTick.classList.add("bi-check2-all");
                    } else if (message.Status == "Read") {
                        statusTick.classList.add("bi-check2-all");
                        statusTick.classList.add("read");
                    }                    
                    statusTick.classList.add("check-status");

                    statusContainer.appendChild(statusTick);
                    var outgoingContainer = document.createElement("div");
                    outgoingContainer.classList.add("outgoing");

                    var outgoingMessagesContainer = document.createElement("div");
                    outgoingMessagesContainer.classList.add("outgoing-messages");

                    var outgoingMessageContentContainer = document.createElement("div");
                    outgoingMessageContentContainer.classList.add("outgoing-message-content")

                    messageBodyContainer.append(outgoingContainer);
                    outgoingContainer.append(conversationBodyContainer);
                    conversationBodyContainer.append(outgoingMessagesContainer);
                    outgoingMessagesContainer.append(outgoingMessageContentContainer);
                    messageInfoContainer.append(statusContainer, timeSentContainer);
                    outgoingMessageContentContainer.append(messageContainer, messageInfoContainer);
                }
            })
            messageBodyContainer.scrollTo(0, messageBodyContainer.scrollHeight);
        })
    }))
}

function checkRecentMessages() {
    var unreadMessages = document.querySelectorAll(".number-of-messages");
    if (unreadMessages.length > 0) {
        var userPhoneNumber = '';
        $.ajax({
            url: '/?handler=PhoneNumber',
            method: 'GET',
        }).done(result => {
            userPhoneNumber = result;
            connection.invoke("UpdateOnLogin", userPhoneNumber).catch(function (err) {
                return console.error(err.toString());
            });
        });
    }
}
//closing open chat
document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape' || event.keyCode === 27) {
        var chatContainer = document.querySelector(".chat");
        if (!chatContainer.classList.contains("hidden")) {
            chatContainer.classList.add("hidden");
        }
    }
});

class Message {
    constructor(
        recieverPhoneNumber,
        context
    ) {
        this.ReceiverPhoneNumber = recieverPhoneNumber;
        this.Context = context;
    }
}