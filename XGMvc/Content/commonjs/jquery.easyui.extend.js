


/*优化记录
2014-2-24
*/
/*
扩展validatebox
*/
$.extend($.fn.validatebox.methods, {
    keyboardRespFun: function (e) {
        if (e.keyCode == "13") {

        }
    },
    //转换当前可编辑状态


    alterStatus: function (jq, status) {
        return jq.each(function () {
            if (!status) return;
            var opts = $(this).validatebox('options');
            //绑定控件 一般的情况下只绑定一次就可以 
            //如果需要持续解绑 和 绑定 那么就设置 mustBind为true即可
            if (!opts.customBind || opts.mustBind)
                $.view.bind.call($(this), 'validatebox');
            var statusArr = opts.forbidstatus;
            if (statusArr && (statusArr.indexOf(-1) >= 0 || statusArr.exist(status))) {

                $(this).validatebox('disabled');
                return;
            }

            $(this).validatebox('enabled');
        });
    },
    disabled: function (jq) {
        return jq.each(function () {

            $(this).attr('disabled', true);
        })
    },
    enabled: function (jq) {
        return jq.each(function () {

            $(this).attr('disabled', false);
        })
    },
    setValue: function (jq, value) {
        $(jq).val(value);
    },
    getValue: function (jq) {
        return $(jq).val();
    },
    setText: function (jq, text) {
        var opts = $(jq).validatebox('options');
        var changeColor = function (jq, text) {
            if (!text) return '';
            var style = $(jq).attr('style');
            if (!style) return text;
            text = text.toString();
            style = style.toString();
            if (text.indexOf('-') == 0) {
                if (style.indexOf(';color:red;') < 0) {
                    $(jq).attr('style', style + ";color:red;")
                }
            } else {
                if (style.indexOf(';color:red;') > 0) {
                    $(jq).attr('style', style.replace(";color:red;", ''));
                }
            }
            //修改validatebox 的setText 方法 sxh 2014/04/02 15:17
            //当输入金额中有负号(-)且索引值是0，那么将字符串中的负号(-)去掉，否则正常返回  
            if (text.indexOf('-') > 0) {
                return text;
            } else {
                return text = text.replace('-', '');
            }
        }
        if (!opts.changeColor) {
            text = changeColor(jq, text);
        }
        if (opts.chinese) {
            text = text.replace('-', '');
            text = this.moneytransferchinese(text);
        }
        $(jq).val(text);
    },
    getText: function (jq) {
        return $(jq).val();
    },
    //获得格式数据
    getData: function (jq) {
        var vals = $(jq).attr('id');
        if (vals) {
            var vals = vals.split('-');
            if (vals) {
                var result = { m: vals[1], n: vals[2], v: $(jq).validatebox('getValue') };
                return result;
            }
        }
        return null;
    },
    //加载格式数据
    setData: function (jq, hs) {
        return jq.each(function () {

            var idAttr = $(this).attr('id').split('-');
            if (idAttr.length < 2) return; //没有属性值的控件  不是合法后台数据属性控件

            if (!idAttr) return;
            if (hs) {
                var a = hs[idAttr[1]], b;
                if (a && a.hasOwnProperty(idAttr[2])) {
                    b = a[idAttr[2]];
                    var state = $.view.getStatus(idAttr[0]); //有数据集合 并且是新建 且有默认值

                    var opts = $(this).validatebox('options');
                    if (state == 1 && opts.defalutValue) {
                        b = opts.defalutValue || '';
                    }
                    //                    $(this).validatebox('setText', b);
                    $(this).val(b); //sxh 2014/03/17 14:37
                    return;
                }
                return;
            }
            //清空所有值

            $(this).validatebox('setText', '');
        })
    },
    //金额转中文大写

    moneytransferchinese: function (money) {
        
        try {
            money = money.toString().replace(/,/g, '');
            money = this.moneytrim(money);
            var i = 1;
            var dw2 = ["", "万", "亿"]; //大单位

            var dw1 = ["拾", "佰", "仟"]; //小单位

            var dw = ["零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖"]; //整数部分用

            //以下是小写转换成大写显示在合计大写的文本框中
            //分离整数与小数

            var source = this.moneysplits(money);
            var num = source[0];
            var dig = source[1];
            //转换整数部分
            var k1 = 0; //计小单位
            var k2 = 0; //计大单位 
            var sum = 0;
            var str = "";
            var len = source[0].length; //整数的长度

            for (i = 1; i <= len; i++) {
                var n = source[0].charAt(len - i); //取得某个位数上的数字
                var bn = 0;
                if (len - i - 1 >= 0) {
                    bn = source[0].charAt(len - i - 1); //取得某个位数前一位上的数字

                }
                sum = sum + Number(n);
                if (sum != 0) {
                    str = dw[Number(n)].concat(str); //取得该数字对应的大写数字，并插入到str字符串的前面
                    if (n == '0') sum = 0;
                }
                if (len - i - 1 >= 0) {//在数字范围内
                    if (k1 != 3) {//加小单位
                        if (bn != 0) {
                            str = dw1[k1].concat(str);
                        }
                        k1++;
                    }
                    else {//不加小单位，加大单位
                        k1 = 0;
                        var temp = str.charAt(0);
                        if (temp == "万" || temp == "亿")//若大单位前没有数字则舍去大单位

                            str = str.substr(1, str.length - 1);
                        str = dw2[k2].concat(str);
                        sum = 0;
                    }
                }
                if (k1 == 3) {//小单位到千则大单位进一
                    k2++;
                }
            }
            //转换小数部分
            var strdig = "", strZ = "整";
            if (dig != "") {
                var n = dig.charAt(0);
                if (n != 0) {
                    strdig += dw[Number(n)] + "角"; //加数字
                } else {
                    var n1 = dig.charAt(1);
                    if (n1 != 0) {
                        strdig += "零"; //加数字 2015 12 14 处理如￥325.04应写成人民币叁佰贰拾伍元零肆分。
                    }
                }
                var n = dig.charAt(1);
                if (n != 0) {
                    strdig += dw[Number(n)] + "分"; //加数字

                    strZ = "";
                }
            }
            if (str == "") {
                str = strdig;
            }
            else {
                str += "元" + strdig;
            }
        }
        catch (e) {
            return "";
        }
        return str == "" ? str : str + strZ;
    },
    //拆分整数与小数

    moneysplits: function (money) {
        var value = new Array('', '');
        temp = money.split(".");
        for (var i = 0; i < temp.length; i++) {
            value[i] = temp[i];
        }
        return value;
    },
    //从左到右去掉0直到第一个非0数字或.
    moneytrim: function (money) {
        var len = money.length; //长度
        var str = money;
        for (i = 0; i <= len; i++) {
            var n = money.charAt(i); //取得某个位数上的数字
            if (n != '0' || n == '.') {
                str = money.substring(i, len);
                break;
            }
        }
        return str;
    },
    //验证validatebox
    verifyData: function () {
        var msg = "";
        if ($(this).val() == "") {
            var id = $(this).attr('id');
            var labid = "#lbl-" + id.split('-')[2];
            var labelVal = $(labid).text().replace('*', '').NoSpace();
            msg = $.trim(labelVal) + "不能为空！";
        }
        return msg;
    }
});
$.extend($.fn.validatebox.defaults, {
    backValue: "", //后台字段
    chinese: false,
    scope: null,
    forbidstatus: null,
    bindmethod: null
});
$.extend($.fn.validatebox.defaults.rules, {
    chs: {
        validator: function (value, param) {
            return /^[\u0391-\uFFE5]+$/.test(value);
        },
        message: '请输入汉字'
    },
    zipcode: {
        validator: function (value, param) {
            return /^[0-9]\d{5}$/.test(value);
        },
        message: '邮政编码不存在'
    },
    qq: {
        validator: function (value, param) {
            return /^[1-9]\d{4,10}$/.test(value);
        },
        message: 'QQ号码不正确'
    },
    bankNumber: {//验证银行账号 银行账号存在16位和19位

        validator: function (value) {

            return /^\d{16}|\d{19}$/.test(value);
        },
        message: '银行账号只能是数字!'
    },
    tel_phone: {
        validator: function (value) {
            
            if (value != null && value != '') {
                return /^((\d{11})|^((\d{7,8})|(\d{4}|\d{3})-(\d{7,8})|(\d{4}|\d{3})-(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1})|(\d{7,8})-(\d{4}|{\d{3}|\d{2}|\d{1}))$)/.test(value);
            }
            return false;
        },
        message: '手机或固定电话的格式不正确'

    },
    phone: {
        validator: function (value, param) {

            return /^((\(\d{2,3}\))|(\d{3}\-))?13\d{9}$/.test(value);
        },
        message: '手机号码不正确'
    },
    tel: {
        validator: function (value) {

            return /^((\(\d{2,3}\))|(\d{3}\-))?(\(0\d{2,3}\)|0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,4})?$/i.test(value);
        },
        message: '格式不正确，请使用下面格式：020-88888888'
    },
    loginName: {
        validator: function (value, param) {
            return /^[\u0391-\uFFE5\w]+$/.test(value);
        },
        message: '登录名称只允许汉字、英文字母、数字及下划线。'
    },
    username: {//验证用户名

        validator: function (value) {
            return /^[a-zA-Z][a-zA-Z0-9_]{5,15}$/i.test(value);
        },
        message: '用户名不合法(字符开头，允许6-16字节，允许字母下划线)'
    },
    faxno: {//验证传真号

        validator: function (value) {

            return /^((\d{2,3}\))|(\d{3}\-))?(\(0\d{2,3}\)|0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,4})?$/i.test(value);
        },
        message: '传真号不正确'
    },
    ip: {//验证IP地址
        validator: function (value) {
            return /d+.d+.d+.d+/i.test(value);
        },
        message: 'IP地址格式不正确'
    },
    intOrFloat: {//验证整数或小数

        validator: function (value) {
            return /^\d+(\.\d+)?$/i.test(value);
        },
        message: '请输入数字，并确保格式正确'
    },
    currency: {
        validator: function (value) {
            return /^\d+(\.\d+)?$/i.test(value);
        },
        message: '货币格式不正确'
    },
    integer: {//验证整数 可正负数
        validator: function (value) {
            return /^([+]?[0-9])|([-]?[0-9])+\d*$/i.test(value);
        },
        message: '请输入整数'
    },
    age: {//验证年龄
        validator: function (value) {
            return /^(?:[1-9][0-9]?|1[01][0-9]|150)$/i.test(value);
        },
        message: '年龄必须是0到150之间的整数'
    },
    chinese: {//验证中文
        validator: function (value) {
            return /^[\A-\￥]+$/i.test(value);
        },
        message: '请输入中文'
    },
    english: {//验证英语
        validator: function (value) {
            return /^[A-Za-z]+$/i.test(value);
        },
        message: '请输入英文'
    },
    unnormal: {//验证是否包含空格和非法字符

        validator: function (value) {
            return /.+/i.rest(value);
        },
        message: '输入值不能为空和包含其他非法字符'
    },
    safepass: {
        validator: function (value, param) {
            return safePassword(value);
        },
        message: '密码由字母和数字组成，至少6位'
    },
    equalTo: {
        validator: function (value, param) {
            return value == $(param[0]).val();
        },
        message: '两次输入的字符不一至'
    },
    number: {
        validator: function (value, param) {
            return /^\d+$/.test(value);
        },
        message: '请输入数字'
    },
    integer: {
        validator: function (value, param) {
            return /^[0-9]*[1-9][0-9]*$/.test(value);
        },
        message: '请输入正整数'
    },
    isDocNum: {
        validator: function (value, param) {
            if (!value) return true;
            return /^[0-9]*$/.test(value);
        },
        message: '请输入正确的单号'
    },
    idcard: {
        validator: function (value, param) {
            return idCard(value);
        },
        message: '请输入正确的身份证号码'
    },
    safePassword: {
        validator: function (value, param) {
            return !(/^(([A-Z]*|[a-z]*|\d*|[-_\~!@#\$%\^&\*\.\(\)\[\]\{\}<>\?\\\/\'\"]*)|.{0,5})$|\s/.test(value));
        },
        message: '密码由字母和数字组成，至少6位'
    },
    idcard2: {
        validator: function (idcard, param) {
            var idcard = value;
            var Errors = new Array(
"验证通过!",
"身份证号码位数不对!",
"身份证号码出生日期超出范围或含有非法字符!",
"身份证号码校验错误!",
"身份证地区非法!"
);
            var area = { 11: "北京", 12: "天津", 13: "河北", 14: "山西", 15: "内蒙古", 21: "辽宁", 22: "吉林", 23: "黑龙江", 31: "上海", 32: "江苏", 33: "浙江", 34: "安徽", 35: "福建", 36: "江西", 37: "山东", 41: "河南", 42: "湖北", 43: "湖南", 44: "广东", 45: "广西", 46: "海南", 50: "重庆", 51: "四川", 52: "贵州", 53: "云南", 54: "西藏", 61: "陕西", 62: "甘肃", 63: "青海", 64: "宁夏", 65: "新疆", 71: "台湾", 81: "香港", 82: "澳门", 91: "国外" }

            var idcard, Y, JYM;
            var S, M;
            var idcard_array = new Array();
            idcard_array = idcard.split("");
            //地区检验

            if (area[parseInt(idcard.substr(0, 2))] == null) {
                alert(Errors[4]);
                return false;
            }
            //身份号码位数及格式检验

            switch (idcard.length) {
                case 15:
                    if ((parseInt(idcard.substr(6, 2)) + 1900) % 4 == 0 || ((parseInt(idcard.substr(6, 2)) + 1900) % 100 == 0 && (parseInt(idcard.substr(6, 2)) + 1900) % 4 == 0)) {
                        ereg = /^[1-9][0-9]{5}[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|[1-2][0-9]))[0-9]{3}$/; //测试出生日期的合法性

                    } else {
                        ereg = /^[1-9][0-9]{5}[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|1[0-9]|2[0-8]))[0-9]{3}$/; //测试出生日期的合法性

                    }
                    if (ereg.test(idcard)) return true;
                    else {
                        alert(Errors[2]);
                        return false;
                    }
                    break;
                case 18:
                    //18位身份号码检测

                    //出生日期的合法性检查 
                    //闰年月日:((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|[1-2][0-9]))
                    //平年月日:((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|1[0-9]|2[0-8]))
                    if (parseInt(idcard.substr(6, 4)) % 4 == 0 || (parseInt(idcard.substr(6, 4)) % 100 == 0 && parseInt(idcard.substr(6, 4)) % 4 == 0)) {
                        ereg = /^[1-9][0-9]{5}(19|20)[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|[1-2][0-9]))[0-9]{3}[0-9Xx]$/; //闰年出生日期的合法性正则表达式
                    } else {
                        ereg = /^[1-9][0-9]{5}(19|20)[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|1[0-9]|2[0-8]))[0-9]{3}[0-9Xx]$/; //平年出生日期的合法性正则表达式
                    }
                    if (ereg.test(idcard)) {//测试出生日期的合法性

                        //计算校验位

                        S = (parseInt(idcard_array[0]) + parseInt(idcard_array[10])) * 7
+ (parseInt(idcard_array[1]) + parseInt(idcard_array[11])) * 9
+ (parseInt(idcard_array[2]) + parseInt(idcard_array[12])) * 10
+ (parseInt(idcard_array[3]) + parseInt(idcard_array[13])) * 5
+ (parseInt(idcard_array[4]) + parseInt(idcard_array[14])) * 8
+ (parseInt(idcard_array[5]) + parseInt(idcard_array[15])) * 4
+ (parseInt(idcard_array[6]) + parseInt(idcard_array[16])) * 2
+ parseInt(idcard_array[7]) * 1
+ parseInt(idcard_array[8]) * 6
+ parseInt(idcard_array[9]) * 3;
                        Y = S % 11;
                        M = "F";
                        JYM = "10X98765432";
                        M = JYM.substr(Y, 1); //判断校验位

                        if (M == idcard_array[17]) return true; //检测ID的校验位
                        else {
                            alert(Errors[3]);
                            return false;
                        }
                    }
                    else {
                        alert(Errors[2]);
                        return false;
                    }
                    break;
                default:
                    alert(Errors[1]);
                    return false;
                    break;
            }
        },
        message: '身份证号非法'
    },
    idCard: {
        validator: function (value, param) {
            if (value.length == 18 && 18 != value.length) return false;
            var number = value.toLowerCase();
            var d, sum = 0, v = '10x98765432', w = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2], a = '11,12,13,14,15,21,22,23,31,32,33,34,35,36,37,41,42,43,44,45,46,50,51,52,53,54,61,62,63,64,65,71,81,82,91';
            var re = number.match(/^(\d{2})\d{4}(((\d{2})(\d{2})(\d{2})(\d{3}))|((\d{4})(\d{2})(\d{2})(\d{3}[x\d])))$/);
            if (re == null || a.indexOf(re[1]) < 0) return false;
            if (re[2].length == 9) {
                number = number.substr(0, 6) + '19' + number.substr(6);
                d = ['19' + re[4], re[5], re[6]].join('-');
            } else d = [re[9], re[10], re[11]].join('-');
            if (!isDateTime.call(d, 'yyyy-MM-dd')) return false;
            for (var i = 0; i < 17; i++) sum += number.charAt(i) * w[i];
            return (re[2].length == 9 || number.charAt(17) == v.charAt(sum % 11));
        },
        message: '身份证号非法'
    },
    isDateTime:
    {
        validator: function (value, param) {
            format = param[0] || 'yyyy-MM-dd';
            var input = this, o = {}, d = new Date();
            var f1 = format.split(/[^a-z]+/gi), f2 = input.split(/\D+/g), f3 = format.split(/[a-z]+/gi), f4 = input.split(/\d+/g);
            var len = f1.length, len1 = f3.length;
            if (len != f2.length || len1 != f4.length) return false;
            for (var i = 0; i < len1; i++) if (f3[i] != f4[i]) return false;
            for (var i = 0; i < len; i++) o[f1[i]] = f2[i];
            o.yyyy = s(o.yyyy, o.yy, d.getFullYear(), 9999, 4);
            o.MM = s(o.MM, o.M, d.getMonth() + 1, 12);
            o.dd = s(o.dd, o.d, d.getDate(), 31);
            o.hh = s(o.hh, o.h, d.getHours(), 24);
            o.mm = s(o.mm, o.m, d.getMinutes());
            o.ss = s(o.ss, o.s, d.getSeconds());
            o.ms = s(o.ms, o.ms, d.getMilliseconds(), 999, 3);
            if (o.yyyy + o.MM + o.dd + o.hh + o.mm + o.ss + o.ms < 0) return false;
            if (o.yyyy < 100) o.yyyy += (o.yyyy > 30 ? 1900 : 2000);
            d = new Date(o.yyyy, o.MM - 1, o.dd, o.hh, o.mm, o.ss, o.ms);
            var reVal = d.getFullYear() == o.yyyy && d.getMonth() + 1 == o.MM && d.getDate() == o.dd && d.getHours() == o.hh && d.getMinutes() == o.mm && d.getSeconds() == o.ss && d.getMilliseconds() == o.ms;
            return reVal && reObj ? d : reVal;
            function s(s1, s2, s3, s4, s5) {
                s4 = s4 || 60, s5 = s5 || 2;
                var reVal = s3;
                if (s1 != undefined && s1 != '' || !isNaN(s1)) reVal = s1 * 1;
                if (s2 != undefined && s2 != '' && !isNaN(s2)) reVal = s2 * 1;
                return (reVal == s1 && s1.length != s5 || reVal > s4) ? -10000 : reVal;
            }
        },
        message: '身份证号非法'
    }
});
/*
扩展numberbox
*/
$.extend($.fn.numberbox.methods, {
    //给datagrid的某一列赋值

    setAssociate: function (jq) {
        
        var opts = $(this).numberbox('options');
        var val = $(this).numberbox('getValue');
        var gridconfig = opts.gridassociate;
        if (gridconfig && gridconfig.gridId && gridconfig.map && gridconfig.map.col) {
            var col = gridconfig.map.col;
            var gridId = '#' + gridconfig.gridId;
            var grid = $(gridId);
            var selRow = grid.datagrid("getSelected"); //选中行

            var rowIndex = grid.datagrid("getRowIndex", selRow);
            if (!selRow) return;
            for (var i = 0, j = col.length; i < j; i++) {
                var field = col[i]; //选中行field集合
                var temp = new Number(val).formatThousand(val);
                $('#datagrid-row-r1-2-' + rowIndex + ' div.datagrid-cell.datagrid-cell-c1-' + field).text(temp);
                selRow[field] = val;
            }
        }
    },
    onChangeValue: function (a, b, c) {
        
    },
    //转换当前可编辑状态

    alterStatus: function (jq, status) {
        return jq.each(function () {
            if (!status) return;
            var opts = $(this).numberbox('options');
            if (opts.forbidstatus) {

                for (var i = 0; i < opts.forbidstatus.length; i++) {

                    var c = opts.forbidstatus[i];
                    if (c == -1 || c == status) {
                        if (opts.disabled) return;
                        $.view.bind.call($(this), 'numberbox', true);
                        $(this).numberbox('disabled');
                        return;
                    }
                }
            }
            if (opts.disabled) return;
            $(this).numberbox('enabled');
            $.view.bind.call($(this), 'numberbox');
        });
    },
    setText: function (jq, text) {
        return jq.each(function () {

            var opts = $(jq).numberbox('options');
            var changeColor = function (jq, text) {
                if (!text) return '';
                var style = $(jq).attr('style');
                if (!style) return text;
                text = text.toString();
                style = style.toString();
                if (text.indexOf('-') == 0) {
                    if (style.indexOf(';color:red;') < 0) {
                        $(jq).attr('style', style + ";color:red;")
                    }
                } else {
                    if (style.indexOf(';color:red;') > 0) {
                        $(jq).attr('style', style.replace(";color:red;", ''));
                    }
                }
                return text = text.replace('-', '');
            }
            if (opts.changeColor) {
                text = changeColor(this, text);
            }

            $(this).numberbox('setValue', text);
        });
    },
    getText: function (jq) {
        return jq.each(function () {

            var text = $(jq).numberbox('getValue');
            return text;
        });
    },
    //获得格式数据
    getData: function (jq) {

        var vals = $(jq).attr('id');

        if (vals) {
            var vals = vals.split('-');
            if (vals) {
                var result = { m: vals[1], n: vals[2], v: $(jq).numberbox('getValue') };
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
                var a = hs[idAttr[1]], b;
                if (a && a.hasOwnProperty(idAttr[2])) {
                    b = a[idAttr[2]];
                    $(this).numberbox('setValue', b);
                    return;
                }
                return;
            }
            var opts = $(this).numberbox('options');
            $(this).numberbox('setValue', opts.defalutValue || '0.00');
        })
    },
    //验证numberbox
    verifyData: function () {
        var msg = "";
        if ($(this).val() == "") {
            var id = $(this).attr('id');
            var labid = "#lbl-" + id.split('-')[2];
            var label = $(labid).text().replace('*', '');
            msg = label + "不能为空！";
        }
        return msg;
    },
    //格式化金额函数

    formatNumber: function (jq, parms) {

        if (parms.length <= 0 || parms == "") { return ""; }
        var val = parms.val, isBool = parms.isBool;
        if (isBool) {
            if (!/^(\+|-)?(\d+)(\.\d+)?$/.test(val)) {
                return val;
            }
            var a = RegExp.$1, b = RegExp.$2, c = RegExp.$3;
            var re = new RegExp();
            re.compile("(\\d)(\\d{3})(,|$)");
            while (re.test(b)) b = b.replace(re, "$1,$2$3");
            val = a + "" + b + "" + c;
        } else {
            val = val.replace(/,/g, '')
        }
        return val;
    },
    //如果numberbox的值为0.00 当鼠标focus 的时候自动清空 离开的时候自动赋值成 0.00
    //    focusfun: function (jq) {
    //        console.log(1);
    //        return;
    //        
    //        var id = '#' + $(this).attr('id');
    //        var tempVal = $(id).numberbox('getValue') || 0.00;

    //        $(id).bind('focus', function (v, c, d, e) {
    //            if (tempVal == 0) {
    //                $(id).numberbox('setText', '');
    //            }
    //        });
    //    },
    blurfun: function (jq) {

        return;

        var id = '#' + $(this).attr('id');
        var tempVal = $(id).numberbox('getValue') || 0.00;

        $(this).bind('blur', function (v, c, d, e) {
            if (tempVal == 0) {
                $(id).numberbox('setText', '0.00');
            } else {
                $(id).numberbox('setText', tempVal);
            }
        });
    }
});
$.extend($.fn.numberbox.defaults, {
    scope: null,
    forbidstatus: null,
    bindmethod: null
});

$.extend($.fn.numberbox.defaults.rules, {
    /*和某字段匹配*/
    equalTo: {
        validator: function (value, param) {
            
            try {
                var d = param[1];
                var s = $("#" + param[0]).numberbox('getValue');
                if (!s || !value) return true
                value=value.replace(/,/g,"");
                s=s.replace(/,/g,"");
                if (d) {
                    var flag = parseFloat(value) <= parseFloat(s);
                    return flag;
                } else {
                    var flag = parseFloat(value) >= parseFloat(s);
                    return flag;
                }

            } catch (e) {

                return true;
            }


        },
        message: '结束金额不能小于开始金额！'
    }
});



/*
扩展datebox
*/
$.extend($.fn.datebox.methods, {
    setAssociate: function () {

        var dateVal = $(this).combo('getValue');
        var date = new Date().FormatDate(dateVal);
        $(this).datebox('setValue', date);
    },
    //转换当前可编辑状态

    alterStatus: function (jq, status) {
        return jq.each(function () {
            if (!status) return;
            var opts = $(this).datebox('options');
            if (opts.forbidstatus) {
                for (var i = 0; i < opts.forbidstatus.length; i++) {
                    var c = opts.forbidstatus[i];
                    if (c == -1 || c == status) {
                        $(this).datebox('disabled');
                        return;
                    }
                }
                $(this).datebox('enabled');
            }
            else {
                $(this).datebox('enabled');
            }
        });
    },
    disabled: function (jq) {
        $(jq).datebox({ 'disabled': true });
        $.view.bind.call($(jq), 'datebox', true);
    },
    enabled: function (jq) {
        $(jq).datebox({ 'disabled': false });
        $.view.bind.call($(jq), 'datebox');
    },
    getValue: function (jq) {
        
        return $(jq).combo('getText');
    },
    setText: function (jq, text) {
        
        $(jq).combo('setValue', text);
    },
    getText: function (jq) {
        return $(jq).combo('getValue');
    },
    //获得格式数据
    getData: function (jq) {
        var vals = $(jq).attr('id');
        if (vals) {
            var vals = vals.split('-');
            if (vals) {
                var result = { m: vals[1], n: vals[2], v: $(jq).combo('getText') };
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
                var a = hs[idAttr[1]], b;
                if (a && a.hasOwnProperty(idAttr[2])) {
                    b = a[idAttr[2]];
                    if (b) {
                        var vals = b.split('-');
                        if (vals[1].length < 2) {
                            vals[1] = "0" + vals[1];
                        }
                        if (vals[2].length < 2) {
                            vals[2] = "0" + vals[2];
                        }
                        b = vals.join('-');
                    }
                    $(this).datebox('setText', b);
                    $(this).datebox('setValue', b);
                    return;
                }
                return;
            }
            var opts = $(this).datebox('options');
            var val = opts.tempValue ? opts.tempValue : '';
            $(this).datebox('setText', val);
            $(this).datebox('setValue', val);
        });
    },
    //验证datebox
    verifyData: function () {

        var opts = $(this).datebox('options');
        opts.required = true;
        var msg = "";
        var TempVal = $(this).datebox('getValue');
        if (TempVal == "") {
            var id = $(this).attr('id');
            var labid = "#lbl-" + id.split('-')[2];
            var labelVal = $(labid).text().replace('*', '');
            msg = $.trim(labelVal) + "不能为空！";
        }
        return msg;
    }
});
$.extend($.fn.datebox.defaults, {
    scope: null,
    forbidstatus: null
});

//验证开始时间是否大于结束时间

$.extend($.fn.datebox.defaults.rules, {
    TimeCheck: {
        validator: function (value, param) {
            try {
             
                var d = param[1];
                var s = $("#" + param[0]).combo('getValue');
                if (value === s) return true;
                if (!s || !value) return true
                if (d) {
                    var flag = new Date(value.replace('-','/')).getTime() <= new Date(s.replace('-','/')).getTime();
                    return flag;
                } else {
                    var flag = new Date(value.replace('-', '/')).getTime() >= new Date(s.replace('-', '/')).getTime();
                    return flag;
                }
            } catch (e) {

                return true;
            }
        },
        message: "结束日期不能小于开始日期"
    }
});

/*
扩展tree
*/
$.extend($.fn.tree.methods, {
    setAssociate: function (node) {
        var opts = $(this).tree('options');
        //重新设置表格行默认值
        if (!opts.notexpand) {//配置notexpand有熟悉 就不折叠 默认折叠
            $(this).tree(node.state === 'closed' ? 'expand' : 'collapse', node.target);
            node.state = node.state === 'closed' ? 'open' : 'closed';
        }
        
        if (opts.griddefaultassociate) {
            var r = $(this).tree('getSelected');
            if (r == undefined || r == null) return;
            r = r.attributes;
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

        if (opts.associate) {
            var r = $(this).tree('getSelected');
            if (r && r.attributes && r.attributes.valid) {
                for (var lab in opts.associate) {
                    var valuefield = opts.associate[lab][0];
                    var textfield = opts.associate[lab][1];
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
                            if (plugin == "combogrid")//为了有时候分页加载当前页数据
                            {
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

        var treeconfig = opts.gridassociate;
        debugger
        if (treeconfig && treeconfig.gridId && treeconfig.map) {
            var r = $(this).tree('getSelected');
            if (!r) return;
            var rData = r.attributes;
            var map = treeconfig.map;
            if (rData && rData.valid) {
                var gridId = '#' + treeconfig.gridId, params = { map: map, treeData: rData };

                treeconfig.append ? $(gridId).edatagrid('addRow', params) : $(gridId).edatagrid('updateSelectedRow', params);
            }
        }

       
    },
    //历史查询 事件绑定方法
    historySelect: function (r) {

        var tree = $(this);
        var treeCondition = { treeModel: r.attributes.m, treeValue: r.id };
        var parpms = tree.tree('getParms', 'historySelect');
        if (!parpms || parpms.length < 4) return;
        var fun = $.fn.linkbutton.methods["getHistoryByFilter"];
        fun(parpms[0], parpms[1], parpms[2], parpms[3], treeCondition);
    },
    //转换当前可编辑状态

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
    },
    disabled: function (jq) {
        $.view.bind.call($(jq), 'tree', true);
    },
    enabled: function (jq) {
        $.view.bind.call($(jq), 'tree');
    },
    setValue: function (jq, value) {
    },
    getValue: function (jq) {
    },
    setText: function (jq, text) {
    },
    getText: function (jq) {
    },
    getParms: function (jq, methodName) {
        var opts = $(jq).tree('options'), funConfig = opts.bindparms;
        if (funConfig && funConfig[methodName]) {
            return funConfig[methodName];
        }
        return null;
    },
    //获得格式数据
    //    getData: function (jq) { return; },
    //加载格式数据
    setData: function (jq, data) { }
});
$.extend($.fn.tree.defaults, {
    scope: null,
    forbidstatus: null,
    bindmethod: null, // { 'dblclick': ['setAssociate'] }
    associate: null, //{'label1':['field1','field2'],'label2':['field1','field2']},
    gridassociate: null //{gridId:'g1',map:{'a1':'v1','a2':'v2'},append:false}
//    onDblClick: function(node) {  
//            $(this).tree(node.state === 'closed' ? 'expand' : 'collapse', node.target);  
//            node.state = node.state === 'closed' ? 'open' : 'closed';  
//    }
});
/*
插件扩展:searchbox
*/

$.extend($.fn.searchbox.methods, {
    getData: function (jq) {
        var vals = $(jq).attr('id');
        if (vals) {
            var vals = vals.split('-');
            if (vals) {
                var result = { m: vals[1], n: vals[2], v: $(jq).searchbox('getValue') };
                return result;
            }
        }
        return null;
    },
    //加载格式数据
    setData: function (jq, hs) {
        return jq.each(function () {

            var idAttr = $(this).attr('id').split('-');
            if (idAttr.length < 2) return; //没有属性值的控件  不是合法后台数据属性控件



            if (!idAttr) return;
            if (hs) {
                var a = hs[idAttr[1]], b;
                if (a && a.hasOwnProperty(idAttr[2])) {
                    b = a[idAttr[2]];

                    var state = $.view.getStatus(idAttr[0]); //有数据集合 并且是新建 且有默认值


                    var opts = $(this).searchbox('options');
                    if (state == 1 && opts.defalutValue) {
                        b = opts.defalutValue || '';
                    }
                    $(this).searchbox('setValue', b);
                    return;
                }
                return;
            }
            //清空所有值



            $(this).searchbox('setValue', '');
        })
    }

});
/*
插件扩展:checkbox
*/
(function ($) {
    /*创建ck*/
    function createCheckbox(target) {

        var t = $(target);
        var opts = t.checkbox('options');
        if (opts.checked) {
            t.attr('checked', opts.checked || '');
        }
        t.attr('id', opts.id || '');
        t.bind('click', function (e, c, d) {
            var checked = $(this).attr('checked') ? true : false;
            opts.checked = checked;
            opts.onClick.call(this, checked);
        })
        if (opts.disabled) {
            t.attr('disabled', true);
        }
    }
    /*设置选中 不选中*/
    function setChecked(target, checked) {
        var t = $(target);
        var opts = t.checkbox('options');
        if (checked) {
            t.attr('checked', true);
            opts.checked = true;
        } else {
            t.removeAttr('checked');
            opts.checked = false;
        }
    }
    /*设置禁用*/
    function setDisabled(target, disabled) {
        var t = $(target);
        var opts = t.checkbox('options');
        if (disabled) {
            t.attr('disabled', true);
            opts.disabled = true;
        } else {
            t.removeAttr('disabled');
            opts.disabled = false;
        }
    }
    /*初始化*/
    $.fn.checkbox = function (options, param) {
        if (typeof options == 'string') {
            return $.fn.checkbox.methods[options](this, param);
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, 'checkbox');
            if (state) {
                $.extend(state.options, options);
            } else {
                $.data(this, 'checkbox', {
                    options: $.extend({}, $.fn.checkbox.defaults, $.fn.checkbox.parseOptions(this), options)
                });
                $(this).removeAttr('disabled');
            }
            createCheckbox(this);
        });
    };
    /*默认函数*/
    $.fn.checkbox.methods = {
        options: function (jq) {
            return $.data(jq[0], 'checkbox').options;
        },
        enable: function (jq) {
            return jq.each(function () {
                setDisabled(this, false);
            });
        },
        disable: function (jq) {
            return jq.each(function () {

                setDisabled(this, true);
            });
        },
        alterStatus: function (jq, status) {
            return jq.each(function () {
                if (!status) return;
                var opts = $(this).checkbox('options');
                $.view.bind.call($(this), 'checkbox');
                if (opts.forbidstatus) {
                    for (var i = 0; i < opts.forbidstatus.length; i++) {
                        var c = opts.forbidstatus[i];
                        if (c == -1 || c == status) {
                            opts.disabled ? "" : $(this).checkbox('disabled');
                            return;
                        }
                    }
                    opts.disabled ? $(this).checkbox('enabled') : "";
                }
                else {
                    opts.disabled ? $(this).checkbox('enabled') : "";
                }
            });
        },
        disabled: function (jq) {
            $(jq).checkbox('disable');
        },
        enabled: function (jq) {
            $(jq).checkbox('enable');
        },
        setText: function (jq, text) {
            $(jq).checkbox('setValue', text);
        },
        getText: function (jq) {
            return $(jq).checkbox('getValue');
        },
        setValue: function (jq, text) {
            text = $.trim(text + "");
            !text || text.toLowerCase() == "false" || text.toLowerCase() == "0" || text.toLowerCase() == "否" ? setChecked(jq, false) : setChecked(jq, true);
        },
        getValue: function (jq) {
            var checked = $(jq).attr('checked');
            return checked ? true : false;
        },
        //获得格式数据
        getData: function (jq) {

            var vals = $(jq).attr('id');
            if (vals) {
                var vals = vals.split('-');
                if (vals) {
                    var result = { m: vals[1], n: vals[2], v: $(jq).checkbox('getValue') };
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
                    var a = hs[idAttr[1]], b;
                    if (a && a.hasOwnProperty(idAttr[2])) {
                        b = a[idAttr[2]];
                        $(this).checkbox('setValue', b);
                        return;
                    }
                    return;
                }
                $(this).checkbox('setValue', '');
            })
        },
        setCCZFCode: function (jq) {
            var parms = $(this).checkbox('getParms', 'setCCZFCode');
            if (parms && parms.length > 7) {
                var bgTypeColID = parms[0], functionClassColID = parms[1], economyClassColID = parms[2],
                expendTypeColID = parms[3], bgSourceColID = parms[4], projectColID = parms[5], isGouKuColID = parms[6], curColID = parms[7];
                GetPayCode.call(this, bgTypeColID, functionClassColID, economyClassColID, expendTypeColID, bgSourceColID, projectColID, isGouKuColID, curColID)
            }
        },
        setControlValue: function () {
            
            var parms = $(this).checkbox('getParms', 'setControlValue');
            if (!parms || !parms[0]) return;
            var opts = $(this).checkbox('options');
            var scope=parms[0].split('-')[0];
            var detail=parms[0].split('-')[1];
            //libin 预算类型
            
            var projectvalue=$('#' + scope + '-' + detail + '-GUID_Project').combogrid('getValue');
//            $('#' + parms[0]).combogrid('grid').datagrid('selectRow', opts.checked ? 1 : 0);
            //$(this).checkbox('setValue',projectvalue?'1':'0');
            var bgtype=$('#' + parms[0]);
            if (bgtype){
                bgtype.combogrid('grid').datagrid('selectRow', projectvalue ? 1 : 0);
            }
        },
        getParms: function (jq, methodName) {
            var opts = $(jq).checkbox('options'), funConfig = opts.bindparms;
            if (funConfig && funConfig[methodName]) {
                return funConfig[methodName];
            }
            return null;
        }

    };
    /*如果dom上配置了敏感属性，将属性转换为options形式*/
    $.fn.checkbox.parseOptions = function (target) {
        var t = $(target);
        return $.extend({}, $.parser.parseOptions(target,
			['id', 'disabled', 'checked']
		), {
		    id: t.attr('id'),
		    disabled: (t.attr('disabled') ? true : false),
		    checked: (t.attr('checked') ? true : false)
		});
    };
    /*默认配置*/
    $.fn.checkbox.defaults = {
        id: null,
        disabled: false,
        checked: false,
        onClick: function () { }
    };

})(jQuery);
/*
插件扩展:textarea
*/
(function ($) {
    /*创建textarea*/
    function createTextarea(target) {

        var t = $(target);
        var opts = t.textarea('options');
        t.attr('id', opts.id || '');
        if (opts.disabled) {
            t.attr('disabled', true);
        }
    }
   
    /*设置禁用*/
    function setDisabled(target, disabled) {
        var t = $(target);
        var opts = t.checkbox('options');
        if (disabled) {
            t.attr('disabled', true);
            opts.disabled = true;
        } else {
            t.removeAttr('disabled');
            opts.disabled = false;
        }
    }
    /*初始化*/
    $.fn.textarea = function (options, param) {
        if (typeof options == 'string') {
            return $.fn.textarea.methods[options](this, param);
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, 'textarea');
            if (state) {
                $.extend(state.options, options);
            } else {
                $.data(this, 'textarea', {
                    options: $.extend({}, $.fn.textarea.defaults, $.fn.textarea.parseOptions(this), options)
                });
                $(this).removeAttr('disabled');
            }
            createTextarea(this);
        });
    };
    /*默认函数*/
    $.fn.textarea.methods = {
        options: function (jq) {
            return $.data(jq[0], 'checkbox').options;
        },
        enable: function (jq) {
            return jq.each(function () {
                setDisabled(this, false);
            });
        },
        disable: function (jq) {
            return jq.each(function () {

                setDisabled(this, true);
            });
        },
        alterStatus: function (jq, status) {
            return jq.each(function () {
                if (!status) return;
                var opts = $(this).textarea('options');
                $.view.bind.call($(this), 'textarea');
                if (opts.forbidstatus) {
                    for (var i = 0; i < opts.forbidstatus.length; i++) {
                        var c = opts.forbidstatus[i];
                        if (c == -1 || c == status) {
                            opts.disabled ? "" : $(this).textarea('disabled');
                            return;
                        }
                    }
                    opts.disabled ? $(this).textarea('enabled') : "";
                }
                else {
                    opts.disabled ? $(this).textarea('enabled') : "";
                }
            });
        },
        disabled: function (jq) {
            $(jq).textarea('disable');
        },
        enabled: function (jq) {
            $(jq).textarea('enable');
        },
        setText: function (jq, text) {
            $(jq).textarea('setValue', text);
        },
        getText: function (jq) {
            return $(jq).textarea('getValue');
        },
        setValue: function (jq, text) {
            text = $.trim(text + "");
            $(jq).val(text);
        },
        getValue: function (jq) {
           return $(jq).val();
        },
        //获得格式数据
        getData: function (jq) {

            var vals = $(jq).attr('id');
            if (vals) {
                var vals = vals.split('-');
                if (vals) {
                    var result = { m: vals[1], n: vals[2], v: $(jq).textarea('getValue') };
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
                    var a = hs[idAttr[1]], b;
                    if (a && a.hasOwnProperty(idAttr[2])) {
                        b = a[idAttr[2]];
                        $(this).textarea('setValue', b);
                        return;
                    }
                    return;
                }
                $(this).textarea('setValue', '');
            })
        },
      
        getParms: function (jq, methodName) {
            var opts = $(jq).textarea('options'), funConfig = opts.bindparms;
            if (funConfig && funConfig[methodName]) {
                return funConfig[methodName];
            }
            return null;
        }

    };
    /*如果dom上配置了敏感属性，将属性转换为options形式*/
    $.fn.textarea.parseOptions = function (target) {
        var t = $(target);
        return $.extend({}, $.parser.parseOptions(target,
			['id', 'disabled']
		), {
		    id: t.attr('id'),
		    disabled: (t.attr('disabled') ? true : false)
		});
    };
    /*默认配置*/
    $.fn.textarea.defaults = {
        id: null,
        disabled: false,
        onClick: function () { }
    };

})(jQuery);
/*
为easyui增加自定义的插件
*/
$.extend($.parser.plugins, ['edatagrid', 'checkbox','textarea']);
/**
* 扩展树表格级联选择（点击checkbox才生效）：
* 		自定义属性：
        isAssociate:是否级联 是个开关属性 true 或者1 
* 		threeLinkCheck  :  三级联动(父节点和子节点都被选中)
* 		cascadeCheck    :  普通级联(不包括未加载的子节点),针对子节点。(这种个人认为主要区别应该在异步加载，如果非异步加载，看不出区别)
* 		deepCascadeCheck:  深度级联(包括未加载的子节点),针对子节点
*/
$.extend($.fn.treegrid.defaults, {
    onLoadSuccess: function () {
        var target = $(this);
        var opts = $.data(this, "treegrid").options;
        if (!opts.isAssociate) return;
        var panel = $(this).datagrid("getPanel");
        var gridBody = panel.find("div.datagrid-body");
        var idField = opts.idField; //这里的idField其实就是API里方法的id参数
        gridBody.find("div.datagrid-cell-check input[type=checkbox]").click(function (e) {
            //if(opts.singleSelect) return;//单选不管

            if (opts.cascadeCheck || opts.deepCascadeCheck || opts.threeLinkCheck) {
                var id = $(this).parent().parent().parent().attr("node-id");
                var status = false;
                if ($(this).attr("checked")) status = true;

                if (opts.threeLinkCheck) {
                    //三级联动,是否深度级联还需要设置deepCascadeCheck的值

                    selectParent(target, id, idField, status);
                    selectChildren(target, id, idField, opts.deepCascadeCheck, status);
                } else {
                    //只设置cascadeCheck或者deepCascadeCheck
                    if (opts.cascadeCheck || opts.deepCascadeCheck) {
                        //普通级联

                        selectChildren(target, id, idField, opts.deepCascadeCheck, status);
                    }
                }
                /**
                * 级联选择父节点
                * @param {Object} target
                * @param {Object} id 节点ID
                * @param {Object} status 节点状态，true:勾选，false:未勾选
                * @return {TypeName} 
                */
                function selectParent(target, id, idField, status) {
                    var parent = target.treegrid('getParent', id);
                    if (parent) {
                        var parentId = parent[idField];
                        if (status)
                            $("input[type=checkbox][value='" + parentId + "']").attr("checked", true);
                        else
                            $("input[type=checkbox][value='" + parentId + "']").attr("checked", false);
                        selectParent(target, parentId, idField, status);
                    }
                }
                /**
                * 级联选择子节点
                * @param {Object} target
                * @param {Object} id 节点ID
                * @param {Object} deepCascade 是否深度级联
                * @param {Object} status 节点状态，true:勾选，false:未勾选
                * @return {TypeName} 
                */
                function selectChildren(target, id, idField, deepCascade, status) {
                    //深度级联时先展开节点
                    if (status && deepCascade) {
                        target.treegrid('expand', id);
                    }

                    //根据ID获取所有孩子节点

                    var children = target.treegrid('getChildren', id);
                    for (var i = 0; i < children.length; i++) {
                        var childId = children[i][idField]; //可以根据key取到任意值

                        if (status) {
                            $("input[type=checkbox][value='" + childId + "']").attr("checked", true);
                        } else {
                            $("input[type=checkbox][value='" + childId + "']").attr("checked", false);
                        }
                    }
                }
            }
            e.stopPropagation(); //停止事件传播
        });
    }
});