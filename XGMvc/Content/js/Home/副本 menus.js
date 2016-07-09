/**123123**/
//var 

$(document).ready(function () {
    //初始化
    getMenus();
});
//获取Menu
var getMenus = function () {
    $.ajax({
        url: '/Home/GetSysMenu',
        cache: false,
        async: true,
        type: 'post',
        datatype: 'json',
        success: function (data) {
            $.each(data, function (index, fitem) {
                if (fitem.MenuKey == "00") {
                    var strsub = "";
                    if (fitem.Child.length != 0) {
                        $.each(fitem.Child, function (index, gitem) {
                            var aTag = '<a id="{4}" title="{0}"  href="#" onclick="openTabs(\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\')">{0}</a>';
                            var sTag = '<s class="info{4}" onclick="openTabs(\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\')"></s>';
                            gitem = this;
                            var Scope = gitem.scope;
                            if (Scope == null) {
                                Scope = "";
                            }
                            strsub = strsub + [
                                index % 3 == 0 ? '<dl class="fore">' : '',
                                    $.format('<dt class="iconcls{4}" onclick="openTabs(\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\')">', gitem.MenuName, Scope, "", '1', gitem.MenuKey),
                                        '<em>',
                                            $.format(aTag, gitem.MenuName, Scope, "", '1', gitem.MenuKey),
                                            $.format(sTag, gitem.MenuName, Scope, "", '1', gitem.MenuKey),
                                        '</em>',
                                    '</dt>',
                                index % 3 == 2 ? '</dl>' : ''
                            ].join('');
                        });
                        var i = index + 1, title = fitem.MenuName, key = fitem.MenuKey;
                        var str = [
                            $.format('<div class="item fore{0}">', i),
                                '<h3><b>&gt;</b>',
                                   $.format('<s class="i{0}"><div class="Square" /><div/></s>', i, key, title),
                                   $.format('<a id="{0}" title="{1}"  href="#">{1}</a>', key, title),
                                '</h3>',
                                '<div class="con"></div>',
                                '<div class="item-mc">',
                                    '<div class="panel-container">',
                                            strsub,
                                    '</div>',
                                '</div>',
                            '</div>'
                        ].join('');
                        $(str).appendTo('.mc');
                        return;
                    }
                }
                var strLi = "";
                var strSon = "";
                var fitem = this;
                if (fitem.Child.length != 0) {
                    $.each(fitem.Child, function (index, sitem) {
                        var sitem = this;
                        strLi = strLi + [
                            '<li class="tab" style="margin:0px;padding:0px;list-style:none;vertical-algin:none">',
                            $.format('<a href="#{0}" title="{1}">', sitem.MenuKey, sitem.MenuName),
                                sitem.MenuName,
                            '</a>',
                            '</li>',
                        ].join('');
                        var son = this;
                        var strPanelContent = "";
                        if (sitem.Child.length != 0) {
                            $.each(sitem.Child, function (index, gitem) {
                                var aTag = '<a id="{4}" title="{0}"  href="#" onclick="openTabs(\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\')">{0}</a>';
                                var sTag = '<s class="info{4}" onclick="openTabs(\'{0}\', \'{1}\'), \'{2}\', \'{3}\', \'{4}\'"></s>';
                                var Scope = gitem.scope;
                                if (Scope == null) {
                                    Scope = "";
                                }
                                strPanelContent = strPanelContent + [
                                    $.format('<div id="{0}">', son.MenuKey),
                                        '<dl class="fore">',
                                            $.format('<dt class="iconcls{4}" onclick="openTabs(\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\')">', gitem.MenuName, Scope, "", '1', gitem.MenuKey),
                                                '<em>',
                                                    $.format(aTag, gitem.MenuName, Scope, "", '1', gitem.MenuKey),
                                                    $.format(sTag, gitem.MenuName, Scope, "", '1', gitem.MenuKey),
                                                '</em>',
                                            '</dt>',
                                        '</dl>',
                                    '</div>',
                                ].join('');
                            });
                            strSon += strPanelContent;
                        } else {
                            strSon = strSon + [
                                $.format('<div id="{0}">', son.MenuKey),
                                    '<dl class="fore">',
                                        '<dt>',
                                            '<em>',
                                            '</em>',
                                        '</dt>',
                                    '</dl>',
                                '</div>',
                            ].join('');
                        }
                    });
                }
                if (fitem.MenuKey == "") {

                }
                var i = index + 1, title = fitem.MenuName, key = fitem.MenuKey;
                var str = [
                $.format('<div class="item fore{0}">', i),
                    '<h3><b>&gt;</b>',
                       $.format('<s class="i{0}"><div class="Square" /><div/></s>', i, key, title),
                       $.format('<a id="{0}" title="{1}" onclick="return false;" href="#">{1}</a>', key, title),
                    '</h3>',
                    '<div class="con"></div>',
                    '<div class="item-mc">',
                        $.format('<div id="tab-container{0}" class="tab-container" style="padding-left:4px;">', i),
                            '<ul class="etabs" style="padding-left: 36px;float:right;list-style:none;margin:0 auto">',
                                    strLi,
                            '</ul>',
                            '<div class="panel-container">',
                                    strSon,
                            '</div>',
                        '</div>',
                    '</div>',
                '</div>'
            ].join('');
                $(str).appendTo('.mc');
                if (fitem.Child.length != 0) {
                    $($.format('#tab-container{0}', i)).easytabs();
                } else {
                    return;
                }
            });

            jQuery("#sortlist .item").unbind("mouseover").unbind("mouseout");
            var sortlist = jQuery("#sortlist .item");
            jQuery.each(sortlist, function (i, element) {
                if (!!jQuery("h3", element).find("b").get(0)) {
                  jQuery(element).hoverForIE6({ delay: 150 });
                }
            });
        }
    });
}
//新增tab标签
var openTabs = function (title, scope, guid, status, key, Common) {
    
    var scope = $.trim(scope);
    if (scope != undefined && scope.length >= 0 && scope != "") {
        var existIndex = existsTab($('#tt'), title, guid);
        if (existIndex >= 0) {
            $('#' + scope).attr('src', GetUrl(scope, guid, status)); //hanyx
            $('#tt').tabs('select', existIndex);
            return;
        }
        var content = '<iframe id="' + scope + '" scrolling="no" frameborder="0" src="' + GetUrl(scope, guid, status, Common) + '" style="width:100%;height:99%;"></iframe>';
        var subTitle = title;
        if (title.length > 6) {
            subTitle = title.substring(0, 4) + "..";
        }
        $('#tt').tabs('add', { title: subTitle, closable: true, content: content, dataguid: guid });

        var $iframe = $("iframe#" + scope);
        if ($iframe) {
            var element = $iframe[0];
            if (!element) return;
            if (element.attachEvent) {
                element.attachEvent("onload", function () {
                });
            } else {
                element.onload = function () {
                    if (this.contentDocument.title == "登陆界面") {
                        location.href = "/Logon/Index";
                    }
                };
            }
        }

    } else {
        $.messager.alert('提示', '当前操作员对单据:"' + title + '"无操作权限！', 'info');
    }
}
//验证标签是否存储在
function existsTab(jq, which, guid) {
    if (which.length > 6) { which = which.substring(0, 4) + ".."; }
    var tabs = $.data(jq[0], "tabs").tabs;
    if (typeof which == "number") {
        if(which<0||which>=tabs.length){
            return -1;
        }else{
            var tab = tabs[which];
            var opt = tab.panel("options");
            if (opt.dataguid == undefined) opt.dataguid = "";
            return opt.dataguid == guid ? which : -1;
        }
    }
    for (var i = 0; i < tabs.length; i++) {
        var tab = tabs[i];
        var opt = tab.panel("options");
        //if (opt.dataguid == undefined) opt.dataguid = "";
        if (opt.title == which) {//&& opt.dataguid == guid
            return i;
        }
    }
    return -1;
};
//得到href
function GetUrl(scope, guid, status,data) {
    var url = "";
    if (scope != undefined && scope.length >= 0 && scope != "") {
        if (data) {
            url = $.format("/{0}?scope={1}&isProcess={2}&common={3}&ProcessID={4}", scope, scope, data ? 1 : 0, data.Common, data.ProcessID);
        } else {
            url = $.format("/{0}?scope={1}", scope, scope);
        }
      
    }
    else {
        return "";
    }
    if (guid) {
        if (url.indexOf("?") >= 0) {
            url += "&guid=" + guid;
        }
        else {
            url += "?guid=" + guid;
        }

    }
    if (status) {
        if (url.indexOf("?") >= 0) {
            url += "&status=" + status;
        }
        else {
            url += "?status=" + status;
        }
    }
    return url;
}
window.CloseTabs = function () {
    var pp = $('#tt').tabs('getSelected');
    var index = $('#tt').tabs('getTabIndex',pp);
    var tab = pp.panel('options').tab;
    $('#tt').tabs('close', index);
}
/*
    页面中的跳转应用
*/
/*******************************/
//新增tab标签
var openPageTabs = function (title,scopePage,scope, guid, status, key, Common) {
    var scope = $.trim(scope);
    if (scope != undefined && scope.length >= 0 && scope != "") {
        var existIndex = existsTab($('#tt'), title, guid);
        if (existIndex >= 0) {
            $('#' + scope).attr('src', GetPageUrl(scopePage,scope, guid, status));
            $('#tt').tabs('select', existIndex);
            return;
        }
        var content = '<iframe id="' + scope + '" scrolling="no" frameborder="0" src="' + GetPageUrl(scopePage,scope, guid, status, Common) + '" style="width:100%;height:99%;"></iframe>';
        var subTitle = title;
        if (title.length > 6) {
            subTitle = title.substring(0, 4) + "..";
        }
        $('#tt').tabs('add', { title: subTitle, closable: true, content: content, dataguid: guid });

        var $iframe = $("iframe#" + scope);
        if ($iframe) {
            var element = $iframe[0];
            if (!element) return;
            if (element.attachEvent) {
                element.attachEvent("onload", function () {
                });
            } else {
                element.onload = function () {
                    if (this.contentDocument.title == "登陆界面") {
                        location.href = "/Logon/Index";
                    }
                };
            }
        }

    } else {
        $.messager.alert('提示', '当前操作员对单据:"' + title + '"无操作权限！', 'info');
    }
}
/*scopePage 指controler中对应的跳转页面的函数名称*/
function GetPageUrl(scopePage,scope, guid, status, data) {
    var url = "";
    if (scope != undefined && scope.length >= 0 && scope != "") {
        if (data) {
            url = $.format("/{0}?scope={1}&isProcess={2}&common={3}&ProcessID={4}", scope + "/" + scopePage, scope, data ? 1 : 0, data.Common, data.ProcessID);
        } else {
            url = $.format("/{0}?scope={1}", scope + "/" + scopePage, scope);
        }

    }
    else {
        return "";
    }
    if (guid) {
        if (url.indexOf("?") >= 0) {
            url += "&guid=" + guid;
        }
        else {
            url += "?guid=" + guid;
        }

    }
    if (status) {
        if (url.indexOf("?") >= 0) {
            url += "&status=" + status;
        }
        else {
            url += "?status=" + status;
        }
    }
    return url;
}
/*******************************/