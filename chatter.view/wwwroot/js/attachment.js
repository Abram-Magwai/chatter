var imageInput = document.getElementById("image-input");
var fileInput = document.getElementById("file-input");
var imageAttachment = document.querySelector(".image-attachment");
var fileAttachment = document.querySelector(".file-attachment");

imageAttachment.addEventListener("click", () => {
    imageInput.click();
});

fileAttachment.addEventListener("click", () => {
    fileInput.click();
});

imageInput.addEventListener("change", (event) => {
    var file = event.target.files[0];
    var fileName = file.name;
});

fileInput.addEventListener("change", (event) => {
    var file = event.target.files[0];
    var fileName = file.name;
});