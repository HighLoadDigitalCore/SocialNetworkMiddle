$(function () {
	resizeMap();
	gmapsInit();
    multiselect();
    carousel();
    dropdowns();
    loadNews();
    loadTabs();
    openWindow();
    forms();
    select();
    copySocials();
    syncObjectFilters();
    editorButtons();
    mapcontrols();
    editorControls();
    newsFavorite();
    $('.form-login input:text').keyup(function (event) {
        var btn = $(this).closest('.form-login').find('.-submit');

        if (event.keyCode == 13) {//Enter
            btn.click();
        }
    });
    profileselect();
    $('.dropdown-list a').click(function () {
        document.location.href = $(this).attr('href');
			if (document.location.href.indexOf('/map') >= 0) {
            document.location.reload();
        }
        return true;
    });
    
/*
    if ($('form[data-ajax-update="#SimpleRegister"]').length > 1) {
        $('.profile-name #enter').remove();
    }
*/

});


var currentPoint;
var filterData;
var loadedMarkerList;


function profileselect() {
    if (!$('#SmokingType').length)
        return;
    if ($('#SmokingType').val().length) {
        var inp = $('.profileselect input[arg="' + $('#SmokingType').val() + '"]');
        inp.prop('checked', 'checked');
        inp.parent().addClass('-active');
    }

    $('.profileselect input[type="radio"]').change(function () {
        if ($(this).is(':checked')) {
            $('#SmokingType').val($(this).attr('arg'));
        }
    });
}

function newsFavorite() {
    $('.stream .icon-star').parent().click(function () {
        var a = $(this);
        $.post('/Master/ru/Lenta/ToggleFavorite', { id: $(this).attr('arg') }, function (d) {
            if (d.length) {
                jAlert(d, "Произошла ошибка");
                return false;
            }
            a.toggleClass('-active');
            var num = parseInt($.trim(a.parent().find('span').text()));
            if (a.hasClass('-active')) {
                num++;
            } else {
                num--;
            }
            a.parent().find('span').html(num);
        });
        return false;
    });
}


function clearPoint(point) {
    if (!point)
        point = currentPoint;

    closeBox(point.ib);
    point.setMap(null);
    if (point.polygon) {
        point.polygon.setMap(null);
    }
}


function addExistMarkers(list, filterStable) {

    function contains(a, obj) {
        for (var i = 0; i < a.length; i++) {
            if (a[i] === obj) {
                return true;
            }
        }
        return false;
    }

    if (!filterStable && loadedMarkerList) {
        for (var k = 0; k < loadedMarkerList.length; k++) {
            if (loadedMarkerList[k].polygon)
                loadedMarkerList[k].polygon.setMap(null);

            loadedMarkerList[k].setMap(null);
        }
        loadedMarkerList = new Array();
    }

    var existIds = getLoadedIds();



    for (var i = 0; i < list.length; i++) {
        var d = list[i];


        if (contains(existIds, d.ID)) {
            continue;
        }
        if (currentPoint && currentPoint.ib.ObjectID == d.ID) {
            continue;
        }


        var markUp = createMarkup(d.IsRegion ? 3 : 2, '', d);


        var icon = '/content/client/i/markers/';

        if (d.IsMyFavorite) {
            icon += 'marker2.png';
        } else {
            icon += "icon-obj-small" + d.IconNum + ".png";
            /*if (d.SmokingType == -1)
                icon += "marker1.png";
            if (d.SmokingType == 0)
                icon += "marker3.png";
            if (d.SmokingType == 1)
                icon += "marker4.png";*/
        }


        var m = new google.maps.Marker({
            map: map,
            position: new google.maps.LatLng(d.PointPosition.Lat, d.PointPosition.Lng),
            visible: true,
            icon: icon,
            draggable: false,
            zIndex: 2000
        });

        m.ibc = markUp;




        if (!loadedMarkerList)
            loadedMarkerList = new Array();



        var ibOptions = {
            content: markUp,
            alignBottom: false,
            pixelOffset: new google.maps.Size(24, -71),
            infoBoxClearance: new google.maps.Size(10, 55)
        };

        var box = new InfoBox(ibOptions);

        box.PointType = d.IsRegion ? 3 : 2;
        box.marker = m;
        m.ib = box;
        m.ObjectID = d.ID;



        if (d.IsRegion) {


            var color = '#ff0000';
            if (d.SmokingType == 0)
                color = '#000000';
            if (d.SmokingType == 1) {
                color = '#00ff00';
            }

            var polygone = new Array();
            for (var j = 0; j < d.RegionPosition.length; j++) {
                polygone.push(new google.maps.LatLng(d.RegionPosition[j].Lat, d.RegionPosition[j].Lng));
            }


            var defPolygon = new google.maps.Polygon({
                paths: polygone,
                fillColor: color,
                fillOpacity: 0.35,
                strokeWeight: 2,
                strokeColor: color,
                strokeOpacity: 0.7,
                clickable: false,
                editable: false,
                zIndex: 1000,
                draggable: false
            });
            defPolygon.marker = m;
            m.polygon = defPolygon;

            defPolygon.setMap(map);
        }

        with ({ m: m }) {
            google.maps.event.addListener(m, 'click', function () {
                closeExcept(m.ObjectID);
                toggleMarkerBox(m);
                openBoxTimeout = setTimeout('console.log("moved")', openBoxTimeoutDelay);
                
            });
        }



        google.maps.event.addListener(box, 'domready', function () {
            forms();
            select();
            initClient();
        });

        loadedMarkerList.push(m);



    }
}

function closeExcept(id) {
    if (loadedMarkerList) {
        for (var i = 0; i < loadedMarkerList.length; i++) {
            if (loadedMarkerList[i].ObjectID != id) {
                closeBox(loadedMarkerList[i].ib);
            }
        }
    }
}

function editorButtons() {
    $('.editor-asidecontrols a[action], .-actionbtn[action]').click(function () {
        closeAllPopups();
        var a = $(this).attr('action');
        if (currentPoint == null || currentPoint.PointType != a) {
            if (currentPoint != null) {
                clearPoint(currentPoint);
            }
            geocodePosition(map.getCenter(), function (adress) {
                currentPoint = addMarker(createMarkup(a, adress), a);
                if (a == 0) {
                    setTimeout("openBox();", 300);
                }
            });

        }
        else {

            map.setCenter(currentPoint.getPosition());
            openBox();
        }
        return false;
    });
}

function toggleMarkerBox(m) {
    if (!m) return;
    if (isInfoWindowOpen(m)) {
        m.ib.close();
    } else {
        m.ib.open(map, m);
    }
}


function toggleBox(box) {
    if (!box)
        box = currentPoint.ib;

    if (isInfoWindowOpen(box.marker)) {
        closeBox(box);
    } else {
        openBox(box);
    }
}


function isInfoWindowOpen(marker) {
    var map = marker.ib.getMap();
    return (map !== null && typeof map !== "undefined");
}



function openBox(box) {

    if (!box)
        box = currentPoint.ib;
    if (isInfoWindowOpen(box.marker)) {

    } else {
        box.open(map, box.marker);
    }
}


function loadBoxFilledValues(box) {
    if (box.PointType == 0 || box.PointType == 1) {
        if (box.marker.UserData) {
            $('#ZoneAdress').text(box.marker.UserData.ZoneAdress);
            $('#ZoneName').val(box.marker.UserData.ZoneName);
            $('#ZoneType').val(box.marker.UserData.ZoneType);
            if (box.marker.UserData.ZoneSmokingType) {
                var all = $('#ZoneAddCell input[type="checkbox"]').filter('[arg!="' + box.marker.UserData.ZoneSmokingType + '"]');
                all.parent().removeClass('-active');
                all.removeAttr('checked');

                var cb = $('#ZoneAddCell input[type="checkbox"]').filter('[arg="' + box.marker.UserData.ZoneSmokingType + '"]');
                cb.parent().addClass('-active');
                cb.attr('checked', 'checked');
            }
            $('#ZoneDescr').val(box.marker.UserData.ZoneDescr);
            $('#ZonePhoto').val(box.marker.UserData.ZonePhoto);
            if (box.marker.UserData.ZonePhoto && box.marker.UserData.ZonePhoto.length) {
                $('#previewImg').attr('src', box.marker.UserData.ZonePhoto);
                $('#previewImg').parent().show();
            }
        }
    }
}

function createMarkup(type, addr, md) {
    if (type == 0 || type == 1) {
        var arg = 0, existName = '', c1 = '', c2 = 'checked', c3 = '', existDescr = '', imgPreviewShow = 'style="display:none"', imgPreviewSrc = '', smoke = '-1', uploaded = '';
        if (md) {
            arg = md.ID;
            existName = md.Name;
            addr = md.Address;
            c2 = '';
            if (md.SmokingType == 1)
                c1 = 'checked';
            if (md.SmokingType == 0)
                c3 = 'checked';
            if (md.SmokingType == -1)
                c2 = 'checked';

            existDescr = md.Description;

            if (md.ImageLink.length) {
                imgPreviewShow = '';
                imgPreviewSrc = "src='" + md.ImageLink + "'";
            }
            smoke = md.SmokingType;

        }
        return sprintf(newMarkerTemplate, arg, addr, type == 0 ? "Название объекта" : "Название зоны", existName, type == 0 ? "Тип объекта" : "Тип зоны", getOptionList(md), c1, c2, c3, existDescr, imgPreviewShow, imgPreviewSrc, type == 0 ? "Сохранить объект" : "Сохранить зону", smoke, uploaded);
    }

    if (type == 2 || type == 3) {
        return sprintf(existMarkerTemplate, "zone-info", md.ID, md.HeaderText, getAuthor(md), md.Name, getImageHtml(md.ImageLink), md.Description, getBtnHtml(md), md.CommentsLink, md.CommentCount, getPostFix(md.CommentCount));

    }


    function getAuthor(md) {
        return '<a href="' + md.UserLink + '">' + md.UserName + '</a> добавил(а) ' + (type == 2 ? 'объект' : 'зону') + ':';
    }

    function getImageHtml(url) {
        if (url && url.length) {
            return '<div class="image"><img src="' + url + '"></div>';
        }
        return '';

    }


    function getPostFix(cnt) {
        var rest = parseInt(cnt) % 10;
        if (rest == 0 || rest >= 5)
            return "ев";
        if (rest == 1)
            return "й";
        if (rest > 1 && rest < 5)
            return "я";

        return "";
    }

    function getBtnHtml(d) {
        if (d.IsMyPoint) {
            return '<a title="Редактировать" href="' + d.EditLink + '" arg="edit" onclick="return editObj(' + d.ID + ');"><i class="icon-edit"></i></a>' +
                            '<a href="#" onclick="return false;" arg="photo"></a>';//<i class="icon-photo"></i>
        } else {
            return ''/* '<a href="#" onclick="return false;"></a>' +
                '<a href="#" onclick="return false;"></a>'*/;

        }
    }


    function getOptionList(d) {
        filterData = getFilterDefaulData();
        var list = "";
        for (var i = 0; i < filterData.ObjectTypeList.length; i++) {
            list += "<option " + (d ? (d.TypeID == filterData.ObjectTypeList[i].ID ? 'selected' : '') : '') + " value='" + filterData.ObjectTypeList[i].ID + "'>" + filterData.ObjectTypeList[i].Name + "</option>";
        }
        return list;
    }

    return "";

}

function editObj(id) {
    $.post('/Master/ru/GoogleMap/GetObject', { id: id }, function (o) {
        if (o && o.ID > 0) {
            closeAllPopups();
            removeObject(o.ID);
            var t = o.IsRegion ? 1 : 0;
            map.setCenter(new google.maps.LatLng(o.PointPosition.Lat, o.PointPosition.Lng));
            if (currentPoint) {
                closeBox(currentPoint.ib);
                if (currentPoint.polygon) {
                    currentPoint.polygon.setMap(null);
                }
                currentPoint.setMap(null);
            }
            currentPoint = addMarker(createMarkup(t, o.Address, o), t, o, function () {
                openBox();
                loadFilteredObjectList(true);
            });

            /*
                        if (t == 0) {
                            setTimeout("openBox();", 300);
                        }
            */
        }
    });
    return false;
}


function removeObject(id) {
    if (loadedMarkerList && loadedMarkerList.length) {
        for (var i = 0; i < loadedMarkerList.length; i++) {
            if (loadedMarkerList[i].ObjectID == id) {
                closeBox(loadedMarkerList[i].ib);
                if (loadedMarkerList[i].polygon)
                    loadedMarkerList[i].polygon.setMap(null);
                loadedMarkerList[i].setMap(null);

                loadedMarkerList.splice(i, 1);
                break;
            }
        }
    }
}

function geocodePosition(pos, callback) {
    var geocoder = new google.maps.Geocoder();
    geocoder.geocode({
        latLng: pos
    },
        function (results, status) {
            if (status == google.maps.GeocoderStatus.OK && results.length) {
                return callback(results[0].formatted_address);
            } else {
                return callback("");
            }
        }
    );
}

function getAdress(latlng, callback) {
    var url = "http://maps.google.com/maps/api/geocode/json?latlng=%s,%s&sensor=false";
    $.get(sprintf(url, latlng.lat(), latlng.lng()), function (responce) {
        if (responce.results.length && responce.status == google.maps.GeocoderStatus.OK) {
            return callback(responce.results[0].formatted_address);
        }
    });

    return callback("");
}


String.format = function () {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }

    return s;
}



var existMarkerTemplate = '\
			<div class="gmap-balloon %s" OjectID="%s"> \
				<p class="name">%s</p> \
				<p class="zone">%s</p> \
				<h3>%s</h3> \
				%s \
				<p>%s</p> \
				<div class="buttons"> \
					%s\
					<a title="Написать комментарий" href="%s" class="comments"><span>%s</span>Комментари%s <i class="icon-rightarr"></i></a> \
				</div> \
			</div>';


var newMarkerTemplate = '\
			<div class="gmap-balloon newmarker-info" id="ZoneAddCell" arg="%s"> \
				<p class="zone" id="ZoneAdress">%s</p> \
				<textarea id="ZoneName" data-placeholder="%s">%s</textarea> \
				<select id="ZoneType">\
					<option>%s</option>\
                    %s\
				</select> \
				<div class="checkboxes radio"> \
					<label></i><input %s id="ZoneIsForSmoke"  arg="1" type="radio" name="ST" />Курят</label> \
					<label></i><input %s id="ZoneIsNotForSmokeicon-edit" arg="-1" type="radio" name="ST"/>Не курят</label> \
					<label></i><input %s id="ZoneIsForStopping" arg="0" type="radio" name="ST" />Бросают</label> \
				</div> \
				<textarea id="ZoneDescr" data-placeholder="Описание">%s</textarea> \
				<div class="image" %s><img %s id="previewImg"/></div> \
				<div class="message" id="message" ></div> \
				<div class="buttons"> \
					<a href="#" class="-green" arg="save"><i class="icon-check"></i> %s</a> \
					<input type="hidden" id="ZoneSmokingType" value="%s" /> \
					<input type="hidden" id="ZonePhoto" value="%s" /> \
					<input type="file" id="ZonePhotoUploader" data-text="Добавить фото" /> \
				</div> \
			</div>';


var openBoxTimeout;
var openBoxTimeoutDelay = 1000;
function addMarker(markerMarkup, type, fromData, callback) {
    var icon = '/content/client/i/markers/';

    if (fromData)
        type = (fromData.IsRegion ? 1 : 0);


    if (type == 0)
        icon += "marker5.png";
    if (type == 1)
        icon += "marker6.png";

    var latlng = map.getCenter();


    if (fromData)
        latlng = new google.maps.LatLng(fromData.PointPosition.Lat, fromData.PointPosition.Lng);

    currentPoint = new google.maps.Marker({
        map: map,
        position: latlng,
        visible: true,
        icon: icon,
        draggable: true,
        zIndex: 2000
    });

    var ibOptions = {
        content: markerMarkup,
        alignBottom: false,
        pixelOffset: new google.maps.Size(35, -94),
        infoBoxClearance: new google.maps.Size(10, 55)
    };

    var ib = new InfoBox(ibOptions);
    ib.PointType = type;
    ib.ObjectID = 0;
    if (fromData)
        ib.ObjectID = fromData.ID;
    ib.marker = currentPoint;
    currentPoint.ib = ib;
    if (type == 1) {
        var polygone;
        if (!fromData) {
            var bounds = map.getBounds();
            var leftUp = bounds.getNorthEast();
            var bottomDown = bounds.getSouthWest();
            var distance = Math.abs(leftUp.lat() - bottomDown.lat()) / $('#map').width();

            var pixelMargin = 50;

            var magicDigit = pixelMargin * distance;

            var center = map.getCenter();
            polygone = [
                new google.maps.LatLng(center.lat() - magicDigit / 2, center.lng() - magicDigit),
                new google.maps.LatLng(center.lat() + magicDigit / 2, center.lng() - magicDigit),
                new google.maps.LatLng(center.lat() + magicDigit / 2, center.lng() + magicDigit),
                new google.maps.LatLng(center.lat() - magicDigit / 2, center.lng() + magicDigit),
                new google.maps.LatLng(center.lat() - magicDigit / 2, center.lng() - magicDigit)
            ];
        } else {
            polygone = new Array();
            for (var i = 0; i < fromData.RegionPosition.length; i++) {
                polygone.push(new google.maps.LatLng(fromData.RegionPosition[i].Lat, fromData.RegionPosition[i].Lng));
            }
        }
        var defPolygon = new google.maps.Polygon({
            paths: polygone,
            fillColor: '#000000',
            fillOpacity: 0.35,
            strokeWeight: 5,
            strokeColor: "#000000",
            strokeOpacity: 0.8,
            clickable: true,
            editable: true,
            zIndex: 1000,
            draggable: true
        });
        defPolygon.marker = currentPoint;
        currentPoint.polygon = defPolygon;

        defPolygon.setMap(map);


        google.maps.event.addListener(defPolygon, 'dragstart', function () {
            defPolygon.FirstPoint = defPolygon.getPath().getAt(0);
            closeBox();
            currentPoint.setMap(null);
        });

        google.maps.event.addListener(defPolygon, 'drag', function () {

        });

        google.maps.event.addListener(defPolygon, 'dragend', function () {

            var latChange = defPolygon.getPath().getAt(0).lat() - defPolygon.FirstPoint.lat();
            var lngChange = defPolygon.getPath().getAt(0).lng() - defPolygon.FirstPoint.lng();
            var oldPos = currentPoint.getPosition();
            currentPoint.setPosition(new google.maps.LatLng(oldPos.lat() + latChange, oldPos.lng() + lngChange));
            currentPoint.setMap(map);
            geocodePosition(currentPoint.getPosition(), function (adress) {
                if (!currentPoint.UserData)
                    currentPoint.UserData = new Object();
                currentPoint.UserData.ZoneAdress = adress;
            });
        });


    }

    google.maps.event.addListener(currentPoint, 'click', function () {
        openBoxTimeout = setTimeout('console.log("moved")', openBoxTimeoutDelay);
        toggleBox();
        //openBox();
    });

    google.maps.event.addListener(ib, 'domready', function () {
        if (ib.ObjectID > 0) {
        } else {
            loadBoxFilledValues(ib);
        }

        forms();
        select();
        initClient();
        initSave();
    });

    google.maps.event.addListener(currentPoint, 'dragstart', function () {
        closeBox();
    });

    google.maps.event.addListener(currentPoint, 'drag', function () {
        var polygon = currentPoint.polygon;
        if (polygon) {
            var pos = currentPoint.getPosition();
            var isWithinPolygon = containsLatLng(polygon, pos);
            if (!isWithinPolygon) {
                currentPoint.setDraggable(false);
                setTimeout(function () {
                    currentPoint.setDraggable(true);
                }, 200);
                currentPoint.setPosition(currentPoint.LastPointInside);
                return false;
            } else {
                currentPoint.LastPointInside = pos;
            }
        }
    });

    google.maps.event.addListener(currentPoint, 'dragend', function () {
        geocodePosition(currentPoint.getPosition(), function (adress) {
            if (!currentPoint.UserData)
                currentPoint.UserData = new Object();
            currentPoint.UserData.ZoneAdress = adress;
            openBox(currentPoint.ib);
        });
    });


    if (fromData)
        saveFieldsInMarker(ib, fromData);

    if (callback)
        callback();

    return currentPoint;
}

var pd = null;
function initSave() {
    $('a[arg="save"]').click(function () {
        var msg = '';
        var box = $(this).parents('.infoBox');
        if (!$.trim(box.find('#ZoneName').val()).length) {
            msg += "Необходимо указать название<br/>";
        }
        if (!$('#ZoneType').val() || !$('#ZoneType').val().length || $('#ZoneType').val() == 'Тип объекта') {
            msg += "Необходимо выбрать тип объекта<br/>";
        }
        if (msg.length) {
            $('.message').html(msg);
            return false;
        }

        $('.message').html('');

        

        var saver = '/Master/ru/GoogleMap/AddObject';
        pd = getFormData();
        
        var exist = pd.UserData.ID > 0;
        $.post(saver, { qs: JSON.stringify(pd) }, function (data) {
            if (data == "1") {
                $('.message').html(exist ? "Точка сохранена" : "Точка успешно добавлена на карту");
                setTimeout(function () {
                    closeBox(currentPoint.ib);
                    if (currentPoint.polygon) {
                        currentPoint.polygon.setMap(null);
                    }
                    currentPoint.setMap(null);
                    if ($('#SmokingType').val() && $('#SmokingType').val() != 'null') {
                        if ($('#SmokingType').val() != pd.UserData.ZoneSmokingType) {

                            $('#SmokingType').val(/*pd.UserData.ZoneSmokingType*/'null');
                            var $this = $('.mapcontrols a[arg="null"]');
                            $this.addClass('-active');
                            $this.css('display', 'block');
                            var another = $('.mapcontrols a[arg!="null"]');
                            another.css('display', 'inline');
                            another.removeClass('-active');
                            autoSave();
                        }
                    }

                    if ($('#SearchWord').val().length) {
                        $('#SearchWord').val('');
                        autoSave();
                    }

                    if ($('.objects-list .-active').length) {
                        var objI = $('.objects-list i[typeid="' + (pd.UserData.HasPolygon ? '1' : '0') + '"]').filter('[type-name-id="' + pd.UserData.ZoneType + '"]');
                        if (!objI.parent().hasClass('-active')) {
                            changeSelection(false, objI.attr('type-name-id'), objI.attr('typeid'));
                            setSelectionFromData();
                        }

                    }
                    currentPoint = null;
                    loadFilteredObjectList(!exist);

                }, 500);
            } else {
                $('.message').html(data);
            }
        });
        return false;
    });
}


function closeBox(box) {
    if (!box)
        box = currentPoint.ib;
    if (isInfoWindowOpen(box.marker)) {
        if (box.PointType == 0 || box.PointType == 1) {
            saveFieldsInMarker(box);
        }
    }


    if (isInfoWindowOpen(box.marker)) {
        box.close();
    } else {
    }
}


function saveFieldsInMarker(box, fromData) {
    if (!box.marker.UserData)
        box.marker.UserData = new Object();
    if (!fromData) {
        $('#ZoneSmokingType').val($('.rd.-active input').attr('arg'));
        box.marker.UserData.ID = $('#ZoneAddCell').attr('arg');
        box.marker.UserData.ZoneAdress = $('#ZoneAdress').text();
        box.marker.UserData.ZoneName = $('#ZoneName').val();
        box.marker.UserData.ZoneType = $('#ZoneType').val();
        
        box.marker.UserData.ZoneSmokingType = $('#ZoneSmokingType').val() == 'null' ? null : $('#ZoneSmokingType').val();
        box.marker.UserData.ZoneDescr = $('#ZoneDescr').val();
        box.marker.UserData.ZonePhoto = $('#ZonePhoto').val();
    } else {
        box.marker.UserData.ID = fromData.ID;
        box.marker.UserData.ZoneAdress = fromData.Address;
        box.marker.UserData.ZoneName = fromData.Name;
        box.marker.UserData.ZoneType = fromData.TypeID;
        box.marker.UserData.ZoneSmokingType = fromData.SmokingType;
        box.marker.UserData.ZoneDescr = fromData.Description;
        box.marker.UserData.ZonePhoto = fromData.ImageLink;
    }
}


function syncObjectFilters() {
    setSelectionFromData();

    $('.objects-list a i, .multiselect ul li').click(function () {
        changeSelection($(this).hasClass('-active'), $(this).attr('type-name-id'), $(this).attr('typeid'));
        setSelectionFromData();
        loadFilteredObjectList();
        return false;
    });
}


function changeSelection(forRemove, typenameid, typeid) {
    if (filterData == null)
        filterData = getFilterDefaulData();


    if (forRemove) {
        for (var i = 0; i < filterData.SelectedTypes.length; i++) {
            if (filterData.SelectedTypes[i].ObjectType == typeid && filterData.SelectedTypes[i].TypeID == typenameid) {
                filterData.SelectedTypes.splice(i, 1);
                break;
            }
        }
    } else {
        filterData.SelectedTypes.push({ ObjectType: typeid, TypeID: typenameid });
    }

    autoSave();
}



function autoSave() {

    if (filterData)
        $('#ObjectFilter').val(JSON.stringify(filterData));
    if (map)
        $('#CurrentMapView').val(JSON.stringify(map.CurrentMapView));

    if (currentPoint != null) {
        $('#NewPointData').val(JSON.stringify(getFormData()));
    }

    $('input[rel="autoprop"]').each(function () {
        $.cookie($(this).attr('id'), $(this).val(), { expires: 365 });
    });
}

function getFormData() {
    saveFieldsInMarker(currentPoint.ib);
    var data = new Object();
    data.UserData = new Object();
    data.UserData.ID = currentPoint.UserData.ID;
    data.UserData.ZoneAdress = currentPoint.UserData.ZoneAdress;
    data.UserData.ZoneDescr = currentPoint.UserData.ZoneDescr;
    data.UserData.ZoneName = currentPoint.UserData.ZoneName;
    data.UserData.ZonePhoto = currentPoint.UserData.ZonePhoto;
    data.UserData.ZoneSmokingType = currentPoint.UserData.ZoneSmokingType;
    data.UserData.ZoneType = currentPoint.UserData.ZoneType;
    data.HasPolygon = (currentPoint.polygon ? true : false);
    if (currentPoint.polygon) {
        data.Polygon = new Array();
        var path = currentPoint.polygon.getPath();
        for (var i = 0; i < path.length; i++) {
            data.Polygon.push({ Lat: path.getAt(i).lat(), Lng: path.getAt(i).lng() });
        }
    }
    var pos = currentPoint.getPosition();
    data.Marker = new Object();
    data.Marker.Lat = pos.lat();
    data.Marker.Lng = pos.lng();
    return data;
}

function setSelectionFromData() {
    if (filterData == null)
        filterData = getFilterDefaulData();

    $('.objects-list a i').removeClass('-active');
    $('.multiselect li').removeClass('-active');

    for (var i = 0; i < filterData.SelectedTypes.length; i++) {
        $('.objects-list[typeid="' + filterData.SelectedTypes[i].ObjectType + '"]').find('a i[type-name-id="' + filterData.SelectedTypes[i].TypeID + '"]').addClass('-active');
        $('.multiselect[typeid="' + filterData.SelectedTypes[i].ObjectType + '"]').find('li[type-name-id="' + filterData.SelectedTypes[i].TypeID + '"]').addClass('-active');
    }
    $('.objects-list').filter('[typeid="' + filterData.ActiveList + '"]').show();
    $('.objects-list').filter('[typeid!="' + filterData.ActiveList + '"]').hide();

    $('.multiselect').filter('[typeid!="' + filterData.ActiveList + '"]').removeClass('-green1');
    $('.multiselect').filter('[typeid="' + filterData.ActiveList + '"]').addClass('-green1');


}

function copySocials() {
    $('#SimpleRestore').hide();
    $('a[data-fancybox="#enter"]').click(function () {
        var uLogin = $('#uLogin');
        var target = $('.social-login1');
        if (uLogin.length && target.length) {
            target.html('');
            uLogin.clone().appendTo('.social-login1');
        }

    });
}

function changeFormTo(targetClass, sourceClass) {
    $(sourceClass).parent().parent().hide();
    $(targetClass).parent().parent().show();
    return false;
}

function submitForm(selector) {
//    console.log($(selector).find('form'));
    $('form[data-ajax-update="' + selector + '"]').trigger('submit');

//    $(selector).find('form').trigger('submit');
    return false;
}

function loadTabs() {
    $('.tabs a').click(function () {
        $('.tabs a').removeClass('active');
        $(this).addClass('active');
        $('.tab-content > div').hide();
        $('.tab-content div[id="' + $(this).attr('arg') + '"]').show();
        return false;
    });
}

function multiselect() {

    $('#IsFavoriteBtn').click(function () {
        $(this).toggleClass('-active');
        $('#IsFavorite').val($(this).hasClass('-active') ? 1 : 0);
        autoSave();
        loadFilteredObjectList();
        return false;
    });

    $('.mapcontrols a').click(function () {
        if ($(this).hasClass('-active'))
            return false;
        $(this).toggleClass('-active');
        var another = $('.mapcontrols a[arg!="' + $(this).attr('arg') + '"]');
        another.css('display', 'inline');
        another.removeClass('-active');
        if (!$('.mapcontrols a.-active').length) {
            $('.mapcontrols a[arg="null"]').addClass('-active');
        }

        $('#SmokingType').val($('.mapcontrols a.-active').attr('arg'));
        autoSave();
        loadFilteredObjectList();
        return false;
    });



    $('.search-box-inp').change(function () {
        setWord();
        return false;
    });

    $('#SearchFrom a').click(function () {
        setWord();
        return false;
    });


    function setWord() {
        $('#SearchWord').val($('.search-box-inp').val());
        autoSave();
        loadFilteredObjectList(false, function (data) {
            if (data.length) {
                var d = data[0];
                var pos = new google.maps.LatLng(d.PointPosition.Lat, d.PointPosition.Lng);
                map.panTo(pos);
                map.setCenter(pos);
            }
        });
    }

    var select = $('.multiselect');

    select.each(function () {
        var $this = $(this),
            list = $this.find('ul'),
            items = list.find('li');

        //Toggle dropdown list with checkboxes
        $this.click(function (e) {

            select.removeClass('-green1');
            $this.addClass('-green1');
            var type = $this.attr('typeid');
            $('.objects-list').fadeOut(300, function () {
                $('.objects-list[typeid="' + type + '"]').fadeIn(300);
            });
            filterData.ActiveList = type;
            autoSave();

            select.removeClass('-active');
            select.find('ul').fadeOut(100);

            if (!list.is(':visible')) {
                $this.addClass('-active');
                list.css({
                    top: $this.offset().top
                }).fadeIn(50);
            } else {
                $this.removeClass('-active');
                list.fadeOut(100);
            }

            list.css('minWidth', $this.outerWidth());
            e.stopPropagation();
        });

        list.click(function (e) {
            e.stopPropagation();
        });

        items.click(function () {
            //$(this).toggleClass('-active');
        });

        //Stop accidentally hide
        $(document).click(function () {
            select.removeClass('-active');
            select.find('ul').fadeOut(100);
        });
    });
}


function addScript(url, callback) {
    var script = document.createElement('script');
    if (callback) script.onload = callback;
    script.type = 'text/javascript';
    script.src = url;
    document.body.appendChild(script);
}

function loadMapsAPI() {
    addScript('https://maps.googleapis.com/maps/api/js?key=' + $('#GoogleAPI').val() + '&sensor=false&callback=mapsApiReady');
}

function mapsApiReady() {
    addScript('/content/client/js/libs/infobox_packed.js', mapInit);
}

function gmapsInit() {

    loadMapsAPI();
	$(window).resize();
}


function getMapDefaulData() {

    var json = $('#CurrentMapView').val();
    var fromCook = $.cookie('CurrentMapView');
    if (fromCook && fromCook.length) {
        json = fromCook;
    }

    return JSON.parse(json);
}

function getFilterDefaulData() {

    if (filterData)
        return filterData;

    var json = $('#ObjectFilter').val();

    var fromCook = $.cookie('ObjectFilter');
    if (fromCook && fromCook.length) {
        json = fromCook;
    }

    filterData = JSON.parse(json);
    return filterData;
}


function loadInputsFromHash() {
    var hash = document.location.hash.replace('#', '');
    var rv = '';
    if (hash.length) {
        var pairs = hash.split('&');
        for (var i = 0; i < pairs.length; i++) {
            var p = pairs[i].split('=');
            $('#' + p[0]).val(p[1]);
            if (p[0] == 'MapCenter')
                rv = p[1];
        }
    }
    document.location.hash = "";
    autoSave();
    return rv;

}


function closeAllPopups() {
    if (loadedMarkerList) {
        for (var i = 0; i < loadedMarkerList.length; i++) {
            closeBox(loadedMarkerList[i].ib);
        }
    }
}

function loadFilteredObjectList(filterStable, callback) {
    if (!openBoxTimeout) {
        closeAllPopups();
    } else {
    }
    var filter = new Object();
    filter.ObjectFilter = getFilterDefaulData();
    filter.SearchWord = $.trim($('.search-box-inp').val());
    var st = $('.mapcontrols .-active').attr('arg');
    filter.SmokingType = ((!st || st == 'null' || !st.length) ? null : st);
    filter.IsFavorite = $('#IsFavoriteBtn').hasClass('-active') ? "1" : "0";
    filter.CurrentMapView = map.CurrentMapView;
    filter.LoadedPoints = (filterStable ? getLoadedIds() : new Array());
    $.post('/Master/ru/GoogleMap/GetPoints', { qs: JSON.stringify(filter) }, function (data) {
        addExistMarkers(data, filterStable);
        if (callback) {
            callback(data);
        }
    }, "json");
}


function getLoadedIds() {
    var arr = new Array();
    if (!loadedMarkerList || !loadedMarkerList.length)
        return arr;
    for (var i = 0; i < loadedMarkerList.length; i++) {
        var oid = loadedMarkerList[i].ObjectID;
        if (oid)
            arr.push(oid);
    }
    return arr;
}

function loadControlsFromInputs() {
    if ($.cookie('SearchWord'))
        $('#SearchWord').val($.cookie('SearchWord'));
    if ($.cookie('SmokingType'))
        $('#SmokingType').val($.cookie('SmokingType'));

    if (!$('#SmokingType').val()) {
        //alert($('#SmokingType').val());
        $('#SmokingType').val('null');
    }

    if ($.cookie('IsFavorite'))
        $('#IsFavorite').val($.cookie('IsFavorite'));

    if ($('#SearchWord').length && $('#SearchWord').val().length) {
        $('.search-box-inp').val($('#SearchWord').val());
    }
    $('.mapcontrols a[arg="' + $('#SmokingType').val() + '"]').addClass('-active');
    if ($('#IsFavorite').val() == '1') {
        $('#IsFavoriteBtn').addClass('-active');
    }

}

var mapDragging;
var map;
var drawingManager;
var mapRefreshTime = 500;

function mapInit() {
    if (!$('#map').length)
        return;

    var eoid = '';
    if (document.location.hash.indexOf('EditObj') >= 0) {
        var hash = document.location.hash.replace('#', '');

        if (hash.length) {
            var pairs = hash.split('&');
            for (var i = 0; i < pairs.length; i++) {
                var p = pairs[i].split('=');
                if (p[0] == 'EditObj')
                    eoid = p[1];
            }
        }
    }

    var rv = loadInputsFromHash();
    loadControlsFromInputs();

    /*if (document.location.href.indexOf('/map') >= 0) {
        $('#map').height(
            $('.aside').outerHeight() -
                ($('header').height() + $('.filter').height())
        );
    }*/

    var mapData = getMapDefaulData();

    if (rv.length) {
        var pairs = rv.split(';');
        if (pairs.length == 3) {
            mapData.Zoom = parseInt(pairs[2]);
            mapData.MapCenter.Lat = parseFloat(pairs[0]);
            mapData.MapCenter.Lng = parseFloat(pairs[1]);
        }
    }

    var mapOptions = {
        zoom: mapData.Zoom,
        disableDefaultUI: true,
        center: new google.maps.LatLng(mapData.MapCenter.Lat, mapData.MapCenter.Lng),
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        zoomControl: true,
        zoomControlOptions: {
            position: google.maps.ControlPosition.RIGHT_BOTTOM
        }
    };

    map = new google.maps.Map(document.getElementById('map'), mapOptions);
    map.CurrentMapView = mapData;
    map.panTo(mapOptions.center);

    autoSave();

    google.maps.event.addListener(map, 'click', function () {
        if (currentPoint != null)
            closeBox(currentPoint.ib);

        if (loadedMarkerList) {
            for (var i = 0; i < loadedMarkerList.length; i++) {
                closeBox(loadedMarkerList[i].ib);
            }
        }
    });

    google.maps.event.addListener(map, 'zoom_changed', function () {
        map.CurrentMapView.Zoom = map.getZoom();
        var center = map.getCenter();
        map.CurrentMapView.MapCenter = new Object();
        map.CurrentMapView.MapCenter.Lat = center.lat();
        map.CurrentMapView.MapCenter.Lng = center.lng();
        var bounds = map.getBounds();
        map.CurrentMapView.LeftUpperCorner = new Object();
        map.CurrentMapView.LeftUpperCorner.Lat = bounds.getSouthWest().lat();
        map.CurrentMapView.LeftUpperCorner.Lng = bounds.getNorthEast().lng();
        map.CurrentMapView.RightBottomCorner = new Object();
        map.CurrentMapView.RightBottomCorner.Lat = bounds.getNorthEast().lat();
        map.CurrentMapView.RightBottomCorner.Lng = bounds.getSouthWest().lng();

        autoSave();
        if (mapDragging) {
            clearTimeout(mapDragging);
        }
        mapDragging = setTimeout(function () {
            loadFilteredObjectList(true);
        }, mapRefreshTime);


    });

    google.maps.event.addListener(map, "bounds_changed", function () {
        map.CurrentMapView.Zoom = map.getZoom();
        var center = map.getCenter();
        map.CurrentMapView.MapCenter = new Object();
        map.CurrentMapView.MapCenter.Lat = center.lat();
        map.CurrentMapView.MapCenter.Lng = center.lng();
        var bounds = map.getBounds();
        map.CurrentMapView.LeftUpperCorner = new Object();
        map.CurrentMapView.LeftUpperCorner.Lat = bounds.getSouthWest().lat();
        map.CurrentMapView.LeftUpperCorner.Lng = bounds.getNorthEast().lng();
        map.CurrentMapView.RightBottomCorner = new Object();
        map.CurrentMapView.RightBottomCorner.Lat = bounds.getNorthEast().lat();
        map.CurrentMapView.RightBottomCorner.Lng = bounds.getSouthWest().lng();

        autoSave();
        if (mapDragging) {
            clearTimeout(mapDragging);
        }
        mapDragging = setTimeout(function () {
            loadFilteredObjectList(true);
        }, mapRefreshTime);

    });

    if (eoid.length) {
        editObj(eoid);
    }

}


function carousel() {
    var carousel = $('.carousel');

    if (carousel.length) {
        var nav = carousel.find('.carousel-nav'),
			buttons = carousel.find('.icon-prev, .icon-next');

        if (carousel.find('li').length > 1) {
            carousel.find('.items').jCarouselLite({
                btnNext: buttons[1],
                btnPrev: buttons[0],
                auto: false,
                speed: 300,
                visible: 1,
                responsive: true,
                easing: 'easeInOutQuad'
            });
        }
    }
}


function loadNews() {
	if ($('.container-main').length) {
		var btn = $('.btn_slide'),
			main_container = $('.container-main'),
			main_container_height = main_container.height(),
			news_container = $('#newsContainer'),
			news_container_height,
			counter = 0,
			counterUp = 0;

        
		news_container.load('/news .layout', function () {
		    //debugger;
			news_container_height = news_container.height();
			news_container.height(0);
			//console.log(news_container.find('.profile-name #enter'));
		    news_container.find('.profile-name #enter').remove();
		});

		$(window).mousewheel(function(e, delta) {
			if (delta > 0) {//Up
				counter = 0;

				if ($(window).scrollTop() == 0) {
					if (news_container.hasClass('visible')) {
						counterUp++;
						if (counterUp == 3) {
							animateToTop()
						}
					}
				}
			} else if (delta < 0) {//Down
				counterUp = 0;

				if ($(window).scrollTop() + $(window).height() >= main_container.height()) {
					counter++;
					if (counter == 3) {
						if (!news_container.hasClass('visible')) {
							animateToNews();
						}
					}
				}
			}
		});

		btn.click(function() {
			animateToNews();
		});

		function animateToNews() {
			main_container
				.animate({
					height: 35
				}, {easing: 'easeInOutQuad', duration: 550});

			$('html, body').animate({scrollTop: 0}, 550);

			news_container.animate({height: news_container_height},
				{easing: 'easeInOutQuad', duration: 550, complete: function() {
					news_container.addClass('visible')
				}});

			btn.prependTo(news_container);
			btn.find('i')
				.removeClass('icon-slidedown')
				.addClass('icon-slideup');
			btn.unbind('click').click(function() {
				animateToTop();
			});

			counter = 0;

			return false;
		}


		function animateToTop() {
			news_container.animate({height: 0}, {easing: 'easeInOutQuad', duration: 550, complete: function() {
				news_container.removeClass('visible')
			}});

			main_container.animate({height: main_container_height}, {easing: 'easeInOutQuad', duration: 550});

			btn.appendTo(main_container.find('.map'));
			btn.find('i')
				.removeClass('icon-slideup')
				.addClass('icon-slidedown');

			btn.unbind('click').click(function() {
				animateToNews();
			});
		}
	}
}


function dropdowns() {
    var current_visible_list = null,
		origin;

    $('body').on('click', '.dropdown', function (e) {
        var $this = $(this),
			origin = $this,
			text = $this.find('> span:first'),
			list = $this.find('.dropdown-list'),
			items = list.find('> *');

        if (!list.hasClass('visible')) {
            current_visible_list = list;
            $this.addClass('visible');
            list.appendTo('body');
            list.fadeIn(50, function () {
                list.css({
                    left: $this.offset().left,
                    top: $this.offset().top + $this.height()
                })
            });
            list.css('minWidth', $this.outerWidth());

            $this.click(function () {
                hideList($this, list);
                $this.removeClass('visible');
            });
        }

        items.click(function () {
            items.removeClass('-current');
            $(this).addClass('-current');
            text.text($(this).text());
            hideList($this, current_visible_list);

            return true;
        });

        e.stopPropagation();
    });

    function hideList(origin, list) {
        if (current_visible_list != null) {
            list.fadeOut(100, function () {
                list.appendTo(origin);
            });
        }
    }

    $(document).click(function () {
        if (current_visible_list != null) {
            hideList(origin, current_visible_list);
            current_visible_list.removeClass('visible');
        }
    });
}


var fancyboxDefaults = {
    fitToView: false,
    autoSize: true,
    openEffect: 'none',
    closeEffect: 'none',
    scrolling: 'no',
    /*.tpl: {
        wrap: '<div class="fancybox-wrap" tabIndex="-1"><div class="fancybox-skin fancybox-skin1"><div class="fancybox-outer"><div class="fancybox-inner"></div></div></div></div>',
        closeBtn: '<a class="fancybox-item fancybox-close" href="javascript:;"></a>'
    },*/
    helpers: {
        overlay: {
            locked: false
        }
    }
};


function messageSend() {
    var e = $('#PrivateError');
    if (!$('#PrivateMessage').val().length) {
        e.html("Необходимо заполнить текст сообщения");
        e.css('color', 'red');
        return;
    }
    $.post('/Master/ru/Account/SendLetter', { uid: $('#PrivateUser').val(), msg: $('#PrivateMessage').val() }, function (d) {
        if (d.length) {
            e.html(d);
            e.css('color', 'red');
        } else {
            $('#PrivateMessage').hide();
            $('#PrivateMessageButton').hide();
            $('#personalMessage h2').html('Сообщение отправлено.');
            $('#personalMessage .form').hide();
            $('#personalMessage h2').css('color', 'green');
            $('#resultText').show();
        }
    });
}


function openWindow() {
    $('[data-fancybox]')
		.not('[data-fancybox="close"]')
		.click(function () {
		    $.fancybox.open($($(this).data('fancybox')), fancyboxDefaults);
		    return false;
		}
	);

    $(document).on('click', '[data-fancybox="close"]', function () {
        $.fancybox.close();
        return false;
    });
}


function forms() {
    var cb = $('label :checkbox'),
		rd = $('label :radio'),
		input_file = $(':file');

    $('[data-placeholder]').each(function () {
        var $this = $(this);

        if (!$this.closest('.input-wrapper').length) {
            var wrapper = $this.wrap('<div class="input-wrapper">').closest('.input-wrapper'),
				placeholder = ($this.is('input[type="text"]') || $this.is('input[type="password"]')) ? $('<input type="text">') : $('<textarea>');

            placeholder
				.val($this.attr('data-placeholder'))
				.addClass('placeholder')
				.removeAttr('data-placeholder')
				.appendTo(wrapper)
				.css({
				    width: $this.outerWidth(),
				    height: $this.outerHeight()
				});

            wrapper.css({
                float: $this.css('float'),
                width: $this.outerWidth(),
                marginTop: $this.css('marginTop'),
                marginRight: $this.css('marginRight'),
                marginBottom: $this.css('marginBottom'),
                marginLeft: $this.css('marginLeft')
            });

            if ($this.val() != '') {
                placeholder.fadeOut(200, function () {
                    placeholder.addClass('hidden')
                });
            }

            placeholder.focus(function () {
                $this.focus();
                placeholder.fadeOut(100, function () {
                    placeholder.addClass('hidden')
                });
            });

            $this
				.focus(function () {
				    placeholder.fadeOut(100, function () {
				        placeholder.addClass('hidden')
				    });
				})
				.blur(function () {
				    if ($this.val() == '') {
				        placeholder.removeClass('hidden');
				        placeholder.fadeIn(100);
				    }
				}
			)
        }
    });

    //Checkboxes
    cb.each(function () {
        var $this = $(this);

        if (!$this.parent().hasClass('cb')) {
            $this.parent().addClass('cb');

            $this.before('<i class="icon-cb"></i>');

            if ($this.prop('checked')) {
                $this.parent().addClass('-active');
            } else {
                $this.parent().removeClass('-active');
            }
        }
    })
		.click(function () {
		    var $this = $(this);

		    if ($this.prop('checked')) {
		        $this.parent().addClass('-active');
		    } else {
		        $this.parent().removeClass('-active');
		    }
		})
		.change(function () {
		    var $this = $(this);

		    if ($this.prop('checked')) {
		        $this.parent().addClass('-active');
		    } else {
		        $this.parent().removeClass('-active');
		    }
		});

    //Radios
    var radios;
    rd.parent().addClass('rd');
    rd.each(function () {
        var $this = $(this);

        $this.before('<i class="icon-rd"></i>');
        if ($this.prop('checked')) {
            $this.parent().addClass('-active');
        } else {
            $this.parent().removeClass('-active');
        }
    })
		.click(function () {
		    var $this = $(this);

		    radios = $('body').find($('[name=' + $this.attr('name') + ']'));
		    if ($this.prop('checked')) {
		        radios.not($this).each(function () {
		            $(this).parent().removeClass('-active');
		        });
		        $this.parent().addClass('-active');
		    }
		    radios = '';
		});

    input_file.each(function () {
        var this_ = $(this);

        if (this_.length) {
            var wrapper = this_.wrap('<div class="fileinput" tabindex="0"></div>').closest('.fileinput'),
				button = wrapper.prepend('<div data-btn />').find('[data-btn]');

            button
				.text(this_.data('text'))
				.addClass(this_.data('css'));

            this_
				.appendTo(button)
				.prop({
				    'tabindex': '-1',
				    'id': this_.prop('id'),
				    'name': this_.prop('name')
				})
        }
    })
}


function select() {
    $('select').selectmenu();
}


function mapcontrols() {
    var controls = $('.mapcontrols'),
		items = controls.find('a');

    controls.hover(
		function () {
		    items.show();
		    items.each(function (i) {
		        $(this).stop().animate({
		            left: (i * 140),
		            opacity: 0.8
		        }, 150)
		    });
		},
		function () {
		    items.stop().animate({ left: 0, opacity: 0 }, 150, function () {
		        $(this).not('.-active').hide();
		    })
		}
	);
}


function editorControls() {
    $('.editor-controls .btn').hover(
		function () {
		    $(this).find('span').addClass('current').stop().animate({
		        'width': '150px',
		        'marginLeft': '25px'
		    }, 100);
		},
		function () {
		    $(this).find('span').removeClass('current').stop().animate({
		        'width': 0,
		        'marginLeft': 0
		    }, 100);
		}
	)
}


function resizeMap() {
	if (document.location.href.indexOf('/map') >= 0) {
		var $map = $('#map'),
		filter = $('.filter'),
		main = $('.main'),
		body = $('body');

		$(window).resize(function() {
			res();
		}).scroll(function() {
			res();
			google.maps.event.trigger(map, "resize");
		});

		function res() {
			$map.css({
				height: body.height() - filter.outerHeight() + $(window).scrollTop() - filter.offset().top - 1
			});
		}
	}
}
