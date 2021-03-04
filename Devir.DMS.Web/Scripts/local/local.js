var myApp;
myApp = myApp || (function () {
    var pleaseWaitDiv = $('<div class="modal hide" id="pleaseWaitDialog" data-backdrop="static" data-keyboard="false"><div class="modal-header"><h1>Обработка ...</h1></div><div class="modal-body"><div class="progress progress-striped active"><div class="bar" style="width: 100%;"></div></div></div></div>');
    return {
        showPleaseWait: function () {
            pleaseWaitDiv.modal();
        },
        hidePleaseWait: function () {
            pleaseWaitDiv.modal('hide');
        },

    };
})();


//Добавляем кнопки в div #buttons
function AddButton(list, addUrl, editUrl, delUrl, buttonsDiv, modalDivId) {
    var onlyText = $(buttonsDiv).hasClass("onlyText");
    var onlyIcons = $(buttonsDiv).hasClass("onlyIcons");
    var addButton =
       $("<button id='AddButton' class='btn' type='button' style='margin: 0 4px 4px 0'></button>"
   ).click(function () {
       openNewDialogAndLoadData(true, modalDivId, addUrl);
   });
    if ($(buttonsDiv).find("#AddButton").length) {
        //Нифига не делаем
    } else {
        $(addButton).appendTo(buttonsDiv);
    }


    var editButton =
        $("<button id='EditButton' class='btn' type='button' style='margin: 0 4px 4px 0'></button>"
        ).click(function () {
            var grid = jQuery(list);
            var entryId = grid.jqGrid('getGridParam', 'selrow');
            if (entryId == null) {
                info('.top-right', 'Выберите запись в таблице для редактрирования');
            } else {
                var urlStr = editUrl;
                if (editUrl.indexOf('?') != -1) {
                    urlStr = editUrl + "&Id=" + entryId;
                } else {
                    urlStr = editUrl + "?Id=" + entryId;
                }
                openNewDialogAndLoadData(true, modalDivId, urlStr);
            }
        });

    if ($(buttonsDiv).find("#EditButton").length) {
        //если div уже существует, то нифига не делаем
    } else {
        $(editButton).appendTo(buttonsDiv);
    }

    var delButton =
        $("<button id='DelButton' class='btn' type='button' style='margin: 0 4px 4px 0'></button> "
        ).click(function () {
            var grid = jQuery(list);
            var entryID = grid.jqGrid('getGridParam', 'selrow');
            if (entryID == null) {
                info('.top-right', 'Выберите запись в таблице чтобы удалить');
            } else {
                bootbox.confirm("Вы действительно хотите удалить запись?", "Отменить", "Удалить", function (result) {
                    if (result) {
                        jQuery.ajax({
                            url: delUrl,
                            data: '{"Id":"' + entryID + '"}',
                            dataType: "json",
                            type: 'POST',
                            contentType: "application/json; charset=utf-8",
                            success: function (xmldata, stat) {
                                eval(xmldata);
                                //info('.top-right', 'Запись удалена');
                                //grid.trigger("reloadGrid");
                            },
                            error: function (xml) {
                                var data = eval("(" + xml.responseText + ")");
                                $.growlUI(data.Message, "");
                            }
                        });
                    }
                });
            }
        });
    if ($(buttonsDiv).find("#DelButton").length) {
        //если div уже существует, то нифига не делаем
    } else {

        $(delButton).appendTo(buttonsDiv);
    }


    if (!onlyText) {
        addButton.append('<i class="icon-plus icon-black"></i>');
        editButton.append('<i class="icon-pencil icon-black"></i>');
        delButton.append('<i class="icon-trash icon-black"></i>');
    }
    if (!onlyIcons) {
        addButton.append(' Добавить');
        editButton.append(' Редактировать');
        delButton.append(' Удалить');
    }

}

var OpenModalLoaded = null;

function openNewDialogAndLoadData(bool, modalDivId, loadUrl, divClass, width) {
    if (!divClass)
        divClass = "";
    isNewEntry = bool;
    currentModal.push(modalDivId);
    $("#addModal_" + modalDivId).remove();

    $("<div id='addModal_" + modalDivId + "'  class='modal hide fade in " + divClass + "' tabindex='-1' style='display:none; width:800px' role='dialog' aria-labelledby='myModalLabel' aria-hidden='true'></div>").appendTo($("body"));
    //$("#addModal_" + modalDivId).empty();
    
    
   
    $("#addModal_" + modalDivId).load(loadUrl, null, function (response, status, xhr) {
        $("#addModal_" + modalDivId).modal('show');
        $(document).on('shown', "#addModal_" + modalDivId, function (e) {          
            $("#addModal_" + modalDivId).find("input[type=text],textarea,select").filter(":visible:first").focus();
        });

        $(document).on('hidden', "#addModal_" + modalDivId, function (e) {
            CloseCurrentModal();
        });
        if (status === 'success' && typeof OpenModalLoaded === 'function')
            OpenModalLoaded();
    });

   

}

//из Скайпа )))
//[15.08.2013 19:14:09] Каримов Алибек Каримович: функция в модал форму че бы она обновлялась при валидации
function fixFormTarget() {
    var divId = "#addModal_" + currentModal[currentModal.length - 1];
    var form = $(divId).find("form")[0];
    $(form).attr('data-ajax-update', divId);
}

function jqGridTable(tableName, list, pager, getData, deleteData, updateData, addData, cn, cm, mtype, width, buttonsDiv, ModalDivId) {
    //if (!width)
    //    width = 'auto';
    jQuery(list).jqGrid({
        //width: width,
        height: 450,
        autowidth: true,
        shrinkToFit: true,
        singleSelect: true,
        viewrecords: true,
        datatype: "json",
        url: getData,
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            cell: "cell",
            id: "Id",
            userdata: "userdata"
        },
        colNames: cn,
        colModel: cm,
        rowNum: 20,
        scroll: 1,
        loadonce: false,
        rownumbers: true,
        gridview: true,
        hidegrid: false,
        sortname: 'Id',
        sortable: true,
        mtype: "Post",
        //beforeRequest: function () {
        //      if ($(list).find("tr").length > 1) {
        //        var height = $(list).outerHeight();
        //        var tr = $(list).find("tr")[1];
        //        var trHeight = $(tr).outerHeight();
        //        var rowNum = height % trHeight;
        //        $(list).jqGrid("setGridParam", {
        //            rowNum : rowNum
        //        });
        //    }
        //}
    });
    AddButton(list, addData, updateData, deleteData, buttonsDiv, ModalDivId);
}

function jqGridGroupTable(list,
                            getData,
                            deleteData,
                            updateData,
                            addData,
                            cn,
                            cm,
                            groupFieldName,
                            groupColumnShow,
                            sortname,
                            sortorder,
                            rowNum,
                            scroll,
                            serializeGridData,
                            gridComplete,
                            buttonsDiv,
                            ModalDivId
                            ) {
    //if (!width)
    //    width = 'auto';
    if (!rowNum)
        rowNum = 20;
    if (!scroll)
        scroll = 1;
    jQuery(list).jqGrid({
        //width: width,
        height: 450,
        autowidth: true,
        shrinkToFit: true,
        singleSelect: true,
        viewrecords: true,
        datatype: "json",
        url: getData,
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            cell: "cell",
            id: "Id",
            userdata: "userdata"
        },
        colNames: cn,
        colModel: cm,
        rowNum: rowNum,
        scroll: scroll,
        loadonce: false,
        rownumbers: true,
        gridview: true,
        hidegrid: false,
        sortname: sortname,
        sortorder: sortorder,
        sortable: true,
        mtype: "POST",
        grouping: true,
        groupingView: {
            groupField: [groupFieldName],
            groupDataSorted: false,
            groupColumnShow: groupColumnShow,
            groupOrder: "desc"
        },
        loadComplete: function (data) {
            var rows = $(list).jqGrid('getDataIDs');
            $.each(rows, function () {
                var classes = $(list).getCell(this, "Classes");
                //alert(classes);
                $("#" + this).addClass(classes);
            });

        },
        serializeGridData: serializeGridData,
        gridComplete: gridComplete


    });
    AddButton(list, addData, updateData, deleteData, buttonsDiv, ModalDivId);
}

function jqGridTableSerialize(list, getData, deleteData, updateData, addData, cn, cm, serializeGridData, loadComplete) {
    //if (!width)
    //    width = 'auto';
    jQuery(list).jqGrid({
        //width: width,
        height: 450,
        autowidth: true,
        shrinkToFit: true,
        singleSelect: true,
        viewrecords: true,
        datatype: "json",
        url: getData,
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            cell: "cell",
            id: "Id",
            userdata: "userdata"
        },
        colNames: cn,
        colModel: cm,
        rowNum: 20,
        scroll: 1,
        loadonce: false,
        rownumbers: true,
        gridview: true,
        hidegrid: false,
        sortname: 'Id',
        sortable: true,
        mtype: "Post",
        serializeGridData: serializeGridData,
        loadComplete: loadComplete
        //beforeRequest: function () {
        //      if ($(list).find("tr").length > 1) {
        //        var height = $(list).outerHeight();
        //        var tr = $(list).find("tr")[1];
        //        var trHeight = $(tr).outerHeight();
        //        var rowNum = height % trHeight;
        //        $(list).jqGrid("setGridParam", {
        //            rowNum : rowNum
        //        });
        //    }
        //}
    });

}

function jqGridTemplates(tableName, typeTable, cn, cm, func, buttonsDiv, ModalDivId) {
    if (!buttonsDiv)
        buttonsDiv = "#buttons";
    var list = "#list";
    var pager = "#pager";
    jQuery(list).jqGrid({
        url: 'GetDataFor' + typeTable,
        datatype: "json",
        jsonReader:
        {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            id: 'Id',
            repeatitems: false
        },
        colNames: cn,
        colModel: cm,
        scroll: 1,
        loadonce: false,
        rowNum: 20,
        height: 'auto',
        autowidth: true,
        sortname: 'Id',
        viewgrid: true,
        viewrecords: true,
        sortable: true,
        mtype: 'Post',
        rownumbers: true,
        onSelectRow: function (id) { if (func != null) func(id); },
    });

    AddButton(list, 'Add' + typeTable, 'Update' + typeTable, 'Delete' + typeTable, buttonsDiv, ModalDivId);


    //jQuery(list).trigger("reloadGrid");
}



function jqGrid() {
    jQuery(list).jqGrid({
        url: getData,
        datatype: "json",
        jsonReader: { id: 'Id', repeatitems: false },
        colNames: cn,
        colModel: cm,
        rowNum: 10,
        rowList: [10, 30],
        width: 900,
        hidegrid: false,
        pager: pager,
        sortname: 'Id',
        viewrecords: true,
        sortorder: "desc",
        caption: tableName
    }).navGrid(pager, {
        edit: false,
        add: false,
        del: false,
        search: false
    }).navButtonAdd(pager, {
        buttonicon: "icon-trash",
        caption: "&nbsp;",
        id: "deleteButton",
        onClickButton: function (id) {
            var grid = jQuery(list);
            var entryID = grid.jqGrid('getGridParam', 'selrow');
            if (entryID == null) {
                info('.top-right', 'Выберите запись в таблице чтобы удалить');
            } else {
                jQuery.ajax({
                    url: deleteData,
                    data: '{"Id":"' + entryID + '"}',
                    dataType: "json",
                    type: 'POST',
                    contentType: "application/json; charset=utf-8",
                    success: function (xmldata, stat) {
                        info('.top-right', 'Запись удалена');
                        grid.trigger("reloadGrid");
                    },
                    error: function (xml) {
                        var data = eval("(" + xml.responseText + ")");
                        $.growlUI(tableName, data.Message, "");
                    }
                });
            }
        },
        position: "first"
    }).navButtonAdd(pager, {
        buttonicon: "icon-pencil",
        caption: "&nbsp;",
        id: "editButton",
        onClickButton: function (id) {
            var grid = jQuery(list);
            var entryId = grid.jqGrid('getGridParam', 'selrow');
            if (entryId == null) {
                info('.top-right', 'Выберите запись в таблице для редактрирования');
            } else {
                openDialog(false);
                $("#addModal").load(updateData + "?Id=" + entryId);
            }
        },
        position: "first"
    }).navButtonAdd(pager, {
        buttonicon: "ui-icon-plus",
        caption: "Добавить&nbsp;",
        onClickButton: function (id) {
            openDialog(true);
            $("#addModal").load(addData);
        },
        position: "first"
    });
}

function jqGridDocuments() {
    var list = "#list";
    var pager = "#pager";
    jQuery(list).jqGrid({
        url: 'GetDataFor' + typeTable,
        datatype: "json",
        jsonReader: { id: 'Id', repeatitems: false },
        colNames: cn,
        colModel: cm,
        rowNum: 10,
        rowList: [10, 30],
        width: 900,
        hidegrid: false,
        pager: pager,
        sortname: 'Id',
        viewrecords: true,
        sortorder: "desc",
        caption: tableName,
        onSelectRow: function (id) { OnSelectRow(id); },
    }).navGrid(pager, {
        edit: false,
        add: false,
        del: false,
        search: false
    });
    jQuery(list).trigger("reloadGrid");
}


function jqGridTreeTable(tableName,
    list,
    expandColumn,
    pager,
    getData,
    deleteData,
    updateData,
    addData,
    cn,
    cm,
    headertitles,
    width,
    id,
    buttonsDiv) {
    if (!buttonsDiv)
        buttonsDiv = "buttons";
    jQuery(list).jqGrid({
        url: getData,
        datatype: "json",
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            cell: "cell",
            id: id,
            userdata: "userdata"
        },
        colNames: cn,
        colModel: cm,
        //rowNum: 10,
        //rowList: [10, 30],
        width: width,
        hidegrid: false,
        pager: pager,
        sortname: id,
        viewrecords: true,
        sortorder: "desc",
        caption: tableName,
        treeGrid: true,
        treeGridModel: "adjacency",
        ExpandColumn: expandColumn,
        headertitles: headertitles,
        id: id
    }).navGrid(pager, {
        edit: false,
        add: false,
        del: false,
        search: false
    });


    AddButton(list, addData, updateData, deleteData, buttonsDiv);

}

function openDialog(bool) {
    isNewEntry = bool;
    $("#addModal").empty();
    $("#addModal").modal('show');
    //.css({ 'max-width': '1000px', 'margin-left': function() { return -($(this).width() / 2); } })
}


function info(position, text) {
    $(position).notify({
        message: { text: text },
        type: 'bangTidy'
    }).show();
}

function replaceAll() {
    $('#field .content').each(function (indx, fields) {
        $(fields).children().each(function (idx, field) {
            field = field.tagName == "INPUT" ? field : field.children;
            $(field).attr('name', function (i, val) {
                if (typeof val != 'undefined') {
                    return val.replace(/\d+/, function (n) { return indx; });
                }
            });
            if ($(field).hasClass("order")) {
                $(field).attr('value', indx);
            }
        });
    });
}

function EditModal(action, detail, id) {
    $('#addModal').modal('hide');
    $('#edit-modal').modal('hide');
    $('#list').trigger('reloadGrid');
    info('.top-right', action);
    if (id != null) {
        $("#details").load(detail + "?id=" + id);
    } else {
        $("#details").empty();
    }

}


$(".isDisplay").change(function () {
    if ($("input:checked.isDisplay").length > 0) {
        $("input:checkbox.isDisplay").first().attr('value', true).attr('checked', 'checked');
    }
});

$(document).on('click', 'input:checkbox.isRequired', function () {
    $('input:checkbox.isRequired').each(function (indx, element) {
        if ($(this).prop('checked'))
            $(this).attr('value', true).attr('checked', 'checked');
        else
            $(this).attr('value', false).removeAttr('checked');
    });
});
$(document).on('click', 'input:checkbox.isDisplay', function () {
    $('input:checked.isDisplay').not(this).removeAttr('checked').attr('value', false);
    $(this).attr('value', true);
});

$(document).on('click', '#up', function () {
    var current = $(this).parent().parent('.content');
    current.prev().before(current);
});
$(document).on('click', '#down', function () {
    var current = $(this).parent().parent('.content');
    current.next().after(current);
});

$(document).on('switch-change', '#mySwitch', function (e, data) {
    var $el = $(data.el), value = data.value;
    console.log(e, $el, value);
});

//Скрыть наименование колонок для jqGrid

function HideColumnNames(list) {
    $(list).parents("div.ui-jqgrid-view").children("div.ui-jqgrid-hdiv").hide();
}


//Закрыть текущее модальное окно

function CloseCurrentModal() {
    var div = $('#addModal_' + currentModal.pop());
   
    div.modal('hide');
    div.remove(); //Чистим буфера (*)(*)
    OpenModalLoaded = null;
}

function CloseRefModal(vbAddModal, drf, dfn, list) {

    var dataid = jQuery(list).jqGrid('getGridParam', 'selrow');
    //var data = jQuery(list).jqGrid('getRowData', dataid);
    var dataCol = jQuery(list).jqGrid('getCell', dataid, dfn);
   
    var div = $('#addModal_' + currentModal.pop());
    div.modal('hide');
    div.remove(); //Чистим буфера (*)(*)

    $('#appendedInputButton_' + vbAddModal).val(dataCol);
    $('#' + drf).val(dataid);
}


//Получить параметр из адресной строки   
function getURLParameter(name) {
    return decodeURI(
        (RegExp(name + '=' + '(.+?)(&|$)').exec(location.search) || [, null])[1]
    );
}



//Получить название месяца
function GetMonth(month) {
    if (month == 0)
        return "Января";
    if (month == 1)
        return "Февраля";
    if (month == 2)
        return "Марта";
    if (month == 3)
        return "Апреля";
    if (month == 4)
        return "Марта";
    if (month == 5)
        return "Июня";
    if (month == 6)
        return "Июля";
    if (month == 7)
        return "Августа";
    if (month == 8)
        return "Сентября";
    if (month == 9)
        return "Октября";
    if (month == 10)
        return "Ноября";
    if (month == 11)
        return "Декабря";
    return "";
}

//Форматеры
function timeFormatter(cellval, opts, rwdat, act) {
    var hours = cellval.Hours.toString();
    if (hours.length == 1)
        hours = "0" + hours;
    var minutes = cellval.Minutes.toString();
    if (minutes.length == 1)
        minutes = "0" + minutes;
    return hours + ":" + minutes;
}

function documentDateFormatter(cellval, opts, rwdat, act) {
    var start = parseInt(cellval.replace(/(^.*\()|([+-].*$)/g, ''), 10);//cellval.replace(/\/Date\((.*?)[+-]\d+\)\//i, "$1")
    var date = new Date(start);
    var dd = date.getDate();
    var dm = date.getMonth();
    var dy = date.getFullYear();
    var today = new Date();
    var td = today.getDate();
    var tm = today.getMonth();
    var ty = today.getFullYear();
    if (date.getDate() == today.getDate())
        return "Сегодня, " + dd + " " + GetMonth(dm);

    if (tm != 0)
        var pmld = new Date(ty, tm - 1, 0);
    if (dd == (td - 1) && dm == tm && dy == ty
        || td == 1 && pmld && pmld.getDate() == date.getDate()
        || td == 1 && tm == 0 && dd == 31 && dm == 11 && (dy + 1) == ty)
        return "Вчера, " + dd + " " + GetMonth(dm);

    if (dd == (td - 2) && dm == tm && dy == ty
        || pmld && pmld == date && td == 2
        || pmld && new Date(pmld.getFullYear(), pmld.getMonth() + 1, pmld.getDay() - 1).getDate() == date.getDate() && td == 1
        || (dy + 1) == ty && tm == 0 && dm == 11 && td == 2 && dd == 30
        || (dy + 1) == ty && tm == 0 && dm == 11 && td == 1 && dd == 29
        )
        return "Позавчера, " + dd + " " + GetMonth(dm);
    return dd.toString() + " " + GetMonth(dm) + " " + dy.toString();
}

function tasksGroupFormatter(cellval, opts, rwdat, act) {
    if (cellval == 0)
        return "Просроченные";
    if (cellval == 1)
        return "Сегодня";
    if (cellval == 2)
        return "Завтра";
    if (cellval == 3)
        return "Предстоящие задачи";
}



//Функции для FileUpload

//используются в AddSignResult
function btnOkFileInModal() {
    //e.preventDefault();
    $("#uploadFormInModal").submit();
    $("#dlgFileUpload_inModal").modal('hide');
    $("#dlgFileUploadBody_inModal").html(""); //чистим буфера (*)(*)
}

function showModalInModal() {

    $('<div id="dlgFileUpload_inModal" class="modal hide fade">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' +
        '<h3>Загрузить файл</h3></div><div class="modal-body">' +
        '<div id="dlgFileUploadBody_inModal">' +
        '</div></div>' +
        '<div class="modal-footer">' +
        '<a id="modal-form-submit" onclick="btnOkFileInModal(); return false;" class="btn btn-primary" href="#">Ок</a></div></div>').appendTo($("body"));

    $("#dlgFileUploadBody_inModal").load("/File/UploadFileModalTOModal");
    $("#dlgFileUpload_inModal").modal('show');

}

//используются в AddDocument
function btnOkFile() {
    e.preventDefault();
    $("#uploadForm").submit();
    $("#dlgFileUploadBody").html(""); //чистим буфера (*)(*)
}

function showModal() {

    $("#dlgFileUploadBody").load("/File/UploadFileModal");
    $("#dlgFileUpload").modal('show');

    //привязываем сабмит формы к клику
    $("#modal-form-submit").click(function (e) {
        e.preventDefault();
        $("#uploadForm").submit();
        $("#dlgFileUploadBody").html(""); //чистим буфера (*)(*)
        //return false;
    });
}

// удалить файл (клик по корзине)
$(document).on('click', '#btnRemoveFile', function () {
    $(this).parent('.uploadedFile').remove();
    RecalculateIndexes();
});

function RecalculateIndexes() {
    var i = 0;
    $('#attachments').find('.uploadedFile').each(function () {
        $(this).find('input[type=hidden]').attr('name', 'attachment[' + i + ']');
        i++;
    });

    var x = 0;
    $('#attachments_inModal').find('.uploadedFile').each(function () {
        $(this).find('input[type=hidden]').attr('name', 'attachment[' + x + ']');
        x++;
    });
}


//var AttachmentsDivCount;
//var FieldsForAttachments;

function getImageFromScaner() {
    $(".submitter").hide();

    setTimeout(function () {
        $(".submitter").show();
    }, 10000);
    
    $.getJSON('http://127.0.0.1:6143/GetScanedImage?callback=?', function (data) {
        $(".submitter").hide();
        scanned(data);       
    });
}

function scanned(data) {
    //AttachmentsDivCount = $('#attachments .uploadedFile').size();
    
    //FieldsForAttachments =
    //   '<input type="hidden" name="attachment[' + AttachmentsDivCount + ']" value="' + data + '"/>' +
    //   '<a style="margin: 5px 15px; float:left;" class="btn" href="#" id="btnRemoveFile"><i class="icon-trash"></i></a>';

    $.get("/File/ShowUploadedFile?guid=" + $.trim(data), function (s) {
        $('#attachments').append(s);
        $(".submitter").show();
    });
};








//обновление списка динамических справочников (Меню слева)

function RefreshLeftMenu() {
    $.ajax({
        url: "/Reference/GetDynamicReferenceName",
        success: function (data) {
            $('.dynamicReference').remove();

            $(data).each(function () {
                $('<li class="sub dynamicReference"><a id="ShowDynamicReference?Id=' + this.Id + '"  href="#gridArea"> ' + this.name + ' </a></li>').appendTo('#leftNav');
            });
        },
    });
}


