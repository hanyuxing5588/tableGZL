
$.extend($.fn.datagrid.defaults.editors, {
    textLWF: {
        init: function (_176, _177) {
            var _178 = $("<input type=\"text\" class=\"datagrid-editable-input\">").appendTo(_176);
            _178.bind("keydown", function (event) {

                if (event.keyCode == '13') {
                    var val = $(this).val();
                    $.gridKeydownEnter(val);
                }
            });
            return _178;
        },
        getValue: function (_179) {

            //过滤掉所有html标签
            var temp = $(_179).val().stripHTML();
            return temp;
        },
        setValue: function (_17a, _17b) {
            
            $(_17a).val(_17b);
        },
        resize: function (_17c, _17d) {
            $(_17c)._outerWidth(_17d)._outerHeight(22);
        }
    },
    numberbox: {
        init: function (container, options) {

            var input = $("<input type=\"text\" max=\"99999999.99\" class=\"datagrid-editable-numberbox\" />").appendTo(container);
            options = $.extend(options, { min: -99999999.99 });
            input.numberbox(options);
            //绑定事件
            input.bind("keydown", function (event) {
                if (event.keyCode == '13') {
                    var val = $(this).val();
                    $.gridKeydownEnterNum(val);

                }
            });
            //加载数据
            return input;
        },
        destroy: function (target) {
            $(target).numberbox('destroy');
        },
        getValue: function (target) {
            var result = $(target).val();
            if (!result || parseFloat(result) == 0) {
                result = "0.00";
            }
            result = result.toString().replace(/,/g, '');
            return result;
        },
        setValue: function (target, value) {
            if (value == undefined) value = "";
            value = (value + "").toString().replace(/,/g, '');
            if ($.isNumeric(value) && parseFloat(value) == 0) {
                value = '';
            }
            $(target).val(value);
            return $(target).numberbox('setValue', value);
        },
        resize: function (_5f1, _5f2) {
            $(_5f1)._outerWidth(_5f2)._outerHeight(22);
        }
    }
});
$.CTax = function (iMoney) {
    var iTax, iM; //dTax税后收入 dm应纳税所得额
    if (iMoney <= 3360) {
        iM = (iMoney - 800) / 0.8;
        iTax = iM * 0.2;
    }
    else if (iMoney > 3360 && iMoney <= 21000) {
        iM = iMoney * (1 - 0.2) / (1 - 0.2 * (1 - 0.2));
        iTax = iM * 0.2;
    }
    else if (iMoney > 21000 && iMoney <= 49500) {
        iM = ((iMoney - 2000) * (1 - 0.2)) / (1 - 0.3 * (1 - 0.2));
        iTax = iM * 0.3 - 2000;
    }
    else {//49500
        iM = ((iMoney - 7000) * (1 - 0.2)) / (1 - 0.4 * (1 - 0.2));
        iTax = iM * 0.4 - 7000;

    }
    if (iTax < 0) return 0.00;
    return iTax.toFixed(2);
};
$.extend($.fn.edatagrid.defaults, {
//    editBefore: function (param) {
//        return true;
//        var allRows = $('#lwflkd-BX_InviteFee').datagrid('getRows');
//        if (!allRows) return true;
//        var selRow = allRows[param.index];
//        if (!selRow) return true;
//        var rowIndex = param.index;
//        // 获得当前的控件值
//        var val = selRow["lwflkd-BX_InviteFee-Total_Real"];
//        if (val) {
//            var temp1 = 0;
//            var temp2 = new Number(val);
//            if (isNaN(temp2)) return;
//            var temp = temp2.formatThousand(temp2);
//            $('#tableGridTd .datagrid-view2 tr[datagrid-row-index=' + rowIndex + '] td[field="lwflkd-BX_InviteFee-Total_BX"] div').text(temp)
//            selRow['lwflkd-BX_InviteFee-Total_BX'] = temp2;
//            temp = new Number(temp1).formatThousand(temp1);
//            $('#tableGridTd .datagrid-view2 tr[datagrid-row-index=' + rowIndex + '] td[field="lwflkd-BX_InviteFee-Total_Tax"] div').text(0.00)
//            selRow['lwflkd-BX_InviteFee-Total_Tax'] = 0.00;
//        }
//        return true;
//    },
    editEditAfterEvent: function (i, data, changes) {
        debugger
       if (i == undefined || i == null) return;
        var val = data["lwflkd-BX_InviteFee-Total_Real"];
        var rows = $(this).datagrid('getRows');
       var row = rows[i];
        if (val) {
            var temp1 = 0;
            var temp2 = new Number(val);
            if (isNaN(temp2)) return;
            var temp = temp2.formatThousand(temp2);
            row["lwflkd-BX_InviteFee-Total_BX"] = temp;
            row["lwflkd-BX_InviteFee-Total_Tax"] = 0.00;
            $('#tableGridTd .datagrid-view2 tr[datagrid-row-index=' + i + '] td[field="lwflkd-BX_InviteFee-Total_BX"] div').text(temp)
           // $(this).datagrid('updateRow', { index: i, row: row });
        }
        return true;
    }
});
$.extend($.fn.numberbox.methods, {
    //给datagrid的某一列赋值
    setAssociate: function (jq) {
        
    }
});
$.extend($.fn.linkbutton.methods, {
    submitSelectedPerson: function () {
        var selectedRow = $('#persongrid').datagrid('getSelected');
        if (!selectedRow) {
            $.messager.alert('提示', '请选择一条记录');
            return;
        }
        $('#b-window').dialog('close');
        $.GetSelecteRowSetValue(selectedRow);
    },
    printDoc: function () {
        var val = $('#lwflkd-print-type').combobox('getText');
        var path, index;
        switch (val) {
            case '签收单打印（有合计）': //1、有合计:printBeforePass1(页面3) 未提交
                path = '/Print/lwflkdgcy3';
                index = '3';
                break;
            case '签收单打印（无合计）': //1、无合计:printBeforePass1(页面3) 未提交
                path = '/Print/lwflkdgcy4';
                index = '4';
                break;
            case '领款人未签字': //1：未签字：printAfterPass1(页面1)
                path = '/Print/lwflkdgcy2';
                index = '2';
                break;
            default: //1：已签字：printAfterPass1(页面1)
                path = '/Print/lwflkdgcy1';
                index = '1';
                break;

        }
        $.fn.linkbutton.methods.printLWF(path, index);
    },
    printLWF: function (path, index) {
        var guid = $.view.getKeyGuid('lwflkd');
        var data = $.view.retrieveDoc(guid, 'lwflkd');
        var ids = ['lwflkd-BX_Main-moneychinese', 'lwflkd-BX_Detail-Total_Real', 'lwflkd-BX_Main-YingFaheji', 'lwflkd-BX_Main-DaiShoushuie1'];
        for (var i = 0; i < ids.length; i++) {
            
            var id = ids[i].split('-');
            var text = $('#' + ids[i]).val();
            if (id[2] == "moneychinese") {
                text = $.fn.validatebox.methods.moneytransferchinese($('#lwflkd-BX_Main-YingFaheji').val());
            }
            data.m.push({ m: id[1], n: id[2], v: text });
        }
        var url = path + "?pturl=lwflkdgcy" + index;    //根据状态打印不同的模板
        $("#printIframe").jqprintEx(url, data);
    },
    //未提交打印模板
    //两种情况：1、有合计:printBeforePass1(页面3)
    //         2、无合计：printBeforePass2(页面4)
    printBeforePass1: function () {
        //重新获取数据，
        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'printBeforePass');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        var data = $.view.retrieveDoc(guid, scope);
        if (!data || !data.m) return;
        var ids = parms[1];
        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i].split('-');
                var text = $('#' + ids[i]).val();
                data.m.push({ m: id[1], n: id[2], v: text });
            }
        }
        //动态匹配打印模板
        var tempScope = parms[0].split("/")[2]; //获取单据类型(scope)
        var url = parms[0] + "?pturl=" + tempScope + "gcy3";    //根据状态打印不同的模板
        $("#printIframe").jqprintEx(url, data);
    },
    printBeforePass2: function () {
        //重新获取数据，
        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'printBeforePass');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        var data = $.view.retrieveDoc(guid, scope);
        if (!data || !data.m) return;
        var ids = parms[1];
        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i].split('-');
                var text = $('#' + ids[i]).val();
                data.m.push({ m: id[1], n: id[2], v: text });
            }
        }
        //动态匹配打印模板
        var tempScope = parms[0].split("/")[2]; //获取单据类型(scope)
        var url = parms[0] + "?pturl=" + tempScope + "gcy4";    //根据状态打印不同的模板
        $("#printIframe").jqprintEx(url, data);
    },
    //提交后打印模板。
    //两种情况：1：未签字：printAfterPass1(页面1)
    //          2：已签字printAfterPass2(页面2)
    printAfterPass1: function () {
        //重新获取数据，
        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'printAfterPass1');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        var data = $.view.retrieveDoc(guid, scope);
        if (!data || !data.m) return;
        var ids = parms[1];
        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i].split('-');
                var text = $('#' + ids[i]).val();
                data.m.push({ m: id[1], n: id[2], v: text });
            }
        }
        //动态匹配打印模板
        var tempScope = parms[0].split("/")[2]; //获取单据类型(scope)
        var url = parms[0] + "?pturl=" + tempScope + "gcy1";
        $("#printIframe").jqprintEx(url, data);
    },
    printAfterPass2: function () {
        //重新获取数据，
        var scope = $(this).linkbutton('options').scope;
        var parms = $(this).linkbutton('getParms', 'printAfterPass2');
        if (!parms || !parms.length) return;
        var guid = $.view.getKeyGuid(scope);
        var data = $.view.retrieveDoc(guid, scope);
        if (!data || !data.m) return;
        var ids = parms[1];
        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i].split('-');
                var text = $('#' + ids[i]).val();
                data.m.push({ m: id[1], n: id[2], v: text });
            }
        }
        //动态匹配打印模板
        var tempScope = parms[0].split("/")[2]; //获取单据类型
        var url = parms[0] + "?pturl=" + tempScope + "gcy2";
        $("#printIframe").jqprintEx(url, data);
    },
    pritsubmitProcess: function () {
        var opts = $(this).linkbutton('options');
        if ($.view.judgePageCancleState(opts.docState)) {
            $.messager.alert('提示', '该单据已经作废,操作无效')
            return;
        }

    },
 
    beforeSave: function () {
        var rows = $('#lwflkd-BX_InviteFee').datagrid('getRows');
        var dic = {};
        for (var i = 0, j = rows.length; i < j; i++) {
            var row = rows[i];
            var typeKey = row['lwflkd-BX_InviteFee-CredentialTypeKey']; //证件类型
//            if (typeKey == "01") {//身份证 
//                if (!row["lwflkd-BX_InviteFee-InvitePersonIDCard"].validateIdCard()) {
//                    $.messager.alert("提示", "第"+i+1+"行的身份证号码输入错误！");
//                    return false;
//                }
//            }
            var key = row['lwflkd-BX_InviteFee-CredentialTypeKey'] + row["lwflkd-BX_InviteFee-InvitePersonName"] + row["lwflkd-BX_InviteFee-InvitePersonIDCard"];
            if (dic[key]) {
                $.messager.alert("提示", "人员不能重复！");
                return false;
            }
            dic[key] = i + 1;
        }
    }
});
$.extend($.fn.datagrid.methods, {
    InsertRow: function (jq, params) {
        return jq.each(function () {
            var dg = $(this);
            var selRow = $(jq).datagrid('getSelected');
            var index = $(jq).datagrid('getRowIndex', selRow);
            if (!selRow) {
                dg.edatagrid('addRow');
            } else {

                if (index >= 0) {
                    dg.edatagrid('endEdit', index);
                }
                var rows = dg.edatagrid('getRows');
                dg.edatagrid('insertRow', { index: index, row: {} });
                dg.edatagrid('selectRow', index);
            }
        });
    }
});

/*
* 身份证15位编码规则：dddddd yymmdd xx p
* dddddd：6位地区编码
* yymmdd: 出生年(两位年)月日，如：910215
* xx: 顺序编码，系统产生，无法确定
* p: 性别，奇数为男，偶数为女
* 
* 身份证18位编码规则：dddddd yyyymmdd xxx y
* dddddd：6位地区编码
* yyyymmdd: 出生年(四位年)月日，如：19910215
* xxx：顺序编码，系统产生，无法确定，奇数为男，偶数为女
* y: 校验码，该位数值可通过前17位计算获得
* 
* 前17位号码加权因子为 Wi = [ 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 ]
* 验证位 Y = [ 1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2 ]
* 如果验证码恰好是10，为了保证身份证是十八位，那么第十八位将用X来代替
* 校验位计算公式：Y_P = mod( ∑(Ai×Wi),11 )
* i为身份证号码1...17 位; Y_P为校验码Y所在校验码数组位置
*/
String.prototype.validateIdCard = function () {
    
    var idCard=this;
    //15位和18位身份证号码的正则表达式
    var regIdCard = /^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$/;

    //如果通过该验证，说明身份证格式正确，但准确性还需计算
    if (regIdCard.test(idCard)) {
        if (idCard.length == 18) {
            var idCardWi = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2); //将前17位加权因子保存在数组里
            var idCardY = new Array(1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2); //这是除以11后，可能产生的11位余数、验证码，也保存成数组
            var idCardWiSum = 0; //用来保存前17位各自乖以加权因子后的总和
            for (var i = 0; i < 17; i++) {
                idCardWiSum += idCard.substring(i, i + 1) * idCardWi[i];
            }

            var idCardMod = idCardWiSum % 11; //计算出校验码所在数组的位置
            var idCardLast = idCard.substring(17); //得到最后一位身份证号码

            //如果等于2，则说明校验码是10，身份证号码最后一位应该是X
            if (idCardMod == 2) {
                if (idCardLast == "X" || idCardLast == "x") {
                    return true;
                } else {
                    return false;
                }
            } else {
                //用计算出的验证码与最后一位身份证号码匹配，如果一致，说明通过，否则是无效的身份证号码
                if (idCardLast == idCardY[idCardMod]) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    } else {
        return false;
    }
}