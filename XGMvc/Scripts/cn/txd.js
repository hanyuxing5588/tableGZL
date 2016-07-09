
$.extend($.fn.linkbutton.methods, {
    //需求
    xuqiu: function () {

        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        var parms = $(this).linkbutton('getParms', 'xuqiu');
        if (!parms && parms.length != 4) return;
        var url = parms[0];
        var winId = '#' + opts.window;
        $(winId).dialog({
            title: '需求',
            width: 1000,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: url,
            onLoad: function (c) {

                $.view.setViewEditStatus('Demand', pageState);
//                $('#Demand-Demand-YWType').combobox('setValue', '00000000-0000-0000-0000-000000000000');
//                $('#Demand-Demand-GUID_DocType').combobox('setValue', '00000000-0000-0000-0000-000000000000');
            }
        });
    },
    submitDemand: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitDemand');
        if (parms && parms.length >= 2) {
            var targetScope = parms[1];
            var gridId = '#' + parms[0];
            var winId = '#' + opts.window;
            var selRow = $(gridId).datagrid('getChecked');
            if (!selRow || selRow.length == 0) {
                $.messager.alert('系统提示', '请选择一条数据');
                return;
            }
            $(winId).dialog('close');
            $.view.setStatus(targetScope, 4); //历史确定之前先将页面状态改为4

            var arrId = []; //待优化成后台一次请求
            for (var i = 0, j = selRow.length; i < j; i++) {
                var objId = {};
                objId.GUID = selRow[i].GUID;
                objId.YWTypeKey = selRow[i].YWTypeKey;
                arrId.push(objId);
            }
            ;
            var dataguid = JSON.stringify(arrId)
            var data = $.view.retrieveMoneyDoc(dataguid, targetScope);

            if (data) {
                $.view.loadData(targetScope, data);
            }
            $.view.setViewEditStatus(targetScope, 1);
            $.view.cancelObj = { data: data, status: status };

            //$.view.init(targetScope, 4, selRow.GUID);
        }
    }
});

$.extend($.view, {
    retrieveMoneyDoc: function (guid, scope) {
        
        var data;
        if (guid && scope) {
            var url = '/' + scope + '/RetrieveMoneyDoc';
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

//将列表数据行的数据添加到主datagrid中
$.extend($.fn.datagrid.defaults, {
    onDblClickRow: function (rowIndex, rowData) {debugger
        //获取datagrid所有属性        var opts = $(this).datagrid('options');
        //
        var treeconfig = opts.gridassociate;
        //
        if (treeconfig && treeconfig.gridId && treeconfig.map) {
            var gridId = '#' + opts.gridId;
            var map = treeconfig.map;
            var gridId = '#' + treeconfig.gridId, params = { map: map, treeData: rowData };
            //如果主datagrid数据表没有一行数据，则提示选择数据行
            if ($(gridId).datagrid('getSelected')) {
                //将数据添加到edatagrid
                $(gridId).edatagrid('updateSelectedRow', params);
            }
            else {
                $.messager.alert('提示', '您未选择要填充支票号的记录行！');
                return;
            }
        }
    }
});

//点击树，刷新下方数据表数据
$.extend($.fn.tree.methods, {
    //点击节点时发生
    setAssociate: function (node) {debugger
        //得到节点id
        var id = node.id;
        //获取树        var options = $(this).tree('options');
        //得到树的id
        var gridId = options.gridId;
        //console.log(gridId);
        //将节点数据重新reload到datagrid上，将后台的参数传过来作为参数        $('#' + gridId).datagrid('reload', { bankAccountID: id });
    }
});

