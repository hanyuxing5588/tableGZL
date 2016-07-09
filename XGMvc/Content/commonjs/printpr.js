
//strPrintName  打印任务名称
//printDatagrid     要打印的datagrid

//function CreateFormPage(strPrintName, printDatagrid) {
//    var tableString = '<table id="test" cellpadding="0" cellspadding="0" class="pb">';
//    var frozenColumns = printDatagrid.datagrid('options').frozenColumns;    //得到frozenColumns对象
//    var columns = printDatagrid.datagrid('options').columns;    //得到columns对象
//    var nameList = '';
//    
//    //载入title
//    if (typeof columns != 'undefined' && columns != '') {
//        $(columns).each(function (index) {
//            tableString += '\n<tr>';
//            if (typeof frozenColumns != 'undefined' && typeof frozenColumns[index] != 'undefined') {
//                for (var i = 0; i < frozenColumns[index].length; i++) {
//                    if (!frozenColumns[index][i].hidden) {
//                        tableString += '\n<th width="' + frozenColumns[index][i].width + '"';
//                        if (typeof frozenColumns[index][i].rowspan != 'undefined' && frozenColumns[index][i].rowspan > 1) {
//                            tableString += 'rowspan"' + frozenColumns[index][i].rowspan + '"';
//                        }
//                        if (typeof frozenColumns[index][i].colspan != 'undefined' && frozenColumns[index][i].colspan > 1) {
//                            tableString += 'colspan"' + frozenColumns[index][i].colspan + '"';
//                        }
//                        if (typeof frozenColumns[index][i].field != 'undefined' && frozenColumns[index][i].field != '') {
//                            nameList += ',{"f":"' + frozenColumns[index][i].field + '","a":"' + frozenColumns[index][i].align + '"}';
//                        }
//                        tableString += '>' + frozenColumns[0][i].title + '</th>';
//                    }
//                }
//            }

//            for (var i = 0; i < columns[index].length; i++) {
//                if (!columns[index][i].hidden) {
//                    tableString += '\n<th width="' + columns[index][i].width + '"';
//                    if (typeof columns[index][i].rowspan != 'undefined' && typeof columns[index][i].rowspan > 1) {
//                        tableString += 'rowspan="' + columns[index][i].rowspan + '"';
//                    }
//                    if (typeof columns[index][i].colspan != 'undefined' && typeof columns[index][i].colspan > 1) {
//                        tableString += 'colspan="' + columns[index][i].colspan + '"';
//                    }
//                    if (typeof columns[index][i].field != 'undefined' && typeof columns[index][i].field != '') {
//                        nameList += ',{"f":"' + columns[index][i].field + '","a":"' + columns[index][i].align + '"}';
//                    }
//                    tableString += '>' + columns[index][i].title + '</th>';
//                }
//            }
//            tableString += '\n</tr>';
//        });
//    }
//    //载入内容
//    var rows = printDatagrid.datagrid('getRows');   //获取当前所有行
//    var nl = eval('([' + nameList.substring(1) + '])');
//    for (var i = 0; i < rows.length; i++) {
//        tableString += '\n<tr>';
//        $(nl).each(function (j) {
//            var e = nl[j].f.lastIndexOf('_0');
//            tableString += '\n<td';
//            if (nl[j].a != 'undefined' && nl[j].a != '') {
//                tableString += 'style="text-align:' + nl[j].a + ';"';
//            }
//            tableString += '>';
//            if (e + 2 == nl[j].f.length) {
//                tableString += rows[i][nl[j].f.substring(0, e)];
//            }
//            else
//                tableString += rows[i][nl[j].f];
//            tableString += '</td>';
//        });
//        tableString += '\n</tr>';
//    }
//    tableString += '\n</table>';
//    //预览()
//    window.showModalDialog(tableString, window.print());

    //window.showModalDialog(window.print(), tableString, $($('<div></div>').html(this.clone())).html());
    //window.showModalDialog("/print.html", tableString, "location:NO;status:NO;help:NO;dialogWidth:800px;height:600px; scroll:auto;");
}



//strPrintName  打印任务名称
//printDatagrid     要打印的datagrid
//var tableString = "<link rel = 'syslesheet' type='text/css' href=''/><table cellspadding='0' cellpadding='0' id='PrintBody'>";
//function doPrintpr() {
//    tableString += '<script type="text/javascript">window.print();</s"' + '"cript>"';
//    document.open('', '', height = 500, width = 600, scrollbars = yes, status = yes);
//    document.write(tableString);
//    document.close();
//}
//function CreateFormPage(strPrintName, printDatagrid) {
//
//    // var tableString = '<table id="test" cellpadding="0" cellspadding="0" class="pb">';
//    var frozenColumns = printDatagrid.datagrid('options').frozenColumns;    //得到frozenColumns对象
//    var columns = printDatagrid.datagrid('options').columns;    //得到columns对象

//    //载入title
//    tableString += '\n<tr>';
//    if (frozenColumns != undefined && frozenColumns != '') {
//        for (var i = 0; i < frozenColumns[0].length; i++) {
//            if (frozenColumns[i][0].hidden != true) {
//                tableString = tableString += '\n<th width="' + frozenColumns[0][i].width + '">"' + frozenColumns[0][i].title + '"</th>"';
//            }
//        }
//    }
//    if (columns != undefined && columns != '') {
//        for (var i = 0; i < columns[0].length; i++) {
//            if (columns[0][i].hidden != true) {
//                tableString = tableString += '\n<th width="' + columns[0][i].width + '">"' + columns[0][i].title + '"</th>"';
//            }
//        }
//    }
//    tableString = tableString + '\n</tr>';

//    //载入内容
//    var rows = printDatagrid.datagrid('getRows');   //获取当前所有行
//    for (var j = 0; j < rows.lenght; j++) {
//        tableString += '\n<tr>';
//        if (frozenColumns != undefined && frozenColumns != '') {
//            for (var i = 0; i < frozenColumns[0].length; i++) {
//                if (frozenColumns[0][i].hidden != true) {
//                    tableString = tableString += '\n<td>"' + rows[j][frozenColumns[0][i].field] + '"</td>';
//                }
//            }
//        }
//        if (columns != undefined && columns != '') {
//            for (var i = 0; columns[0].length; i++) {
//                if (columns[0][i].hidden != true) {
//                    tableString = tableString += '\n<td>"' + rows[j][columns[0][i].field] + '"</th>';
//                }
//            }
//        }
//        tableString = tableString + '\n</tr>';
//    }
//    tableString = tableString + '\n<table>';
//    doPrintpr();
//}

















































