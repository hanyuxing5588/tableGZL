/**

* Description 分页插件
* Powered By eirc
* QQ 1299567348
* E-mail suxiaohua1020@163.com
* Data 2013-12-02
* Dependence jQuery v1.7.2
 
**/


(function ($) {

    $.fn.selfPages = function (options) {
        var opts = $.extend({}, $.fn.selfPages.defaults, options);

        return this.each(function () {
            
            var $this = $(this);
            var $pager = $('#pager');
            var $PagesClass = opts.PagesClass; // 分页元素
            var $AllMth = $this.find($PagesClass).length;  //总个数
            var $Mth = opts.PagesMth; //每页显示个数
            var $NavMth = opts.PagesNavMth; // 导航显示个数

            
            // 定义分页导航
            var PagesNavHtml = "<a href=\"javascript:;\" class=\"homePage\"> << </a><a href=\"javascript:;\" class=\"PagePrev\"> < </a><span class=\"Ellipsis\"><b>...</b></span><div class=\"pagesnum\"></div><span class=\"Ellipsis\"><b>...</b></span><a href=\"javascript:;\" class=\"PageNext\"> > </a><a href=\"javascript:;\" class=\"lastPage\"> >> </a><cite class=\"FormNum\">第<input type=\"text\" name=\"PageNum\" id=\"PageNum\">页</cite><a href=\"javascript:;\" class=\"PageNumOK\">确定</a>";
            /*默认初始化显示*/

            if ($AllMth > $Mth) {
                //判断显示
                var relMth = $Mth - 1;
                $this.find($PagesClass).filter(":gt(" + relMth + ")").hide();

                // 计算数量,页数
                var PagesMth = Math.ceil($AllMth / $Mth); // 计算页数
                var PagesMthTxt = "<span>共<b>" + $AllMth + "</b>条，共<b>" + PagesMth + "</b>页</span>";
                
                $pager.append(PagesNavHtml).append(PagesMthTxt);

                // 计算分页导航显示数量

                var PagesNavNum = "";
                for (var i = 1; i <= PagesMth; i++) {

                    PagesNavNum = PagesNavNum + "<a href=\"javascript:;\">" + i + "</a>";

                };
                $pager.find(".pagesnum").append(PagesNavNum).find("a:first").addClass("PageCur");


                //判断是否显示省略号
                if ($NavMth < PagesMth) {

                    $pager.find("span.Ellipsis:last").show();
                    var relNavMth = $NavMth - 1;
                    $pager.find(".pagesnum a").filter(":gt(" + relNavMth + ")").hide();

                } else {

                    $pager.find("span.Ellipsis:last").hide();
                };


                /*默认显示已完成,下面是控制区域代码*/


                //跳转页面
                var $input = $pager.find("#PageNum");
                var $submit = $pager.find(".PageNumOK");
                //跳转页面文本框
                $input.keyup(function () {

                    var pattern_d = /^\d+$/; //全数字正则

                    if (!pattern_d.exec($input.val())) {

                        alert("提示：请填写正确的数字！");
                        $input.focus().val("");
                        return false

                    };

                });

                //跳转页面确定按钮
                $submit.click(function () {
                    

                    if ($input.val() == "") {

                        alert("提示：请填写您要跳转到第几页！");
                        $input.focus().val("");
                        return false

                    } if ($input.val() > PagesMth) {

                        alert("提示：您跳转的页面不存在！");
                        $input.focus().val("");
                        return false

                    } else {

                        showPages($input.val());

                    };

                });


                //导航控制分页
                var $PagesNav = $pager.find(".pagesnum a"); //导航指向
                var $PagesFrist = $pager.find(".homePage"); //首页
                var $PagesLast = $pager.find(".lastPage"); //尾页
                var $PagesPrev = $pager.find(".PagePrev"); //上一页
                var $PagesNext = $pager.find(".PageNext"); //下一页

                //导航指向
                $PagesNav.click(function () {
                    
                    var NavTxt = $(this).text();
                    
                    showPages(NavTxt);

                });

                //首页
                $PagesFrist.click(function () {
                    
                    showPages(1);

                });

                //尾页
                $PagesLast.click(function () {
                    
                    showPages(PagesMth);

                });

                //上一页
                $PagesPrev.click(function () {
                    
                    var OldNav = $pager.find(".pagesnum a[class=PageCur]");
                    if (OldNav.text() == 1) { alert("提示：已到首页！"); } else {

                        var NavTxt = parseInt(OldNav.text()) - 1;
                        showPages(NavTxt);

                    };

                });

                //下一页
                $PagesNext.click(function () {
                    
                    var OldNav = $pager.find(".pagesnum a[class=PageCur]");

                    if (OldNav.text() == PagesMth) { alert("提示：已到尾页！"); } else {

                        var NavTxt = parseInt(OldNav.text()) + 1;
                        showPages(NavTxt);

                    };

                });

                // 主体显示隐藏分页函数
                function showPages(page) {
                    
                    $PagesNav.each(function () {

                        var NavText = $(this).text();

                        if (NavText == page) {
                            
                            $(this).addClass("PageCur").siblings().removeClass("PageCur");

                        };
                    });

                    //显示导航样式
                    var AllMth = PagesMth / $NavMth;
                    for (var i = 1; i <= AllMth; i++) {

                        if (page > (i * $NavMth)) {

                            $PagesNav.filter(":gt(" + (i * $NavMth - 1) + ")").show();
                            $PagesNav.filter(":gt(" + (i * $NavMth - 1 + $NavMth) + ")").hide();
                            $PagesNav.filter(":lt(" + (i * $NavMth) + ")").hide();

                            $pager.find("span.Ellipsis:first").show();

                        };

                        if (page <= $NavMth) {

                            $PagesNav.filter(":gt(" + ($NavMth - 1) + ")").hide();
                            $PagesNav.filter(":lt(" + $NavMth + ")").show();

                            $pager.find("span.Ellipsis:first").hide();

                        };

                    };


                    // 显示内容区域
                    var LeftPage = $Mth * (page - 1);
                    var NowPage = $Mth * page;

                    $this.find($PagesClass).hide();
                    $this.find($PagesClass).filter(":lt(" + (NowPage) + ")").show();
                    $this.find($PagesClass).filter(":lt(" + (LeftPage) + ")").hide();

                };


            }; //判断结束			

        }); //主体代码
    };

    // 默认参数
    $.fn.selfPages.defaults = {
        PagesClass: '.item', //需要分页的元素
        PagesMth: 4, //每页显示个数		
        PagesNavMth: 5 //显示导航个数
    };

    $.fn.selfPages.setDefaults = function (settings) {
        $.extend($.fn.selfPages.defaults, settings);
    };

})(jQuery);