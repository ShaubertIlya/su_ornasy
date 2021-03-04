function CreateJqGrid(GridContainer, GetJSONDataURL, DeleteDataURL, UpdateDataURL, AddDataURL, ColNames, ColModel, ButtonsDIV, ModalDivId, OnDblClick) {
    
    jQuery(GridContainer).jqGrid({
        height: 350,       
        autowidth: 1,       
        singleSelect: true,
        viewrecords: true,
        datatype: "json",
        url: getData,
        jsonReader: { id: 'Id', repeatitems: false },
        colNames: ColNames,
        colModel: ColModel,
        rowNum: 20,
        scroll: 1,
        loadonce: false,      
        gridview: true,
        hidegrid: false,
        pager: pager,
        sortname: 'Id',
        sortable: 'true',
        mtype: "Post",
        ondblClickRow: OnDblClick
    })

    AddButton(GridContainer, AddDataURL, UpdateDataURL, DeleteDataURL, ButtonsDIV, ModalDivId);

}