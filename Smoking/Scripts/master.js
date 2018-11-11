$().ready(function () {
    loadTreeFilter();
    //loadEditable();
    loadSwitcher();
    loadOdds();
    disableBoxes();
    loadOrderBoxes();
    loadLangSwitcher();
    setRedirect();
    initFiltersDDL();
    initCalendars();
    initEditors();
});


function loadEditor(id) {
    var instance = CKEDITOR.instances[id];
    if (instance) {
        CKEDITOR.remove(instance);
    }
    CKEDITOR.replace(id, {
        filebrowserBrowseUrl: '/Content/ckeditor/ckfinder/ckfinder.html',
        filebrowserImageBrowseUrl: '/Content/ckeditor/ckfinder/ckfinder.html?type=Images',
        filebrowserFlashBrowseUrl: '/Content/ckeditor/ckfinder/ckfinder.html?type=Flash',
        filebrowserUploadUrl: '/Content/ckeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files',
        filebrowserImageUploadUrl: '/Content/ckeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images',
        filebrowserFlashUploadUrl: '/Content/ckeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash'
    });
}

function initEditors() {
    $('.text-editor').each(function () {

        loadEditor($(this).attr('ID'));
/*
        CKEDITOR.replace($(this).attr('ID'), {
            filebrowserBrowseUrl: '/Content/ckeditor/ckfinder/ckfinder.html',
            filebrowserImageBrowseUrl: '/Content/ckeditor/ckfinder/ckfinder.html?type=Images',
            filebrowserFlashBrowseUrl: '/Content/ckeditor/ckfinder/ckfinder.html?type=Flash',
            filebrowserUploadUrl: '/Content/ckeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files',
            filebrowserImageUploadUrl: '/Content/ckeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images',
            filebrowserFlashUploadUrl: '/Content/ckeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash'
        });
*/

    });
}

/*
function initUploadify() {
    $('#file_upload').uploadify({
        'formData': {
            'SyndicateID': $('#SyndicateID').val()
        },
        'buttonText': 'Выберите файл',
        'swf': '/Scripts/Uploadify/uploadify.swf',
        'uploader': '/Master/ru/MainPage/SyndicateTicketUpload?SyndicateID=' + $('#SyndicateID').val(),
        'onUploadComplete': function (file) {
            setTimeout(function () {
                $.get('/Master/ru/MainPage/SyndicateTicket', { SyndicateID: $('#SyndicateID').val() }, function (data) {
                    $('#UploadForm').html(data);
                    initUploadify();
                });
            }, 500);
        }
    });

    $('.deleter').click(function () {
        $.get('/Master/ru/MainPage/SyndicateTicketDelete', { SyndicateID: $('#SyndicateID').val() }, function (data) {
            $('#UploadForm').html(data);
            initUploadify();
        });
        return false;
    });
}
*/

function ajaxComplete() {
    setRedirect();
    initCalendars();
    initEditors();
}

function initCalendars() {
    $.datepicker.regional['ru'] = {
        closeText: 'Закрыть',
        prevText: '<Пред',
        nextText: 'След>',
        currentText: 'Сегодня',
        monthNames: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
        'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'],
        monthNamesShort: ['Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн',
        'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек'],
        dayNames: ['воскресенье', 'понедельник', 'вторник', 'среда', 'четверг', 'пятница', 'суббота'],
        dayNamesShort: ['вск', 'пнд', 'втр', 'срд', 'чтв', 'птн', 'сбт'],
        dayNamesMin: ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'],
        weekHeader: 'Не',
        dateFormat: 'dd.mm.yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['ru']);


    $.timepicker.regional['ru'] = {
        timeOnlyTitle: 'Выберите время',
        timeText: 'Время',
        hourText: 'Часы',
        minuteText: 'Минуты',
        secondText: 'Секунды',
        millisecText: 'Миллисекунды',
        timezoneText: 'Часовой пояс',
        currentText: 'Текущее',
        closeText: 'Закрыть',
        timeFormat: 'HH:mm',
        amNames: ['AM', 'A'],
        pmNames: ['PM', 'P'],
        isRTL: false
    };
    $.timepicker.setDefaults($.timepicker.regional['ru']);
    $('input[rel="calendar"]').datetimepicker({
        dateFormat: "dd.mm.yy",
        timeFormat: "HH:mm",
    });

    $('input[type="datetime"]').datepicker({
        dateFormat: "dd.mm.yy"
    });
}

function initFiltersDDL() {
    var cells = $('#AutoFilterTable select, #AutoFilterTable input');
    var base = $('#BaseURL').val();
    cells.change(function () {
        var params = "";
        var main = cells.filter('[main="1"]');
        if (main.length && main.attr('id') == $(this).attr('id')) {
            main.each(function() {

                if ($(this).val().length) {
                    if (params.length)
                        params += "&";
                    params += $(this).attr('id') + "=" + $(this).val();
                }
            });

        } else {
            cells.each(function() {

                if ($(this).val().length) {
                    if (params.length)
                        params += "&";
                    params += $(this).attr('id') + "=" + $(this).val();
                }
            });
        }
        if (params) {
            if (base.indexOf("?") >= 0)
                base = base + "&" + params;
            else base = base + "?" + params;

            document.location.href = base;
        }

    });
}

function loadLangSwitcher() {
    $('#MasterLang').change(function () {
        $.cookie('MasterLang', $('#MasterLang').val(), { expires: 365, path: '/' });
        document.location.href = '/Master/' + $('#MasterLang').val() + '/' + $('#MasterLangURL').val();
    });
}

function loadOrderBoxes() {
    $('input[box="orderbox"]').change(function () {
        var box = $(this);

        var link = '/Master/ru/' + $(this).attr('target') + "/" + $(this).attr('action');

        
        var query = document.location.search.slice(1);
        if ($(this).attr('oldval') != $(this).val()) {
            $.post(link, { id: $(this).attr('arg'), value: $(this).val(), page: $(this).attr('page'), tablename: $(this).attr('tablename'), uidname: $(this).attr('uidname'), ordername: $(this).attr('ordername'), cc: $(this).attr('cc'), ca: $(this).attr('ca'), addqs: $(this).attr('addqs'), query:query }, function (d) {
                if (d.length) {
                    var table = box.parent().parent().parent();
                    table.find('tr').each(function () {
                        if (!$(this).find('th').length)
                            $(this).remove();
                    });
                    table.append(d);
                    loadOrderBoxes();
                }
            }, "html");
        }
    });
}


function setRedirect() {
    var cell = $('input[type="hidden"]').filter('#RedirectURL');
    if (cell.length) {
        var val = cell.val();
        if (val.length) {
            document.location.href = val;
        }
    }
}

function disableBoxes() {
    $('input[inactive="1"], select[inactive="1"]').attr('disabled', 'disabled');
}

var filterDataLink = "/Master/ru/Pages/getTreeData";
var saveFieldDataLink = "/Master/ru/Catalog/SaveField";



function loadOdds() {
    $('.odd-grid tr:odd').addClass('odd');
}



function loadSwitcher() {
    $('.switcher').click(function () {
        $('.switcher-content').toggle();
        var currentSwitch = $.cookie('switcher');
        if (currentSwitch == null)
            currentSwitch = '1';

        if (currentSwitch == '0')
            currentSwitch = '1';
        else currentSwitch = '0';
        $.cookie('switcher', currentSwitch);
        return false;
    });

    var currentSwitch = $.cookie('switcher');
    if (currentSwitch == null)
        currentSwitch = '1';

    if (currentSwitch == '0')
        $('.switcher-content').hide();
    else $('.switcher-content').show();

}

function loadEditable() {
    $('.editable').click(function () {
        if ($(this).hasClass('editing')) return false;
        $('.editable').filter('.editing').each(function () {
            $(this).html($(this).attr('val'));
            $(this).removeClass('editing');
        });
        $(this).html('<div class="cell"><input type="text" value="' + $(this).attr('val') + '"></div><div class="btns"><a class="accept" href="/"/><a class="cancel" href="/"></a></div>');
        $(this).addClass('editing');
        $('.editing .cancel').click(function () {
            var cell = $(this).parents('.editing');
            cell.html(cell.attr('val'));
            cell.removeClass('editing');
            return false;
        });
        $('.editing .accept').click(function () {
            var cell = $(this).parents('.editing');
            $.post(saveFieldDataLink, { field: cell.attr('target'), id: cell.attr('targetId'), value: cell.find('input').val() }, function (d) {
                if (d.length) {
                    cell.attr('val', d);
                    cell.html(cell.attr('val'));
                    cell.removeClass('editing');
                }
            });
            return false;
        });
    });
}


var changing = false;
function loadTreeFilter() {
    try {

        if ($('#tree-filter').length) {
            var argList = $('#PageListPlain').val().split(';');
            $.getJSON(filterDataLink, {}, function (res) {


                $('#tree-filter').jstree({
                    "plugins": [
                        "themes", "json_data", "ui", "cookies", "dnd", "search", "types", "checkbox"
                    ],

                    "cookies": {
                        "save_opened": "js_tree_catalog_filter",
                        "cookie_options": { expires: 365 }
                    },

                    "checkbox": {
                        "two_state": false
                    },

                    "themes": {
                        "theme": "apple",
                        "url": "/Content/themes/apple/style.css"
                    },

                    "json_data": {
                        "data": res,
                        "progressive_render": true
                    }
                }).bind("change_state.jstree", function (e, d) {
                    if (changing) return false;
                    var single = false;
                    try {
                        single = singleSelection;
                    } catch (e) {
                    }
                    var sections = $('#tree-filter').jstree("get_checked", null, true);

                    if (single) {
                        //console.log(d);
                        changing = true;
                        var current = d.rslt[0];
                        $('#tree-filter').jstree('uncheck_all');
                        $('#tree-filter').jstree('check_node', current);
                        changing = false;
                        sections = $('#tree-filter').jstree("get_checked", null, true);
                        //console.log(e);

                    }

                    var sectionPlain = '';
                    sections.each(function () {
                        sectionPlain += $(this).attr('id').replace('x', '') + ";";
                    });
                    $('#PageListPlain').val(sectionPlain);
                }).bind("loaded.jstree", function (event, data) {

                    for (var i = 0; i < argList.length; i++) {
                        $('#tree-filter').jstree("check_node", 'x' + argList[i]);
                    }

                }).bind("open_node.jstree", function (e, data) {
                    for (var i = 0; i < argList.length; i++) {
                        $('#tree-filter').jstree("check_node", '#x' + argList[i]);
                    }
                });
            });
        }
    }
    catch (exc) {

    }
}