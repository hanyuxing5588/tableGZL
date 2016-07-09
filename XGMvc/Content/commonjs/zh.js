$(document).ready(function () {
    loadCombobox();
});


var loadCombobox = function () {
    
    //初始化Combobox
    $('#comboM').combobox({
        required: false,
        editable: false,
        panelHeight: 80,
        width: 80,
        value: '万元',
        valueField: 'value',
        textField: 'label',
        data: [{
            label: '万元',
            value: '1'
        }, {
            label: '千元',
            value: '2'
        }, {
            label: '元',
            value: '3'
        }],
        onSelect: function (record) {
            //获取所有行记录
            var currRows = $('#xmjdylb_grid').datagrid('getRows');
            $.each(currRows, function (index, fitem) {
                //获取行索引                var rowNum = index;
                //字段值：T7:安排合计，T8:可用额度，T9:经费使用合计，T10:本年借款，T11:以前年借款，T12::借款合计，T13:本年支出，T14:以前年支出，T15:支出合计，T16:执行率，T17:支出率，T18:备注，T19:2006年安排，T20:2007年安排，T21:2008年安排，T22:2009年安排，T23:2010年安排，T24:2011年安排，T25:2012年安排，T26:2013年安排
                //获取所有列的集合对象                var Arr = new Array();
                Arr = [fitem.T7, fitem.T8, fitem.T9, fitem.T10, fitem.T11, fitem.T12, fitem.T13, fitem.T14, fitem.T15, fitem.T16, fitem.T17, fitem.T18, fitem.T19, fitem.T20, fitem.T21, fitem.T22, fitem.T23, fitem.T24, fitem.T25, fitem.T26];
                //所有列值                Arr1 = ["T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "T16", "T17", "T18", "T19", "T20", "T21", "T22", "T23", "T24", "T25", "T26"];

                var temp = "";
                var single = record.value;
                $.each(Arr, function (index, value) {
                    switch (single) {
                        case "1":
                            temp = new Number(value).toFixed(2);
                            //设置保留两位小数
                            //$(fitem).html(temp.toFixed(2));
                            break;
                        case "2":
                            temp = Number(value * 1000).toFixed(2);
                            break;
                        case "3":
                            temp = Number(value * 10000).toFixed(2);
                            break;
                    }

                    //动态行索引和列索引，赋值                    $('#datagrid-row-r1-2-' + rowNum + ' div.datagrid-cell.datagrid-cell-c1-' + Arr1[index]).text(formatNumber(temp));
                });
            });
        }
    });
}


//三位符
var formatNumber = function (val) {

    if (val.length <= 0 || val == "") { return ""; }
    if (true) {
        if (!/^(\+|-)?(\d+)(\.\d+)?$/.test(val)) {
            return val;
        }
        var a = RegExp.$1, b = RegExp.$2, c = RegExp.$3;
        var re = new RegExp();
        re.compile("(\\d)(\\d{3})(,|$)");
        while (re.test(b)) b = b.replace(re, "$1,$2$3");
        val = a + "" + b + "" + c;
    } else {
        val = val.replace(/,/g, '')
    }
    return val;
}