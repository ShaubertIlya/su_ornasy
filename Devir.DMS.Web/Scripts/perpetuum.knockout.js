executeOnServer = function (model, url) {

    $.ajax({
        url: url,
        type: 'POST',
        data: ko.mapping.toJSON(model),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {            
            ko.mapping.fromJS(data, model);
        },
        error: function (error) {
            alert("There was an error posting the data to the server: " + error.responseText);
        }
    });

};

executeOnServerInstructionsSend = function (model, url, documentId, actionId) {
 
    $.ajax({
        url: url,
        type: 'POST',
        data: ko.mapping.toJSON(model),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            
          
            if (typeof data == 'string' || data instanceof String)
            {
            if (data.indexOf("Err:") != -1)
            {
                alert(data.substring(4));
                return;
            }
            }
          
            $.get("/Document/AddSignResult", { DocumentId: documentId, ActionId: actionId }, function (data) {
                $("#ModalSignResult").empty();
                $("#ModalSignResult").html(data);
                $("#ModalSignResult").modal("show");
            });

            ko.mapping.fromJS(data, model);
        },
        error: function (error) {
            alert("There was an error posting the data to the server: " + error.responseText);
        }
    });

};