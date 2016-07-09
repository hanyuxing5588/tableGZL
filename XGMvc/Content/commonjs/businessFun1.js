$.view = {
    //当前页面状态 注：新增：1  修改：2  删除：3  浏览：4 废除：5  恢复：6
    curPageState: 1,
    warning: "此次会话已过期，请重新登陆！",
    //明细用的数据
    detailObj: {
        data: { r: [], ad: [] },
        defaultData: null,
        numObj: {},
        cur: 1
    },
    //全局数据为取消时候用
    cancelObj: {
        data: null,
        status: 4
    },
    //设置页面废除 效果(已废除字样的显示与隐藏)
    setPageCancleState: function (scope) {
        var id = $('#' + scope + "-abandonedStatus");
        var $abandoned = $(id);
        var colId = $abandoned.attr("statusControlID");
        $('#' + colId).val() == "9" ? $abandoned.show() : $abandoned.hide();
    },
    //判断页面的状态  废除 恢复 等

    judgePageCancleState: function (docState) {
        //获取到当前页面的状态值

        var val = $('#' + docState).val();
        switch (val) {
            case "9": return true; break;
            case "6": return false; break;
            default: return false; break;
        }
    },
    //获取页面的状态

    //获取单据状态

    getPageState: function (docState) {
        var val = $('#' + docState).val();
        return val;
    },
    setPageState: function (state) {
        if (!state) return;
        var t = $('#initstatus');
        state ? t.attr("value", state) : t.attr("value", "");
    },
    //初始化页面   

    init: function (scope, status, dataguid) {

        $.view.initBefore(scope, status, dataguid);
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
                    if (data) {

                        $.view.loadData(scope, data);
                    }
                    $.view.setViewEditStatus(scope, status);
                    $.view.cancelObj = { data: data, status: status };

                    break;
            }

            $.view.initAfter(scope, status, dataguid);
        }

    },
    //初始化加载完毕
    initAfter: function (scope, status, dataguid) {

    },
    initBefore: function (scope, status, dataguid) {

    },
    //获取已知单据的所有信息


    retrieveDoc: function (guid, scope) {

        var data;
        if (guid && scope) {
            var url = '/' + scope + '/Retrieve';
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
    },
    /*
    methiod:设置页面编辑状态    parms:scope 作用域 status 状态 notLoadData 默认是需要load数据
    */
    setViewEditStatus: function (scope, status, notLoadData) {
        var dt = new Date();
        if (!scope || !status) return;
        var types = ["linkbutton", "datebox", "combogrid", "combobox", "validatebox", "numberbox", "edatagrid", "datagrid", "checkbox", "tree"];
        var preselector = "[id^='" + scope + "-'].easyui-";
        for (var i = 0, j = types.length; i < j; i++) {
            var type = types[i];

            var id = "[id^='" + scope + "-'].easyui-" + type;
            var contorls = $(id);
            contorls[type]("alterStatus", status);

            if (type == "edatagrid") {
                this.zkk = contorls;
            }

            if (!notLoadData && (type == "combogrid" || type == "combobox")) {
                contorls[type]('loadRemoteDataToLocal');
            }
        }
        this.setStatus(scope, status);
        this.setPageCancleState(scope);
        //        alert(dt - new Date()); alert('a')
    },
    //修改页面状态
    setStatus: function (scope, status) {

        if (!scope) return;
        var t = $('#' + scope + "-status");
        status ? t.attr("value", status) : t.attr("value", "");
    },
    //获取页面的状态
    getStatus: function (scope) {
        if (!scope) return;
        return $('#' + scope + "-status").attr("value");
    },
    //获取主单的Guid
    getKeyGuid: function (scope) {
        if (!scope) return;
        return $("[id^='" + scope + "-'][keyattr^='1']").val();
    },
    //清除页面数据
    clearView: function (scope, region) {
        if (!scope) return;
        var types = ["datebox", "combogrid", "combobox", "validatebox", "numberbox", "edatagrid", "checkbox"], preselector;
        region ? preselector = '#' + scope + "-" + region + " [id^='" + scope + "-']" : preselector = "[id^='" + scope + "-']";
        /*优化1*/
        //        
        //        var controls = $(preselector);
        //        controls.each(function () {
        //            var controlType = $(this).GetEasyUIType();
        //            if (!controlType) return;
        //            $(this)[controlType]('setData');
        //        })
        /*优化2*/
        preselector += ".easyui-";
        for (var i = 0, j = types.length; i < j; i++) {
            var type = types[i];
            $(preselector + type)[type]('setData');
        }
        $("#" + scope + "delrecordtemp").remove(); //20131226优化到此
    },
    //加载页面数据 isNotTemp 直接赋值
    loadData: function (scope, data, isNotTemp, isNotLoadGrid) {
        if (!scope) return;
        var ditem, id, jq, classattr, method;
        //data是从全局变量$.view.cancelObj中获取到得值，初始化时为null，需要进行判断处理。

        if (!data) return;

        if (isNotLoadGrid) {
        }
        else {
            //给列表赋默认值

            this.setDataForGrid(data.f, scope, 'setDefaultRow');
            //给列表赋值
            this.setDataForGrid(data.d, scope, 'setData');
        }

        var m = data.m;
        //主单
        if (m) {
            var hsM = { isNotTemp: isNotTemp };
            for (var i = 0, j = m.length; i < j; i++) {
                var field = m[i];
                hsM[field.m] = hsM[field.m] || {};
                hsM[field.m][field.n] = field.v;
            }
            var types = ["datebox", "combogrid", "combobox", "validatebox", "numberbox", "checkbox", "searchbox"];
            var temp = "[id^='" + scope + "'].easyui-";

            for (var i = 0, j = types.length; i < j; i++) {
                var type = types[i];
                var id = temp + type;
                var eles = $(id);
                eles[type]('setData', hsM);
            }
            delete hsM;
        }
        //        if (isNotLoadGrid) return;
        //        //给列表赋默认值

        //        this.setDataForGrid(data.f, scope, 'setDefaultRow');
        //        //给列表赋值
        //        this.setDataForGrid(data.d, scope, 'setData');

    },
    //给列表赋值
    setDataForGrid: function (d, scope, funName) {

        if (!d || d.length == 0) {
            return;
        }

        for (var i = 0; i < d.length; i++) {
            var ditem = d[i];
            if (ditem) {
                var id = scope + "-" + ditem.m;
                var jq = $('#' + id);
                if (!jq) continue;
                var classattr1 = jq.attr('class');
                if (!classattr1) continue;
                classattr = classattr1.split(' ')[0];
                if (classattr && classattr.indexOf('easyui-') == 0) {
                    var method = $.fn[classattr.replace("easyui-", "")].methods[funName];
                    if (method) {
                        method(jq, ditem);
                    }
                }
            }
        }
    },
    //抽取页面数据 准备删除 弃用
    retrieveData: function (scope, region, isText) {

        if (!scope) return;
        var result = { m: [], d: [] }, eauiType, gdata = null, method, selectortag;
        region ? selectortag = '#' + scope + "-" + region + " [id^='" + scope + "-']" : selectortag = "[id^='" + scope + "-']";
        $(selectortag).each(function () {
            eauiType = $(this).GetEasyUIType();
            var types = ["datebox", "combogrid", "combobox", "validatebox", "numberbox", "checkbox", 'datagrid', 'edatagrid', 'searchbox'];
            if (!types.exist(eauiType)) return;
            gdata = $(this)[eauiType]('getData', isText);

            if (gdata) {

                switch (eauiType) {
                    case "datagrid":
                    case "edatagrid":
                        result.d.push(gdata);
                        break;
                    default:
                        if (gdata.length) {
                            for (var i = 0, j = gdata.length; i < j; i++) {
                                result.m.push(gdata[i]);
                            }
                        } else {
                            result.m.push(gdata);
                        }
                        break;

                }
            }

        });
        return result;
    },
    //新的抽取页面数据 方法
    retrieveDataNew: function (scope, region, isText) {
        if (!scope) return;
        var result = { m: [], d: [] }, classattr, gdata = null, method, selectortag;
        region ? selectortag = '#' + scope + "-" + region + " [id^='" + scope + "-']" : selectortag = "[id^='" + scope + "-']";
        var types = ["datebox", "combogrid", "combobox", "validatebox", "numberbox", "combobox", "checkbox", "searchbox"];
        var temp = "[id^='" + scope + "'].easyui-";
        for (var m = 0, h = types.length; m < h; m++) {

            var type = types[m];
            var id = temp + type;
            $(id).each(function () {
                var gdata = $(this)[type]('getData', isText);
                if (gdata.n == "OrderNum") {

                }
                var gdata = $(this)[type]('getData', isText);
                if (gdata) {
                    if (gdata.length) {
                        for (var i = 0, j = gdata.length; i < j; i++) {
                            result.m.push(gdata[i]);
                        }
                    } else {
                        result.m.push(gdata);
                    }

                }
            });
        }
        return result;
    },
    //数据显示 转换方法集


    bind: function (type, unbind) {
        var me = this;

        var opts = this[type]('options');
        var bindmethod = opts.bindmethod; //控件配置
        if (!bindmethod) return; //无绑定方法 返回
        var pk = {}, preFun = null; //方法链 和 当前方法
        opts.customBind = !unbind; //设置标记 如果需要绑定一次的控件 判断其即可


        for (var event in bindmethod) {
            var methods = bindmethod[event];
            if (!methods) continue; //对应事件 方法集合不存在返回


            var funPush = [];
            for (var i = methods.length - 1; i >= 0; i--) {//
                var funName = methods[i];
                if (funName == 'bind') continue;
                var curFun = $.fn[type].methods[funName];
                if (!curFun) continue;
                funPush.push(curFun);
            }
            var fun = function (parms) {
                for (var i = funPush.length - 1; i >= 0; i--) {//
                    var funTemp = funPush[i];
                    funTemp.call(me, parms);
                }
            }
            ////是控件自有的事件  例如 onSelect OnClickCell等


            if (opts.hasOwnProperty(event)) {
                unbind ? pk[event] = function () { } : pk[event] = fun;
            }
            else {//有可能是原生的 blur click

                unbind ? this.unbind(event, preFun) : this.bind(event, fun);

            }

        }
        //控件事件而言 多个事件 一次初始化 对于原生的直接bind即可
        this[type](pk);
    },
    //公共事件处理函数
    eventFuc: {
        maxLength: function (event) {
            var $this = $(this);
            var max = $this.attr("maxLength");
            if (max) {
                var keystr = String.fromCharCode(event.keyCode)
                var reg = /[0-9,a-z,A-Z,.]/;
                if (reg.test(keystr)) {
                    var value = $this.attr("value");
                    value = value ? value : "";
                    var valuelen = value.length;
                    if (typeof max == "string") {
                        max = parseInt(max);
                    }
                    return valuelen <= max;
                }
            }
        }
    },
    //刷新
    refresh: function () {

        var scope = $("#initscope").val();
        var status = $("#initstatus").val();
        var dataguid = $("#initguid").val();
        //    alert(status);

        var data = $.view.retrieveDoc(dataguid, scope);
        if (data) {
            $.view.loadData(scope, data);
        }
        $.view.cancelObj = { data: data, status: status }
    },
    //格式化数据函数

    formatters: {
        boolbox: function (value1, row, index) {
            if (value1) {
                var value = $.trim(value1 + "").toLowerCase();
                if (value == "true" || value == "1" || value == "是")
                    return "是";
            }
            return "否";
        },
        datebox: function (date, row, index) {

            if (date && date.length != 0) {
                var date = new Date().FormatDate(date);
            } else {
                date = '';
            }
            return date;
        },
        numberbox: function (value1, row, index) {

            if (!value1) return '0.00';
            if ($.isNumeric(value1) && value1 == 0) {
                value1 = '';
            }
            return $.isNumeric(value1) ? new Number(value1).formatThousand(value1) : '0.00'
        },
        // dongsheng.zhang  2014-5-6     
        numberboxNoZero: function (value1, row, index) {

            if (!value1) return '';
            if ($.isNumeric(value1) && value1 == 0) {
                value1 = '';
            }
            return $.isNumeric(value1) ? new Number(value1).formatThousand(value1) : ''
        }
    }
};

/*
*财政支付令 
*控件的ID罗列参数
*model 模型*bgTypeColID 项目预算类型 基本支出 1 项目支出 2
*functionClassColID 功能分类 
*economyClassColID  经济分类
*expendTypeColID    支出类型
*bgSourceColID      预算来源
*projectColID 项目ID 只要为了取项目扩展码 ExtraCode
*isGouKuColID 是否国库
*curColID 财政支付码的值

*/
var GetPayCode = function (bgTypeColID, functionClassColID, economyClassColID, expendTypeColID, bgSourceColID, projectColID, isGouKuColID, curColID) {
debugger
    var temp = $("#" + curColID);
    temp.val('');
    var isGouKu = $('#' + isGouKuColID).checkbox('getValue');
    if (!isGouKu) {//是 国库才能生成财政支付码
        return;
    }
    var bgType = $('#' + bgTypeColID).combo('getValue'); //预算类型
    var funcitonClass = $('#' + functionClassColID).combo('getValue'); //功能分类
    var economyClass = $('#' + economyClassColID).combo('getValue'); //经济分类
    var expendType = $('#' + expendTypeColID).combo('getValue'); //支出类型
    var bgSource = $('#' + bgSourceColID).combo('getValue'); //预算来源
    var projectExtra = $('#' + projectColID).combo('getValue'); //扩展码
    if (!(bgType && funcitonClass && economyClass && expendType && bgSource)) {
        //五个信息来源都不能为空


        $("#" + curColID).val('');
    }
    function GetCZZFValue(id) {//给combogrid控件的需要值


        var c = $('#' + id).combogrid('options').CZZFField;
        var g = $('#' + id).combogrid('grid'); // get datagrid object
        var r = g.datagrid('getSelected'); // get the selected row、


        return r ? r[c] : '';
    };
    function GetComboxRowValue(id, field) {//给combogrid控件的需要值


        var c = field;
        var g = $('#' + id).combogrid('grid'); // get datagrid object
        var r = g.datagrid('getSelected'); // get the selected row、


        return r ? r[c] : '';
    };
    function GetFormat0(value, digit) {
        var attr = new Array();
        if (value && $.trim((value + "")).toUpperCase() != "NULL") {
            if (value.length > digit) {
                value = value.substr(0, digit);
                attr.push(value);
            } else {
                attr.push(value);
                for (var i = 0; i < digit - value.length; i++) {
                    attr.push("0");
                }
            }
        } else {
            for (var i = 0; i < digit; i++) {
                attr.push("0");
            }
        }
        return attr.join('');
    };
    var czzfCode = "";
    //    //预算类型
    //    var value = GetCZZFValue(bgTypeColID);
    //    if (value == "01") {
    //        czzfCode = "1";
    //    } else {
    //        czzfCode = "2";
    //    }

    /*判断要不要加后面的3个0 跟李总确认 根据功能分类判断是否为项目 如果项目为否则去挑3个000*/
    var d = GetComboxRowValue(functionClassColID, "IsProject");
    var value = "";
    if (d == "0" || d == "false" || d == "False" || d == false) {
        czzfCode = "1";
    }
    else {
        czzfCode = "2";
    }

    //功能分类
    var value = GetCZZFValue(functionClassColID);
    //FinanceCode
    czzfCode += GetFormat0(value, 7);
    //经济分类
    var value = GetCZZFValue(economyClassColID);
    czzfCode += value;
    //科目预算码 2015-12-24
    
    var pid = projectColID.split('-');
    var guidBGCode = $('#' + pid[0] + '-' + pid[1] + '-GUID_BGCode').combo('getValue');
    //.toUpperCase()
    guidBGCode = guidBGCode.toUpperCase();
    czzfCode += window.bgCodeDic[guidBGCode];
    //支出类型
    var value = GetCZZFValue(expendTypeColID);
    czzfCode += value;
    //预算来源bgSourceColID
    var value = GetCZZFValue(bgSourceColID);
    czzfCode += value;
    //项目扩展码
    var value = GetCZZFValue(projectColID);
    
    //项目附加码  2016-1-25
    if (!value) {
        var id =pid[0] + '-CN_PaymentNumber-GUID_ProjectEx';
        value = GetCZZFValue(id);
    }
    value ? czzfCode += value : czzfCode += "000";
    (czzfCode && (czzfCode.length == 16 || czzfCode.length == 18)) ? temp.val(czzfCode) : temp.val('');
    if ((d == "0" || d == "false" || d == "False" || d == false) && czzfCode && (czzfCode.length == 16 || czzfCode.length == 18)) {
        temp.val(czzfCode.substr(0, czzfCode.length - 3));
    }

};

var SetGridPayCode = function (index, row, modelScopeName, gridId, scope) {
    debugger
    function GetFormat0(value, digit) {
        var attr = new Array();
        if (value && $.trim((value + "")).toUpperCase() != "NULL") {
            if (value.length > digit) {
                value = value.substr(0, digit);
                attr.push(value);
            } else {
                attr.push(value);
                for (var i = 0; i < digit - value.length; i++) {
                    attr.push("0");
                }
            }
        } else {
            for (var i = 0; i < digit; i++) {
                attr.push("0");
            }
        }
        return attr.join('');
    };
    var strfields = $.format("{0}-IsGuoKu,{0}-FinanceCode,{0}-ExtraCode,{0}-EconomyClassKey,{0}-ExpendTypeKey,{0}-BGSourceKey,{0}-BGType,{0}-IsProject", modelScopeName);
    var fields = strfields.split(',');
    var isGouKu = row[scope + '-CN_PaymentNumber-IsGuoKu'] + '' || '';
    if (isGouKu.toLocaleLowerCase() != 'true') {//是 国库才能生成财政支付码
        return;
    }
    var czzfCode = "";
    var yslxCode = "";

    /*判断要不要加后面的3个0 跟李总确认 根据功能分类判断是否为项目 如果项目为否则去挑3个000*/
    var d = row[fields[7]];
    var value = "";
    if (d == "0" || d == "false" || d == "False" || d == "否" || d == false) {
        yslxCode = "1";
        czzfCode = "1";
    }
    else {
        yslxCode = "2";
        czzfCode = "2";
    }



    //功能分类
    value = row[fields[1]];
    if (value == undefined) return '';
    //FinanceCode
    czzfCode += GetFormat0(value, 7);
    //经济分类
    value = row[fields[3]];
    if (value == undefined) return '';
    czzfCode += value;
    /*加2位的预算码2015-12-24*/
    
    var guidBGCode = row[gridId + '-BGCodeKey'] + '';
    czzfCode += window.bgCodeKeyDic[guidBGCode];
    /*加2位的预算码2015-12-24*/
    //支出类型
    value = row[fields[4]];
    if (value == undefined) return '';
    czzfCode += value;
    //预算来源bgSourceColID
    value = row[fields[5]] || 1;
    if (value == undefined) return '';
    czzfCode += value;
    //项目扩展码
    value = row[fields[2]];
    //项目附加码
    //xjbxd-CN_PaymentNumber-ExtraCodeEx
    if (!value) {
        var filed = scope + '-CN_PaymentNumber-ExtraCodeEx';
        value = row[filed];
    }
    value ? czzfCode += value : czzfCode += "000";


    if (czzfCode && (czzfCode.length == 16 || czzfCode.length == 18)) {
        if (d == "0" || d == "false" || d == "False" || d == "否" || d == false) {
            value = czzfCode.substr(0, czzfCode.length - 3);
        }
        else {
            value = czzfCode;
        }
    } else {
        value = "";
    }
    //修改行
    return value;
};
//Grid编辑以后生成支付码
$.gridEdit = function (i, row, changes) {
    debugger
    var scope = '', gridId = '', scopeModelName = '';
    var optionsGrid = $(this).edatagrid('options');
    var payCodeConfig = optionsGrid.payCodeConfig;
    scope = payCodeConfig[0];
    gridId = payCodeConfig[1];
    scopeModelName = payCodeConfig[2];

    var code = SetGridPayCode(0, row, scopeModelName, gridId, scope);
    var gdata = $.data($('#' + gridId)[0], "datagrid");
    var dc = gdata.dc;
    var view2 = dc.view2;
    var filter = $.format('tr[datagrid-row-index={0}] td[field="{1}"] div', i, scope + '-CN_PaymentNumber-PaymentNumber');
    var $div = view2.find(filter);
    if ($div) {
        $div.html(code || '');
        row[scope + '-CN_PaymentNumber-PaymentNumber'] = code || '';
    }
};

/*
为easyui增加自定义的插件
*/
$.extend($.parser.plugins, ['edatagrid', 'checkbox']);
//页面初始化


$(document).ready(function () {
    var scope = $("#initscope").val();
    var status = $("#initstatus").val();
    var guid = $("#initguid").val();
    $.view.init(scope, status, guid);

    $(document).click(function (e) {
        
        if (e.target.tagName.toLowerCase() == "input") {
            $.fn.linkbutton.methods['wholeGridEndEdit'].call(this);
        }
    });
});


window.bgCodeDic =
{
    'EE7E6DD4-7C08-4108-A112-3A8D2EFA1F19' : '01',
    'BFD35918-58ED-4BAD-8161-0BB06B1764FC' : '03',
    '3831724D-E059-4072-B391-C70E8B07C679' : '03',
    '4ADBA77B-5882-4FE5-9545-1E7C6946653B' : '03',
    '03AF05A9-DD75-4AF8-A106-75F27CF772C0' : '03',
    '9534D074-AA0E-4F14-8363-78E1B5426407' : '03',
    'DE2E3761-DC78-48B4-8EA4-DDF5A4C91C83' : '03',
    'D4C4D20A-D34B-4820-BF59-1062FE90A49A' : '03',
    '3481ECEF-B35F-42EE-86FB-8114C90F03F0' : '03',
    'B17A7DA3-ED6A-4D07-98E8-501B7ADA67BB' : '03',
    '223D59F0-BCF4-4AE0-9FFA-1E919ACCF6D0' : '03',
    '3D330E29-4FC0-4E20-BA4C-10D6BB3A3D37' : '03',
    'DA74089B-1736-4EF3-ABEF-D446544EA56C' : '03',
    '77C52B17-17C9-4A55-A53B-E3941E3D40AD' : '99',
    'EC2B9F53-D53B-4AE3-A345-7A4A12836648' : '03',
    '5D5A797E-EDF4-4CED-BCBF-62A98B084DEC' : '07',
    'C9C895A6-47DC-41C0-A627-E160CFC542C1' : '07',
    '58D9CDE4-0BB9-4BA1-BB3C-04D736100DE3' : '07',
    'D3DCF8BF-7226-41F5-A0B6-A48CC8241506' : '07',
    '445A0155-31DB-47B4-950A-C66B09B1D5B9' : '99',
    'BBE7A940-6853-4202-836B-A7CE7F879C8E' : '99',
    '12AF5DEF-409A-4AD6-BB78-01AC3C720B8E' : '99',
    '59A54084-55E1-4660-838E-6C136F307435' : '99',
    '1EBDCD4B-2FF2-422B-903D-4E4926E3A695' : '99',
    '704856F9-8189-4B43-8128-BF5FEF538E1B' : '99',
    '4B53C387-72A8-4A0E-BF03-1612783CC01F' : '99',
    '97C1145B-05D9-4249-B13D-FD68C1008C86' : '99',
    '26EA95BE-A839-4614-86CB-8DB95431EB32' : '99',
    'C41950AB-7651-4B8D-859D-44F9213627A3' : '99',
    '387649C9-B499-49F0-88D9-1CDD0B68A503' : '99',
    'AD3512BE-E4E3-4962-B0D2-5FE56669F3AE' : '99',
    '3B204039-C73E-46FD-A7D1-3EF54E033E36' : '99',
    '37599FA1-470B-44D9-A23F-D4B3EBC7F6ED' : '99',
    '66467FDC-42AB-4750-A809-C11D07228B85' : '99',
    'E0843625-4E05-4D41-98F5-0420E7657EB1' : '99',
    '1C4CEFEF-59E6-4623-84DC-45D1D0FFA644' : '99',
    '6FFAE434-6C85-4B2C-8C6A-CFB6D55BADB4' : '99',
    '372E0417-787B-420C-AFF2-B815C6AE9096' : '99'
}

window.bgCodeKeyDic =
{
    '10101'    : '01',
    '1020101'  : '03',
    '1020102'  : '03',
    '1020103'  : '03',
    '1020104'  : '03',
    '1020105'  : '03',
    '1020106'  : '03',
    '1020107'  : '03',
    '1020108'  : '03',
    '1020109'  : '03',
    '1020110'  : '03',
    '1020111'  : '03',
    '1020112'  : '03',
    '1020113'  : '99',
    '1020201'  : '03',
    '102020201': '07',
    '102020202': '07',
    '102020203': '07',
    '102020204': '07',
    '1030101'  : '99',
    '1030102'  : '99',
    '1030103'  : '99',
    '1030104'  : '99',
    '1030105'  : '99',
    '1030106'  : '99',
    '1030107'  : '99',
    '1030108'  : '99',
    '1030109'  : '99',
    '1030110'  : '99',
    '1030111'  : '99',
    '10302'    : '99',
    '10303'    : '99',
    '10304'    : '99',
    '10305'    : '99',
    '10306'    : '99',
    '10307'    : '99',
    '10308'    : '99',
    '10311'    : '99'

}