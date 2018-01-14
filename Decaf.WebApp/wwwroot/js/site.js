function onSubmitClick() {
    var inputBox = document.getElementById("inputBox");
    var query = "api/convert/?javaCode=" + encodeURIComponent(inputBox.value);
    getUrl(query, loadResults);
}

function getUrl(url, callback) {
    enableSubmit(false);
    var xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.setRequestHeader("Accept", "text/html");
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4) {
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
    var container = document.getElementById("outputDiv");
    if (container) {
        container.innerHTML = data;
    }
}

function loadResults(data) {
    setResult(data);
}
