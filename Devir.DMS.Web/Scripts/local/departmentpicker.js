var dpDepartmentId = "";
var dpDepartmentName = "";
function InitDepartmentPicker() {
    $(".departmentpicker").each(function () {
        $(this).css("border", 1);
        $(this).css("width", '100%');
        $(this).addClass("input-append");
        //$(".departmentpicker").append("<input id=\"ppUser\" type=\"text\"/><input type=\"hidden\" id='ppUserId' /><button id='ppBtnSelect' class=\"btn\" type=\"button\">Выбрать</button>")
        $(this).append("<button class='dpBtnSelect btn' type=\"button\">Выбрать</button>")
        //$("<div id='dpDialog' class='modal hide fade in' tabindex='-1' role='dialog' aria-labelledby='myModalLabel' aria-hidden='true' style='width: 400px; height: 400px;'></div>").insertAfter(".departmentpicker");

        //$(document).click('.dpBtnSelect', )

        $(this).on("click", ".dpBtnSelect", function () {
            dpDepartmentId = $(this).parent().find("input[type=hidden]").attr("id");
            dpDepartmentName = $(this).parent().find("input[type=text]").attr("id");
            openNewDialogAndLoadData(true, "dpDialog", "/OrganizationStructure/DepartmentPicker");
        });
    });

}