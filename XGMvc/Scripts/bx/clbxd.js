
window.isDeleteRow = false;
$.extend($.fn.datagrid.defaults.editors, {
    datebox: {
        init: function (_19f, _1a0) {
            var _1a1 = $("<input type=\"text\" class=\"datagrid-editable-datebox\">").appendTo(_19f);
            _1a1.datebox(_1a0);
            $.view.bind.call($(_1a1), 'datebox');
            return _1a1;
        },
        destroy: function (_1a2) {
            $(_1a2).datebox("destroy");
        },
        getValue: function (target) {
            var date = $(target).datebox("getValue");
            var dateType = $(target).datebox('options').dateType;
            if (date != "" && date.length != 0) {
                date = new Date().FormatDate(date);
            }
            target.parents().find("div.datagrid-editable").attr("oaovalue", date);
            return date.FormatDate1(dateType);
        },
        setValue: function (target, _1a5) {
            var ivalue = target.parents().find("div.datagrid-editable").attr("oaovalue");
            $(target).datebox("setValue", ivalue);
        },
        resize: function (_1a6, _1a7) {
            $(_1a6).datebox("resize", _1a7);
        }
    },
    numberspinner: {
        init: function (container, options) {

            var input = $("<input type=\"text\" class=\"datagrid-editable-numberspinner\" />").appendTo(container);
            options = $.extend(options, { min: 0, max: 23 });
            input.numberspinner(options);
            //绑定事件
            //加载数据
            return input;
        },
        destroy: function (target) {
            
            var opts = $(target).numberspinner('options');
            var selRow = $('#clbxd-BX_Travel').datagrid('getSelected');
            var fields = opts.sumFields, colName = fields[0];
            var date = selRow[colName];
            if (date) {
                var date = date.split(' ');
                selRow[colName] = $.format("{0} {1}:00:00", date[0], selRow[fields[1]] ? selRow[fields[1]] : '00');
            }
            $(target).numberspinner('destroy');
        },
        getValue: function (target) {
            return $(target).numberspinner('getValue');
        },
        setValue: function (target, value) {
            return $(target).numberspinner('setValue', value);
        },
        resize: function (_5f1, _5f2) {
            $(_5f1).numberspinner("resize", _5f2);
        }
    }
});
$.extend($.fn.datebox.methods, {
    setGridAssociate: function () {
        var options = $(this).datebox('options');
        var dateVal = $(this).combo('getValue');
        var gridId = '#clbxd-BX_Travel';
       
        if (dateVal) {
            var map = options.map;
            var view = options.view ? options.view : 'view1';
            var gdata = $.data($(gridId)[0], "datagrid");
            var dc = gdata.dc;
            var view2 = dc[view];
            var selectElement = $(view2).find("tr.datagrid-row-selected");
            if (!selectElement) return;
            for (var field in map) {
                var selRow = $(gridId).datagrid("getSelected");
                var dateType = map[field];
                var $td = $($(selectElement).find("td[field='" + field + "']"));
                var $div = $($td.find("div"));
                if ($div) {
                    if (dateType.length < 2) {
                        //给月或者日 联动赋值
                        var temp1 = dateVal.FormatDate1(dateType);
                        $div.html(temp1);
                        selRow[field] = temp1;
                        $div.attr("oaovalue", dateVal ? dateVal : "");
                    } else {
                        //组合成 开始日期和到达日期
                        selRow[field] = dateVal + $.format(" {0}:00:00", selRow[dateType] ? selRow[dateType] : '00');
                    }
                }
            }
        }
    }
});
$.extend($.fn.validatebox.defaults.rules, {
    //先定义一个当前的时间
    //分别获取当前的月和日
    //在分别获取该单据grid中开始月、日和结束月、日的值
    //将开始月的值跟当前月比较，不能小于当前月，日也是，跟当前日比较
    //在点击结束月和日的时候先将月和日跟当前时间比较，如果大于，在跟开始的月和日比较
    isAfter: {
        validator: function (value, param) {
            var DateTime = new DateTime();
            var Month = DateTime.Month();
            var Day = DateTime.Day();
            var options = $(this).datebox('options');
            var dateVal = $(this).combo('getValue');
            var StartMoth = $('#clbxd-BX_Travel-startMonth').datebox('getValue');
            var EndMoth = $('#clbxd-BX_Travel-endMonth').datebox('getValue');
            var StartDay = $('#clbxd-BX_Travel-startDay').datebox('getValue');
            var EndDay = $('#clbxd-BX_Travel-endDay').datebox('getValue');
            var gridId = '#clbxd-BX_Travel';
//            if (dateVal) {
//                return dateA < dateB;
//            }

        },
        //message: '结束时间不能小于开始时间！'
        message: '出发时间不能大于结束时间！'
    },
    isLaterToday: {
        validator: function (value, param) {
            var date = $.fn.datebox.defaults.parser(value);
            return date > new Date();
        },
        message: '出发时间不能小于当前时间！'
    }
});

$.extend($.fn.edatagrid.defaults, {
    editBefore: function (index, field) {
        if (index) {//第0行不需要判断
            var opts = $(this).edatagrid('options');
            if (!opts.allEditing) {//不能编辑的时候 直接返回
                return;
            }
            var rows = $(this).datagrid('getRows');
            var row = rows[index - 1];
            if (!row) return true;
            if (row[field]) {
                return true;
            } else {
                var strVal = '';
                var fields = ['clbxd-BX_TravelAllowance-AllowanceName', 'clbxd-BX_TravelAllowance-Persons', 'clbxd-BX_TravelAllowance-AllowanceDays', 'clbxd-BX_TravelAllowance-AllowancePrice', 'clbxd-BX_TravelAllowance-AllowenMoney'];
                for (var arr in row) {
                    if (fields.exist(arr)) continue;
                    strVal += row[arr];
                }
                //                if ($.trim(strVal)) return true;
                //                $.messager.alert('提示', '请先编辑上一行数据');
                //                return false;
            }
        }
        return true;
    },
    onDblClickCell: function (rowIndex, field, value) {

        //获取单据状态传到《差旅报销单明细》窗体中
        var winstate = $.view.getStatus('clbxd');

        var opts = $('#clbxd-BX_Travel').edatagrid('options');
        if (!opts.allEditing) {//不能编辑的时候 直接返回
            var fields = ['clbxd-BX_TravelAllowance-AllowanceName', 'clbxd-BX_TravelAllowance-Persons', 'clbxd-BX_TravelAllowance-AllowanceDays', 'clbxd-BX_TravelAllowance-AllowancePrice', 'clbxd-BX_TravelAllowance-AllowenMoney'];
            if (!fields.exist(field)) return;
        }
        var fields = ['clbxd-BX_TravelAllowance-AllowanceName', 'clbxd-BX_TravelAllowance-Persons', 'clbxd-BX_TravelAllowance-AllowanceDays', 'clbxd-BX_TravelAllowance-AllowancePrice', 'clbxd-BX_TravelAllowance-AllowenMoney'];
        if (!fields.exist(field)) return;
        $('#b-window').dialog({
            resizable: false,
            title: '差旅报销单明细',
            width: 900,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: '/clbxd/MX',
            onLoad: function (rec) {
                
                var opts = $('#clbxd-BX_Travel').datagrid('options');
                //拿到明细卡片grid的options属性 sxh 2014/03/18 11:00
                var optsmx = $('#clbxdmx-BX_TravelAllowance').edatagrid('options');
                //获取明细卡片的状态的集合 sxh 2014/03/18 11:00
                var states = optsmx.forbidstatus;
                $('#clbxdmx-BX_TravelAllowance').edatagrid({
                    noStatus: true,
                    onAfterEdit: function (index, row, changes) {
                        if ((changes+"").indexOf("AllowenMoney")<0) {
                            $(this).edatagrid('getSelectRowDomValues', { index: index, row: row });
                        }
                    }
                });

                if (opts.mxData) {
                    //                    if (winstate == "1") {
                    //                        //当明细卡片的状态为新增的时候，将明细卡片grid的数据清空 sxh 2014/03/18 11:00
                    //                        $("#clbxdmx-BX_TravelAllowance").edatagrid('loadData', { total: 0, rows: [] });

                    //                    } else {

                    //                    }
                    $('#clbxdmx-BX_TravelAllowance').edatagrid('setData', opts.mxData);
                }
                //在主页面处于浏览状态时，双击明细进入到差旅报销单明细页面，等页面数据加载完后，将改变页面状态(status:4)
                $.view.setViewEditStatus('clbxdmx', winstate);
                //根据明细卡片的状态改变grid控件的编辑状态  sxh 2014/03/18 11:00
                if (states.exist(winstate)) {
                    $('#clbxdmx-BX_TravelAllowance').edatagrid('disableEditing');
                } else {
                    $('#clbxdmx-BX_TravelAllowance').edatagrid('enableEditing');
                }

            }
        });
    }
});
$.extend($.fn.edatagrid.methods, {
    getSelectRowDomValues: function (jq, parms) {
        $(jq).edatagrid('selectRow', parms.index);
        var opts = $(jq).edatagrid('options');
        var map = opts.calculateConfig;
        var gdata = $.data($(jq).get(0), "datagrid");
        var dc = gdata.dc;
        var view2 = dc.view2;
        var selectElement = $(view2).find("tr.datagrid-row-selected");
        if (!selectElement) return;
        for (var field in map) {
            var r = $(jq).datagrid("getSelected");
            var gridfields = map[field];
            var $td = $($(selectElement).find("td[field='" + field + "']"));
            var $div = $($td.find("div"));
            if ($div) {
                var temp0 = $.toCurrency(r[gridfields[0]] || 0), temp1 = $.toCurrency(r[gridfields[1]] || 0);
                var sum = temp1 * temp0;
                if (sum != 0) {
                    sum = $.toCurrency(sum);
                    $div.html(sum);
                    r[field] = sum;
                }
            }
        }
    },
    sumField: function (jq, fields) {
        if (!fields) {
            return null;
        }
        var rows = $(jq).datagrid('getRows');
        var value = 0;
        for (var i = 0; i < rows.length; i++) {
            var dv = rows[i];
            for (var j = 0; j < fields.length; j++) {
                var vv = dv[fields[j]];
                var zs = 1; // dv['clbxd-BX_Travel-TicketCount'] || 0;
                if (vv) {
                    var c = parseFloat(vv);
                    if (j == 0) {
                        c = c * parseFloat(zs);
                    }
                    value += isNaN(c) ? 0 : c;
                }
            }

        }
        return value.toFixed(2);
    },
    cconcat: function (jq, params) {

        $.fn.edatagrid.methods['copyCurrentRowValueToAnotherRow'](jq, params);
        var value = $.fn.edatagrid.methods['concatField'](jq, 'cconcat');
        $('#clbxd-BX_Main-DocMemo').validatebox('setText', value);
    },
    concatField: function (jq, fields) {
        if (!fields) return '';
        var seperator = '、';
        var rows = $(jq).datagrid('getRows');
        /*
        从+第一行出发地点+第一行到达地点、第二行到达地点（超过两行以上的加等字，如果第二行到达地点与第一行出发地点相同则不用显示在此处）出差
        
        20150403 规则修改为 libin
        出差人 + 从+第一行出发地点+第一行到达地点、第二行到达地点（超过两行以上的加等字，如果第二行到达地点与第一行出发地点相同则不用显示在此处）出差
        20150414 规则修改 hanyx
        差旅费报销单的摘要规则改为：出差人+到达地点（屏蔽北京）+差旅费（两行以上加等地俩字）；例：张三到天津，大连等地差旅费

        */
        var chuchairen = $('#clbxd-BX_Travel-TravelPerson').validatebox('getText');
        var str = "";
        for (var i = 0; i < rows.length; i++) {
            var dv = rows[i];
            var PlaceFrom = dv["clbxd-BX_Travel-PlaceFrom"], PlaceTo = dv["clbxd-BX_Travel-PlaceTo"];
            if (!PlaceFrom || !PlaceTo) {
                continue;
            }
            if (i == 0) {
                str = "到" + PlaceTo;
            }
            if (i == 1) {
                if (rows[0]["clbxd-BX_Travel-PlaceFrom"] != PlaceTo) {
                    str += '、' + PlaceTo
                }
            }
            if (i == 2) {
                str += "等地"
            }
            if (i > 2) break;
        }
        if (str) {
            str += "差旅费";
        }
        if (str) str = chuchairen + str;
        return str;
    },
    transGridDataToBackstage: function (jq, rows) {
        var data = rows || $(jq).datagrid('getRows');
        var result = [], ritem;

        var showAndHide = $(jq).edatagrid('getColumnMapping', true);

        var fields = ['clbxd-BX_TravelAllowance-Persons', 'clbxd-BX_TravelAllowance-AllowanceName', 'clbxd-BX_TravelAllowance-AllowanceDays', 'clbxd-BX_TravelAllowance-AllowancePrice', 'clbxd-BX_TravelAllowance-AllowenMoney'];
        if (data) {
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                var AllIsNullFlag = '';
                ritem = [];
                for (var attr in item) {
                    if (fields.exist(attr)) continue;
                    var attrAttr = attr.split('-'), val = "";
                    if (attrAttr.length < 2) continue;
                    if (showAndHide[attr]) {
                        val = $(jq).edatagrid('getCellValue', { field: attr, rowindex: i });
                    }
                    var temp = val ? val : item[attr];
                    ritem.push({ n: attrAttr[2], v: temp, m: attrAttr[1] });
                    //每一行数据不是全空才进行 收集  但是 张数和 金额 列除外
                    if (attrAttr[2] != "TicketMoney" && attrAttr[2] != "TicketCount") {
                        AllIsNullFlag += temp ? temp + '' : '';
                    }
                }
                if (!$.trim(AllIsNullFlag)) break;
                result.push(ritem);
            }
        }

        return result;
    },
    setDataByCLBXD: function (jq, dataObj) {

        var items = dataObj.items, appRows = dataObj.appRows || {};
        var opts = $(jq).datagrid('options');
        if (items) {
            var vals = $(jq).attr('id');
            vals = vals.split('-');
            if (vals && vals.length) {
                var r1 = $(jq).edatagrid('transGridDataFromBackstage', { s: vals[0], m: vals[1], d: items.r, f: opts.defaultRow });
                opts.hideRows = r1.hideRows;
                var rows = r1.rows;
                var data1 = [];
                for (var j = 0; j < 20; j++) {
                    var row = rows[j];

                    if (row || appRows[j]) {
                        if (row && row['clbxd-BX_Travel-StartDate']) {
                            var date = new Date(row['clbxd-BX_Travel-StartDate']);
                            opts.hideRows[j] = $.extend(opts.hideRows[j], { 'clbxd-BX_Travel-startMonth': date.Format('yyyy-MM-dd'), 'clbxd-BX_Travel-startDay': date.Format('yyyy-MM-dd') });
                            var ch = date.Format('h');
                            if (ch == 0 || ch == '0') ch = '';
                            row = $.extend(row, { 'clbxd-BX_Travel-startMonth': date.Format('M'), 'clbxd-BX_Travel-startDay': date.Format('d'), 'clbxd-BX_Travel-startHour': ch });
                        }
                        if (row && row['clbxd-BX_Travel-ArriveDate']) {
                            var date = new Date(row['clbxd-BX_Travel-ArriveDate']);
                            opts.hideRows[j] = $.extend(opts.hideRows[j], { 'clbxd-BX_Travel-endMonth': date.Format('yyyy-MM-dd'), 'clbxd-BX_Travel-endDay': date.Format('yyyy-MM-dd') });
                            var chz = date.Format('h');
                            if (chz == 0 || chz == '0') chz = '';
                            row = $.extend(row, { 'clbxd-BX_Travel-endMonth': date.Format('M'), 'clbxd-BX_Travel-endDay': date.Format('d'), 'clbxd-BX_Travel-endHour': chz });
                        }
                        data1.push($.extend(row || {}, appRows[j] || {}));
                    } else {
                        data1.push({});
                    }
                }
                $(jq).datagrid('loadData', data1);
            }
        }
        else {
            if (opts.isNotClearData) return;
            $(jq).datagrid('loadData', []);
        }
    },
    //將當前行的值賦給另一行的目標列 sxh 2014/03/18 14:09
    copyCurrentRowValueToAnotherRow: function (jq, params) {

        if (!params) return;
        var ArriveDate = "clbxd-BX_Travel-ArriveDate", StartDate = "clbxd-BX_Travel-StartDate", startHour = "clbxd-BX_Travel-startHour", endHour = "clbxd-BX_Travel-endHour";
        var tempArriveDate = params.row[ArriveDate];
        var tempStartDate = params.row[StartDate];
        var tempendHour = params.row[endHour];
        var tempstartHour = params.row[startHour];
        var dateA, dateB;
        dateA = new Date(new Date(tempArriveDate).Format("yyyy-MM-dd"));
        dateB = new Date(new Date(tempStartDate).Format("yyyy-MM-dd"));
        if (!isNaN(tempendHour) || !isNaN(tempstartHour)) {
            if (dateB.toString() == dateA.toString()) {
                if (true) {
                    if (parseInt(tempstartHour) >= parseInt(tempendHour)) {
                        $.messager.alert('提示', '同一天内，出发地和到达地的小时数不能相同！');

                        $(jq).edatagrid('updateRow', {
                            index: params.index,
                            row: {
                                "clbxd-BX_Travel-endHour": ""
                            }
                        });
                    }
                }
            }
        }
        if (dateB > dateA) {
            $.messager.alert('提示', '出发时间不能大于到达时间！');
            $(jq).edatagrid('updateRow', {
                index: params.index,
                row: {
                    "clbxd-BX_Travel-ArriveDate": "",
                    "clbxd-BX_Travel-endDay": "",
                    "clbxd-BX_Travel-endHour": "",
                    "clbxd-BX_Travel-endMonth": ""
                }
            });
        }

        var PlaceTo = "clbxd-BX_Travel-PlaceTo";
        //        var currRowVal = params.row[PlaceTo];
        var currRowIndex = params.index;
        var rowLength = $(jq).edatagrid('getRows').length;
        if (currRowIndex == 0) return;
        var preRowVal = $(jq).edatagrid('getCellText', { field: PlaceTo, rowindex: currRowIndex - 1 });
        //        if (preRowVal == "" || preRowVal.length == 0) {
        //            $.messager.alert('提示', '请从上一行开始填写！');
        //            return;
        //        }

        if (currRowIndex > rowLength - 1) return;
        if (tempStartDate) {

            if (params.index + 1 <= rowLength) {
                $(jq).edatagrid('updateRow', {
                    index: params.index,
                    row: {
                        "clbxd-BX_Travel-PlaceFrom": preRowVal
                    }
                });
            }
        }
    },
    //獲取當前編輯單元格上一行對應單元格的數據 sxh 2014/03/19 11:37
    getCellText: function (jq, pars) {
        if (isDeleteRow) {
            window.isDeleteRow = false;
            return "";
        }
        if (pars && pars.rowindex >= 0 && pars.field) {
            var opts = $(jq).edatagrid('options');
            return opts.finder.getTr($(jq)[0], pars.rowindex) ? opts.finder.getTr($(jq)[0], pars.rowindex).find("td[field='" + pars.field + "']").find("div").text() : "";
        }
        return "";
    }
})
$.extend($.view, {
    retrieveData: function (scope, region, isText) {

        if (!scope) return;
        var result = { m: [], d: [] }, eauiType, gdata = null, method, selectortag;
        region ? selectortag = '#' + scope + "-" + region + " [id^='" + scope + "-']" : selectortag = "[id^='" + scope + "-']";
        $(selectortag).each(function () {
            eauiType = $(this).GetEasyUIType();
            var types = ["datebox", "combogrid", "combobox", "validatebox", "numberbox", "checkbox", 'edatagrid'];
            if (!types.exist(eauiType)) return;
            gdata = $(this)[eauiType]('getData', isText);
            if (gdata) {

                switch (eauiType) {
                    case "datagrid":
                    case "edatagrid":
                        result.d.push(gdata);
                        var opts = $(this)[eauiType]('options');
                        //需要将横表转换为纵表
                        
                        var tData = $.ChageDataObj.ZTableToHTable(opts.mxData);
                        result.d.push(tData);
                        break;
                    default:
                        if (gdata.length) {
                            for (var i = 0, j = gdata.length; i < j; i++) {
                                result.m.push(gdata[i]);
                            }
                        } else {
                            result.m.push(gdata);
                        }
                        break;

                }
            }

        });
        //由于控件在grid上 所以解析成页面的时候 就变成了2个控件  特性是 用id去找 找到是'真'得FeeMemo控件
        result.m.push({ m: 'BX_Detail', n: 'FeeMemo', v: $('#clbxd-BX_Detail-FeeMemo').combo('getValue') });
        return result;
    },
    setDataForGrid: function (d, scope, funName) {
       
        if (!d || funName == 'setDefaultRow') return;
        if (d.length == 0) {
            $.ChageDataObj.ChageDataAdd();
            return;
        }
        var dataObj = {};
        
        for (var i = 0; i < d.length; i++) {
            var ditem = d[i];
            if (ditem) {
                if (ditem.m == 'BX_TravelAllowance') {
                    var o = $.ChageDataObj;
                    var opts = $('#clbxd-BX_Travel').datagrid('options');
                    
                    dataObj.appRows = o.ChangeGridToTotalData1(ditem, true);
                    opts.mxData = o.HTableToZTable(ditem) ;
                   
                } else {
                    dataObj.items = ditem;
                }
            }
        }
        $('#clbxd-BX_Travel').edatagrid('setDataByCLBXD', dataObj);
    },
    formatters: {
        boolbox: function (value1, row, index) {
            if (value1) {
                var value = $.trim(value1 + "").toLowerCase();
                if (value == "true" || value == "1" || value == "是")
                    return "是";
            }
            return "否";
        },
        datebox: function (date, row, index) {

            if (date && date.length != 0) {
                var date = new Date().FormatDate(date);
            } else {
                date = '';
            }
            return date;
        },
        numberbox: function (value1, row, index) {

            if (!value1) return '';
            if ($.isNumeric(value1) && value1 == 0) {
                value1 = '';
            }
            return $.isNumeric(value1) ? new Number(value1).formatThousand(value1) : ''
        }

    }
});
$.extend($.fn.linkbutton.methods, {
    submitDetail1: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitDetail1');
        if (parms && parms.length >= 1) {
            var targetscope = parms[0];
            var winId = '#' + opts.window;
            //当前页面的数据
            var winOpts = $(winId).dialog('options');
            var data = $.view.retrieveData(opts.scope, '', true);
            $.view.loadData(winOpts.scope, data, true, true);
            $.view.setViewEditStatus(winOpts.scope, winOpts.curState, true);
            $(winId).dialog('close');
            $(winId).trigger('mxSubmit');
        }
    },
    //    print: function () {
    //        //重新获取数据，
    //        var scope = $(this).linkbutton('options').scope;
    //        var parms = $(this).linkbutton('getParms', 'print');
    //        if (!parms || !parms.length) return;
    //        var guid = $.view.getKeyGuid(scope);
    //        var data = $.view.retrieveDoc(guid, scope);
    //        if (!data || !data.m) return;
    //        var ids = parms[1];
    //        if (ids && ids.length) {
    //            for (var i = 0; i < ids.length; i++) {
    //                var id = ids[i].split('-');
    //                var text = $('#' + ids[i]).val();
    //                data.m.push({ m: id[1], n: id[2], v: text });
    //            }
    //        }
    //        //加报销人
    //        data.m.push({ m: 'BX_Travel', n: 'PersonName', v: $('#clbxd-BX_Travel-TravelPerson').val() })
    //        if (!data.d || !data.d[0].r) return;
    //        var rowsTemp = [];
    //        var rows = $('#clbxd-BX_Travel').datagrid('getRows');
    //        for (var i = 0; i < rows.length; i++) {
    //            var row = rows[i];
    //            var falg = "";
    //            for (var attr in row) {
    //                falg += row[attr];
    //            }
    //            if ($.trim(falg) == "")
    //                break;
    //            rowsTemp.push(row);
    //        }
    //        data.d[0].r = rowsTemp;
    //        data.d[1] = undefined;
    //        //动态匹配打印模板
    //        var tempScope = parms[0].split("/")[2]; //获取单据类型
    //        var url = parms[0] + "?pturl=" + tempScope + "print";
    //        $("#printIframe").jqprintEx(url, data, true);
    //    },
    printView: function () {

        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'print');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        
        var data = $.view.retrieveDoc(guid, scope);
        if (!data || !data.m) return;
        var ids = parms[1];
        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i].split('-');
                var text = $('#' + ids[i]).val();
                data.m.push({ m: id[1], n: id[2], v: text });
            }
        }
        //加报销人
        data.m.push({ m: 'BX_Travel', n: 'PersonName', v: $('#clbxd-BX_Travel-TravelPerson').val() })
        if (!data.d || !data.d[0].r) return;
        var rowsTemp = [];
        var rows = $('#clbxd-BX_Travel').datagrid('getRows');
        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            var falg = "";
            for (var attr in row) {
                falg += row[attr];
            }
            if ($.trim(falg) == "")
                break;
            row["clbxd-BX_TravelAllowance-AllowancePrice"] = "";
            rowsTemp.push(row);
        }
        data.d[0].r = rowsTemp;
        data.d[1] = undefined;
        //动态匹配打印模板
        
        var tempScope = parms[0].split("/")[2]; //获取单据类型
        var url = parms[0];
        var argData = JSON.stringify(data);
        $.ajax({
            url: '/Print/clbxdData',
            data: { data: argData },
            dataType: "text",
            type: "POST",
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {
                window.open(url);
            }
        })
        //window.open(url);

    },
    submitDetail: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitDetail');
        if (parms && parms.length >= 1) {
            var result = { m: [], d: [] }
            //结束当前页面编辑状态
            var cgrid = $("#clbxdmx-BX_TravelAllowance");
            var rowSelect = cgrid.datagrid('getSelected');
            if (rowSelect) {
                rowIndex = cgrid.datagrid('getRowIndex', rowSelect);

                cgrid.datagrid('endEdit', rowIndex);
            }

            //当前页面的数据

            
            var dataCur = $('#clbxdmx-BX_TravelAllowance').edatagrid('getDataDetail');
            var opts = $('#clbxd-BX_Travel').datagrid('options');
            opts.mxData = dataCur;
            $.ChageDataObj.ChangeGridToTotalData(dataCur);
            
            var rows = $("#clbxd-BX_Travel").datagrid('getRows');
            var value = 0;
            for (var i = 0; i < rows.length; i++) {
                var dv = rows[i];
                var vv = dv["clbxd-BX_Travel-TicketMoney"];
                var zs = 1; //dv['clbxd-BX_Travel-TicketCount'] || 0;
                if (vv) {
                    var c = parseFloat(vv); c = c * parseFloat(zs);
                    value += isNaN(c) ? 0 : c;
                }
                var c = parseFloat(dv['clbxd-BX_TravelAllowance-AllowenMoney']);
                value += isNaN(c) ? 0 : c;
            }
            if (value) {
                value = value.toFixed(2);
                var val = new Number(value).formatThousand(value);
                $('#clbxd-BX_Main-moneychinese').validatebox('setText', val);
                $('#clbxd-BX_Main-moneyunmber').validatebox('setText', val);
            }
            $('#b-window').dialog('close');
        }

        //自动生成差旅费报销单摘要
        //20150403 规则修改为 libin
        //出差人 + 从+第一行出发地点+第一行到达地点、第二行到达地点（超过两行以上的加等字，如果第二行到达地点与第一行出发地点相同则不用显示在此处）出差
        var chuchairen = $('#clbxd-BX_Travel-TravelPerson').validatebox('getText');
        var cmemo = $('#clbxd-BX_Main-DocMemo').validatebox('getText');

        var preindex = cmemo.indexOf("到");

        if (cmemo) cmemo = chuchairen + cmemo.substr(preindex, cmemo.length - preindex);
        $('#clbxd-BX_Main-DocMemo').validatebox('setText', cmemo);

    },
    delRowClear: function (jq) {
        isDeleteRow = true;
        //删除前判断是否确定，否就直接return
        var parms = $(this).linkbutton('getParms', 'delRowClear');
        if (!parms) return;
        var gridId = '#' + parms[0];
        var dg = $(gridId);
        var opts = dg.datagrid('options');
        var fields = ['clbxd-BX_TravelAllowance-Persons', 'clbxd-BX_TravelAllowance-AllowanceName', 'clbxd-BX_TravelAllowance-AllowanceDays', 'clbxd-BX_TravelAllowance-AllowancePrice', 'clbxd-BX_TravelAllowance-AllowenMoney'];
        var rows = dg.datagrid('getRows');

        var opts = $('#clbxd-BX_Travel').datagrid('options');
        
        //拿到grid列表明細選項
        var mxData = opts.mxData;
        //        if (!mxData.r) return;
        //获取选中行
        var selrow = dg.datagrid('getSelected');

        //根据选中行得到索引值
        var index = dg.datagrid('getRowIndex', selrow);
        if (selrow == undefined || selrow == null) {
            $.messager.alert('提示', '请选中要删除的记录行！', 'info');
            return;
        } else {

            //先將選中行刪除
            dg.datagrid('deleteRow', index);
            //在最末尾插一行
            //獲取到最末行索引值
            var lastIndex = dg.datagrid('getRows').length - 1;
            dg.datagrid('insertRow', {
                index: lastIndex,
                row: {}
            });
            if (!mxData || !mxData.r) {
                mxData = $('#clbxdmx-BX_TravelAllowance').edatagrid('getDataDetail');
            } else {
                //將從明細中取到的值付給右側的grid列表
                $.ChageDataObj.ChangeGridToTotalData(mxData);
            }
            //                    var temp = [];
            //                    var obj = { name: "", value: "" };
            //                    for (var cell in selrow) {
            //                        if (fields.exist(cell)) {
            //                            obj.name = cell;
            //                            obj.value = selrow[cell];
            //                            temp.push(obj);
            //                        };
            //                    }
            //                    var rowDelIndex = 19, ritem = {}, row;
            //                    for (var i = rows.length - 1; i > -1; i--) {
            //                        var AllIsNullFlag = '', item = rows[i];
            //                        for (var attr in item) {
            //                            if (fields.exist(attr)) continue;
            //                            AllIsNullFlag += item[attr] ? item[attr] + '' : '';
            //                            ritem[attr] = '';
            //                        }
            //                        if ($.trim(AllIsNullFlag)) {
            //                            row = item;
            //                            rowDelIndex = i;
            //                            break;
            //                        }
            //                    }
            //                    /*sxh 2014/03/19 14:46*/
            //                    for (var i = index; i < 20; i++) {
            //                        dg.datagrid('updateRow', { index: i, row: rows[i + 1] });
            //                    }
            ////                    dg.datagrid('updateRow', { index: rowDelIndex, row: $.extend(row, ritem) });
            //                    var dom = opts.finder.getTr(dg.get(0), rowDelIndex);
            //                    for (var attr in ritem) {
            //                        if (!dom) return;
            //                        var cellDom = dom.find("td[field='" + attr + "']").find("div");
            //                        if (!cellDom) continue;
            //                        cellDom.attr("oaovalue", "");
            //                    }
            //执行公式
            opts.onAfterDestroy.call(dg[0], index, selrow);
        }
    }
});


//定义数据转换的特殊对象
//改变数据 对从库里面或者历史里面的查询的数据进行处理


$.ChageDataObj = {
    xmDatas: [],
    xmDataGuids: [],
    xmToCol: {},

    //数据
    //新增的时候
    ChageDataAdd: function () {
        
        this.Init();
        var xmData = this.xmDatas;
        var data = [];
        for (var j = 0; j < 20; j++) {
            var temp = {};
            if (xmData[j]) {
                temp['clbxd-BX_TravelAllowance-AllowanceName'] = xmData[j];
                temp['clbxd-BX_TravelAllowance-Persons'] = '';
                temp['clbxd-BX_Travel-StartDate'] = '',
                temp['clbxd-BX_Travel-ArriveDate'] = '',
                temp['clbxd-BX_Travel-PlaceFrom'] = '',
                temp['clbxd-BX_Travel-PlaceTo'] = ''
            }
            data.push(temp);
        }
        var $grid = $('#clbxd-BX_Travel');
        var opts = $grid.edatagrid('options');
        opts.mxData = [];
        $grid.edatagrid('loadData', data);
    },
    //从纵表转换为横表
    HTableToZTable: function (dataCur) {
        this.Init();
        var backstageData = dataCur.r;
        var dic2person = {};   //以人为标示的行及其隐藏行的字典 eg: '人的ＩＤ':{row:row,hideRow:hideRow}
        var xmToCol = this.xmToCol;
        for (var i = 0; i < backstageData.length; i++) {
            var item = backstageData[i];
            var persionId = '', index, arr = []; //项目的Id和人得Id
            for (var j = 0; j < item.length; j++) {
                var col = item[j];
                if (col.n == 'GUID_Allowance') {
                    index = xmToCol[col.v];
                }
                if (col.n == 'GUID_Person') {
                    persionId = col.v;
                    if (dic2person[persionId]) {
                        arr = dic2person[persionId];
                    }
                }
                if (col.n == 'AllowancePrice' || col.n == 'AllowanceDays' || col.n == 'AllowenMoney') {
                    col.n = col.n + index;
                    if (arr.length > 0) {
                        arr.push(col);
                    }
                }
            }
            if (!dic2person[persionId]) {
                dic2person[persionId] = item;
            }
        }
        var rows = [];
        for (var i in dic2person) {
            rows.push(dic2person[i]);
        }
        return { m: dataCur.m, r: rows };
    },
    //从横表转换为纵表
    ZTableToHTable: function (dataCur) {
        
        this.Init();
        if (!dataCur || !dataCur.r) {
            this.ChageDataAdd();
            return;
        }
        var xmToCol = this.xmDataGuids;
        var data = dataCur.r;
        var dataResult = { "m": dataCur.m, "r": [] }
        var step = 3;
        for (var i = 0; i < data.length; i++) {

            var curRow = data[i];
            var rows = [], rowsTemp = [];
            var index = 0;
            var dicRepat = {};
            var upIndex = 1;
            for (var j = 0; j < curRow.length; j++) {
                var attr = curRow[j];
                index = attr.n.substr(attr.n.length - 1, 1); //每次拿最后一个字母
                if (!$.isNumeric(index)) { //如果不是整数 说明数据时后台返回的数据 或者是历史中有的数据
                    index = 0;
                    if (attr.n != "GUID_Allowance" && dicRepat[attr.n]!=1) {
                        rows.push(attr);
                        dicRepat[attr.n]=1;
                    }
                }
                else {
                    if (upIndex != index) {
                        rowsTemp.push({ m: attr.m, n: 'GUID_Allowance', v: xmToCol[upIndex - 1] });
                        dataResult.r.push(rows.concat(rowsTemp));
                        rowsTemp = [];
                        upIndex = index;
                    }
                    rowsTemp.push({ m: attr.m, n: attr.n.substr(0, attr.n.length - 1), v: attr.v });
                    if (j == curRow.length - 1) {//处理最后一泼人
                        rowsTemp.push({ m: attr.m, n: 'GUID_Allowance', v: xmToCol[upIndex - 1] });
                        dataResult.r.push(rows.concat(rowsTemp));
                        rowsTemp = [];
                        upIndex = index;
                    }
                }
            }

        }
        return dataResult;
    },
    //将列表获得的模型数据 装换成合计数据
    ChangeGridToTotalData: function (dataCur, isReturnData) {
        
        this.Init();
        var xmToCol = this.xmToCol;
        var xmData = this.xmDatas;
        var xmSumArr = []; //以项目标示的行 计算总数
        for (var m = 0; m < 4; m++) {
            var ttemp = { 'clbxd-BX_TravelAllowance-Persons': 0, 'clbxd-BX_TravelAllowance-AllowanceDays': 0, 'clbxd-BX_TravelAllowance-AllowancePrice': 0, 'clbxd-BX_TravelAllowance-AllowenMoney': 0 }
            ttemp['clbxd-BX_TravelAllowance-AllowanceName'] = xmData[m];
            xmSumArr.push(ttemp);
        }
        var persons = [];
        var data = dataCur.r;
        var cbt = 'clbxd-BX_TravelAllowance-';
        if (!data) return;
        for (var i = 0; i < data.length; i++) {
            var row = data[i];
            for (var j = 0; j < row.length; j++) {
                var col = row[j];
                var colName = col.n;
                if (colName == 'InvitePersonName') {
                    persons.push(col.v);
                }
                var index = colName.substr(colName.length - 1, 1);
                if (!isNaN(index)) {
                    var iIndex = new Number(index) - 1;
                    var xm = xmSumArr[iIndex];
                    var nName = colName.substr(0, colName.length - 1);
                    xm[cbt + nName] += new Number(col.v);
                    if (nName == 'AllowenMoney') {//有钱就算人头
                        xm['clbxd-BX_TravelAllowance-Persons']++;
                    }

                }
            }
        }
        if (isReturnData) return xmSumArr;
        //赋值 给grid
        var data = $('#clbxd-BX_Travel').datagrid('getRows');
        for (var i = 0; i < data.length; i++) {
            var row = data[i];
            if (xmSumArr[i]) {
                var row1 = $.extend(row, xmSumArr[i]);

                if (row1 != null && row1 != undefined) row1["clbxd-BX_TravelAllowance-AllowancePrice"] = "";
                $('#clbxd-BX_Travel').datagrid('updateRow', { index: i, row: row1 });
            } else {
                break;
            }
        }
        //赋值 给人控件
        $('#clbxd-BX_Travel-TravelPerson').val(persons.join(','));
        //        return xmSumArr;
    },
    //初次加载数据时候转换为和的数据 即保存成功后
    ChangeGridToTotalData1: function (dataCur) {
        
        var dic2xm = {}, xmSumArr = [];
        this.Init();
        var xmToCol = this.xmToCol;
        var xmData = this.xmDatas;

        for (var m = 0; m < 4; m++) {
            var ttemp = { 'clbxd-BX_TravelAllowance-Persons': 0, 'clbxd-BX_TravelAllowance-AllowanceDays': 0, 'clbxd-BX_TravelAllowance-AllowancePrice': 0, 'clbxd-BX_TravelAllowance-AllowenMoney': 0 }
            ttemp['clbxd-BX_TravelAllowance-AllowanceName'] = xmData[m];
            xmSumArr.push(ttemp);
        }
        var persons = [];
        var data = dataCur.r;
        var cbt = 'clbxd-BX_TravelAllowance-';
        if (!data) return;
        var sumCols = ['AllowanceDays', 'AllowancePrice', 'AllowenMoney'];
        debugger
        for (var i = 0; i < data.length; i++) {
            var row = data[i];
            var index, xm;
            for (var j = 0; j < row.length; j++) {
                var col = row[j];
                var colName = col.n;
                if (colName == 'InvitePersonName') {
                    if (!persons.exist(col.v)) {
                        persons.push(col.v);
                    }
                }
                if (colName == 'GUID_Allowance') {

                    index = xmToCol[col.v];
                    xm = xmSumArr[index - 1];
                }
                if (sumCols.exist(colName)) {

                    xm[cbt + colName] += new Number(col.v);
                }
            }
            xm['clbxd-BX_TravelAllowance-Persons']++;
        }
        //赋值 给人控件
        $('#clbxd-BX_Travel-TravelPerson').val(persons.join(','));
        return xmSumArr;
    },
    Init: function () {
        //    $.ajax({
        //        url: '/Combogrid/GetTravelAllowance/',
        //        dataType: 'json',
        //        type: 'post',
        //        success: function (data) {
        //            var changeDataObj
        //            var changeDataObj = $.ChageDataObj;
        //            for (var i = 0, j = data.length; i < j; i++) {
        //                var row = data[i];
        //                changeDataObj.xmDatas.push(row.AllowanceName);
        //                changeDataObj.xmDataGuids.push(row.GUID);
        //                changeDataObj.xmToCol[row.GUID] = i + 1;
        //                $.ChageDataObj.ChageDataAdd();
        //            }
        //        }
        //    })
        var data = [
            { "GUID": "f3468c4f-f8bf-4da2-8391-e32ea8eb0482", "AllowanceKey": "001", "AllowanceName": "市内车费" },
            { "GUID": "0c971834-3773-4eb3-96e0-89af8940774a", "AllowanceKey": "002", "AllowanceName": "住宿费" },
            { "GUID": "c824667a-f74a-4db0-9447-5d43772d829b", "AllowanceKey": "003", "AllowanceName": "伙食补助" },
            { "GUID": "cc6c0a61-b9c9-4abe-815d-52dc62244b93", "AllowanceKey": "004", "AllowanceName": "其他"}]
        if (this.xmDatas == undefined || this.xmDatas.length == 0) {
            for (var i = 0, j = data.length; i < j; i++) {
                var row = data[i];

                this.xmDatas.push(row.AllowanceName);
                this.xmDataGuids.push(row.GUID);
                this.xmToCol[row.GUID] = i + 1;
                $.ChageDataObj.ChageDataAdd();
            }
        }
    }
};
