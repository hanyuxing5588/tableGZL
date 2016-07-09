$.extend($.fn.linkbutton.methods, {
    newlyAdd: function () {
        var opts = $(this).linkbutton('options');
        var status = $.view.getStatus(opts.scope);
        if (status == 4) {//页面状态时固定的，如果等于当前状态，就改变
            $(this).linkbutton('setWholeStatus');   //改变状态时，更改控件是否编辑
            $(this).linkbutton('saveStatus');
            $.view.clearView(opts.scope);    //清空页面数据，除datagrid
        }
        $.fn.linkbutton.methods["AfterNew"].call($(this), status);
    },
    //新增之后
    AfterNew: function (status) {

    },
    saveDoc: function () {
        var me = this;
        
        $.fn.linkbutton.methods["beforeSave"].call($(me));
        
        var saveAjax = function (status, indata, opts) {
            $.ajax({
                url: url,
                data: { "status": status, "m": JSON.stringify(indata.m), "scope": opts.scope },
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
                                //还原页面初始状态
                                $.view.clearView(opts.scope, "dataregion");
                                //加载页面数据
                                $.view.loadData(opts.scope, data);
                                $.view.setViewEditStatus(opts.scope, opts.status);
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
                                $.view.clearView(opts.scope, "dataregion");
                                //加载页面数据
                                $.view.loadData(opts.scope, data);
                                var sTemp = opts.status;
                                $.view.setViewEditStatus(opts.scope, sTemp);
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
                                $.view.setViewEditStatus(opts.scope, 4); //                                  
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
            if (status == "3") {
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
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'saveDoc');
        if (!parms) return;
        var url = parms[0];
        var scope = opts.scope;
        if (!url) return;
        if (opts.checkOnlyOne) {
            var idColls = opts.bindparmsEx;
            var chkId1 = '#' + idColls.idColls[0], chkId2 = '#' + idColls.idColls[1];
            var checked = 'checked', flag1, flag2;
            flag1 = $(chkId1).attr('checked');
            flag2 = $(chkId2).attr('checked');
            if ((flag1 == checked && flag2 == checked) || (flag1 == undefined && flag2 == undefined)) {
                var msg = $(chkId1).attr('exname') + '和' + $(chkId2).attr('exname') + '只能选择其一';
                $.messager.alert('提示', msg, 'info');
                return;
            }
        }
        var isSussess = true;

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
    afterSave: function (status) {
        
        var opts = this.linkbutton('options');
        var treeId = opts.treeId;
        var comboId = opts.comboId;
        //var scope = opts.scope;
        //var retStatus = $('#' + treeId).tree('options').retStatus;
        if (comboId) {
            $('#' + comboId).combogrid('grid').datagrid('reload');
        }
        for (var i = 0, j = treeId.length; i < j; i++) {
            var currTreeId = treeId[i];
            $('#' + currTreeId).tree('reload');
        }
    },
    setDisable: function () {
        var parms = $(this).linkbutton('getParms', 'setDisable');
        var opts = $(this).linkbutton('options');
        //得到是否停用的id
        if (!parms) return;
        var controlId = '#' + parms[0];
        var treeId = '#' + parms[1];
        var selTree = $(treeId).tree('getSelected');
        if (selTree == null) {
            $.messager.alert("提示", '请选中要停用的对象！', 'info');
            return;
        }
        //改变其状态
        $(controlId).attr('checked', true);
        $.view.setStatus(opts.scope, opts.status);
        $("[id^='" + opts.scope + "'].easyui-linkbutton").linkbutton('alterStatus', opts.status);
    },
    setEnable: function () {
        var parms = $(this).linkbutton('getParms', 'setEnable');
        var opts = $(this).linkbutton('options');
        //得到是否停用的id
        if (!parms) return;
        var controlId = '#' + parms[0];
        var treeId = '#' + parms[1];
        var selTree = $(treeId).tree('getSelected');
        if (selTree == null) {
            $.messager.alert("提示", '请选中要启用的对象！', 'info');
            return;
        }
        //改变其状态
        $(controlId).attr('checked', false);
        $.view.setStatus(opts.scope, opts.status);
        $("[id^='" + opts.scope + "'].easyui-linkbutton").linkbutton('alterStatus', opts.status);
    }
});
$.extend($.fn.tree.methods, {
    setAssociate: function () {
        
        var opts = $(this).tree('options');
        if (!opts.associate) return;
        var r = $(this).tree('getSelected');
        var map = opts.associate;
        var status = $.view.getStatus(opts.scope);
        if (r.attributes.IsStop) {
            var flag = r.attributes.IsStop.toLocaleLowerCase();
            var type = $($('.easyui-tabs .tabs-header .tabs-wrap .tabs .tabs-selected a span')[0]).html().trim();
            var msg = '该' + type + '已停用,不能增加下级' + type + '！';
            if (status == "1" && flag == "true") {
                $.messager.alert('提示', msg, 'info');
                return;
            }
        }
        if (opts.IsDocStatusAssociate) {
            if ([1, 2].exist(status)) {
                map = opts.associateEx;
            }
        }

        if (r && r.attributes && r.attributes.valid) {
            for (var lab in map) {
                var valuefield = map[lab][0];
                var textfield = map[lab][1];
                if (!textfield) {
                    textfield = valuefield;
                }
                var control = $('#' + lab);
                var plugin = control.GetEasyUIType();
                var mfn = $.fn[plugin];
                if (mfn) {
                    var sv = mfn.methods['setValue'];
                    if (sv) {
                        var guid = r.attributes[valuefield];
                        if (plugin == "combogrid") {
                            control[plugin]('setPageNumber', guid);
                        }
                        sv($('#' + lab), guid);
                    }
                    var st = mfn.methods['setText'];
                    if (st) {
                        st(control, r.attributes[textfield]);
                    }
                }
            }
        }
    }

});