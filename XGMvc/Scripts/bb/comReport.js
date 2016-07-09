
$.extend($.fn.datagrid.defaults, {
    onBeforeLoad: function () {
        var opts = $(this).datagrid('options');
        if (opts.formatters) {
            for (var field in opts.formatters) {
                var colConfig = $(this).datagrid('getColumnOption', field);
                colConfig.formatter = $.view.formatters[opts.formatters[field]];

            }
        }
    }
});

$.view = {
    formatters: {
        numberbox: function (value1, row, index) {
            if (!value1) return '0.00';
            if ($.isNumeric(value1) && value1 == 0) {
                value1 = '';
            }
            return $.isNumeric(value1) ? new Number(value1).formatThousand(value1) : '0.00'
        }
    }
};

$(document).ready(function () {
    Number.prototype.formatThousand = function (s1) {
        s1 = s1.toString().replace(/,/g, '');
        s1 = new Number(s1).toFixed(2);
        var p = /(\d+)(\d{3})/;
        while (p.test(s1)) {
            s1 = s1.replace(p, "$1,$2");
        }
        return s1;
    };
 });