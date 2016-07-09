$.extend($.fn.linkbutton.methods, {
    //保存单据 与原来的稍有不同 成功后不设置状态
    saveDoc: function () {

        var me = this;
        $.fn.linkbutton.methods["beforeSaveFun"].call($(me));

        var saveAjax = function (status, indata, opts) {
            if ($.view.curPageState == 6 || $.view.curPageState == 5) {
                status = '2';
            }
            $.ajax({
                url: url,
                data: { "status": status, "m": JSON.stringify(indata.m), "d": JSON.stringify(indata.d) },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {

                    switch (status) {
                        case "1": //新建
                            if (data && data.result == "success") {

                                //设置页面编辑状态

                                var date = new Date()
                                $.view.loadData(opts.scope, data);

                                var date = new Date()
                                // $.view.setViewEditStatus(opts.scope, opts.status);

                            } else {
                                $.view.setStatus(opts.scope, status);
                            }
                            //弹出提示
                            if (!data.s) return;
                            $.messager.alert(data.s.t, data.s.m, data.s.i);
                            break;
                        case "2": //修改
                            if (data && data.result == "success") {
                                //还原页面初始状态
                                //$.view.clearView(opts.scope, "dataregion");
                                //加载页面数据
                                $.view.loadData(opts.scope, data);
                                var sTemp = opts.status;
                                if ($.view.curPageState == 6 || $.view.curPageState == 5) {
                                    //sTemp = 3;
                                }
                                $.view.setViewEditStatus(opts.scope, sTemp);
                                //支票
//                                var zp = new window();
//                                zp.zp = new ScriptEngine;
                            }
                            else {
                                $.view.setStatus(opts.scope, status);
                            }
                            if (!data.s) return;
                            $.messager.alert(data.s.t, data.s.m, data.s.i);
                            break;
                        case "3": //删除
                            if (data && data.result == "success") {
                                //还原页面初始状态
                                //$.view.clearView(opts.scope, "dataregion");
                                //设置页面编辑状态
                                $.view.setViewEditStatus(opts.scope, 1); //                                  
                                $.view.loadData(opts.scope, data);
                            }
                            else {
                                var msg = data.s.m;
                                if (msg) {
                                    $.messager.alert('提示', msg);
                                }
                                $.view.setStatus(opts.scope, status);
                            }
                            break;
                    }

                    //对单据操作完成以后判断单据是否保存成功
                    //  $(me).linkbutton('afterSave', status);

                    $.fn.linkbutton.methods["afterSave"].call($(me), status);

                    $.view.cancelObj = { data: data, status: 4 };
                }
            });
        }
        var saveAjaxBefore = function (opts, saveAjax) {
            var status = $.view.getStatus(opts.scope);
            var indata = $.view.retrieveData(opts.scope, "dataregion");
            //设置保存按钮的状态属性


            $.view.setStatus(opts.scope, opts.status);
            if (!indata) return;
            //删除提示
            if (status == "3" && $.view.curPageState != 5 && $.view.curPageState != 6) {
                $.messager.confirm("提示", "确定要删除吗?", function (data) {
                    if (!data) {
                        $.view.setStatus(opts.scope, status);
                        return;
                    } else {
                        saveAjax(status, indata, opts);
                    }
                })
            } else {
                saveAjax(status, indata, opts);
            }
        }
        $(this).linkbutton('wholeGridEndEdit');
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'saveDoc');
        if (!parms) return;
        var url = parms[0];
        if (!url) return;
        var isSussess = true;

        //基础档案校验 
        if (opts.JCDA) {
            isSussess = $.fn.linkbutton.methods["examine"]($(this));    //跳转路径：Scripts/jc/jc.js
            if (!isSussess) {
                return;
            }
        } else {
            //常规校验
            if (parms[1]) {
                isSussess = $.fn.linkbutton.methods["examine"].call($('#' + parms[1]), true);
                if (!isSussess) {
                    return;
                }
            }
        }
        if (isSussess == true) {
            saveAjaxBefore(opts, saveAjax);
        } else {
            $.messager.confirm("提示", isSussess, function (data) {
                if (data) {
                    saveAjaxBefore(opts, saveAjax);
                }
            })
        }
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

        //列表项值
        var itemValue = $("#" + configCol[0]).validatebox("getText").replace(",", "");
        var itemField = $("#" + configCol[0]).attr("itemfield");
        //selectdetail[itemField] = itemValue;

        //列表项的隐藏值
        var actionDetailGUID = $("#" + configCol[2]).validatebox("getText");
        var fieldKey;
        var arr = itemField.split('-');
        if (arr.length > 1) {
            fieldKey = arr[2];
        }
        var itemKey = fieldKey.toLowerCase();
        var itemnameIndex = itemKey.indexOf("itemname");
        itemKey = itemKey.substring(itemnameIndex + 8);

        var hiddenField = "gzd-SA_PlanItem-gzlkxsz" + itemKey;

        //当前编辑列值
        var currentEdit = {};
        if (actionDetailGUID) {
            currentEdit[itemField] = itemValue;
            currentEdit[hiddenField] = actionDetailGUID;

            //重新计算合计
            var changes = {};
            changes[itemField] = itemValue;
            $.view.SumEditAfterClose(detailGrid, changes);
        }

        var rowIndex = detailGrid.datagrid("getRowIndex", selectdetail);
        selectdetail = $.extend(selectdetail, currentEdit);

        detailGrid.datagrid('refreshRow', rowIndex);

        //关闭窗口
        var winId = "#" + opts.window;
        $(winId).dialog('close');
    }
})

$.extend($.view, {  
    //加载数据，不清除主表数据（即表头信息） 
    loadData: function (scope, data, isTemp, isNotLoadGrid) {
    
        if (!scope) return;
        var ditem, id, jq, classattr, method;
        //data是从全局变量$.view.cancelObj中获取到得值，初始化时为null，需要进行判断处理。
        

        if (!data) return;
        var m = data.m;
        //主单
        if (m) {
            var hsM = { isNotTemp: null };
            for (var i = 0, j = m.length; i < j; i++) {
                var field = m[i];
                hsM[field.m] = hsM[field.m] || {};
                hsM[field.m][field.n] = field.v;
            }
            if (isTemp) {
                $("#gzlkxsz-SA_PlanActionDetail-GUID")["validatebox"]('setData', hsM);
            }
            else {               
                var types = ["datebox", "combogrid", "combobox", "validatebox", "numberbox", "checkbox", "searchbox"];
                var temp = "[id^='" + scope + "'].easyui-";

                for (var i = 0, j = types.length; i < j; i++) {
                    var type = types[i];
                    var id = temp + type;
                    $(id)[type]('setData', hsM);
                }
               
            }           
            delete hsM;
        }
        if (isNotLoadGrid) return;
        //给列表赋默认值
        this.setDataForGrid(data.f, scope, 'setDefaultRow');
        //给列表赋值
        this.setDataForGrid(data.d, scope, 'setData');
    },
    setDataForGrid: function (d, scope, funName) {
        if (!d) return;
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
    //款项设置窗口关闭时从新计算合计数据
    SumEditAfterClose: function (grid,changes) {
        var itemValue = 0;
        var itemName = "";

        var gridOpts = $.data(grid[0], 'edatagrid').options;
        var curEditValue = gridOpts.curEditValue;
        beforeItemValue = $.isNumeric(curEditValue) ? parseFloat(curEditValue) : 0;

        for (var item in changes) {
            itemName = item;
            itemValue = $.isNumeric(changes[item]) ? parseFloat(changes[item]) : 0;
        }

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
            colspan: 5
        });
    }
});