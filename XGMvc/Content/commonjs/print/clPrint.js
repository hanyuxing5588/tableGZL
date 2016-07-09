//差旅报销
$(document).ready(function () {
    LoadData();
    BindEvent();
});
var BindEvent = function () {
    
    $("#btn_print").live("click", function () {       
        window.print();
    });
    $("#btn_cancel").live("click", function () {
        window.close();
    });
}

var LoadData = function () {
    debugger
    var data = $("#dataid").val();
    data = eval('(' + data + ')');
    var printer = $("div#print"); //打印句柄
    if (printer.length <= 0) return;
    if (!data.m && !data.m.length > 0) return;
    $Print.jqprint(data, printer, true);
    $("#BX_Main-moneyunmber").text("￥" + $("#BX_Main-moneyunmber").text());

}


