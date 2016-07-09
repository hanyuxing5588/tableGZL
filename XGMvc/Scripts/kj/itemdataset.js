$.extend($.fn.linkbutton.methods, {
    //提交设置值
    submitSetValue: function () {
        $('#gzxsj-SA_PlanItemSetupModel').edatagrid('saveRow');
        $.fn.linkbutton.methods["submitAjax"].call();

    },
    submitAjax: function () {
        var indata = $.view.retrieveData("gzxsj", "dataregion");
        if (!indata) return;

        var indata1 = $.view.retrieveData("gzd", "dataregion");
        if (!indata1) return;

        $.ajax({
            url: "/gzd/SetValueData",
            data: { "status": status, "m": JSON.stringify(indata.m), "d": JSON.stringify(indata.d), "m1": JSON.stringify(indata1.m), "d1": JSON.stringify(indata1.d) },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {

                //加载页面数据
                $.view.loadData("gzd", data);
                var sTemp = "1";
                $.view.setViewEditStatus("gzd", sTemp);
                $("#b-window").dialog('close');
            }
        });
    },
    //确定 提交数据
    submitData: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitData');
        var detailGrid = $("#" + parms[0]);
        var gridopts = $.data(detailGrid[0], 'edatagrid').options;
        var defaultCol = detailGrid.edatagrid('options').defalutCol; //默认列
        var configCol = opts.configCol;

        var getRows = detailGrid.datagrid("getRows");
        var personGuid = $("#" + configCol[1]).validatebox("getText");

        var selectdetail; //detailGrid.datagrid("getSelected");
        for (var i = 0; i < getRows.length; i++) {
            if (getRows[i]["gzd-SA_PlanPersonSetModel-GUID_Person"] == personGuid) {
                selectdetail = getRows[i];
            }
        }
        var rowIndex = detailGrid.datagrid("getRowIndex", selectdetail);
        selectdetail = $.extend(selectdetail, defaultCol);

        //列表项值
        var itemValue = $("#" + configCol[0]).validatebox("getText");
        var itemField = $("#" + configCol[0]).attr("itemfield");
        selectdetail[itemField] = itemValue;

        detailGrid.datagrid('refreshRow', rowIndex);
        //关闭窗口
        var winId = "#" + opts.window;
        $(winId).dialog('close');
    }
})

$.extend($.view, {   
    submitData: function () { 
        
    }   
});

$(document).ready(function () {
    $("#gzxsj-quxiao").click(function () {
        $.messager.confirm("提示", "正在编辑，是否退出？", function (r) {
            if (r) {
                $("#b-window").dialog('close');
            }
        });

    });
    $("#gzxsj-queding").click(function () {
        $.fn.linkbutton.methods["submitSetValue"].call();
     });
});
