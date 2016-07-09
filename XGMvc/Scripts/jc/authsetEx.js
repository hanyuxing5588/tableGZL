/*权限设置*/
//选中数据
$.selectParent = function (target, id, idField, status) {
    var parent = target.treegrid('getParent', id);
    

    if (parent) {
        var parentId = parent[idField];
        $.setCheckRowByField(parent, parentId, target, status);
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
        //add sxb
        $.setCheckDisabled(parentId, status, target);
        $.selectParent(target, parentId, idField, status);

    }
}
$.selectChildren = function (target, id, idField, deepCascade, status) {
    //深度级联时先展开节点
    if (status && deepCascade) {
        target.treegrid('expand', id);
    }

    //根据ID获取所有孩子节点
    var children = target.treegrid('getChildren', id);
    for (var i = 0; i < children.length; i++) {
        var childId = children[i][idField]; //可以根据key取到任意值
        children[i].checked = status ? true : false;
        $.setCheckRowByField(children[i], childId, target,status);
        if (status) {
            $("input[type=checkbox][value='" + childId + "']").attr("checked", true);
        } else {
            $("input[type=checkbox][value='" + childId + "']").attr("checked", false);
        }
        children[i].checked = status;
        //設置Check狀態 sxb
        $.setCheckDisabled(childId, status);
    }
}
$.getHashTable = function (isChecked) {
    var hs = {};
    var v = !isChecked ? false : $('#chkView').attr('checked') == "checked";
    v = !isChecked ? false : $('#chkEnableDate').attr('checked') == "checked";
    hs["IsTimeLimited"] = v;
    if (v) {
        hs["StartTime"] = $('#startDate').datebox('getText');
        hs["StopTime"] = $('#endDate').datebox('getText');
    }
    return hs;
}
$.setCheckRowByField = function (data, guid, t, status) {
    var hsTable = $.hsTable;
    for (var field in hsTable) {
        if (field == 'StartTime' || field == 'StopTime') continue;
        var b = hsTable[field];
        data[field] = b;
        data.checked = status;
        if (field == "IsTimeLimited") {
            data['StartTime'] = b ? hsTable["StartTime"] : '';
            data['StopTime'] = b ? hsTable["StopTime"] : '';
        }
    }
    t.treegrid('refresh', guid);
    $("input[type=checkbox][value='" + guid + "']").bind('click', $.initBind)

}
$.initBind = function (e) {
    var target = $('#dwAuthTreeGrid');
    var opts = target.treegrid('options');
    var idField = opts.idField; //这里的idField其实就是API里方法的id参数
    //if(opts.singleSelect) return;//单选不管
    if (opts.cascadeCheck || opts.deepCascadeCheck || opts.threeLinkCheck) {
        var id = $(this).parent().parent().parent().attr("node-id");
        var status = false;
        if ($(this).attr("checked")) status = true;
         $.hsTable = $.getHashTable(status);
        /**/
        //                var t = $('#dwAuthTreeGrid')
        var selectNode = target.treegrid('find', id);
        $.setCheckRowByField(selectNode, id, target, status);
        //add sxb
        $.setCheckDisabled(id, status);
        /**/
        if (opts.threeLinkCheck) {
            //三级联动,是否深度级联还需要设置deepCascadeCheck的值
            $.selectParent(target, id, idField, status);
            $.selectChildren(target, id, idField, opts.deepCascadeCheck, status);
        } else {
            //只设置cascadeCheck或者deepCascadeCheck
            if (opts.cascadeCheck || opts.deepCascadeCheck) {
                //普通级联
                $.selectChildren(target, id, idField, opts.deepCascadeCheck, status);
            }
        }
       
    }
    e.stopPropagation(); //停止事件传播
};
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
                $.hsTable = $.getHashTable(status);
                var selectNode = target.treegrid('find', id);
                $.setCheckRowByField(selectNode, id, target, status);
                if (opts.threeLinkCheck) {
                    //三级联动,是否深度级联还需要设置deepCascadeCheck的值
                    $.selectParent(target, id, idField, status);
                    $.selectChildren(target, id, idField, opts.deepCascadeCheck, status);
                } else {
                    //只设置cascadeCheck或者deepCascadeCheck
                    if (opts.cascadeCheck || opts.deepCascadeCheck) {
                        //普通级联
                        $.selectChildren(target, id, idField, opts.deepCascadeCheck, status);
                    }
                }
                
                //設置Check狀態 sxb
                $.setCheckDisabled(id, status);

            }
            e.stopPropagation(); //停止事件传播
        });
        //加載完成后，更改權限Check的可用狀態
        $('.datagrid-cell-check input[type="checkbox"][checked="checked"]').each(function (i) {
            
            var id = $(this).val();
            var selectNode = target.treegrid('find', id);
            if (selectNode.IsAble) {
                //設置Check狀態
                $.setCheckDisabled(id, true);
            }
            else {
                $(this).attr("disabled", "disabled");
            }
        });
    }
});
//点击有效期显示 编辑日期的控制
$.editingId = '';
$.onCheckIsTimeChange = function (Guid, field, isEdit, treeGrid) {
    if (field == 'IsTimeLimited') {
        if (!isEdit) {
            treeGrid.treegrid('endEdit', $.editingId);
            //add sxb
            $.setCheckDisabled($.editingId, true);
            $.editingId = undefined;
            
        } else {
            if ($.editingId) {
                treeGrid.treegrid('endEdit', $.editingId);
                //add sxb
                $.setCheckDisabled($.editingId, true);
            }
            $.editingId = Guid;
            treeGrid.treegrid('beginEdit', Guid);
        }
    } else {
        treeGrid.treegrid('endEdit', $.editingId);
        $.editingId = undefined;
    }
};
$.OnCheckClick = function (e, a) {
    
    var id = e.id;
    var idAttr = id.split('|');
    var ch = $(e).attr('checked') == 'checked' ? true : false;
    
    var t = $('#dwAuthTreeGrid');
    //将选中的checkbox的值保存下来
    if (idAttr.length > 1) {
//        var selectNodePre = t.treegrid('getSelected');
       var selectNode= t.treegrid('find', idAttr[1]);
       //        var selectNode = t.treegrid('getSelected');
       $("input[type=checkbox][value='" + idAttr[1] + "']").attr("checked", true);
        selectNode[idAttr[0]] = ch;
        //如果选中的是使用有效期
        $.onCheckIsTimeChange(idAttr[1], idAttr[0], ch, t);
        //        var data = t.treegrid('getData');
        //        $.selectNodeWithHavingData(data);
    }

    //选中已经选中的 列头的文本框
    //    var panel = t.datagrid("getPanel");
    //    var gridBody = panel.find("div.datagrid-body");
    //    var checkeds = gridBody.find("div.datagrid-cell-check input[type=checkbox]")
    //    $.each(checkeds, function (i, item) {
    //        var ch = $(item);
    //        console.log(ch.attr('checked') == 'checked');
    //    });

}
$.gridCheckBoxCol = function (index, row, e1, field, e) {
    var opts = $('#dwAuthTreeGrid').treegrid('options');
    var ch = $('#' + field + '|' + row.Guid).attr('checked') == "checked" ? true : false;
    var chk;
    if ((row[field]+'').toLowerCase() == "true") {
        chk = $.format("<input id=\"{0}\" checked='{1}' type=\"checkbox\" disabled='disabled' onclick=\"javascript:$.OnCheckClick(this)\">", field + '|' + row.Guid, ch);
    } else {
        chk = $.format("<input id=\"{0}\" type=\"checkbox\" disabled='disabled' onclick=\"javascript:$.OnCheckClick(this)\">", field + '|' + row.Guid, ch);
    }
    return chk;
}
//树点击相应单机事件
$.treeCheckNode = function (node, checked) {
    if (!checked) {
        //取消選擇時
        $.dataGridUnCheck();
        return;
    }
    var tree = $(this);
    var opts = tree.tree('options');
    var selectedNodes = tree.tree('getChecked');
    var isMoreSet = $('#' + opts.comboId).combo('getValue');
    $.each(selectedNodes, function (i, item) {
        if (item.id != node.id && isMoreSet != "1") {
            tree.tree('uncheck', item.target);
        }
    });
    if (isMoreSet != "1") {
        var isRole = tree.attr('id').indexOf('role') > 0;
        $.loadTreeGridData(node.id, isRole);
    }
};
//dataGrid 取消選擇 sxb
$.dataGridUnCheck = function () {
    $('.datagrid-cell-check input[type="checkbox"][checked="checked"]').each(function (i) {        
        $(this).removeAttr("checked");
        $(this).removeAttr("disabled");
        var id = $(this).val();
        //設置Check狀態
        $.setCheckDisabled(id, false);
    })
    $("input[type=checkbox][checked='true']").removeAttr("checked");
    $("input[type=checkbox][checked='false']").removeAttr("checked");

};
//设置权限Check的可用状态
$.setCheckDisabled = function (guid, checkState, target) {
    var arr = "IsTimeLimited,".split(',');
    
    var isAble = true;
    if (target) {
        var selectNode = target.treegrid('find', guid);
        if (selectNode) {
            if (selectNode.IsAble == false) {
                isAble = false;
                $("input[type=checkbox][value='" + guid + "']").attr("disabled", "disabled");
                // $("input[type=checkbox][id*='" + guid + "']").attr("disabled", "disabled");
            }
        }
    }
    for (var i = 0; i < arr.length; i++) {
        if (!arr[i]) continue;
        if (checkState) {
            if (isAble) {
                $("input[type=checkbox][id*='" + guid + "']").removeAttr("disabled");
            }
        }
        else {
            $("input[type=checkbox][id*='" + guid + "']").attr("disabled", "disabled");
        }
    }
   
};

//当是逐一设置的时候 加载树
$.treeData = [];
$.loadDataUrl='/AuthSet/GetDWAuth'
$.loadTreeGridData = function (guid,isRole) {
    $.ajax({
        url: $.loadDataUrl,
        data: { guid: guid, isRole: isRole ? 1 : 0 },
        success: function (data) {
            $('#dwAuthTreeGrid').treegrid('loadData', data);
            $.treeData = data;
            var opts = $('#dwAuthTreeGrid').treegrid('options');
            opts.isLoad = true;
        }
    });
}
//获得树节点 包括子集的所有节点集合
$.getDataNodes = function (dataNode) {

    var oArr = [], o = {};
    var getNodesO = function (dataNode) {

        for (var i = 0; i < dataNode.length; i++) {
            var node = dataNode[i];
            //libin 2015-4-13 子节点不是全部选择的话，父节点相当于没有选中.为了修改此问题
            var ischecked = $.isNodeChecked(node);

            if (!ischecked) {
                continue;
            }
            var o = {
                Guid: node.Guid,
                ModelName: node.ModelName,
                ClassId: node.ClassId,
                IsTimeLimited: node.IsTimeLimited,
                StartTime: node.StartTime,
                StopTime: node.StopTime,
                IsDefault: node.IsDefault
            };
            oArr.push(o);

            if (node.children && node.children.length) {
                arguments.callee(node.children);
            }
        }
        
    };
    getNodesO(dataNode);
    return oArr;
};

//判断树节点是否被选中（尤其是父节点）
$.isNodeChecked = function (node) {
    if (node == undefined || node == null) return false;
    if (node.children && node.children.length) {
        for (var i = 0; i < node.children.length; i++) {
            if ($.isNodeChecked(node.children[i])) {
                return true;
            }
        }
        return false;
    }
    else {
        return node.checked;
    }
};

//获得树节点 包括子集的所有节点集合
$.getNodeSelected = function (target, dataNode, isSelectedAll) {
    for (var i = 0; i < dataNode.length; i++) {
        var node = dataNode[i];
        node.checked = isSelectedAll;
//        target.treegrid('refresh', node.Guid);
        if (node.children && node.children.length) {
            arguments.callee(target,node.children, isSelectedAll);
        }
    }
};
$(document).ready(function () {
    $('#chkEnableDate').bind('change', function (a, b) {
        var m = $(this).attr('checked') ? "enabled" : "disabled";
        $('#startDate').datebox(m);
        $('#endDate').datebox(m);
    })
    //设置方式
    $('#auth-setPay').combobox({
        onSelect: function (r) {
            var tree = $('#dwAuth-tree-role');
            var selectedNodes = tree.tree('getChecked');
            $.each(selectedNodes, function (i, item) {
                tree.tree('uncheck', item.target);
            });
            tree = $('#dwAuth-tree-per');
            selectedNodes = tree.tree('getChecked');
            $.each(selectedNodes, function (i, item) {
                tree.tree('uncheck', item.target);
            });
            $.loadTreeGridData();
        }
    })
    //角色树
    $('#dwAuth-tree-role').tree({
        onCheck: $.treeCheckNode
    });
    //人员树
    $('#dwAuth-tree-per').tree({
        onCheck: $.treeCheckNode
    })
    var opts = $('#auth-save').linkbutton('options');
    $.loadDataUrl = opts.dataLoadUrl;
    $.loadTreeGridData();
    //    var editingId;
    //    $('#dwAuthTreeGrid').treegrid({
    //    //        onSelect: function () {
    //    ////            
    //    ////            var t = $(this);
    //    ////            var data = t.treegrid('getData');
    //    ////            $.selectNodeWithHavingData(data);
    //    //            //           
    //    //        },
    //    //        onClickCell: function (field, r) {
    //    //            
    //    //          
    //    //        }
    //})


    //按钮事件 保存
    $('#auth-save').bind('click', function () {
        
        var opts = $('#auth-save').linkbutton('options');
        var treegridObj = $('#' + opts.gridId);
        var gridRow = treegridObj.treegrid('getSelected');
        if (gridRow) {
            treegridObj.treegrid('endEdit', gridRow["Guid"]);
        }
        var dataNode = treegridObj.treegrid('getData');
        var tab = $('#' + opts.tabId).tabs('getSelected');
        var title = tab.panel('options').title
        var isRole = title == '角色' ? true : false;
        var nodes = isRole ? $('#dwAuth-tree-role').tree('getChecked') : $('#dwAuth-tree-per').tree('getChecked');
        var role2userId = [];
        for (var i = 0, j = nodes.length; i < j; i++) {
            var n = nodes[i];
            role2userId.push(n.id);
        }
        
        var nodeData = $.getDataNodes(dataNode)
        $.ajax({
            url: '/AuthSet/SaveAuthData',
            type: 'post',
            data: { roleOrUser: role2userId.join(','), classId: opts.classId, authData: JSON.stringify(nodeData) },
            success: function (data) {
                $.messager.alert('提示', data.msg);
            }

        })
    });
    //退出
    $('#auth-close').bind('click', function () {
        $.messager.confirm("提示", "正在编辑,是否退出?", function () {
            //$($(parent.document).find("#closeTab")).click();
            parent.window.CloseTabs();
        })
    });
    //全选
    $('#auth-selectAll').bind('click', function () {
        var target = $('#dwAuthTreeGrid');
        $.CheckOrUnCheckAll(target, true);
    });
    //全消
    $('#auth-cancel').bind('click', function () {
        var target = $('#dwAuthTreeGrid');
        $.CheckOrUnCheckAll(treeGridObj, false);
    });
    //Grid中的全选
    var treeGridObj = $('#dwAuthTreeGrid');
    treeGridObj.datagrid({
        onCheckAll: function (rows) {
            var target = treeGridObj;
            $.CheckOrUnCheckAll(target, true);
        },
        onUncheckAll: function (rows) {
            $.CheckOrUnCheckAll(treeGridObj, false);
        }
    });

});

//全选或者全消
$.CheckOrUnCheckAll = function (target, check) {
    var nodes = target.treegrid('getData');
    $.getNodeSelected(target, nodes, check);
    var elements = $('.datagrid-cell-check input[type=checkbox]');
    $.each(elements, function (i, item) {
        $(item).attr('checked', check);
    });
    elements.click();
}
