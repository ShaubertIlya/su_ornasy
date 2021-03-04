function resizeGrid() {
    $("#GridContainer").height($(".tasksContainer").height() - $("#gridHeader").height() - $("#instructionsNavBar").height() -20);
}

$(window).resize(resizeGrid);

//ViewModel для грида
var vm = {
    RawData: ko.observableArray([]),
    GroupedData: ko.observableArray([]),
    Header: ko.observable("Заголовка"),

    Page: 0,
    working: ko.observable(false),
    _recordsOnPage: 20,
    _sortColumn: "FinishDate",
    _sortDirection: 0,
    _groupColumn: "FinishDate",
    _gotAllData: false,
    _scrollingNow: false,    

    //Owner: "All",
    //Period: "oneMonth",
    //SearchPhrase: "",

    //Метод для получения данных
    getData: function () {

        if (!this._gotAllData) {
            var that = this;
            $.ajax({
                url: '/Document/GetIstructionsNew',
                type: 'GET',
                contentType: "application/json",
                dataType: "json",
                data: {
                    type: docType, //переменная с вьюхи 
                    page: that.Page,
                    recordsOnPage: that._recordsOnPage,
                    sortColumn: that._sortColumn,
                    groupColumn: that._groupColumn,
                    sortDirection: that._sortDirection,
                    //owner: that.Owner,
                    //period: that.Period,
                    //searchPhrase: that.SearchPhrase,
                },
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
        $.get("/Document/GetDocument", { DocumentId: item.RootDocumentId(), isModal: true, Tab:3 }, function (data) {
            $("#documentModal").append(data).show();
            $('#content').height($("#scrolledDocumentAdd").height() - 57 * 2);
        });

        //window.location = "/Document/GetDocument?DocumentId=" + item.RootDocumentId() + "&Tab=3";
    },

    //Показываем/скрываем группировку
    showHideGroup: function (groupItem) {
        groupItem.Visible(!groupItem.Visible());
        scrollReaction();
    },


};
//!-Viewmodel для грида

function setFiltrationForGrid() {
    //vm.Owner = $("#navOwner li.active").attr("id");
    //vm.Period = $("#periodSelect li.selected").attr("id");
    //vm.SearchPhrase = $("#pdpSearch").val();
    vm.clearData();
    vm.getData();
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




    vm._sortColumn = sortColumn;
    vm._sortDirection = sortDirection;
    vm._groupColumn = groupColumn;
    vm.clearData();
    vm.getData();    
}
//!-сортировка

//Обработка скролла
function scrollReaction() {
    if ($("#GridContainer").prop('scrollHeight') - $("#GridContainer").scrollTop() <= $("#GridContainer").height() && !vm._scrollingNow) {
        vm.getData();
    } else {
        return;
    }
}

//$(Document).ready(function () {

    $("#GridContainer").scroll(function () {
        scrollReaction();
    });
    //!-Обработка скролла
    resizeGrid();
    //Применяем байндинги
    ko.applyBindings(vm, document.getElementById("AllDocumentsGrid"));

    //Первоначальная загрузка данных
    setFiltrationForGrid();
    //vm.getData();


    function RefreshInstructionsCount(userdata) {
        $("#instCountAll").html(userdata.instCountAll);
        $("#instCountOutOfDate").html(userdata.instCountOutOfDate);
        $("#instCountInComplete").html(userdata.instCountInComplete);
        $("#instCountControl").html(userdata.instCountControl);
        $("#instCountCompleted").html(userdata.instCountCompleted);
    }

    $.get("/Document/GetInstructionsCount", {}, function (data) {
        RefreshInstructionsCount(data);
    });
//});