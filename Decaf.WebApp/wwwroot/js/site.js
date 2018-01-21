var event = ace.require("ace/lib/event");

const KEYDOWN_FLUSH_THRESHOLD = 5;

var input, output;
var keydownsToFlush = 0;

function buildQueryString(params) {
    var pairs = Object.keys(params).map(function (key) {
        return key + "=" + encodeURIComponent(params[key]);
    });
    return pairs.join("&");
}

function convertCode() {
    keydownsToFlush = 0;

    var params = {
        javaCode: input.getValue(),
        csharpLanguageVersion: getCsharpLangVersion(getValue("csharpLangVersion")),
        translateCollectionTypes: getCheckboxValue("translateCollectionTypes"),
        unqualifyTypeNames: getCheckboxValue("unqualifyTypeNames"),
        useVarInDeclarations: getCheckboxValue("useVarInDeclarations"),
    };

    var query = buildQueryString(params);
    var url = "api/convert/?" + query;
    console.log(url);
    getUrl(url, loadResults);
}

function getCheckboxValue(id) {
    return document.getElementById(id).checked;
}

function getCsharpLangVersion(sliderValue) {
    var langVersions = [
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "7.1",
        "7.2",
        "latest"
    ];
    return langVersions[sliderValue];
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

function getValue(id) {
    return document.getElementById(id).value;
}

function loadResults(data) {
    setResult(data);
}

function onCsharpLangVersionSlide(slideEvent) {
    var label = document.getElementById("csharpLangVersionLabel");
    label.textContent = "C# language version: " + getCsharpLangVersion(slideEvent.value);
}

function onInputKeydown(e) {
    keydownsToFlush++;
    if (keydownsToFlush === KEYDOWN_FLUSH_THRESHOLD) {
        convertCode();
    }
}

function onOptionValueChanged() {
    convertCode();
}

function setResult(data) {
    // Passing -1 positions the (invisible) cursor for 'output' at the start.
    // It ensures that the div is always scrolled to the left as much as possible, and prevents the text of the div from being highlighted.
    output.setValue(data, -1);
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
input.$blockScrolling = Infinity; // Disables some warning
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
output.$blockScrolling = Infinity; // Disables some warning
output.renderer.$cursorLayer.element.style.display = "none"; // Disables the cursor

// TODO: Don't use jQuery here.
$("#csharpLangVersion").slider()
    .on("slide", onCsharpLangVersionSlide)
    .on("slideStop", onOptionValueChanged);
