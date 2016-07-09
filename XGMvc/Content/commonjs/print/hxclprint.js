/*核销中的汇款审批单*/
$(document).ready(function () {

    LoadData();
    BindEvent();
});
var BindEvent = function () {

    $("#btn_print").live("click", function () {
        var arr = document.getElementsByTagName("td");
        for (var i = 0; i < arr.length; i++) {
            arr[i].style.border = "0px";
        }
       
        window.print();
        for (var i = 0; i < arr.length; i++) {
            arr[i].style.border = "1px solid #000000";
        }
    });
    $("#btn_cancel").live("click", function () {
        window.close();
        parent.window.opener = null;
        window.parent.close();
    });
}

var LoadData = function () {
    //GetPrintData
    
    var guid = $("#dataguid").val();
    var scope = $("#scope").val();
    var doctypekey = $("#doctypekey").val();
    var guidpaymentnumber = $("#paymentnumber").val();
    var url = '/hxcl/GetPrintData';
    var argData = { "guid": guid, "doctypekey": doctypekey, "paymentnumber": guidpaymentnumber };
    var data = $Common.ajaxData(url, argData);
    if (!data) return;
    
    var modelName = $("table").attr("id");
    $Common.SetInputValue(modelName, data);
    var numberMoney = (data.Total_BX == null || data.Total_BX == undefined) ? 0 : parseInt(data.Total_BX).formatThousand(data.Total_BX);

    $Common.SetInputNumberToMoney(modelName, numberMoney);

    $("#" + modelName + "-moneychinese").val((data.Total_BX + "").moneytransferchinese());
}



