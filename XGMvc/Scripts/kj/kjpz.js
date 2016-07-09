$(document).ready(function () {
    var gridId = '#kjpz-CW_PZDetail';
    $(gridId).edatagrid({
        editEditEvent: function (parms) {
            var opts = $(this).edatagrid('options');
            opts.CurFieldColName = parms.field;
        },
        editEditAfterEvent: function (index, curRow) {
//            return;
//            var opts = $(this).edatagrid('options');
//            var field = opts.CurFieldColName;
//            var borrow = parseFloat(curRow['kjpz-CW_PZDetail-Total_Borrow'] + ""); ;
//            var Loan = parseFloat(curRow['kjpz-CW_PZDetail-Total_Loan'] + "");
//            borrow = isNaN(borrow) ? 0 : borrow;
//            Loan = isNaN(Loan) ? 0 : Loan;
//            curRow['kjpz-CW_PZDetail-Total_PZ'] = Math.abs(borrow - Loan);
//            curRow['kjpz-CW_PZDetail-IsDC'] = borrow > Loan;
//            $(this).datagrid('refreshRow', index);

        }
    });
});
/*
扩展datagrid
*/
$.extend($.fn.edatagrid.methods, {
    //加载格式数据
    setData: function (jq, data) {
        return jq.each(function () {
            var opts = $(this).datagrid('options');
            if (data) {
                var vals = $(this).attr('id');
                vals = vals.split('-');
                if (vals && vals.length) {
                    var r1 = $(this).edatagrid('transGridDataFromBackstage', { s: vals[0], m: vals[1], d: data.r, f: opts.defaultRow });
                    opts.hideRows = r1.hideRows;
                    //处理过的数据
                    var accessData = r1.rows;
                    for (var i = 0, j = accessData.length; i < j; i++) {
                        if (accessData[i]['kjpz-CW_PZDetail-IsDC'] == 'True') {
                            accessData[i]['kjpz-CW_PZDetail-Total_Borrow'] = accessData[i]['kjpz-CW_PZDetail-Total_PZ'];
                        } else if (accessData[i]['kjpz-CW_PZDetail-IsDC'] == 'False') {
                            accessData[i]['kjpz-CW_PZDetail-Total_Loan'] = accessData[i]['kjpz-CW_PZDetail-Total_PZ'];
                        }
                    }
                    $(this).datagrid('loadData', accessData);
                    if (data.ad) {
                        for (var i = 0; i < data.ad.length; i++) {
                            $(this).edatagrid('recordDelRow', { GUID: data.ad[i] });
                        }
                    }
                }
            }
            else {
                if (opts.isNotClearData) return;
                $(this).datagrid('loadData', []);
            }
        })
    }
});

$.extend($.fn.linkbutton.methods, {
    //获取首条，上条，下条，末条  --lb 2015-03-24 17:08
    FetchItem: function () {
        MaskUtil.mask();
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'FetchItem');
        if (!parms) return;
        var url = parms[0]; var fetchtype = parms[1];
        var indata = $.view.retrieveData(opts.scope, "dataregion");
        var cwperiod = indata.m[0].v;
        var accountkey = indata.m[13].v;
        var fiscalyear = indata.m[5].v;
        var cguid = indata.m[14].v;

        if (url) {
            $.ajax({
                url: url,
                data: { "fetchtype": fetchtype, "cwperiod": cwperiod, "accountkey": accountkey, "fiscalyear": fiscalyear, "guid": cguid },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    MaskUtil.unmask();
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {
                    //$.view.setStatus(targetScope, 4);
                    MaskUtil.unmask();
                    if (data.id != "") {

                        $.view.init(opts.scope, 4, data.id);
                    }
                    else if (data.errcode != "") {
                        $.messager.alert("消息", data.errcode, "");
                    }

                }
            });
        }
        else {
            MaskUtil.unmask();
        }
    }
});