function fncPhotoClick(obj, id) {
    var strSplit;
    var tmpColor;

    strSplit = document.PublishView.tmpColor.value.split(',');

    for (var i = 0; i < document.all['imageTable'].length; i++) {
        tmpColor = "#" + strSplit[i];
        document.all['imageTable'][i].style.backgroundColor = tmpColor;
    }

    while (obj.tagName != "TABLE") {
        obj = obj.parentElement;
    }

    obj.style.backgroundColor = "green";

    onPhotoClick(id);
}

function onPhotoClick(id) {
    var selectedID = "M" + id;

    //for(i=0; i<$("li[id^='M']").length; i++){
    //$($("#M88888811")[0]).children('.thumb').attr('style', 'border: 2px solid green;');
    //}

    //$('li').each(function() {
    //    if ($(this).attr('id') == selectedID) {
    //        $(this).children('.thumb').attr('border', '2px solid green');
    //    }
    //});

    //for (var i = 0; i < $('.thumb').length; i++) {
    //    if (document.all['li'][i].parentElement.id == selectId) {
    //        document.all['li'][i].style.backgroundColor = "red";
    //        //document.all['li'][i].focus();
    //    }
    //    else {
    //        document.all['li'][i].style.backgroundColor = "buttonface";
    //    }
    //}
}
