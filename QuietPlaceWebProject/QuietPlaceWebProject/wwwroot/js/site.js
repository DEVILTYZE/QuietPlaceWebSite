// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function spoiler(text) {
    if (text.classList.contains('unspoiler')) {
        text.classList.remove('unspoiler');
    }

    text.classList.add('spoiler');
}

function unspoiler(text) {
    if (text.classList.contains('spoiler')) {
        text.classList.remove('spoiler');
    }

    text.classList.add('unspoiler');
}

function countSymbols(textForm) {
    let counter = $('#countOfSymbols');
    counter.text(5000 - textForm.value.length);
}

function setTextTag(tagStart, tagEnd) { // Не работает мразина... :с
    let textForm = $('#textPost');
    
    let selectedText = window.getSelection(); 
    let range = selectedText.getRangeAt(0);
    
    textForm.text(
        textForm.val().slice(0, range.startOffset) + 
        tagStart + 
        selectedText.toString() +
        tagEnd +
        textForm.val().slice(range.endOffset)
    );
}