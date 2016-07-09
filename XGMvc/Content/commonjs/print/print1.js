
$(document).ready(function () {
    $("#btn_print").live("click", function () {
        window.print();
    });
    $("#btn_cancel").live("click", function () {
        window.close();
    });
    LoadData();
});

var LoadData = function () {
    var guid = $("#dataguid").val();
    var scope = $("#scope").val();
    var data = $.view.retrieveDoc(guid, scope);
    var printer = $("div#print"); //打印句柄
    if (printer.length <= 0) return;
    if (data == undefined || (!data.m && !data.m.length > 0)) return;
    //金额
    var c = $("#moneychinese").val();
    var n = $("#moneyunmber").val();
    var cField = $("#moneychineseField").val();
    var numberField = $("#moneyunmberField").val();
    var cFieldArr = cField.split('-');
    var numberFieldArr = numberField.split('-');
    data.m.push({ m: cFieldArr[1], n: cFieldArr[2], v: c });
    data.m.push({ m: numberFieldArr[1], n: numberFieldArr[2], v: n });
    var dataRowCount = data.d[0].r.length;
    var r = [], d = 0.0;
    var ys =5- dataRowCount % 5;
    for (var i = 0; i < dataRowCount; i++) {
        if ((i + 1) % 5 == 0) {
            d += parseFloat(data.d[0].r[i][9].v);
            r.push(data.d[0].r[i]);
            r.push([{ m: "BX_InviteFee", n: "InvitePersonIDCard", v: "合计"}]);
            d = 0.0;
        } else {
            d += parseFloat(data.d[0].r[i][9].v);
            r.push(data.d[0].r[i]);
            if (i == dataRowCount - 1) {
                for (var j = 0; j < ys; j++) {
                    r.push([{ m: "BX_InviteFee", n: "InvitePersonName", v: "" }, { m: "BX_InviteFee", n: "Total_Real", v: ''}]);
                }
                r.push([{ m: "BX_InviteFee", n: "InvitePersonIDCard", v: "合计" }, { m: "BX_InviteFee", n: "Total_Real", v: ''}]);
            }
        }

    }
    data.d[0].r = r;
    $Print.jqprint(data, printer);


}

