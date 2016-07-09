
//用户登录
$(function () {
    $("#btnLogon").click(function () {
        
        if (loginInfo()) {
            var postData = {};
            postData.Name = $("#username").val();
            postData.PassWord = $("#password").val();
            logSys(postData);       //调取用户登录方法
        }
    });

    //取消
    $("#btnCancle").click(function () {
        $('#formId')[0].reset();
        testForm.username.focus();
    });


    $(document).keydown(function (e) {
        var chrKey = e.which;
        if (chrKey == 13) {
            $("#btnLogon").click();
            return false;
        }
    });
});
//登陆验证
function loginInfo() {
    var name = document.testForm.username.value;
    var pass = document.testForm.password.value;
    if (name.length > 50 || name.length < 2) {
//        $.jGrowl("用户名长度必须大于等于2,小于等于50!");
//        testForm.username.focus();
//        return false;
    } else if (isSpace()) {
//        $.jGrowl("用户名只能包含字母,数字,下划线'_',以及'-'!");
//        testForm.username.focus();
//        return false;
    } else if (pass.length < 1) {
        $.jGrowl("请输入密码!!!");
        testForm.password.focus();
        return false;
    }
    return true;
};
//else if (isFirst()) {
//        $.jGrowl("用户名头字母不能为数字!");
//        testForm.username.focus();
//        return false;
//    } 
//是否空白
function isSpace() {
    var name = document.testForm.username.value.split("");
    for (i = 0; i < name.length; i++) {
        if (name[i] == " ") {
            return true;
        } else if (!((name[i] <="z" && name[i] >= "a") || (name[i] <= "Z" && name[i] >= "A")
        || name[i] == "-" || name[i] == "_" || (name[i] <= "9" && name[i] >= "0"))) {
            return true;
        }
    }
    return false;
};
//不能以数字开头
function isFirst() {
    var name = document.testForm.username.value.split("");
    if (name[0] < "9" && name[0] > "0") {
        return true;
    }
    return false;
};
//通过ajax用户登录传值，url跳转到后台
function logSys(postData) {
    $.ajax({
        url: '/Logon/LogonUserInfo',
        data: postData,
        cache: false,
        async: true,
        type: 'post',
        datatype: 'Text',
        success: function (data) {
            
            if (data == "ok") {
                window.location.href = "/Home/Index";
            } else {
                $.jGrowl(data);
                $('#formId')[0].reset();
                testForm.username.focus();
            }
        }
    });
}




