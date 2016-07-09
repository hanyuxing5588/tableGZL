$.gridCheckBoxCol1 = function (a, b, c) {
    if (b.Color == 1) {
        return '<font color="red">' + a + '</font>';
    } else {
        return a;
    }
}
$(document).ready(function () {
    $('#xmdygxsz-searchYY').on('click', function (type) {
        var text = $('#gwkbxd-YY-GUID').val();
        var rows = $('#projectGrid').datagrid('getRows');
        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (row.citemcode.indexOf(text) > 0) {
                $('#projectGrid').datagrid('selectRow', i);
                return;
            }
            if (row.citemname.indexOf(text) > 0) {
                $('#projectGrid').datagrid('selectRow', i);
                return;
            }
        }
    });
    function loadData(id) {
        $.post('/JCjksz/GetProject', { id: id }, function (data) {
            $('#projectTreeGrid').treegrid('loadData', data);
        }, 'json');
    }
    var SetPRalation = function (type) {
        var node = $('#xmdygxsz-tree-SS_ComparisonMain').tree('getSelected');
        if (!node) {
            $.messager.alert('系统提示', '请选择要设置的帐套.');
            return;
        }
        var treeNode = $('#projectTreeGrid').treegrid('getSelected');
        if (!treeNode) {
            $.messager.alert('系统提示', '请选择要对应的项目');
            return;
        }
        var id = treeNode.id;
        var child = $('#projectTreeGrid').treegrid('getChildren', treeNode.Guid);
        if (child && child.length > 0) {
            $.messager.alert('系统提示', '请选择要对应的项目的末级节点');
            return;
        }
        if (id.length == 0) {
            $.messager.alert('系统提示', '请选择要对应的项目的末级节点');
            return;
        }
        var row1 = $('#projectGrid').datagrid('getSelected');
        if (!row1 && type == 0) {
            $.messager.alert('系统提示', '请选择用友项目！');
            return;
        }
        var toformID = row1.citemcode;
        $.ajax({
            url: '/JCjksz/Savexmdygxsz',
            data: { id: id, toformKey: toformID, compasrMainID: node.id, type: type },
            error: function () {
                $.messager.alert('系统提示', '设置失败！');
            },
            success: function (data) {
                loadData(node.id);
                $('#projectGrid').datagrid('clearSelections');
                if (data.sucess == "1") {
                    $.messager.alert('系统提示', '操作成功！');
                    return
                }
                $.messager.alert('系统提示', data.msg);
            }
        })
    }
    $('#xmdygxsz-close').on('click', function (type) {
            parent.window.CloseTabs();
    });
    $('#xmdygxsz-save1').on('click', function (type) {
        SetPRalation(0);
    });
    $('#xmdygxsz-cancel1').on('click', function (type) {
        SetPRalation(1);
    });
    $('#projectTreeGrid').treegrid({
        height: $('body').height() - 145,
        singleSelect: true,
        onClickRow: function (node) {
            debugger
            var ExtKey = node.ExtKey;
            var rows = $('#projectGrid').datagrid('getRows');
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                if (row.citemcode == ExtKey) {
                    $('#projectGrid').datagrid('selectRow', i);
                    return;
                } else {
                    $('#projectGrid').datagrid('clearSelections');

                }
            }
        }
    });

    $('#projectGrid').datagrid({
        checkbox: true,
        checkOnSelect: true,
        singleSelect: true,
        selectOnCheck: true,
        url: '/Grid/GetU8Project',
        height: $('body').height() - 145
    });
    $('#xmdygxsz-tree-SS_ComparisonMain').tree({

        onClick: function (node) {
            debugger
            var url = "/Combo/UfFitem?extdb=" + node.attributes.ExteriorDataBase;
            url = url + "&year=" + node.attributes.ExteriorYear;
            $('#xmdygxsz-ufdata-fitem').combobox('clear');
            $('#xmdygxsz-ufdata-fitem').combobox('reload', url);
            loadData(node.id);
        }
    });
    $('#xmdygxsz-ufdata-fitem').combobox({
        onSelect: function (record) {
            var treenode = $('#xmdygxsz-tree-SS_ComparisonMain').tree("getSelected");
            if (treenode && record) {
                loadu8project(treenode, record);
            }
        }

    });
});


function loadu8project(node, record) {
    $("#projectGrid").datagrid('load', { "extdb": node.attributes.ExteriorDataBase, "year": node.attributes.ExteriorYear, "prj": record.ID });
};

