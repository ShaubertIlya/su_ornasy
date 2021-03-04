function resizeGrid() {  
    $("#GridContainer").height($(".rightSide").height() - $("#gridHeader").height() - $(".rightSide .navbar").height()-20);
}

$(window).resize(resizeGrid);

//ViewModel для грида
var AllDocumentGridViewModel = {
    RawData: ko.observableArray([]),
    GroupedData: ko.observableArray([]),
    Header: ko.observable("Заголовок"),   
    isCancelyaria: ko.observable(false),
    
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
   
    //фильтрация
    //DepartmentId: null,
    //StartDate: null,
    //EndDate: null,
    //SearchHeader: "",
    //IsSearchInHeader: false,

    //Метод для получения данных
    getData: function () {       
        
        if (!this._gotAllData) {
            var that = this;
            $.ajax({
                url: '/Document/GetAllDocumentsNew',
                type: 'Post',
                contentType: "application/json",
                dataType: "json",
                data: ko.mapping.toJSON({
                    page: that.Page,
                    recordsOnPage: that._recordsOnPage,
                    sortColumn: that._sortColumn,
                    groupColumn: that._groupColumn,
                    sortDirection: that._sortDirection,
                    owner: that.Owner,
                    period: that.Period,
                    searchPhrase: that.SearchPhrase,

                    documentFilterVM: DocumentFilterVM == null ? null : DocumentFilterVM.RowData()

                    //departmentId: null,
                    //startDate: null,
                    //endDate: null,
                    //searchHeader: null,
                    //isSearchInHeader: false
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
                   
                    that.isCancelyaria(data.isCancelyaria);

                    that.Page++;
                    that._scrollingNow = false;
                    that.working(false);
                    scrollReaction();
                }
            });
            
        }
   
    },

    //Метод очистки грида
    clearData: function(){
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
        //window.location = "/Document/GetDocument?DocumentId="+item.Id();
    },

    //Показываем/скрываем группировку
    showHideGroup: function (groupItem) {    
        groupItem.Visible(!groupItem.Visible());
        scrollReaction();
    },
    
   
};
//!-Viewmodel для грида

function setFiltrationForGrid() {
    AllDocumentGridViewModel.Owner = $("#navOwner li.active").attr("id");    
    AllDocumentGridViewModel.Period = $("#periodSelect li.selected").attr("id");
    AllDocumentGridViewModel.SearchPhrase = $("#pdpSearch").val();
    AllDocumentGridViewModel.clearData();
    AllDocumentGridViewModel.getData();
}

//Сортировка и группировка
function sortAndGroup(elem, sortColumn, groupColumn) {

    $(".ascendingSorting").not(elem).removeClass("ascendingSorting").find("i").removeClass();
    $(".descendingSorting").not(elem).removeClass("descendingSorting").find("i").removeClass();

    var sortDirection = 0;

    if ($(elem).hasClass("ascendingSorting"))
    {
        sortDirection = 0;
        $(elem).removeClass("ascendingSorting").addClass("descendingSorting").find("i").removeClass().addClass("icon-arrow-down");
        
    } else if ($(elem).hasClass("descendingSorting")) {
        sortDirection = 1;
        $(elem).removeClass("descendingSorting").addClass("ascendingSorting").find("i").removeClass().addClass("icon-arrow-up");
    } else {
        sortDirection = 0;
        $(elem).addClass("descendingSorting").find("i").removeClass().addClass("icon-arrow-down");
    }
    



    AllDocumentGridViewModel._sortColumn = sortColumn;
    AllDocumentGridViewModel._sortDirection = sortDirection;
    AllDocumentGridViewModel._groupColumn = groupColumn;
    AllDocumentGridViewModel.clearData();
    AllDocumentGridViewModel.getData();
}
//!-сортировка

//Обработка скролла
function scrollReaction() {  
    if ($("#GridContainer").prop('scrollHeight') - $("#GridContainer").scrollTop()-2 <= $("#GridContainer").height() && !AllDocumentGridViewModel._scrollingNow) {
        AllDocumentGridViewModel.getData();       
    } else {
        return;
    }
}
$("#GridContainer").scroll(function () {
    scrollReaction();
});
//!-Обработка скролла
resizeGrid();

    
//$(document).on('click', '#applyFilter', function () {

//    AllDocumentGridViewModel.DepartmentId = $('#dpDepartmentId').val();

//    var sDate = $('#filterStartDate').data("datepicker");
//    var eDate = $('#filterEndDate').data("datepicker");
   
//    if (sDate !== undefined) {       
//        AllDocumentGridViewModel.StartDate = sDate.date;
//    }
//    if (eDate !== undefined) {        
//        AllDocumentGridViewModel.EndDate = eDate.date;
//    }      
    
//    AllDocumentGridViewModel.SearchHeader = $('#searchHeader').val();
//    AllDocumentGridViewModel.IsSearchInHeader = $('#IsSearchInHeader').is(':checked');

//    AllDocumentGridViewModel.clearData();
//    AllDocumentGridViewModel.getData();

//});

//Применяем байндинги
ko.applyBindings(AllDocumentGridViewModel, document.getElementById("AllDocumentsGrid"));

//Первоначальная загрузка данных
setFiltrationForGrid();
//AllDocumentGridViewModel.getData();




