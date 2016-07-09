$.extend($.fn.linkbutton.methods, {
    setJCDA: function () {
        
        var opts = $(this).linkbutton('options');
        var IsExist = opts.IsExist;
        var customId = '#hkspd-BX_Detail-GUID_Cutomer';
        var d = $(customId).combo('getValue');
        //        if (d) {
        //            $.messager.alert('提示', '校验成功！');
        //            return;
        //        }
        var result = { m: [], d: [] }
        var customs = $("[id^='hkspd-'][wldw='1']");
        customs.each(function () {
            var eauiType = $(this).GetEasyUIType();
            var gdata = $(this)[eauiType]('getData', true);
            if (gdata) {
                if (gdata.length > 1) {
                    gdata[0].m = "SS_Customer";
                    gdata[1].m = "SS_Customer";
                    result.m = result.m.concat(gdata);
                } else {
                    result.m.push(gdata);
                }
            }
        });
        $('#b-window').dialog({
            resizable: false,
            title: '收款单位',
            width: 900,
            height: 600,
            modal: true,
            minimizable: false,
            maximizable: false,
            collapsible: false,
            href: '/JCqtsz/WLDW/',
            onLoad: function (c) {
                var scope = 'wldw';
                $.view.loadData(scope, result);
                //新增的时候将单据的状态改为4,4表示浏览，即不可编辑的状态 sxh 2014/04/02 13:16
                if (IsExist) {
                    $.view.setViewEditStatus(scope, 4);
                } else {
                    $.view.setViewEditStatus(scope, 1);
                }
                
            }
        });

    },
    //取消单据
    wldwCancel: function () {
        var me = this;
        var cancel = $.view.cancelObj;
        if (!cancel.data) return;
        $.messager.confirm("提示", "正在编辑,是否取消?", function (data) {

            if (!data) {

                return;
            } else {
                var opts = $(me).linkbutton('options');

                $.view.clearView(opts.scope);
                $.view.loadData(opts.scope, cancel.data);
                //单据取消的时候将状态修改为4，及不可编辑状态  sxh 2014/04/02 13:18
                $.view.setViewEditStatus(opts.scope, 4); //5为可编辑状态
            }
        })

    },
    wldwSubmit: function () {

        var opts = $(this).linkbutton('options');
        var winId = '#' + opts.window;
        //当前页面的数据
        var opts = $('#hkspd-BX_Detail-GUID_Cutomer').combogrid('options');
        var guid = $('#wldw-SS_Customer-GUID').val();
        if (!guid) {
            $(winId).dialog('close');
            return;
        }
        opts.remoteData = [];
        opts.tempValue = guid;
        opts.isLoadSetData = true;

        $('#hkspd-BX_Detail-GUID_Cutomer').combogrid('loadRemoteDataToLocal1');
        $(winId).dialog('close');
    },
    wldwCancle: function () {
        var opts = $(this).linkbutton('options');
        var winId = '#' + opts.window;
        $(winId).dialog('close');
    },
    //重写examine方法为examine1，防止跟通用的examine方法冲突
    examine1: function (isSave) {
        //获取到scope
        var scope = $(this).linkbutton('options').scope;
        var pageState = $.view.getStatus(scope);

        //拿到界面中需要验证的控件id
        var id = "[id^='" + scope + "-'].easyui-validatebox";
        //msg如果为空那么不需要验证反之需要验证
        var msg = $(id)["validatebox"]("examineMsg", "");
        //手动去校验规则字段，暂时写死，回头有时间再做处理
        var PostcodeId = "#" + "wldw-SS_Customer-CustomerPostcode";
        var TelephoneId = "#" + "wldw-SS_Customer-CustomerTelephone";
        var WebsiteId = "#" + "wldw-SS_Customer-CustomerWebsite";
        var FaxId = "#" + "wldw-SS_Customer-CustomerFax";
        var arryId = ["#wldw-SS_Customer-CustomerPostcode", "#wldw-SS_Customer-CustomerTelephone", "#wldw-SS_Customer-CustomerWebsite", "#wldw-SS_Customer-CustomerFax"];
        var bPostcode = true, bTelephoneId = true, bWebsiteId = true, bFaxId = true;
        if ($(PostcodeId).val() != 0) {
            bPostcode = /^[0-9]\d{5}$/.test($(PostcodeId).val());
        }
        if ($(TelephoneId).val() != 0) {
            bTelephoneId = /^((\d{11})|^((\d{7,8})|(\d{4}|\d{3})-(\d{7,8})|(\d{4}|\d{3})-(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1})|(\d{7,8})-(\d{4}|{\d{3}|\d{2}|\d{1}))$)/.test($(TelephoneId).val());
        }
        if ($(WebsiteId).val() != 0) {
            bWebsiteId = /^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i.test($(WebsiteId).val());
        }
        if ($(FaxId).val() != 0) {
            bFaxId = /^((\d{2,3}\))|(\d{3}\-))?(\(0\d{2,3}\)|0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,4})?$/i.test($(FaxId).val());
        }
        var retMsg = "";
        if (!bPostcode) { retMsg += "邮政编码格式不正确[100000]</br>"; }
        if (!bTelephoneId) { retMsg += "联系电话的格式不正确[13000000000|010-0000000|0000000]</br>"; }
        if (!bWebsiteId) { retMsg += "公司网站地址格式不正确[http://www.example.com]</br>"; }
        if (!bFaxId) { retMsg += "传真号格式不正确[000-0000000]</br>"; }
        var arr = { "#wldw-SS_Customer-CustomerPostcode": bPostcode, "#wldw-SS_Customer-CustomerTelephone": bTelephoneId, "#wldw-SS_Customer-CustomerWebsite": bWebsiteId, "#wldw-SS_Customer-CustomerFax": bFaxId };
        //步骤分析 sxh-2014/04/02 13:40
        //1、返回值(msg)为空那么不需要校验规则字段直接提示哪些是必填项
        //2、返回值(msg)不为空需要校验规则字段同时提示哪些是必填项
        if (msg) {
            //返回值不为空
            //调用校验方法
            return verifyfun(this, retMsg, arr);
        } else {
            //返回值为空
            //调用校验方法
            return verifyfun(this, "", arr);
        }
        //声明校验方法
        function verifyfun(id, Msg, arr) {
            var msgError = "";
            var parms = $(id).linkbutton('getParms', 'examine1');
            if (parms) {
                for (var colType in parms) {
                    var controls = parms[colType];
                    for (var i = 0, j = controls.length; i < j; i++) {
                        var id = "#" + controls[i];
                        var fn = $.fn[colType].methods['verifyData'];
                        if (!fn) continue;
                        var msgtemp = fn.call($(id));
                        if (msgtemp)
                            msgError += msgtemp + "</br>";
                    }
                }
            }
            for (var elem in arr) {
                var val = arr[elem];
                if (Msg.length != 0) {
                    if (!val) $(elem).addClass("validatebox-invalid");
                } else {
                    if (val) $(elem).removeClass("validatebox-invalid");
                }
            }
            msgError += Msg;
            if (msgError.length != 0) {
                $.messager.alert('提示', msgError);
                return false;
            }
            if (isSave != true) {
                $.messager.alert('提示', "校验成功！");
                return;
            }
            return true;
        }
    },
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

                $.view.loadData(opts.scope, data);
                $.view.setViewEditStatus(opts.scope, opts.status);
                $.view.cancelObj = { data: data, status: opts.status };
                $.view.curPageState = 1;
            }
            else { //失败
                //弹出错误提示
                $.messager.alert(data.s.t, data.s.m, data.s.i);
            }
        });
    },
    newDoc1: function () {

        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'newDoc1');
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

                $.view.loadData(opts.scope, data);
                $.view.setViewEditStatus(opts.scope, opts.status);
                $.view.cancelObj = { data: data, status: opts.status };
                $.view.curPageState = 1;
            }
            else { //失败
                //弹出错误提示
                $.messager.alert(data.s.t, data.s.m, data.s.i);
            }
        });
    },
    saveDoc1: function () {

        var saveAjax = function (status, indata, opts) {
            if ($.view.curPageState == 6 || $.view.curPageState == 5) {
                status = '2';
            }
            $.ajax({
                url: url,
                data: { "status": status, "m": JSON.stringify(indata.m), "d": JSON.stringify(indata.d) },
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    $.messager.alert("错误", textStatus, 'error');
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
                                if ($.view.curPageState == 6 || $.view.curPageState == 5) {
                                    //sTemp = 3;
                                }
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
                })
            } else {
                saveAjax(status, indata, opts);
            }
        }
        $(this).linkbutton('wholeGridEndEdit');
        var opts = $(this).linkbutton('options');
        var parms = $(this).linkbutton('getParms', 'saveDoc');
        if (!parms) return;
        var url = parms[0];
        if (!url) return;
        var isSussess = true;
        if (parms[1]) {

            isSussess = $.fn.linkbutton.methods["examine1"].call($('#' + parms[1]), true);
            if (!isSussess) {
                return;
            }
        }
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
    //汇款审批单-收款单位窗体中的删除功能 sxh 2014/04/02 15:30
    delRow1: function () {
        //treeId  暂时将treeId写死，回头修改成在界面上配置的形式
        var treeId = "#wldw-tree-per";
        //获取当前选中tree的节点
        var currNode = $(treeId).tree('getSelected');
        if (!currNode) { $.messager.alert('提示', "请选择要删除的对象！"); return; }
        //调用按钮设置状态

        var opts = $(this).linkbutton('options');
        $.view.curPageState = opts.status;
        $(this).linkbutton('setWholeStatus');
        $(this).linkbutton('saveStatus');

    }
});
$.extend($.fn.combogrid.methods, {
    loadRemoteDataToLocal1: function (jq) {
        return jq.each(function () {
            
            var combogrid = $(this);
            var opts = combogrid.combogrid('options');
            if (!opts.remoteData.length && opts.remoteUrl) {
                if (opts.pagination) {
                    var gridOpts = combogrid.combogrid('grid').datagrid("options");
                    gridOpts.loadFilter = $.fn.datagrid.methods["pagerFilter"];
                }
                var cols = combogrid.combogrid('getColumns');
                $.ajax({
                    url: opts.remoteUrl,
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
                        //给要分页的控件的total附值，防止total为0时默认选中第一页
                    combogrid.combogrid('grid').datagrid('getPager').pagination('options').total = data.length;
                        
                        combogrid.combogrid('setPageNumber', opts.tempValue);
                        g.datagrid('loadData', data);
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
    }
});
$.extend($.fn.numberbox.methods, {
    //加载格式数据
    setData: function (jq, hs) {
        return jq.each(function () {

            var idAttr = $(this).attr('id').split('-');
            if (!idAttr) return;
            if (hs) {
                var a = hs[idAttr[1]], b;
                if (a && a.hasOwnProperty(idAttr[2])) {
                    b = a[idAttr[2]];
                    $(this).numberbox('setValue', b);
                    return;
                }
                return;
            }
            var opts = $(this).numberbox('options');
            $(this).numberbox('setValue', opts.defalutValue || '');
        })
    }
});
$.extend($.fn.validatebox.methods, {
    examineMsg: function (jq, Msg) {
        jq.each(function () {
            //获取验证控件中是否有validtype这个属性
            var isExit = $(this).attr('validtype');
            //如果没有validtype这个属性，那么直接返回
            if (typeof isExit == "undefined") return true;
            //临时变量用来存储每次遍历的值
            var temp = $(this).val();
            Msg += temp;
        });
        return Msg;
    }
});
