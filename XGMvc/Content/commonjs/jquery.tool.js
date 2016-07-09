/**
* Cookie plugin
*
* Copyright (c) 2006 Klaus Hartl (stilbuero.de)
* Dual licensed under the MIT and GPL licenses:
* http://www.opensource.org/licenses/mit-license.php
* http://www.gnu.org/licenses/gpl.html
*
*/
/*
* cookie操作
1.设置cookie值,把name变量的值设为value
example $.cookie('name','value');
2.新建一个cookie 包括有效期 路径 域名等
example $.cookie('name','value',{expires:7,path:'/',domain:'jquery.com',secure:true});
3.新建cookie
example $.cookie('name','value');
4.删除一个cookie
example $.cookie('name',null);
5.取一个cookie(name)的值
example var account=$.cookie('name');
*/
jQuery.cookie = function (name, value, options) {
    if (typeof value != 'undefined') {
        options = options || {}; if (value === null) {
            value = '';
            options = jQuery.extend({}, options);
            options.expires = -1;
        }
        var expires = '';
        if (options.expires && (typeof options.expires == 'number' || options.expires.toUTCString)) {
            var date; if (typeof options.expires == 'number') {
                date = new Date(); date.setTime(date.getTime() + (options.expires * 24 * 60 * 60 * 1000));
            }
            else {
                date = options.expires;
            }
            expires = '; expires=' + date.toUTCString();
        }
        var path = options.path ? '; path=' + (options.path) : '';
        var domain = options.domain ? '; domain=' + (options.domain) : '';
        var secure = options.secure ? '; secure' : '';
        document.cookie = [name, '=', encodeURIComponent(value), expires, path, domain, secure].join('');
    }
    else {
        var cookieValue = null; if (document.cookie && document.cookie != '') {
            var cookies = document.cookie.split(';'); for (var i = 0; i < cookies.length; i++) {
                var cookie = jQuery.trim(cookies[i]); if (cookie.substring(0, name.length + 1) == (name + '=')) {
                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                    break;
                }
            }
        }
        return cookieValue;
    }
};
String.prototype.format = function () {
    var a = this,
    b = arguments.length;
    if (b > 0) for (var c = 0; b > c; c++) a = a.replace(new RegExp("\\{" + c + "\\}", "g"), arguments[c]);
    return a
};
//--------------------------------------------------
// extend String Function replaceAll
//--------------------------------------------------
String.prototype.replaceAll = function (reallyDo, replaceWith, ignoreCase) {
    if (!RegExp.prototype.isPrototypeOf(reallyDo)) {
        return this.replace(new RegExp(reallyDo, (ignoreCase ? "gi" : "g")), replaceWith);
    } else {
        return this.replace(reallyDo, replaceWith);
    }
};
//--------------------------------------------------
//正则过滤部分sql关键字
// extend String Function strSQL
//--------------------------------------------------
String.prototype.stripSQL = function () {
    var reg = /(.*?((select)|(from)|(count)|(delete)|(update)|(drop)|(truncate)).*?){2,}/i;
    return reg;
};
//--------------------------------------------------
//正则过滤HTML关键字
// extend String Function stripHTML
//
String.prototype.stripHTML = function () {
    var reTag = /<(?:.|\s)*?>/g;
    return this.replace(reTag,"");
};
//--------------------------------------------------
//正则去除字符串头和尾的空格
// extend String Function Trim
//
String.prototype.Trim = function () {
    var reTag =/(^\s*)|(\s*$)/g;
    return this.replace(reTag,"");
};
//--------------------------------------------------
//正则去除字符串头部空格
// extend String Function Trim
//
String.prototype.LTrim = function () {
    var reTag = /(^\s*)/g;
    return this.replace(reTag,"");
};
//--------------------------------------------------
//正则去除字符串尾部空格
// extend String Function Trim
//
String.prototype.RTrim = function () {
    
    var reTag = /(\s*$)/g;
    return this.replace(reTag, "");
};
//--------------------------------------------------
//正则去除字符中的所有空格
// extend String Function NoSpace
//
String.prototype.NoSpace = function () {
    return this.replace(/\s+/g, "");
};

//--------------------------------------------------
//去除数组中重复元素// extend String Function deleteEle
//
Array.prototype.deleteEle = function () {
    var o = {}, newArr = [], i, j;
    for (var i = 0; i < this.length; i++) {
        if (typeof (o[this[i]]) == "undefined") {
            o[this[i]] = "";
        }
    }
    for (j in o) {
        newArr.push(j);
    }
    return newArr;
};
//--------------------------------------------------
//格式化日期函数
// extend String Function Format
//
Date.prototype.Format = function (fmt) {
    var o = {
        "M+": this.getMonth() + 1, //月份
        "d+": this.getDate(), //日
        "h+": this.getHours(), //小时
        "m+": this.getMinutes(), //分
        "s+": this.getSeconds(), //秒
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度
        "S": this.getMilliseconds()//毫秒
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;

};
String.prototype.FormatDate1 = function (t) {
    var arr = this.split('-');
    if (arr < 2) return '';
    if (t == 'm') {
        return arr[1].substr(0, 1) == 0 ? arr[1].substr(1, 1) : arr[1];
    } else {
        return arr[2].substr(0, 1) == 0 ? arr[2].substr(1, 1) : arr[2];
    }
};
//--------------------------------------------------
//格式化日期函数
// extend String Function FormatDate
//
Date.prototype.FormatDate = function (date) {
    var resultDate = "";
    if (date.length == 8) {
        date = date.substr(0, date.length - 4) + "-" + date.substr(date.length - 4, date.length - 6) + "-" + date.substr(date.length - 2, 8);
    }
    if (IsValidDate(date)) {
        resultDate = new Date(date).Format("yyyy-MM-dd");
    }
    //正则验证日期格式包括：2012/12/12、2012-12-12、2012.12.12 三种格式，其他字符串或格式不匹配
    function IsValidDate(DateStr) {
        var Expression = /((^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(10|12|0?[13578])([-\/\._])(3[01]|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(11|0?[469])([-\/\._])(30|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(0?2)([-\/\._])(2[0-8]|1[0-9]|0?[1-9])$)|(^([2468][048]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([3579][26]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][13579][26])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][13579][26])([-\/\._])(0?2)([-\/\._])(29)$))/;
        var objExp = new RegExp(Expression);
        if (objExp.test(DateStr) == true) {
            return true;
        } else {
            return false;
        }
    }
    return resultDate;
};
///
///重写原生的JS中的toFixed方法
///extend String Function toFixed
//Number.prototype.toFixed = function (d) {
//    
//    var s = this + "";
//    if (isNaN(s)) return;
//    if (!d) d = 0;
//    if (s.indexOf(".") == -1) s += ".";
//    s += new Array(d + 1).join("0");
//    if (new RegExp("^(-|\\+)?(\\d+(\\.\\d{0," + (d + 1) + "})?\\d*$").test(s)) {
//        var s = "0" + RegExp.$2, pm = RegExp.$1, a = RegExp.$3.length, b = true;
//        if (a == d + 2) {
//            a = s.match(/\d/g);
//            if (parseInt(a[a.length - 1]) > 4) {
//                for (var i = a.length - 2; i >= 0; i--) {
//                    a[i] = parseInt(a[i]) + 1;
//                    if (a[i] == 10) {
//                        a[i] = 0;
//                        b = i != 1;
//                    } else break;
//                }
//            }
//            s = a.join("").replace(new RegExp("(\\d+)(\\d{" + d + "})\\d$"), "$1.$2");
//        } if (b) s = s.substr(1);
//        return (pm + s).replace(/\.$/, "");
//    } return this + "";
//}
//-------------------------------------------------
// 判断字符串是否为Guid
// format为String 类型的可选参数，其含义为:
// "N": xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
// "D":由连字符分隔的32位字符 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
// "B":括在大括号中，由连字符分隔的32位字符{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
// "P":括在小括号中，由连字符分隔的32位字符(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
// 默认为D
//-------------------------------------------------
String.prototype.isGuid = function (format) {
    var reg = "";
    function getRegExp(format) {
        switch (format) {
            case "N":
                return /^[A-Za-z0-9]*$/;
            case "B":
                return /^\{[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12}\}$/;
            case "P":
                return /^\([A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12}\)$/;
            case "D":
            default:
                return /^([A-Za-z0-9]{8})-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-([A-Za-z0-9]{12})$/;
        }
    };
    if (typeof (format) == "string") {
        format = format.toUpperCase();
        reg = getRegExp(format);
    }
    else {
        reg = getRegExp("D");
    }
    return reg.test(this);
};
Array.prototype.indexOf = function (val) {
    for (var i = 0, j = this.length; i < j; i++) {
        if (this[i] == val) return i;
    }
    return -1;
};
Array.prototype.remove = function (val) {
    var index = this.indexOf(val);
    if (index > -1) {
        this.splice(index, 1);
    }
};
// -----------------------------------------------------------------------
// Eros Fratini - eros@recoding.it
// jqprint 0.3
//
// - 19/06/2009 - some new implementations, added Opera support
// - 11/05/2009 - first sketch
//
// Printing plug-in for jQuery, evolution of jPrintArea: http://plugins.jquery.com/project/jPrintArea
// requires jQuery 1.3.x
//
// Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
//------------------------------------------------------------------------
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


/*
扩展数组原型方法
*/
Array.prototype.seperator = ',';
Array.prototype.exist = function (value) {
    try {
        var reg = new RegExp(this.seperator + value + this.seperator);
        return reg.test(this.seperator + this.join(this.seperator) + this.seperator);
    } catch (e) {
        return false;
    }

};
(function ($) {
    var opt;
    var printer, contenter, tableprorotypes, printInfo;

    //扩展打印内容
    extendcontent = function (data) {
        
        if (printInfo && printInfo.extendcontent) {
            var ext = printInfo.extendcontent;
            for (var m in ext) {
                var extinfo = ext[m];
                if (!extinfo) continue;
                var tab = tableprorotypes[m]; //获取对应表格的源信息
                if (!tab) continue;
                var d = gettabledata(data, m); //获取数据对象
                if (!d) continue;
                var gist = ext.gist ? gist.gist : "rowcount";
                var orgrowcount = tab[gist];
                var drowcount = d.r ? d.r.length : 0;
                var differ = drowcount - orgrowcount; //数据行与原始行的差值
                if (differ > 0) {
                    var extmodel = extinfo.model ? extinfo.model : "row";
                    //扩展页
                    if (extmodel == "page") {
                        var target = extinfo.target ? extinfo.target : "content"; //获取扩展目标
                        var extendpages = Math.ceil(drowcount / orgrowcount) - 1; //获得需要扩展的页的个数
                        extendpage(target, extendpages);
                    }
                    //扩展行
                    if (extmodel == "row") {
                        var extendrows = differ;
                        extendtablerow(tab.$lastrow, differ);
                    }
                }
            }
        }
    };

    //扩展页    //target:页内容id
    //number:需扩展的页数    extendpage = function (target, number) {
        
        var element = $(printer).find("div#" + target);
        if (!element) return;
        for (var i = 0; i < number; i++) {
            $($(element).parent()).append($(element).clone());
        }

        //页码赋值 eg:1/2 2/2
        var num = 1, totalnum = number + 1;
        $(printer).find("#pagination").each(function () {
            $(this).html(num + "/" + totalnum);
            num++;
        });
    };
      //扩展页    //target:页内容id
    //number:需扩展的页数    extendpage = function (target, number) {
        var element = $(printer).find("div#" + target);
        if (!element) return;
       
        var lastPage=$(element).clone();
        for (var i = 0; i < number; i++) {
            var parent=$($(element).parent());
            var  c=$(element).clone();
//            if(i!=number-1){
//                c.find("#pageRemoveTr").remove();
//                c.find("#pageRemoveTfoot").remove();
//                c.find("#pageRemoveBottom").remove();
//            }
//            if(i==0){
//                parent.find("#pageRemoveTr").remove();
//                parent.find("#pageRemoveTfoot").remove();
//                parent.find("#pageRemoveBottom").remove();
//            }
            if(i==number-1){
                parent.append(lastPage);
            }else{
                parent.append(c);
            }

        }
        //页码赋值 eg:1/2 2/2
        var num = 1, totalnum = number + 1;
        $(printer).find("#pagination").each(function () {
            $(this).html(num + "/" + totalnum);
            num++;
        });
    };
    //扩展行    //target:目标表格的最后一行元素id
    //number:需增加的行数    extendtablerow = function (target, number) {
        if (!target) return;
        for (var i = 0; i < number; i++) {
            $(target).after($(target).clone());
        }
    };

    //获取表格的源信息
    gettableprototype = function (data) {
        debugger
        var tables = tables || {};console.log(contenter);
        
        $(contenter).find("table").each(function () {
            debugger
            var id = $(this).attr("id");
            if (id) {
                var columns = [];
                var i = 0;
                var serialcolumn = null;
                //创建列信息                var colConfig=$(this).attr('colConfig');
                if(colConfig){
                   columns=eval(colConfig);
                   serialcolumn=columns[0];
                }else{
                    $($(this).find("thead")).find("tr th").each(function () {
                    var field=$(this).attr("field");
                    //是金额的时候将金额行记录，为后面数据转换做标记
                    var isDecimal = $(this).attr("isDemical");
                    var defalutValue=$(this).attr("defalutValue");
                    if(field){
                        var column = {};
                        column.field = field;
                        column.index = i;
                        column.isDecimal = isDecimal;
                        column.defalutValue=defalutValue;
                        columns.push(column);
                        if (column.field == "colindex") {
                            serialcolumn = column;
                        }
                        i++;
                     }
                  });
                }
                
                //根据传过来的数据，得到datagrid的ID，根据id得到当前datagrid的rows.length
                //根据动态获取的rows，去动态创建tr
                //tr是获取的数据源的rows，td是获取模板上的列数
//                var gridData = data.d;
//                var gridModelName = gridData[0].m;
//                var gridRows = $("#gridModelName");

//                var str = "";
//                str+="<tr>";
//                str+="<td></td>";
//                str+="<td file=''></td>";
//                str+="<td file=''></td>";
//                str+="<td file=''></td>";
//                str+="<td file=''></td>";
//                str+="</tr>";
//                $(this).append(str);

                //这个模板上的Row必须是动态创建的，根据前台datagrid返回来的行数在去动态生成多少条tr
                var rows = $(this).find("tbody tr");    //获取当前表格的tr(Rows)

                var rowcount = rows.length ? rows.length : 0;
                
                var info = {
                    serialcolumn: serialcolumn, //序号列                    columns: columns,
                    rowcount: rowcount,
                    $lastrow: rowcount > 0 ? rows[rowcount - 1] : null
                };
                tables[id] = info;
            }
        });
        return tables;
    };

    //转换数据格式
    transdata = function (data) {
        if (data) {
            var result = {};
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                var id = item.m + "-" + item.n;
                result[id] = item.v;
            }
            return result;
        }
    };
    //根据模型名称获取表格数据对象
    gettabledata = function (data, model) {
        if (data) {
            for (var i = 0, j = data.length; i < j; i++) {
                if (data[i].m.toLowerCase() == model.toLowerCase()) {
                    return data[i];
                }
            }
        }
    };
    //编写表格序号
    writetableserial = function (tablemodel) {
        if (tableprorotypes) {
            var tabprototype = tableprorotypes[tablemodel];
            if (tabprototype && tabprototype.serialcolumn) {
                var id = "#" + tablemodel + " tbody tr";
                rows = $(printer).find(id);
                if (rows && rows.length) {
                    for (var i = 0, j = rows.length; i < j; i++) {
                        var row = rows[i];
                        var cols = $(row).find("td");
                        selcol = cols[tabprototype.serialcolumn.index];
                        $(selcol).html(i + 1);
                    }
                }
            }
        }
    };
    /*
        isChange ：true 时 表示数据已经转化过 不用转换。    */
    $.fn.jqprintEx = function (url, data, isChange) {
        if (url) {
            var $element = (this instanceof jQuery) ? this : $(this);
            if ($element[0].tagName.toLowerCase() != "iframe") return;
            var iframe = $element[0];
            if (iframe.attachEvent) {
                iframe.attachEvent("onload", function () {
//                    console.log("ie");
                });
            } else {
                iframe.onload = function () {
                    
                    var styles = [];
                    $(iframe.contentDocument).find("style").each(function () {
                        styles.push($(this).html());
                    });
                    printer = $(iframe.contentDocument.body).find("div#print"); //打印句柄
                    if (printer.length<=0) return;
                    printInfo = $(printer).attr("options");
                    if (printInfo==undefined||printInfo==null) return;
                    printInfo = printInfo.replaceAll("'", "\"");
                    printInfo = JSON.parse(printInfo);
                    
                    //contenter = $(printer).find("div#content"); //打印内容
                    contenter = $(printer).find("#content"); //打印内容
                    
                    tableprorotypes = gettableprototype(data); //获取表格源信息                                        extendcontent(data.d); //扩展打印内容
                    //填充固定单元格内容                    $(printer).writefix(data.m);
                    if (data.d) {
                        var tabdatas = data.d;
                        for (var i = 0; i < tabdatas.length; i++) {
                            $(printer).writefix(tabdatas[i]);
                        }
                    }
                    if (data.d) {
                        var tabdatas = data.d;
                        for (var i = 0; i < tabdatas.length; i++) {
                            //填充序号
                            if(!tabdatas[i]||!tabdatas[i].m)continue;
                                writetableserial(tabdatas[i].m);
                            $(printer).writetable(tabdatas[i],isChange);
                        }
                    }
                    

                    $(printer).jqprint({ styles: styles });
                };
            }
            $element.attr("src", url);
        }
    };

    //填充固定单元格内容    $.fn.writefix = function (data) {
        if (data) {
        
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                var id = "#" + item.m + "-" + item.n;
                //获取元素
                $($(printer).find(id)).each(function () {
                    $(this).html(item.v);
                });
            }
        }
    };

    //填充列表内容
    $.fn.writetable = function (data,isChange) {
        if (data && data.m && data.r) {
            var tabprototype = tableprorotypes[data.m];
            if (!tabprototype) return;
            var columns = tabprototype.columns;
            if (!columns) return;
            var id = "#" + data.m + " tbody tr";
            rows = $(printer).find(id);
            if (!rows || !rows.length) return;
            var datarows = data.r;
            var selcolfield = tabprototype.serialcolumn && tabprototype.serialcolumn.field ? tabprototype.serialcolumn.field : undefined;
            for (var i = 0, j = datarows.length; i < j; i++) {
                var drow = isChange?datarows[i]:transdata(datarows[i]);
                if (!drow) continue;
                var row = rows[i];
                if (row) {
                    var cols = $(row).find("td");
                    
                    if (!cols || !cols.length) continue;
                    
                    for (var m = 0, n = columns.length; m < n; m++) {
                        var column = columns[m];
                        if (column.field != selcolfield) {
                            if (!column.field) continue;
                            var reVal = drow[column.field];
                            if (column.isDecimal=="true") {
                               reVal = new Number(reVal).formatThousand(reVal);
                            }
                            
                            if(column.defalutValue){
                                reVal=reVal||column.defalutValue;
                            }
                            $(cols[column.index]).html(reVal);
                        }
                    }
                }
            }
        }
        
    };

    $.fn.jqprintSetting = function (options) {
       var opt = $.extend({}, $.fn.jqprint.defaults, options);
        if (window.ActiveXObject) {
            try {
                var whs = new ActiveXObject("WScript.Shell");
                //设置页眉
                whs.RegWrite(this.HKEY_ROOT + this.HKEY_PATH + "header","");// opt.header);
                //设置页脚
                whs.RegWrite(this.HKEY_ROOT + this.HKEY_PATH + "footer","");// opt.footer);
                //设置页边距
                whs.RegWrite(this.HKEY_ROOT + this.HKEY_PATH + "margin_top", opt.margin_top);
                whs.RegWrite(this.HKEY_ROOT + this.HKEY_PATH + "margin_left", opt.margin_left);
                whs.RegWrite(this.HKEY_ROOT + this.HKEY_PATH + "margin_right", opt.margin_right);
                whs.RegWrite(this.HKEY_ROOT + this.HKEY_PATH + "margin_bottom", opt.margin_bottom);
            }
            catch (e) {
            }
        }
    };

    $.fn.jqprint = function (options) {
        opt = $.extend({}, $.fn.jqprint.defaults, options);
        var $element = (this instanceof jQuery) ? this : $(this);
        if (opt.operaSupport && $.browser.opera) {
            var tab = window.open("", "jqPrint-preview");
            tab.document.open();
            var doc = tab.document;
        }
        else {
            var $iframe = $("<iframe  />");

            if (!opt.debug) { $iframe.css({ position: "absolute", width: "0px", height: "0px", left: "-600px", top: "-600px" }); }

            $iframe.appendTo("body");
            var doc = $iframe[0].contentWindow.document;
        }
        
        if (opt.importCSS) {
            if (!opt.styles) {
                if ($("link[media=print]").length > 0) {
                    $("link[media=print]").each(function () {
                        doc.write("<link type='text/css' rel='stylesheet' href='" + $(this).attr("href") + "' media='print' />");
                    });
                }
                else if ($("link").length > 0) {
                    $("link").each(function () {
                        doc.write("<link type='text/css' rel='stylesheet' href='" + $(this).attr("href") + "' />");
                    });
                }
                else if ($("style") > 0) {
                    $("style").each(function () {
                        doc.write("<style type='text/css'>" + $(this).html() + "</style>");
                    });
                }
            }
            else {
                for (var i = 0; i < opt.styles.length; i++) {
                    doc.write("<style type='text/css'>" + opt.styles[i] + "</style>");
                }
            }
        }

        if (opt.printContainer) { doc.write($element.outer()); }
        else { $element.each(function () { doc.write($(this).html()); }); }

        doc.close();
        console.log(doc);
        (opt.operaSupport && $.browser.opera ? tab : $iframe[0].contentWindow).focus();
        setTimeout(function () {
//            (opt.operaSupport && $.browser.opera ? tab : $iframe[0].contentWindow).jqprintSetting();
            (opt.operaSupport && $.browser.opera ? tab : $iframe[0].contentWindow).print();
            if (tab) {
                tab.close();
            }
        }, 1000);
    };

    $.fn.jqprint.defaults = {
        debug: false,
        importCSS: true,
        printContainer: true,
        operaSupport: true,
        HKEY_PATH: "\\Software\\Microsoft\\Internet Explorer\\PageSetup\\",
        HKEY_ROOT: "HKEY_CURRENT_USER",
//        footer: "&u&b&d",
//        header: "&w&bPage &p of &P",
        margin_top: "0.75000",
        margin_left: "0.75000",
        margin_right: "0.75000",
        margin_bottom: "0.75000"
    };

    $.fn.outer = function () {
        return $($('<div></div>').html(this.clone())).html();
    };


})(jQuery);
(function($){
    $.fn.GetEasyUIType=function(){
        var classattr = $(this).attr('class');
        if (!classattr) return "";
        var classattr = classattr.split(' ')[0];
        if (!classattr || classattr.indexOf('easyui-') != 0) return "validatebox";
        return classattr = classattr.replace("easyui-", "");
    }
})(jQuery);

/*
扩展datagrid添加上下方向键移动
*/
var DatagridMoveRow = (function ($) {
    function DatagridMoveRow(gridTarget) {
        this.el = gridTarget; //当前grid的dom对象
        this.$el = $(this.el); //当前grid的jquery对象
        this.rowIndex = -1; //默认选中行索引为第一行的索引
        this.rowCount = 0; //获取当前加载的grid的数据条数
        return this; //返回当前grid对象
    }
    DatagridMoveRow.prototype = {
        getRowIndex: function () {
            //给当前grid的总数据条数赋值
            this.rowCount = this.$el.datagrid('getData').rows.length
            //得到选中行数据
            var selRow = this.$el.datagrid('getSelected');
            //得到选中行索引
            var selRowIndex = this.$el.datagrid('getRowIndex', selRow);
            //如果没有选中行，那么将会默认选中第一行
            if (selRowIndex == -1) {
                this.rowIndex = 0;
            } else {
                this.rowIndex = selRowIndex;
            }
        },
        moveUp: function () {
            this.getRowIndex();
            if (this.rowIndex == 0) {
                this.$el.datagrid('selectRow', this.rowCount - 1);
                return;
            }
            var i = --this.rowIndex;
            if (!!i) {
                this.$el.datagrid('selectRow', i);
            } else {
                this.$el.datagrid('selectRow', 0);
            }
        },
        moveDown: function () {
            this.getRowIndex();
            if (this.rowIndex == this.rowCount - 1) {
                this.$el.datagrid('selectRow', 0);
                return;
            }
            var i = ++this.rowIndex;
            this.$el.datagrid('selectRow', i);
        }
    }
    return DatagridMoveRow;
})(jQuery);