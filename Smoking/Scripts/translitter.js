
var prevVal = '';
var prevValTrans = '';

function autofill(source, target) {
    $(document).keypress(function() {
        setTimeout(function() {
            if (target.val() == source.val().substring(0, source.val().length - 1))
                target.val(source.val());
            if (source.val() == target.val().substring(0, target.val().length - 1))
                target.val(source.val());
        }, 1);


    }).keyup(function() {
        if (!target.val().length && source.val().length) {
            target.val(source.val());
        }
    });
}

function transliterate(source, target) {
    $(document).keyup(function () {
        if (source.val() != prevVal && (prevValTrans == target.val() || prevValTrans.indexOf(target.val())>=0)) {
            target.val(transliterateWord(source.val()));
        }
//        console.log(prevVal);

    }).keypress(function () {
        
    }).keydown(function () {
        prevVal = source.val();
        prevValTrans = transliterateWord(prevVal);
        
    });

}


function transliterateWord(str) {
    var alphabetMap = { а: 'a', б: 'b', в: 'v', г: 'g', д: 'd', е: 'e', ё: 'yo', ж: 'zh', з: 'z', и: 'i', й: 'i', к: 'k', л: 'l', м: 'm', н: 'n', о: 'o', п: 'p', р: 'r', с: 's', т: 't',у: 'u', ф: 'f', х: 'kh',ц: 'ts', ч: 'ch', ш: 'sh', щ: 'sch', ы: 'y', э: 'e', ю: 'yu', я: 'ya' };

    var result = $.map(
        str.split(''),
        function (el, idx) {
            if (/\w/i.test(el)) {
                return el;
            }
            if (/\s/.test(el)) {
                return '_';
            }
            if (alphabetMap[el.toLowerCase()]) {
                return alphabetMap[el.toLowerCase()];
            }
            return '';
        });

    return result.join('');
}