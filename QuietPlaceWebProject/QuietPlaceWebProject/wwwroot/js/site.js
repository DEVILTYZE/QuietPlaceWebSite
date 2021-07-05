// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// NOTIFICATION
function sendNotification(title, options) {
    // Проверка поддержки браузером уведомлений
    if (!("Notification" in window)) {
        alert("This browser does not support desktop notification");
    }

    // Проверка разрешения на отправку уведомлений
    else if (Notification.permission === "granted") {
        // Если разрешено, то создаём уведомление
        let notification = new Notification(title, options);
    }

    // В противном случае, запрашиваем разрешение
    else if (Notification.permission !== 'denied') {
        Notification.requestPermission(function (permission) {
            // Если пользователь разрешил, то создаём уведомление
            if (permission === "granted") {
                let notification = new Notification(title, options);
            }
        });
    }

    // В конечном счёте, если пользователь отказался от получения
    // уведомлений, то стоит уважать его выбор и не беспокоить его
    // по этому поводу.
}

// CARET POSITION
function setSelectionRange(input, selectionStart, selectionEnd) {
    if (input.setSelectionRange) {
        input.focus();
        input.setSelectionRange(selectionStart, selectionEnd);
    }
    else if (input.createTextRange) {
        let range = input.createTextRange();
        range.collapse(true);
        range.moveEnd('character', selectionEnd);
        range.moveStart('character', selectionStart);
        range.select();
    }
}

function setCaretToPos (input, pos) {
    setSelectionRange(input, pos, pos);
}

// CHANGE CLASS FOR THE ELEMENT
function changeClass(text, className) {
    if (text.classList.contains(className)) {
        text.classList.remove(className);
        return;
    }

    text.classList.add(className);
}

// COUNTER OF REMAINING SYMBOLS
function countSymbols(textForm) {
    let counter = $('#countOfSymbols');
    let textlength;
    
    try {
        textlength = textForm.value.length;
    } catch (err) {
        textlength = textForm.val().length;
    }
    
    counter.text(5000 - textlength);
}

// SET TEXT TAGS FOR TEXT
function setTextTag(tagStart, tagEnd) { // РАБОТАЕТ, НАКОНЕЦ-ТО! АХХАХАХАХА
    let textForm = document.getElementById('textPost');
    textForm.focus();
    
    let text = textForm.value;
    let startOffset = textForm.selectionStart ?? 0, endOffset = textForm.selectionEnd ?? 0;
    let textBefore = text.substr(0, startOffset), textInside = text.substr(startOffset, 
        endOffset - startOffset), textAfter = text.substr(endOffset);
    textForm.value = textBefore + tagStart + textInside + tagEnd + textAfter;
    
    // console.log("-------------------------------------------------------------------------");
    // console.log("text: |" + text + "|");
    // console.log("startOffset: " + startOffset);
    // console.log("endOffset: " + endOffset);
    // console.log("textBefore: |" + textBefore + "|");
    // console.log("textInside: |" + textInside + "|");
    // console.log("textAfter: |" + textAfter + "|");
    // console.log("newText: |" + newText + "|");
    
    // let selection = window.getSelection(), range = selection.getRangeAt(0);
    // let selectedText = range.toString();
    // let startNode = range.startContainer, endNode = range.endContainer;
    
    // let newText = startNode.textContent.substr(0, range.startOffset) + tagStart + selectedText +
    //     endNode.textContent.substr(range.endOffset);
    // textForm.text(newText);
    
    // selection.removeAllRanges();
    
    setCaretToPos(textForm, tagStart.length + startOffset);
    countSymbols(textForm);
}