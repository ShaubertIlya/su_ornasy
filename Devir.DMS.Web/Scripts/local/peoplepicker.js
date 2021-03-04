var ppUserId = "";
var ppUserName = "";


function InitPeoplePicker() {
    $("#ppDialog").remove();
    $("<div id='ppDialog' class='modal hide fade in' tabindex='-1' role='dialog' aria-labelledby='myModalLabel' aria-hidden='true' style='width: 700px,height: 500px; '></div>").appendTo($("body"));


    $(".peoplepicker").each(function () {
        $(this).css("border", 1);
        $(this).css("width", '100%');
        $(this).addClass("input-append");
        //$(".peoplepicker").append("<input id=\"ppUser\" type=\"text\"/><input type=\"hidden\" id='ppUserId' /><button id='ppBtnSelect' class=\"btn\" type=\"button\">Выбрать</button>")
        $(this).find(".ppBtnSelect").remove();
        $(this).append("<button class='ppBtnSelect btn' type=\"button\">...</button>");
       
        $(this).find(".ppBtnSelect").click(function () {
            ppUserId = $(this).parent().find("input[type=hidden]").attr("id");
            ppUserName = $(this).parent().find("input[type=text]").attr("id");
            //openDialog(true);
            $("#ppDialog").load('/Users/PeoplePicker').modal({
                show: true
            });
    });

  
        //.dialog({
        //    title: "Выбрать пользователя",
        //    modal: true,
        //    autoOpen: false,
        //    width: 'auto',
        //    height: 'auto',
        //}).dialog("open");
    });

}

var pdpUserId = "";
var pdpUserName = "";
var pdpCallbackArray = new Array();
var pdpCurId = "";
var isMultiple = false;

function InitPeopleByDepartmentPicker() {
    
    //$("#pdpDialog").remove();
    //$("<div id='pdpDialog' class='modal hide fade in' tabindex='-1' role='dialog' aria-labelledby='myModalLabel' aria-hidden='true' style='width: 700px,height: 500px; '></div>").appendTo($("body"));

    $(".pdPicker").each(function () {
        $(this).css("border", 1);
        $(this).css("width", '100%');
        $(this).addClass("input-append");
        
        if (!$(this).hasClass("multiple")) 
        {
            $(this).find(".pdpBtnSelect").remove();
            $(this).append("<button class='pdpBtnSelect btn' type=\"button\">...</button>");
        }
        //$(".peoplepicker").append("<input id=\"ppUser\" type=\"text\"/><input type=\"hidden\" id='ppUserId' /><button id='ppBtnSelect' class=\"btn\" type=\"button\">Выбрать</button>")
        
        // $(this).find(".pdpBtnSelect").click(function () {
        $(document).on("click", ".pdpBtnSelect", function () {
            var parentDiv = $(this).parents(".pdPicker");
            if (!$(parentDiv).hasClass("multiple")) {
                pdpUserId = $(this).parent().find("input[type=hidden]").attr("id");
                pdpUserName = $(this).parent().find("input[type=text]").attr("id");
            } else
            {
                isMultiple = true;
            }
            pdpCurId = $(parentDiv).attr("id");
            openNewDialogAndLoadData(true, "pdpDialog", "/Users/PeopleByDepartmentPicker");
            //$("#pdpDialog").load('/Users/PeopleByDepartmentPicker').modal({
            //    show: true
            //});

        });

       
    });

}

function SetCallback(id, func)
{
    pdpCallbackArray[id] = func;
}