$.extend($.fn.linkbutton.methods, {
    newDoc: function () {
        $.view.setStatus('djlb', 1);
        $.view.setViewEditStatus('djlb', 1);
    },
    export: function () {
        if (window.through) {
            window.open("/djlb/Export/");
            return;
        }
        var parms = $(this).linkbutton('getParms', 'djlbSearch');

        if (parms && parms.length) {
            var url = parms[0];
            var gridId = parms[1];
            var scope = parms[2];
            var region = parms[3], tcondition;
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
                        window.open("/djlb/Export/");
                    }
                });
            }
            else {
                MaskUtil.unmask();
            }
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
                !opts.closeWin ? $('#b-ChildWindow').dialog('close') : parent.window.CloseTabs();
            })
        } else {
            !opts.closeWin ? $('#b-ChildWindow').dialog('close') : parent.window.CloseTabs();
        }
    },
    checkTree: function (jq, parms) {
        var trees = parms.trees;
        var funName = parms.check ? 'check' : 'uncheck';
        for (var i = 0, j = trees.length; i < j; i++) {
            var id = '#' + trees[i];
            var roots = $(id).tree('getRoots');
            for (var h = 0; h < roots.length; h++) {
                var target = roots[h].target;
                $(id).tree(funName, target)
            }
        }
    },
    Submitfilter: function () {

        var opts = $(this).linkbutton('options');
        var treeCondition = [];
        //生命数组用来记录子窗体要向父窗体传值的参数
        var passDjlbVal = ['djlb-list-DocNum', 'djlb-list-StartDate', 'djlb-list-EndDate', 'djlb-list-IsShowDetail'];
        var passFilterVal = ['filter-list-IsShowDetail'];
        var trees = ['filter-tree-filterper', 'filter-tree-filtercode', 'filter-tree-filterproject', 'filter-tree-filterfunction'];
        var region = 'filterdatafilter';
        var guids = [];

        for (var i = 0, j = trees.length; i < j; i++) {
            guids = [];
            //去树上自己配置模型
            var tempTree = { treeModel: '', treeValue: '' };
            tempTree.treeModel = $('#' + trees[i]).attr('model');
            var nodes = $('#' + trees[i]).tree('getChecked');
            for (var m = 0, n = nodes.length; m < n; m++) {
                var node = nodes[m];
                //判断是否是叶子节点
                if ($('#' + trees[i]).tree('isLeaf', node.target)) {
                    //带不带引号
                    guids.push(node.attributes["GUID"]);
                }
            }
            if (guids.length > 0) {
                tempTree.treeValue = guids.join(',');
                treeCondition.push(tempTree);
            }
        }
        //-------------
        var nodes = $('#filter-tree-filterdoctype').tree('getChecked');
        var o1 = { treeModel: 'SS_DocType', treeValue: '' };
        var o2 = { treeModel: '', treeValue: '' };
        var guidO1 = [], gridO2 = [];
        for (var m = 0, n = nodes.length; m < n; m++) {
            var node = nodes[m];
            //判断是否是叶子节点
            if ($('#filter-tree-filterdoctype').tree('isLeaf', node.target)) {
                //带不带引号
                var guid = node.attributes["GUID"];
                guidO1.push(guid);
                var pm = node.attributes["pm"];
                if (pm) {
                    var arr1 = pm.split('#');
                    if (!o2.treeModel) {
                        o2.treeModel = arr1[0];
                    }
                    if (!gridO2.exist(arr1[1])) {
                        gridO2.push(arr1[1]);
                    }
                }

            }
        }
        o1.treeValue = guidO1.join(',');
        o2.treeValue = gridO2.join(',');
        if (!!o1.treeValue.length) treeCondition.push(o1);
        if (!!o2.treeValue.length) treeCondition.push(o2);
        //---------------
        var data = $.view.retrieveData(opts.scope, region);
        var url = '/djlb/HistoryFilter/';
        var result = {};
        if (data && data.m) {
            for (var i = 0, j = data.m.length; i < j; i++) {
                var temp = data.m[i];
                if (!temp && !temp.v) continue;
                result[temp.n] = temp.v;
            }
        }

        //将过滤窗体中的查询参数值付给父窗体中相对应的查询参数
        if (!result) return;
        $('#' + passDjlbVal[0]).numberbox('setValue', result.DocNum);
        $('#' + passDjlbVal[1]).datebox('setValue', result.StartDate);
        $('#' + passDjlbVal[2]).datebox('setValue', result.EndDate);

        var cheVal = $('#' + passFilterVal[0]).checkbox('getValue');
        if (cheVal) {
            $('#' + passDjlbVal[3]).attr('checked', true);
        } else {
            $('#' + passDjlbVal[3]).attr('checked', false);
        }

        //treeConditions 后台名称匹配的
        result = $.extend(result, { TreeNodeList: treeCondition });


        if (url) {
            $.ajax({
                url: url,
                data: { condition: JSON.stringify(result) },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {

                    //$('#djlb-list').find('thead tr th:last')
                    if (result.IsShowDetail) {
                        $('#djlb-list').datagrid('showColumn', 'BGCodeName');
                        $('#djlb-list').datagrid('showColumn', 'BGCodeKey');
                    } else {
                        $('#djlb-list').datagrid('hideColumn', 'BGCodeName');
                        $('#djlb-list').datagrid('hideColumn', 'BGCodeKey');
                    }
                    $('#djlb-list').datagrid('loadData', data);
                    $('#b-window').dialog('close');
                }
            });
        }
    },
    //查询
    djlbSearch: function () {

        var parms = $(this).linkbutton('getParms', 'djlbSearch');

        if (parms && parms.length) {
            var url = parms[0];
            var gridId = parms[1];
            var scope = parms[2];
            var region = parms[3];
            var fun = $.fn.linkbutton.methods["getDjlbByFilter"];
            fun(url, gridId, scope, region);
        }

    },
    getDjlbByFilter: function (url, gridId, scope, region, tcondition) {

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

                    if (result.IsShowDetail) {
                        $('#djlb-list').datagrid('showColumn', 'BGCodeName');
                        $('#djlb-list').datagrid('showColumn', 'BGCodeKey');
                    } else {
                        $('#djlb-list').datagrid('hideColumn', 'BGCodeName');
                        $('#djlb-list').datagrid('hideColumn', 'BGCodeKey');
                    }
                    //单据列表当查询的时候默认将页码设置成第一页的数据  sxh 2014-03-20
                    $('#' + gridId).datagrid('getPager').pagination('options').pageNumber = 1;
                    $('#' + gridId).datagrid('loadData', data);
                    MaskUtil.unmask();
                }
            });
        }
        else {
            MaskUtil.unmask();
        }
    },

    //过滤
    filter: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'filter');
        var PassComboVal = ['djlb-list-ApproveStatus', 'djlb-list-CheckStatus', 'djlb-list-WithdrawStatus', 'djlb-list-PayStatus', 'djlb-list-CertificateStatus'];
        var PassDjlbVal = ['djlb-list-IsShowDetail', 'djlb-list-StartDate', 'djlb-list-EndDate', 'djlb-list-DocNum'];
        var PassFilterVal = ['filter-list-IsShowDetail', 'filter-list-StartDate', 'filter-list-EndDate', 'filter-list-DocNum'];
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
                    title: title || '过滤',
                    width: width || 1000,
                    height: height || 600,
                    modal: true,
                    minimizable: false,
                    maximizable: false,
                    collapsible: false,
                    href: url,
                    onLoad: function (c) {
                        if (PassComboVal) {
                            var val0 = $('#' + PassComboVal[0]).combobox('getValue');
                            var val1 = $('#' + PassComboVal[1]).combobox('getValue');
                            var val2 = $('#' + PassComboVal[2]).combobox('getValue');
                            var val3 = $('#' + PassComboVal[3]).combobox('getValue');
                            var val4 = $('#' + PassComboVal[4]).combobox('getValue');
                            switch (val0) {
                                case "0":
                                    var stor = ['filter-1', 'filter-4', 'filter-7', 'filter-10'];
                                    for (var i = 0, j = stor.length; i < j; i++) {
                                        $('#' + stor[i]).attr('checked', true);
                                    }
                                    break;
                                case "1": $('#filter-4').attr('checked', true); break;
                                case "2": $('#filter-7').attr('checked', true); break;
                                case "3": $('#filter-7').attr('checked', true); break;
                                default: break
                            }
                            switch (val1) {
                                case "0":
                                    var stor = ['filter-2', 'filter-5', 'filter-8'];
                                    for (var i = 0, j = stor.length; i < j; i++) {
                                        $('#' + stor[i]).attr('checked', true);
                                    }
                                    break;
                                case "1": $('#filter-5').attr('checked', true); break;
                                case "2": $('#filter-8').attr('checked', true); break;
                                default: break
                            }
                            switch (val2) {
                                case "0":
                                    var stor = ['filter-3', 'filter-6', 'filter-9'];
                                    for (var i = 0, j = stor.length; i < j; i++) {
                                        $('#' + stor[i]).attr('checked', true);
                                    }
                                    break;
                                case "1": $('#filter-6').attr('checked', true); break;
                                case "2": $('#filter-9').attr('checked', true); break;
                                default: break
                            }
                            switch (val3) {
                                case "0":
                                    var stor = ['filter-11', 'filter-13', 'filter-16'];
                                    for (var i = 0, j = stor.length; i < j; i++) {
                                        $('#' + stor[i]).attr('checked', true);
                                    }
                                    break;
                                case "1": $('#filter-13').attr('checked', true); break;
                                case "2": $('#filter-16').attr('checked', true); break;
                                default: break
                            }
                            switch (val4) {
                                case "0":
                                    var stor = ['filter-12', 'filter-14', 'filter-17'];
                                    for (var i = 0, j = stor.length; i < j; i++) {
                                        $('#' + stor[i]).attr('checked', true);
                                    }
                                    break;
                                case "1": $('#filter-14').attr('checked', true); break;
                                case "2": $('#filter-17').attr('checked', true); break;
                                default: break
                            }

                        }
                        if (PassDjlbVal) {
                            var val0 = $('#' + PassDjlbVal[0]).checkbox('getValue');
                            var val1 = $('#' + PassDjlbVal[1]).datebox('getValue');
                            var val2 = $('#' + PassDjlbVal[2]).datebox('getValue');
                            var val3 = $('#' + PassDjlbVal[3]).numberbox('getValue');

                            if (val0) {
                                $('#' + PassFilterVal[0]).attr('checked', true);
                            } else {
                                $('#' + PassFilterVal[0]).attr('checked', false);
                            }

                            $('#' + PassFilterVal[1]).datebox('setValue', val1);
                            $('#' + PassFilterVal[2]).datebox('setValue', val2);
                            $('#' + PassFilterVal[3]).numberbox('setValue', val3);
                        }
                        $.view.setViewEditStatus(targetscope, 4);
                    }
                });
            }
        }
    },
    //删除
    deleteRow: function () {
        //获取到当前按钮的options属性
        var opts = $(this).linkbutton('options');
        //获取datagrid的id
        var gridId = '#' + opts.scope + '-list';
        //获取选中行数据

        var selRow = $(gridId).datagrid('getSelected');
        if (!selRow && selRow.length != 1) {
            $.messager.alert('系统提示', '请一条单据进行删除！');
            return;
        }
        var CancelStatus = selRow.CancelStatus;
        if (CancelStatus == "已作废") {
            $.messager.alert('提示', '该单据已经作废，不可删除！');
            return;
        }
        var SubmitNotApprove = selRow.SubmitNotApprove;
        if (SubmitNotApprove == "未审批") {
            $.messager.alert('提示', '该单据已进入流程，您无当前节点修改权限！');
            return;
        }
        var CheckStatus = selRow.CheckStatus;
        if (CheckStatus == "已领取") {
            $.messager.alert('提示', '该单据已领取，您无权限不能删除！');
            return;
        }
        var WithdrawStatus = selRow.WithdrawStatus;
        if (WithdrawStatus == "已提现") {
            $.messager.alert('提示', '该单据已经提现不能再进行此操作！');
            return;
        }
        var PayStatus = selRow.PayStatus;
        if (PayStatus == "已付款") {
            $.messager.alert('提示', '该单据已经付款，不能再进行此操作！');
            return;
        }

        var ApproveStatus = selRow.ApproveStatus;
        if (ApproveStatus == "审核中" || ApproveStatus == "已审核") {
            $.messager.alert('提示', '该单据已审核或正在审核，不能再进行此操作！');
            return;
        }
        var rowIndex = $(gridId).datagrid('getRowIndex', selRow);
        //定义模型变量，用来存放选中行的guid和模型名称
        var data = [{ m: '', n: 'GUID', v: ''}], url = '/{0}/Save';

        data[0].m = selRow.ModelName;
        data[0].v = selRow.GUID;
        url = $.format(url, selRow.DocTypeUrl);
        $.messager.confirm("提示", "确定要删除吗?", function (event) {

            if (!event) return;
            $.ajax({
                url: url,
                data: { "status": 3, "m": JSON.stringify(data), "d": "" },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {

                    $(gridId).datagrid('deleteRow', rowIndex);
                }
            });
        });
    },
    DJLBSelectAll: function () {
        var trees = [];
        var tree = $('#tabsId').tabs('getSelected');
        trees.push($(tree.find('.tree')).attr('id'));
        $(this).linkbutton('checkTree', { trees: trees, check: true });
    },
    DJLBUnSelectAll: function () {
        //        var trees = ['filter-tree-filterper', 'filter-tree-filtercode', 'filter-tree-filterproject', 'filter-tree-filterfunction', 'filter-tree-filterdoctype'];
        var trees = [];
        var tree = $('#tabsId').tabs('getSelected');
        trees.push($(tree.find('.tree')).attr('id'));
        $(this).linkbutton('checkTree', { trees: trees, check: false });
    },
    //反核销
    djlbFHX: function () {
        var selRow = $('#djlb-list').datagrid('getSelected');
        if (selRow.YWTypeName == "收付款管理") {
            return;
        }
        if (!selRow && selRow.length != 1) {
            $.messager.alert('系统提示', '请一条单据进行核销！');
            return;
        }
        $.ajax({
            url: '/djlb/FHX',
            data: { "billId": selRow.GUID, "DocTypeKey": selRow.DocTypeKey },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {
                $.messager.alert("提示", "反核销成功！");
            }
        });

    },
    djlbUpdate: function () {

        //获取到当前按钮的options属性
        var opts = $(this).linkbutton('options');
        //获取datagrid的id
        var gridId = '#' + opts.scope + '-list';
        //获取选中行数据

        var selRow = $(gridId).datagrid('getSelected');
        if (!selRow) {
            $.messager.alert('系统提示', '请选择单据！');
            return;
        }
        //拿到单据类型
        var scope = selRow.DocTypeUrl;
        //拿到单据的GUID
        var guid = selRow.GUID;
        //拿到单据状态
        var status = opts.status;
        var title = selRow['DocTypeName'];
        var scopePage = "Index";
        if (scope == "xjtq") { //现金提取跳转的页面与其他页面不同
            scopePage = "XJTQ";
        }
        window.parent.openPageTabs(title, scopePage, scope, guid, status);

        //        var GetUrl = function (scope, guid, status) {
        //            var url = "";
        //            if (scope != undefined && scope.length >= 0 && scope != "") {
        //                var url = "/" + scope + "?scope=" + scope + "";
        //            }
        //            else {
        //                return "";
        //            }
        //            if (guid) {
        //                if (url.indexOf("?") >= 0) {
        //                    url += "&guid=" + guid;
        //                }
        //                else {
        //                    url += "?guid=" + guid;
        //                }

        //            }
        //            if (status) {
        //                if (url.indexOf("?") >= 0) {
        //                    url += "&status=" + status;
        //                }
        //                else {
        //                    url += "?status=" + status;
        //                }
        //            }
        //            return url;
        //        }        

    }
})

$.extend($.fn.checkbox.methods, {
    //给总的
    setValueAndAssociat: function (check) {
        
        var parms = $(this).checkbox('getParms', 'setValueAndAssociat');
        var setValControls = parms.GroupControl;
        for (var i = 0; i < setValControls.length; i++) {
            var col = setValControls[i];
            $('#' + col).checkbox('setValue', true);
        }
        var setV = parms.target;
        $('#' + setV).validatebox('setValue', 0);
        $(this).checkbox('setValue', true);
    },
    //
    setValueAndAssociat1: function (check) {
        var parms = $(this).checkbox('getParms', 'setValueAndAssociat1');
        var setGroupControls = parms.setControlsGroup;
        var main = parms.GroupID;
        var target = parms.target;
        var falg = true;
        var vals = [];
        for (var i = 0; i < setGroupControls.length; i++) {
            var control = setGroupControls[i];
            var val = $('#' + control).checkbox('getValue');
            if (!val) {
                falg = false;
            } else {
                vals.push($('#' + control).attr('val'));
            }
        }
        $('#' + main).checkbox('setValue', falg);
        if (vals.length == 0) {
            $(this).checkbox('setValue', true);
            $('#' + target).validatebox('setValue', $(this).attr('val'));
        } else {
            $('#' + target).validatebox('setValue', falg || vals.length == 0 ? 0 : vals.join(','));
        }
    },
    //互斥选择
    setValueAndAssociat2: function (check) {
        
        var parms = $(this).checkbox('getParms', 'setValueAndAssociat2');
        var setGroupControls = parms.setControlsGroup;
        var main = parms.GroupID;
        var target = parms.target;
        var falg = true;
        var valMain;
        for (var i = 0; i < setGroupControls.length; i++) {
            var control = setGroupControls[i];
            var val = $('#' + control).checkbox('setValue', !check);
            valMain = $('#' + control).attr('val');
        }
        $('#' + main).checkbox('setValue', false);

        $('#' + target).validatebox('setValue', check ? $(this).attr('val') : valMain);
    }
})

