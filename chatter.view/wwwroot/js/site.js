var click = document.querySelector('.click');
var profile = document.querySelector('.profile-details');
var chats = document.querySelectorAll('.chat-blog');

click.addEventListener('click', function () {
    profile.classList.toggle('show');
});

document.addEventListener('click', function (event) {
    const isClickInsideClickElement = click.contains(event.target);
    const isClickInsideProfileElement = profile.contains(event.target);

    if (!isClickInsideClickElement && !isClickInsideProfileElement) {
        profile.classList.remove('show');
    }
});

chats.forEach(chat => {
    chat.addEventListener('click', function () {
        // Remove 'show' class from all chats
        chats.forEach(otherChat => {
            if (otherChat !== chat) {
                otherChat.classList.remove('active');
            }
        });

        // Toggle 'show' class for the clicked chat
        chat.classList.toggle('active');
    });
});
