


$.extend($.fn.linkbutton.methods, {
//验证Combogrid字段值



});

//验证validatebox
$.extend($.fn.validatebox.defaults.rules, {
    idcard: {
        validator: function (value, param) {
            return idCard(value);
        },
        message: '请输入正确的身份证号码'
    },
    number: {
        validator: function (value, param) {
            return /^\d+$/.test(value);
        },
        message: '请输入数字'
    },
    integer: {
        validator: function (value, param) {
            return /^[0-9]*[1-9][0-9]*$/.test(value);
        },
        message: '请输入正整数'
    },
    bankNumber: {//验证银行账号 银行账号存在16位和19位
        validator: function (value) {

            return /^\d{16}|\d{19}$/.test(value);
        },
        message: '银行账号只能是数字!'
    },

});



