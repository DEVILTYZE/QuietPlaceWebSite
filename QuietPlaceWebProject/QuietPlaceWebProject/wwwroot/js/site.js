// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

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

function changeClass(text, setClassName, removeClassName) {
    if (removeClassName.length > 0) {
        text.classList.remove(removeClassName);
    }

    if (setClassName.length > 0) {
        text.classList.add(setClassName);
    }
}

function countSymbols(textForm) {
    let counter = $('#countOfSymbols');
    let textLength = 0;
    let textlength;
    
    try {
        textlength = textForm.value.length;
    } catch {
        textlength = textForm.val().length;
    }
    
    counter.text(5000 - textlength);
}

function setTextTag(tagStart, tagEnd) { // РАБОТАЕТ, НАКОНЕЦ-ТО! АХХАХАХАХА
    let textForm = document.getElementById('textPost');
    textForm.focus();
    
    let text = textForm.value;
    let startOffset = textForm.selectionStart ?? 0, endOffset = textForm.selectionEnd ?? 0;
    let textBefore = text.substr(0, startOffset), textInside = text.substr(startOffset, 
        endOffset - startOffset), textAfter = text.substr(endOffset);
    let newText = textBefore + tagStart + textInside + tagEnd + textAfter;
    textForm.value = newText;
    
    console.log("-------------------------------------------------------------------------");
    console.log("text: |" + text + "|");
    console.log("startOffset: " + startOffset);
    console.log("endOffset: " + endOffset);
    console.log("textBefore: |" + textBefore + "|");
    console.log("textInside: |" + textInside + "|");
    console.log("textAfter: |" + textAfter + "|");
    console.log("newText: |" + newText + "|");
    
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

function loadFile(event) {
    let data = event.dataTransfer.getData("text");
    alert(data);
}