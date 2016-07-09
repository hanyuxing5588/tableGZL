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
    'D3C1F74D-C420-4F32-8519-F0DDC3522C36': '01', //0101	基本工资
    '9086B73B-2A4A-49FD-BE69-F9E8DEEE56AA': '02', //0102	津贴补贴
    '1B3AE4F3-9836-435B-802E-2AFEC8081327': '01', //0201	办公费
    '0575D1C9-ED1B-47E1-8904-BF1709D2A064': '02', //0202	印刷费
    '49AC9818-87BD-4FD0-AEFB-2DF01E5ECA17': '03', //0203	咨询费
    '69A6A1D3-C386-4B34-9AE9-058D7D5E46FC': '04', //0204	手续费
    'C79818AA-50E9-4EC8-B815-1A37276FA1B0': '05', //0205	水费
    'D18FBF0D-1478-4466-BD10-8E072106C319': '06', //0206	电费
    '1D151F5D-9BA2-4C32-B4B4-6477F3016595': '07', //0207	邮电费
    '761A358F-C3CC-40BF-AE10-155B22CE06A5': '08', //0208	取暖费
    '3D624A59-42AF-4D25-9ACE-E92505C36700': '09', //0209	物业管理费
    '3F074A28-0851-4121-A397-243820CC78EF': '11', //0219	差旅费
    '1ED70D60-768B-4A3D-815F-1316BC39C8EB': '12', //0212	因公出国(境)费用
    '450C8D20-CD10-44A8-A8B0-72754C4E7EE3': '13', //0213	维修(护)费
    '8B67977C-D323-4115-817D-52BF13F40DE9': '14', //0214	租赁费
    'D187B044-1870-48C7-AD8D-CE0CBD256F81': '15', //0215	会议费
    '7C35F2C1-319E-49AB-BE21-20F8569EBBC7': '16', //0216	培训费
    '11A0E53A-2A5A-4667-B53D-A0BD4467C70B': '18', //0218	专用材料费
    '631C7FAC-EE60-4C42-A964-A6DAA015367F': '26', //0220	劳务费
    '35A7221B-8BA6-4ADF-A998-21B82B5A1107': '27', //0221	委托业务费
    '862CF5FB-B16E-4386-805C-9495730BEB4E': '28', //0222	工会经费
    '974ADE78-24F5-470F-A9DC-30592A266084': '29', //0223	福利费
    '41A6F987-A30E-43BD-A5E0-70E862C79A6D': '31', //0210	公务用车运行维护费
    '0831E5A0-90D7-485B-8E79-FF51D5F186D5': '99', //0224	其他商品和服务支出
    '9CC52834-2F67-4598-962C-6AA57DF4B57F': '01', //0301	离休费
    '1054DD7F-C7D0-4048-98BE-A67CD25DC32C': '02', //0302	退休费
    'DE813440-6CC9-4D0A-9FDD-A5BFC4DF41DC': '03', //0303	退职(役)费
    'F73E6365-FBFD-47EC-BA05-1A1A588618FB': '04', //0304	抚恤金
    'FC4E96B7-B6A0-41C1-BD82-AC8CBEA0D014': '05', //0305	生活补助
    'D0BCC1F0-8B40-47CB-B57F-FA2269E2A720': '07', //0306	医疗费
    '9A8C946A-4E56-424F-B8F7-EE87EE0C936E': '09', //0311	奖励金
    'A3655719-D5FB-4148-AEC8-A03C0DD6714F': '11', //0307	住房公积金
    '9CD55F0F-7183-43C4-B81B-11B43B1EA5DF': '12', //0308	提租补贴
    '5AFFE3EC-79C5-4C4C-AD5D-F53261B9C236': '13', //0309	购房补贴
    '667564C0-902F-4996-8E33-8A96016AF049': '02', //0401	办公设备购置
    'A0D6B00C-C329-46B6-B1F4-CCC157BCB3B0': '03', //0402	专用设备购置
    'C6BB1C56-5C18-4BA4-B87B-76E75F3B92BE': '06', //0404	大型修缮
    '05EA57E9-5AA9-4E49-B78E-F6B650EEBC1C': '13', //0409	公务用车购置
    '3C53CA50-1163-6E47-AD00-1389227D5081': '19', //0410	其他交通工具购置
    'D268491E-E294-4BF8-BF0A-58191CE0CC7A': '99', //0406	其他资本性支出
    '59690509-63F7-44BF-9EC0-B4A00D27D048': '01', //0501	建筑安装工程投资
    'A2049913-C041-4C93-9DA5-98ABD8339618': '02', //0502	专用设备(安装)
    'F8E35E18-F799-41D2-A825-16111ACE10BD': '02', //0503	其他辅助设备(安装)
    '5C39B062-3342-604A-91B9-74BFD45AD893': '02', //0504	设备配件（安装）
    'C2DEB5FF-6386-4CEA-94C7-B0F03A8F4A0D': '02', //0505	设备运杂费(安装)
    'A5052E9A-FFC8-4412-9760-EDE7721358D9': '02', //0506	设备检测费(安装)
    'E87B59F4-FBEC-47AF-BE0E-EDF37D8123F2': '02', //0508	专用设备(不安装)
    '44B16B68-0C4E-D44A-8139-2625A4A2CDC5': '02', //0509	其他辅助设备（不安装）
    '4ECC763C-E82E-6444-9BE7-DA78A48C2D61': '02', //0513	野外装备
    '675E7B13-B893-C84F-81DC-A5F87D6F3F21': '02', //0514	埋石工具
    'DDE8F9B2-6543-4583-A009-57A5CFEE0734': '02', //0515	其他工具及器具
    '386FEB66-7B86-E341-AB44-24CFBA9C6F2D': '07', //0511	专业软件（不安装）
    'A0A014C1-C530-C945-BA8C-5C2E73EE798E': '07', //0512	定制软件（不安装）
    '6E1B0855-E6A2-422A-8369-D4922004C6CB': '19', //0510	交通工具(不安装)
    '99D57856-C7E9-4C6D-93A4-6B52251D12AB': '99', //0516	办公费-基建
    '1BAB011F-3C31-401B-A6E4-FDDDB9021758': '99', //0517	差旅费-基建
    '18AA9220-4ADD-42B4-88D9-062F0A620199': '99', //0518	交通费-基建
    '92FF83F5-F700-804B-A325-B1F666124245': '99', //0519	邮电费-基建
    'C4BA5FEC-1C88-4941-A661-88085EEF4A21': '99', //0520	印刷费-基建
    '8FD65DA3-F3F6-44EF-9FAB-08E378DC1EAD': '99', //0522	劳务费-基建
    '445337FC-C726-44FF-9A09-C62A1668CF86': '99', //0523	培训费-基建
    '50683B35-9540-4F59-9E9B-37D000FD8E33': '99', //0524	会议费-基建
    'BE01783C-A06C-4663-A5D5-5A098E9C9928': '99', //0525	招待费-基建
    'ADA15DF4-ACA8-774C-BEE8-6E7946D8878B': '99', //0526	其他建设单位管理费-基建
    'F44FAC51-26AB-C743-B7D7-CA7010F9E3DA': '99', //0527	维修费-基建
    '25689781-1766-904B-8A51-08FE1828FEE6': '99', //0528	测试费-基建
    'CD35345D-AF6E-B74E-B060-3A84C2D7A42D': '99', //0529	咨询费-基建
    'B934F54C-41DB-1B4A-890E-66AD8A1FCF9B': '99', //0530	专用材料-基建
    '9E99A361-F818-4EB5-B770-C1DED846E583': '99', //0531	前期工作咨询费
    'AFE09F22-4DE7-473E-9085-A30D30BC5D8C': '99', //0532	初步设计费
    'E90426CD-6971-3B48-87D5-E7108AF3A8B3': '99', //0535	工程监理费
    'DA40EFB3-8B8C-F64E-A107-27327D08662A': '99', //0538	技术培训费
    '5279A468-C76E-714A-8143-225008B34CB6': '99', //0539	建设期间线路租用
    '04A89C54-F92B-4D83-824B-DBA35A92C778': '99', //0540	土地使用费
    'E98454DC-A4C6-43CF-BB45-0CEDCE9D6DC9': '99', //0310    其他对个人和家庭的补助
    'EEDC22DF-BCF2-F24F-A4D1-16296573F797': '19', //0403    交通工具购置
    'E787DACD-854C-DA4E-B05B-4EE619C52C2B': '07', //0507    系统集成及安装调试（安装）
    'BFD73EE7-AE90-47BC-8D43-5B5DCBB142C9': '04', //0103    社会保障费
    '52A20C0D-D682-3445-BD37-62245014FD62': '39', //0226    其他交通费

    'B442777F-4FB1-4EAF-B213-8D095FB3ABEE': '17', //0217    公务招待费
    '73DC2CDD-5200-48BC-8782-BD5C44C8CE7F': '99', //0106    其他工资福利支出
    '0E036204-4EC4-D343-8AB2-C22BF529F93C': '03', //0407    专用设备购置费（基建）
    '6D9C23CE-A8AF-4E62-BCDE-C978CE8E8712': '07', //0405    信息网络购建
    '57A97B17-4EDD-3744-8D8B-E0AEFFF970C1': '99', //0536    招投标代理服务费
    'BFB09D46-3953-4AB2-BCCD-F779ECB9AE37': '99'  //0541    其它待摊费用
}

window.bgCodeKeyDic =
{
    '0101': '01', //0101	基本工资
    '0102': '02', //0102	津贴补贴
    '0201': '01', //0201	办公费
    '0202': '02', //0202	印刷费
    '0203': '03', //0203	咨询费
    '0204': '04', //0204	手续费
    '0205': '05', //0205	水费
    '0206': '06', //0206	电费
    '0207': '07', //0207	邮电费
    '0208': '08', //0208	取暖费
    '0209': '09', //0209	物业管理费
    '0219': '11', //0219	差旅费
    '0212': '12', //0212	因公出国(境)费用
    '0213': '13', //0213	维修(护)费
    '0214': '14', //0214	租赁费
    '0215': '15', //0215	会议费
    '0216': '16', //0216	培训费
    '0218': '18', //0218	专用材料费
    '0220': '26', //0220	劳务费
    '0221': '27', //0221	委托业务费
    '0222': '28', //0222	工会经费
    '0223': '29', //0223	福利费
    '0210': '31', //0210	公务用车运行维护费
    '0224': '99', //0224	其他商品和服务支出
    '0301': '01', //0301	离休费
    '0302': '02', //0302	退休费
    '0303': '03', //0303	退职(役)费
    '0304': '04', //0304	抚恤金
    '0305': '05', //0305	生活补助
    '0306': '07', //0306	医疗费
    '0311': '09', //0311	奖励金
    '0307': '11', //0307	住房公积金
    '0308': '12', //0308	提租补贴
    '0309': '13', //0309	购房补贴
    '0401': '02', //0401	办公设备购置
    '0402': '03', //0402	专用设备购置
    '0404': '06', //0404	大型修缮
    '0409': '13', //0409	公务用车购置
    '0410': '19', //0410	其他交通工具购置
    '0406': '99', //0406	其他资本性支出
    '0501': '01', //0501	建筑安装工程投资
    '0502': '02', //0502	专用设备(安装)
    '0503': '02', //0503	其他辅助设备(安装)
    '0504': '02', //0504	设备配件（安装）
    '0505': '02', //0505	设备运杂费(安装)
    '0506': '02', //0506	设备检测费(安装)
    '0508': '02', //0508	专用设备(不安装)
    '0509': '02', //0509	其他辅助设备（不安装）
    '0513': '02', //0513	野外装备
    '0514': '02', //0514	埋石工具
    '0515': '02', //0515	其他工具及器具
    '0511': '07', //0511	专业软件（不安装）
    '0512': '07', //0512	定制软件（不安装）
    '0510': '19', //0510	交通工具(不安装)
    '0516': '99', //0516	办公费-基建
    '0517': '99', //0517	差旅费-基建
    '0518': '99', //0518	交通费-基建
    '0519': '99', //0519	邮电费-基建
    '0520': '99', //0520	印刷费-基建
    '0522': '99', //0522	劳务费-基建
    '0523': '99', //0523	培训费-基建
    '0524': '99', //0524	会议费-基建
    '0525': '99', //0525	招待费-基建
    '0526': '99', //0526	其他建设单位管理费-基建
    '0527': '99', //0527	维修费-基建
    '0528': '99', //0528	测试费-基建
    '0529': '99', //0529	咨询费-基建
    '0530': '99', //0530	专用材料-基建
    '0531': '99', //0531	前期工作咨询费
    '0532': '99', //0532	初步设计费
    '0535': '99', //0535	工程监理费
    '0538': '99', //0538	技术培训费
    '0539': '99', //0539	建设期间线路租用
    '0540': '99', //0540	土地使用费
    '0310': '99', //0310    其他对个人和家庭的补助
    '0403': '19', //0403    交通工具购置
    '0507': '07', //0507    系统集成及安装调试（安装）
    '0103': '04', //0103    社会保障费
    '0226': '39', //0226    其他交通费

    '0217': '17', //0217    公务招待费
    '0106': '99', //0106    其他工资福利支出
    '0407': '03', //0407    专用设备购置费（基建）
    '0405': '07', //0405    信息网络购建
    '0536': '99', //0536    招投标代理服务费
    '0541': '99'  //0541    其它待摊费用
}