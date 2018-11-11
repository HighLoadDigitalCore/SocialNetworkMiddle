$(function () {
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
	$('.form-login input:text').keyup(function(event) {
		var btn = $(this).closest('.form-login').find('.-submit');

		if (event.keyCode == 13) {//Enter
			btn.click();
		}
	});
});


var currentPoint;
var filterData;
var loadedMarkerList;



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
            zIndex: 2000,
        });

        m.ibc = markUp;




        if (!loadedMarkerList)
            loadedMarkerList = new Array();



        var ibOptions = {
            content: markUp,
            alignBottom: false,
            pixelOffset: new google.maps.Size(24, -71),
            infoBoxClearance: new google.maps.Size(5, 5)
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
            if (box.marker.UserData.ZonePhoto.length) {
                $('#previewImg').attr('src', box.marker.UserData.ZonePhoto);
                $('#previewImg').parent().show();
            }
        }
    }
}
/*

    [Serializable]
    public class UploadingPointData
    {
        public int ID { get; set; }
        public bool IsMyPoint { get; set; }
        public bool IsRegion { get; set; }
        public Coordinate PointPosition { get; set; }
        public List<Coordinate> RegionPosition { get; set; }
        public bool IsMyFavorite { get; set; }
        public string HeaderText { get; set; }
        public string Name { get; set; }
        public string ImageLink { get; set; }
        public string CommentsLink { get; set; }
        public int CommentCount { get; set; }
        public string Description { get; set; }
        public int SmokingType { get; set; }

    }


*/

function createMarkup(type, addr, md) {
    if (type == 0 || type == 1) {
        return sprintf(newMarkerTemplate, addr, type == 0 ? "Название объекта" : "Название зоны", type == 0 ? "Тип объекта" : "Тип зоны", getOptionList(), type == 0 ? "Сохранить объект" : "Сохранить зону");
    }

    if (type == 2 || type == 3) {
        return sprintf(existMarkerTemplate, "zone-info", md.ID, md.HeaderText, md.Name, getImageHtml(md.ImageLink), md.Description, getBtnHtml(md), md.CommentsLink, md.CommentCount, getPostFix(md.CommentCount));

    }

    function getImageHtml(url) {
        if (url && url.length) {
            return '<div class="image"><img src="' + url + '"></div>';
        }
        return '';

    }


    function getPostFix(cnt) {
        var rest = cnt % 10;
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
            return '<a href="#" arg="edit"><i class="icon-edit"></i></a> \
                            <a href="#" arg="photo"><i class="icon-photo"></i></a>';
        } else {
            return '';
        }
    }


    function getOptionList() {
        filterData = getFilterDefaulData();
        var list = "";
        for (var i = 0; i < filterData.ObjectTypeList.length; i++) {
            list += "<option value='" + filterData.ObjectTypeList[i].ID + "'>" + filterData.ObjectTypeList[i].Name + "</option>";
        }
        return list;
    }

    return "";

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
				<p class="zone">%s</p> \
				<h3>%s</h3> \
				%s \
				<p>%s</p> \
				<div class="buttons"> \
					%s\
					<a href="%s" class="comments"><span>%s</span>Комментари%s <i class="icon-rightarr"></i></a> \
				</div> \
			</div>';


var newMarkerTemplate = '\
			<div class="gmap-balloon newmarker-info" id="ZoneAddCell"> \
				<p class="zone" id="ZoneAdress">%s</p> \
				<textarea id="ZoneName" data-placeholder="%s"></textarea> \
				<select id="ZoneType">\
					<option>%s</option>\
                    %s\
				</select> \
				<div class="checkboxes radio"> \
					<label><input id="ZoneIsForSmoke" arg="1" type="checkbox" />Курят</label> \
					<label><input id="ZoneIsNotForSmoke" arg="-1" type="checkbox" checked />Не курят</label> \
					<label><input id="ZoneIsForStopping" arg="0" type="checkbox" />Бросают</label> \
				</div> \
				<textarea id="ZoneDescr" data-placeholder="Описание"></textarea> \
				<div class="image" style="display:none"><img id="previewImg"/></div> \
				<div class="message" id="message" ></div> \
				<div class="buttons"> \
					<a href="#" class="-green" arg="save"><i class="icon-check"></i> %s</a> \
					<input type="hidden" id="ZoneSmokingType" value="-1" /> \
					<input type="hidden" id="ZonePhoto" /> \
					<input type="file" id="ZonePhotoUploader" data-text="Добавить фото" /> \
				</div> \
			</div>';



function addMarker(markerMarkup, type) {
    var icon = '/content/client/i/markers/';

    if (type == 0)
        icon += "marker5.png";
    if (type == 1)
        icon += "marker6.png";

    var latlng = map.getCenter();
    currentPoint = new google.maps.Marker({
        map: map,
        position: latlng,
        visible: true,
        icon: icon,
        draggable: true,
        zIndex: 2000,
    });

    var ibOptions = {
        content: markerMarkup,
        alignBottom: false,
        pixelOffset: new google.maps.Size(35, -94),
        infoBoxClearance: new google.maps.Size(5, 5)
    };

    var ib = new InfoBox(ibOptions);
    ib.PointType = type;
    ib.marker = currentPoint;
    currentPoint.ib = ib;

    if (type == 1) {
        var bounds = map.getBounds();
        var leftUp = bounds.getNorthEast();
        var bottomDown = bounds.getSouthWest();
        var distance = Math.abs(leftUp.lat() - bottomDown.lat()) / $('#map').width();

        var pixelMargin = 50;

        var magicDigit = pixelMargin * distance;

        var center = map.getCenter();
        var polygone = [
            new google.maps.LatLng(center.lat() - magicDigit / 2, center.lng() - magicDigit),
            new google.maps.LatLng(center.lat() + magicDigit / 2, center.lng() - magicDigit),
            new google.maps.LatLng(center.lat() + magicDigit / 2, center.lng() + magicDigit),
            new google.maps.LatLng(center.lat() - magicDigit / 2, center.lng() + magicDigit),
            new google.maps.LatLng(center.lat() - magicDigit / 2, center.lng() - magicDigit)
        ];

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
        toggleBox();
        //openBox();
    });

    google.maps.event.addListener(ib, 'domready', function () {
        loadBoxFilledValues(ib);

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
        $.post(saver, { qs: JSON.stringify(pd) }, function (data) {
            if (data == "1") {
                $('.message').html("Точка успешно добавлена на карту");
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
                    loadFilteredObjectList(true);

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


function saveFieldsInMarker(box) {
    if (!box.marker.UserData)
        box.marker.UserData = new Object();

    box.marker.UserData.ZoneAdress = $('#ZoneAdress').text();
    box.marker.UserData.ZoneName = $('#ZoneName').val();
    box.marker.UserData.ZoneType = $('#ZoneType').val();
    box.marker.UserData.ZoneSmokingType = $('#ZoneSmokingType').val() == 'null' ? null : $('#ZoneSmokingType').val();
    box.marker.UserData.ZoneDescr = $('#ZoneDescr').val();
    box.marker.UserData.ZonePhoto = $('#ZonePhoto').val();
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
        //console.log($(this).attr('id') + " :: " + $(this).val());
        $.cookie($(this).attr('id'), $(this).val(), { expires: 365 });
    });
}

function getFormData() {
    saveFieldsInMarker(currentPoint.ib);
    var data = new Object();
    data.UserData = new Object();
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
    $(selector).find('form').trigger('submit');
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
        loadFilteredObjectList(false, function(data) {
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
    if (hash.length) {
        var pairs = hash.split('&');
        for (var i = 0; i < pairs.length; i++) {
            var p = pairs[i].split('=');
            $('#' + p[0]).val(p[1]);
        }
    }
    document.location.hash = "";
    autoSave();

}


function closeAllPopups() {
    if (loadedMarkerList) {
        for (var i = 0; i < loadedMarkerList.length; i++) {
            closeBox(loadedMarkerList[i].ib);
        }
    }
}

function loadFilteredObjectList(filterStable, callback) {
    closeAllPopups();
    var filter = new Object();
    filter.ObjectFilter = getFilterDefaulData();
    filter.SearchWord = $.trim($('.search-box-inp').val());
    var st = $('.mapcontrols .-active').attr('arg');
    filter.SmokingType = ((st == 'null' || !st.length) ? null : st);
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

    if ($('#SearchWord').val().length) {
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

    loadInputsFromHash();
    loadControlsFromInputs();

    if (document.location.href.indexOf('/map') >= 0) {
        $('#map').height(
            $('.aside').outerHeight() -
                ($('header').height() + $('.filter').height())
        );
    }

    var mapData = getMapDefaulData();


    var mapOptions = {
        zoom: mapData.Zoom,
        disableDefaultUI: true,
        center: new google.maps.LatLng(mapData.MapCenter.Lat, mapData.MapCenter.Lng),
        mapTypeId: google.maps.MapTypeId.ROADMAP
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

}
/*


function insertTestMarkers(map, marker_markup, zone_markup, newmarker_markup, newzone_markup) {
    var marker = new google.maps.Marker({
        map: map,
        position: new google.maps.LatLng(55.776573, 37.60025),
        visible: true,
        icon: '/content/client/i/markers/marker1.png'
    }),
	//InfoBox reference: http://google-maps-utility-library-v3.googlecode.com/svn/trunk/infobox/docs/reference.html
	ibOptions = {
	    content: marker_markup,
	    alignBottom: false,
	    pixelOffset: new google.maps.Size(24, -71),
	    infoBoxClearance: new google.maps.Size(5, 5)
	},
	ib = new InfoBox(ibOptions);

    google.maps.event.addListener(marker, 'click', function () {
        ib.open(map, marker);
    });

    google.maps.event.addListener(ib, 'domready', function () {
        forms();
        select();
    });

    google.maps.event.addListener(map, 'click', function () {
        ib.close();
    });


    var marker1 = new google.maps.Marker({
        map: map,
        position: new google.maps.LatLng(55.701678, 37.579737),
        visible: true,
        icon: '/content/client/i/markers/marker2.png'
    }),
	ibOptions1 = {
	    content: zone_markup,
	    alignBottom: false,
	    pixelOffset: new google.maps.Size(24, -71),
	    infoBoxClearance: new google.maps.Size(5, 5)
	},
	ib1 = new InfoBox(ibOptions1);

    google.maps.event.addListener(marker1, 'click', function () {
        ib1.open(map, marker1);
    });

    google.maps.event.addListener(ib1, 'domready', function () {
        forms();
        select();
    });

    google.maps.event.addListener(map, 'click', function () {
        ib1.close();
    });


    /*
        //Новый маркер
        var marker2 = new google.maps.Marker({
            map: map,
            position: new google.maps.LatLng(55.746573, 37.66025),
            visible: true,
            icon: '/content/client/i/markers/marker5.png'
        }),
        ibOptions2 = {
            content: newmarker_markup,
            alignBottom: false,
            pixelOffset: new google.maps.Size(35, -94),
            infoBoxClearance: new google.maps.Size(5, 5)
        },
        ib2 = new InfoBox(ibOptions2);
            google.maps.event.addListener(marker2, 'click', function () {
            ib2.open(map, marker2);
        });
    
        google.maps.event.addListener(ib2, 'domready', function () {
            forms();
            select();
        });
    
        google.maps.event.addListener(map, 'click', function () {
            ib2.close();
        });
    
    
    #1#


    /*  //Новая зона
      var marker3 = new google.maps.Marker({
          map: map,
          position: new google.maps.LatLng(55.746573, 37.55025),
          visible: true,
          icon: '/content/client/i/markers/marker6.png'
      }),
      ibOptions3 = {
          content: newzone_markup,
          alignBottom: false,
          pixelOffset: new google.maps.Size(35, -94),
          infoBoxClearance: new google.maps.Size(5, 5)
      },
      ib3 = new InfoBox(ibOptions3);
  
      google.maps.event.addListener(marker3, 'click', function () {
          ib3.open(map, marker3);
      });
  
      google.maps.event.addListener(ib3, 'domready', function () {
          forms();
          select();
      });
  
      google.maps.event.addListener(map, 'click', function () {
          ib3.close();
      });#1#
}

*/

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


/*function loadNews() {
    var btn = $('.btn_slide'),
		main_container = $('.container-main'),
		news_container = $('#newsContainer'),
		counter = 0;

    $(window).mousewheel(function (e, delta) {
        if (!main_container.hasClass('container-main-hidden')) {
            if (delta > 0) {//Up
                counter = 0;
            } else if (delta < 0) {//Down
                if ($(window).scrollTop() + $(window).height() == $(document).height()) {
                    counter++;
                    if (counter == 3) {
                        btn.click()
                    }
                }
            }
        } else if (main_container.hasClass('container-main-hidden')) {
            if (delta > 0) {//Up
                if ($(window).scrollTop() == 0) {
                    counter++;
                    if (counter == 3) {
                        btn.click()
                    }
                }
            } else if (delta < 0) {//Down
                counter = 0;
            }
        }
    });


    btn.click(function () {
        if (!main_container.hasClass('container-main-hidden')) {
            news_container.load($('#NewsLink').val() + ' .container', function () {
                $('html, body').animate({
                    scrollTop: 0
                }, 550);

                btn.find('i')
					.removeClass('icon-slidedown')
					.addClass('icon-slideup');

                main_container.addClass('container-main-hidden');
                main_container.animate({ 'margin-top': -(main_container.height() - 50) },
				{
				    easing: 'easeInOutQuad',
				    duration: 550
				});
            });
        } else if (main_container.hasClass('container-main-hidden')) {
            main_container.animate(
				{ 'margin-top': 0 },
				{
				    easing: 'easeInOutQuad',
				    duration: 550,
				    complete: function () {
				        main_container.removeClass('container-main-hidden');
				        news_container.empty();
				        btn.find('i')
							.removeClass('icon-slideup')
							.addClass('icon-slidedown');
				    }
				}
			);
        }

        counter = 0;

        return false;
    })
}*/


function loadNews() {
	var btn = $('.btn_slide'),
		main_container = $('.container-main'),
		main_container_height = main_container.height(),
		news_container = $('#newsContainer'),
		news_container_height,
		counter = 0,
		counterUp = 0;

	news_container.load('http://smoking.ftp.sh/news .layout', function() {
		news_container_height = news_container.height();
		news_container.height(0);
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


function dropdowns() {
	var current_visible_list = null,
		origin;

	$('body').on('click', '.dropdown', function(e) {
		var $this = $(this),
			origin = $this,
			text = $this.find('> span:first'),
			list = $this.find('.dropdown-list'),
			items = list.find('> *');

		if (!list.hasClass('visible')) {
			current_visible_list = list;
			$this.addClass('visible');
			list.appendTo('body');
			list.fadeIn(50, function() {
				list.css({
					left: $this.offset().left,
					top: $this.offset().top + $this.height()
				})
			});
			list.css('minWidth', $this.outerWidth());

			$this.click(function() {
				hideList($this, list);
				$this.removeClass('visible');
			});
		}

		items.click(function() {
			items.removeClass('-current');
			$(this).addClass('-current');
			text.text($(this).text());
			hideList($this, current_visible_list);

			return false;
		});

		e.stopPropagation();
	});

	function hideList(origin, list) {
		if (current_visible_list != null) {
			list.fadeOut(100, function() {
				list.appendTo(origin);
			});
		}
	}

	$(document).click(function() {
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

	$('[data-placeholder]').each(function() {
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
				placeholder.fadeOut(200, function() {
					placeholder.addClass('hidden')
				});
			}

			placeholder.focus(function() {
				$this.focus();
				placeholder.fadeOut(100, function() {
					placeholder.addClass('hidden')
				});
			});

			$this
				.focus(function() {
					placeholder.fadeOut(100, function() {
						placeholder.addClass('hidden')
					});
				})
				.blur(function() {
					if ($this.val() == '') {
						placeholder.removeClass('hidden');
						placeholder.fadeIn(100);
					}
				}
			)
		}
	});

	//Checkboxes
	cb.each(function() {
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
		.click(function() {
			var $this = $(this);

			if ($this.prop('checked')) {
				$this.parent().addClass('-active');
			} else {
				$this.parent().removeClass('-active');
			}
		})
		.change(function() {
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
	rd.each(function() {
		var $this = $(this);

		$this.before('<i class="icon-rd"></i>');
		if ($this.prop('checked')) {
			$this.parent().addClass('-active');
		} else {
			$this.parent().removeClass('-active');
		}
	})
		.click(function() {
			var $this = $(this);

			radios = $('body').find($('[name=' + $this.attr('name') + ']'));
			if ($this.prop('checked')) {
				radios.not($this).each(function() {
					$(this).parent().removeClass('-active');
				});
				$this.parent().addClass('-active');
			}
			radios = '';
		});

	input_file.each(function() {
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
		function() {
			items.show();
			items.each(function(i) {
				$(this).stop().animate({
					left: (i * 140),
					opacity: 0.8
				}, 150)
			});
		},
		function() {
			items.stop().animate({left: 0, opacity: 0}, 150, function() {
				$(this).not('.-active').hide();
			})
		}
	);
}


function editorControls() {
	$('.editor-controls .btn').hover(
		function() {
			$(this).find('span').addClass('current').stop().animate({
				'width': '150px',
				'marginLeft': '25px'
			}, 100);
		},
		function() {
			$(this).find('span').removeClass('current').stop().animate({
				'width': 0,
				'marginLeft': 0
			}, 100);
		}
	)
}
