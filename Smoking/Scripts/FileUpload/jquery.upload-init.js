
$(document).ready(function () {
    initAutoFileUpload();
    initClient();

});

function initAutoFileUpload() {
    'use strict';
    $('.delete-img-btn').click(function () {
        var cell = $(this).parent();
        if (cell.attr('oldfileexist') == '1') {
            if (cell.find('img').attr('src') == cell.attr('oldfile')) {
                $.get(cell.attr('clear-handler'), function () {
                    cell.find('img').attr('src', '');
                    cell.hide();
                    cell.parent().find('input[type="hidden"]').val('');
                });
            } else {
                cell.find('img').attr('src', cell.attr('oldfile'));
                cell.parent().find('input[type="hidden"]').val('');
            }
        } else if (cell.attr('oldfileexist') == '0') {
            cell.find('img').attr('src', '');
            cell.hide();
            cell.parent().find('input[type="hidden"]').val('');
        } else {
            $.get(cell.attr('clear-handler'), function () {
                cell.find('img').attr('src', '');
                cell.hide();
                cell.parent().find('input[type="hidden"]').val('');
            });
        }
        return false;
    });
    $('.db-img-upload').each(function () {
        var fu = $(this);
        fu.fileupload({
            autoUpload: true,
            url: '/Master/ru/UniversalEditor/UploadFile?fileColumn=' + fu.attr('id'),
            dataType: 'json',
            add: function (e, data) {
                var jqXHR = data.submit()
                    .success(function (data, textStatus, jqXHR) {
                        if (data.isUploaded) {
                            try {
                                $("#" + data.filedName + "_Path").val(data.path);
                                var cell = $("#" + data.filedName + "_Preview");

                                cell.attr('oldfile', cell.find('img').attr('src'));
                                if (!cell.attr('oldfileexist')) {
                                    cell.attr('oldfileexist', cell.is(':hidden') ? "0" : "1");
                                }
                                cell.find('img').attr('src', data.path);
                                cell.find('a').attr('href', '');
                                cell.show();
                            } catch (ex) {
                                alert(ex);
                            }
                        }
                        else {
                            alert(data.message);
                        }

                    })
                    .error(function (data, textStatus, errorThrown) {
                        if (typeof (data) != 'undefined' || typeof (textStatus) != 'undefined' || typeof (errorThrown) != 'undefined') {
                            alert(textStatus + errorThrown + data);
                        }
                    });
            },
            fail: function (event, data) {
                if (data.files[0].error) {
                    alert(data.files[0].error);
                }
            }
        });
    });


}


function initClient() {

    $('#ZonePhotoUploader').each(function () {
        var fu = $(this);
        fu.fileupload({
            autoUpload: true,
            url: '/Master/ru/GoogleMap/UploadFile?fileColumn=' + fu.attr('id'),
            dataType: 'json',
            add: function (e, data) {


                $('#message').html('Загрузка изображения...');

                var jqXHR = data.submit()
                    .success(function (data, textStatus, jqXHR) {
                        if (data.isUploaded) {
                            $('#message').html('Изображение загружено на сервер');
                            $('#ZonePhoto').val(data.path);
                            $('#previewImg').attr('src', data.path);
                            $('#previewImg').parent().show();
                            if (currentPoint) {
                                if (!currentPoint.UserData) {
                                    saveFieldsInMarker(currentPoint.ib);
                                }
                                currentPoint.UserData.ZonePhoto = data.path;
                            }
                            autoSave();
                        } else {
                            $('#message').html(data.message);
                        }

                    })
                    .error(function (data, textStatus, errorThrown) {
                        if (typeof (data) != 'undefined' || typeof (textStatus) != 'undefined' || typeof (errorThrown) != 'undefined') {
                            jAlert("onerror:" + textStatus + errorThrown + data);
                        }
                    });
            },
            fail: function (event, data) {
                if (data.files[0].error) {
                    jAlert("onfail:" + data.files[0].error);
                }
            }
        });
    });

    $('#UserPhotoUploader').each(function () {
        var fu = $(this);
        fu.fileupload({
            autoUpload: true,
            url: '/Master/ru/Users/UploadAvatar?uid=' + fu.parents('.photo').find('> a').attr('uid'),
            dataType: 'json',
            add: function (e, data) {


                $('#message').html('Загрузка изображения...');

                var jqXHR = data.submit()
                    .success(function (data, textStatus, jqXHR) {
                        if (data.isUploaded) {
                            $('#message').html('Изображение загружено на сервер');
                            var src = $('.photo > a > img').attr('src');
                            src += '&rnd=' + new Date().getTime();
                            $('.photo > a > img').attr('src', src);
                            $.fancybox.close();
                        } else {
                            $('#message').html(data.message);
                        }

                    })
                    .error(function (data, textStatus, errorThrown) {
                        if (typeof (data) != 'undefined' || typeof (textStatus) != 'undefined' || typeof (errorThrown) != 'undefined') {
                            jAlert("onerror:" + textStatus + errorThrown + data);
                        }
                    });
            },
            fail: function (event, data) {
                if (data.files[0].error) {
                    jAlert("onfail:" + data.files[0].error);
                }
            }
        });
    });

}