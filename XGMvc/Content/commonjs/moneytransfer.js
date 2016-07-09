String.prototype.moneytransferchinese = function () {debugger
    try {
        var money = "";
        for (var i = 0, j = this.length; i < j; i++) {
            money += this[i];
        }
        money = money.toString().replace(/,/g, '');
        money = moneytransfer.moneytrim(money);
        var i = 1;
        var dw2 = ["", "万", "亿"]; //大单位
        var dw1 = ["拾", "佰", "仟"]; //小单位
        var dw = ["零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖"]; //整数部分用
        //以下是小写转换成大写显示在合计大写的文本框中
        //分离整数与小数
        var source = moneytransfer.moneysplits(money);
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
};
//数据千分位
Number.prototype.formatThousand = function (s1) {

    if (!s1) return "";
    s1 = s1.toString().replace(/,/g, '');
    s1 = new Number(s1).toFixed(2);
    var p = /(\d+)(\d{3})/;
    while (p.test(s1)) {
        s1 = s1.replace(p, "$1,$2");
    }
    return s1;
};
var moneytransfer = {
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
    //拆分整数与小数
    moneysplits: function (money) {
        var value = new Array('', '');
        temp = money.split(".");
        for (var i = 0; i < temp.length; i++) {
            value[i] = temp[i];
        }
        return value;
    },
    numberTransferChinese: function (number) {debugger
        var dw = { 0: "零", 1: "壹", 2: "贰", 3: "叁", 4: "肆", 5: "伍", 6: "陆", 7: "柒", 8: "捌", 9: "玖" }; //整数部分用
        var str = "";
        var arr = number.split('');
        for (var i = 0, j = arr.length; i < j; i++) {
            str += dw[arr[i]];
        }
        return str;
    },
    numberTransferChinese1: function (number) {

        var dw = { 0: "零", 1: "壹", 2: "贰", 3: "叁", 4: "肆", 5: "伍", 6: "陆", 7: "柒", 8: "捌", 9: "玖" }; //整数部分用
        var str = "";
        var arr = number.split('');
        for (var i = 0, j = arr.length; i < j; i++) {
            str += dw[arr[i]];
        }
        var c = parseInt(number);
        if (c > 9) {
            if (c == 10 || c == 20 || c == 30) {
                str = str.replace("零", "拾");
            } else {
                str = str.charAt(0) + "拾" + str.charAt(1);
            }
        }
        return str;
    }

}
function numberTransferChinese(number) {
    var dw = { 0: "零", 1: "壹", 2: "贰", 3: "叁", 4: "肆", 5: "伍", 6: "陆", 7: "柒", 8: "捌", 9: "玖" }; //整数部分用
    var str = "";

    var arr = (number + "").split('');
    for (var i = 0, j = arr.length; i < j; i++) {
        str += dw[arr[i]];
    }
    return str;
}
function numberTransferChinese1(number) {
    var dw = { 0: "零", 1: "壹", 2: "贰", 3: "叁", 4: "肆", 5: "伍", 6: "陆", 7: "柒", 8: "捌", 9: "玖" }; //整数部分用
    var str = "";
    if ((number + "").length == 1) {
        number = "0" + number + "";
    }
    var arr = (number + "").split('');
    for (var i = 0, j = arr.length; i < j; i++) {
        str += dw[arr[i]];
    }
    var c = parseInt(number);
    if (c > 9) {
        if (c == 10 || c == 20 || c == 30) {
            str = str.replace("零", "拾");
        } else {
            str = str.charAt(0) + "拾" + str.charAt(1);
        }
    }
    return str;
}