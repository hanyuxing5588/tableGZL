
var tableprorotypes;
$Print = {
    //说下 
    //打印
    jqprint: function (data, printer, isChange) {
        tableprorotypes = $Print.gettableprototype(data, printer); //获取表格源信息
        //分页
        if (data && data.d && data.d.length > 0) {
            var dataRowCount = data.d[0].r.length;
            var pageRowCount;
            for (var item in tableprorotypes) {
                pageRowCount = tableprorotypes[item].rowcount;
                break;
            }
            $Print.SetPage(dataRowCount, pageRowCount, printer);
        }
        printer = $("div#print"); //打印句柄
        //填充固定单元格内容
        $Print.writefix(data.m, printer);
        if (data.d) {
            var tabdatas = data.d;
            for (var i = 0; i < tabdatas.length; i++) {
                if (tabdatas[i] && tabdatas[i].r) {
                    $Print.writefix(tabdatas[i].r[0], printer);
                }
            }
        };
        //填充表格
        if (data.d) {
            var tabdatas = data.d;
            for (var i = 0; i < tabdatas.length; i++) {
                //填充序号
                if (!tabdatas[i] || !tabdatas[i].m) continue;
                $Print.writetableserial(tabdatas[i].m, printer);

                $Print.writetable(tabdatas[i], printer, isChange);
            }
        };
    },
    //设置分页
    SetPage: function (dataRowCount, pageRowCount, printer) {
        debugger
        var pageCount = $Print.fullCount(dataRowCount, pageRowCount);
        var addPage = pageCount;
        if (addPage > 0) {
            var element = $(printer);
            if (!element) return;
            var html = $('#btn_print').parent().html();
            $('#btn_print').parent().remove();
            for (var i = 0; i < addPage - 1; i++) {
                $($(element).parent()).append($(element).clone());
            }
            var c = '  <div class="noprint" id="printButtonArr" style="float: left; height:10%;  margin-top:100px; margin-left:-550px">      ';
            c = c + html + "</div>";
            $($(element).parent()).append(c);
        }
    },
    //整除
    fullCount: function (exp1, exp2) {
        var n1 = Math.round(exp1);
        var n2 = Math.round(exp2);
        var result = n1 / n2;
        if (n1 > n2 && result >= 1) {
            result = Math.floor(result); //返回值为小于等于其数值参数的最大数值
        }
        else {
            result = 0;
        }
        return result;
    },
    //填充固定单元格内容
    writefix: function (data, dom) {

        if (data) {
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                var id = "#" + item.m + "-" + item.n;
                //获取元素
                $(dom.find(id)).each(function () {
                    $(this).html(item.v);
                });
            }
        }
    },
    //填充列表内容
    writetable: function (data, dom, isChange) {

        if (data && data.m && data.r) {
            var tabprototype = tableprorotypes[data.m];
            if (!tabprototype) return;
            var columns = tabprototype.columns;
            if (!columns) return;
            var id = "#" + data.m + " tbody tr";
            rows = dom.find(id);
            if (!rows || !rows.length) return;
            var datarows = data.r;
            var selcolfield = tabprototype.serialcolumn && tabprototype.serialcolumn.field ? tabprototype.serialcolumn.field : undefined;
            for (var i = 0, j = datarows.length; i < j; i++) {
                var drow = isChange ? datarows[i] : $Print.transdata(datarows[i]);
                if (!drow) continue;
                var row = rows[i];
                if (row) {
                    var cols = $(row).find("td");

                    if (!cols || !cols.length) continue;

                    for (var m = 0, n = columns.length; m < n; m++) {
                        var column = columns[m];
                        if (column.field != selcolfield) {
                            if (!column.field) continue;
                            var reVal = drow[column.field];
                            if (column.isDecimal == "true") {
                                reVal = new Number(reVal).formatThousand(reVal);

                            }

                            if (column.defalutValue) {
                                reVal = reVal || column.defalutValue;
                            }
                            $(cols[column.index]).html(reVal);
                        }
                    }
                }
            }
        }

    },
    //获取表格的源信息
    gettableprototype: function (data, dom) {

        var tables = tables || {};
        dom.find("table").each(function () {
            var id = $(this).attr("id");
            if (id) {
                var columns = [];
                var i = 0;
                var serialcolumn = null;
                //创建列信息
                var colConfig = $(this).attr('colConfig');
                if (colConfig) {
                    columns = eval(colConfig);
                    serialcolumn = columns[0];
                } else {
                    $($(this).find("thead")).find("tr th").each(function () {
                        var field = $(this).attr("field");
                        /*是金额的时候将金额行记录，为后面数据转换做标记*/
                        var isDecimal = $(this).attr("isDemical");
                        var defalutValue = $(this).attr("defalutValue");
                        if (field) {
                            var column = {};
                            column.field = field;
                            column.index = i;
                            column.isDecimal = isDecimal;
                            column.defalutValue = defalutValue;
                            columns.push(column);
                            if (column.field == "colindex") {
                                serialcolumn = column;
                            }
                            i++;
                        }
                    });
                }

                /*这个模板上的Row必须是动态创建的，根据前台datagrid返回来的行数在去动态生成多少条tr*/
                var rows = $(this).find("tbody tr");    /*获取当前表格的tr(Rows)*/

                var rowcount = rows.length ? rows.length : 0;
                var info = {
                    serialcolumn: serialcolumn, //序号列
                    columns: columns,
                    rowcount: rowcount,
                    $lastrow: rowcount > 0 ? rows[rowcount - 1] : null
                };
                tables[id] = info;
            }
        });
        return tables;
    },
    /*转换数据格式*/
    transdata: function (data) {
        if (data) {
            var result = {};
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                var id = item.m + "-" + item.n;
                result[id] = item.v;
            }
            return result;
        }
    },
    /*编写表格序号*/
    writetableserial: function (tablemodel, dom) {

        if (tableprorotypes) {
            var tabprototype = tableprorotypes[tablemodel];
            if (tabprototype && tabprototype.serialcolumn) {
                var id = "#" + tablemodel + " tbody tr";
                var rows = dom.find(id);
                if (rows && rows.length) {
                    for (var i = 0, j = rows.length; i < j; i++) {
                        var row = rows[i];
                        var cols = $(row).find("td");
                        var selcol = cols[tabprototype.serialcolumn.index];
                        $(selcol).html(i + 1);
                    }
                }
            }
        }
    }

};
$Common = {
    ajaxData: function (url, argData) {
        var data;
        $.ajax({
            url: url,
            async: false, /*同步*/
            data: argData,
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
    },
    SetSpanValue: function (modelName, data) {
        for (var item in data) {
            $("#" + modelName + "-" + item).html(data[item]);
        }

    },
    SetInputValue: function (modelName, data) {
        for (var item in data) {
            $("#" + modelName + "-" + item).val(data[item]);
        }
    },
    SetSpanNumberToMoney: function (modelName, checkMoney) {
        checkMoney=checkMoney.replace(",", "").replace(".", "").replace(" ", "");
        var obj = $Common.NumberToMoney(checkMoney);
        for (var i in obj) {
            $("#" + modelName + "-" + i).html(obj[i]);
        }
    },
    SetInputNumberToMoney: function (modelName, checkMoney) {
        checkMoney = checkMoney.replace(",", "").replace(".", "").replace(" ", "").replace(",", "").replace(",", "");
        var obj = $Common.NumberToMoney(checkMoney);
        for (var i in obj) {
            $("#" + modelName + "-" + i).val(obj[i]);
        }
    },
    NumberToMoney: function (v) {
        v = $Common.ReverseChange(v);
        var dwMoney = ["fenMoney", "jiaoMoney", "yuanMoney", "shiMoney", "baiMoney", "qianMoney", "wanMoney", "shiwMoney", "baiwMoney", "qianwMoney", "yiwMoney"];
        var arr = v.split('');
        var obj = {};
        var len = arr.length;
        for (var i = 0; i < len; i++) {
            obj[dwMoney[i]] = arr[i];
        }
        obj[dwMoney[len]] = "￥";
        return obj;
    },
    ReverseChange: function (v) {
        v = v + "";
        var str = "";
        for (var i = v.length - 1; i >= 0; i--) {
            str += v.charAt(i);
        }
        return str;
    }
}
