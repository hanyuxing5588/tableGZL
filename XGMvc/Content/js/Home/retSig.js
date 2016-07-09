/**
* Define Global Variable
**/
var json;
//返回信号值为真
var retSingleTrue = function () {
    json = [
//        { MenuName: "会计凭证", Menukey: "M010501" }, { MenuName: "核销处理", Menukey: "M010401" }, { MenuName: "报销单列表", Menukey: "M010301" },
        { MenuName: "单据列表", Menukey: "M010601" }, { MenuName: "应付单填制", Menukey: "M010502" }, { MenuName: "现金报销单", Menukey: "M010302" }
//        { MenuName: "支票领取", Menukey: "M010402" }, { MenuName: "支票申领单", Menukey: "M010303" }, { MenuName: "凭证科目规则", Menukey: "M010403" },
//        { MenuName: "其他报销单", Menukey: "M010304" }, { MenuName: "公务卡汇总报销单", Menukey: "M010305" }, { MenuName: "项目档案", Menukey: "M010901" },
//        { MenuName: "公务卡报销单", Menukey: "M010306" }, { MenuName: "收款凭单", Menukey: "M010307" }, { MenuName: "收入凭单", Menukey: "M010308" }
    ];

    $.each(json, function (index, item) {
       
       var i = index + 1, title = item.MenuName, key = item.Menukey;
       var strTemp = [
        $.format('<s class="info{0}">', key),
        '</s>'
        ].join('');
       $(strTemp).appendTo("em a #M010302");
    });
    $('.infoM010302').show();
}
//返回信号值为假
var retSingleFalse = function () {
        $('.infoM010302').hide();
}