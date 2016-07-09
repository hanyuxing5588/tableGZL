$.extend($.fn.tree.methods, {
    setAss: function (node) {
        var opts = $(this).tree('options');
        var status = $.view.getStatus(opts.scope); //根据scope获取当前的status
        var con = opts.associate;                  //获取tree的associate属性
        //定义模型
        var mModel = opts.m;
        if (status == '4' || status == '2') {//这个是页面所处于的当前状态
            if (con) {
                var r = $(this).tree('getSelected');
                //得到当前对象模型
                var rModel = r.attributes.m;
                if (mModel == rModel) {
                    if (r && r.attributes && r.attributes.valid) {
                        for (var lab in con) {
                            var validFied = con[lab][0];
                            var textField = con[lab][1];
                            if (!textField) {
                                textField = validFied;
                            }
                            var control = $('#' + lab);
                            var plugin = control.GetEasyUIType();
                            var mfn = $.fn[plugin];
                            if (mfn) {
                                var sv = mfn.methods['setValue'];
                                if (sv) {
                                    var guid = r.attributes[validFied];
                                    if (plugin == "combogrid")//为了有时候分页加载当前页数据
                                    {
                                        control[plugin]('setPageNumber', guid);
                                    }
                                    sv($('#' + lab), guid);
                                }
                                var st = mfn.methods['getText'];
                                if (st) {
                                    st(control, r.attributes[textField]);
                                }
                            }
                        }
                    }
                    $.fn.tree.methods["setAssoAfter"](r);
                }
            }
            //向grid中传值
            var treeId = $('#gzjhrysz-tree-gpro');
            var gridId = $('#gzjhrysz-SA_PlanPersonSet');
            var opts = treeId.tree('options');
            var assicId = opts.gridassociate.associateId;   //要传的GUID
            var scope = opts.scope;
            $.ajax({
                url: '/JCxcsz/Retrievegzjhrysz',
                dataType: "json",
                type: "post",
                data: { id: node.id },
                success: function (data) {
                    if (data.d.length > 0) {
                        $.view.loadData(scope, data);
                    }
                    else {
                        $.view.clearView(scope);
                    }
                }
            });
        }
    }
});

//扩展方法
//遍历datagrid中的checkbox，如果返回的数据不为空，则返回true，否则为false
$.extend($.fn.linkbutton.methods, {
    //点击修改的时候去修改页面控件的状态
    setStatus: function () {
        var opts = $(this).linkbutton('options');
        var gridId = '#' + opts.gridId;
        var treeId = '#' + opts.treeId;
        var treeOpts = $(treeId).tree('options');
        var scope = treeOpts.scope;
        var retStatus = treeOpts.retStatus;
        var msg = treeOpts.msg;
        var msgtext = "修改";
        if ($(treeId).tree('getSelected') == null) {
            $.messager.alert('提示', '请选择要' + msgtext + '的' + msg + '！');
            $.view.setViewEditStatusJC(scope, retStatus);
            return false;                     //不成功返回false
        } else {
            //如果选择要修改的项，那么将datagrid的选中事件更改，只能checkbox选中，没有选中行
            //给datagrid设置属性
            $(gridId).datagrid({ checkOnSelect: $(gridId).is(':checked') });
            $(gridId).datagrid({ selectOnCheck: $(gridId).is(':checked') });
            $(gridId).datagrid({ singleSelect: true });
            $(gridId).datagrid('checkRow');
        }
        var status = $.view.getStatus(opts.scope);
        if (status == 4) {//页面状态时固定的，如果等于当前状态，就改变
            $(this).linkbutton('setWholeStatus');   //改变状态时，更改控件是否编辑
            $(this).linkbutton('saveStatus');
        }
    },
    TreeChecked: function () {
        var opts = $(this).linkbutton('options');
        var but = opts.quanxuan;
        var data = $.data($('#gzjhrysz-SA_PlanPersonSet')[0], "datagrid").data;
        if (!data) return;
        data = data.originalRows;
        for (var i = 0, j = data.length; i < j; i++) {
            var row = data[i];
            row['checkbox'] = but ? true : undefined;
            row['b-sel'] = but ? true : undefined;
        }
        $('#gzjhrysz-SA_PlanPersonSet').datagrid('loadData', data);
    }
});

$.extend($.fn.combobox.methods, {
    //选择设置方式
    onSel: function () {
        var ItemTypeId = $("#gzjhrysz-SA_Plan-ItemType");
        var BankNameId = $("#gzjhrysz-SA_Plan-BankName");
        var value = $(ItemTypeId).combobox('getValue');
        switch (value) {
            case "1":
                $(BankNameId).combogrid('disabled');
                break;
            case "2":
                $(BankNameId).combogrid('enabled');
                break;
        }
    }
});

$.extend($.fn.combogrid.methods, {
    //根据选择的银行信息，进行批量设置
    BankInfo: function () {
        var selectRow = $(this).combogrid('grid').datagrid('getSelected');
        var opts = $('#gzjhrysz-SA_PlanPersonSet').edatagrid('options');
        var data = $.data($('#gzjhrysz-SA_PlanPersonSet')[0], "datagrid").data;
        if (!data) return;
        data = data.originalRows;
        for (var i = 0, j = data.length; i < j; i++) {
            var row = data[i];
            row["gzjhrysz-gzjhryszModel-BankKey"] = selectRow["BankKey"];
            row["gzjhrysz-gzjhryszModel-BankName"] = selectRow["BankName"];
            row["gzjhrysz-gzjhryszModel-GUID_SS_Bank"] = selectRow["GUID"];
        }
        $('#gzjhrysz-SA_PlanPersonSet').datagrid('loadData', data);

    }
});

$.extend($.fn.datagrid.defaults.editors, {
    combogrid: {
        init: function (container, options) {
            var input = $("<input type=\"text\" class=\"datagrid-editable-combogrid\" />").appendTo(container);
            input.combogrid(options);
            //绑定事件
            $.view.bind.call($(input), 'combogrid');
            //加载远程数据
            $(input).combogrid('loadRemoteDataToLocal');
            return input;
        },
        destroy: function (target) {
            $(target).combogrid('destroy');
        },
        getValue: function (target) { //回到grid
            var text = $(target).combogrid('getText');
            return text;
        },
        setValue: function (target, value) {//从grid的来
            $(target).combogrid('setValue', value)
            return $(target).combogrid('setText', value);
        },
        resize: function (target, width) {
            $(target).combogrid("resize", width);
        }
    }
});

$.extend($.fn.edatagrid.methods, {  //前台页面列表值转换成后台值
    transGridDataToBackstage: function (jq, rows) {
        alert(1);
        var data = rows || $(jq).datagrid('getRows');
        var result = [], ritem;
        var showAndHide = $(jq).edatagrid('getColumnMapping', true);

        if (data) {
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                if (!item.checkbox) continue;
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
    transGridDataFromBackstage: function (jq, params) {//gai

        var showAndHide = $(jq).edatagrid('getColumnMapping', true);
        var showAndHide1 = $(jq).edatagrid('getColumnMapping', false);
        var cols = $(jq).datagrid("getColumnFields", true).concat($(jq).datagrid("getColumnFields", false));
        var scope = params.s, model = params.m, backstageData = params.d, defalut = params.f;
        var result = { rows: [], hideRows: [] }, ritem, rhideItem, nattr;
        if (scope && backstageData) {
            for (var i = 0; i < backstageData.length; i++) {
                var item = backstageData[i];
                if (item) {
                    ritem = {}, rhideItem = {};
                    for (var j = 0; j < item.length; j++) {
                        var col = item[j];
                        nattr = scope + "-" + col.m + "-" + col.n;
                        if (nattr == 'gzjhrysz-gzjhryszModel-GUID' && col.v) {
                            ritem['checkbox'] = true;
                            ritem['b-sel'] = true;
                        }
                        if (showAndHide[nattr]) {
                            var sattr = showAndHide[nattr];
                            if (sattr == col.n) {
                                ritem[nattr] = col.v;
                            }
                            rhideItem[nattr] = col.v;
                        }
                        else if (showAndHide1[col.n]) {
                            var c = showAndHide1[col.n].split('-');
                            if (c[1] == col.m) {
                                ritem[showAndHide1[col.n]] = col.v;
                            }
                        }
                        else {
                            if (cols.indexOf(nattr) < 0) continue;
                            ritem[nattr] = col.v;
                        }
                    }
                }
                if (defalut && defalut.row) {
                    ritem = $.extend({}, defalut.row, ritem);
                }
                if (defalut && defalut.hideRow) {
                    rhideItem = $.extend({}, defalut.hideRow, rhideItem);
                }
                result.rows.push(ritem);
                result.hideRows.push(rhideItem);
            }
        }
        return result;
    }
});

$(document).ready(function () {
    //给设置方式加一个默认值
    var datagridObj = $('#gzjhrysz-SA_PlanPersonSet');
    datagridObj.datagrid({
        onCheck: function (rowIndex, rowData) {
            
            rowData['checkbox'] = true;
            rowData['b-sel'] = true;
        },
        onUncheck: function (rowIndex, rowData) {
            
            rowData['checkbox'] = false;
            rowData['b-sel'] = false;
        },
        onCheckAll: function (rows) {
            for (var i = 0; i < rows.length; i++) {
                
                rows[i]['checkbox']= true;
                rows[i]['b-sel'] = true;
            }
        },
        onUncheckAll: function (rows) {
            for (var i = 0; i < rows.length; i++) {
                
                rows[i]['checkbox'] = false;
                rows[i]['b-sel'] = false;
            }
        }
    });
    $("#gzjhrysz-SA_Plan-ItemType").combobox('setValue', 1);
});
    



