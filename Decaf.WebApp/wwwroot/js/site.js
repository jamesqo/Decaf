var input, output;

function onSubmitClick() {
    var query = "api/convert/?javaCode=" + encodeURIComponent(input.getValue());
    getUrl(query, loadResults);
}

function changeSubmitMessage() {
    var submitButton = document.getElementById("submitButton");
    var messages = [
        "Extract Cocoa Beans",
        "Decrease Work Productivity",
        "Sharpen The Mind",
        "Reduce Energy Levels",
        "Prevent Afternoon Sleepiness",
        "Brew Your Soul",
        "Increase Longevity"
    ];

    var currentMessage = submitButton.value;
    var newMessage;

    do {
        var randIndex = Math.floor(Math.random() * messages.length);
        newMessage = messages[randIndex];
    } while (newMessage === currentMessage);

    submitButton.value = newMessage;
}

function getUrl(url, callback) {
    enableSubmit(false);
    var xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.setRequestHeader("Accept", "text/html");
    xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
            var data = xhr.responseText;
            if (typeof data === "string" && data.length > 0) {
                callback(data);
            }
        }

        enableSubmit(true);
    };
    xhr.send();
    return xhr;
}

function enableSubmit(enabled) {
    var submitButton = document.getElementById("submitButton");
    submitButton.disabled = !enabled;
}

function setResult(data) {
    output.setValue(data);
}

function loadResults(data) {
    setResult(data);
    changeSubmitMessage();
}

input = ace.edit("input");
input.setOptions({
    highlightActiveLine: false,
    highlightGutterLine: false,
    maxLines: Infinity,
    mode: "ace/mode/java",
    showGutter: false,
    theme: "ace/theme/visualstudio"
});

output = ace.edit("output");
output.setOptions({
    highlightActiveLine: false,
    highlightGutterLine: false,
    maxLines: Infinity,
    mode: "ace/mode/csharp",
    readOnly: true,
    showGutter: false,
    theme: "ace/theme/visualstudio"
});
output.renderer.$cursorLayer.element.style.display = "none"; // Disables the cursor
