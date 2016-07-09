$.datagridF = {};
var swfu;
var gridId = "#gzd-SA_PlanActionDetail";
$.extend($.view, {
 retrieveDoc: function (guid, scope) {

        var data;
        if (guid && scope) {
            var url = '/' + scope + '/Retrieve1';
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
        }
        return data;
    }
});


$.extend($.fn.edatagrid.methods, {
    //前台页面列表值转换成后台值

    transGridDataToBackstage: function (jq, rows) {
        var data = rows || $(jq).datagrid('getRows');
        var defaultCol = $(jq).edatagrid('options').defalutCol; //默认列    
        //        var defaultColData = {}; //缓冲默认列 待处理 
        //        for (var item in defaultCol) { 
        //            defaultColData[item]=defaultCol[item];
        //        }
        var result = [], ritem;
        var showAndHide = $(jq).edatagrid('getColumnMapping', true);

        if (data) {
            for (var i = 0, j = data.length - 1; i < j; i++) {
                var item = data[i];

                //每次都清空defaultCol列对应的值

                //defaultCol = defaultColData;
                for (var colitem in defaultCol) {
                    if (colitem.toLowerCase().indexOf('itemname') >= 0) {
                        defaultCol[colitem] = "";
                    }
                    if (colitem.toLowerCase().indexOf('gzlkxsz') >= 0) {
                        defaultCol[colitem] = "";
                    }
                }

                //默认列处理只有一个编辑的列

                item = $.extend(defaultCol, item);

                ritem = [];
                for (var attr in item) {
                    var attrAttr = attr.split('-'), val = "";
                    if (attrAttr.length < 2) continue;
                    if (showAndHide[attr]) {
                        val = $(jq).edatagrid('getCellValue', { field: attr, rowindex: i });
                    }
                    ritem.push({ n: attrAttr[2], v: val ? val : item[attr], m: attrAttr[1] });
                }
                result.push(ritem);
            }
        }
        return result;
    },
    //删除 最后一条数据不能删除

    delGZDRow: function (jq) {
        return jq.each(function () {

            var dg = $(this);
            var opts = $.data(this, 'edatagrid').options;
            var row = dg.datagrid('getSelected');
            if (!row) {
                return;
            }
            var index = dg.datagrid('getRowIndex', row);
            var gridData = dg.datagrid("getRows");
            if (index == gridData.length - 1) {
                $.messager.alert("提示...", "合计不能删除！", "info");
                return;
            }
            if (row.isNewRecord) {
                dg.datagrid('endEdit', index);
                dg.datagrid('deleteRow', index);
            } else {
                dg.datagrid('endEdit', index);
                dg.datagrid('deleteRow', index);
            }
            dg.edatagrid('recordDelRow', row, opts);
            var rows = $(jq).datagrid('getRows');
            if (rows && rows.length) {
                if (index <= parseInt(rows.length - 2)) {
                    $(jq).datagrid('selectRow', index); //将光标定位到当前行的下一行

                    opts.editIndex1 = index;
                } else {
                    $(jq).datagrid('selectRow', rows.length - 2); //将光标定位到最后一行

                    opts.editIndex1 = rows.length - 2;
                }
            } else {
                $(jq).edatagrid('addScroll');
            }
            //执行公式
            opts.onAfterDestroy.call(dg[0], index, row);
            //删除后重新计算合计

            $.view.SumAfterDel(dg, row);
        });
    }

});
$.extend($.fn.datagrid.methods, {
    //验证datagrid
    verifyData: function () {

        var opts = $(this).datagrid('options');
        var cols = opts.requireCol;
        if (!cols) return;
        var rows = $(this).datagrid('getRows');
        if (rows.length == 0) return;
        var msgError = "", fieldTitle = "";
        for (var i = 0, j = rows.length - 1; i < j; i++) {
            for (var m = 0, n = cols.length; m < n; m++) {

                var col = cols[m];
                if ($.trim(rows[i][col] + '').toLowerCase().length <= 0) {
                    var colO = $(this).datagrid('getColumnOption', col);
                    fieldTitle += $.trim(colO.title) + "、";
                }
            }
            var tempVal = fieldTitle.substring(0, fieldTitle.length - 1);
            fieldTitle = "";
            msgError += "第" + (i + 1) + "行：" + tempVal + "不能为空！</br>";
            if (tempVal == "") {
                msgError = "";
            }
        }
        return msgError;
    }
});
$.extend($.view, {
    setViewEditStatus: function (scope, status, notLoadData) {
        var dt = new Date();
        if (!scope || !status) return;
        var types = ["linkbutton", "datebox", "combogrid", "combobox", "validatebox", "numberbox", "edatagrid", "datagrid", "checkbox", "tree"];
        var preselector = "[id^='" + scope + "-'].easyui-";
        for (var i = 0, j = types.length; i < j; i++) {
            var type = types[i];

            var id = "[id^='" + scope + "-'].easyui-" + type;
            var contorls = $(id);
            contorls[type]("alterStatus", status);

            if (type == "edatagrid") {
                this.zkk = contorls;
            }
            if (!notLoadData && (type == "combogrid")) {
                contorls[type]('loadRemoteDataToLocal');
            }
        }
        this.setStatus(scope, status);
        this.setPageCancleState(scope);
        //        alert(dt - new Date()); alert('a')
    },
    loadDataEx: function (scope, data) {
        if (!scope) return;
        var ditem, id, jq, classattr, method;
        //data是从全局变量$.view.cancelObj中获取到得值，初始化时为null，需要进行判断处理。
        var m = data.m;
        //主单
        var m = data.m;
        //主单
        if (m) {
            var hsM = {};
            for (var i = 0, j = m.length; i < j; i++) {
                var field = m[i];
                hsM[field.m] = hsM[field.m] || {};
                hsM[field.m][field.n] = field.v;
            }
            var types = ["combobox", "validatebox"];
            var temp = "[id^='" + scope + "'].easyui-";

            for (var i = 0, j = types.length; i < j; i++) {
                var type = types[i];
                var id = temp + type;
                var eles = $(id);
                eles[type]('setData', hsM);
            }
            delete hsM;
        }
        if (!data) return;
        this.setDataForGrid(data.f, scope, 'setDefaultRow');
        //给列表赋值
        this.setDataForGrid(data.d, scope, 'setData');
    },
    loadData: function (scope, data, isNotTemp, isNotLoadGrid) {
        if (!scope) return;
        var ditem, id, jq, classattr, method;
        //data是从全局变量$.view.cancelObj中获取到得值，初始化时为null，需要进行判断处理。
        var m = data.m;
        //主单
        if (m) {
            var hsM = { isNotTemp: isNotTemp };
            for (var i = 0, j = m.length; i < j; i++) {
                var field = m[i];
                hsM[field.m] = hsM[field.m] || {};
                hsM[field.m][field.n] = field.v;
            }
            var types = ["datebox", "combogrid", "combobox", "validatebox", "numberbox", "checkbox", "searchbox"];
            var temp = "[id^='" + scope + "'].easyui-";

            for (var i = 0, j = types.length; i < j; i++) {
                var type = types[i];
                var id = temp + type;
                var eles = $(id);
                eles[type]('setData', hsM);
            }
            delete hsM;
        }
        if (!data) return;

        if (isNotLoadGrid) {
        }
        else {
            //给列表赋默认值
            debugger
            this.setDataForGrid(data.f, scope, 'setDefaultRow');
            //给列表赋值
            this.setDataForGrid(data.d, scope, 'setData');
        }



    },
    setDataForGrid: function (d, scope, funName) {
        if (funName == "setDefaultRow") {
            $.datagridF = d;
            return;
        }
        if (!d) return;
        if ($.datagridF != undefined && $.datagridF[0] != undefined && $.datagridF[0].r != undefined) {
            $("#gzd-SA_PlanActionDetail").datagrid({
                columns: $.view.createColumn($.datagridF[0].r)
            });
        }

        for (var i = 0; i < d.length; i++) {
            var ditem = d[i];
            if (ditem) {
                var id = scope + "-" + ditem.m;
                var jq = $('#' + id);
                if (!jq) continue;
                var classattr1 = jq.attr('class');
                if (!classattr1) continue;
                classattr = classattr1.split(' ')[0];
                if (classattr && classattr.indexOf('easyui-') == 0) {
                    var method = $.fn[classattr.replace("easyui-", "")].methods[funName];
                    if (method) {
                        method(jq, ditem);
                    }
                }
            }
        }
    },
    //动态创建列
    createColumn: function (itemArry) {
        var opts = $('#gzd-SA_PlanActionDetail').edatagrid('options');
        opts.defalutCol = {};
        opts.defaultHeadCol = {}; //默认的标头值 列字段名：列名称
        itemArry = $.datagridF[0].r;
        var columns = [[
                    { field: 'gzd-SA_PlanPersonSetModel-GUID_Person', title: '编号', align: 'center', hidden: 'true' },
                    { field: 'gzd-SA_PlanPersonSetModel-GUID_Department', title: '人员1', width: 60, align: 'center', hidden: 'true' }, //
                    {field: 'gzd-SA_PlanPersonSetModel-GUID_Bank', title: '人员2', width: 100, align: 'left', hidden: 'true' }, //
                    {field: 'gzd-SA_PlanPersonSetModel-DepartmentName', title: '所属部门', width: 160, align: 'left',
                    editor: {
                        type: 'combogrid',
                        options: {
                            gridassociate: {
                                gridId: 'gzd-SA_PlanActionDetail',
                                map: {
                                    'gzd-SA_PlanPersonSetModel-GUID_Department': ['GUID']
                                }
                            },
                            bindmethod: { 'onSelect': ['setAssociate'] },
                            panelWidth: 220,
                            width: 220,
                            remoteUrl: '/Combogrid/Department',
                            method: 'get',
                            idField: 'DepartmentName',
                            textField: 'DepartmentName',
                            filterField: 'DepartmentKey,DepartmentName',
                            columns: [[
                                    { field: 'GUID', title: '名称', width: '60', hidden: true },
                                    { field: 'DepartmentName', title: '部门名称', width: '200' }
                                ]]
                        }
                    }
                },
                    { field: 'gzd-SA_PlanPersonSetModel-BankName', title: '所属银行', width: 100, align: 'left',
                        editor: {
                            type: 'combogrid',
                            options: {
                                gridassociate: {
                                    gridId: 'gzd-SA_PlanActionDetail',
                                    map: {
                                        'gzd-SA_PlanPersonSetModel-GUID_Bank': ['GUID']
                                    }
                                },
                                bindmethod: { 'onSelect': ['setAssociate'] },
                                width: 520,
                                panelWidth: 240,
                                remoteUrl: '/Combogrid/Bank',
                                method: 'get',
                                idField: 'BankName',
                                delay: 1500,
                                filterField: 'BankKey,BankName',
                                textField: 'BankName',
                                sortName: 'BankKey',
                                columns: [[
                                            { field: 'GUID', hidden: 'true' },
                                            { field: 'BankKey', title: '银行编码', width: '80' },
                                            { field: 'BankName', title: '银行名称', width: '130' }
                                      ]]
                            }
                        }
                    },
                    { field: 'gzd-SA_PlanPersonSetModel-BankCardNo', title: '银行卡号', width: 120, align: 'left',
                        editor: {
                            type: 'validatebox' //''
                        }
                    },
                    { field: 'gzd-SA_PlanActionDetailModel-GUID', title: '明细编号', align: 'left', hidden: 'true' }
                    ]];


        var index = 6;
        for (var i = 0, j = itemArry.length; i < j; i++) {
            //工资项目类型 :金钱，日期，文本
            var itemType = itemArry[i][3].v;
            //类型+ItemName+Key
            var valueField = itemType + itemArry[i][2].n + itemArry[i][1].v; //itemArry[i][1].v编号Key
            var Valuetitle = itemArry[i][2].v;

            index = index + i;
            var colName = "gzd-SA_PlanItem-" + valueField;
            opts.defalutCol[colName] = "";
            opts.defaultHeadCol[colName] = Valuetitle;
            var objCol = $.view.GetColumnsObj(colName, Valuetitle, itemType);
            columns[0].splice(index, 0, objCol);

        }
        //添加隐藏列(工作类款项设置应用)
        index = index + itemArry.length;
        for (var i = 0, j = itemArry.length; i < j; i++) {
            var valueField = "gzlkxsz" + itemArry[i][1].v; // itemArry[i][2].n + itemArry[i][1].v; 
            var Valuetitle = "a" + i; //itemArry[i][2].v;
            index = index + i;
            var colName = "gzd-SA_PlanItem-" + valueField;
            opts.defalutCol[colName] = "";
            opts.defaultHeadCol[colName] = Valuetitle;
            columns[0].splice(index, 0, { field: colName, title: Valuetitle, width: 100, align: 'center', hidden: true });
        }
        return columns;
    },
    //列对象

    GetColumnsObj: function (colName, Valuetitle, itemType) {

        var obj = {};
        obj["field"] = colName;
        obj["title"] = Valuetitle;
        obj["width"] = 100;
        obj["align"] = 'right';
        switch (itemType) {
            case "2":
                obj["editor"] = { type: 'datebox' };
                obj["formatter"] = function (value, row, index) {
                    return value;
                };
                break;
            case "3":
                obj["editor"] = { type: 'text' };
                obj["formatter"] = function (value, row, index) {
                    return value;
                };
                break;
            default:
                obj["editor"] = { type: 'text' };
                obj["options"] = {
                    precision: 2,
                    max: 99999999.99
                };
                obj["formatter"] = function (value, row, index) {

                    if ((value != "" && value) || value == 0) {
                        return $.view.FormatNum(value);
                    } else {
                        return "";
                    }
                };
        }

        return obj;
    },
    //金钱格式化

    FormatNum: function (s1) {
        var isNumeric = $.isNumeric(s1);
        if (isNumeric == false) return "";
        s1 = new Number(s1).toFixed(2);
        var p = /(\d+)(\d{3})/;
        while (p.test(s1)) {
            s1 = s1.replace(p, "$1,$2");
        }
        return s1 == 0 ? "0.00" : s1;
    },
    OpenWin: function (index) {
        var grid = $("#gzd-SA_PlanActionDetail");
        var selectedRow = grid.datagrid("getSelected");
        var gridOpts = $.data(grid[0], 'edatagrid').options; //grid.datagrid('options');
        var curEditField = gridOpts.curEditField;
        var itemType = curEditField.split('-')[2].substring(0, 1);
        if (itemType == "2" || itemType == "3") return;
        $('#b-setwindow').dialog({
            resizable: false,
            title: '工资类款项设置',
            width: 1000,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: '/gzlkxsz/index',
            onLoad: function (rec) {

                var itemName = grid.edatagrid('options').defaultHeadCol[curEditField];
                var data = $.view.GetModelData(itemName, curEditField);
                $.view.loadData("gzlkxsz", data); //, true { m: valuesRow }
                $.view.setViewEditStatus("gzlkxsz", 1);
                $("#gzlkxsz-SA_PlanAction-ItemValue").attr("itemField", curEditField);

            },
            onClose: function () {
                $('#b-windowmx').dialog('destroy');
            }
        });
    },
    GetModelData: function (itemName, curEditField) {

        var data = {};
        var valuesRow = [];

        //部门
        var obj = { m: "", n: "", v: "" };
        var selectRow = $("#gzd-SA_PlanActionDetail").datagrid("getSelected")
        obj.m = "SA_PlanPersonSetModel";
        obj.v = selectRow['gzd-SA_PlanPersonSetModel-GUID_Department'];
        obj.n = "GUID_Department";
        valuesRow.push(obj);
        var obj0 = {};
        obj0.m = "SA_PlanPersonSetModel";
        obj0.v = selectRow['gzd-SA_PlanPersonSetModel-GUID_Department'];
        obj0.n = "DepartmentName";
        valuesRow.push(obj0);

        //人员
        var obj1 = {};
        obj1.m = "SA_PlanPersonSetModel";
        obj1.v = selectRow['gzd-SA_PlanPersonSetModel-PersonName'];
        obj1.n = "PersonName";
        valuesRow.push(obj1);

        var obj11 = {};
        obj11.m = "SA_PlanPersonSetModel";
        obj11.v = selectRow['gzd-SA_PlanPersonSetModel-GUID_Person'];
        obj11.n = "GUID_Person";
        valuesRow.push(obj11);

        //工作计划名称
        var obj2 = {};
        obj2.m = "SA_PlanAction";
        obj2.v = $("#gzd-SA_PlanAction-GUID_Plan").combogrid("getValue");
        obj2.n = "GUID_Plan";
        valuesRow.push(obj2);
        var obj21 = {};
        obj21.m = "SA_PlanAction";
        obj21.v = $("#gzd-SA_PlanAction-GUID_Plan").combogrid("getText");
        obj21.n = "PlanName";
        valuesRow.push(obj21);

        //单位名称
        var obj3 = {};
        obj3.m = "SA_PlanAction";
        obj3.v = $("#gzd-SA_PlanAction-GUID_DW").combogrid("getValue");
        obj3.n = "GUID_DW";
        valuesRow.push(obj3);

        var obj31 = {};
        obj31.m = "SA_PlanAction";
        obj31.v = $("#gzd-SA_PlanAction-GUID_DW").combogrid("getText");
        obj31.n = "DWName";
        valuesRow.push(obj31);

        //工资项名称

        var obj4 = {};
        obj4.m = "SA_PlanItem";
        obj4.v = itemName;
        obj4.n = "ItemName";
        valuesRow.push(obj4);


        //明细列GUID gzlkxsz-SA_PlanActionDetail-GUID
        //工资项Detail GUID
        var itemKey;
        var arr = curEditField.split('-');
        if (arr.length > 1) {
            itemKey = arr[2].toLowerCase();
            var index = itemKey.indexOf("itemname");
            itemKey = itemKey.substring(index + 8);
        }
        var itemFieldName = "gzd-SA_PlanItem-gzlkxsz" + itemKey;
        var itemDetailGUID = selectRow[itemFieldName];

        var obj41 = {};
        obj41.m = "SA_PlanActionDetail";
        obj41.v = itemDetailGUID;
        obj41.n = "GUID";
        valuesRow.push(obj41);

        var d = $.view.GetItemSetDetailData(itemDetailGUID);

        data.m = valuesRow;
        data.d = d;

        return data;
    },
    //工资类款项设置明细信息

    GetItemSetDetailData: function (guid) {
        var rowGird = {};
        $.ajax({
            url: "/gzlkxsz/Retrieve1",
            data: { "guid": guid },
            dataType: "json",
            type: "POST",
            async: false, //同步
            success: function (data) {
                if (data) {
                    rowGird = data.d;
                }
            }
        });
        return rowGird;
    },
    //工资项数据加载方式设置

    OpenItemSetWin: function () {
        $('#b-window').dialog({
            resizable: false,
            title: '工资项数据加载方式设置',
            width: 600,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: '/gzd/itemDataSet',
            onLoad: function (rec) {

                var combogrid = $("#gzd-SA_PlanAction-GUID_Plan");
                var planGUID = combogrid.combogrid("getValue");
                $.view.GetRetrieveData(planGUID);
                // $.view.setViewEditStatus("gzlkxsz", 2);

            }
        });
    },
    GetItemSetHeadData: function () {
        var valuesRow = [];
        var combogrid = $("#gzd-SA_PlanAction-GUID_Plan");
        var planGUID = combogrid.combogrid("getValue");
        //名称
        var obj = { m: "", n: "", v: "" };
        obj.m = "SA_Plan";
        obj.v = combogrid.combogrid("getText");
        obj.n = "PlanName";
        valuesRow.push(obj);
        //GUID
        var obj1 = { m: "", n: "", v: "" };
        obj1.m = "SA_Plan";
        obj1.v = planGUID;
        obj1.n = "GUID";
        valuesRow.push(obj1);
        //Key
        var obj2 = { m: "", n: "", v: "" };
        obj2.m = "SA_Plan";
        obj2.v = $("#gzd-SA_PlanAction-PlanKey").val();
        obj2.n = "PlanKey";
        valuesRow.push(obj2);
        //var data = $.view.GetRetrieveData(planGUID);

        return valuesRow;
    },
    GetRetrieveData: function (guid) {
        $.ajax({
            url: "/gzd/RetrievePlanItemSetUp",
            data: { "guid": guid },
            dataType: "json",
            type: "POST",
            async: false, //同步
            success: function (data) {

                if (data) {
                    $.view.loadData("gzxsj", data);
                    // $.view.setViewEditStatus("gzd", 1);
                }
                return null;
            }
        });
    },
    //计算获取数据
    JSGetData: function () {
        debugger
        var indata = $.view.retrieveData("gzd", "dataregion");
        debugger;
        if (!indata) return;
        var status = $.view.curPageState;
        $.ajax({
            url: "/gzd/JSGetData",
            data: { "status": status, "m": JSON.stringify(indata.m), "d": JSON.stringify(indata.d) },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {
                if (data.result == "success") { //成功                        
                    //加载页面数据
                    $.isLoadGrid = true;
                    $.view.loadDataEx("gzd", data);
                    $.view.setViewEditStatus("gzd", status);
                    $.messager.alert("提示", "计算完成！", "success")
                }
                else { //失败
                    //弹出错误提示
                    $.messager.alert(data.s.t, data.s.m, data.s.i);
                }
                ;
            }
        });
    },
    //根据计划获取对应的区间

    GetPlanPeriod: function (planId, year, month) {

        $.ajax({
            url: "/combo/GetPeriodByPlanID",
            data: { "guid": planId, "year": year, "month": month },
            dataType: "json",
            type: "POST",
            async: false, //同步
            success: function (data) {
                if (data) {
                    $('#gzd-SA_PlanAction-ActionPeriod').combobox({
                        valueField: 'ID',
                        textField: 'Text',
                        data: data
                    });
                }
            },
            error: function (xmlhttprequest, textStatus, errorThrown) {

            }
        });

    },
    //删除后重新计算合计

    SumAfterDel: function (grid, rowData) {
        var itemValue = 0;
        var itemName = "";
        var gridOpts = $.data(grid[0], 'edatagrid').options;
        var allRow = grid.datagrid("getRows");
        var hjIndex = allRow.length - 1;
        if (allRow.length > 1) {
            var row = allRow[hjIndex];
            var i = 0;
            for (var item in rowData) {
                if (item.toLowerCase().indexOf('itemname') < 0) continue;
                itemName = item;
                itemValue = $.isNumeric(rowData[item]) ? parseFloat(rowData[item]) : 0;

                var itemSumValue = $.isNumeric(row[itemName]) ? parseFloat(row[itemName]) : 0;
                row[itemName] = itemSumValue - itemValue;
                i++;
            }
            grid.datagrid("refreshRow", hjIndex);
        }
        //合并合计
        grid.datagrid('mergeCells', {
            index: hjIndex,
            field: 'gzd-SA_PlanPersonSetModel-PersonKey',
            rowspan: 0,
            colspan: 2
        });
    }
});
$.isLoadGrid = false;
//是否是历史过来的数据，如果是历史过来的数据，工资计划部可以选择，年月期间不可以编辑
$.isHistoryData = false;
$.extend($.fn.linkbutton.methods, {
    history: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'history');
        if (parms && parms.length >= 2) {
            var url = parms[0];
            var scope = opts.scope;
            var targetscope = parms[2];
            var pageState = $.view.getStatus(scope);
            if (url && targetscope) {
                var title = parms[3];
                var width = parms[4];
                var height = parms[5];
                var winId = '#' + opts.window;
                $(winId).dialog({
                    resizable: false,
                    title: title || '历史',
                    width: width || 1000,
                    height: height || 600,
                    modal: true,
                    minimizable: false,
                    maximizable: false,
                    collapsible: false,
                    href: url,
                    onLoad: function (c) {
                        var year = new Date().getFullYear();
                        var mothn = new Date().getMonth() + 1
                        $('#history-history-Month').combobox('setValue', mothn);
                        $('#history-history-Month').combobox('setText', mothn);
                        $('#history-history-Year').combobox('setValue', year);
                        $('#history-history-Year').combobox('setText', year);
                        $.view.setViewEditStatus(targetscope, pageState);
                      
                        
                        if (opts.historyBtnId) {
                            $("#" + opts.historyBtnId).click();
                        }
                    }
                });
            }
        }



    },
    //提交流程
    submitProcess: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitProcess');
        if (parms && parms.length) {
            var url = parms[0];
            if (url) {
                var guid = $.view.getKeyGuid(opts.scope);
                if (guid) {
                    $.ajax({
                        url: url,
                        data: { "guid": guid, "scope": opts.scope },
                        dataType: "json",
                        type: "POST",
                        traditional: true,
                        error: function (xmlhttprequest, textStatus, errorThrown) {
                            $.messager.alert("错误", $.view.warning, 'error');
                        },
                        success: function (response) {

                            $.fn.linkbutton.methods['submitProcessAfter'](guid, opts.scope);
                            var data = eval(response);
                            $.messager.alert(data.Title, data.Msg, data.Icon);
                        }
                    });
                }
            }
        }
    },
    //退回
    sendBack: function () {

        var opts = $(this).linkbutton('options');
        if ($.view.judgePageCancleState(opts.docState)) {
            $.messager.alert('提示', '该单据已经作废,操作无效')
            return;
        }

        var guid = $.view.getKeyGuid(opts.scope);
        $.ajax({
            url: '/' + opts.scope + '/SendBackFlow/',
            data: { "guid": guid, "scope": opts.scope },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (response) {
                $.fn.linkbutton.methods['submitProcessAfter'](guid, opts.scope);
                var data = eval(response);
                $.messager.alert(data.Title, data.Msg, data.Icon);
            }
        });
    },
    //流程
    viewProcess: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'viewProcess');
        if (!parms && parms.length != 4) return;
        var url = parms[0];
        var winId = '#' + opts.window;
        $(winId).dialog({
            isCancel: true,
            resizable: false,
            title: '流程',
            width: 1000,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: url,
            onLoad: function (c) {
                $.view.setViewEditStatus('process', 1);
                getData(parms[1], parms[2]);
            }

        });
        var getData = function (url, gridId) {
            var guid = $.view.getKeyGuid(opts.scope);
            if (!guid) return;
            $.ajax({
                url: url,
                data: { "Guid": guid },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {
                    if (data.Title) {
                        $.messager.alert(data.Title, data.Msg, data.Icon);
                    } else {
                        $('#' + gridId).datagrid('loadData', data);
                    }
                }
            });
        }
    },
    selectPerson: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'selectPerson');

        if (parms.length < 2) return;
        $('#b-window').dialog({
            isCancel: true,
            resizable: false,
            title: '选择人员',
            width: 400,
            height: 650,
            modal: true,
            draggable: true,
            resizable: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: parms[0],
            onLoad: function (c) {
                $.view.setViewEditStatus(parms[1], 4);
            }
        });
    },
    //发邮件
    SAemail: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'SAemail');
        if (parms.length < 1) return;
        $('#b-window').dialog({
            isCancel: true,
            resizable: false,
            title: '发送邮件',
            width: 250,
            height: 350,
            modal: true,
            draggable: true,
            resizable: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: parms[0],
            onLoad: function (c) {
                $.view.setViewEditStatus(parms[1], 4);
            }
        });
    },

    //提交选择项

    submitSelect: function () {

        var parms = $(this).linkbutton("getParms", "submitSelect");
        var gridId = parms[0];
        var treeId = parms[2]; //树ID
        //树选择项


        var getSelectRow = $("#" + treeId).tree("getChecked"); //selectPerson-tree-person
        if (getSelectRow.length == 0) return;
        var dg = $("#" + gridId);
        //增加
        //$('#' + gridId).edatagrid("addRow");       

        var gridRows = dg.datagrid('getRows');
        var isExistPerson = function (guid) {
            var personid = "";
            for (var i = 0; i < gridRows.length; i++) {
                personid = gridRows[i]["gzd-SA_PlanPersonSetModel-GUID_Person"];
                if (personid == guid) {
                    return true;
                }
            }
            return false;
        }

        var isTreeExistPerson = function (guid) {
            var personid = "";
            for (var i = 0; i < getSelectRow.length; i++) {
                personid = getSelectRow[i].attributes.GUID;
                if (personid == guid) {
                    return true;
                }
            }
            return false;
        }

        var personGUID = "";
        var rowIndex = gridRows.length - 1;
        var m = 0;
        for (var i = 0, j = getSelectRow.length; i < j; i++) {
            personGUID = getSelectRow[i].attributes.GUID;
            var mName = getSelectRow[i].attributes.m;
            if (isExistPerson(personGUID) == false && mName == "SS_Person") {
                var rowCopy = [];
                rowCopy['gzd-SA_PlanPersonSetModel-PersonKey'] = getSelectRow[i].attributes.PersonKey;
                rowCopy['gzd-SA_PlanPersonSetModel-PersonName'] = getSelectRow[i].attributes.PersonName;
                rowCopy['gzd-SA_PlanPersonSetModel-GUID_Person'] = personGUID;
                rowCopy['gzd-SA_PlanPersonSetModel-GUID_Department'] = getSelectRow[i].attributes.DepartmentName; //getSelectRow[i].attributes.GUID_Department;
                rowCopy['gzd-SA_PlanPersonSetModel-DepartmentName'] = getSelectRow[i].attributes.DepartmentName;
                rowCopy['gzd-SA_PlanPersonSetModel-GUID_Bank'] = "";
                rowCopy['gzd-SA_PlanPersonSetModel-BankCardNo'] = getSelectRow[i].attributes.BankCardNo;

                var index = rowIndex + m;
                var row = $.extend({}, rowCopy);
                dg.datagrid("insertRow", {
                    index: index,
                    row: row
                });
                dg.edatagrid('setCellValue', { field: 'gzd-SA_PlanPersonSetModel-GUID_Department', value: getSelectRow[i].attributes.GUID_Department, rowindex: index })

                // dg.datagrid("refreshRow",index)
                //dg.datagrid('appendRow', row);
                m++; //m不能用i代替 如果有相同的I值就不相同了
            }
        }
        //Grid删除Tree中没有选择的项
        for (var i = 0; i < gridRows.length; i++) {
            personid = gridRows[i]["gzd-SA_PlanPersonSetModel-GUID_Person"];
            var personKey = gridRows[i]["gzd-SA_PlanPersonSetModel-PersonKey"];
            if (isTreeExistPerson(personid) == false && personKey != "合计") {
                dg.datagrid("deleteRow", i);
            }
        }

        $.view.setViewEditStatus(parms[1], 1);
        //关闭
        $('#b-window').dialog('close');

    },
    beforeSaveFun: function (jq, status) {
        $.isLoadGrid = true;
    },
    //导入
    ExcelImport: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'ExcelImport');
        if (parms.length < 2) return;
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
            href: parms[0],
            onLoad: function (c) {
                $.view.setViewEditStatus(parms[1], 4);
            }
        });
    },
    //工资单中删除方法
    delGZDRow: function () {
        $(this).linkbutton('executeFun', 'delGZDRow');
    },
    beforeSave: function (jq, status) {

        $('#b-windowPBar').window({
            title: "提示",
            width: 600,
            height: 200,
            modal: true,
            minimizable: false,
            maximizable: false,
            closable: false,
            collapsible: false,
            border: false
        });

    },
    afterSave: function (status, isSuccess) {
        $('#b-windowPBar').window('close');
    },
    export: function () {

        var opts = $(this).linkbutton('options');
        //var indata = $.view.retrieveData(opts.scope, "dataregion");
        var parms = $(this).linkbutton('getParms', 'export');
        var url = parms[0];
        var datagrid = parms[1];
        var colName = [];
        var objGrid = $("#" + datagrid);
        var m = [];
        var d = [];
        var rows = objGrid.datagrid("getRows");
        var r = []; //所有行

        var dRow = "";
        for (var i = 0; i < rows.length; i++) {
            dRow = rows[i];
            var rowsCol = []; //一行所有列
            for (var item in dRow) {
                if (ColExist(item)) {
                    var rColObj = {}; //单个列对象
                    rColObj.m = "";
                    rColObj.n = item.split('-')[2];
                    rColObj.v = dRow[item];
                    rowsCol.push(rColObj);
                }
            }
            r.push(rowsCol);
        }
        d.m = "";
        d.push({ r: r });

        var froColFields = objGrid.datagrid("getColumnFields", true);
        for (var i = 0; i < froColFields.length; i++) {
            var col = objGrid.datagrid("getColumnOption", froColFields[i]);
            if (col.hidden == undefined) {
                if (ColExist(froColFields[i])) {
                    colName.push({ ColKey: froColFields[i].split('-')[2], ColName: col.title });
                }
            }
        }
        var colFields = objGrid.datagrid("getColumnFields");
        for (var i = 0; i < colFields.length; i++) {
            var col = objGrid.datagrid("getColumnOption", colFields[i]);
            if (col.hidden == undefined) {
                if (ColExist(colFields[i])) {
                    colName.push({ ColKey: colFields[i].split('-')[2], ColName: col.title });
                }
            }
        }

        $.ajax({
            url: url,
            data: { "m": JSON.stringify(m), "d": JSON.stringify(d), 'colName': JSON.stringify(colName) },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {
                if (data.msg) {
                    $.messager.alert("错误", data.msg, 'error');
                }
                else {
                    var path = encodeURIComponent(data.data);
                    var url = $.format("/gzd/DataExport?path={0}", path)
                    window.open(url);
                }
            }
        });

    }
});
 /*
扩展combobox
*/
$.extend($.fn.combobox.methods, {
    //    //转换当前可编辑状态
    setData: function (jq, hs) {
        return jq.each(function () {debugger
            var id = $(this).attr('id');
            var idAttr = id.split('-');
            if (!idAttr) return;
            if (hs) {
             
                var opts = $(this).combobox('options');
                var a = hs[idAttr[1]];
                if (a && a.hasOwnProperty(idAttr[2])) {
                    if (id == "gzd-SA_PlanAction-ActionState") {
                        var text = a[idAttr[2]] == "1" ? "已发放" : "未发放";
                        $(this).combobox('setValue', a[idAttr[2]]);
                        $(this).combobox('setText', text);
                    } else {
                        $(this).combobox('setValue', a[idAttr[2]]);
                        $(this).combobox('setText', a[opts.textV] || '');
                    }
                    return;
                }
                return;
            }
            $(this).combobox('setText', '');
            $(this).combobox('setValue', '');
        })
    },
    alterStatus: function (jq, status) {
        return jq.each(function () {
            if (!status) return;
            var opts = $(this).combobox('options');
            if (!opts.customBind || opts.mustBind)
                $.view.bind.call($(this), 'combobox');
            if (!opts.forbidstatus) return;
            for (var i = 0; i < opts.forbidstatus.length; i++) {
                var c = opts.forbidstatus[i];
                if (c == -1 || c == status) {
                    if (!opts.disabled) {
                        debugger
                        $(this).combo('disable');
                    }
                    return;
                }
            }
            if (opts.disabled) {
                $(this).combo('enable');
            }

            //如果是历史来的数据 设置工资计划为不可以选择，年 月 期间不可以编辑

            if ($.isHistoryData && status == '2') {
                setEditState();
            }

        });
    }
});

$(function () {
    //工资计划选择
    $("#gzd-SA_PlanAction-GUID_Plan").combobox({
        //onSelect: function (rowIndex, rowData) {
        onChange: function (newValue, oldValue) {
            debugger

            if ($.isLoadGrid || oldValue == "" || newValue == "") {
                $.isLoadGrid = false;
                return;
            }
            $.ajax({
                url: "/gzd/NewByPlan",
                data: { "guid": newValue },
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    $.message.alert("提示...", "获取数据错误！", "error");
                    return;
                },
                success: function (data) {
                    debugger
                    if (data.result == "success") { //成功                        
                        $.view.loadData("gzd", data);
                        ReLoadPeriod();
                        //$.view.GetPlanPeriod(newValue);
                    }
                    else { //失败
                        //弹出错误提示
                        $.messager.alert(data.s.t, data.s.m, data.s.i);
                    }
                }
            });
        },
        onLoadSuccess: function (data) {
            //重置计划对应的期间

            ReLoadPeriod();
        }
    });


    //工资发放 UpdateActionState
    $("#gzd-fafang").click(function () {
        $('#gzd-SA_PlanActionDetail').edatagrid('saveRow');
        var guid = $("#gzd-SA_PlanAction-GUID").validatebox("getValue");
        if (!guid) {
            $.messager.alert("提示", "请先保存工资数据！");
            return;
        }
        $.ajax({
            url: "/gzd/UpdateActionState",
            data: { "guid": guid },
            dataType: "json",
            type: "POST",
            success: function (data) {

                if (data) {
                    $("#gzd-SA_PlanAction-ActionState").combobox("setValue", 1);
                    $.messager.alert(data.t, data.m, data.i);
                }
                else {
                    $.messager.alert("提示", "系统错误！", "error");
                }
            }
        });
    });
    $("#gzd-setValue").click(function () {
        $('#gzd-SA_PlanActionDetail').edatagrid('saveRow');
        $.view.OpenItemSetWin();
    });
    $("#gzd-Count").click(function () {debugger
        var id = $('#gzd-SA_PlanAction-GUID').val();
        if (!id || id == "00000000-0000-0000-0000-000000000000") {
            $.messager.alert("提示", "请先保存在进行计算！");
            return;
        }
        $('#gzd-SA_PlanActionDetail').edatagrid('saveRow');
        //$.messager.confirm("提示", "确定要删除吗?", function (data) {
        $.messager.confirm("提示", "会重新计算有公式的工资项值，确定要执行吗？", function (r) {
            if (r) {
                $.view.JSGetData();
            }
        })

    })

    $('#gzd-SA_PlanAction-ActionMouth').combobox({
        onSelect: function (rec) {
            var planID = $("#gzd-SA_PlanAction-GUID_Plan").combogrid("getValue");
            var year = $("#gzd-SA_PlanAction-ActionYear").combobox("getText");
            var month = rec.value;
            $.view.GetPlanPeriod(planID, year, month);
        }
    });
    $('#gzd-SA_PlanAction-ActionYear').combobox({
        onSelect: function (rec) {
            var planID = $("#gzd-SA_PlanAction-GUID_Plan").combogrid("getValue");
            var year = rec.value;
            var month = $("#gzd-SA_PlanAction-ActionMouth").combobox("getValue");
            $.view.GetPlanPeriod(planID, year, month);
        }
    });
    var grid = $("#gzd-SA_PlanActionDetail");
    var beforeItemValue = 0;
    //加载数据
    $.isLoadSetValue = true;
    //合并并并计算合计
    grid.datagrid({
        onLoadSuccess: function (data) {
            var funLoad = function (data, grid) {
                var hjRow = {};
                //计算合计数据
                var rowCount = data.rows.length;
                var colArrary = grid.datagrid("getColumnFields");
                var colCount = colArrary.length;
                //是否一列数据都为Null值

                var isColNull = true;
                var itemSumValue = 0;
                for (var j = 5; j < colCount; j++) {
                    var columnName = colArrary[j];
                    itemSumValue = 0;
                    if (columnName.toLowerCase().indexOf("itemname") >= 0) {

                        var itemType = columnName.split('-')[2].substring(0, 1);
                        if (itemType == "2" || itemType == "3") continue;
                        for (var i = 0; i < rowCount; i++) {
                            var row = data.rows[i];
                            var stritemValue = row[columnName];
                            if (stritemValue == undefined || stritemValue == null) continue;
                            isColNull = false;
                            var itemValue = $.isNumeric(stritemValue) ? parseFloat(stritemValue) : 0;
                            itemSumValue += itemValue;
                        }
                        //
                        if (isColNull) {
                            hjRow[columnName] = null;
                        }
                        else {
                            hjRow[columnName] = itemSumValue;
                        }
                    }
                }

                hjRow["gzd-SA_PlanPersonSetModel-PersonKey"] = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;合计";
                //添加合计行

                grid.datagrid("appendRow", hjRow); //{"gzd-SA_PlanPersonSetModel-PersonKey":"合计"}
                var hjIndex = data.rows.length - 1;
                grid.datagrid('mergeCells', {
                    index: hjIndex,
                    field: 'gzd-SA_PlanPersonSetModel-PersonKey',
                    rowspan: 0,
                    colspan: 2
                });

                grid.datagrid("loaded");
            }
            funLoad(data, grid);

        },
        onBeforeEdit: function (rowIndex, rowData, changes) {
            var index = grid.datagrid("getRows").length - 1;
            if (rowIndex == index) {
                return false;
            }
        },
        editEditAfterEvent: function (rowIndex, rowData, changes) {
            var itemValue = 0;
            var itemName = "";
            var isrefresh = true;
            var gridOpts = $.data(grid[0], 'edatagrid').options;
            var curEditValue = gridOpts.curEditValue;
            beforeItemValue = $.isNumeric(curEditValue) ? parseFloat(curEditValue) : 0;

            for (var item in changes) {
                itemName = item;

                var itemType = itemName.split('-')[2].substring(0, 1);
                if (itemType == "2" || itemType == "3") { //日期 文本格式不统计

                    isrefresh = false;
                }
                itemValue = $.isNumeric(changes[item]) ? parseFloat(changes[item]) : 0;
            }
            if (isrefresh == false) return;

            var allRow = grid.datagrid("getRows"); //getRows;
            var hjIndex = allRow.length - 1;
            if (allRow.length > 1) {
                var row = allRow[hjIndex];
                var itemSumValue = $.isNumeric(row[itemName]) ? parseFloat(row[itemName]) : 0;
                row[itemName] = itemSumValue - beforeItemValue + itemValue;
                grid.datagrid("refreshRow", hjIndex);
            }

            //合并合计
            grid.datagrid('mergeCells', {
                index: hjIndex,
                field: 'gzd-SA_PlanPersonSetModel-PersonKey',
                rowspan: 0,
                colspan: 2
            });
        }
    });

}
);
//设置编辑状态

var setEditState = function () {   
    $("#gzd-SA_PlanAction-GUID_Plan").combogrid('disable');
    $("#gzd-SA_PlanAction-ActionYear").combobox('disable');
    $("#gzd-SA_PlanAction-ActionMouth").combobox('disable');
    $("#gzd-SA_PlanAction-ActionPeriod").combobox('disable');

}
var ReLoadPeriod= function () {
    //重置计划对应的期间

    var planID = $("#gzd-SA_PlanAction-GUID_Plan").combogrid("getValue");
    var year = $("#gzd-SA_PlanAction-ActionYear").combobox("getValue");
    var month = $("#gzd-SA_PlanAction-ActionMouth").combobox("getValue");
    $.view.GetPlanPeriod(planID, year, month);
}
//列是否存在
var ColExist = function (value) {
    var arr = ['PersonKey', 'PersonName', 'ItemName'];
    for (var i = 0; i < arr.length; i++) {
        if (value.indexOf(arr[i]) >= 0) {
            return true;
        }
    }
    return false;
}