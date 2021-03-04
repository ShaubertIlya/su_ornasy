function resizeGrid() {
    $("#GridContainer").height($(".rightSide").height() - $("#gridHeader").height() - $(".rightSide .navbar").height() - $("#DropdownFiltr").height() - 20);
}
$(window).resize(resizeGrid);

//ViewModel для грида
var DocumentsByTypeViewModel = {
    RawData: ko.observableArray([]),
    RawDataExport: ko.observableArray([]),
    GroupedData: ko.observableArray([]),
    Header: ko.observable("Заголовка"),
    canViewNumber: ko.observable(false),

    Page: 0,
    working: ko.observable(false),
    _recordsOnPage: 20,
    _sortColumn: "",
    _sortDirection: 0,
    _groupColumn: "gDate",
    _gotAllData: false,
    _scrollingNow: false,

    Owner: "All",
    Period: "oneMonth",
    SearchPhrase: "",
    DocType: docType, //приходит с @ViewBag.DocType
    IdToDynamicFieldFilter: idToDynamicFieldFilter,

    //фильтрация
    //DepartmentId: null,
    //StartDate: null,
    //EndDate: null,
    //SearchHeader: "",
    //IsSearchInHeader: false,
    getDataForExport: function() {
        this.RawDataExport([]);
        var that = this;
        $.ajax({
            url: '/Document/GetDocumentsByTypeNew',
            type: 'Post',
            contentType: "application/json",
            dataType: "json",
            data: ko.mapping.toJSON({
                docType: that.DocType,
                idToDynamicFieldFilter: that.IdToDynamicFieldFilter,
                page: that.Page,
                recordsOnPage: that._recordsOnPage,
                sortColumn: that._sortColumn,
                groupColumn: that._groupColumn,
                sortDirection: that._sortDirection,
                owner: that.Owner,
                period: that.Period,
                searchPhrase: that.SearchPhrase,
                isExcel: true,

                documentFilterVM: DocumentFilterVM == null ? null : DocumentFilterVM.RowData()

                //departmentId: DepartmentId,
                //startDate: StartDate,
                //endDate: EndDate,
                //searchHeader: SearchHeader,
                //isSearchInHeader: IsSearchInHeader,
                //dynamicValue: DynamicValue

            }),
            beforeSend: function () {
                that.working(true);
                that._scrollingNow = true;
            },
            complete: function () {

            },
            success: function (data) {

                //Больше данных нет
                //if (!data.More) {
                //    that._gotAllData = true;
                //}
               
                //Обработка данных
                for (var dataIndex in data.Data) {
                    //Проверка существует группа или нет
                    var index = -1;
                    for (var groupIndex in that.RawDataExport()) {
                        if (that.RawDataExport()[groupIndex].Group() == data.Data[dataIndex].Group)
                            index = groupIndex;
                    }
                    //Если существует то вставляем в группу
                    if (index != -1) {
                        for (var valueIndex in data.Data[dataIndex].Values)
                            that.RawDataExport()[groupIndex].Values.push(ko.mapping.fromJS(data.Data[dataIndex].Values[valueIndex]));
                    } else {
                        //Иначе создаем новую
                        that.RawDataExport.push(ko.mapping.fromJS(data.Data[dataIndex]));
                    }
                }

              

                //that.canViewNumber(data.canViewNumber);

                //that.Page++;
                that._scrollingNow = false;
                that.working(false);


                $.ajax({
                    url: '/Document/SaveReportToSession',
                    type: 'Post',
                    data: { str: $("#exportExcell").html() }
                }).success(function(data) {
                    location.href = "/Document/FormReport";
                });
                

                 //'data:application/vnd.ms-excel,' + ;
              
               // scrollReaction();
            }
        });

    },
    //Метод для получения данных
    getData: function () {
        
        if (!this._gotAllData) {
            var that = this;
            $.ajax({
                url: '/Document/GetDocumentsByTypeNew',
                type: 'Post',
                contentType: "application/json",
                dataType: "json",
                data: ko.mapping.toJSON( {
                    docType: that.DocType,
                    idToDynamicFieldFilter: that.IdToDynamicFieldFilter,
                    page: that.Page,
                    recordsOnPage: that._recordsOnPage,
                    sortColumn: that._sortColumn,
                    groupColumn: that._groupColumn,
                    sortDirection: that._sortDirection,
                    owner: that.Owner,
                    period: that.Period,
                    searchPhrase: that.SearchPhrase,


                    documentFilterVM: DocumentFilterVM == null ? null : DocumentFilterVM.RowData()

                    //departmentId: DepartmentId,
                    //startDate: StartDate,
                    //endDate: EndDate,
                    //searchHeader: SearchHeader,
                    //isSearchInHeader: IsSearchInHeader,
                    //dynamicValue: DynamicValue

                }),
                beforeSend: function () {
                    that.working(true);
                    that._scrollingNow = true;
                },
                complete: function () {

                },
                success: function (data) {

                    //Больше данных нет
                    if (!data.More) {
                        that._gotAllData = true;
                    }

                    //Обработка данных
                    for (var dataIndex in data.Data) {
                        //Проверка существует группа или нет
                        var index = -1;
                        for (var groupIndex in that.RawData()) {
                            if (that.RawData()[groupIndex].Group() == data.Data[dataIndex].Group)
                                index = groupIndex;
                        }
                        //Если существует то вставляем в группу
                        if (index != -1) {
                            for (var valueIndex in data.Data[dataIndex].Values)
                                that.RawData()[groupIndex].Values.push(ko.mapping.fromJS(data.Data[dataIndex].Values[valueIndex]))
                        } else {
                            //Иначе создаем новую
                            that.RawData.push(ko.mapping.fromJS(data.Data[dataIndex]));
                        }
                    }

                    that.canViewNumber(data.canViewNumber);

                    that.Page++;
                    that._scrollingNow = false;
                    that.working(false);
                    scrollReaction();
                }
            });

        }

    },

    //Метод очистки грида
    clearData: function () {
        this.RawData.removeAll();
        this.Page = 0;
        this._gotAllData = false;       
    },

    clickRow: function (item) {

        $("body").append("<div id='documentModal' style='display:none; position: fixed; top:40px; bottom:41px; right: 9px; left:9px;'></div>");
        $.get("/Document/GetDocument", { DocumentId: item.Id(), isModal: true }, function (data) {
            $("#documentModal").append(data).show();
            $('#content').height($("#scrolledDocumentAdd").height() - 57 * 2);
        });

        //window.location = "/Document/GetDocument?DocumentId=" + item.Id();
    },

    //Показываем/скрываем группировку
    showHideGroup: function (groupItem) {
        groupItem.Visible(!groupItem.Visible());
        scrollReaction();
    },


};
//!-Viewmodel для грида

function setFiltrationForGrid() {
    DocumentsByTypeViewModel.Owner = $("#navOwner li.active").attr("id");
    DocumentsByTypeViewModel.Period = $("#periodSelect li.selected").attr("id");
    DocumentsByTypeViewModel.SearchPhrase = $("#pdpSearch").val();   
    DocumentsByTypeViewModel.clearData();
    DocumentsByTypeViewModel.getData();
}

//Сортировка и группировка
function sortAndGroup(elem, sortColumn, groupColumn) {

    $(".ascendingSorting").not(elem).removeClass("ascendingSorting").find("i").removeClass();
    $(".descendingSorting").not(elem).removeClass("descendingSorting").find("i").removeClass();

    var sortDirection = 0;

    if ($(elem).hasClass("ascendingSorting")) {
        sortDirection = 0;
        $(elem).removeClass("ascendingSorting").addClass("descendingSorting").find("i").removeClass().addClass("icon-arrow-down");

    } else if ($(elem).hasClass("descendingSorting")) {
        sortDirection = 1;
        $(elem).removeClass("descendingSorting").addClass("ascendingSorting").find("i").removeClass().addClass("icon-arrow-up");
    } else {
        sortDirection = 0;
        $(elem).addClass("descendingSorting").find("i").removeClass().addClass("icon-arrow-down");
    }




    DocumentsByTypeViewModel._sortColumn = sortColumn;
    DocumentsByTypeViewModel._sortDirection = sortDirection;
    DocumentsByTypeViewModel._groupColumn = groupColumn;
    DocumentsByTypeViewModel.clearData();
    DocumentsByTypeViewModel.getData();
}
//!-сортировка

//Обработка скролла
function scrollReaction() {
    if ($("#GridContainer").prop('scrollHeight') - $("#GridContainer").scrollTop() <= $("#GridContainer").height() && !DocumentsByTypeViewModel._scrollingNow) {
        DocumentsByTypeViewModel.getData();
    } else {
        return;
    }
}
$("#GridContainer").scroll(function () {
    scrollReaction();
});

$('#DynamicFIlterField').change(function () {   
    DocumentsByTypeViewModel.clearData();
    DocumentsByTypeViewModel.IdToDynamicFieldFilter = $('#DynamicFIlterField').val();
    DocumentsByTypeViewModel.getData();
});

//!-Обработка скролла

//$(document).on('click', '#applyFilter', function () {

//    DocumentsByTypeViewModel.DepartmentId = $('#dpDepartmentId').val();

//    var sDate = $('#filterStartDate').data("datepicker");
//    var eDate = $('#filterEndDate').data("datepicker");

//    if (sDate !== undefined) {
//        DocumentsByTypeViewModel.StartDate = sDate.date;
//    }
//    if (eDate !== undefined) {
//        DocumentsByTypeViewModel.EndDate = eDate.date;
//    }

//    DocumentsByTypeViewModel.SearchHeader = $('#searchHeader').val();
//    DocumentsByTypeViewModel.IsSearchInHeader = $('#IsSearchInHeader').is(':checked');

//    DocumentsByTypeViewModel.clearData();
//    DocumentsByTypeViewModel.getData();

//});

resizeGrid();
//Применяем байндинги
ko.applyBindings(DocumentsByTypeViewModel, document.getElementById("DocumentsByTypeGrid"));

//Первоначальная загрузка данных
setFiltrationForGrid();

//DocumentsByTypeViewModel.getData();


