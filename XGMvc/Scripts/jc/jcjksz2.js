$.gridCheckBoxCol1 = function (a, b, c) {
    if (b.Color == 1) {
        return '<font color="red">' + a + '</font>';
    } else {
        return a;
    }
}
$(document).ready(function () {
    $('#sprojectTreeGrids').on('click', function (type) {
        debugger
        var text = $('#gwkbxd-PZ-GUID').val();
        var rows = $('#projectTreeGrid').datagrid('getRows');
        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (row.ModelName.indexOf(text) >= 0) {
                $('#projectTreeGrid').datagrid('selectRow', i);
                return;
            }
           
        }
    });

    $('#xmdygxsz-searchYY').on('click', function (type) {
        debugger
        var text = $('#gwkbxd-YY-GUID').val();
        var rows = $('#projectGrid').datagrid('getRows');
        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (row.citemcode.indexOf(text) >= 0) {
                $('#projectGrid').datagrid('selectRow', i);
                return;
            }
            if (row.citemname.indexOf(text) >= 0) {
                $('#projectGrid').datagrid('selectRow', i);
                return;
            }
        }
    });
    function loadData(id) {
        $.post('/JCjksz/GetKH', { id: id }, function (data) {
            $('#projectTreeGrid').datagrid('loadData', data);
        }, 'json');
    }

    var SetPRalation = function (type) {
        var node = $('#xmdygxsz-tree-SS_ComparisonMain').tree('getSelected');
        if (!node) {
            $.messager.alert('系统提示', '请选择要设置的帐套.');
            return;
        }
        var treeNode = $('#projectTreeGrid').datagrid('getSelected');
        if (!treeNode) {
            $.messager.alert('系统提示', '请选择要对应的客户');
            return;
        }
        var id = treeNode.Guid;
     
        if (id.length == 0) {
            $.messager.alert('系统提示', '请选择要对应的客户');
            return;
        }
        var row1 = $('#projectGrid').datagrid('getSelected');
        if (!row1 && type == 0) {
            $.messager.alert('系统提示', '请选择用友客户！');
            return;
        }
        var toformID = row1.citemcode;
        $.ajax({
            url: '/JCjksz/Savexmdygxsz',
            data: { id: id, toformKey: toformID, compasrMainID: node.id, type: type, Ctype: 'Customer', ClassID: '17' },
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
    $('#projectTreeGrid').datagrid({
        height: $('body').height() - 145,
        singleSelect: true,
        onClickRow: function (index,node) {
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
        url: '/Grid/GetU8Project2',
        height: $('body').height() - 145
    });
    $('#xmdygxsz-tree-SS_ComparisonMain').tree({

        onClick: function (node) {
            loadData(node.id);
            loadu8project(node);
        }
    });
});


function loadu8project(node, record) {
    $("#projectGrid").datagrid('load', { "extdb": node.attributes.ExteriorDataBase, "year": node.attributes.ExteriorYear, "prj": "Customer" });
};

