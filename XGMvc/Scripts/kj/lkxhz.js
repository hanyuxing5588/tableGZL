/*
类款项汇总单
*/
$.extend($.view, {
    loadData: function (scope, data) {
        ;
        if (!data || data == null) {
            return;
        }
        $('#lkxhz-grid').datagrid({
            columns: createColumn(data[0].FieldList)
        });
        $('#lkxhz-grid').datagrid('loadData', data[0].objData);
    }
});

$(document).ready(function () {
    InitData();
});

//初始数据
function InitData() {
    SetDefaultValue();
    $("#lkxhz-chaxun").click(function () {
        var url = '/lkxhz/GetLoadlkxhzData/';
        GetData(url);
    });

    $("#lkxhz-tuichu").click(function () {
        WindowClose();
    });
}
function SetDefaultValue() {
    $("#lkxhz-lkxhz-Year").combobox("setValue", new Date().getFullYear());
    $("#lkxhz-lkxhz-Month").combobox("setValue", new Date().getMonth() + 1);
}
//查询获取数据
function GetData(url) {
    var condition = {};
    condition.Year = $("#lkxhz-lkxhz-Year").combobox("getValue");
    condition.Month = $("#lkxhz-lkxhz-Month").combobox("getValue");
    $('#lkxhz-grid').datagrid("loading");
    $.ajax({
        url: url,
        data: { condition: JSON.stringify(condition) },
        dataType: "json",
        type: "POST",
        error: function (xmlhttprequest, textStatus, errorThrown) {
            $.messager.alert("错误", '网络超时,请重新登录', 'error');
        },
        success: function (data) {
            $('#lkxhz-grid').datagrid("loaded");
            ;
            if (!data || data == null) {
                return;
            }
            $('#lkxhz-grid').datagrid({
                columns: createColumn(data[0].FieldList)
            });
            $('#lkxhz-grid').datagrid('loadData', data[0].objData);
        }
    });

}

//关闭
function WindowClose() {
    $($(parent.document).find("#closeTab")).click();
}
////导出数据    
//function ExportExcel() {
//    var l = $('#lkxhz-grid').datagrid('getRows');
//    if (l <= 1) {
//        $.messager.alert('系统提示', '没有要输出的数据！');
//        return;
//    }
//    var url = $.format("/lkxhz/ExportlkxhzReport?year={0}&month={1}", $("#lkxhz-lkxhz-Year").combobox("getValue"), $("#lkxhz-lkxhz-Month").combobox("getValue"))
//    window.open(url);
//}
//创建列
function createColumn(itemArry) {
    var columns = [[
    //                    { field: 'PaymentNumber', title: '财政支付令', align: 'left' },
    //                    { field: 'IsProject', title: '是否项目', width: 60, align: 'left' },
    //                    { field: 'FinanceCode', title: '政府收支分类', width: 100, align: 'left' },
    //                    { field: 'EconomyClassName', title: '经济分类', width: 100, align: 'left' },
    //                    { field: 'ExpendTypeKey', title: '支出类型', width: 100, align: 'left' },
    //                    { field: 'BGSourceName', title: '预算来源', width: 100, align: 'left' }                    
                    ]];


    var index = 0;
    for (var i = 0, j = itemArry.length; i < j; i++) {
        var valueField = itemArry[i].FieldName;
        var Valuetitle = itemArry[i].FieldTitle;
        index = index + i;
        columns[0].splice(index, 0, { field: valueField, title: Valuetitle, width: 100, align: 'center' });
    }
    return columns;
}
