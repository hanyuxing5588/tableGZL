$(document).ready(function () {
    var d = new Date();
    $('#hxcl-close').bind('click', function () {
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
    })
    $('#hxcl-DocDate').datebox('setValue', d.Format('yyyy-MM-dd'));
    $('#hxcl-DocDate').datebox({
        onSelect: function (date) {
            var dateHx = date.Format('yyyy-MM-dd');
            var o = $.TransferObject.GetUrlFilter();
            if (!o.guid) return;
            o.hxDate = dateHx;
            $('#hxcl-xuandan').linkbutton('submitLoadDoc', o);
        }
    });
    var isProcess = $('#initguid').val();
    if (isProcess == "") {
        $.ajax({
            url: '/hxcl/GetBorrowMenoy',
            dataType: "json",
            type: "POST",
            error: function (xmlhttprequest, textStatus, errorThrown) {


            },
            success: function (data) {
                var value = data.sumMenoy ? data.sumMenoy : 0;
                $('#hxcl-borrow-Money').val(value);
            }
        });
    } else {

        var strGuid = $('#initguid').val();
        var docType = $('#initDocType').val()||0;
        var o = $.TransferObject.GetUrlFilter({ guid: strGuid, docType: docType });
        o.hxDate = d.Format('yyyy-MM-dd');
        $('#hxcl-save').linkbutton('enable');
        $('#hxcl-save').bind('click', function () {
            $.fn.linkbutton.methods["saveDoc"].call(this);
        })
        $('#hxcl-xuandan').linkbutton('submitLoadDoc', o); //用mark：0来标记当前为原单查询
    }
})
$.TransferObject = {
    MainObject: {
        '0': [], //报销管理
        '1': [], //收入管理
        '2': [], //往来管理
        '3': [], //提现管理
        '4': [], //收款管理
        '5': [], //公务卡汇总管理
        '6': [], //专用基金管理
        '7': []   //借款的往来
    },
    GetGuidAndType: function (mainO, gridRows) {
        for (var i = 0, j = gridRows.length; i < j; i++) {
            var curRow = gridRows[i];
            if (!curRow.DocNum) continue;
            if (mainO[curRow.DocFlag]) {
                mainO[curRow.DocFlag].push(curRow.GUID);
            } else {
                mainO[curRow.DocFlag] = [];
                mainO[curRow.DocFlag].push(curRow.GUID);
            }
        }

    },
    GetGridValue: function () {
        var mainO = {};
        var debits = $('#hxcl-DebitDetail').datagrid('getRows');
        var credits = $('#hxcl-CreditDetail').datagrid('getRows');
        this.GetGuidAndType(mainO, debits);
        this.GetGuidAndType(mainO, credits);
        return mainO;
    },
    GetValue: function (mainO, newSelectObject) {
        if (newSelectObject) {
            if (mainO[newSelectObject.docType]) {
                mainO[newSelectObject.docType].push(newSelectObject.guid);
            } else {
                mainO[newSelectObject.docType] = [newSelectObject.guid];
            }
        }
        var o = { typeArr: [], Guids: [], mark: [] };
        for (var i in mainO) {
            if (mainO[i].length == 0) continue;
            o.typeArr.push(i=="7"?2:i);
            o.Guids.push(mainO[i].join(','));
            o.mark.push(i == '7' ? 1 : 0);
        }
        return { guid: o.Guids.join('$'), docType: o.typeArr.join('$'), mark: o.mark.join('$') };
    },
    GetUrlFilter: function (newSelectObject) {
        var mainO = this.GetGridValue();
        var transO = this.GetValue(mainO, newSelectObject);
        transO = $.extend(transO, this.GetCerObject);
        return transO;
    },
    GetRuleKeyValue: function (firstRow) {
        if (!firstRow.DocNum) return "";
        return [firstRow.DWName, '&', firstRow.DepartmentName, '&', firstRow.PersonName].join(',');
    },
    GetBorrowUserID: function () {
        return $('#hxcl-borrow-PersonId').val();
    },
    GetRuleKey: function () {
        var debits = $('#hxcl-DebitDetail').datagrid('getRows');
        if (debits.length > 0) {
            return this.GetRuleKeyValue(debits[0]);
        }
        var credits = $('#hxcl-CreditDetail').datagrid('getRows');
        if (credits.length > 0) {
            return this.GetRuleKeyValue(credits[0]);
        }
        return "";
    },
    GetCerObject: { m: '', d: '', status: '' }
}
$.TransData = {
    GetM: function () {
        //对方帐套
        var m = [], mName = 'CW_PZMain';
        //业务类型
        m.push({ m: mName, n: 'YWTypeName', v: '核算管理' });
        //单据类型
        m.push({ m: mName, n: 'DocTypeName', v: '会计凭证' });
        //凭证类型
        m.push({ m: mName, n: 'PZTypeName', v: '记账凭证' });
        //核销日期
        var DocDate = $('#hxcl-DocDate').datebox('getValue');
        m.push({ m: mName, n: 'DocDate', v: DocDate });
        //借款单位
        var DWName = $('#hxcl-borrow-DWName').val();
        m.push({ m: mName, n: 'DWName', v: DWName });
        //借款单位GUID
        var GUID_DW = $('#hxcl-borrow-DwId').val();
        m.push({ m: mName, n: 'GUID_DW', v: GUID_DW });
        //对方帐套
        var ExteriorDataBase = $('#hxcl-zaccount').val();

        m.push({ m: mName, n: 'AccountKey', v: ExteriorDataBase });
        //对方帐套年度
        var FiscalYear = $('#hxcl-ztnd').val();
        m.push({ m: mName, n: 'FiscalYear', v: FiscalYear });
        //会计期间
        var CWPeriod = $('#hxcl-kjqj').val();
        m.push({ m: mName, n: 'CWPeriod', v: CWPeriod });
        //凭证编号
        var DocNum = $('#hxcl-DocNum').val();
        m.push({ m: mName, n: 'DocNum', v: DocNum });
        //用友编号
        var Ino_id = $('#hxcl-pzh').val();
        m.push({ m: 'GL_accvouch', n: 'Ino_id', v: Ino_id });
        //借方 贷方
        var sumJE = $('#hxcl-SumTotal').val();
        m.push({ m: mName, n: 'Total_C', v: sumJE });
        m.push({ m: mName, n: 'Total_D', v: sumJE });
        //制单人        var operator = $('#hxcl-maker').val();
        m.push({ m: mName, n: 'Maker', v: operator });
        //附单数据
        var BillCount = $('#hxcl-BillCount').val();
        m.push({ m: mName, n: 'BillCount', v: BillCount });

        return m;
    },
    GetD: function () {
        var d = { m: 'CW_PZDetail', r: [] };
        var data = $('#hxcl-KJPZDetail').datagrid('getData');

        for (var i = 0, j = data.rows.length; i < j; i++) {
            var rowO = [];
            var curRow = data.rows[i];
            var totalFalg = false;
            for (var attr in curRow) {
                if (attr == "Total_JF" || attr == "Total_DF") {
                    if (totalFalg || curRow[attr] == 0) continue;
                    if (curRow.IsDC.toString().toLowerCase() == 'true') {
                        rowO.push({ m: 'CW_PZDetail', n: 'Total_Borrow', v: curRow[attr] });

                        rowO.push({ m: 'CW_PZDetail', n: 'Total_PZ', v: curRow[attr] });
                        //                        rowO.push({ m: 'CW_PZDetail', n: 'IsDC', v: 1 })
                    } else {
                        rowO.push({ m: 'CW_PZDetail', n: 'Total_Loan', v: curRow[attr] });

                        rowO.push({ m: 'CW_PZDetail', n: 'Total_PZ', v: curRow[attr] });
                        //                        rowO.push({ m: 'CW_PZDetail', n: 'IsDC', v: 0 })
                    }
                    totalFalg = true;
                } else if (attr == "PZMemo") {
                    rowO.push({ m: 'CW_PZDetail', n: 'FeeMemo', v: curRow[attr] });
                }
                else {
                    rowO.push({ m: 'CW_PZDetail', n: attr, v: curRow[attr] });
                }
            }
            d.r.push(rowO);
        }

        return [d];
    },
    GetData: function () {
        var data = { m: [], d: {} };
        data.m = this.GetM();
        data.d = this.GetD();
        return data;
    },
    LoadData: function (data) {
        var model = data;
        //加载合计列表
        $('#hxcl-SumDetail').datagrid('loadData', model.listSum || []);
        //加载借方列表
        $('#hxcl-DebitDetail').datagrid('loadData', model.listDebit || []);
        //加载贷方列表
        $('#hxcl-CreditDetail').datagrid('loadData', model.listCredit || []);
        //加载会计凭证
        $('#hxcl-KJPZDetail').datagrid('loadData', model.listKJPZ || []);
        //加载借款信息
        if (model.borrow) {
            var id = "#hxcl-borrow-";
            var o = model.borrow;
            for (var i in o) {
                var type = $(id + i).GetEasyUIType();
                $(id + i)[type]('setValue', o[i] || '');
            }
        } else {
            $('#[id^=' + id + ']').validatebox('setValue', '');
        }
        if (model.SumTotal) {//总金额

            if (model.SumTotal > 0) {
                $('#showje').text("付款金额");
            } else {
                $('#showje').text("收款金额");
            }
            $('#hxcl-SumTotal').numberbox('setValue', Math.abs(model.SumTotal));
        }
        
        $('#hxcl-DocDate').datebox('setValue', model.DocDate ? model.DocDate : '');
        //帐套
        $('#hxcl-zaccount').validatebox('setValue', model.zt ? model.zt : '');
        //年度
        $('#hxcl-ztnd').validatebox('setValue', model.yzt ? model.yzt : '');
        //会计区间
        $('#hxcl-kjqj').validatebox('setValue', model.kjqj ? model.kjqj : '');
        //凭证号(用友编号)
        $('#hxcl-pzh').validatebox('setValue', model.u8Num ? model.u8Num : '');
        //凭证编号
        $('#hxcl-DocNum').validatebox('setValue', model.DwPZMain.DocNum ? model.DwPZMain.DocNum : '');
        //附单数据
        $('#hxcl-BillCount').validatebox('setValue', model.DwPZMain.BillCount ? model.DwPZMain.BillCount : '');

        if (model.borrow) {
            //人员 单位 部门
            var model = model.borrow;
            $('#hxcl-borrow-PersonId').validatebox('setValue', model.PersonId);
            $('#hxcl-borrow-DwId').validatebox('setValue', model.DwId);
            $('#hxcl-borrow-DepartmentId').validatebox('setValue', model.DepartmentId);
        } else {
            $('#hxcl-borrow-PersonId').validatebox('setValue','');
            $('#hxcl-borrow-DwId').validatebox('setValue', '');
            $('#hxcl-borrow-DepartmentId').validatebox('setValue','');
         }


    }

}
$.extend($.fn.linkbutton.methods, {
    //核销提交流程
    submitProcess1: function () {
        var opts = $(this).linkbutton('options');
        if ($.view.judgePageCancleState(opts.docState)) {
            $.messager.alert('提示', '该单据已经作废,操作无效')
            return;
        }
        var parms = $(this).linkbutton('getParms', 'submitProcess');
        if (parms && parms.length) {
            var url = parms[0];
            if (url) {
                var guid = $('#initguid').val();
                if (guid) {
                    $.ajax({
                        url: url,
                        data: { "Guid": guid, "scope": opts.scope },
                        dataType: "json",
                        type: "POST",
                        traditional: true,
                        error: function (xmlhttprequest, textStatus, errorThrown) {
                            $.messager.alert("错误", $.view.warning, 'error');
                        },
                        success: function (response) {
                            var data = eval(response);
                            $.messager.alert(data.Title, data.Msg, data.Icon);
                        }
                    });
                } else {
                    $.messager.alert('提示', '核销的单据没有对应的流程');
                }
            }
        }
    },
    //打开会计凭证
    openCer: function () {

        var rows = $('#hxcl-KJPZDetail').datagrid('getRows');
        if (rows.length == 0) {
            $.messager.alert("提示", "没有要打开的会计凭证");
            return;
        }
        var opts = $('#hxcl-save').linkbutton('options');
        var status = opts.disabled ? 4 : 2;
        var opts = $(this).linkbutton('options');
        $('#b-kjpzwindow').dialog({
            isCancel: true,
            resizable: false,
            title: '会计凭证',
            width: 1200,
            height: 600,
            modal: true,
            draggable: true,
            resizable: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: '/kjpz?scope=kjpz&Flag=1',
            onLoad: function (c) {
                debugger
                var data = $.TransData.GetData();
                //FeeMemo transfer to PZMemo
                for (var i = 0; i < data.d[0].r.length; i++) {
                    var detail = data.d[0].r[i];
                    detail[2].n = 'PZMemo';
                }
                $.view.loadData(opts.scope, data);
                $.view.setViewEditStatus(opts.scope, status);
            }
        });

    },
    //选单确定
    submitOriginal: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitOriginal');
        if (!parms || parms.length < 0) return;
        var grid = $('#' + parms[0]);
        var winId = '#' + opts.window;

        var selRows = grid.datagrid('getSelections');
        if (!selRows.length) {
            $.messager.alert('系统提示', '请选择一条数据');
            return;
        }
        var docType = $('#history-history-DocYWType').combo('getValue');
        var mainObject = $.TransferObject.GetGridValue();

        var GuidValidArr = mainObject[docType];
        //以第一条做为键值

        var firstRow = selRows[0];
        var key = $.TransferObject.GetRuleKey();
        var firstRowKey = [firstRow.DWName, '&', firstRow.DepartmentName, '&', firstRow.PersonName].join(',');
        if (key && firstRowKey != key) {
            $.messager.alert('系统提示', '所选择的数据行中的单位、部门、人员名称与列表已有数据不完全相等,不能进行核销！');
            return;
        }
        //定义数组用来存放除了第一条记录之外的所有记录行
        var selectedGuidArr = [firstRow.Guid];
        for (var i = 1, j = selRows.length; i < j; i++) {
            var curRow = selRows[i];
            if (GuidValidArr && GuidValidArr.exist(curRow.GUID)) {
                $.messager.alert('系统提示', '选择单据已经在贷方或者借方列表中存在！');
                return;
            }
            var curRowKey = [curRow.DWName, '&', curRow.DepartmentName, '&', curRow.PersonName].join(','); // (curRow.DWName + curRow.DepartmentName + curRow.PersonName).replace(/\s/ig, '');
            if (key && curRowKey != key) {
                $.messager.alert('系统提示', '所选择的数据行中的单位、部门、人员名称与列表已有数据不完全相等,不能进行核销！');
                return;
            }
            if (firstRowKey != curRowKey) {
                $.messager.alert('系统提示', '所选择的数据行中的单位、部门、人员名称不完全相等,不能进行核销！');
                return;
            }
            selectedGuidArr.push(curRow.Guid);
        }
        $(winId).dialog('close');

        var strGuid = selectedGuidArr.join(',');
        $('#stortype').val(docType);
        $('#storId').val(strGuid);
        $('#stormark').val(0);
        var dateHx = $('#hxcl-DocDate').datebox('getValue');

        var o = $.TransferObject.GetUrlFilter({ guid: strGuid, docType: docType, mark: 0 });
        o.hxDate = dateHx;
        $(this).linkbutton('submitLoadDoc', o); //用mark：0来标记当前为原单查询

    },
    //借款确定
    submitBorrow: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'submitBorrow');

        if (parms.length > 0) {
            var gridId = '#' + parms[0];
            var winId = '#' + opts.window;
            var selRows = $(gridId).datagrid('getSelections');
            if (!selRows) {
                $.messager.alert('系统提示', '请选择一条数据');
                return;
            }
            var mainObject = $.TransferObject.GetGridValue();
            var GuidValidArr = mainObject[2];

            var firstRow = selRows[0];
            var key = $.TransferObject.GetRuleKey();
            var firstRowKey = [firstRow.DWName, '&', firstRow.DepartmentName, '&', firstRow.PersonName].join(',');
            if (key && firstRowKey != key) {
                $.messager.alert('系统提示', '所选择的数据行中的单位、部门、人员名称与列表已有数据不完全相等,不能进行核销！');
                return;
            }
            //验证
            var selectedGuidArr = [firstRow.Guid];
            for (var i = 1, j = selRows.length; i < j; i++) {
                var curRow = selRows[i];
                if (GuidValidArr && GuidValidArr.exist(curRow.GUID)) {
                    $.messager.alert('系统提示', '选择单据已经在贷方或者借方列表中存在！');
                    return;
                }
                var curRowKey = [curRow.DWName, '&', curRow.DepartmentName, '&', curRow.PersonName].join(','); // (curRow.DWName + curRow.DepartmentName + curRow.PersonName).replace(/\s/ig, '');
                if (key && curRowKey != key) {
                    $.messager.alert('系统提示', '所选择的数据行中的单位、部门、人员名称与列表已有数据不完全相等,不能进行核销！');
                    return;
                }
                if (firstRowKey != curRowKey) {
                    $.messager.alert('系统提示', '所选择的数据行中的单位、部门、人员名称不完全相等,不能进行核销！');
                    return;
                }
                selectedGuidArr.push(curRow.Guid);
            }
            $(winId).dialog('close');

            var strGuid = selectedGuidArr.join(',');

            var dateHx = $('#hxcl-DocDate').datebox('getValue');
            debugger
            var o = $.TransferObject.GetUrlFilter({ guid: strGuid, docType: 7, mark: 1 }); debugger
            o.hxDate = dateHx;
            $(this).linkbutton('submitLoadDoc', o); //用mark：0来标记当前为原单查询

        }
    },
    //选单(借款)提交加载单据
    submitLoadDoc: function (jq, params) {
        debugger

        $.ajax({
            url: '/hxcl/ChangeDocData',
            data: params,
            dataType: "json",
            type: "POST",
            error: function (xmlhttprequest, textStatus, errorThrown) {


            },
            success: function (data) {

                if (data.error) {
                    $.messager.alert('系统提示', data.error);
                    return;
                } else {
                    $.TransData.LoadData(data);
                }
            }
        });
    },
    //选单查询
    getHistoryByFilter: function (url, gridId, scope, region, tcondition) {
        var valid = $('#history-history-DocNum').validatebox('isValid');
        if (!valid) {
            $.messager.alert('提示', '请输入正确的单号');
            return;
        }
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
                    $.messager.alert("错误", textStatus, 'error');
                },
                success: function (data) {

                    $('#' + gridId).datagrid('loadData', data);
                }
            });
        }
    },
    //选单
    selectDoc: function () {
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'selectDoc');

        if (parms.length < 2) return;
        $('#b-window').dialog({
            isCancel: true,
            resizable: false,
            title: '选单',
            width: 1000,
            height: 600,
            modal: true,
            draggable: true,
            resizable: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: parms[0],
            onLoad: function (c) {
                $.view.setViewEditStatus(parms[1], 4); //parms[1] scope
            }
        });
    },
    //添加移除方法 sxh 2014-03-26 9:30
    deleteRow: function () {
        var docGUIDArr = []; var flag = 1;
        var deleteRowMain = function (gridId) {
            var selectRows = $(gridId).datagrid('getChecked');
            for (var i = 0, j = selectRows.length; i < j; i++) {
                flag = 0;
                var curRow = selectRows[i];
                if (curRow && curRow.Guid) {
                    docGUIDArr.push(curRow.Guid);
                }
                //得当当前行的索引值

                var currIndex = $(gridId).datagrid('getRowIndex', curRow);
                $(gridId).datagrid('deleteRow', currIndex);
                $(gridId).datagrid('uncheckAll');
            }
        }
        //获取到当前按钮的options属性

        var opts = $(this).linkbutton('options');
        //获取grid集合
        var gridIds = opts.bindparms.gridIds;
        //借方单据gridId
        var DebitId = '#' + gridIds[0];
        //贷方单据gridId
        var CreditId = '#' + gridIds[1];
        //借方选中行

        deleteRowMain(DebitId);
        deleteRowMain(CreditId);
        var rowsD = $(DebitId).datagrid('getRows');
        var rowsC = $(CreditId).datagrid('getRows');
        if (rowsD.length == 0 && rowsC.length == 0) {
            $('#hxcl-save').linkbutton('enabled');
        }
        if (flag) {
            $.messager.alert('系统提示', '请选中要移除的数据行！');
        }

    },
    //添加全选方法 sxh 2014-03-26 11:40 
    hxclSelectAll: function () {
        var me = this;
        //获得全选按钮的options配置属性
        var opts = $(me).linkbutton('options');
        //获取gridId
        var gridId = opts.bindparms.hxclSelectAll[1];
        var rows = $('#' + gridId).edatagrid('getRows');
        if (rows.length <= 0) {
            $.messager.alert('提示', '列表中没有数据，不能选中！');
            return;
        }
        if (gridId) {
            $('#' + gridId).datagrid('checkAll');
        }
    },
    //添加全消方法 sxh 2014-03-26 12:00 
    hxclUnSelectAll: function () {
        var me = this;
        //获得全选按钮的options配置属性


        var opts = $(me).linkbutton('options');
        //获取gridId
        var gridId = opts.bindparms.hxclUnSelectAll[1];
        var rows = $('#' + gridId).edatagrid('getRows');
        if (rows.length <= 0) {
            $.messager.alert('提示', '列表中没有数据，不能取消！');
            $('#hxcl-save').linkbutton('disabled');
            return;
        }
        if (gridId) {
            $('#' + gridId).datagrid('uncheckAll');
        }
    },
    //重写借款方法 sxh 2014-03-26 14:08
    borrow: function () {
        var opts = $(this).linkbutton('options');
        var pageState = "1";
        var parms = $(this).linkbutton('getParms', 'borrow');
        if (parms && parms.length >= 2) {
            var url = parms[0];
            var userId = $('#hxcl-borrow-PersonId').validatebox('getValue');
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
                    href: url + '?UserBorrowID=' + (userId || ''),
                    onLoad: function (c) {
                        $.view.setViewEditStatus(targetscope, pageState);
                    }
                });
            }
        }
    },
    //保存核销
    saveDoc: function () {
        var $linkButton = $(this);
        var opts = $linkButton.linkbutton('options');
        var url = opts.bindparms.saveDoc[0];
        //从页面上获取单据的guid 和 单据的类型 以及是选单还是借款
        var guid = $('#storId').val(),
            doctype = $('#stortype').val(),
            mark = $('#stormark').val(),
            dateHx = $('#hxcl-DocDate').datebox('getValue'),
            pzNum = $('#hxcl-pzh').validatebox('getValue');

        if (dateHx == "") {
            $.messager.alert('提示', "请选择核销日期");
            return;
        }
        var o = $.TransferObject.GetUrlFilter();
        if (o.guid == "") {
            $.messager.alert('提示', "请选择要核销的单据");
            return;
        }
        if (opts.disabled == true) {
            $.messager.alert('提示', "请耐心等待");
            return;
        }
        $linkButton.linkbutton('disable');
       
        o.hxDate = dateHx;
        o.pzNum = pzNum;
        // return;
        $.ajax({
            url: url,
            data: o,
            dataType: "json",
            type: "POST",
            traditional: true,
            error: function (xmlhttprequest, textStatus, errorThrown) {
                $linkButton.linkbutton('enable');
                $.messager.alert("错误", textStatus, 'error');
            },
            success: function (data) {
                $linkButton.linkbutton('enable');
                if (data.error) {
                    $.messager.alert('系统提示', data.error);
                    return;
                } else {
                    var msg = "用友会计凭证已生成；{0}月，第{1}号";
                    $.messager.alert('提示', $.format(msg, $('#hxcl-kjqj').val(), pzNum));
                    $linkButton.linkbutton('disabled');
                    $.TransData.LoadData(data);
                }
            }
        });
    },
    //修改关闭页签 sxh 2014/05/21 10:40
    closeTab: function () {
        //拿到关闭按钮的属性
        var opts = $(this).linkbutton('options');
        //拿到当前页面的scope属性
        var scope = opts.scope;
        //拿到当前页面状态
        var pageState = $.view.getStatus(scope);
        if (pageState == "1") {
            $.messager.confirm("提示", "正在编辑,是否退出?", function (data) {
                if (!data) return;
                !opts.closeWin ? $('#b-window').dialog('close') : parent.window.CloseTabs();
            });
        }
        if (pageState == "2") {
            $.messager.confirm("提示", "正在编辑,是否退出?", function (data) {
                if (!data) return;
                !opts.closeWin ? $('#b-kjpzwindow').dialog('close') : parent.window.CloseTabs();
            });
        }
    },
    //确定会计凭证
    submitPer: function () {
        //结束grid编辑状态
        var cgrid = $('#kjpz-CW_PZDetail');
        var selectrow = cgrid.datagrid('getSelected');
        var selectindex = cgrid.datagrid('getRowIndex', selectrow);
        cgrid.datagrid('endEdit', selectindex);

        var borrowTotal = $('#kjpz-CW_PZMain-Total_C').val();
        var loanTotal = $('#kjpz-CW_PZMain-Total_D').val();
        if (borrowTotal != loanTotal) {
            $.messager.alert('提示', '借方合计和贷方合计不相等，不能进行确定操作！');
            return;
        }
        var opts = $(this).linkbutton('options');
        var scope = opts.scope;
        var winId = '#' + opts.window;
        var rowO = [];
        //会计期间
        var CWPeriod = $('#kjpz-CW_PZMain-CWPeriod').combobox('getValue');
        //年度
        var FiscalYear = $('#kjpz-CW_PZMain-FiscalYear').combobox('getValue');
        //凭证帐套
        var AccountKey = $('#kjpz-CW_PZMain-AccountKey').combobox('getValue');

        var data = $('#kjpz-CW_PZDetail').edatagrid('getDataDetail');

        for (var i = 0, j = data.r.length; i < j; i++) {
            var curRow = data.r[i];
            var rowNew = {};
            for (var m = 0, n = curRow.length; m < n; m++) {
                var cCol = curRow[m];
                if (cCol.n == "Total_Borrow") {
                    rowNew["Total_JF"] = cCol.v;
                    rowNew["IsDC"] = 'True';
                } else if (cCol.n == "Total_Loan") {
                    rowNew["Total_DF"] = cCol.v;
                    rowNew["IsDC"] = 'False';
                } else if (cCol.n == "FeeMemo") {
                    rowNew["PZMemo"] = cCol.v;
                } else {
                    rowNew[cCol.n] = cCol.v;
                }
            }
            rowO.push(rowNew);
        }
        debugger
        var indata = $.view.retrieveData(scope, "dataregion");
        $.TransferObject.GetCerObject = { "status": 1, "m": JSON.stringify(indata.m), "d": JSON.stringify(indata.d) };
        $(winId).dialog('close');
        $('#hxcl-kjqj').val(CWPeriod);
        $('#hxcl-ztnd').val(FiscalYear);
        $('#hxcl-zaccount').val(AccountKey);
        $('#hxcl-KJPZDetail').datagrid('loadData', rowO);

    },
    //打印浏览
    printView: function () {
        debugger
        //重新获取数据，
        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'print');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        var detailId = parms[1];
        var dataRow = $.TransferObject.GetUrlFilter();
        //$("#" + detailId[0]).datagrid("getRows");
        if (!dataRow) return;
        var urlArg = parms[0];
        var doctypekey = dataRow.docType; //dataRow[0].DocTypeKey;
        switch (doctypekey) {
            case "4": //收入凭单
                urlArg = "/Print/jzd";
                break;

        }
        var docGuid = dataRow.guid;
        var url = urlArg + "?guid=" + docGuid + "&doctypekey=" + doctypekey;
        window.open(url);
    }
});
