$.extend($.fn.linkbutton.methods, {
    zpSearch: function () {
        var checkNumberId = '#zpgl-CN_Check-CheckSelect';
        var parms = $(this).linkbutton('getParms', 'zpSearch');
        var url, gridId, isValid;
        if (parms && parms.length) {
            url = parms[0];
            gridId = parms[1];
        }
        $.ajax({
            url: url,
            data: { checkNumber: $(checkNumberId).val() },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {

                if (data) {
                    $('#' + gridId).datagrid('loadData', data);
                }
            }
        });
    },
    //重写选单的确定
    submitDoc: function () {

        var parms = $(this).linkbutton('getParms', 'submitDoc');
        var opts = $(this).linkbutton('options');
        var gridId, winId, zplbId, ywdjId, ywdjmxId, yhzhId;
        if (parms && parms.length) {
            gridId = '#' + parms[0];
            winId = '#' + opts.window;
            zplbId = '#' + parms[2]; //支票列表grid的Id,前台配置，顺序不可变
            ywdjId = '#' + parms[3]; //业务单据grid的Id,前台配置，顺序不可变
            ywdjmxId = '#' + parms[4]; //业务单据明细grid的Id,前台配置，顺序不可变
            yhzhId = '#' + parms[5]; //左下角银行账号grid的Id,前台配置，顺序不可变
        }
        var selRows = $(gridId).datagrid('getChecked');
        if (selRows.length != 1) {
            $.messager.alert('提示', '请选择一条记录');
            return;
        }debugger
        var guid = selRows[0]["GUID"];
        var DocTypeKey = selRows[0]["DocTypeKey"];
        var YWTypeKey = $('#zplqdj-zplqdj-YWType').combo('getValue') || "02"; ;

        $.ajax({
            url: 'zplq/YWRetrieve',
            data: { guid: guid, ywkey: YWTypeKey },
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {

                if (data) {
                    $(winId).dialog('close');
                    $(zplbId).datagrid('loadData', data.CN_CheckList);
                    $('#zplq-zplq-isLQChecked').val(data.IsLQChecked);

                    $(ywdjId).datagrid('loadData', [data.YWDocList]);
                    $(ywdjmxId).datagrid('loadData', data.YWDocList.DetailList);

                    $(yhzhId).datagrid('reload', { bankAccountID: null, isInvalid: 1, DocTypeKey: DocTypeKey });
                }
            }
        });
    },

    //选单 确定的方法 
    selectDoc: function () {
        var opts = $(this).linkbutton('options');
        var pageState = $.view.getStatus(opts.scope);
        var parms = $(this).linkbutton('getParms', 'selectDoc');
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
                $.view.setViewEditStatus('zplqdj', 1);
            }
        });
    },
    //领取 保存
    makeUsed: function () {
        debugger
        var parms = $(this).linkbutton('getParms', 'makeUsed');
        var YWTypeId, YWTypeKey, zplbId, ywdjId, ywdjmxId;
        if (parms && parms.length) {
            YWTypeId = '#' + parms[1]; //业务类型编码,前台配置，顺序不可变
            zplbId = '#' + parms[2]; //支票列表grid Id,前台配置，顺序不可变
            ywdjId = '#' + parms[3]; //业务单据grid Id,前台配置，顺序不可变
            ywdjmxId = '#' + parms[4]; //业务单据明细grid Id,前台配置，顺序不可变
        }
        YWTypeKey = !$('#zplqdj-zplqdj-YWType')[0]?"02": $('#zplqdj-zplqdj-YWType').combo('getValue');
        var zpRows = $(zplbId).datagrid('getData');
        var ywRows = $(ywdjId).datagrid('getData');

        var data = {
            CN_CheckList: [],
            YWDocList: {}
        }
        ;
        for (var i = 0, j = zpRows.rows.length; i < j; i++) {
            if (zpRows.rows[i].IsLQChecked == 0) {
                data.CN_CheckList.push(zpRows.rows[i]);
            }
        }
        data.YWDocList = ywRows.rows[0];
        //3个参数
        $.ajax({
            url: 'zplq/Save',
            data: { ywkey: YWTypeKey, data: JSON.stringify(data) }, //传过去 单据类型 和 主单的guid
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $.messager.alert("错误", $.view.warning, 'error');
            },
            success: function (data) {
                
                if (!data.s) return;
                if (data.data) {
                    $("#zplq-CN_Check").datagrid('loadData', data.data);
                    //GUID集合
                    var guid = "";
                    //                    for (var i = 0, j = data.data.length; i < j; i++) {
                    //                        guid += data.data[i].GUID_Doc+",";
                    //                    }
                    //                    guid = guid.substring(0, guid.length - 1);
                    guid = data.data[0].GUID_Doc;
                    var url = "/Print/zplqFrame?guid=" + guid;

                    // window.open("/Print/zplqFrame?guid=" + guid, '_blank', 'fullscreen=1');
                    var win = window.open("about:blank", "", "fullscreen=1");
                    win.moveTo(0, 0);
                    win.resizeTo(screen.width + 20, screen.height);
                    win.focus();
                    win.location = url;
                }
                //$.messager.alert(data.s.t, data.s.m, data.s.i);
            }
        });
    },
    
    //换票
    changeCheck: function () {
        ;
        //        var falg = $('#zplq-zplq-isLQChecked').val();
        //        if (falg == '0') return;
        var YWTypeKey = $('#zplq-zplq-YWTypeKey').val();
        var rows = $('#zplq-CN_CheckDrawMain').datagrid('getRows');
        if (rows.length != 1) {
            $.messager.alert('提示', '没有业务单据不能进行换票操作');
            return;
        }
        var guids = [];
        var checkChange = $('#zplq-CN_Check').datagrid('getChecked');
        if (!checkChange || checkChange.length == 0) {
            $.messager.alert('提示', '请选择要换票的支票记录');
            return;
        }

        for (var i = 0; i < checkChange.length; i++) {
            guids.push(checkChange[i].GUID);
        }
        guids = guids.join(",") + "";
        $.messager.confirm("提示", "此操作将作废选中的原支票号，是否要继续？(请在重领后重新领取支票)?", function (data) {
            if (!data) {
                return;
            } else {
                $.ajax({
                    url: '/zplq/ChangeCheck',
                    data: { guid: guids, ywkey: YWTypeKey },
                    dataType: "json",
                    type: "POST",
                    traditional: true,
                    error: function (xmlhttprequest, textStatus, errorThrown) {
                        $.messager.alert("错误", $.view.warning, 'error');
                    },
                    success: function (data) {
                        ;
                        if (data.check) {
                            var rowData = data.check;
                            var rowIndex = $('#zplq-CN_Check').datagrid('getRowIndex', checkChange[0]);
                            //$('#zplq-CN_Check').datagrid('updateRow', $.extend(checkChange[0], data));
                            $('#zplq-CN_Check').datagrid('updateRow', { index: rowIndex, row: rowData[0] });
                        }
                        if (data.msg) {
                            $.messager.alert("提示", data.msg);
                            return;
                        }
                    }
                });
            }
        });
    }

});