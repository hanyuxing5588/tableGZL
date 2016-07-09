var fun = {
    djlbSearch: function () {

        var url = '/djlb/GLHistory';
        var gridId = 'djlb-list';
        var scope = 'djlb';
        var region = 'djlbdatafilter';
        this.getDjlbByFilter(url, gridId, scope, region);
    },
    getDjlbByFilter: function (url, gridId, scope, region, tcondition) {
        var data = $.view.retrieveData(scope, region);
        var url;
        var result = {};
        if (data && data.m) {
            for (var i = 0, j = data.m.length; i < j; i++) {
                var temp = data.m[i];
                if (!temp && !temp.v) continue;
                result[temp.n] = temp.v;
            }
        }
        result = $.extend(result, tcondition || {});
        var guid = $('#initguid123').val();
        if (url) {
            $.ajax({
                url: url,
                data: { condition: JSON.stringify(result) ,guid:guid},
                dataType: "json",
                type: "POST",
                traditional: true,
                error: function (xmlhttprequest, textStatus, errorThrown) {
                    MaskUtil.unmask();
                    $.messager.alert("错误", $.view.warning, 'error');
                },
                success: function (data) {
                    //单据列表当查询的时候默认将页码设置成第一页的数据  sxh 2014-03-20
                    $('#' + gridId).datagrid('getPager').pagination('options').pageNumber = 1;
                    $('#' + gridId).datagrid('loadData', data);
                }
            });
        }
        else {
        }
    }
}

