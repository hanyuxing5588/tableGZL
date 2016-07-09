$.extend($.view,{
    init1: function (scope, status, dataguid) {
    
        if (scope && status) {
            status = status + "";
            var id = "[id^='" + scope + "-'].easyui-linkbutton";
            switch (status) {
                case "1": //新建
                    $(id).linkbutton('bind', true);
                    $('#' + scope + '-add').click();
                    break;
                case "2": //修改
                case "3": //删除
                case "4": //浏览

                    var data = $.view.retrieveDoc(dataguid, scope);
                    if (!data) return;
                    var id = "[id^='" + scope + "-'].easyui-edatagrid";
                    $(id).edatagrid("alterStatus", status);
                    $.view.loadData(scope, data);

                    $.view.setViewEditStatus(scope, status);
                    $.view.cancelObj = { data: data, status: status };
                    break;
            }
        }
    }
});

$.extend($.fn.linkbutton.methods, {
    //关闭页签
    closeTab: function () {
    
        var me = this;
        var opts = $(me).linkbutton('options');
        var arr = [1, 2, 4];
        var scope = opts.scope;
        var pageState = $.view.getStatus(scope);
        if (arr.exist(pageState)) {
            $.messager.confirm("提示", "正在编辑,是否退出?", function (data) {
                if (!data) return;
                !opts.closeWin ? $('#b-yuandanwindow').dialog('close') : $($(parent.document).find("#closeTab")).click();
            })
        } else {
            !opts.closeWin ? $('#b-yuandanwindow').dialog('close') : $($(parent.document).find("#closeTab")).click();
        }
    },
    //参照
    reference: function () {

        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        var parms = $(this).linkbutton('getParms', 'reference');
        if (!parms && parms.length != 4) return;
        var url = parms[0];
        var winId = '#' + opts.window;
        $(winId).dialog({
            title: '参照',
            fit: true,
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
    //原单
    originalSingle: function () {
        
        //获取到当前按钮的options属性
        var opts = $(this).linkbutton('options');
        //拿到单据类型
        var scope = opts.scope;
        //获取datagrid的id
        var gridId = '#' + opts.scope + '-BX_CollectDetail';
        //获取选中行数据
        var selRow = $(gridId).datagrid('getSelected');
        if (!selRow) {
            $.messager.alert('系统提示', '请选择单据！');
            return;
        }
        //拿到单据的GUID
        var guid = selRow['gwkhzbxd-BX_Detail-GUID_BX_Main'];

        //拿到单据状态
        var status = opts.status;
        //目标打开单据类型
        var targetScope = opts.targetScope;

        var GetUrl = function (targetScope, guid, status) {
            var url = "";
            if (targetScope != undefined && targetScope.length >= 0 && targetScope != "") {
                var url = "/" + targetScope + "?scope=" + targetScope + "";
            }
            else {
                return "";
            }
            if (guid) {
                if (url.indexOf("?") >= 0) {
                    url += "&guid=" + guid;
                }
                else {
                    url += "?guid=" + guid;
                }

            }
            if (status) {
                if (url.indexOf("?") >= 0) {
                    url += "&status=" + status;
                }
                else {
                    url += "?status=" + status;
                }
            }
            return url;
        }
        
        $('#b-yuandanwindow').dialog({
            title: "单据",
            resizable: false,
            fit: true,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: GetUrl(targetScope, guid, status),
            onLoad: function (c) {
               
                $.view.init1(targetScope, status, guid);
                
            }
        });
    },
    //全选
    GWKHZBXDSelectAll: function () {

        var me = this;
        //获得全选按钮的options配置属性
        var opts = $(me).linkbutton('options');
        //获取gridId
        var gridId = opts.bindparms.GWKHZBXDSelectAll[1];
        var rows = $('#' + gridId).edatagrid('getRows');
        if (rows.length <= 0) {
            $.messager.alert('提示', '列表中没有数据，不能选中！');
            return;
        }
        if (gridId) {
            $('#' + gridId).datagrid('checkAll');
        }

    },
    //全消
    GWKHZBXDUnSelectAll: function () {

        var me = this;
        //获得全选按钮的options配置属性
        var opts = $(me).linkbutton('options');
        //获取gridId
        var gridId = opts.bindparms.GWKHZBXDUnSelectAll[1];
        var rows = $('#' + gridId).edatagrid('getRows');
        if (rows.length <= 0) {
            $.messager.alert('提示', '列表中没有数据，不能取消！');
            return;
        }
        if (gridId) {
            $('#' + gridId).datagrid('uncheckAll');
        }
    },
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
    //    //退出
    //    closeTab: function () {
    //        var me = this;
    //        //获得退出按钮的options配置属性
    //        var opts = $(me).linkbutton('options');
    //        //获得窗体的id
    //        var winId = opts.window;
    //        if (winId) {
    //            $('#' + winId).dialog('close');
    //        }
    //    },
    //确定
    submitReference: function () {
    
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitReference');
        debugger
        if (parms && parms.length >= 2) {
            var targetScope = parms[1];
            var gridId = '#' + parms[0];
            var targetGridId = '#' + parms[2];
            var winId = '#' + opts.window;
            var selRow = $(gridId).datagrid('getChecked');
            if (selRow.length <= 0) {
                $.messager.alert('系统提示', '请选择数据');
                return;
            }
            var guids = [];
            for (var i = 0, j = selRow.length; i < j; i++) {
                guids.push(selRow[i].GUID);
            }

            if (guids.length == 1) {
                guids = guids + ",";
            } else {
                guids = guids.join(',');
            }
            $(winId).dialog('close');
            $.view.setStatus(targetScope, 1); //历史确定之前先将页面状态改为4
            var data = $.view.retrieveDoc(guids, targetScope);
            if (!data) return;
            $(targetGridId).edatagrid('setData', data.d[0]);

            $(targetGridId).edatagrid('disableEditing');
        }
    }
})


$.extend($.fn.edatagrid.methods, {
    LoadSkdwInfosToGrid: function (jq) {
        
        if ('gwkhzbxd-BX_CollectDetail' != $(jq).attr('id')) return;
        //目标列数据
        var dataT = []; //target grid　data
        var dic2pn = {}; //pn:dataTIndex
        var cols = ['gwkhzbxd-CN_PaymentNumber-PaymentNumber', 'gwkhzbxd-CN_PaymentNumber-BGSourceName', 'gwkhzbxd-BX_Detail-Total_Real', 'gwkhzbxd-BX_CollectMain-DocMemo', 'gwkhzbxd-BX_Detail-GUID'];
        var rows = $(jq).datagrid('getRows');
        for (var i = 0, j = rows.length; i < j; i++) {
            //目标列名
            var o = {
                'PaymentNumber': '',
                'BGSourceName': '',
                'Total_Real': '',
                'DocMemo': ''
            };

            var rowCur = rows[i];
            for (var k = 0; k < cols.length; k++) {
                var colName = cols[k];
                var colData = "";
                if (colName == 'gwkhzbxd-BX_CollectMain-DocMemo') {
                    colData = "公务卡汇总";
                } else {
                    colData = rowCur[colName];
                }
                if (k == 2) {
                    colData = $.toCurrency(colData);
                }
                var objP = colName.split('-');
                o[objP[2]] = colData;

            }
            //财政支付令+预算来源作为唯一标示
            var czzflGuid = rowCur[cols[1]] + rowCur[cols[0]];
            if (dic2pn.hasOwnProperty(czzflGuid)) {
                var ii = dic2pn[czzflGuid];
                var row = dataT[ii];
                var je = cols[2]; //金额
                var colName = je.split('-')[2];
                row[colName] = parseFloat($.toCurrency(row[colName])) + parseFloat($.toCurrency(rowCur[je])); //字符串 转double
            } else {
                dataT.push(o);
                dic2pn[czzflGuid] = dataT.length - 1;
            }

        }

        $('#gwkhzbxd-gridColumnSum').datagrid('loadData', dataT);
    }
});