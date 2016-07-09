 /*
扩展combobox
*/
$.extend($.fn.combobox.methods, {
    //转换当前可编辑状态

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
                        $(this).combo('disable');
                    }
                    return;
                }
            }
            if (opts.disabled) {
                $(this).combo('enable');
            }
        });
    },
    getValue: function (jq) {
        return $(jq).combo('getValue');
    },
    getText: function (jq) {
        return $(jq).combo('getText');
    },
    //获得格式数据
    getData: function (jq, isText) {

        var vals = $(jq).attr('id');
        if (vals) {
            var vals = vals.split('-');
            if (vals) {
                var result = { m: vals[1], n: vals[2], v: $(jq).combobox('getValue') };
                if (isText) {
                    var re = [];
                    re.push(result);
                    var opts = $(jq).combobox('options'); //textField
                    if (opts.textField) {
                        re.push({ m: vals[1], n: opts.textField, v: $(jq).combobox('getText') });
                        return re;
                    }
                }
                return result;
            }
        }
        return null;
    },
    //加载格式数据
    setData: function (jq, hs) {


        return jq.each(function () {
            var idAttr = $(this).attr('id').split('-');
            if (!idAttr) return;
            if (hs) {
                var opts = $(this).combobox('options');
                var a = hs[idAttr[1]];
                if (a && a.hasOwnProperty(idAttr[2])) {
                    opts.tempValue = a[idAttr[2]];

                    return;
                }
                return;
            }
            $(this).combobox('setText', '');
            $(this).combobox('setValue', '');
        })
    },
    //加载数据到本地

    loadRemoteDataToLocal: function (jq) {
        return jq.each(function () {
            var me = $(this);
            var opts = me.combobox('options');
            if (opts.defaulttempValue) {
                opts.tempValue = opts.defaulttempValue;
            }
            if (!opts.remoteData.length && opts.remoteUrl) {
                $.ajax({
                    url: opts.remoteUrl,
                    dataType: "json",
                    type: "POST",
                    success: function (data) {
                        opts.remoteData = data;
                        me.combobox({ 'onLoadSuccess': function () {
                            //赋值

                            me.combobox('setValue', opts.tempValue);
                        }
                        });

                        me.combobox('loadData', data);
                    }
                });
            } else {

                me.combobox('setValue', opts.tempValue);
            }
        })

    },
    //验证combobox
    verifyData: function () {
        var opts = $(this).combobox('options');
        opts.required = true;
        var msg = "";
        var TempVal = $(this).combobox('getValue');
        if (TempVal == "") {
            var id = $(this).attr('id');
            var labid = "#lbl-" + id.split('-')[2];
            var labelVal = $(labid).text().replace('*', '');
            msg = $.trim(labelVal) + "不能为空！";
        }
        return msg;
    }

});
$.extend($.fn.combobox.defaults, {
    backValue: "", //后台字段
    filterField: "", //过滤列(只支持英文)
    scope: null,
    forbidstatus: null,
    bindmethod: null, //{ 'onSelect': ['setAssociate'] }
    associate: null, //{'label1':['field1','field2'],'label2':['field1','field2']}
    gridassociate: null, //{gridId:'g1',map:{'a1':'v1','a2':'v2'},append:false}
    remoteData: [], //远程获得的数据
    url: null,
    remoteUrl: null//远程url
});
/*
扩展combogrid
*/
$.extend($.fn.combogrid.methods, {
    setAssociate: function (jq) {

        var cls = $(this).attr('class');
        var opts = {}, r = null;
        if (cls && (cls.indexOf('easyui-combogrid') >= 0 || cls.indexOf('datagrid-editable-combogrid') >= 0)) {
            opts = $(this).combogrid('options');
            r = $(this).combogrid('getSelected');
        }
        else {
            opts = $(this).datagrid('options');
            r = $(this).datagrid('getSelected');
        }
        //重新设置表格行默认值

        if (opts.griddefaultassociate) {
            if (r == undefined || r == null) return;
            var gid = opts.griddefaultassociate.gridId;
            var gridopts = $('#' + gid).edatagrid('options');
            var defaultData = gridopts.defaultData;
            var defaultRow = gridopts.defaultRow.row;
            var defaultHideRow = gridopts.defaultRow.hideRow;
            if (defaultData && defaultData.r) {
                var defaultdatamap = opts.griddefaultassociate.defaultdatamap;
                var defaultrowmap = opts.griddefaultassociate.defaultrowmap;
                var defaulthiderowmap = opts.griddefaultassociate.defaulthiderowmap;
                if (defaultdatamap && defaultData) {
                    for (var r1 = 0, r2 = defaultData.r.length; r1 < r2; r1++) {
                        for (mapper in defaultdatamap) {
                            var defdataproperty = defaultdatamap[mapper];
                            var rproperty = mapper;
                            for (var v1 = 0, v2 = defaultData.r[r1].length; v1 < v2; v1++) {
                                if (defaultData.r[r1][v1].n == defdataproperty) {
                                    defaultData.r[r1][v1].v = r[rproperty];
                                }
                            }
                        }
                    }
                }
                if (defaultrowmap && defaultRow) {
                    for (mapper in defaultrowmap) {
                        var defdataproperty = defaultrowmap[mapper];
                        var rproperty = mapper;
                        for (rowpt in defaultRow) {
                            var rpt = rowpt.split('-');
                            if (rpt.length == 3) {
                                if (rpt[2] == defdataproperty) {
                                    defaultRow[rowpt] = r[rproperty];
                                }
                            }
                        }
                    }
                }
                if (defaulthiderowmap && defaultHideRow) {
                    for (mapper in defaulthiderowmap) {
                        var defdataproperty = defaulthiderowmap[mapper];
                        var rproperty = mapper;
                        for (rowpt in defaultHideRow) {
                            var rpt = rowpt.split('-');
                            if (rpt.length == 3) {
                                if (rpt[2] == defdataproperty) {
                                    defaultHideRow[rowpt] = r[rproperty];
                                }
                            }
                        }
                    }
                }
            }
        }

        //主单
        if (opts.associate) {

            //设置控件disable

            if (opts.associate.disableMap) {
                var thisvalue = $(this).combo("getValue");
                var map = opts.associate.disableMap;
                for (var targetid in map) {

                    var value = map[targetid];
                    var plugin = $('#' + targetid).attr('class') + "";
                    plugin = plugin.split(' ')[0].replace('easyui-', '');
                    var mfn = $.fn[plugin];
                    if (!mfn) continue;
                    var sd = undefined;
                    if (thisvalue) {
                        sd = value && r ? mfn.methods['disabled'] : mfn.methods['enabled'];
                    }
                    else {

                        sd = value || r ? mfn.methods['enabled'] : mfn.methods['disabled'];
                    }
                    if (!sd) continue;
                    sd($('#' + targetid));
                }
            }

            if (!r) {
                $(this).combo('setValue', '')
                opts.customValue ? '' : $(this).combo('setText', '');
            }
            r = r || {};




            for (var lab in opts.associate) {
                var fields = opts.associate[lab];
                if (!fields || !fields.length) continue;
                var control$ = $('#' + lab);
                var plugin = control$.GetEasyUIType();
                if (!plugin) continue;
                var valuefield = fields[0];
                if (valuefield) {
                    var sv = control$[plugin]('setValue', r[valuefield] || "");
                }
                var textfield = fields[1];
                if (textfield) {
                    var sv = control$[plugin]('setText', r[textfield] || "");
                }
            }


        }

        //表格
        var gridconfig = opts.gridassociate;
        if (gridconfig && gridconfig.gridId && gridconfig.map) {

            var map = gridconfig.map;
            var gridId = '#' + gridconfig.gridId, params = { map: map, treeData: r };


            if (gridconfig.append) {
                $(gridId).edatagrid('addRow', params);
            } else {

                if (opts.customValue && !r) {//可以自定义值 当录入的不在列表中的数据时


                    var gdata = $.data($(gridId)[0], "datagrid");
                    var dc = gdata.dc;
                    var view2 = dc.view2;
                    var selectElement = $(view2).find("tr.datagrid-row-selected");
                    if (!selectElement) return;
                    var selRow = $(gridId).datagrid("getSelected");
                    for (var field in map) {
                        var selRow = $(gridId).datagrid("getSelected");
                        var gridfields = map[field];
                        var $td = $($(selectElement).find("td[field='" + field + "']"));
                        var $div = $($td.find("div"));
                        if ($div) {
                            $div.attr("oaovalue", "");
                            selRow[field] = ""; $div.html(""); //增加这句是为了清除关联单元格的现实值

                        }
                    }
                    return;
                }
                if (r == null || r == undefined) {
                    var defaultData = $(gridId).edatagrid('options').defaultData;

                    if (defaultData && defaultData.r) {
                        r = $.fn.combogrid.methods.transDefaultData(defaultData.r[0]);
                    }
                }
                r = r || {};

                //libin 为了报销单列表的预算类型和是否项目正确显示
                if (opts.setprojectfunc == "true") {
                    if (r["GUID"]) {
                        r["BGTypeName"] = "项目支出";
                        r["GUID_BGType"] = "972d61af-bf77-490d-9a7f-c51bcac9d075";
                    }
                    else {
                        r["BGTypeName"] = "基本支出";
                        r["GUID_BGType"] = "683daeb0-6333-474e-a713-0c91d438cb81";
                    }
                }


                var gdata = $.data($(gridId)[0], "datagrid");
                var dc = gdata.dc;
                var view2 = dc.view2;
                var selectElement = $(view2).find("tr.datagrid-row-selected");
                if (!selectElement) return;
                for (var field in map) {

                    var selRow = $(gridId).datagrid("getSelected");

                    var gridfields = map[field];
                    var $td = $($(selectElement).find("td[field='" + field + "']"));
                    var $div = $($td.find("div"));

                    if ($div) {

                        if (gridfields.length == 1) {
                            var temp = r[gridfields[0]];
                            selRow[field] = temp || '';

                        } else if (gridfields.length == 2) {

                            var temp0 = r[gridfields[0]], temp1 = r[gridfields[1]];
                            $div.html(temp1 ? temp1 : "");
                            selRow[field] = temp1 || '';
                            $div.attr("oaovalue", temp0 ? temp0 : "");
                        }
                    }
                }
            }

            //                gridconfig.append? $(gridId).edatagrid('addRow',params):$(gridId).edatagrid('updateSelectedRow',params);
        }
    },
    clearValue: function () {
        ;
        $(this).combogrid('setValue', '');
        $(this).combogrid('setText', '');

    },
    transDefaultData: function (defaultData) {

        var result = {};
        if (defaultData == undefined || defaultData == null) return result;
        for (var i = 0, j = defaultData.length; i < j; i++) {
            var ditem = defaultData[i];
            result[ditem.n] = ditem.v;
        }
        result['IsFinance'] = result['IsGuoKu']
        return result;
    },
    clearMyself: function (opts) {
        var r = null;
        textField = opts.textField;
        var attr = "GUID_" + textField.substring(0, textField.indexOf("Name"));
        GUID = opts.idField;

        //表格
        var gridconfig = opts.gridassociate;
        if (gridconfig && gridconfig.gridId && gridconfig.map) {
            var map = gridconfig.map;
            map[gridconfig.gridId + "-" + attr] = [GUID, textField];
            var gridId = '#' + gridconfig.gridId, params = { map: map, treeData: r };
            r = r || {};
            var gdata = $.data($(gridId)[0], "datagrid");
            var dc = gdata.dc;
            var view2 = dc.view2;
            var selectElement = $(view2).find("tr.datagrid-row-selected");
            if (!selectElement) return;
            for (var field in map) {
                var selRow = $(gridId).datagrid("getSelected");

                var gridfields = map[field];
                var $td = $($(selectElement).find("td[field='" + field + "']"));
                var $div = $($td.find("div"));

                if ($div) {
                    if (gridfields.length == 1) {
                        var temp = r[gridfields[0]];
                        selRow[field] = temp || '';

                    } else if (gridfields.length == 2) {
                        var temp0 = r[gridfields[0]], temp1 = r[gridfields[1]];
                        $div.html(temp1 ? temp1 : "");
                        selRow[field] = temp1 || '';
                        $div.attr("oaovalue", temp0 ? temp0 : "");
                    }
                }
            }
        }

    },
    //转换当前可编辑状态

    alterStatus: function (jq, status) {
        return jq.each(function () {
            var id = $(this).attr('id');
            if (id == "parti-CN_PaymentNumber-GUID_BGResource") {
                debugger
                id = "a";
            }
            if (!status) return;
            var opts = $(this).combogrid('options');

            //绑定控件 一般的情况下只绑定一次就可以 
            //如果需要持续解绑 和 绑定 那么就设置 mustBind为true即可
            if (!opts.customBind || opts.mustBind)
                $.view.bind.call($(this), 'combogrid');
            if (opts.forbidstatus) {
                for (var i = 0; i < opts.forbidstatus.length; i++) {
                    var c = opts.forbidstatus[i];
                    if (c == -1 || c == status) {
                        if (!opts.disabled) {
                            $(this).combogrid('disabled');
                        }
                        return;
                    }
                }
            }
            if (opts.disabled) {
                $(this).combogrid('enabled');
            }
        });
    },
    setDisableMap: function (jq) {
        return jq.each(function () {
            var opts = $(this).combogrid('options');
            if (opts.associate && opts.associate.disableMap) {
                var thisvalue = $(this).combo("getValue");
                var map = opts.associate.disableMap;

                for (var targetid in map) {
                    var value = map[targetid];
                    var plugin = $('#' + targetid).attr('class') + "";
                    plugin = plugin.split(' ')[0].replace('easyui-', '');
                    var mfn = $.fn[plugin];
                    if (!mfn) continue;
                    if (thisvalue) {
                        value ? $('#' + targetid)[plugin]('disabled') : $('#' + targetid)[plugin]('enabled');
                    }
                    else {
                        value ? $('#' + targetid)[plugin]('enabled') : $('#' + targetid)[plugin]('disabled');
                    }

                }
            }
        })
    },
    disabled: function (jq) {
        $(jq).combo('disable');
    },
    enabled: function (jq) {
        $(jq).combo('enable');

    },
    getSelected: function (jq) {
        var g = $(jq).combogrid('grid');
        if (g) {
            return g.datagrid('getSelected');
        }
        return null;
    },
    getColumns: function (jq) {
        var cols = [];
        var dg = $(jq).combogrid('grid');
        if (dg) {
            return cols = $(dg).datagrid('getColumnFields');
        }
    },
    getValue: function (jq) {
        return $(jq).combo('getValue');
    },
    getText: function (jq) {
        return $(jq).combo('getText');
    },
    //获得格式数据
    getData: function (jq, isText) {
        var jq$ = $(jq),
        vals = jq$.attr('id');
        if (vals) {
            var vals = vals.split('-');
            if (vals) {
                var result = { m: vals[1], n: vals[2], v: jq$.combogrid('getValue') };
                if (isText) {
                    var opts = jq$.combogrid('options'), re = [], czzfm = opts.CZZFField; //textField
                    if (czzfm) {//财政支付码

                        var g = jq$.combogrid('grid'); // get datagrid object
                        var r = g.datagrid('getSelected'); // get the selected row、

                        re.push({ m: vals[1], n: czzfm, v: r ? r[opts.CZZFField] : '' });
                    }
                    if (opts.hideValue) {//只需要返回value值即可 跟datagrid有关
                        re.push({ m: vals[1], n: vals[2], v: jq$.combogrid('getValue') });
                    } else {
                        //显示值和隐藏值都要

                        re.push({ m: vals[1], n: vals[2], v: jq$.combogrid('getValue') });
                        re.push({ m: vals[1], n: opts.textField, v: jq$.combogrid('getText') });
                    }
                    return re;
                }
                return result;
            }
        }
        return null;
    },
    //加载格式数据
    setData: function (jq, hs) {

        return jq.each(function () {


            var idAttr = $(this).attr('id').split('-');

            if (!idAttr) return;
            if (hs) {
                var opts = $(this).combogrid('options');
                var a = hs[idAttr[1]];
                if (a && a.hasOwnProperty(idAttr[2])) {

                    if (hs.isNotTemp) {
                        //为了解决差旅报销单明细中选中项目返回时由于本地过滤儿显示项目GUID["clbxd", "BX_Main", "GUID_Project"]
                        if ((idAttr[0] == "clbxd" && idAttr[1] == "BX_Detail" && idAttr[2] == "GUID_Project")) {

                            var griddata = opts.remoteData;
                            if (griddata.originalRows) {
                                griddata = data.originalRows;
                            }
                            var grid = $(this).combogrid('grid');
                            grid.datagrid('loadData', griddata);
                        }
                        $(this).combogrid('setValue', a[idAttr[2]]);

                    } else {
                        opts.tempValue = a[idAttr[2]];
                    }
                    return;
                }
                return; //有些值不需要赋值

            }
            $(this).combogrid('setText', '');
            $(this).combogrid('setValue', '');
        })
    },
    //财政支付码

    setCCZFCode: function (jq) {

        var parms = $(this).combogrid('getParms', 'setCCZFCode');

        if (parms && parms.length > 7) {
            var bgTypeColID = parms[0], functionClassColID = parms[1], economyClassColID = parms[2],
        expendTypeColID = parms[3], bgSourceColID = parms[4], projectColID = parms[5], isGouKuColID = parms[6], curColID = parms[7];

            GetPayCode.call(this, bgTypeColID, functionClassColID, economyClassColID, expendTypeColID, bgSourceColID, projectColID, isGouKuColID, curColID)
        }

    },
    //财政支付码1 针对grid中的列的财政支付码的规则
    setCCZFCodeForGrid: function (jq) {
        var parms = $(this).combogrid('getParms', 'setCCZFCodeForGrid');
        if (parms.length < 4) return
        var scope = parms[0], gridId = '#' + parms[1], isGuoKu = parms[2], CFM = parms[3];
        var sRow = $(gridId).datagrid('getSelected');
        if (!sRow) return;
        var cols = parms.col || ['ExtraCode', 'FinanceCode', 'EconomyClassKey', 'ExpendTypeKey', 'BGSourceKey', ''];

    },
    setCCZFCodeForPage: function (jq) {
        
        var opts = $(this).combogrid('options');
        var scope = opts.scope;
        var isGouKu = $('#' + scope + '-CN_PaymentNumber-IsGuoKu').checkbox('getValue');
        if (!isGouKu) {//是 国库才能生成财政支付码

            return;
        }
        var colId = [
            '-CN_PaymentNumber-IsProject',
            '-CN_PaymentNumber-FinanceCode',
            '-CN_PaymentNumber-EconomyClassKey',
            '-CN_PaymentNumber-ExpendTypeKey',
            '-CN_PaymentNumber-BGSourceKey',
            '-BX_Detail-ExtraCode'
          ];
        var czzfCode = "";
        //是否项目
        var value = $('#' + scope + colId[0]).val();
        if (value == "0" || value == "false" || value == "False" || value == false) {
            czzfCode = "1";
        }
        else {
            czzfCode = "2";
        }
        //功能分类
        value = $('#' + scope + colId[1]).val();
        //FinanceCode
        czzfCode += GetFormat0(value, 7);
        //经济分类
        value = $('#' + scope + colId[2]).val();
        czzfCode += value;
        //支出类型
        value = $('#' + scope + colId[3]).val();
        czzfCode += value;
        //预算来源bgSourceColID
        value = $('#' + scope + colId[4]).val();
        czzfCode += value;
        //项目扩展码

        value = $('#' + scope + colId[5]).val();
        value ? czzfCode += value : czzfCode += "000";
        var temp = $('#' + scope + '-CN_PaymentNumber-PaymentNumber');
        (czzfCode && czzfCode.length == 16) ? temp.val(czzfCode) : temp.val('');
        function GetFormat0(value, digit) {
            var attr = [];
            if (value && $.trim((value + "")).toUpperCase() != "NULL") {
                if (value.length > digit) {
                    value = value.substr(0, digit);
                    attr.push(value);
                } else {
                    attr.push(value);
                    for (var i = 0; i < digit - value.length; i++) {
                        attr.push("0")
                    }
                }
            } else {
                for (var i = 0; i < digit; i++) {
                    attr.push("0")
                }
            }
            return attr.join('');
        }
    },
    //固定方法 当功能分类选择（项目未选）的时候 是否国库为选中
    setIsGK: function () {

        var opts = $(this).combogrid('options');
        var parms = $(this).combogrid('getParms', 'setIsGK');
        if (!parms || !parms[0]) return;
        if (opts.disabled) {
            $('#' + parms[0]).checkbox('disable');
        } else {
            var val = $(this).combogrid('getValue');
            if (val) {

                $('#' + parms[0]).checkbox('setValue', true);
                $('#' + parms[0]).checkbox('disable');
                if (parms[1]) {
                    $('#' + parms[1]).checkbox('disable');
                }
            } else {
                $('#' + parms[0]).checkbox('enable');
                $('#' + parms[0]).checkbox('setValue', false);
                if (parms[1]) {
                    $('#' + parms[1]).checkbox('enable');
                    $('#' + parms[1]).checkbox('setValue', false);
                }
            }
        }

    },
    getParms: function (jq, methodName) {
        var opts = $(jq).combogrid('options'), funConfig = opts.bindparms;
        if (funConfig && funConfig[methodName]) {
            return funConfig[methodName];
        }
        return null;
    },
    //加载数据到本地

    loadRemoteDataToLocal: function (jq) {
        return jq.each(function () {
            var combogrid = $(this);
            var opts = combogrid.combogrid('options');
            var isLoad = opts.isLoad;

            if (!opts.remoteData.length && opts.remoteUrl) {
                if (opts.pagination) {
                    var gridOpts = combogrid.combogrid('grid').datagrid("options");
                    gridOpts.loadFilter = $.fn.datagrid.methods["pagerFilter"];
                }

                var cols = combogrid.combogrid('getColumns');

                var remoteUrl = opts.remoteUrl;
                //为了提现单明细中加载支票号用 libin

                if (opts.appendcurcheckvalue == '1') {
                    var checkguid = ""; var selectcheckguid = ""; var mtype = 1;
                    var selectrows = $.view.detailObj.data.r;
                    if (selectrows == null || selectrows == undefined || selectrows.length == 0) {
                        var datas = $('#' + opts.gridassociate.gridId).edatagrid('getDataDetail');
                        if (datas == undefined || datas == null) {
                            return;
                        }
                        var rowindex = parseInt(combogrid.parents("tr[class^='datagrid-row']").attr("datagrid-row-index"));

                        selectrows = [datas.r[rowindex]];
                        mtype = 2;
                    }
                    if (selectrows) {
                        for (var j = 0; j < selectrows.length; j++) {
                            var selectrow = selectrows[j];
                            if (selectrow) {
                                for (var i = 0; i < selectrow.length; i++) {
                                    var m = selectrow[i].m.toLowerCase();
                                    var n = selectrow[i].n.toLowerCase();
                                    if (m == 'cn_check' && n == 'guid') {
                                        var cv = selectrow[i].v;
                                        if (cv) {

                                            if (mtype = 1) {
                                                if (j == $.view.detailObj.cur - 1) {
                                                    selectcheckguid = cv;
                                                }
                                            }
                                            else {
                                                selectcheckguid = cv;
                                            }
                                            checkguid = checkguid + cv + ",";
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    if (checkguid) {
                        checkguid = checkguid.substr(0, checkguid.length - 1);
                        remoteUrl = remoteUrl + "?checkguid=" + checkguid;
                    }
                }



                $.ajax({
                    url: remoteUrl,
                    data: { "filter": cols },
                    dataType: "json",
                    type: "POST",
                    traditional: true,
                    success: function (data) {
                        opts.remoteData = data;
                        var g = combogrid.combogrid('grid');
                        g.datagrid({ 'onLoadSuccess': function () {

                            //赋值

                            if (opts.tempValue) {
                                combogrid.combogrid('setValue', opts.tempValue);
                                if (opts.isLoadSetData) {//一开始加载就走联动 
                                    $.fn.combogrid.methods["setAssociate"].call(g);
                                }
                                combogrid.combogrid('setDisableMap');
                                opts.tempValue = "";
                            }
                        }
                        });

                        //初始化时判断是否需要加载数据sxh
                        if (isLoad != false) {

                            g.datagrid('loadData', data);

                        }
                        if (opts.associateControl) {
                            var single = $(combogrid).combogrid('gridassociateControl');
                            //如果过滤后结果为空则显示全部的话就不注释接下来的一行代码，否则就注释掉接下来的一行代码

                            //                            if (!single) g.datagrid('loadData', data);

                            //为了提现单明细中加载支票号用 libin
                            if (selectcheckguid && opts.appendcurcheckvalue == '1') {
                                var datas = g.datagrid('getData');
                                if (datas) datas = datas.originalRows;
                                for (var dindex = 0; dindex <= datas.length; dindex++) {
                                    var cdata = datas[dindex];

                                    if (cdata["GUID"] == selectcheckguid) {
                                        var pagernumber = parseInt((dindex + 1) / opts.pageSize) + 1;
                                        var rowindex = ((dindex + 1) % opts.pageSize) - 1;
                                        var pager = g.datagrid('getPager');
                                        pager.pagination('select', pagernumber);
                                        break;
                                    }
                                }
                            }
                        }

                    }
                });
            } else {

                var guid = opts.tempValue;
                combogrid.combogrid('setPageNumber', guid);

                combogrid.combogrid('setValue', guid);
                combogrid.combogrid('setDisableMap');
                opts.tempValue = "";
            }
        });
    },
    setControlFun: function () {

        var parms = $(this).combogrid('getParms', 'setControlFun');
        if (!parms) return;
        var funs = parms;
        for (var colId in funs) {

            var l = funs[colId];
            if (l.length != 2) continue;
            var c = $('#' + colId);
            if (!c) continue;
            $.fn[l[0]].methods[l[1]].call(c);
        }
    },
    setPageNumber: function (jq, guid) {

        //求当前操作员所在第几页
        var getPageNumberByGuid = function (guid, rows, pageSize) {
            if (!rows) return 1;
            var rowIndex = 0;
            var pgIndex = 0;
            for (var i = 0, j = rows.length; i < j; i++) {
                var row = rows[i];
                if (row.GUID == guid) {
                    rowIndex = (i + 1);
                    break;
                }
            }
            if (rowIndex % pageSize == 0) {
                pgIndex = div(rowIndex, pageSize);
            } else {
                pgIndex = div(rowIndex, pageSize) + 1;
            }

            return pgIndex;
        }
        //整除函数
        var div = function (exp1, exp2) {
            var n1 = Math.round(exp1); //四舍五入
            var n2 = Math.round(exp2); //四舍五入
            var rslt = n1 / n2;
            if (rslt >= 0) {
                rslt = Math.floor(rslt); //返回值为小雨等于其数值参数的最大整数值




            } else {
                rslt = Math.ceil(rslt); //返回值为大于等于其数值参数的最小整数值




            }
            return rslt;
        }
        var comob = $(jq);
        var cOpts = comob.combogrid('options');
        if (!cOpts.pagination || !cOpts.remoteData) return;
        var dg = comob.combogrid('grid');
        var dataRows = dg.datagrid('getRows');
        var pager = dg.datagrid('getPager');
        var opts = pager.pagination('options')
        var curPage = getPageNumberByGuid(guid, cOpts.remoteData, parseInt(opts.pageSize));
        if (opts.pageNumber != curPage) {
            pager.pagination('select', curPage);
        }
    },
    //验证combogrid
    verifyData: function () {

        var txt = $(this).combo('textbox');
        txt.required = true;
        var msg = "";
        var TempVal = $(this).combogrid('getValue');
        if (TempVal == "") {
            var id = $(this).attr('id');
            var labid = "#lbl-" + id.split('-')[2];
            var labelVal = $(labid).text().replace('*', '');
            msg = $.trim(labelVal) + "不能为空！";
        }
        return msg;
    },
    //根据所选科目带出科目摘要信息

    gridassociateControl: function (jq) {
        var opts = $(jq).combogrid('options');
        var remoteData = opts.remoteData;
        var gridId = "#" + opts.associateControl.gridId;
        var map = opts.associateControl.map;
        var tempData = [];
        var selRow = $(gridId).datagrid("getSelected");
        if (!selRow) return;
        var gdata = $.data($(gridId)[0], "datagrid");
        var dc = gdata.dc;
        var view2 = dc.view2;
        var selectElement = $(view2).find("tr.datagrid-row-selected");
        if (!selectElement) return;
        for (var field in map) {
            var fields = map[field];
            if (!selRow[fields]) continue;
            var id = fields.split('-')[2];
            var $td = $($(selectElement).find("td[field='" + fields + "']"));
            var $div = $($td.find("div"));
            if ($div) {
                var oaovalue = $div.attr("oaovalue");
                if (!oaovalue) return;
                for (var i = 0, j = remoteData.length; i < j; i++) {
                    if (oaovalue == remoteData[i][id]) {
                        tempData.push(remoteData[i]);
                    }
                }
            }
        }
        var g = $(jq).combogrid('grid');
        g.datagrid('loadData', tempData);
        var bsingle = tempData.length > 0 ? true : false;
        return bsingle;
    }
});
$.extend($.fn.combogrid.defaults, {
    backValue: "", //后台字段
    filterField: "", //过滤列(只支持英文)
    scope: null,
    forbidstatus: null,
    bindmethod: null, //{ 'onSelect': ['setAssociate'] }
    associate: null, //{'label1':['field1','field2'],'label2':['field1','field2']}
    gridassociate: null, //{gridId:'g1',map:{'a1':'v1','a2':'v2'},append:false}
    remoteData: [], //远程获得的数据

    remoteSort: false,
    url: null,
    remoteUrl: null, //远程url
    //数据基础上过滤

    //动态过滤

    query: function (q) {
        var opts = $(this).combogrid('options');

        var data = opts.remoteData;
        if (data.originalRows) {
            data = data.originalRows;
        }
        var filterField = opts.filterField
        if (!filterField) return;
        var filterField = filterField.split(',');
        var d = [], t1 = {};
        for (var m = 0, n = data.length; m < n; m++) {

            var c = function (r1) {
                var t11 = 0, w; //返回匹配项  w 看看是否匹配
                for (var i = 0; i < filterField.length; i++) {
                    q = q || "";
                    var t =(r1[filterField[i]]||"").toLowerCase().indexOf(q.toLowerCase());
                    if (!w && q == r1[filterField[i]]) w = 1;
                    t11 += t > -1 ? (t + 1) : 0;
                }
                return { t: t11, w: w };
            }
            t1 = c(data[m]);
            if (t1.t > 0) {
                if (t1.w == 1) { d.unshift(data[m]); } else d.push(data[m]);
            }
        }
        var grid = $(this).combogrid('grid');
        grid.datagrid({ 'onLoadSuccess': function () {

            grid.datagrid('clearSelections');
            //            if (w == 1) {//d.length == 1 ||放开就是有一行的时候也默认选中 

            //                grid.datagrid('selectRow', 0);
            //            }
            //赋值

            if (opts.tempValue) {
                grid.combogrid('setValue', opts.tempValue);
                if (opts.isLoadSetData) {//一开始加载就走联动 
                    $.fn.combogrid.methods["setAssociate"].call(combogrid);
                }
                grid.combogrid('setDisableMap');
                opts.tempValue = "";
            }
        }
        });
        grid.datagrid('loadData', d.length == 0 ? data : d);
        var matchall = function (r1) {
            var w; //返回匹配项  w 看看是否匹配
            for (var i = 0; i < filterField.length; i++) {

                if (!w && q == r1[filterField[i]]) w = 1;

            }
            return w;
        }
        if (d.length == 1) {
            var cw = matchall(d[0]);
            if (cw) {
                grid.datagrid("selectRow", 0);
            }
        }
        //        if () {
        //            setTimeout(function () {
        //                try {
        //                  
        //                } catch (e) {

        //                }
        //            }, 2000);
        //        }
    }
});
