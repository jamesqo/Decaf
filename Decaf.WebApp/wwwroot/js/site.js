var event = ace.require("ace/lib/event");

const KEYDOWN_FLUSH_THRESHOLD = 5;

var input, output;
var keydownsToFlush = 0;

function convertCode() {
    keydownsToFlush = 0;

    var query = "api/convert/?javaCode=" + encodeURIComponent(input.getValue());
    getUrl(query, loadResults);
}

function getUrl(url, callback) {
    // TODO: Indicate you are busy.
    var xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.setRequestHeader("Accept", "text/html");
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            var data = xhr.responseText;
            if (typeof data === "string" && data.length > 0) {
                callback(data);
            }
        }

        // TODO: Indicate you are no longer busy.
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
}

function onInputKeydown(e) {
    keydownsToFlush++;
    if (keydownsToFlush === KEYDOWN_FLUSH_THRESHOLD) {
        convertCode();
    }
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
var el = input.textInput.getElement();
event.addListener(el, "keydown", onInputKeydown);

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
