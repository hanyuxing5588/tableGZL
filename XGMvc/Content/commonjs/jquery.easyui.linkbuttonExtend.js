/*
扩展linkbutton
*/
$.extend($.fn.linkbutton.methods, {
    /*关联单据*/
    linkDoc: function () {
        var opts = $(this).linkbutton('options');
        var guid = $.view.getKeyGuid(opts.scope);
        var parms = $(this).linkbutton('getParms', 'linkDoc');
        var scope = opts.scope;
        var winId = '#' + opts.window;
        $(winId).dialog({
            resizable: false,
            title: '关联单据',
            width: 1000,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: '/gldoc/Index?guid=' + guid,
            onLoad: function (c) {
            }
        });
    },
    //设置状态

    //4:浏览 1:新建 2:修改 3:删除
    setStatus: function () {

        var opts = $(this).linkbutton('options');
        //将之前的废除状态9改为5 sxh 2014/04/02 15:23
        if ($.view.getPageState(opts.docState) == 5) {
            $.messager.alert('提示', '该单据已经作废,操作无效')
            return;
        }
        if ($.fn.linkbutton.methods["setStatusBefore"].call(this, opts.status) == false) return;
        $.view.curPageState = opts.status;
        $(this).linkbutton('setWholeStatus');
        $(this).linkbutton('saveStatus');
        var pageState = $.view.getStatus(opts.scope);
        $.fn.linkbutton.methods["setStatusAfter"].call(this, pageState);
    },
    setStatusAfter: function (jq, status) {

    },
    setStatusBefore: function (jq, status) {

    },
    addRow: function () {
        $(this).linkbutton('executeFun', 'addRow');
    },
    InsertRow: function () {
        $(this).linkbutton('executeFun', 'InsertRow');
    },
    delRow: function () {
        $(this).linkbutton('executeFun', 'delRow');
    },
    moveUp: function () {
        $(this).linkbutton('executeFun', 'moveUp');
    },
    moveDown: function () {
        $(this).linkbutton('executeFun', 'moveDown');
    },
    executeFun: function (jq, funName) {

        var parms = $(jq).linkbutton('getParms', funName);
        if (parms && parms.length) {
            var gridId = parms[0];
            if (gridId) {
                $('#' + gridId).edatagrid(funName);
            }
        }
    },

    //添加金额正负切换功能 sxh 2014/04/09 10:40
    //提示：暂时先将switchValue和switchConquer这两个方法分开，

    //等回头整理代码的时候，将这两个功能整合到一个方法中去

    switchConquer: function () {
        //获取按钮option属性  
        var opts = $(this).linkbutton('options');
        //获取按钮前台配置的属性




        var parms = $(this).linkbutton('getParms', 'switchConquer');
        //获取到grid的id
        var gridId = '#' + parms[0];
        //获取借方金额和贷方金额的id
        var borrId = '#' + parms[1], loanId = '#' + parms[2];
        //获取到grid的当前选中行




        var selRow = $(gridId).edatagrid('getSelected');
        if (!selRow) {
            $.messager.alert('提示', '请先选中一条记录然后再红蓝！', 'info');
            return;
        }
        //从选中行中获取借方金额和贷方金额的数值




        var borrVal = selRow[parms[1]], loanVal = selRow[parms[2]];
        if (borrVal == undefined) borrVal = 0;
        if (loanVal == undefined) loanVal = 0;
        if (!!parseInt(borrVal) || !!parseInt(loanVal)) {
            selRow[parms[1]] = -borrVal;
            selRow[parms[2]] = -loanVal;
        }
        //选中行的索引值




        selRowIndex = $(gridId).edatagrid('getRowIndex', selRow);
        //将借方金额或贷方金额修改以后重新刷新当前行
        $(gridId).edatagrid("refreshRow", selRowIndex);
        $(gridId).edatagrid("selectRow", selRowIndex);
        $(gridId).edatagrid('doFormula');
    },
    //添加金额互换功能 sxh 2014/04/09 11:10
    //提示：暂时先将switchValue和switchConquer这两个方法分开，




    //等回头整理代码的时候，将这两个功能整合到一个方法中去




    switchValue: function () {
        //获取按钮option属性  
        var opts = $(this).linkbutton('options');
        //获取按钮前台配置的属性




        var parms = $(this).linkbutton('getParms', 'switchValue');
        //获取到grid的id
        var gridId = '#' + parms[0];
        //获取借方金额和贷方金额的id
        var borrId = '#' + parms[1], loanId = '#' + parms[2];
        //获取到grid的当前选中行




        var selRow = $(gridId).edatagrid('getSelected');

        if (!selRow) {
            $.messager.alert('提示', '请先选中一条记录然后再借贷！', 'info');
            return;
        }
        //从选中行中获取借方金额和贷方金额的数值




        var borrVal = selRow[parms[1]], loanVal = selRow[parms[2]], Total_PZ = selRow[parms[3]], IsDC = selRow[parms[4]];
        if (borrVal == undefined) borrVal = 0;
        if (loanVal == undefined) loanVal = 0;
        if (Total_PZ == undefined) Total_PZ = 0;
        if (!!parseInt(borrVal) || !!parseInt(loanVal)) {
            selRow[parms[1]] = loanVal;
            selRow[parms[2]] = borrVal;
            if (!parseInt(borrVal) && !!parseInt(loanVal)) {
                selRow[parms[3]] = loanVal;
                selRow[parms[4]] = "False";
            } else if (!parseInt(borrVal) && !!parseInt(loanVal)) {
                selRow[parms[3]] = borrVal;
                selRow[parms[4]] = "True";
            }

        }
        //选中行的索引值




        selRowIndex = $(gridId).edatagrid('getRowIndex', selRow);
        //将借方金额或贷方金额修改以后重新刷新当前行
        $(gridId).edatagrid("refreshRow", selRowIndex);
        $(gridId).edatagrid("selectRow", selRowIndex);
        $(gridId).edatagrid('doFormula');
    },
    //借款
    borrow: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        var parms = $(this).linkbutton('getParms', 'borrow');
        if (parms && parms.length >= 2) {
            var url = parms[0];
            var scope = opts.scope;
            var targetscope = parms[2];
            if (url && targetscope) {
                var title = parms[3];
                var width = parms[4];
                var height = parms[5];
                var winId = '#' + opts.window;
                $(winId).dialog({
                    resizable: false,
                    title: '借款',
                    width: 1000,
                    height: 600,
                    modal: true,
                    minimizable: false,
                    maximizable: false,
                    collapsible: false,
                    href: url,
                    onLoad: function (c) {

                        $.view.setViewEditStatus(targetscope, pageState);
                    }
                });
            }
        }
    },
    //预算
    BudgetStatistics: function () { //sxb   
        var GetBudgetDataForCal = function (detailGridId, mapText, map, isView) {
            var rowDto = {}, oParmasArr = [];

            if (isView) {
                for (var controlType in mapText) {
                    for (var i = 0, j = mapText[controlType].length; i < j; i++) {
                        var controlId = mapText[controlType][i], control$ = $('#' + controlId);
                        if (controlType == "combogrid") {
                            var opts = $('#' + controlId).combogrid('options');
                            var hideV = control$.combo('getValue');
                            var showV = control$.combo('getText');
                            var mapControl = map[controlId]
                            if (mapControl.length > 1) {
                                rowDto[mapControl[0]] = hideV;
                                rowDto[mapControl[1]] = showV;
                            }
                            else if (mapControl.length == 1) {
                                rowDto[mapControl[0]] = showV;
                            }
                        } else {
                            var v = control$.val();
                            var attrArr = controlId.split('-');
                            if (attrArr.length > 2) {
                                //如果在map中有匹配的项找对应的匹配的字段


                                if (map[controlId]) {
                                    rowDto[map[controlId]] = v;
                                }
                                else {
                                    rowDto[attrArr[2]] = v;
                                }
                            }
                        }
                    }
                }
                oParmasArr.push(rowDto);
            } else {
                var detailGrid = $("#" + detailGridId);
                var detailRowArr = detailGrid.datagrid("getRows"), j = detailRowArr.length;
                if (j == 0) {
                    $.messager.alert('提示', '没有明细就没有预算信息');
                    return;
                }
                for (var i = 0; i < j; i++) {
                    rowDto = {};
                    var curRow = detailRowArr[i];
                    //取隐藏值


                    for (var colName in map) {
                        var val = curRow[colName];
                        if (colName.indexOf('GUID') > 0) {
                            val = detailGrid.edatagrid('getCellValue', { field: colName, rowindex: i });
                        }
                        rowDto[map[colName]] = val || '';
                    }
                    //取显示值


                    for (var colName in mapText) {

                        var val = curRow[colName];

                        rowDto[mapText[colName]] = val || '';
                    }

                    oParmasArr.push(rowDto);
                }
            }
            return oParmasArr;
        }
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        var parms = $(this).linkbutton('getParms', 'BudgetStatistics');
        if (parms && parms.length >= 2) {

            var url = parms[0], dataurl = parms[1], gridId = parms[2], docDateId = parms[3], detailGridId = parms[4], targetscope = parms[5], ywKey = parms[6];

            pageState = $.view.getStatus(opts.scope);

            var oParmasArr = [], map = opts.columnMap, docDate = $('#' + docDateId).datebox('getValue'), mapText = opts.columnMapText;
            oParmasArr = GetBudgetDataForCal(detailGridId, mapText, map, opts.isGetDataByView);
            //configBudget 预算信息
            if (url && targetscope) {
                var winId = '#' + opts.window;
                $(winId).dialog({
                    resizable: false,
                    title: '预算统计',
                    width: 800,
                    height: 600,
                    modal: true,
                    minimizable: false,
                    maximizable: false,
                    collapsible: false,
                    href: url,
                    onLoad: function (c) {
                        if (parms && parms.length) {
                            $.ajax({
                                data: { condition: JSON.stringify(oParmasArr), docDate: docDate, docId: $.view.getKeyGuid(opts.scope) || '', ywKey: ywKey },
                                url: dataurl,
                                type: "POST",
                                success: function (data) {
                                    if (data) {
                                        $('#' + gridId).datagrid('loadData', data);
                                    } else {
                                        $('#' + gridId).edatagrid('addScroll');
                                    }

                                    //预算页面加载信息
                                    var valss = [];
                                    var config = opts.configBudget;
                                    for (var attrId in config) {
                                        var v = $('#' + attrId)[config[attrId]]('getData', true);
                                        if (v && v.length > 0) {
                                            // v.length ? (valss = valss.concat(v)) : valss.push(v);
                                            for (var i = 0; i < v.length; i++) {
                                                v[i].m = "BX_Main";
                                                valss.push(v[i]);
                                            }

                                        }
                                        else {
                                            v.m = "BX_Main";
                                            valss.push(v);
                                        }
                                    }
                                    $.view.loadData(targetscope, { m: valss });
                                    $.view.setViewEditStatus(targetscope, pageState);
                                }
                            })

                        }
                    }
                });
            }
        }
    },
    wholeGridEndEdit: function () {
        $('.easyui-edatagrid').edatagrid('saveRow');
    },
    gridEndEdit: function () {
        var parms = $(this).linkbutton('getParms', 'gridEndEdit');
        if (parms && parms.length) {
            var gridId = parms[0];
            if (gridId) {
                $('#' + gridId).edatagrid('saveRow');
            }
        }
    },
    //新增单据信息
    newDoc: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'newDoc');
        if (!parms || !parms.length) return;
        var url = parms[0];
        if (!url) return;
        $.post(url, null, function (data, status) {

            $.view.setStatus(opts.scope, opts.status);

            if (!data) return;
            if (data.result == "success") { //成功
                $.view.clearView(opts.scope, "dataregion");
                //加载页面数据
                //设置页面编辑状态
                $.view.curPageState = 1;

                $.view.loadData(opts.scope, data);

                $.view.setViewEditStatus(opts.scope, opts.status);

                $.view.cancelObj = { data: data, status: opts.status };
            }
            else { //失败
                //弹出错误提示
                $.messager.alert(data.s.t, data.s.m, data.s.i);
            }
        });
    },
    //保存单据
    saveDoc: function () {
        
        var me = this;
        var opts = $(me).linkbutton('options');
        var status = $.view.getStatus(opts.scope);
        var bResult = $.fn.linkbutton.methods["beforeSave"].call(this, status);
        if (bResult == false) return;

        var saveAjax = function (status, indata, opts) {
            if ($.view.curPageState == 6 || $.view.curPageState == 5) {
                status = '2';
            }
            var date = new Date();
            $.ajax({
                url: url,
                data: { "status": status, "m": JSON.stringify(indata.m), "d": JSON.stringify(indata.d) },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {debugger
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {

                    switch (status) {
                        case "1": //新建
                            if (data && data.result == "success") {
                                //还原页面初始状态


                                //                                var date = new Date()
                                /*2014-7-23 hyxUpdate*/
                                //                                $.view.clearView(opts.scope, "dataregion");
                                //加载页面数据
                                //设置页面编辑状态


                                //alert("清空数据时间" + new Date() - date)
                                //                                var date = new Date()


                                $.view.loadData(opts.scope, data);
                                //                                alert("设置加载数据时间" + (new Date() - date))
                                //                                var date = new Date()
                                $.view.setViewEditStatus(opts.scope, opts.status);
                                //                                alert("设置状态时间" + (new Date() - date))
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

                                /*2014-7-23 hyxUpdate*/
                                //                              $.view.clearView(opts.scope, "dataregion");
                                //加载页面数据
                                //                                var date = new Date()
                                $.view.loadData(opts.scope, data);
                                //                                alert("设置加载数据时间" + (new Date() - date))
                                var sTemp = opts.status;
                                if ($.view.curPageState == 6 || $.view.curPageState == 5) {
                                    //sTemp = 3;
                                }
                                //                                var date = new Date()
                                $.view.setViewEditStatus(opts.scope, sTemp);
                                //                                alert("设置加载数据时间" + (new Date() - date))
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


                                $.view.clearView(opts.scope, "dataregion");
                                //设置页面编辑状态
                                $.view.loadData(opts.scope, data);
                                $.view.setViewEditStatus(opts.scope, 1); //                                  

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




                    //                    $(me).linkbutton('afterSave', status);
                    $.fn.linkbutton.methods["afterSave"].call($(me), status, data.result);

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
                });
            } else {
                //如果有提示确认信息，要提示确认

                if (opts.isConfirm) {
                    var msg = opts.confirmMsg;
                    $.messager.confirm("提示", msg, function (data) {
                        if (!data) {
                            $.view.setStatus(opts.scope, status);
                            return;
                        } else {
                            saveAjax(status, indata, opts);
                        }
                    });
                }
                else {
                    saveAjax(status, indata, opts);
                }
            }

        }
        //        $.view.saveDocMemoNotCconcat = true;
        $(this).linkbutton('wholeGridEndEdit');
        //        $.view.saveDocMemoNotCconcat = false;
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
                    //afterSave
                    $.fn.linkbutton.methods["afterSave"].call(this, status)
                    return;
                }
            }
        }

        if (isSussess == true) {

            var isExmine = $.fn.linkbutton.methods["examineAfter"]($(this), status, function () { saveAjaxBefore(opts, saveAjax); });

        } else {
            $.messager.confirm("提示", isSussess, function (data) {
                if (data) {
                    var isExmine = $.fn.linkbutton.methods["examineAfter"]($(this), status, function () { saveAjaxBefore(opts, saveAjax); });
                }
            })
        }
    },
    //判断单据是否保存成功
    //可扩展方法---如果想在保存成功之后做任何处理的，都可以重新封装这个方法
    //参数：jq:传过来的控件    status:传过来的页面状态

    beforeSave: function (jq, status) {
    },
    examineAfter: function (jq, status, callback) { callback(); },

    //传过来两个参数，status：状态，isSuccess：返回的信息
    afterSave: function (status, isSuccess) {
    },
    //恢复
    recover: function () {
        var opts = $(this).linkbutton('options');
        if (!$.view.judgePageCancleState(opts.docState)) {
            $.messager.alert('提示', '该单是恢复状态,不能恢复处理')
            return;
        }
        var parms = $(this).linkbutton('getParms', 'recover');
        if (!parms && parms.length != 2) return;
        $('#' + parms[0]).val(parms[1]);
        $(this).linkbutton('saveStatus');
        $(this).linkbutton('setWholeStatus');
        $.view.curPageState = 6;
    },
    //废除
    abandoned: function () {

        var opts = $(this).linkbutton('options');
        if ($.view.judgePageCancleState(opts.docState)) {
            $.messager.alert('提示', '该单据已经作废,操作无效')
            return;
        }
        var button = $(this);
        var guid = $.view.getKeyGuid(opts.scope);
        if (!guid) return;
        $.ajax({
            url: '/Home/AbandonedDoc',
            data: { "guid": guid, "scope": opts.scope },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {
                if (data.IsSuccess) {
                    $.messager.alert('提示', '在流程中的单据,不能作废')
                    return
                }
                var parms = button.linkbutton('getParms', 'abandoned');
                if (!parms && parms.length != 2) return;
                $('#' + parms[0]).val(parms[1]);
                button.linkbutton('saveStatus');
                button.linkbutton('setWholeStatus');
                $.view.curPageState = 5;
            }
        });
    },
    submitProcessAfter: function (dataguid, scope) {
        var data = $.view.retrieveDoc(dataguid, scope);
        if (data) {
            $.view.loadData(scope, data);
        }
        $.view.cancelObj = { data: data, status: 4 }
    },
    //提交流程
    submitProcess: function () {
        var opts = $(this).linkbutton('options');
        if ($.view.judgePageCancleState(opts.docState)) {
            $.messager.alert('提示', '该单据已经作废,操作无效')
            return;
        }
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
    //会计凭证(历史)  --zzp 2014-04-03 10:31
    kjpzls: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        var parms = $(this).linkbutton('getParms', 'kjpzls');
        if (!parms && parms.length != 4) return;
        var url = parms[0];
        var winId = '#' + opts.window;
        $(winId).dialog({
            title: '历史',
            width: 1000,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: url,
            onLoad: function (c) {
                $.view.setViewEditStatus(parms[3], pageState);
            }
        });
    },
    //--zzp 2014-03-19
    //单据(选单)
    dj: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        var parms = $(this).linkbutton('getParms', 'dj');
        if (!parms && parms.length != 4) return;
        var url = parms[0];
        var winId = '#' + opts.window;
        $(winId).dialog({
            title: '单据',
            width: 1000,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: url,
            onLoad: function (c) {
                $.view.setViewEditStatus(parms[3], pageState);
            }
        });
    },
    //明细
    particulars: function () {
        var parms = $(this).linkbutton('getParms', 'particulars');
        var opts = $(this).linkbutton('options');
        var tempFunName = 'particularsDetail';
        if (opts.sourceBody) {
            tempFunName = 'particularsDetail1';
        }
        var fun = $.fn.linkbutton.methods[tempFunName];

        fun(parms, opts.window);
        // $(this).linkbutton('particularsDetail',parms);
    },
    detailInsertRow: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        //拿到明细对象
        var detailObj = $.view.detailObj;
        if (detailObj.cur == 0)//删除最后一条的情况
            return;
        //当前页面的数据




        var dataCur = $.view.retrieveData(opts.scope, '', true);
        //将当前的数据保存
        if (detailObj.cur != 0)//删除最后一条的情况
            detailObj.data.r[detailObj.cur - 1] = dataCur.m;
        var defaultData = detailObj.defaultData; //找到储存的默认数据




        var rIndex = detailObj.data.r.length;
        detailObj.numObj.v = detailObj.cur;
        //默认数据中索引数据+1
        var loadData = defaultData;
        loadData.push(detailObj.numObj);
        //保存默认数据到明细储存对象中
        detailObj.data.r.splice(detailObj.cur - 1, 0, defaultData);
        //将当前数据的索引不变
        $.view.clearView(opts.scope, "dataregion");
        //页面load数据
        $.view.loadData(opts.scope, { m: loadData });
        //改变状态




        $.view.setViewEditStatus(opts.scope, pageState);
    },
    detailAddRow: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        //当前页面的数据




        var dataCur = $.view.retrieveData(opts.scope, '', true);
        //拿到明细对象
        var detailObj = $.view.detailObj;
        //将当前的数据保存
        if (detailObj.cur != 0)//删除最后一条的情况
            detailObj.data.r[detailObj.cur - 1] = dataCur.m;
        var defaultData = detailObj.defaultData; //找到储存的默认数据




        var rIndex = detailObj.data.r.length;
        //默认数据中索引数据+1
        detailObj.numObj.v = rIndex + 1;
        var loadData = rIndex > 0 ? $(this).linkbutton('copyDetailDefaultValue', detailObj.data.r[rIndex - 1]) : defaultData;
        loadData.push(detailObj.numObj);
        //保存默认数据到明细储存对象中
        detailObj.data.r.push(loadData);
        detailObj.cur = detailObj.cur + 1;
        $.view.clearView(opts.scope, "dataregion");
        $.view.loadData(opts.scope, { m: loadData });
        $.view.setViewEditStatus(opts.scope, pageState);
    },
    //拷贝明细默认值




    copyDetailDefaultValue: function (jq, preData) {
        var winOpts = $('#b-window').dialog('options');
        var gridId = winOpts.gridId;
        var scope = gridId.split('-')[0];
        var gridOpts = $('#' + gridId).edatagrid('options');
        var copyCols = gridOpts.copyField;
        var resultData = [];
        for (var i = 0, j = preData.length; i < j; i++) {
            var row = preData[i];
            var colName = scope + '-' + row.m + '-' + row.n;
            if (copyCols.exist(colName)) {
                resultData.push(row);
            }
        }
        return resultData;
    },
    detailDeleteRow: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        //拿到明细对象
        var detailObj = $.view.detailObj;
        //将当前的数据保存
        if (detailObj.cur == 0)//删除最后一条的情况
            return;
        var attDel = detailObj.data.r.splice(detailObj.cur - 1, 1);
        //找到删除的guid保存
        var guid = $.view.getKeyGuid(opts.scope);
        if (guid) {
            detailObj.data.ad.push(guid);
        }
        //记录当前删除的GUID ?????????????
        var l = detailObj.data.r.length;
        if (l == 0) {
            detailObj.cur = 0;
            detailObj.numObj.v = 0;
            var loadData = [];
            //将序号添加都展示数组中




            loadData.push(detailObj.numObj);
            //清空数据
            $.view.clearView(opts.scope, "dataregion");
            //页面load数据
            $.view.loadData(opts.scope, { m: loadData });
            //改变状态




            $.view.setViewEditStatus(opts.scope, pageState);
            return;
        }
        //将当前数据的索引设置为1
        detailObj.cur = 1;
        detailObj.numObj.v = 1;
        var loadData = detailObj.data.r[0];
        //将序号添加都展示数组中





        loadData.push(detailObj.numObj);
        //清空数据
        $.view.clearView(opts.scope, "dataregion");
        //页面load数据
        $.view.loadData(opts.scope, { m: loadData });
        //改变状态





        $.view.setViewEditStatus(opts.scope, pageState);
    },
    detailUpRow: function () { //clearView
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        //获取当前行 
        var dataCur = $.view.retrieveData(opts.scope, '', true);
        //拿到明细对象
        var detailObj = $.view.detailObj;
        var curIndex = detailObj.cur;
        if (curIndex == 0 || curIndex == 1) return; //删除获取 最前一条返回




        // 保存当前行的数据
        detailObj.data.r[detailObj.cur - 1] = dataCur.m;
        //拿到数据集合
        var data = detailObj.data.r;
        //当前索引减1
        detailObj.cur = curIndex - 1;
        //修改序号
        detailObj.numObj.v = detailObj.cur;
        //找到将要load的数据




        var loadData = data[detailObj.cur - 1];
        //将序号添加都展示数组中




        loadData.push(detailObj.numObj);
        //清空数据
        $.view.clearView(opts.scope, "dataregion");
        //页面load数据
        $.view.loadData(opts.scope, { m: loadData });
        //改变状态




        $.view.setViewEditStatus(opts.scope, pageState);
    },
    detailDownRow: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        //获取当前行 
        var dataCur = $.view.retrieveData(opts.scope, '', true);
        //拿到明细对象
        var detailObj = $.view.detailObj;
        var curIndex = detailObj.cur;
        if (curIndex == 0 || curIndex == detailObj.data.r.length) return; //删除获取 最后一条返回




        // 保存当前行的数据
        detailObj.data.r[detailObj.cur - 1] = dataCur.m;
        //拿到数据集合
        var data = detailObj.data.r;
        //当前索引+1
        detailObj.cur = curIndex + 1;
        //修改序号
        detailObj.numObj.v = detailObj.cur;
        //找到将要load的数据




        var loadData = data[detailObj.cur - 1];
        //将序号添加都展示数组中




        loadData.push(detailObj.numObj);
        //清空数据
        $.view.clearView(opts.scope, "dataregion");
        //页面load数据
        $.view.loadData(opts.scope, { m: loadData });
        //改变状态




        $.view.setViewEditStatus(opts.scope, pageState);
    },
    //点击列表末尾行和按钮的明细 公用方法
    particularsDetail: function (parms, win) {
        if (parms && parms.length >= 3) {
            var url = parms[0];
            var gridId = parms[1];
            var scope = gridId.split('-')[0];
            var pageState = $.view.getStatus(scope);
            var targetscope = parms[2];
            var targetregion = parms[3]; //区域
            if (url && gridId && targetscope) {
                var title = parms[4];
                var width = parms[5];
                var height = parms[6];
                var winId = '#' + win;
                $(winId).dialog({
                    isCancel: true,
                    resizable: false,
                    title: title || '明细',
                    width: width || 900,
                    height: height || 550,
                    gridId: gridId,
                    modal: true,
                    draggable: true,
                    resizable: true,
                    minimizable: false,
                    maximizable: false,
                    collapsible: false,
                    href: url,
                    onLoad: function (c) {

                        //找到grid
                        var grid = $('#' + gridId);
                        //存储对象
                        var detailObj = $.view.detailObj;
                        //找到选中行

                        var rowSelect = grid.datagrid('getSelected'), rowIndex = 1;
                        if (rowSelect) {
                            rowIndex = grid.datagrid('getRowIndex', rowSelect);

                            grid.datagrid('endEdit', rowIndex);
                            grid.edatagrid('selectRow', rowIndex);
                        }
                        //列表默认配置
                        var opts = grid.edatagrid('options');
                        //将所有明细数据存储

                        detailObj.data = grid.edatagrid('getDataDetail');
                        //列表默认值

                        detailObj.defaultData = opts.defaultData ? opts.defaultData.r[0] : [];
                        if (rowSelect) {
                            var row = detailObj.data.r[rowIndex];
                            if (opts.orderNum) {
                                var s = opts.orderNum.split('-');
                                rowIndex = rowIndex + 1;
                                //找到行索引 并且保存序号对象
                                detailObj.numObj = { m: s[1], n: s[2], v: rowIndex };
                                row.push(detailObj.numObj)
                            }
                            $.view.loadData(targetscope, { m: row });
                            $.view.setViewEditStatus(targetscope, pageState);

                        } else {
                            //保存序号对象
                            if (opts.orderNum) {
                                var s = opts.orderNum.split('-');
                                detailObj.numObj = { m: s[1], n: s[2], v: 1 };
                            }
                            var loadData = detailObj.defaultData;
                            if (detailObj.data.r.length >= 0 && detailObj.data.r[0]) {
                                loadData = detailObj.data.r[0];
                            } else {
                                //保存第一行的临时数据
                                detailObj.data.r.push(opts.defaultData);
                            }
                            loadData.push(detailObj.numObj);

                            $.view.loadData(targetscope, { m: loadData });
                            $.view.setViewEditStatus(targetscope, pageState);  //sxh
                        }
                        detailObj.cur = rowIndex; //当前的数据索引

                    }
                });
            }
        }
    },
    //为整合 只为劳务费领款单用




    particularsDetail1: function (parms, winid) {
        if (parms && parms.length >= 3) {
            var url = parms[0];
            var gridId = parms[1];
            var scope = gridId.split('-')[0];
            var pageState = $.view.getStatus(scope);
            var targetscope = parms[2];
            var targetregion = parms[3]; //区域
            if (url && gridId && targetscope) {
                var title = parms[4];
                var width = parms[5];
                var height = parms[6];
                var winId = '#' + winid;
                var date = new Date();
                $(winId).dialog({
                    isCancel: true,
                    resizable: false,
                    curState: pageState,
                    scope: scope,
                    title: title || '明细',
                    width: width || 900,
                    height: height || 550,
                    modal: true,
                    draggable: true,
                    resizable: true,
                    minimizable: false,
                    maximizable: false,
                    collapsible: false,
                    href: url,
                    onLoad: function (c) {
                        //                        alert("明细界面加载时间" + (new Date() - date))
                        date = new Date();
                        var data = $.view.retrieveDataNew(scope, '', true);
                        //                        alert("明细界面获取数据时间" + (new Date() - date))
                        date = new Date();

                        $.view.loadData(targetscope, data, false, true);
                        //                        alert("明细界面加载数据时间" + (new Date() - date))
                        date = new Date();
                        $.view.setViewEditStatus(targetscope, pageState);
                        //                        alert("明细界面修改控件状态数据时间" + (new Date() - date))
                    }
                });
            }
        }
    },
    submitDetail1: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitDetail1');
        if (parms && parms.length >= 1) {
            var targetscope = parms[0];
            var winId = '#' + opts.window;
            //当前页面的数据




            var winOpts = $(winId).dialog('options');
            var data = $.view.retrieveData(opts.scope, '', true);

            $.view.loadData(winOpts.scope, data, true);
            $.view.setViewEditStatus(winOpts.scope, winOpts.curState, true);
            $(winId).dialog('close');
            $(winId).trigger('mxSubmit');
        }
    },
    cancelDetail: function () {

        var opts = $(this).linkbutton('options');
        var winId = '#' + opts.window;
        var winOpts = $(winId).dialog('options');
        winOpts.isCancel = true;
        $(winId).dialog('close');
        $.view.detailObj = {
            data: { r: [], ad: [] },
            defaultData: null,
            numObj: {},
            cur: 1
        };
    },
    submitDetail: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitDetail');
        if (parms && parms.length >= 1) {
            var targetScope = parms[0];
            var winId = '#' + opts.window;
            //当前页面的数据




            var dataCur = $.view.retrieveData(opts.scope, '', true);
            //拿到明细对象
            var detailObj = $.view.detailObj;
            //将当前的数据保存
            if (detailObj.cur != 0)//删除最后一条的情况
                detailObj.data.r[detailObj.cur - 1] = dataCur.m;
            var winOpts = $(winId).window('options');
            gridId = '#' + winOpts.gridId;



            $(gridId).edatagrid('setData', detailObj.data);
            $(winId).dialog('close');
        }
        $.view.detailObj = {
            data: { r: [], ad: [] },
            defaultData: null,
            numObj: {},
            cur: 1
        };
    },
    submitHistory: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitHistory');
        if (parms && parms.length >= 2) {
            var targetScope = parms[1];
            var gridId = '#' + parms[0];
            var winId = '#' + opts.window;
            var selRow = $(gridId).datagrid('getSelected');
            if (!selRow) {
                $.messager.alert('系统提示', '请选择一条数据');
                return;
            }
            $(winId).dialog('close');
            $.view.setStatus(targetScope, 4); //历史确定之前先将页面状态改为4

            $.view.init(targetScope, 4, selRow.GUID);
        }
    },
    //关闭页签
    closeTab: function () {

        var me = this;
        var opts = $(me).linkbutton('options');
        var arr = [1, 2];
        var scope = opts.scope;
        var pageState = $.view.getStatus(scope);
        if (arr.exist(pageState)) {
            $.messager.confirm("提示", "正在编辑,是否退出?", function (data) {
                if (!data) return;
                parent.window.CloseTabs();
            })
        } else {
            parent.window.CloseTabs();
        }
    },

    //历史
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
                        $.view.setViewEditStatus(targetscope, pageState);
                        if (opts.historyBtnId) {
                            $("#" + opts.historyBtnId).click();
                        }
                    }
                });
            }
        }



    },
    historySearch: function () {

        var parms = $(this).linkbutton('getParms', 'historySearch');
        if (parms && parms.length) {
            var url = parms[0];
            var gridId = parms[1];
            var scope = parms[2];
            var region = parms[3];
            var fun = $.fn.linkbutton.methods["getHistoryByFilter"];

            fun(url, gridId, scope, region);
        }
    },
    getHistoryByFilter: function (url, gridId, scope, region, tcondition) {
        MaskUtil.mask();
        var data = $.view.retrieveData(scope, region);
        var url;
        var result = {};
        if (data && data.m) {
            for (var i = 0, j = data.m.length; i < j; i++) {
                var temp = data.m[i];
                if (!temp && !temp.v) continue;
                result[temp.n] = temp.v;
            }
        }
        result = $.extend(result, tcondition || {});

        if (url) {
            $.ajax({
                url: url,
                data: { condition: JSON.stringify(result) },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    MaskUtil.unmask();
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {
                    MaskUtil.unmask();

                    if (data.errorcode != undefined && data.errorcode != "") {
                        $.messager.alert("错误", data.errorcode, 'error');
                    }
                    else {

                        //修改通用的历史查询脚本，当查询以后将页码修改成第一页  sxh 2014-04-03
                        $('#' + gridId).datagrid('getPager').pagination('options').pageNumber = 1;

                        $('#' + gridId).datagrid('loadData', data);
                    }
                }
            });
        } else {
            MaskUtil.unmask();
        }
    },
    //单据打印公共方法
    print: function () {
        //重新获取数据，
        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'print');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        //var data = $.view.retrieveDoc(guid, scope);
        //if (!data || !data.m) return;
        debugger
        var ids = parms[1];
        var urlArg = "";
        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i].split('-');
                var text = $('#' + ids[i]).val();
                if (id[2] == "moneychinese") {
                    if ($('#' + ids[i]).attr('style').indexOf('red') >= 0) {
                        text = "负" + text;
                    }
                } else {
                    if ($('#' + ids[i]).attr('style').indexOf('red') >= 0) {
                        text = "-" + text;
                    }
                }

                //data.m.push({ m: id[1], n: id[2], v: text });
                urlArg += "&" + id[2] + "=" + text;
            }
            urlArg += "&cn=" + ids[0] + "&number=" + ids[1];
        }
        // 动态匹配打印模板
        var tempScope = parms[0].split("/")[2]; //获取单据类型
        //var url = parms[0] + "?pturl=" + tempScope + "print";
        var url = parms[0] + "?guid=" + guid + urlArg;
        window.open(url);
        //因为打印有问题 暂时屏蔽掉
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
        //        //动态匹配打印模板


        //        var tempScope = parms[0].split("/")[2]; //获取单据类型
        //        var url = parms[0] + "?pturl=" + tempScope + "print";

        //        $("#printIframe").jqprintEx(url, data);
    },
    //打印浏览
    printView: function () {

        //重新获取数据，
        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'print');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        //var data = $.view.retrieveDoc(guid, scope);
        //if (!data || !data.m) return;
        var ids = parms[1];
        var urlArg = "";
        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i].split('-');
                var text = $('#' + ids[i]).val();
                if (id[2] == "moneychinese") {
                    if ($('#' + ids[i]).attr('style').indexOf('red') >= 0) {
                        text = "负" + text;
                    }
                } else {
                    if ($('#' + ids[i]).attr('style').indexOf('red') >= 0) {
                        text = "-" + text;
                    }
                }
                urlArg += "&" + id[2] + "=" + text;
            }
            urlArg += "&cn=" + ids[0] + "&number=" + ids[1];
        }
        //动态匹配打印模板
        var tempScope = parms[0].split("/")[2]; //获取单据类型
        //var url = parms[0] + "?pturl=" + tempScope + "print";
        var url = parms[0] + "?guid=" + guid + urlArg;
        window.open(url);
    },
    //报表打印公共方法
    PrintReport: function () {
        //拿到打印按钮的选项
        var opts = $(this).linkbutton('options');
        //获取到报表的scope
        var scope = opts.scope;
        //获取到打印按钮中要传入模板中的参数




        var parms = $(this).linkbutton('getParms', 'PrintReport');
        if (!parms || !parms.length) return;
        //   $.reportInit(parms);
    },
    //转换当前可编辑状态




    alterStatus: function (jq, status) {
        return jq.each(function () {
            if (!status) return;
            var opts = $(this).linkbutton('options');
            var forbidArr = opts.forbidstatus;
            if ($.view.curPageState == 5 || $.view.curPageState == 6) {
                status1 = $.view.curPageState;
                status = 4;
                if (forbidArr && (forbidArr.exist(5) || forbidArr.exist(6))) {
                    forbidArr.exist(status1) ? $(this).linkbutton('disabled') : $(this).linkbutton('enabled');
                    return;
                }
            }
            if (forbidArr && (forbidArr.exist(-1) || forbidArr.exist(status))) {
                $(this).linkbutton('disabled');
            }
            else {
                $(this).linkbutton('enabled');
            }
        });
    },
    bind: function (jq, bind) {
        return jq.each(function () {
            var opts = $(this).linkbutton('options');
            var bindmethod = opts.bindmethod;
            if (bindmethod) {
                for (var event in bindmethod) {
                    var methods = bindmethod[event];
                    if (methods) {
                        for (var i = 0, j = methods.length; i < j; i++) {
                            var mt = methods[i];
                            if (!mt || mt == 'bind') continue;
                            var m = $.fn.linkbutton.methods[mt];
                            if (m) {
                                if (opts.hasOwnProperty(event)) {
                                    var pk = {};
                                    pk[event] = null;
                                    if (bind) {
                                        pk[event] = m;
                                    }
                                    $(this).combogrid(pk);
                                }
                                else {
                                    $(this).unbind(event, m);
                                    if (bind) {
                                        $(this).bind(event, m);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });
    },
    saveStatus: function (jq) {
        var opts = $(jq).linkbutton('options');
        if (opts.status == "3") {
            var status = $.view.getStatus(opts.scope);
            opts.preStatus = opts.status;
        }
        $.view.setStatus(opts.scope, opts.status);
        $.view.curPageState = opts.status;
    },
    getStatus: function (jq) {
        var opts = $(jq).linkbutton('options');
        return $.view.getStatus(opts.scope);
    },
    setWholeStatus: function (jq) {
        var opts = $(jq).linkbutton('options');
        if (!opts.scope) opts.scope = $(jq).attr('id').split("-")[0];
        $("[id^='" + opts.scope + "'].easyui-linkbutton").linkbutton('alterStatus', opts.status);
        $("[id^='" + opts.scope + "'].easyui-datebox").datebox('alterStatus', opts.status);
        $("[id^='" + opts.scope + "'].easyui-combogrid").combogrid('alterStatus', opts.status);
        $("[id^='" + opts.scope + "'].easyui-combobox").combobox('alterStatus', opts.status); //新增combobox修改状态功能 zzp 2014/04/24 11:33
        $("[id^='" + opts.scope + "'].easyui-checkbox").checkbox('alterStatus', opts.status); //新增checkbox修改功能   zzp 2014/04/25 10:50
        $("[id^='" + opts.scope + "'].easyui-validatebox").validatebox('alterStatus', opts.status);
        $("[id^='" + opts.scope + "'].easyui-numberbox").numberbox('alterStatus', opts.status);
        $("[id^='" + opts.scope + "'].easyui-edatagrid").edatagrid('alterStatus', opts.status);
        $("[id^='" + opts.scope + "'].easyui-tree").tree('alterStatus', opts.status);
    },
    disabled: function (jq) {
        $(jq).linkbutton({ 'disabled': true });
        this.bind(jq, false);
    },
    enabled: function (jq) {
        $(jq).linkbutton({ 'disabled': false });
        this.bind(jq, true);
    },
    getParms: function (jq, methodName) {
        var opts = $(jq).linkbutton('options'), funConfig = opts.bindparms;
        if (funConfig && funConfig[methodName]) {
            return funConfig[methodName];
        }
        return null;
    },
    //取消单据
    cancel: function () {

        var me = this;
        var cancel = $.view.cancelObj;
        if (!cancel.data) cancel.data = [];
        $.messager.confirm("提示", "正在编辑,是否取消?", function (data) {
            if (!data) {
                return;
            } else {
                var opts = $(me).linkbutton('options');
                $.view.clearView(opts.scope);
                $.view.loadData(opts.scope, cancel.data);
                $.view.setViewEditStatus(opts.scope, cancel.status);
                $.fn.linkbutton.methods.cancelAfter.call(this, cancel.status, cancel.data);
            }
        })
    },
    cancelAfter: function (status, data) { },
    //校验单据字段信息
    examine: function (isSave) {

        var opts = $(this).linkbutton('options');
        //获取单据类型
        var scope = opts.scope;
        //获取当前单据状态值



        var pageState = $.view.getStatus(scope);
        if (!(pageState == "1" || pageState == "2")) return true;
        var parms = $(this).linkbutton('getParms', 'windowExamine');
        if (parms) {
            for (var colType in parms) {
                var control = parms[colType];
                var temp = $('#' + control.source).combogrid('getValue');
                var opts1 = $('#' + control.target).linkbutton('options');
                if (temp == "") {
                    opts1.IsExist = false;
                } else {
                    opts1.IsExist = true;
                }
                var msgtemp = $.fn[colType].methods['verifyData'].call($('#' + control.source));
                if (msgtemp) {
                    $('#' + control.target).click();
                    return;
                }
            }
        }

        var parms = $(this).linkbutton('getParms', 'examine');
        if (parms) {
            var msgError1 = "";
            for (var colType in parms) {
                var controls = parms[colType];
                for (var i = 0, j = controls.length; i < j; i++) {
                    var id = "#" + controls[i];
                    var fn = $.fn[colType].methods['verifyData'];
                    if (!fn) continue;
                    var msgtemp = fn.call($(id));
                    if (msgtemp)
                        msgError1 += msgtemp + "</br>";
                }
            }
        }
        if (msgError1.length != 0) {
            $.messager.alert('提示', msgError1);
            return false;
        }
        if (isSave != true) {
            $.messager.alert('提示', "校验成功！");
            return;
        }
        //串联提示
        var stateArrs = [1, 2];
        if (!stateArrs.exist($.view.curPageState)) return true;
        var parms = $(this).linkbutton('getParms', 'bubbleExamine');
        if (parms) {
            var msgError = "";
            for (var colType in parms) {
                var controls = parms[colType];
                for (var i = 0, j = controls.length; i < j; i++) {
                    var id = "#" + controls[i];
                    var fn = $.fn[colType].methods['verifyData'];
                    if (!fn) continue;
                    var msgtemp = fn.call($(id));
                    if (msgtemp.length > 0) {
                        msgError += msgtemp + "</br>";
                    }
                }
            }
            if (msgError.length != 0) {
                return msgError;
            }
        }
        return true;
    },

    //获得格式数据
    getData: function (jq) { return; },
    //加载格式数据
    setData: function (jq, data) { }
});
$.extend($.fn.linkbutton.defaults, {
    actionUrl: null, //{addurl:'',deleteurl:'',modifyurl:''}
    scope: null,
    status: null,
    forbidstatus: null,
    bindmethod: null, //{ 'click': ['setStatus'] }
    bindparms: null //{'history':['url'],'delRow':['gridId'],'addRow':['gridId']}
});