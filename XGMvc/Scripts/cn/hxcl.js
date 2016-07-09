$.extend($.fn.linkbutton.methods, {
    //确定会计凭证
    submitPer: function () {
        
        var borrowTotal = $('#kjpz-CW_PZMain-Total_C').val();
        var loanTotal = $('#kjpz-CW_PZMain-Total_D').val();
        if (borrowTotal != loanTotal) {
            $.messager.alert('提示', '借方合计和贷方合计不相等，不能进行确定操作！');
            return;
        }
        var opts = $(this).linkbutton('options');
        var scope = opts.scope;
        var winId = '#' + opts.window;
        var rowO = [];
        //会计期间
        var CWPeriod = $('#kjpz-CW_PZMain-CWPeriod').combobox('getValue');
        //年度
        var FiscalYear = $('#kjpz-CW_PZMain-FiscalYear').combobox('getValue');
        //凭证帐套
        var AccountKey = $('#kjpz-CW_PZMain-AccountKey').combobox('getValue');

        var data = $('#kjpz-CW_PZDetail').edatagrid('getDataDetail');

        for (var i = 0, j = data.r.length; i < j; i++) {
            var curRow = data.r[i];
            var rowNew = {};
            for (var m = 0, n = curRow.length; m < n; m++) {
                var cCol = curRow[m];
                if (cCol.n == "Total_Borrow") {
                    rowNew["Total_JF"] = cCol.v;
                    rowNew["IsDC"] = 'True';
                } else if (cCol.n == "Total_Loan") {
                    rowNew["Total_DF"] = cCol.v;
                    rowNew["IsDC"] = 'False';
                } else if (cCol.n == "FeeMemo") {
                    rowNew["PZMemo"] = cCol.v;
                } else {
                    rowNew[cCol.n] = cCol.v;
                }
            }
            rowO.push(rowNew);
        }

        var indata = $.view.retrieveData(scope, "dataregion");
        debugger
        $.TransferObject.GetCerObject = { "status": 1, "m": JSON.stringify(indata.m), "d": JSON.stringify(indata.d) };
        $(winId).dialog('close');
        $('#hxcl-kjqj').val(CWPeriod);
        $('#hxcl-ztnd').val(FiscalYear);
        $('#hxcl-zaccount').val(AccountKey);
        $('#hxcl-KJPZDetail').datagrid('loadData', rowO);

    }
});


