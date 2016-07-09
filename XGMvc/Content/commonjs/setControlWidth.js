






var arrGrid;
$.setControl = function (westDivId, datagridId, rowColCount, tableId) {//rowColCount 参数指函数的行列数 根据列数计算宽公式 tableId 布局Table的ID

    var westWidth = $("#" + westDivId).css("width").replace("px", "")
    var tableWidth = 1024;
    if (tableId) {
        var d = $('#' + tableId).width();
        if (d > 0) {
            tableWidth = d;
        }
    }
    else {
        var d = $('#table').width();
        if (d > 0) {
            tableWidth = d;
        }
    }

    if (westWidth) {
        tableWidth = tableWidth - westWidth;
    }
    else {
        tableWidth = tableWidth - 250; //默认值
    }
    if (datagridId) {
        arrGrid = datagridId.split(',');
    }
    var width = tableWidth * 0.33 * 0.8 * 0.96 //平均分三份，输入文本占用80%，并且占用单用表格的98% 
    if (rowColCount) {
        switch (rowColCount) {
            case 4:
                width = tableWidth * (1 / (rowColCount / 2)) * 0.8 * 0.96 // 1/(rowColCount/2)指把一列平均分成若干份，每份占用的百分比
                break;
            case 8:
                width = tableWidth * (1 / (rowColCount / 2)) * 0.68 * 0.96 // 1/(rowColCount/2)指把一列平均分成若干份，每份占用的百分比
                break;
            default:
                width = tableWidth * 0.33 * 0.8 * 0.96;
                break;
        }
    }
    //IE 中布局设置
    if (window.attachEvent) {
        $.setTableStyle(tableWidth, rowColCount);
        width = $.setIEControlCalculateWidth(tableWidth, rowColCount);
    }

    $.setDataboxWidth(width);

    $.setValidatebox(width);
    $.setNumberbox(width);
    $.setCombobox(width);
    $.setCombogrid(width);
    $.setEdatagrid(tableWidth, datagridId);


}
$.setDataGridControlWidth = function (tableId, datagridId) {    
    //var westWidth = $("#" + westDivId).css("width").replace("px", "")
    var tableWidth = 1024;
    if (tableId) {
        var d = $('#' + tableId).width();
        if (d > 0) {
            tableWidth = d;
        }
    }
    else {
        var d = $('#table').width();
        if (d > 0) {
            tableWidth = d;
        }
    }
    if (datagridId) {
        arrGrid = datagridId.split(',');
    }
    
    $.setEdatagrid(tableWidth);
}

//布局没有Table布局,用Div布局时设置DataGrid 应用在基础权限页面
$.setDataGridControlSize = function (centerDivId, datagridId) {
    
    if (datagridId) {
        arrGrid = datagridId.split(',');
    }
    var centerWidth = $("#" + centerDivId).css("width").replace("px", "");  
    var tableWidth = 1024;
    if (centerWidth) {
        tableWidth = centerWidth * 0.80;
    }
    $.setEdatagrid(tableWidth);
}
//设置Databox 的宽度
$.setDataboxWidth = function (width) {
    $(".easyui-datebox").each(function (e) {
        $(this).datebox({
            width: width
        });
    });
}
//设置Combogrid宽度
$.setCombogrid = function (width) {
    $(".easyui-combogrid").each(function (e) {
        $(this).combogrid({
            width: width
        });
    });
}
//easyui-validatebox
$.setValidatebox = function (width) {
    $(".easyui-validatebox").filter(":not(:hidden)").each(function (i) {
        if (!$(this)[0].style.width) {
            $(this).css("width", width - 4);
        }
    });
}
//numberbox
$.setNumberbox = function (width) {
    $(".easyui-numberbox").filter(":not(:hidden)").each(function (i) {
        if (!$(this)[0].style.width) {
            $(this).css("width", width - 4);
        }
    });
}
//easyui-combobox
$.setCombobox = function (width) {
    $(".easyui-combobox").each(function (e) {
    
        $(this).combobox({
            width: width
        });
    });
    //暂时不起作用
//    $("select.easyui-combobox").each(function (e) {
//        $(this).css("width", width - 3);
//    });
}
//easyui-edatagrid
$.setEdatagrid = function (width) {
    if (window.attachEvent) {
        if (width > 1000) {
            width = width// - 250;
        }
    }
    var dataGrid;
    $(".easyui-edatagrid").each(function (i) {
        var dataGrid = $(this);
        var id = dataGrid.attr("id");
        if ($.IsExist(id)) {
            var view2 = dataGrid.prev();
            var view1 = view2.prev();
            var view = dataGrid.parent('div');
            var wrap = view.parent('div');
            var dg = wrap.parent('div');
            dg.width(width);
            wrap.width(width - 2);
            view.width(width - 2);
            var view2width = width - view1.width();
            view2.width(view2width);
            view2.children().width(view2width);
        }


        //        var opts = dataGrid.edatagrid('options');

        //        opts.width = width;
        //        if ($.IsExist(id)) {
        //            debugger
        //            dataGrid.edatagrid({
        //                width: width,
        //                allEditing: opts.allEditing
        //            });

        //        }
    });
    $(".easyui-datagrid").each(function (i) {
        var dataGrid = $(this);
        var opts = dataGrid.edatagrid('options');
        var id = dataGrid.attr("id");

        if ($.IsExist(id)) {
            var view2 = dataGrid.prev();
            var view1 = view2.prev();
            var view = dataGrid.parent('div');
            var wrap = view.parent('div');
            var dg = wrap.parent('div');
            dg.width(width);
            wrap.width(width - 2);
            view.width(width - 2);
            var view2width = width - view1.width();
            view2.width(view2width);
            view2.children().width(view2width);
        }


//        if ($.IsExist(id)) {
//            dataGrid.edatagrid({
//                width: width - 6,
//                allEditing: opts.allEditing
//            });
//        }
    });
}
//设置IE下的width为具体的px
$.setTableStyle = function (tableWidth, rowColCount) {
    
    var width = tableWidth * 0.33 * 0.4 * 0.96 //平均分三份，输入文本占用80%，并且占用单用表格的98% 显示文本为20%
    var valueWidth = tableWidth * 0.33 * 0.6 * 0.96

    if (rowColCount) {
        switch (rowColCount) {
            case 4:
                width = tableWidth * (1 / (rowColCount / 2)) * 0.4 * 0.96 // 1/(rowColCount/2)指把一列平均分成若干份，每份占用的百分比
                valueWidth = tableWidth * (1 / (rowColCount / 2)) * 0.6 * 0.96 
                break;
            case 8:
                width = tableWidth * (1 / (rowColCount / 2)) * 0.5 * 0.96 // 1/(rowColCount/2)指把一列平均分成若干份，每份占用的百分比
                valueWidth = tableWidth * (1 / (rowColCount / 2)) * 0.5 * 0.96 
                break;
            default:
                width = tableWidth * 0.33 * 0.4 * 0.96;
                valueWidth = tableWidth * 0.33 * 0.6 * 0.96;
                break;
        }
    }
    $(".tdText").each(function (i) {
        
        if (!$(this)[0].style.width) {
            $(this).css("width", width);
        }
    });

    $(".tdValue").each(function (i) {
        
        if (!$(this)[0].style.width) {
            $(this).css("width", valueWidth);
        }
    });

}
//设置Ie下的控件width
$.setIEControlCalculateWidth = function (tableWidth, rowColCount) {
    var width = tableWidth * 0.33 * 0.7 * 0.96 //平均分三份，输入文本占用80%，并且占用单用表格的98% 显示文本为20%
    if (rowColCount) {
        switch (rowColCount) {
            case 4:
                width = tableWidth * (1 / (rowColCount / 2)) * 0.7 * 0.96 // 1/(rowColCount/2)指把一列平均分成若干份，每份占用的百分比
                break;
            case 8:
                width = tableWidth * (1 / (rowColCount / 2)) * 0.7 * 0.96 // 1/(rowColCount/2)指把一列平均分成若干份，每份占用的百分比
                break;
            default:
                width = tableWidth * 0.33 * 0.7* 0.96;
                break;
        }
    }
    return width;
}
//判断是否存在
$.IsExist = function (id) {
    for (var i = 0; i < arrGrid.length; i++) {
        if (arrGrid[i] == id) {
            return true;
            break;
        }
        return false;
    }
}


