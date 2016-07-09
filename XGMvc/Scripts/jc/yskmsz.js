
//控制treegrid上checkbox的
$.gridCheckBoxCol = function (index, row, e1, field, e) {
    var opts = $('#yskmsz-SS_BGCode').treegrid('options');
    var ch = $('#' + field + '|' + row.Guid).attr('checked') == "checked" ? true : false;
    var chk;
    if ((row[field] + '').toLowerCase() == "true") {
        chk = $.format("<input id=\"{0}\" checked='{1}' type=\"checkbox\" onclick=\"javascript:$.OnCheckClick(this)\">", field + '|' + row.Guid, ch);
    } else {
        chk = $.format("<input id=\"{0}\" type=\"checkbox\" onclick=\"javascript:$.OnCheckClick(this)\">", field + '|' + row.Guid, ch);
    }
    return chk;
};
//点击treegrid上checkbox的事件
$.OnCheckClick = function (e, a) {
    var id = e.id;
    var idAttr = id.split('|');
    var ch = $(e).attr('checked') == 'checked' ? true : false;
    var t = $('#yskmsz-SS_BGCode');
    //将选中的checkbox的值保存下来
    if (idAttr.length > 1) {
        var selectNode = t.treegrid('find', idAttr[1]);
        $("input[type=checkbox][value='" + idAttr[1] + "']").attr("checked", true);
        selectNode[idAttr[0]] = ch;
    }
};

//树结构深度--向下(父节点-子节点)
$.selectChildren = function (target, id, idField, deepCascade, status) {
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
        children[i].checked = status;
    }
};
//树结构深度--向上(子节点到父节点)
$.selectParent = function (target, id, idField, status) {
    var parent = target.treegrid('getParent', id);
    if (parent) {
        var parentId = parent[idField];
        if (status) {
            $("input[type=checkbox][value='" + parentId + "']").attr("checked", true);
            parent.checked = true;
        }
        else {
            var childs = target.treegrid('getChildren', parentId);
            for (var i = 0, j = childs.length; i < j; i++) {
                var child = childs[i];
                if (child.checked) return;
            }
            parent.checked = false;
            $("input[type=checkbox][value='" + parentId + "']").attr("checked", false);
        }
        $.selectParent(target, parentId, idField, status);
    }
};

$.initBind = function (e) {
    var target = $('#yskmsz-SS_BGCode');
    var opts = target.treegrid('options');
    var idField = opts.idField; //这里的idField其实就是API里方法的id参数
    var id = $(this).parent().parent().parent().attr("node-id");
    var status = false;
    if ($(this).attr("checked")) status = true;
    /**/
    var selectNode = target.treegrid('find', id);
    selectNode.checked = status;
    /**/
    if (opts.threeLinkCheck) {
        //三级联动,是否深度级联还需要设置deepCascadeCheck的值
        $.selectParent(target, id, idField, status);
        $.selectChildren(target, id, idField, opts.deepCascadeCheck, status);
    }
    e.stopPropagation(); //停止事件传播
};

//tree的方法扩展
$.extend($.fn.treegrid.defaults, {
    //treegrid加载完后处理的事情
    onLoadSuccess: function () {
        var target = $(this);
        var opts = $.data(this, "treegrid").options;
        if (!opts.isAssociate) return;
        var panel = $(this).treegrid("getPanel");
        var gridBody = panel.find("div.datagrid-body");
        var idField = opts.idField; //这里的idField其实就是API里方法的id参数
        gridBody.find("div.datagrid-cell-check input[type=checkbox]").click(function (e) {
            var id = $(this).parent().parent().parent().attr("node-id");
            var status = false;
            if ($(this).attr("checked")) status = true;
            var selectNode = target.treegrid('find', id);
            selectNode.checked = status;
            if (!status) {
                selectNode.IsDefault = false;
            }
            if (opts.threeLinkCheck) {
                //三级联动,是否深度级联还需要设置deepCascadeCheck的值
                $.selectParent(target, id, idField, status);
                $.selectChildren(target, id, idField, opts.deepCascadeCheck, status);
            }
            e.stopPropagation(); //停止事件传播
        });
    }
});

//控制当前是按照列进行编辑还是选中行就进行编辑
var editIndex = undefined; //全局变量
function endEditing() {
    if (editIndex == undefined) { return true; }
    $('#yskmsz-SS_BGCode').treegrid('endEdit', editIndex);
}
//编辑单元格事件
function onClickCell(field, row) {
    endEditing();
    $('#yskmsz-SS_BGCode').treegrid('beginEdit', row.GUID);
    editIndex = row.GUID
}
//合计
function onAfterEdit(row, changes) {
    var target = $('#yskmsz-SS_BGCode');
    //得到当前节点的父节点
    var parNode = target.treegrid('getParent', row.GUID);
    //递归函数(target：treegrid的Id    parNode：父节点)
    $.gNode(target, parNode);

    //以下是总合计（合计当前treegrid的所有父节点的值）
    var roots = target.treegrid('getRoots');
    var sum = 0;
    //得到合计节点
    var hjNode = target.treegrid('find', '00000000-0000-0000-0000-000000000000');
    for (var i = 0; i < roots.length; i++) {
        var root = roots[i];
        //获取所有父节点的时候去掉合计节点
        if (root.GUID == hjNode.GUID) continue;
        sum += parseFloat(root.RateNum) || 0;
    }
    //赋值到节点上
    hjNode.RateNum = sum;
    target.treegrid('refresh', hjNode.GUID);
}

$.gNode = function (target, parNode) {//得到父节点和所有的子节点
    //父节点以下的所有子节点
    var childs = target.treegrid('getChildren', parNode.GUID), sum = 0;
    for (var i = 0, j = childs.length; i < j; i++) {
        var child = childs[i];
        //如果当前节点的PGUID等于父节点的GUID
        if (child.PGUID == parNode.GUID) {
            sum += parseFloat(child.RateNum) || 0;
        }
    }
    //则将值累加到父节点
    parNode.RateNum = sum;
    target.treegrid('refresh', parNode.GUID);

    var parNodeRoot = target.treegrid('getParent', parNode.GUID);
    if (parNodeRoot) {
        arguments.callee(target, parNodeRoot);
    }
}
//这个函数是，点击左侧树进行条件选择的时候去合计
$.gn = function () {
    var target = $('#yskmsz-SS_BGCode');
    var gRoots = target.treegrid('getRoots');
    var sum = 0;
    //得到合计节点
    var hjNode = target.treegrid('find', '00000000-0000-0000-0000-000000000000');
    for (var i = 0; i < gRoots.length; i++) {
        var root = gRoots[i];
        //获取所有父节点的时候去掉合计节点
        if (root.GUID == hjNode.GUID) continue;
        sum += parseFloat(root.RateNum) || 0;
    }
    //赋值到合计节点
    hjNode.RateNum = sum;
    target.treegrid('refresh', hjNode.GUID);
}


$(document).ready(function () {
    var opts = $('#yskmsz-save').linkbutton('options');
    $.loadDataUrl = opts.dataLoadUrl;

    //预算设置
    $('#yskmsz-tree-setup').tree({
        onClick: $.treeCheckNode
    });
    //退出
    $('#auth-close').bind('click', function () {
        $.messager.confirm("提示", "正在编辑,是否退出?", function () {
            $($(parent.document).find("#closeTab")).click();
        })
    });

    //按钮事件 保存
    $('#yskmsz-save').bind('click', function () {
    
        var opts = $('#yskmsz-save').linkbutton('options');
        //保存前要验证是否已有选中项--验证
        if (!$.fn.linkbutton.methods["examine"](this)) return;

        var dataNode = $('#' + opts.gridId).treegrid('getData');
        var treeId = $('#' + opts.treeId).tree('getSelected').id;
        var nodeData = $.getDataNodes(dataNode)
        $.ajax({
            url: '/JCkmsz/SaveJCkm',
            data: { ssSetupId: treeId, authData: JSON.stringify(nodeData) },
            type: 'post',
            success: function (data) {
                $.messager.alert('提示', data.msg);
            }
        });
    });
});

//树点击相应单机事件
$.treeCheckNode = function (node) {

    var tree = $(this);
    var opts = tree.tree('options');
    //$.fn.tree.methods["setAsso"].call();
    var status = '2'; //根据scope获取当前的status
    var con = opts.associate;                  //获取tree的associate属性
    //定义模型
    var mModel = opts.m;
    if (status == '4' || status == '2') {//这个是页面所处于的当前状态
        if (con) {
            var r = $(this).tree('getSelected');
            //得到当前对象模型
            var rModel = r.attributes.m;
            if (mModel == rModel) {
                if (r && r.attributes && r.attributes.valid) {
                    for (var lab in con) {
                        var validFied = con[lab][0];
                        var textField = con[lab][1];
                        if (!textField) {
                            textField = validFied;
                        }
                        var control = $('#' + lab);
                        var plugin = control.GetEasyUIType();
                        var mfn = $.fn[plugin];
                        if (mfn) {
                            var sv = mfn.methods['setValue'];
                            if (sv) {
                                var guid = r.attributes[validFied];
                                sv($('#' + lab), guid);
                            }
                            var st = mfn.methods['getText'];
                            if (st) {
                                st(control, r.attributes[textField]);
                            }
                        }
                    }
                }
                $.fn.tree.methods["setAssoAfter"](r);
            }
        }
    }
    //按条件查询数据
    $.loadTreeGridData(node.id);  //数据加载
};
$.treeData = [];
//按条件查询
$.loadTreeGridData = function (guid) {
    $.ajax({
        url: '/JCkmsz/Retrievekm',
        data: { guid: guid },
        success: function (data) {
            $('#yskmsz-SS_BGCode').treegrid('loadData', data);
            $.gn();
            $.treeData = data;
            var opts = $('#yskmsz-SS_BGCode').treegrid('options');
            opts.isLoad = true;
        }
    });
};

//获得树节点 包括子集的所有节点集合(保存时调用的方法)，得到treegrid的深度所有选中的值
$.getDataNodes = function (dataNode) {
    var oArr = [], o = {};
    var getNodesO = function (dataNode) {
        for (var i = 0; i < dataNode.length; i++) {
            var node = dataNode[i];
            if (!node.checked) {
                continue;
            }
            var o = {
                Guid: node.GUID,
                GUID_BGCode: node.GUID_BGCode,
                BGCodeName: node.BGCodeName,
                RateNum: node.RateNum
            };
            oArr.push(o);
            if (node.children && node.children.length) {
                arguments.callee(node.children);
            }
        }
    };
    getNodesO(dataNode);
    return oArr;
}
//格式化
function numberbox(value1, row, index) {
    if (!value1) return '0';
    if ($.isNumeric(value1) && value1 == 0) {
        value1 = '';
    }
    return $.isNumeric(value1) ? (value1 / 100.00).toFixed(2) + '%' : '0'
}









