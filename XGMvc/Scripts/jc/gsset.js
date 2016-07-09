
$(document).ready(function () {
    var formulaId = 'formula', curTreeId = 'gssz-tree';
    $('#gssz-tab').tabs({
        onSelect: function (title, index) {

            var opts = $('#cancle').linkbutton('options');
            opts.setValueId.content = 'contentId';
            opts.setValueId.formula = formulaId + index;

            var opts1 = $('#save').linkbutton('options');
            opts1.setValueId.content = 'contentId';
            opts1.setValueId.formula = formulaId + index;
            opts1.setValueId.tree = curTreeId + index;

        }
    });
    var ids = ["add", "sub", "mult", "div", "left", "right", "save", "cancle"];
    $('#contentId').attr('disabled', 'disabled');
    for (var i = 0, j = ids.length; i < j; i++) {
        $('#' + ids[i]).linkbutton('disabled');
    }
    $('#gssz-close').bind('click', function () {
        parent.window.CloseTabs();
    });
});
//操作符给内容区域赋值
$.OperatorSymbol = function (jq) {
    var opts = $(jq).linkbutton('options');
    var contentId = '#' + opts.setValueId;
    var symbol = opts.symbol;
    var temp = $(contentId).val();
    temp += symbol;
    $(contentId).val(temp);
}
//根据名称替换为 GUID
$.RepalceNodeNameWithGuid = function (context, data) {
    //工资 guid
    //生活费 + 工资 + 岗位工资
    for (var i = 0, j = data.length; i < j; i++) {
        var node = data[i];
        var id = node.attributes.GUID_Item;
        context = $.FindKeyRepace(context, node.text, id);
    }
    return context;
};
//检索字符串 递归替换
$.FindKeyRepace = function (text, key, repaceValue, start) {
    debugger
    //首先验证有没有 没有返回
    var index = text.indexOf(key, start || 0);
    //2016-2-19改进 aa+aa2+cc只替换aa 不替换aa2的bug  
    //即时找到 也要做替换  所以不改进indexOf方法 决定改进

    if (index < 0) return text;
    var preIndex = (index - 1) > 0 ? (index - 1) : 0;
    var nextIndex = (index + 1) > text.length - 1 ? 0 : index + key.length;
    //验证是否是汉字的正则表达式
    var reg = /^[\u4e00-\u9faf]+$/;
    var reg1 = /^[0-9]*[1-9][0-9]*$/;
    var i = 0; //等于2通过
    if (preIndex != 0) {
        i = reg.test(text[preIndex]) ? 0 : 1;
        //验证汉字的表达式
    } else {
        i = 1;
    }
    if (nextIndex != 0) {
        i = reg.test(text[nextIndex]) || reg1.test(text[nextIndex]) ? 0 : i + 1;
    } else {
        i = i + 1;
    }
    if (i == 2) {
        //如果验证通过
        text = text.substr(0, index) + "{" + repaceValue + "}" + text.substr(index + key.length, text.length - 1);
    }
    //移动索引 继续向下找
    text = arguments.callee(text, key, repaceValue, nextIndex);
    return text;
};
//默认选中tree的第一个节点$.TreeSelected = function (jq, target) {
    var id = $(jq).find('li:eq(0) div').attr('node-id');
    var node = $(jq).tree('find', id);
    $(jq).tree('select', node.target);
}
//tree扩展
$.extend($.fn.tree.methods, {
    getLevel: function (jq, target) {
        var l = $(target).parentsUntil("ul.tree", "ul");
        return l.length + 1;
    },
    //获取到邻近节点
    getClosedNode: function (jq, params) {
        var data = $(jq).tree('getRoots');
        var currNode = params.node, isAsc = params.isAsc;
        var node;
        for (var i = 0, j = data.length; i < j; i++) {
            if (data[i].id == currNode.id) {

                if ((isAsc && i == 0) || (i == (data.length - 1) && !isAsc)) break;
                node = isAsc ? data[i - 1] : data[i + 1];
                break;
            }
        }
        if (!node) return;
        $(jq).tree('remove', currNode.target);
        if (isAsc) {
            $(jq).tree('insert', {
                before: node.target,
                data: {
                    id: currNode.id,
                    text: currNode.text,
                    attributes:currNode.attributes
                }
            });
        } else {
            $(jq).tree('insert', {
                after: node.target,
                data: {
                    id: currNode.id,
                    text: currNode.text,
                    attributes: currNode.attributes
                }
            });
        }
        var node1 = $(jq).tree('find', currNode.id);
        $(jq).tree('select', node1.target);

        return node;
    }
});
//控制选中行移动方向
$.ControlDirection = function (jq) {
    var opts = $(jq).linkbutton('options');
    var treeId = '#' + opts.setValueId;
    var dirc = opts.dirc == "up" ? true : false;
    var selectedNode = $(treeId).tree('getSelected');
    var node = $(treeId).tree('getClosedNode', { node: selectedNode, isAsc: dirc });
   // $(treeId).tree('select', node.target);

}
//点公式按钮将控件置为可编辑状态
$.ControlEnable = function (jq) {
    var saItemV = $('#titleId').val();
    if (!saItemV) {
        $.messager.alert('提示', '请选择要设置公式的项！');
        return;
    }
    var opts = $(jq).linkbutton('options');
    var contentId = '#' + opts.setValueId.content;
    var buttonIds = opts.setValueId.buttons;
    var treeId = '#' + opts.setValueId.tree;
    $(contentId).attr('disabled', false);
    $(jq).linkbutton('disabled');
    for (var i = 0, j = buttonIds.length; i < j; i++) {
        $('#' + buttonIds[i]).linkbutton('enabled');
    }
    var selNode = $(treeId).tree('getSelected');
    var opts1 = $('#save').linkbutton('options');
    opts1.storeNode = selNode;
    var opts2 = $('#cancle').linkbutton('options');
    opts2.storeNode = selNode;
}
//点取消按钮将控件置为不可可编辑状态$.ControlDisable = function (jq) {
    var opts = $(jq).linkbutton('options');
    var contentId = '#' + opts.setValueId.content;
    var formulaId = '#' + opts.setValueId.formula;
    var buttonIds = opts.setValueId.buttons;
    $(contentId).attr('disabled', true);
    $(formulaId).linkbutton('enabled')
    for (var i = 0, j = buttonIds.length; i < j; i++) {
        $('#' + buttonIds[i]).linkbutton('disabled');
    }
    var reg = /\{(.*?)\}/gi, guid;
    var itemFormula = opts.storeNode.attributes.ItemFormula;
    var tempArr = itemFormula.match(reg);
    if (tempArr) {
        for (var m = 0, n = tempArr.length; m < n; m++) {
            guid = tempArr[m].replace('{', '').replace("}", '').toLowerCase();
            itemFormula = itemFormula.replace(tempArr[m], $.ItemGuid2NameDic[guid]);
        }
    }
    $(contentId).val(itemFormula);

}
//字典
$.InitItemGuid2NameDic = function (data) {
    $.ItemGuid2NameDic = {};
    for (var i = 0, j = data.length; i < j; i++) {
        var node = data[i];
        $.ItemGuid2NameDic[node.id] = node.text;
    }
}
//从选择中计划项 编辑公式
$.ClickTree2 = function (node, titleId, contentId) {
    var isDisabled = $('#' + contentId).attr('disabled');
    if (isDisabled == "disabled") {
        
        //以{}花括号分组的正则表达式
        var reg = /\{(.*?)\}/gi, guid;
        var itemFormula = node.attributes.ItemFormula;
        var tempArr = itemFormula.match(reg);
        if (tempArr) {
            for (var m = 0, n = tempArr.length; m < n; m++) {
                guid = tempArr[m].replace('{', '').replace("}", '').toLowerCase();
                itemFormula = itemFormula.replace(tempArr[m], $.ItemGuid2NameDic[guid]);
            }
        }
        $('#' + contentId).val(itemFormula);
        $('#' + titleId).val(node.text + "=");
    } else {
        var result = $('#' + contentId).val();
        result += node.text;
        $('#' + contentId).val(result);
    }
}
//公式设置保存
$.ControlSave = function (jq, node) {

    var opts = $(jq).linkbutton('options');
    var setconfig = opts.setValueId,
            treeId = '#' + setconfig.tree,
            formulaId = '#' + setconfig.formula,
            contextId = '#' + setconfig.content,
            buttonIds = setconfig.buttons;
    //公式
    $(formulaId).linkbutton('enabled');
    for (var i = 0, j = buttonIds.length; i < j; i++) {
        $('#' + buttonIds[i]).linkbutton('disabled');
    }
    $(contextId).attr('disabled', true);
    //点击保存修改 内容的值变为 guid的形式
    var contextVal = $(contentId).val();
    var data = $(treeId).tree('getRoots');
    var result = $.RepalceNodeNameWithGuid(contextVal, data);
    //给节点赋值    var storeNode = opts.storeNode;
    for (var i = 0, j = data.length; i < j; i++) {
        var node = data[i];
        if (node.id == storeNode.id) {
            node.attributes.ItemFormula = result;
            break;
        }
    }
}
//向后台保存的方法
$.SaveDoc = function (jq) {
    
    var items = [];
    var opts = $(jq).linkbutton('options');
    var url = opts.url;
    var tab = $('#gssz-tab').tabs('getSelected');
    var index = $('#gssz-tab').tabs('getTabIndex', tab);
    var treeId = '#gssz-tree' + index;
    var tree = $(treeId);
    var treeOpts = tree.tree('options');
    var guid = treeOpts.Id;
    var dataNodes = tree.tree('getRoots');
    for (var i = 0, j = dataNodes.length; i < j; i++) {
        var node = dataNodes[i];
        items.push({ GUID_Item: node.attributes.GUID_Item, ItemFormula: node.attributes.ItemFormula });
    }
    $.ajax({
        url: url,
        data: { Id: guid, items: JSON.stringify(items) },
        dataType: "json",
        type: "POST",
        success: function (rec) {
            $.messager.alert('提示', rec.msg);
        }
    });
}
//向后台删除的方法
$.deleteDoc = function (jq) {
    var opts = $(jq).linkbutton('options');
    var treeId = '#' + opts.setValueId.tree;
    var msg = opts.msg;
    var url = opts.url;
    var selNode = $(treeId).tree('getSelected');
    var treeOpts = $(treeId).tree('options');
    var guid = treeOpts.Id;
    var itemId = selNode.attributes.GUID_Item;
    var data = $(treeId).tree('getRoots');

    for (var i = 0, j = data.length; i < j; i++) {
        var formula = data[i].attributes.ItemFormula || '';
        if (formula.toLowerCase().indexOf(itemId.toLowerCase()) >= 0) {
            $.messager.alert('提示', msg, 'info');
            return;
        }
    }
    $.ajax({
        url: url,
        data: { Id: guid, itemId: itemId },
        dataType: "json",
        type: "POST",
        success: function (rec) {
            if (rec.IsCan == "1") {
                if ($('#titleId').val().replace('=', '') == selNode.text) {
                    $('#titleId,#contentId').val('');
                    $('#contentId').attr('disabled', true);
                    $('#add,#sub,#mult,#div,#left,#right,#save,#cancle').linkbutton('disable');
                    $('#formula0').linkbutton('enable');
                }
                $(treeId).tree('remove', selNode.target);

            } else {
                $.messager.alert('提示', msg, 'info');
            }
        }
    });

}
