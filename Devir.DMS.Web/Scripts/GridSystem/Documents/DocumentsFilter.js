
var docTypeId = "00000000-0000-0000-0000-000000000000";


var loaderForFilterVM = {
    filterWorking : ko.observable(false)
}
//ko.cleanNode(document.getElementById("GridLoaderForFilter"));
ko.applyBindings(loaderForFilterVM, document.getElementById("GridLoaderForFilter"));



$('#btnShowFilterPopover').on('click', function () {
    $('#btnShowFilterPopover').attr('data-content', "");
    //$("#btnShowFilterPopover").popover("hide");

    if ($("#FilterContainer").length == 0) {
        var p = $(this);


        $.ajax({
            type: 'POST',
            url: '/Document/DocumentDynamicFilter',
            contentType: "application/json",
            dataType: "html",
            data: ko.mapping.toJSON({
                docTypeId: docTypeId,
                documentFilterVM: DocumentFilterVM == null ? null : DocumentFilterVM.RowData(),
                isNewFilter: DocumentFilterVM == null ? true : false
            }),
            beforeSend: function () {
                loaderForFilterVM.filterWorking(true);
            },
        }).done(function (response) {
            p.attr("data-content", response);
            p.popover('show');
            loaderForFilterVM.filterWorking(false);
        });
    }
});

$('#btnShowFilterPopover').popover({
    html: true,
    title: 'Фильтрация<a class="close" id="filterClose" href="#">&times;</a>',    
    template: '<div style="width: 600px;" id="FilterTemplate" class="popover popover-medium"><div class="arrow arrow-gray"></div><div class="popover-inner"><h3 class="popover-title"></h3><div class="popover-content"><p></p></div><div class="popover-footer"><button id="applyFilter" class="btn btn-primary">Фильтровать</button></div></div></div>'
});


$('.popover').click(function (e) {
    e.stopPropagation();
});

//закрыть (х) popover
$(document).on("click", "#FilterTemplate #filterClose", function () {
    $("#btnShowFilterPopover").popover("hide");
});

//датапикер
$(document).on("click", ".date", function () {
    if (!$(this).hasClass("hasDatepicker")) {
        $(this).datepicker({
            format: 'dd.mm.yyyy',
            autoclose: true
        }).on('changeDate', function () {
            $(this).datepicker("hide");
        });
        $(this).datepicker("show");
        $(this).addClass('hasDatepicker');
    }
});
//если изменили дату руками
$(document).on("change", ".date input[type=text]", function () {
    $(this).parent().datepicker("setValue", $(this).val());
});

//клик фильтровать
$(document).on('click', '#FilterTemplate #applyFilter', function () {

    DocumentFilterVM.RowData().DepartmentId($('#dpDepartmentId').val());
    DocumentFilterVM.RowData().DepartmentName($('#filterDepartment').val());    

    $('#btnShowFilterPopover i').removeClass('icon-red').addClass('icon-green');
    $("#btnShowFilterPopover").popover("hide");
    clearAndGetData();

});

var clearAndGetData = function () {
    if ($('#allDocsActive').hasClass('active')) {
        AllDocumentGridViewModel.clearData();
        AllDocumentGridViewModel.getData();
    }
    else {
        if (typeof DocumentsByTypeViewModel !== 'undefined') { //!!!! доделать
            DocumentsByTypeViewModel.clearData();
            DocumentsByTypeViewModel.getData();
        }
    }
}

var filterClear = function () {
    $('#btnShowFilterPopover i').removeClass('icon-green').addClass('icon-red');
    $("#btnShowFilterPopover").popover("hide");

    DocumentFilterVM = null;

    clearAndGetData();
};

$(document).on('click', '.navbar #filterRemove', function () {
    filterClear();

    $("#filterCount").text("");
});

$('#allDocsActive #All').click(function () {
    $('#btnShowFilterPopover').attr('data-content', $('#filterContainer').html());
    $('#btnShowFilterPopover').popover('hide');
    docTypeId = "00000000-0000-0000-0000-000000000000";

    filterClear();
});


$('#leftNav .sub a').click(function () {

    $('#btnShowFilterPopover').attr('data-content', "");
    $('#btnShowFilterPopover').popover('hide');

    filterClear();   

    docTypeId = $(this).attr('id');

});

//прозрачность
//$(document).on('mouseover', '#FilterContainer', (function () {
//    $(this).stop(true, true).animate({ opacity: 1}, 0);
//}));
//$(document).on('mouseout', '#FilterContainer', (function () {
//    $(this).stop(true, true).animate({ opacity: 0.3}, 1);
//}));

//закрываем popover при клике в любом др. месте.
//$('body').on('click', function (e) {
//    $('[data-toggle="popover"]').each(function () {
//        //the 'is' for buttons that trigger popups
//        //the 'has' for icons within a button that triggers a popup
//        if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('.popover').has(e.target).length === 0) {
//            $(this).popover('hide');
//        }
//    });
//});