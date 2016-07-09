
$(document).ready(function () {
    
    LoadData();
    BindEvent();
});
var BindEvent = function () {

    $("#btn_cancel").bind("click", function () {
        //window.parent.close();
        parent.window.opener = null;
        window.parent.close();
        parent.open("", "_self");
        parent.window.close();
    });
}

var LoadData = function () {

    var guid = $("#dataguid").val();
    if (!guid) return;
    var scope = $("#scope").val();
    var data = ajaxData(guid);
    debugger
    if (data) {//PaymentNumberdata.YWDocList.DetailList[0].PaymentNumber
        var docType = (data.YWDocList || {}).DocTypeKey;
        SetPageValue(data.CN_CheckList[0], (data.YWDocList || []).DetailList[0], docType);
    }

}
var SetPageValue = function (data, ywData, docType) {
    debugger
    var datetime = data.CheckDrawDatetime == null ? "" : data.CheckDrawDatetime;
    var datetimeArr = datetime.split("-");
    var year = datetimeArr[0];
    var month = datetimeArr[1];
    var day = datetimeArr[2];
    if (docType == "21" || docType == "20") { //现金提取
        $('#zplq-DocMemo').html(data.PaymentNumber);
    } else {
        if (ywData) {
            $('#zplq-DocMemo').html(ywData.PaymentNumber);
        }
    }
    //时间
    $("#zplq-Year").html(year);
    $("#zplq-Month").html(month);
    $("#zplq-Day").html(day);
    //票据日期
    $("#zplq-uYear").html(moneytransfer.numberTransferChinese(year));
    debugger
    $("#zplq-uMonth").html(moneytransfer.numberTransferChinese1(month));
    $("#zplq-uDay").html(moneytransfer.numberTransferChinese1(day));
    //金额
   
    //票据金额

    var checkMoney = (data.CheckMoney == null || data.CheckMoney == undefined) ? 0 : parseInt(data.CheckMoney).formatThousand(data.CheckMoney);

    //var checkMoney = data.CheckMoney.formatThousand();
    checkMoney = checkMoney.replace(",", "").replace(",", "").replace(",", "").replace(".", "").replace(" ", "");
    $("#zplq-CheckMoney").html(data.CheckMoney);
    var obj = NumberToMoney(checkMoney);
    for (var i in obj) {
        $("#zplq-" + i).html(obj[i]);
    }
    //用途
    if (docType == "21" || docType == "20") { //现金提取
        $("#zplq-CheckUsed").html(ywData.DocMemo);
        $("#zplq-CheckUsed1").html(ywData.DocMemo);
    } else {
        $("#zplq-CheckUsed").html(data.CheckUsed);
        $("#zplq-CheckUsed1").html(data.CheckUsed);
    }
    //收款人
    $("#CustomerName").html(data.CustomerName);
    $("#CustomerName1").html(data.CustomerName);

    $("#zplq-moneychinese").html((data.CheckMoney + "").moneytransferchinese());
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

var NumberToMoney = function (v) {
    v =ReverseChange(v);
    var dwMoney = ["fenMoney", "jiaoMoney", "yuanMoney", "shiMoney", "baiMoney", "qianMoney", "wanMoney", "shiwMoney", "baiwMoney", "qianwMoney", "yiwMoney"];
    var arr = v.split('');
    var obj = {};
    var len = arr.length;
    for (var i = 0; i < len; i++) {
        obj[dwMoney[i]] = arr[i];
    }
    obj[dwMoney[len]] = "";
    return obj;
}
var ReverseChange = function (v) {
    var str = "";
    for (var i = v.length - 1; i >= 0; i--) {
        str += v.charAt(i);
    }
    return str;
}

