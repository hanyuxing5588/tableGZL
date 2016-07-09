/*关于流程意见框的屏蔽*/
//$.extend($.fn.linkbutton.methods, {
//    submitProcess: function () {
//        
//        var opts = $(this).linkbutton('options');
//        opts.window = 'b-window';
//        if ($.view.judgePageCancleState(opts.docState)) {
//            $.messager.alert('提示', '该单据已经作废,操作无效')
//            return;
//        }
//        var url = $.format('/{0}/CommitFlow', opts.scope);
//        $.fn.linkbutton.methods["suggestionWin"].call(this, { url: url, scope: opts.scope }, opts.window);
//    },
//    sendBack: function () {
//        
//        var opts = $(this).linkbutton('options');
//        opts.window = 'b-window';
//        if ($.view.judgePageCancleState(opts.docState)) {
//            $.messager.alert('提示', '该单据已经作废,操作无效')
//            return;
//        }
//        var url = $.format('/{0}/SendBackFlow', opts.scope);
//        $.fn.linkbutton.methods["suggestionWin"].call(this, { url: url, scope: opts.scope }, opts.window);
//    },
//    suggestionWin: function (parms, winId) {
//        $('#' + winId).dialog({
//            isCancel: true,
//            resizable: false,
//            title: '建议窗口',
//            width: 600,
//            height: 400,
//            modal: true,
//            draggable: true,
//            resizable: true,
//            minimizable: false,
//            maximizable: false,
//            collapsible: false,
//            sumbitScope: parms.scope,
//            sumbitUrl: parms.url,
//            href: '/Process/FlowSuggest',
//            onLoad: function (c) {
//                $.view.setViewEditStatus('sug', '1');
//            }
//        });
//    },
//    processAciton: function (url, scope, winId) {
//        var guid = $.view.getKeyGuid(scope);
//        var suggest = $('#suggest').val();
//        if (guid) {
//            $.ajax({
//                url: url,
//                data: { "guid": guid, "scope": scope, "suggest": suggest },
//                dataType: "json",
//                type: "POST",
//                traditional: true,
//                error: function (xmlhttprequest, textStatus, errorThrown) {
//                    $.messager.alert("错误", $.view.warning, 'error');
//                },
//                success: function (response) {
//                    var data = eval(response);
//                    $(winId).dialog('close');
//                    $.messager.alert(data.Title, data.Msg, data.Icon);
//                }
//            });
//        }
//    },
//    suggestCancel: function () {
//        var opts = $(this).linkbutton('options');
//        var win = '#' + opts.window;
//        $(win).dialog('close');
//    },
//    suggestOk: function () {
//        var opts = $(this).linkbutton('options');
//        var suggest = $('#suggest').val();
//        var winId = '#' + opts.window;
//        var optsWin = $(winId).dialog('options');
//        $.fn.linkbutton.methods["processAciton"].call(this, optsWin.sumbitUrl, optsWin.sumbitScope, winId);
//    }
//});