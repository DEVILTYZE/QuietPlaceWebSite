
inputForm = document.getElementById('inputForm');

inputForm.ondrop = function (event) {
    let data = event.dataTransfer.getData('text');
    alert(data);
}