
class Pair {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
}

function getArrows(text) {
    let index = 0;
    let last = -10;
    let array = [];

    while (index < text.length()){
        index = text.indexOf('>', index);

        if (index === last + 1) {
            let numberIndex = getNumbers(index, text);

            array.push(new Pair(index, numberIndex));
        }

        last = index;
        ++index;
    }

    return array;
}

function getNumbers(index, text) {
    let endIndex = index + 1;

    while (typeof(text[endIndex]) === 'number')
        ++endIndex;

    return endIndex;
}

let input = document.getElementById("textPost");
let inputValue = input.value;
let indexOfArrows = getArrows(inputValue);

for (let i = 0; i < indexOfArrows.length; ++i) {
    let str = inputValue.substr(indexOfArrows[i].x, indexOfArrows[i].y);
    
    inputValue = inputValue.replace(str, '<span style=\"color: orange\">${str}</span>')
}

// let text = inputValue.match(/>>\d*/g);
//
// text.forEach(function(item, i, text) {
//     inputValue.innerHTML = inputValue.innerHTML.replace(item, '<span style=\"color: orange\">${item}</span>');  
// });