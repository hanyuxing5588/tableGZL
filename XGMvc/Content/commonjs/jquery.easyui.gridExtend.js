/*
扩展datagrid
*/
$.extend($.fn.datagrid.methods, {
    alterStatus: function (jq, status) {
        return jq.each(function () {
            $(this).edatagrid('moveRow');
            $(this).edatagrid('addScroll');
        });
    },
    //上移
    moveUp: function (jq) {
        return jq.each(function () {
            var me = $(this);
            var row = me.datagrid('getSelected');
            var index = me.datagrid('getRowIndex', row);
            if (index == 0) return;
            var toup = me.datagrid('getData').rows[index];
            var todown = me.datagrid('getData').rows[index - 1];
            me.datagrid('getData').rows[index] = todown;
            me.datagrid('getData').rows[index - 1] = toup;
            me.datagrid('refreshRow', index);
            me.datagrid('refreshRow', index - 1);
            me.datagrid('selectRow', index - 1);
        })
    },
    //下移
    moveDown: function (jq) {
        return jq.each(function () {
            var me = $(this);
            var row = me.datagrid('getSelected');
            var index = me.datagrid('getRowIndex', row);
            var rows = me.datagrid('getRows').length;
            if (index == rows - 1) return;
            var todown = me.datagrid('getData').rows[index];
            var toup = me.datagrid('getData').rows[index + 1];
            me.datagrid('getData').rows[index + 1] = todown;
            me.datagrid('getData').rows[index] = toup;
            me.datagrid('refreshRow', index);
            me.datagrid('refreshRow', index + 1);
            me.datagrid('selectRow', index + 1);
        })
    },
    /** 
    * 开打提示功能   
    * @param {} jq   
    * @param {} params 提示消息框的样式   
    * @return {}   
    */
    doCellTip: function (jq, params) {
        function showTip(data, td, e) {
            if ($(td).text() == "")
                return;
            data.tooltip.text($(td).text()).css({
                top: (e.pageY + 10) + 'px',
                left: (e.pageX + 20) + 'px',
                'z-index': $.fn.window.defaults.zIndex,
                display: 'block'
            });
        };
        return jq.each(function () {
            var grid = $(this);
            var options = $(this).data('datagrid');
            if (!options.tooltip) {
                var panel = grid.datagrid('getPanel').panel('panel');
                var defaultCls = {
                    'border': '1px solid #333',
                    'padding': '1px',
                    'color': '#333',
                    'background': '#f7f5d1',
                    'position': 'absolute',
                    'max-width': '200px',
                    'border-radius': '4px',
                    '-moz-border-radius': '4px',
                    '-webkit-border-radius': '4px',
                    'display': 'none'
                }
                var tooltip = $("<div id='celltip'></div>").appendTo('body');
                tooltip.css($.extend({}, defaultCls, params.cls));
                options.tooltip = tooltip;
                panel.find('.datagrid-body').each(function () {
                    var delegateEle = $(this).find('> div.datagrid-body-inner').length
                            ? $(this).find('> div.datagrid-body-inner')[0]
                            : this;
                    $(delegateEle).undelegate('td', 'mouseover').undelegate(
                            'td', 'mouseout').undelegate('td', 'mousemove')
                            .delegate('td', {
                                'mouseover': function (e) {
                                    if (params.delay) {
                                        if (options.tipDelayTime)
                                            clearTimeout(options.tipDelayTime);
                                        var that = this;
                                        options.tipDelayTime = setTimeout(
                                                function () {
                                                    showTip(options, that, e);
                                                }, params.delay);
                                    } else {
                                        showTip(options, this, e);
                                    }

                                },
                                'mouseout': function (e) {
                                    if (options.tipDelayTime)
                                        clearTimeout(options.tipDelayTime);
                                    options.tooltip.css({
                                        'display': 'none'
                                    });
                                },
                                'mousemove': function (e) {
                                    var that = this;
                                    if (options.tipDelayTime) {
                                        clearTimeout(options.tipDelayTime);
                                        options.tipDelayTime = setTimeout(
                                                function () {
                                                    showTip(options, that, e);
                                                }, params.delay);
                                    } else {
                                        showTip(options, that, e);
                                    }
                                }
                            });
                });

            }

        });
    },
    /** 
    * 关闭消息提示功能   
    * @param {} jq   
    * @return {}   
    */
    cancelCellTip: function (jq) {
        return jq.each(function () {
            var data = $(this).data('datagrid');
            if (data.tooltip) {
                data.tooltip.remove();
                data.tooltip = null;
                var panel = $(this).datagrid('getPanel').panel('panel');
                panel.find('.datagrid-body').undelegate('td',
                                'mouseover').undelegate('td', 'mouseout')
                                .undelegate('td', 'mousemove')
            }
            if (data.tipDelayTime) {
                clearTimeout(data.tipDelayTime);
                data.tipDelayTime = null;
            }
        });
    },
    //验证datagrid
    verifyData: function () {
        
        var opts = $(this).datagrid('options');
        var cols = opts.requireCol;
        if (!cols) return;
        var rows = $(this).datagrid('getRows');
        if (rows.length == 0) return;
        var msgError = "", fieldTitle = "";
        for (var i = 0, j = rows.length; i < j; i++) {
            for (var m = 0, n = cols.length; m < n; m++) {

                var col = cols[m];
                if ($.trim(rows[i][col] + '').toLowerCase().length <= 0) {
                    var colO = $(this).datagrid('getColumnOption', col);
                    fieldTitle += $.trim(colO.title) + "、";
                }
            }
            var tempVal = fieldTitle.substring(0, fieldTitle.length - 1);
            fieldTitle = "";
            msgError += "第" + (i + 1) + "行：" + tempVal + "不能为空！</br>";
            if (tempVal == "") {
                msgError = "";
            }
        }
        return msgError;
    },
    //datagrid分页
    pagerFilter: function (data) {
        //根据操作员的guid获取当前操作员所在页码

        
        var getPageNumberByGuid = function (guid, rows, pageSize) {
            if (!rows) return 1;
            var rowIndex = 0;
            for (var i = 0, j = rows.length; i < j; i++) {
                var row = rows[i];
                if (row.GUID == guid) {
                    rowIndex = (i + 1);
                    break;
                }
            }
            if (rowIndex == 0) return 1;
            return rowIndex % pageSize == 0 ? parseInt(rowIndex / pageSize) : parseInt(rowIndex / pageSize) + 1;
        };
        if (!data) {
            data = {
                total: 0,
                rows: []
            }
        }

        if (typeof data.length == 'number' && typeof data.splice == 'function') {
            data = {
                total: data.length,
                rows: data
            }
        }
        var dg = $(this);
        var opts = dg.datagrid('options');
        var pager = dg.datagrid('getPager');
        //如果datagrid 的pageNumber等于1 那么需要将$(this).datagrid('options').pageNumber 的值修改成 1
        //sxh 2014/04/03 10:54
        var pgNum = $(this).datagrid('getPager').pagination('options').pageNumber;
        if (pgNum == 1) {
            opts.pageNumber = 1;
        }
        pager.pagination({
            onSelectPage: function (pageNum, pageSize) {
                opts.pageNumber = pageNum;
                opts.pageSize = pageSize;
                pager.pagination('refresh', {
                    pageNumber: pageNum,
                    pageSize: pageSize
                });
                dg.datagrid('loadData', data);
            }
        });
        if (!data.originalRows) {
            data.originalRows = (data.rows);
        }
        if (!opts.isLoad) {
            opts.pageNumber = getPageNumberByGuid(opts.tempValue, data.rows, parseInt(opts.pageSize));
            opts.isLoad = true;
            var p = pager.pagination('options');
            p.pageNumber = opts.pageNumber;
        }
        var start = (opts.pageNumber - 1) * parseInt(opts.pageSize);
        var end = start + parseInt(opts.pageSize);
        data.rows = (data.originalRows.slice(start, end));
        return data;
    }
});
$.extend($.fn.datagrid.defaults, {
    onBeforeLoad: function () {

        var opts = $(this).datagrid('options');
        //        if(opts.loadFormatters)return;
        if (opts.formatters) {
            for (var field in opts.formatters) {
                var colConfig = $(this).datagrid('getColumnOption', field);
                colConfig.formatter = $.view.formatters[opts.formatters[field]];

            }
        }

        if (opts.pagination) {
            opts.loadFilter = $.fn.datagrid.methods["pagerFilter"];
        }

        //        opts.loadFormatters=true;
    }
});
$.extend($.fn.datagrid.defaults.editors, {
    combogrid: {
        init: function (container, options) {
            var input = $("<input type=\"text\" class=\"datagrid-editable-combogrid\" />").appendTo(container);
            input.combogrid(options);

            //绑定事件
            $.view.bind.call($(input), 'combogrid');
            //加载远程数据
            $(input).combogrid('loadRemoteDataToLocal');
            

            return input;
        },
        destroy: function (target) {
            //            var val = target.combogrid('getText');
            //            var opts = "", opts1;
            //            try {
            //                opts = target.combogrid('options');
            //            } catch (e) {
            //                
            //            }
            //            if (opts) {
            //                opts1 = { textField: opts.textField, gridassociate: opts.gridassociate, idField: opts.idField };
            //            }
            //            $(target).combogrid('destroy');
            //            if (!val&&opts1) {
            //                $.fn.combogrid.methods["clearMyself"](opts1);
            //                delete opts1;
            //            }

            $(target).combogrid('destroy');
        },
        getValue: function (target) { //回到grid


            var opts = $(target).combogrid('options');
            var val = $(target).combogrid('getValue'), text = $(target).combogrid('getText');
            var grid$ = $(target).combogrid("grid");
            var rowIndex = grid$.datagrid("getRowIndex", val);
            if (!opts.customValue && rowIndex < 0) {
                text = "";
                val = "";
            }
            if (!text) {
                $.fn.combogrid.methods["setAssociate"].call(target);
                val = "";
            }
            var selecteRow = grid$.datagrid('getSelected');
            if (opts.customValue && !selecteRow) {
                val = "";
            }
            target.parents().find("div.datagrid-editable").attr("oaovalue", val);
            return text;
        },
        setValue: function (target, value) {//从grid的来

            var ivalue = target.parents().find("div.datagrid-editable").attr("oaovalue");
            $(target).combogrid('setValue', ivalue)
            return $(target).combogrid('setText', value);
        },
        resize: function (target, width) {
            $(target).combogrid("resize", width);
        }
    },
    booleanbox: {
        init: function (container, options) {
            var input = $("<input type=\"text\"  class=\"datagrid-editable-booleanbox\" />").appendTo(container);

            input.combobox({ panelHeight: 50, editable: false, valueField: 'id', textField: 'text', data: [{ id: 'false', text: '否' }, { id: 'true', text: '是'}] });
            input.combobox(options);
            //绑定事件
            //            $(input).combobox('bind', true);
            $.view.bind.call($(input), 'combobox');
            //加载数据
            return input;
        },
        destroy: function (target) {
            $(target).combobox('destroy');
        },
        getValue: function (target) {
            //            target.parents().find("div.datagrid-editable").attr("oaovalue",$(target).combobox('getValue')); 
            return $(target).combobox('getValue');
        },
        setValue: function (target, value) {

            if (value) {
                return $(target).combobox('setValue', value == '是' || (value + '').toString().toLocaleLowerCase() == "true" ? 'true' : 'false');
            }
            else {
                return $(target).combobox('setValue', 'false');
            }

        },
        resize: function (target, width) {
            $(target).combobox("resize", width);
        }
    },
    numberbox: {
        init: function (container, options) {

            var input = $("<input type=\"text\" max=\"99999999.99\" class=\"datagrid-editable-numberbox\" />").appendTo(container);
            options = $.extend(options, { min: -99999999.99 });
            input.numberbox(options);
            //绑定事件
            //$.view.bind.call($(input), 'numberbox');
            //加载数据
            return input;
        },
        destroy: function (target) {
            $(target).numberbox('destroy');
        },
        getValue: function (target) {
            var result = $(target).numberbox('getValue');
            if (parseFloat(result) == 0 || result == "") {
                result = "0.00";
            }
            return result;
        },
        setValue: function (target, value) {
            if ($.isNumeric(value) && parseFloat(value) == 0) {
                value = '';
            }
            return $(target).numberbox('setValue', value);
        },
        resize: function (_5f1, _5f2) {
            $(_5f1)._outerWidth(_5f2)._outerHeight(22);
        }
    },
    text: {
        init: function (_176, _177) {
            var _178 = $("<input type=\"text\" class=\"datagrid-editable-input\">").appendTo(_176);
            return _178;
        },
        getValue: function (_179) {
            //过滤掉所有html标签
            var temp = $(_179).val().stripHTML();
            return temp;
        },
        setValue: function (_17a, _17b) {
            $(_17a).val(_17b);
        },
        resize: function (_17c, _17d) {
            $(_17c)._outerWidth(_17d)._outerHeight(22);
        }
    },
    datebox: {
        init: function (_19f, _1a0) {
            var _1a1 = $("<input type=\"text\">").appendTo(_19f);
            _1a1.datebox(_1a0);
            return _1a1;
        },
        destroy: function (_1a2) {
            $(_1a2).datebox("destroy");
        },
        getValue: function (_1a3) {

            var date = $(_1a3).datebox("getValue");

            if (date != "" && date.length != 0) {
                date = new Date().FormatDate(date);
            }
            return date;
        },
        setValue: function (_1a4, _1a5) {

            if (_1a5 != "" && _1a5 != undefined && _1a5.length != 0) {
                _1a5 = new Date().FormatDate(_1a5);
            }
            $(_1a4).datebox("setValue", _1a5);
        },
        resize: function (_1a6, _1a7) {
            $(_1a6).datebox("resize", _1a7);
        }
    },

    datetimebox: {
        init: function (_19f, _1a0) {
            var _1a1 = $("<input type=\"text\">").appendTo(_19f);
            _1a1.datetimebox(_1a0);
            return _1a1;
        },
        destroy: function (_1a2) {
            $(_1a2).datetimebox("destroy");
        },
        getValue: function (_1a3) {

            var date = $(_1a3).datebox("getValue");
            if (date != "" && date.length != 0) {
                date = new Date().FormatDate(date);
            }
            return date;
        },
        setValue: function (_1a4, _1a5) {
            if (_1a5 != "" && _1a5.length != 0) {
                _1a5 = new Date().FormatDate(_1a5);
            }
            $(_1a4).datetimebox("setValue", _1a5);
        },
        resize: function (_1a6, _1a7) {
            $(_1a6).datetimebox("resize", _1a7);
        }
    },
    searchbox: {
        init: function (container, options) {
            ;
            var input = $('<input type="text" class="datagrid-editable-searchbox">').appendTo(container);
            input.searchbox({
                config: options,
                searcher: function (value, name) {
                    var d = $(input).searchbox("options").config;
                    var parms = d.particulars;
                    var fun = $.fn.linkbutton.methods["particularsDetail"]
                    if (fun) {
                        fun(parms, d.window);
                    }
                },
                prompt: ''
            });
            return input;
        },
        destroy: function (target) {
            $(target).searchbox('destroy');
        },
        getValue: function (target, t, j, u) {
            var val = $(target).searchbox('getValue');
            return val;
        },
        setValue: function (target, value) {
            var d = $(target).get()[0];
            var o = d.nextElementSibling || d.nextSibling;
            o.childNodes[0].readOnly = true;
            return $(target).searchbox('setValue', value);
        },
        resize: function (target, width) {
            $(target).searchbox("resize", width);
        }
    }
});
/*
插件扩展:edatagrid(可编辑列表)
*/
(function ($) {
    var b_editors = {}; //存储各个列的editor

    function buildGrid(target) {
        var opts = $.data(target, 'edatagrid').options;
        $(target).datagrid($.extend({}, opts, {
            onDblClickCell: function (rowIndex, field, value) {
                opts.onDblClickCell.call(target, rowIndex, field, value);
            },
            onClickCell: function (index, field,value) {
                opts.curEditField=undefined;
                if (opts.editIndex1 != undefined && opts.editIndex1 >= 0) {
                    $(this).datagrid('endEdit', opts.editIndex1);
                }
                if (opts.notEditCols && opts.notEditCols.exist(field)) {
                    opts.editIndex1 = undefined;
                    return;
                }
                if (opts.editBefore) {
                    var isEditCan = opts.editBefore.call(target, {index:index,field:field});
                    if (!isEditCan) return;
                }
                
                $(this).edatagrid('editCell', { index: index, field: field });
                opts.curEditField=field;
                opts.curEditValue=value;//当前表格值


                opts.editIndex1 = index;
                opts.editField = field;
                
            },
            onBeforeEdit:function(index,rowData)
            {
                opts.editIndex1 = index;                
            },
            onAfterEdit: function (index, row) {
                opts.editIndex = undefined;
                if (opts.onAfterEdit) opts.onAfterEdit.call(target, index, row,opts.editField);
                
            },
            onCancelEdit: function (index, row) {
                opts.editIndex = undefined;
                if (row.isNewRecord) {
                    $(this).datagrid('deleteRow', index);
                }
                if (opts.onCancelEdit) opts.onCancelEdit.call(target, index, row);
                
            },
            onBeforeLoad: function (param) {
                $(this).datagrid('rejectChanges');
                if (opts.tree) {
                    var node = $(opts.tree).tree('getSelected');
                    param[opts.treeParentField] = node ? node.id : undefined;
                }
                if (opts.onBeforeLoad) opts.onBeforeLoad.call(target, param);
            }
        }));

        function focusEditor(field) {
            var editor = $(target).datagrid('getEditor', { index: opts.editIndex, field: field });
            if (editor) {
                editor.target.focus();
            } else {
                var editors = $(target).datagrid('getEditors', opts.editIndex);
                if (editors.length) {
                    editors[0].target.focus();
                }
            }
        }

    }

    $.fn.edatagrid = function (options, param) {

        if (typeof options == 'string') {
            var method = $.fn.edatagrid.methods[options];
            if (method) {
                return method(this, param);
            } else {
                return this.datagrid(options, param);
            }
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, 'edatagrid');
            if (state) {
                $.extend(state.options, options);

            } else {
                $.data(this, 'edatagrid', {
                    options: $.extend({}, $.fn.edatagrid.defaults, $.fn.edatagrid.parseOptions(this), options)
                });
            }
            buildGrid(this);
        });
    };

    $.fn.edatagrid.parseOptions = function (target) {
        return $.extend({}, $.fn.datagrid.parseOptions(target), {
    });
   
};

$.fn.edatagrid.methods = {
    sumField: function (jq, field) {
        if (!field) {
            return null;
        }
        
        var opts=$(jq).edatagrid('options');
        
        var rows=undefined;
        if (opts.scope=="kjpz"){
            
            rows=$(jq).datagrid('getData');
            rows=rows.rows;
        }
        else{
            rows = $(jq).datagrid('getRows');
        }
        
        var value = 0;
        for (var i = 0; i < rows.length; i++) {
            var dv = rows[i];
            var vv = dv[field];
            if (vv) {
                value += parseFloat(vv);
            }
        }
        return value.toFixed(2);
    },
    //field formula function [{name:'',field:'',target:['','']},{name:'',field:'',target:['','']}]
    doFormula: function (jq, params) {
        
        var opts = $(jq).datagrid('options');
        if (opts.fieldformula) {
            for (var i = 0; i < opts.fieldformula.length; i++) {
                var formula = opts.fieldformula[i];
                if (formula) {
                    $(jq).edatagrid(formula.name, params);
                }
            }
        }
    },
    csum: function (jq) {
        
        var formula = $.fn.edatagrid.methods['getFormula'](jq, 'csum');
            for (var i = 0,j=formula.length; i < j; i++) {
                if (formula[i] && formula[i].field && formula[i].target) {
                var tempValue = $.fn.edatagrid.methods['sumField'](jq, formula[i].field);

                for (var m = 0, n = formula[i].target.length; m < n; m++) {
                    var mfield = formula[i].target[m];
                    if (mfield) {
                        var val = new Number(tempValue).formatThousand(tempValue);
                        $('#' + mfield).validatebox('setText', val);
                    }
                }
            }
        }
        
    },
    cconcat: function (jq) {
        if ($.view.saveDocMemoNotCconcat) return;
        
        var formulaArray = $.fn.edatagrid.methods['getFormula'](jq, 'cconcat');
        if (formulaArray==null) return;
        for (var fi=0,fil=formulaArray.length;fi<fil;fi++){
            var formula=formulaArray[fi];
            if (formula && formula.field && formula.target) {
                var value = $.fn.edatagrid.methods['concatField'](jq, formula.field);
                for (var i = 0, j = formula.target.length; i < j; i++) {
                    var tfield = formula.target[i];
                    if (tfield && value != undefined) {
                        $('#' + tfield).validatebox('setText', value);
                        //                    $('#' + tfield).focus();
                    }
                }

            }
        }
    },
    //没数据加上滚动条
    addScroll: function (jq, width) {
    
        var gdata = $.data($(jq).get(0), "datagrid");
        var t = gdata.data;
        if (!t || !t.rows || t.rows.length > 0) return;
        var dc = gdata.dc;
        if (!dc) return;
        var view2 = dc.view2;
        if (!view2) return;
        var w = 0;
        var opts = $(jq).edatagrid('options');
        if (opts.isNotSrocll) return;
        if (!opts.scw)//列的总宽度

        {
            var fields = $(jq).datagrid('getColumnFields');
            for (var i = 0; i < fields.length; i++) {
                var colC = $(jq).datagrid('getColumnOption', fields[i]);
                if (colC.width) {
                    w += colC.width;
                }
            }
            opts.scw = w;
        }
        w = opts.scw;
        var c = $.format("<div style=\"width:{0}px;border:0px solid;height:1px;\" > </div>", w || 1080);
        view2.children(".datagrid-body").html(c);
    },
    //键盘控制行
    KeyCtr: function (jq) {
        return jq.each(function () {
            //得到当前的grid
            var grid = $(this);
            //获取datagrid面板,给panel添加事件
            grid.datagrid('getPanel').panel('panel').attr('tabindex', 1).bind('keydown', function (e) {
            //将原来的给datagrid的panel绑定方式改成给datagridrow绑定 sxh 2014/04/08
//            $(document).on('keydown.datagridrow',function(e){
                switch (e.keyCode) {
                    case 38:    //up(上键)
                        //得到的数据行
                        var selected = grid.datagrid('getSelected');
                        if (!selected)return;
                        //得到当前行索引

                        var index = grid.datagrid('getRowIndex', selected);
                        if (index==0) {
                            //获得所有行
                            var allRows = grid.datagrid('getRows');
                            if (!allRows)return;
                            grid.datagrid('selectRow', allRows.length-1);
                        }else {//如果走到第一行，将重新返回到最后一行

                            grid.datagrid('selectRow', index - 1);
                        }
                        break;
                    case 40:    //down(下键)
                        var seleceted = grid.datagrid('getSelected');
                        if (!seleceted)return;
                        var index = grid.datagrid('getRowIndex', seleceted);
                        var allRows = grid.datagrid('getRows');
                        if (!allRows)return;
                        if (index==allRows.length-1) {
                            grid.datagrid('selectRow',0);
                        }else {//如果走到最后一行，那么选中行将从第一行开始

                            grid.datagrid('selectRow',index+1);
                        }
                        break;
                }
            });
        });
    },
    moveRow:function(jq){
        var opts = $(jq).datagrid('options');
        if (!opts.moveRow) {
            var dmr = new DatagridMoveRow(jq);
            opts.moveRow =true;
            $(document).on('keydown.datagridrow', function (e) {
                if (e.keyCode==38) {
                    dmr.moveUp();
                }else if (e.keyCode==40) {
                    dmr.moveDown();
                }
            });
        }
    },
    //转换当前可编辑状态 {'field1':[],'field2':[]}
    alterStatus: function (jq, status) {
        return jq.each(function () {
            
            var opts = $(this).edatagrid('options');

            $(this).edatagrid('moveRow');
            if(opts.noStatus)return;
            if (!opts.bindFun) {
                //绑定公式
                var tempFun = function (i, data) {
                    $(this).edatagrid('doFormula', { index: i, row: data });
                }
                opts.onAfterEdit = function (i, data,changes) {
                    if(opts.editEditAfterEvent){
                        opts.editEditAfterEvent.call(this,i,data,changes);
                    }
                   $(this).edatagrid('doFormula', { index: i, row: data });
                };
                opts.onAdd = tempFun;
                opts.onAfterDestroy = tempFun;

                $(this).edatagrid('addScroll');
               
                opts.bindFun = true;
            }
            
            if (!status) return;

            var statusArr = opts.forbidstatus;
            if (statusArr && statusArr.exist(status)) {
                $(this).edatagrid('disableEditing');
                return;
            }
            $(this).edatagrid('enableEditing');
        });
    },
    alterFieldStatus: function (jq, parmas) {
        return jq.each(function () {
            var field = parmas.field, forbidstatus = parmas.stateArr, status = parmas.state;
            if (!forbidstatus) return;
            for (var i = 0; i < forbidstatus.length; i++) {
                var c = forbidstatus[i];
                if (c == -1 || c == status) {
                    $(this).edatagrid('disableEditing', field);
                    return;
                }
            }
            $(this).edatagrid('enableEditing', field);
        })
    },
    enableEditing: function (jq, field) {
        return jq.each(function () {
            var opts = $(this).edatagrid('options');
            if (!field) {
                opts.allEditing = true;
            } else {
                var column = $(this).edatagrid('getColumnOption', field);
                if (column && column.editor2) {
                    column.editor = column.editor2;
                }
            }
        });
    },
    disableEditing: function (jq, field) {
        return jq.each(function () {
            var opts = $(this).edatagrid('options');
            if (field == undefined) {
                opts.allEditing = false;
            } else {
                var column = $(this).edatagrid('getColumnOption', field);
                if (column && column.editor) {
                    column.editor2 = column.editor;
                    column.editor = null;
                }
                if (opts.editIndex1 >= 0) {
                    $(jq).datagrid('endEdit', opts.editIndex1);
                }
            }
        });

    },
    editCell: function (jq, param) {
        return jq.each(function () {
            
            var opts = $(this).edatagrid('options');
            if (!opts.allEditing) {
                return;
            }
            var fields = $(this).datagrid('getColumnFields').concat($(this).datagrid('getColumnFields', true));
            for (var i = 0; i < fields.length; i++) {
                var col = $(this).datagrid('getColumnOption', fields[i]);
                if (!col.editor) continue;
                col.editor1 = col.editor;
                if (fields[i] != param.field) {
                    col.editor = null;
                }
            }

            $(this).datagrid('beginEdit', param.index);
            for (var i = 0; i < fields.length; i++) {
                var col = $(this).datagrid('getColumnOption', fields[i]);
                col.editor = col.editor1;
            }
             
            if(opts.editEditEvent){
               opts.editEditEvent.call(this,param);
            }
            var editor = $(this).datagrid('getEditor', { index: param.index, field: param.field });
            if (editor) {
                editor.target.focus();
                if (!opts.disableColMap) return;
                $(this).edatagrid('updateColumnEditorStatus', { type: editor.type, target: editor.target, index: param.index,
                    disableColMap: opts.disableColMap, field: param.field
                })
            }
           


        });
    },
    //修改联动所影响的列不能编辑 主 从关系
    //修改某列的编辑器的 是否disable状态 
    /*field status */
    updateColumnEditorStatus: function (jq, parms) {
        var type = parms.type, dom = parms.target; colMaps = parms.disableColMap, field = parms.field;
        var disable = false;
        var colMaps = colMaps[field]
        if (colMaps) {
            var rows = $(jq).datagrid('getRows');
            var rowCur;
            if (rows) {
                rowCur = rows[parms.index];
                if (!rowCur) return;
            }
            var temp = colMaps.length || 0;
            for (var i = 0; i < temp; i++) {
                var colvalue = rowCur[colMaps[i]];
                if (colvalue) {
                    disable = true;
                    break;
                }
                continue;
            }
        }
        switch (type) {
            case 'text':
            case 'numberbox':
                $(dom).attr('disabled', disable);
                break;
            case 'booleanbox':
            case 'combogrid':
                disable ? $(dom).combogrid('disable') : $(dom).combogrid('enable');
                break;
            default:

        }
    },
    addRow: function (jq, params) {
        return jq.each(function () {
            
            var dg = $(this);
            var opts = dg.edatagrid('options');
            var defaultRow = opts.defaultRow || { hideRow: {}, row: {} };
            var row = { isNewRecord: true }, tdValues = {}, rO;
            if (params) {
                var map = params.map, rData = params.treeData;
                for (var col in map) {
                    var valus = map[col];
                    if (valus.length == 1) {
                        row[col] = rData[valus[0]];
                    }
                    else if (valus.length == 2) {
                        row[col] = rData[valus[1]];
                        tdValues[col] = rData[valus[0]];
                    }
                }
            }

            var rows = $(jq).datagrid('getRows');
            if (!rows || !rows.length) {
                row = $.extend({}, defaultRow.row, row);
                dg.datagrid('appendRow', row);
                rO = defaultRow.hideRow;
            }
            else {
                var selRow = $(jq).datagrid('getSelected');
                var index = $(jq).datagrid('getRowIndex', selRow);
                if (index >= 0) {
                    $(jq).datagrid('endEdit', index);
                }
                var rowCopy = dg.edatagrid('copyPreRow', rows.length - 1); //根据配置复制上一行某些列的值


                var rowT = $.extend({}, defaultRow.row, rowCopy, row);
                dg.datagrid('appendRow', rowT);
                rO = $(this).edatagrid('copyPreRowValue', rows.length - 2);
            }
            var rows = $(jq).datagrid('getRows');
            tdValues = $.isEmptyObject(tdValues) ? {} : tdValues;
            var backvalues = $.extend({}, rO, tdValues);
            backvalues = $.extend({}, defaultRow.hideRow, backvalues);
            $(this).edatagrid('setRowValue', { value: backvalues, index: rows.length ? rows.length - 1 : 0 });
            if (rows && rows.length) {
                $(jq).datagrid('selectRow', rows.length - 1);
            }

        });
    },
    InsertRow: function (jq, params) {
        return jq.each(function () {
            
            var dg = $(this);
            var selRow = $(jq).datagrid('getSelected');
            var index = $(jq).datagrid('getRowIndex', selRow);
            if (!selRow) {
                dg.edatagrid('addRow');
            } else {
                
                if (index >= 0) {
                    dg.edatagrid('endEdit', index);
                }
                var rows = dg.edatagrid('getRows');
                var rowCopy = dg.edatagrid('copyPreRow', index); //根据配置复制上一行某些列的值
                var rO = dg.edatagrid('copyPreRowValue', index);
                dg.edatagrid('insertRow', { index: index, row: rowCopy });
                dg.edatagrid('setRowValue', { value: rO, index: index });
                dg.edatagrid('selectRow', index);
            }
        });
    },
    updateSelectedRow: function (jq, params) {
        return jq.each(function () {
            var row = $(this).datagrid('getSelected');
            var rowIndex = $(this).datagrid('getRowIndex', row);
            $(this).datagrid('endEdit', rowIndex);

            if (!(rowIndex >= 0)) return;
            var tdValues = {};
            if (params) {
                var map = params.map, rData = params.treeData;
                for (var col in map) {
                    var valus = map[col];
                    if (valus.length == 1) {
                        row[col] = rData[valus[0]];
                    }
                    else if (valus.length == 2) {
                        row[col] = rData[valus[1]];
                        tdValues[col] = rData[valus[0]];
                    }
                }
            }
            $(this).datagrid('updateRow', { index: rowIndex, row: row });
            $(this).edatagrid('setRowValue', { value: tdValues, index: rowIndex });
            $(this).datagrid('selectRow', rowIndex);
        })
    },
    /*
    赋值给行
    */
    setDataRow: function (jq, data) {
        return jq.each(function () {
            var ritem = {}, rhideItem = {};
            if (data) {
                var vals = $(jq).attr('id');
                vals = vals.split('-');
                if (vals && vals.length) {
                    var showAndHide1 = $(jq).edatagrid('getColumnMapping', true);
                    var showAndHide = $(jq).edatagrid('getColumnMapping', false);
                    var scope = vals[0], nattr;
                    if (scope) {
                        for (var i = 0; i < data.length; i++) {
                            var col = data[i];
                            nattr = scope + "-" + col.m + "-" + col.n;
                            if (showAndHide[col.n]) {
                                if (showAndHide1[nattr]) {
                                    ritem[nattr] = col.v;
                                }
                                rhideItem[showAndHide[col.n]] = col.v;
                            } else {
                                ritem[nattr] = col.v;
                            }

                        }
                    }
                }
            }
            var row = $(this).datagrid('getSelected');
            var rowIndex = $(this).datagrid('getRowIndex', row);
            $(this).datagrid('updateRow', { index: rowIndex, row: ritem });
            $(this).edatagrid('setRowValue', { value: rhideItem, index: rowIndex });
            $(this).datagrid('selectRow', rowIndex);
        })
    },
    saveRow: function (jq) {
        return jq.each(function () {
            var dg = $(this);
            var opts = $.data(this, 'edatagrid').options;
            if(opts.isEndNotEdit)return;/*不用结束编辑*/
            if (opts.onBeforeSave.call(this, opts.editIndex1) == false) {
                setTimeout(function () {
                    dg.datagrid('selectRow', opts.editIndex1);
                }, 0);
                return;
            }
            var selindex = opts.editIndex1;
            
            $(this).datagrid('endEdit', selindex);

            $(this).datagrid('selectRow', selindex);

        });
    },
    cancelRow: function (jq) {
        return jq.each(function () {
            var index = $(this).edatagrid('options').editIndex;
            $(this).datagrid('cancelEdit', index);
        });
    },
    delRow: function (jq) {
        return jq.each(function () {
            
            var dg = $(this);
            var opts = $.data(this, 'edatagrid').options;
            var row = dg.datagrid('getSelected');
            if (!row) {
                return;
            }
            var index = dg.datagrid('getRowIndex', row);
            if (row.isNewRecord) {
                dg.datagrid('endEdit', index);
                dg.datagrid('deleteRow', index);
            } else {
                dg.datagrid('endEdit', index);
                dg.datagrid('deleteRow', index);
            }
            dg.edatagrid('recordDelRow', row, opts);
            var rows = $(jq).datagrid('getRows');
            if (rows && rows.length) {
                if (index<=parseInt(rows.length-1)) {
                    $(jq).datagrid('selectRow', index);//将光标定位到当前行的下一行

                    opts.editIndex1 =index;
                }else {
                    $(jq).datagrid('selectRow', rows.length - 1);//将光标定位到最后一行

                    opts.editIndex1 = rows.length - 1;
                }
            } else {
                $(jq).edatagrid('addScroll');
            }
            //执行公式
            opts.onAfterDestroy.call(dg[0], index, row);
        });
    },
    recordDelRow: function (jq, row) {
        if (row) {
            var guid = row['GUID'];
            if (guid) {
                var gridid = $(jq).attr('id');
                var scope = $(jq).datagrid('options').scope;
                if (gridid) {
                    $("<input id=\"" + scope + "delrecordtemp\" type=\"hidden\" class=\"" + gridid + "\" value=\"" + guid + "\" /></input>").appendTo("body");
                }
            }
        }

    },
    getRecordedDelRow: function (jq) {
        var gridid = $(jq).attr('id');
        if (gridid) {
            var result = null;
            $('.' + gridid).each(function () {
                var guid = $(this).attr('value');
                if (guid) {
                    if (!result) {
                        result = [];
                    }
                    result.push(guid);
                }
            });
            return result;
        }
        return null;
    },
    getRealRow: function (jq, index) {
        var t = $(jq);
        var row = {};
        var cols = t.datagrid("getColumnFields", true).concat(t.datagrid("getColumnFields", false));
        var rowDom = $(t.parent().find('.datagrid-view2 tbody tr')[index + 1]);
        if (!rowDom) return row;
        for (var i = 1; i < cols.length; i++) {
            var colDom = rowDom.find('td:eq(' + i + ') div');
            if (colDom) {
                var valueHide = colDom.attr("oaovalue");
                row[cols[i]] = valueHide ? valueHide : colDom.html();
            }
        }
        return row;
    },
    //前台页面列表值转换成后台值
    transGridDataToBackstage: function (jq, rows) {
        
        var data = rows || $(jq).datagrid('getRows');
        var result = [], ritem;
        var showAndHide = $(jq).edatagrid('getColumnMapping', true);

        if (data) {
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                ritem = [];
                for (var attr in item) {
                    var attrAttr = attr.split('-'), val = "";
                    if (attrAttr.length < 2) continue;
                    if (showAndHide[attr]) {
                        val = $(jq).edatagrid('getCellValue', { field: attr, rowindex: i });
                    }
                    ritem.push({ n: attrAttr[2], v: val ? val : item[attr], m: attrAttr[1] });
                }
                result.push(ritem);
            }
        }
        return result;
    },
    //获得格式数据
    getData: function (jq) {
        
        var vals = $(jq).attr('id').split('-');
        if (vals && vals.length > 1) {
            var result = { m: vals[1] };
            result.r = $(jq).edatagrid('transGridDataToBackstage'); //(vals[0],vals[1],ars.rows);
            var ads = $(jq).edatagrid('getRecordedDelRow');
            if (ads) {
                result.ad = ads;
            }
            return result;
        }
        return null;
    },
    //设置默认值
    transGridDataToBackstageDetail: function (jq, rows) {//gai

        var data = rows || $(jq).datagrid('getRows');
        var result = [], ritem;
        var showAndHide = $(jq).edatagrid('getColumnMapping', true);

        if (data) {
            for (var i = 0, j = data.length; i < j; i++) {
                var item = data[i];
                ritem = [];
                for (var attr in item) {
                    var attrAttr = attr.split('-'), val = "";
                    if (attrAttr.length < 2) continue;
                    if (showAndHide[attr]) {
                        val = $(jq).edatagrid('getCellValue', { field: attr, rowindex: i });
                        ritem.push({ n: attrAttr[2], v: val, m: attrAttr[1] });
                        ritem.push({ n: showAndHide[attr], v: item[attr], m: attrAttr[1] });
                    } else {
                        ritem.push({ n: attrAttr[2], v: item[attr], m: attrAttr[1] });
                    }
                }
                result.push(ritem);
            }
        }
        return result;
    },
    //获得格式数据
    getDataDetail: function (jq) {//gai
        var curid=$(jq).attr('id');
        if (curid==undefined) return;
        var vals = curid.split('-');
        if (vals && vals.length > 1) {
            var result = { m: vals[1] };
            result.r = $(jq).edatagrid('transGridDataToBackstageDetail'); //(vals[0],vals[1],ars.rows);

            var ads = $(jq).edatagrid('getRecordedDelRow');
            if (ads) {
                result.ad = ads;
            } else {
                result.ad = [];
            }
            return result;
        }
        return null;
    },
    setDefaultRow: function (jq, data) {
        return jq.each(function () {
            var opts = $(this).datagrid('options');
            var defaultRow = { hideRow: {}, row: {} };
            opts.defaultData = data;
            if (data) {
                var vals = $(this).attr('id');
                vals = vals.split('-');
                if (vals && vals.length) {
                    if (!opts.defaultRow) {
                        //默认值


                        var result = $(this).edatagrid('transGridDataFromBackstage', { s: vals[0], m: vals[1], d: data.r, f: defaultRow });
                        if (result) {
                            if (result.rows.length == 1) {
                                defaultRow.row = result.rows[0];
                            }
                            if (result.hideRows.length == 1) {
                                defaultRow.hideRow = result.hideRows[0];
                            }
                        }
                        opts.defaultRow = defaultRow;
                    }
                }
            } else {
                opts.defaultRow = defalutRow;
            }
        })
    },
    //加载格式数据
    setData: function (jq, data) {
        return jq.each(function () {
            var opts = $(this).datagrid('options');
            if (data) {
                var vals = $(this).attr('id');
                vals = vals.split('-');
                if (vals && vals.length) {
                    var r1 = $(this).edatagrid('transGridDataFromBackstage', { s: vals[0], m: vals[1], d: data.r, f: opts.defaultRow });
                    opts.hideRows = r1.hideRows;
                    
                    $(this).datagrid('loadData', r1.rows);
                    if (data.ad) {
                        for (var i = 0; i < data.ad.length; i++) {
                            $(this).edatagrid('recordDelRow', { GUID: data.ad[i] });
                        }
                    }
                }
            }
            else {
                if (opts.isNotClearData) return;
                $(this).datagrid('loadData', []);
            }
        })
    },
    //跟隐藏的在td.div的数据 找到 具体列的对应关系
    getColumnMapping: function (jq, bVToText) {

        var showAndHide = {};
        var cols = $(jq).datagrid('getColumnFields');
        for (var i = 0, j = cols.length; i < j; i++) {
            var col = $(jq).datagrid('getColumnOption', cols[i])
            var editor1 = col.editor || col.editor2;
            if (!col.hidden && editor1) {
                var options = editor1.options;
                if (options && options.textField) {
                    if (bVToText) {
                        showAndHide[cols[i]] = options.textField;
                    } else {
                        showAndHide[options.textField] = cols[i];
                    }
                }
            }
        }
        return showAndHide;
    },
    //后台列表值转换成页面需要绑定的值
    transGridDataFromBackstage: function (jq, params) {//gai
        
        var showAndHide = $(jq).edatagrid('getColumnMapping', true);
        var showAndHide1 = $(jq).edatagrid('getColumnMapping', false);
        var cols = $(jq).datagrid("getColumnFields", true).concat($(jq).datagrid("getColumnFields", false));
        var scope = params.s, model = params.m, backstageData = params.d, defalut = params.f;
        var result = { rows: [], hideRows: [] }, ritem, rhideItem, nattr;

        if (scope && backstageData) {
            for (var i = 0; i < backstageData.length; i++) {
                var item = backstageData[i];
                if (item) {
                    ritem = {}, rhideItem = {};
                    for (var j = 0; j < item.length; j++) {
                        var col = item[j];
                        nattr = scope + "-" + col.m + "-" + col.n;

                        if (showAndHide[nattr]) {
                            var sattr = showAndHide[nattr];
                            if (sattr == col.n) {
                                ritem[nattr] = col.v;
                            }
                            rhideItem[nattr] = col.v;
                        }
                        else if (showAndHide1[col.n]) {
                            var c = showAndHide1[col.n].split('-');
                            if (c[1] == col.m) {
                                ritem[showAndHide1[col.n]] = col.v;
                            }
                        }
                        else {
                            if (cols.indexOf(nattr) < 0) continue;
                            ritem[nattr] = col.v;
                        }
                    }
                }
                if (defalut && defalut.row) {
                    ritem = $.extend({}, defalut.row, ritem);
                }
                if (defalut && defalut.hideRow) {
                    rhideItem = $.extend({}, defalut.hideRow, rhideItem);
                }
                result.rows.push(ritem);
               
                result.hideRows.push(rhideItem);

            }
        }
         
        return result;
    },
    //根据copyField配置复制前置行数据 显示值
    copyPreRow: function (jq, index) {
        var row = {};
        var opts = $(jq).datagrid('options');
        if (opts.copyField) {
            var rows = $(jq).datagrid('getRows');
            if (rows.length < 1) return row;
            if (index == 0) {
                index = index;
            } else {
                index = index ? index : rows.length - 1;
            }
            var copyRow = rows[index];
            if (!copyRow) return row;
            for (var i = 0, j = opts.copyField.length; i < j; i++) {
                var field = opts.copyField[i];
                row[field] = copyRow[field];
            }
        }
        return row;
    },
    //根据copyField配置复制前置行数据 隐藏值
    copyPreRowValue: function (jq, index) {//gai
        var row = {};
        
        var opts = $(jq).datagrid('options');
        if (opts.copyField) {
            for (var i = 0, j = opts.copyField.length; i < j; i++) {
                var field = opts.copyField[i];
                var val = $(jq).edatagrid('getCellValue', { field: field, rowindex: index });
                row[field] = val
            }
        }
        return row;
    },
    getRowValue: function (jq, index) {

        var row = {};
        if (!(index >= 0)) return row;
        var colFileds = $(jq).datagrid('getColumnFields');
        var colLength = colFileds.length;
        for (var i = 0; i < colLength; i++) {
            var col = colFileds[i];
            var val = $(jq).edatagrid('getCellValue', { field: col, rowindex: index });
            if (val) {
                row[col] = val;
            }
        }
        return row;
    },
    setRowValue: function (jq, pars) {

        if (pars && pars.value && pars.index >= 0) {
            var values = pars.value;
            var index = pars.index;
            for (var field in values) {
                var fieldvalue = values[field];
                if (fieldvalue) {
                    $(jq).edatagrid('setCellValue', { field: field, rowindex: index, value: fieldvalue });
                }
            }
        }
    },
    setCellValue: function (jq, pars) {
        if (pars && pars.rowindex >= 0 && pars.field) {
            var opts = $(jq).edatagrid('options');
            opts.finder.getTr($(jq)[0], pars.rowindex).find("td[field='" + pars.field + "']").find("div").attr("oaovalue", pars.value);
        }
    },
    getCellValue: function (jq, pars) {//gai
        
        if (pars && pars.rowindex >= 0 && pars.field) {
            var opts = $(jq).edatagrid('options');
            return opts.finder.getTr($(jq)[0], pars.rowindex) ? opts.finder.getTr($(jq)[0], pars.rowindex).find("td[field='" + pars.field + "']").find("div").attr("oaovalue") : "";
        }
        return "";
    },
    getFormula: function (jq, formulaName) {
        
        var opts = $(jq).edatagrid('options');
        var result =[];
        if (opts.fieldformula) {
            for (var i = 0; i < opts.fieldformula.length; i++) {
                var formula = opts.fieldformula[i];
                if (formula && formula.name == formulaName) {
                    result.push(formula);
                }
            }
            return result;
        }
        return null;
    },
    concatField: function (jq, field, seperator) {
        if (!field) {
            return null;
        }
        if (!seperator) {
            seperator = '、';
        }
        var rows = $(jq).datagrid('getRows');
        var value =[];
        value.seperator = seperator;
        for (var i = 0; i < rows.length; i++) {
            var dv = rows[i];
            var vv = dv[field];
            if (vv && vv != "" && !value.exist(vv)) {

                value.push(vv);
            }
        }
        return value.join(value.seperator);
    },
    getStatus: function (jq) {
        var opts = $(jq).edatagrid('options');
        return $.view.getStatus(opts.scope);
    },
    //从明细表中加载收款单位信息到grid列表中
    LoadSkdwInfosToGrid: function (jq) {
        var opts = $(jq).edatagrid('options');
        if (opts.single) {
            var rows = $(jq).edatagrid('getRows');
            if (rows.length == 0) return;
            //获取收款单位的GUID
            var GUID_Customer = rows[0]['hkspd-BX_Detail-GUID_Cutomer'];
            //将获取到得收款单位的GUID赋值给combogrid控件
            var $targetControl = $('#hkspd-BX_Detail-GUID_Cutomer');
            if (!$targetControl) return;
            var optsCombo = $targetControl.combogrid('options');
            optsCombo.tempValue = GUID_Customer;
            //获取combogrid的所有行的数据
            var comRows = $targetControl.combogrid('grid').datagrid('getRows');
            for (var i = 0, j = comRows.length; i < j; i++) {
                if (comRows[i]['GUID'] == GUID_Customer) {
                    //选中刚赋值的combogrid
                    $targetControl.combogrid('grid').datagrid('selectRow', i);
                    $.fn.combogrid.methods['setAssociate'].call($targetControl);
                    break;
                }
            }

        }
    },
    //根据行的间隔合并
    /*
        params：{
            colName  合并列名,
            rowSpace 行间隔 
        }
    */
    mergeColCells:function(jq,params){
        
        if(!params.rowSpace)return;
        var target=$(jq);
        var rows=target.datagrid('getRows');
        var flag=false;
        var startIndex=0;
        var entIndex=0;
        for (var i = 0,j=rows.length; i < j; i=i+params.rowSpace) {
              target.datagrid('mergeCells',{
                    index:i,
                    field:params.colName,
                    rowspan:params.rowSpace,
                    colspan:null
              })
        }
    },
    
    //根据行的间隔合并通过内容
    mergeColCellsByContext:function(jq,params){
        
        var target=$(jq);
        var rows=target.datagrid('getRows');
        var flag=false;
        var compareText="";
        var startIndex=0,endIndex=0;
        for (var i = 0,j=rows.length; i < j; i++) {
             
              var row=rows[i],v=row[params.colName];
              if(v==compareText){
                flag=false;
              }else{
                flag=true;
                compareText=v;
                entIndex=startIndex;
                startIndex=i
              }
              if(flag&&((i-entIndex)||0>0)){
                target.datagrid('mergeCells',{
                    index:entIndex,
                    field:params.colName,
                    rowspan:i-entIndex,
                    colspan:null
                })
              };
        }
    }
};

$.fn.edatagrid.defaults = $.extend({}, $.fn.datagrid.defaults, {
    editing: true,
    allEditing:true,
    editIndex: -1,
    destroyMsg: {
        norecord: {
            title: 'Warning',
            msg: 'No record is selected.'
        },
        confirm: {
            title: 'Confirm',
            msg: 'Are you sure you want to delete?'
        }
    },
    destroyConfirmTitle: 'Confirm',
    destroyConfirmMsg: 'Are you sure you want to delete?',
    recordDelGridId: 'scope_delgrid',
    url: null, // return the datagrid data
    saveUrl: null, // return the added row
    updateUrl: null, // return the updated row
    destroyUrl: null, // return {success:true}

    tree: null, 	// the tree selector
    treeUrl: null, // return tree data
    treeDndUrl: null, // to process the drag and drop operation, return {success:true}
    treeTextField: 'name',
    treeParentField: 'parentId',

    scope: null,
    forbidstatus: null, //{'field1':[],'field2':[]}
    fieldformula: null, //[{name:'',field:'',target:['','']},{name:'',field:'',target:['','']}]
    bindmethod: null, // { 'dblclick': ['setAssociate'] }
    copyField: null, //['field1','field2']

    onAdd: function (index, row) { },
    onBeforeSave: function (index) { },
    onSave: function (index, row) { },
    onDestroy: function (index, row) { },
    onLoadSuccess:function (data) {
        if (!data || !data.rows || data.rows.length == 0) {
            return;
        }
        $(this).edatagrid('doFormula');
        $(this).edatagrid('LoadSkdwInfosToGrid');
    },
    onAfterDestroy: function (index, row) { }
});

})(jQuery);