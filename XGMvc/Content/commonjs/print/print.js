$(document).ready(function () {
    LoadData();
    BindEvent();
});
var BindEvent = function () {//直接
    $("#btn_print").live("click", function (){       
        window.print();
    });
    $("#btn_cancel").live("click", function () {//直接
        window.close();
    });
}

var LoadData = function () {
    var guid = $("#dataguid").val();
    var scope = $("#scope").val();
    var data = $.view.retrieveDoc(guid, scope);
    var printer = $("div#print"); //打印句柄
    if (printer.length <= 0) return;
    if (data==undefined || (!data.m && !data.m.length > 0)) return;
    //金额
    var c = $("#moneychinese").val();
    var n = $("#moneyunmber").val();
    var cField = $("#moneychineseField").val();
    var numberField = $("#moneyunmberField").val();
    var cFieldArr = cField.split('-');
    var numberFieldArr = numberField.split('-');
    data.m.push({ m: cFieldArr[1], n: cFieldArr[2], v: c });
    data.m.push({ m: numberFieldArr[1], n: numberFieldArr[2], v: n });
    $Print.jqprint(data, printer);
}

