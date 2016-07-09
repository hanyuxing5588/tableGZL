$(document).ready(function () {
    var getData = function (accountV) {
        var ry = $('#yskcx-RP_YSK-IsOrNot').attr('checked') ? 1 : 0;
        var type1 = $('#yskcx-RP_YSK-Account').combo('getValue');
        $.ajax({
            url: '/ReportDataInput/GetYskcxData',
            data: { "accountType": accountV || type1, "isYE": ry },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {

            },
            success: function (r) {
                if (r) {
                    $('#grid').datagrid('loadData', r);
                }
            }
        });
    };
    window.getData = getData;
    getData();
    $('#grid').edatagrid({
        height:$('body').height()-130,
        onAfterEdit: function (i, selectedRow, d) {
            var target = $('#grid');
            selectedRow['Balance'] = parseFloat(selectedRow['Loan'] || 0) - parseFloat(selectedRow['Repayment'] || 0);
            target.datagrid('updateRow', { index: i, row: selectedRow });
            target.datagrid('selectRow', i);
            //结束编辑以后重新计算求和
            $.gridSum();
        }
    });
    $.gridSum = function () {
        var $lastRow = $($(".datagrid-footer .datagrid-footer-inner .datagrid-ftable .datagrid-row")[1]);

        var rows = $('#grid').datagrid('getRows');
        var loan = 0, Repayment = 0, Balance = 0;
        for (var i = 0, j = rows.length; i < j; i++) {
            loan += parseFloat(rows[i]["Loan"].toString());
            Repayment += parseFloat(rows[i]["Repayment"].toString());
            Balance += parseFloat(rows[i]["Balance"].toString());
        }
        $lastRow.find("td[field='Loan']").find('div').text($.formatNumber(loan));
        $lastRow.find("td[field='Repayment']").find('div').text($.formatNumber(Repayment));
        $lastRow.find("td[field='Balance']").find('div').text($.formatNumber(Balance));

    }
    $.formatNumber = function (value) {
        return new Number(value).formatThousand(value);
    }
    $('#yskcx-addrow').bind('click', function () {
        $('#grid').edatagrid('addRow');
    });
    $('#yskcx-delrow').bind('click', function () {
        $('#grid').edatagrid('delRow');
    });
    $('#yskcx-save').bind('click', function () {
        $('#grid').edatagrid('saveRow');
        var data = $('#grid').datagrid('getRows');
        var accountType = $('#yskcx-RP_YSK-Account').combobox('getValue');
        var ry = $('#yskcx-RP_YSK-IsOrNot').attr('checked') ? 1 : 0;
        $.ajax({
            url: '/ReportDataInput/Save',
            data: { "accountType": accountType, "data": JSON.stringify(data), "isYE": ry },
            dataType: "json",
            type: "POST",
            success: function (r) {
                if (r.flag) {
                    $.messager.alert('提示', r.msg, 'info');
                    //                        $('#grid').datagrid('reload');
                } else {
                    $.messager.alert('提示', r.msg, 'info');
                    return;
                }
            }
        });

    });
    $('#yskcx-close').bind('click', function () {
        $.messager.confirm("提示", "正在编辑,是否退出?", function (data) {
            if (!data) return;
            parent.window.CloseTabs();
        });
    });
    $('#yskcx-RP_YSK-Account').combobox({
        onSelect: function (record) {
            var value = record.value;
            getData(value);
        }
    });
    $('#yskcx-RP_YSK-IsOrNot').bind('click', function () {
        getData();
    });
    $('#yskcx-ExcelImport').bind('click', function () {
        $('#b-window').dialog({
            isCancel: true,
            resizable: false,
            title: '导入',
            width: 600,
            height: 300,
            modal: true,
            draggable: true,
            resizable: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: '/ReportDataInput/excelImport'
            //            onLoad: function (c) {
            //                $.view.setViewEditStatus(parms[1], 4);
            //            }
        });
    });
});
$.extend($.fn.datagrid.defaults.editors, {
    combogrid: {
        init: function (container, options) {
            var input = $("<input type=\"text\" class=\"datagrid-editable-combogrid\" />").appendTo(container);
            input.combogrid(options);
            input.combogrid('loadRemoteDataToLocal');
            return input;
        },
        destroy: function (target) {
            $(target).combogrid('destroy');
        },
        getValue: function (target) { //回到grid
            var opts = $(target).combogrid('options');

            var val = $(target).combogrid('getValue'), text = $(target).combogrid('getText');
            var selectedRow = $('#grid').datagrid('getSelected');
            if (selectedRow) {
                selectedRow[opts.rField] = val;
                var r = $(target).combogrid('getSelected');
                if (r && opts.assField) {
                    for (var i = 0; i < opts.assField.length; i++) {
                        var field = opts.assField[i];
                        selectedRow[field] = r[field] || '';
                    }
                }
            }
            return text;
        },
        setValue: function (target, value) {//从grid的来

            var opts = $(target).combogrid('options');
            var domHideValue = target.parents().find('tr.datagrid-row-editing td[field="' + opts.rField + '"] div').text();
            $(target).combogrid('setValue', domHideValue)
            return $(target).combogrid('setText', value);
        },
        resize: function (target, width) {
            $(target).combogrid("resize", width);
        }
    }
});
$.GridOnLoadSuccess = function (data) {
    if (!!data.total) {
        var $row = $($(".datagrid-footer .datagrid-footer-inner .datagrid-ftable .datagrid-row")[1]);
        $row.find("td[field='DocNum']").remove();
        $row.find("td[field='DocMemo']").remove();
        $row.find("td[field='DocDate']").attr('colspan', '3').attr('style', 'width:380px').find('div').removeAttr('style').attr('style', 'text-align:center;;white-space:normal;height:auto;width:380px;').text("合计");
        $row.find("td[field='DepartmentName']").remove();
        $row.find("td[field='Remark']").remove();
        $row.find("td[field='Balance']").attr('style', 'background-color:#FFFFFF !important').find('div').removeAttr('style').attr('style', 'text-align:right;height:auto;background-color:#FFFFFF !important;')
        $row.find("td[field='PersonName']").attr('colspan', '3').attr('style', 'width:380px').find('div').removeAttr('style').attr('style', 'text-align:left;white-space:normal;height:auto;color:#FF0000;');
    }
}
$.extend($.fn.combogrid.defaults, {

    query: function (q) {
        var opts = $(this).combogrid('options');
        var data = opts.remoteData;
        if (data.originalRows) {
            data = data.originalRows;
        }
        if (!data || data.length == 0) {
            data = $(this).combogrid('grid').datagrid('getData').rows;
        }
        var filterField = opts.filterField
        if (!filterField) return;
        var filterField = filterField.split(',');
        var d = [], t1 = {};
        for (var m = 0, n = data.length; m < n; m++) {

            var c = function (r1) {
                var t11 = 0, w; //返回匹配项  w 看看是否匹配
                for (var i = 0; i < filterField.length; i++) {
                    var t = r1[filterField[i]].toLowerCase().indexOf(q.toLowerCase());
                    if (!w && q == r1[filterField[i]]) w = 1;
                    t11 += t > -1 ? (t + 1) : 0;
                }
                return { t: t11, w: w };
            }
            t1 = c(data[m]);
            if (t1.t > 0) {
                if (t1.w == 1) { d.unshift(data[m]); } else d.push(data[m]);
            }
        }
        var grid = $(this).combogrid('grid');
        grid.datagrid({ 'onLoadSuccess': function () {

            grid.datagrid('unselectAll');
        }
        });
        grid.datagrid('loadData', d.length == 0 ? data : d);
        //        if () {
        //            setTimeout(function () {
        //                try {
        //                  
        //                } catch (e) {

        //                }
        //            }, 2000);
        //        }
    }
});
