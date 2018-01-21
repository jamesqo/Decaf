var HashHandler = ace.require("ace/keyboard/hash_handler").HashHandler;

const KEYDOWN_FLUSH_THRESHOLD = 5;

var input, output;
var keydownsToFlush = 0;

function convertCode() {
    keydownsToFlush = 0;

    var query = "api/convert/?javaCode=" + encodeURIComponent(input.getValue());
    getUrl(query, loadResults);
}

function getUrl(url, callback) {
    enableSubmit(false);
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
}

function recordKeydown(editor) {
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
input.keyBinding.addKeyboardHandler(new HashHandler({
    exec: recordKeydown
}));

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
