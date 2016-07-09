
$(document).ready(function () {
    
    LoadData();
    BindEvent();
});
var BindEvent = function () {

    $("#btn_print").bind("click", function () {
        var arr = document.getElementsByTagName("td");
        for (var i = 0; i < arr.length; i++) {
            arr[i].style.border = "0px";
        }
        window.print();
        for (var i = 0; i < arr.length; i++) {
            arr[i].style.border = "1px solid #000000";
        }
    });
    $("#btn_cancel").bind("click", function () {
        window.close();
    });
}

var LoadData = function () {

    var guid = $("#dataguid").val();
    if (!guid) return;
    var scope = $("#scope").val();
    var data = ajaxData(guid);
    if (data) {
        var docType = (data.YWDocList || {}).DocTypeKey;
        SetPageValue(data.CN_CheckList[0], (data.YWDocList || []).DetailList[0], docType);
    }

}
var SetPageValue = function (data, ywData, docType) {
    var date = new Date();
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    debugger
    $("#xjtq-Year").html(year);
    $("#xjtq-Month").html(month);
    $("#xjtq-Day").html(day);
    if (docType == "21") { //现金提取
        $('#xjtq-PaymentNumber').html(data.PaymentNumber);
    } else {
        if (ywData) {
            $('#xjtq-PaymentNumber').html(ywData.PaymentNumber);
        } 
    }

}
var ajaxData = function (guid) {
    var data;
    var url = '/zplq/RetrieveModelByGuid';
    $.ajax({
        url: url,
        async: false, //同步
        data: { "guid": guid },
        dataType: "json",
        type: "POST",
        traditional: true,
        error: function (xmlhttprequest, textStatus, errorThrown) {

        },
        success: function (response) {
            data = response;
        }
    });
    return data;
}



