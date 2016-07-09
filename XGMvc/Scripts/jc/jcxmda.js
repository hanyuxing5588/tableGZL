/*基础*/

/*
基础----初始化页面----status默认是4
*/
$.extend($.view, {
    init: function (scope, status, dataguid) {

        if (scope && status) {

            status = status + "";
            var id = "[id^='" + scope + "-'].easyui-linkbutton";
            switch (status) {
                case "1": //新建
                    var data = $.view.retrieveDoc(dataguid, scope);
                    $.view.loadData(scope, data);
                    $(id).linkbutton('bind', true);
                    $('#' + scope + '-add').click();
                    break;
                case "2": //修改
                    var data = $.view.retrieveDoc(dataguid, scope);
                    if (!data) return;
                    //判断拿到的grid是treegrid还是datagrid
                    var gridType = $('#' + dataguid).GetEasyUIType();
                    if (gridType == "treegrid") {
                        //下面只针对预算科目设置的
                        $('#' + dataguid).treegrid('loadData', { rows: data });
                        $.view.setViewEditStatus(scope, status);
                        $.view.cancelObj = { data: data, status: status };
                    } else {
                        $.view.loadData(scope, data);
                        $.view.setViewEditStatus(scope, status);
                        $.view.cancelObj = { data: data, status: status };
                    }
                    break;
                case "3": //删除
                    //设置页面编辑状态
                case "4": //浏览

                    var data = $.view.retrieveDoc(dataguid, scope);
                    if (!data) return;
                    $.view.loadData(scope, data);
                    //修改页面状态
                    $.view.setViewEditStatus(scope, status);
                    $.view.cancelObj = { data: data, status: status };
                    break;
            }
        }

        var projectopts = $('#xmda-SS_Project').datagrid('options');
        var projectcon = projectopts.associateEx;
        $('#xmda-SS_Project').datagrid({
            onSelect: function (rowIndex, rowData) {
                if (!rowData) return;
                var status = $.view.getStatus("xmda");
                if ([1, 2].exist(status)) {
                    var ProjectClassId = rowData["xmda-SS_Project-GUID_ProjectClass"];
                    var ProjectClassName = rowData["xmda-SS_Project-ProjectClassName"];
                    var ProjectClassKey = rowData["xmda-SS_Project-ProjectClassKey"];
                    var ProjectId = rowData["xmda-SS_Project-GUID"];
                    var ProjectName = rowData["xmda-SS_Project-ProjectName"];
                    var ProjectKey = rowData["xmda-SS_Project-ProjectKey"];

                    $('#xmda-SS_Project-PKey').combogrid('setValue', ProjectId);
                    $('#xmda-SS_Project-PKey').combogrid('setText', ProjectKey);
                    $('#xmda-SS_Project-PName').combogrid('setValue', ProjectId);
                    $('#xmda-SS_Project-PName').combogrid('setText', ProjectName);
                    $('#xmda-SS_Project-ProjectClassKey').combogrid('setValue', ProjectClassId);
                    $('#xmda-SS_Project-ProjectClassKey').combogrid('setText', ProjectClassKey);
                    $('#xmda-SS_Project-ProjectClassName').combogrid('setValue', ProjectClassId);
                    $('#xmda-SS_Project-ProjectClassName').combogrid('setText', ProjectClassName);
                    $('#xmda-SS_Project-PGUID').val(ProjectId);
                    $('#xmda-SS_Project-GUID_ProjectClass').val(ProjectClassId);
                }
                else {
                    var r = rowData;
                    if (projectcon && rowData) {
                        for (var lab in projectcon) {
                            var validFied = projectcon[lab][0];
                            var textField = projectcon[lab][1];
                            if (!textField) {
                                textField = validFied;
                            }
                            var control = $('#' + lab);
                            var plugin = control.GetEasyUIType();
                            var mfn = $.fn[plugin];
                            if (mfn) {
                                var sv = mfn.methods['setValue'];
                                if (sv) {
                                    var guid = r[validFied];
                                    if (plugin == "combogrid")//为了有时候分页加载当前页数据
                                    {
                                        control[plugin]('setPageNumber', guid);
                                    }
                                    sv($('#' + lab), guid);
                                }
                                var st = mfn.methods['getText'];
                                if (st) {
                                    st(control, r[textField]);
                                }
                            }
                        }
                    }
                }
            }
        });



    },

    //获取已知单据的所有信息
    retrieveDoc: function (guid, scope) {
        //collerName = 'JCzzjg';
        //在这个地方定义一个当前页面的scope，传给url var url = '/' + scope + '/Retrieve' + scope;
        var data;
        var collerName = $('#initController').val(); //取控制器的名称(根据控制器寻找所要的Controller)
        if (scope) {
            var url = '/' + collerName + '/Retrieve' + scope;
            $.ajax({
                url: url,
                async: false, //同步
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
    },
    //清除页面数据
    clearViews: function (scope, region) {
        if (!scope) return;
        var types = ["validatebox", "datebox", "checkbox", "numberbox", "combogrid", "combobox"], preselector;
        region ? preselector = '#' + scope + "-" + region + " [id^='" + scope + "-']" : preselector = "[id^='" + scope + "-']";
        preselector += ".easyui-";
        for (var i = 0, j = types.length; i < j; i++) {
            var type = types[i];
            $(preselector + type)[type]('setData');
        }
        $("#" + scope + "delrecordtemp").remove();
    },
    //在什么情况下不需要从新加载Combogrid
    //如果在停用或启用，删除或修改时，不对页面的Combogrid进行操作
    setViewEditStatusJC: function (scope, status) {
        if (!scope || !status) return;
        var types = ["linkbutton", "datebox", "combogrid", "combobox", "validatebox", "numberbox", "edatagrid", "datagrid", "checkbox", "tree"];
        var preselector = "[id^='" + scope + "-'].easyui-";
        for (var i = 0, j = types.length; i < j; i++) {
            var type = types[i];
            var id = "[id^='" + scope + "-'].easyui-" + type;
            $(id)[type]("alterStatus", status);
        }
        this.setStatus(scope, status);
        this.setPageCancleState(scope);
    }

});
/*
基础----status为4，点击新增的时候，只改变页面编辑状态，处于新增状态。*/
$.extend($.fn.linkbutton.methods, {
    newStatus: function () {
        var opts = $(this).linkbutton('options');
        var status = $.view.getStatus(opts.scope);
        if (status == 4) {//页面状态时固定的，如果等于当前状态，就改变
            $(this).linkbutton('setWholeStatus');   //改变状态时，更改控件是否编辑
            $(this).linkbutton('saveStatus');
            $.view.clearViews(opts.scope);    //清空页面数据，除datagrid
        }
    },

    //根据scope改变控件状态
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
        $("[id^='" + opts.scope + "'].easyui-datagrid").edatagrid('alterStatus', opts.status);
        $("[id^='" + opts.scope + "'].easyui-tree").tree('alterStatus', opts.status);
    },
    //验证通过返回true 不通过 false
    examine: function (jq) {//先获取当前tree

        var opts = $(jq).linkbutton('options');
        var scope = opts.scope;
        //得到当前是处于修改还是删除

        var status = $.view.getStatus(opts.scope);
        //下面这个变量是针对页面的status为2的时候，对保存进行验证
        var msgtext;
        if (status == '2') {
            msgtext = '修改';
        }
        if (status == '3') {
            msgtext = '删除';
        }

        var pageState = $.view.getStatus(scope);
        if (pageState == "1") return true;
        //拿到页面当前正在显示的项目
        var curProjectId = $('#xmda-SS_Project-GUID').val();
        if (!curProjectId) {
            $.messager.alert('提示', '请选择要' + msgtext + '的项目档案！');
            $.view.setViewEditStatusJC("xmda", "4");
            return false;                     //返回后页面还是处于浏览状态
        }

        return true;
    },

    //参数：selected:传入的this，表示当前点击的组件;    treeMenuId：要操作的tree的Id
    //全选‘全消
    TreeChecked: function () {
        var opts = $(this).linkbutton('options');
        var gridId = opts.gridId;
        var but = opts.quanxuan;
        //拿到所有的datagrid的Id
        var gridIdArr = '#' + gridId;
        var getCkTree = $(gridIdArr).datagrid('getChecked');
        //如果为true，则全选,否则取消
        if (but) {
            $(gridIdArr).datagrid('checkAll');
            //            var currtreeId = '#' + gridId;
            //            //返回当前grid的所有行
            //            var roots = $(currtreeId).datagrid('getRows');
            //            if (!opts.selected) {
            //                for (var k = 0; k < roots.length; k++) {
            //                    //查找节点
            //                    //var node = $(currtreeId).datagrid('selectAll', roots[k].id);
            //                    //将得到的节点选中
            //                    //$(currtreeId).datagrid('check', node.target);
            //                    $(currtreeId).datagrid('selectAll');
            //                }
            //            }
        }
        else {
            $(gridIdArr).datagrid('uncheckAll');
            //var currtreeId = '#' + gridId;
            //返回tree的所有根节点数组
            //            var roots = $(currtreeId).datagrid('getRows');
            //            if (!opts.selected) {
            //                for (var k = 0; k < roots.length; k++) {
            //                    //查找节点
            //                    //var node = $(currtreeId).datagrid('find', roots[k].id);
            //                    //将得到的节点取消
            //                    //$(currtreeId).datagrid('uncheck', node.target);
            //                    $(currtreeId).datagrid('uncheck', node.target);
            //                }
            //            }
        }
    },
    //重写afterSave方法，去完成保存成功后的处理
    //重写单据保存成功后 重新加载控件
    afterSave: function (status, isSuccess) {
        var opts = this.linkbutton('options');
        var treeId = opts.treeId;
        var scope = opts.scope;
        //得到当前是按钮状态(修改或删除)
        var status = $.view.getStatus(opts.scope);
        //自定义状态
        var retStatus = $('#' + treeId).tree('options').retStatus
        //得到返回的状态信息，如果错误测不修改页面状态
        if (isSuccess == 'error') {
            if (status == '3') {
                //删除错误时，则重置页面
                $.view.setViewEditStatusJC(scope, 4);
            } else {
                //返回错误时直接return，不更改当前页面状态,针对修改
                return;
            }
        }
        else {
            for (var i = 0, j = treeId.length; i < j; i++) {
                var currTreeId = treeId[i];
                $('#' + currTreeId).tree('reload');
            }
            //重新reLoadCombogrid中的数据
            var allComboGrid = $(".easyui-combogrid");
            for (var i = 0, j = allComboGrid.length; i < j; i++) {
                var combo = $('#' + allComboGrid[i].id);
                var optsCombo = combo.combogrid('options');
                //optsCombo.remoteData = [];
                //combo.combogrid('loadRemoteDataToLocal');
            }
            $.view.setViewEditStatus(scope, retStatus);
            $.view.clearViews(opts.scope);
        }
    },
    //停用、启用方法
    setEnable: function (node) {
        //在点击停用或启用之前要验证是否选择要停用或启用的数据
        //得到页面状态
        //拿到setEnable中的两个对象
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'setEnable');
        if (parms.length < 3) return;
        //得到是否停用的id
        var controlId = parms[0];
        //得到是停用还是启用
        var isEnable = parms[1];
        var treeId = '#' + parms[2];
        var scope = $(treeId).tree('options').scope;
        var selTree = $(treeId).tree('getSelected');
        if (selTree == null) {
            if (isEnable == '1') {
                $.messager.alert("提示", '请选中要停用的对象！', 'info');
                $.view.setViewEditStatus(scope, 4);
                return;
            }
            else {
                $.messager.alert("提示", '请选中要启用的对象！', 'info');
                $.view.setViewEditStatus(scope, 4);
                return;
            }
        } else {
            //是否停用 || 是否启用
            var IsStopStatus = selTree.attributes.IsStop;
            if (isEnable == "1") {
                if (IsStopStatus == "True") {
                    $.messager.alert('提示', '该对象已停用！', 'info');
                    $.view.setViewEditStatusJC(scope, 4);
                    return;
                }
            } else {
                if (IsStopStatus == "False") {
                    $.messager.alert('提示', '该对象已启用！', 'info');
                    $.view.setViewEditStatusJC(scope, 4);
                    return;
                }
            }
        }
        //改变状态前要先判断当前节点下的所有子节点中是否还有没有停用的对象
        //IsStop==false     得到所有的attribute属性
        if ($(treeId).tree('isLeaf', node.target)) {
            if (selTree) {
                //当前节点下的所有节点
                var getTreeList = $(treeId).tree('getChildren', selTree.target);
                var isStop = false;
                for (var i = 0; i < getTreeList.length; i++) {
                    var strisStop = getTreeList[i].attributes.IsStop;
                    if (strisStop == "True") {
                        isStop = true;
                    } else {
                        isStop = "False";
                        if (isStop == "False") {
                            $.messager.alert('提示', '该级还有未停用的下级，不允许停用！', 'info');
                            $.view.setViewEditStatus(scope, 4);
                            return;
                        }
                    }
                }
                //改变其状态
                $('#' + controlId).checkbox('setValue', isEnable);
                $.view.setStatus(scope, opts.status);
                $("[id^='" + scope + "'].easyui-linkbutton").linkbutton('alterStatus', opts.status);
            }
        }
    },

    //点击删除、修改按钮时提前验证
    setStatusexamine: function () {
        var opts = $(this).linkbutton('options');
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

        //验证方法，成功返回true,失败返回false，放在最下面是为了能够获取到当前按钮点击后赋的值的信息
        if (!$.fn.linkbutton.methods["examine"](this)) return;
    },

    cancel: function () {
        var me = this;
        var cancel = $.view.cancelObj;
        if (!cancel.data) cancel.data = [];
        $.messager.confirm("提示", "正在编辑,是否取消?", function (data) {
            if (!data) {
                return;
            } else {
                var opts = $(me).linkbutton('options');
                //$.view.clearView(opts.scope);
                //$.view.loadData(opts.scope, cancel.data);
                $.view.setViewEditStatusJC(opts.scope, cancel.status);
            }
        })
    }
});

/*
扩展tree
基础----初始化页面--左侧tree的联动*/
//tree的方法扩展
$.extend($.fn.tree.methods, {
    //一级结构的tree
    setAsso: function () {


        var opts = $(this).tree('options');
        var status = $.view.getStatus(opts.scope); //根据scope获取当前的status
        var con = opts.associate;                  //获取tree的associate属性
        //定义模型
        var mModel = opts.m;
        var r = $(this).tree('getSelected');
        //得到当前对象模型
        var rModel = r.attributes.m;
        if (opts.IsDocStatusAssociate) {


            if ([1, 2].exist(status)) {
                //                con = opts.associateEX;
                if (r && r.attributes) {
                    var ProjectClassId = "";
                    var ProjectClassName = r.attributes.ProjectClassName;
                    var ProjectClassKey = r.attributes.ProjectClassKey;
                    if (rModel == "SS_Project") {
                        var ProjectId = r.id;
                        var ProjectName = r.attributes.ProjectName;
                        var ProjectKey = r.attributes.ProjectKey;
                        ProjectClassId = r.attributes.GUID_ProjectClass;
                        $('#xmda-SS_Project-PKey').combogrid('setValue', ProjectId);
                        $('#xmda-SS_Project-PKey').combogrid('setText', ProjectKey);
                        $('#xmda-SS_Project-PName').combogrid('setValue', ProjectId);
                        $('#xmda-SS_Project-PName').combogrid('setText', ProjectName);
                        $('#xmda-SS_Project-ProjectClassKey').combogrid('setValue', ProjectClassId);
                        $('#xmda-SS_Project-ProjectClassKey').combogrid('setText', ProjectClassKey);
                        $('#xmda-SS_Project-ProjectClassName').combogrid('setValue', ProjectClassId);
                        $('#xmda-SS_Project-ProjectClassName').combogrid('setText', ProjectClassName);
                        $('#xmda-SS_Project-PGUID').val(ProjectId);
                        $('#xmda-SS_Project-GUID_ProjectClass').val(ProjectClassId);
                    }
                    else {
                        ProjectClassId = r.id;
                        $('#xmda-SS_Project-ProjectClassKey').combogrid('setValue', ProjectClassId);
                        $('#xmda-SS_Project-ProjectClassKey').combogrid('setText', ProjectClassKey);
                        $('#xmda-SS_Project-ProjectClassName').combogrid('setValue', ProjectClassId);
                        $('#xmda-SS_Project-ProjectClassName').combogrid('setText', ProjectClassName);
                        $('#xmda-SS_Project-GUID_ProjectClass').val(ProjectClassId);
                    }

                }
                return;
            }
        }

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
            $.fn.tree.methods["setAssoAfter"](r, status);
        }
        //向grid中赋值
        var optsGrid = opts.gridassociate;  //得到grid中options的属性
        //如果不对datagrid赋值的话，就没有gridassociate这个属性，直接return;
        if (optsGrid == null) return;
        var getTree = $(this).tree('getSelected');    //得到当前选中树节点的信息
        if (!getTree) return;
        var guidId = '#' + optsGrid.gridId;   //拿到grid的id
        var selRow = $(guidId).datagrid('getRows');
        if (selRow.length > 0) {
            $(guidId).datagrid('clearSelections');
        }
        var scope = opts.scope  //得到页面作用域
        var associateId = optsGrid.associateId; //角色GUID
        var dataId = optsGrid.map[associateId];
        var rAttr = getTree.attributes;  //当前节点的attributes 角色属性
        var mapId = rAttr[dataId];    //要传的GUID
        if (mapId) {
            var arrRole = mapId.split(',');
            //var gridRows = $(guidId).datagrid('getRows');   //得到当前的grid记录行 
            for (var i = 0; i < selRow.length; i++) {
                //将当前选择树的节点去跟datagrid中的行的对象的GUID做匹配
                //先得到grid每行的id
                //在我点击节点传值的时候，要先判断grid中是否以后选择项，在点击tree之前要先清空grid中的项
                //在将对应的grid匹配
                var RowsId = selRow[i][associateId];
                if (arrRole.exist(RowsId)) {
                    if (RowsId.count > 0) return;
                    $(guidId).datagrid('selectRow', i);  //如果datagrid数据中GUID与点击树传过来的guid相等，则选中一行
                }
            }
        }
    },
    setAssoAfter: function (node, status) {
    },

    alterStatus: function (jq, status) {
        return jq.each(function () {
            if (!status) return;
            var opts = $(this).tree('options');
            if (opts.noMustBind) return;
            if (opts.customBindFirst) {//只加载一次
                opts.noMustBind = true;
            }
            if (opts.forbidstatus) {
                for (var i = 0; i < opts.forbidstatus.length; i++) {
                    var c = opts.forbidstatus[i];
                    if (c == -1 || c == status) {
                        $(this).tree('disabled');
                        return;
                    }
                }
                $(this).tree('enabled');
            }
            else {
                $(this).tree('enabled');
            }
        });
    }

});

//默认格式化bool  -- 编辑扩展
$.extend($.fn.edatagrid.defaults.editors, {
    booleanbox: {
        init: function (container, options) {
            var input = $("<input type=\"text\"  class=\"datagrid-editable-booleanbox\" />").appendTo(container);
            input.combobox({ panelHeight: 50, editable: false, valueField: 'id', textField: 'text', data: [{ id: 'false', text: '否' }, { id: 'true', text: '是'}] });
            input.combobox(options);
            //绑定事件
            $.view.bind.call($(input), 'combobox');
            //加载数据
            return input;
        },
        destroy: function (target) {
            $(target).combobox('destroy');
        },
        getValue: function (target) {
            return $(target).combobox('getValue');
        },
        setValue: function (target, value) {
            if (value) {
                return $(target).combobox('setValue', value == '是' || (value + '').toString().toLocaleLowerCase() == "true" ? 'true' : 'false');
            }
            else {
                return $(target).combobox('setValue', 'false');
            }
        },
        resize: function (target, width) {
            $(target).combobox("resize", width);
        }
    }
});

//datagrid方法扩展
$.extend($.fn.edatagrid.methods, {
    //前台页面列表值转换成后台值
    transGridDataToBackstage: function (jq, rows) {

        var opts = $(jq).datagrid('options');
        //var data = rows || (opts.setSelectedType ? $(jq).datagrid('getRows') : $(jq).datagrid('getChecked'));
        var data = rows;
        if (opts.setSelectedType) {
            data = $(jq).datagrid('getRows');
        }
        else {
            data = $(jq).datagrid('getChecked');

            //分页数据处理，要把每个分页中选择的数据都要传入保存 需要分页保存的数据需要设置
            if (opts.savePage) {
                var r = [];
                var allRows = $(jq).datagrid('getData').originalRows;
                for (var i = 0; i < allRows.length; i++) {
                    if (allRows[i]['b-sel']) {
                        r.push(allRows[i]);
                    }
                }
                data = r;
            }
        }
        var result = [], ritem;
        var showAndHide = $(jq).edatagrid('getColumnMapping', true);
        if (data) {
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
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
    }
});



