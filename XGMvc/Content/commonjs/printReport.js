



//报表打印
(function ($) {
    $.fn.reportInit = function (parms) {
        /****/
        //1、定义好的规则tableDiv  report-rules
        //2、attr gridid jquery
        //3、取grid数据源 遍历div集合 组合数据格式
        // var={data:报表数据源,headObject:{} }

        //得到id为report-rules的table
        //拿到grid的id和所有的行
        var gridId = '#' + parms[1][0], dataRows = $(gridId).datagrid('getRows');
        var tableId = '#' + parms[1][1];
        //取到table下的所有td 的div对象
        $div = $(tableId).find("tr td div"); //jquery 
        var rules = {};
        for (var i = 0; i < $div.length; i++) {
            var name = $($div[i]).attr("reportFiled");
            var value = $($div[i]).text();
            rules[name] = value;
        }
        //组织成一个对象集合将所有参数传入公共打印方法中去
        var obj = { data: dataRows, headObject: rules };
        //动态匹配打印模板
        var url = parms[0] + "?pturl=" + scope + "print";
        $("#printIframe").reportPrint(url, obj);
    }
    /*
    1、先找到要打印的区域，也就是句柄(模板)，这个模板就是我们要呈现的页面
    2、根据id找到要打印的内容(区域),DIV下面所有的东西，放在设置的表格中
    3、获取表格信息，设置的格式要跟模板是一样的，之后将找到的打印内容放到
    */
    $.fn.reportPrint = function (url, obj) {
        if (url) {
            var $element = (this instanceof jQuery) ? this : $(this);
            if ($element[0].tagName.toLowerCase() != "iframe") return;
            var iframe = $element[0];
            if (iframe.attachEvent) {
                iframe.attachEvent("onload", function () {
//                    console.log("ie");
                });
            } else {
                //获取区域,如果不为空，就去找options的元素
                printer = $(iframe.contentDocument.body).find("div #print");
                if (printer.length <= 0) return;
                //拿到div中options的所有元素
                ptintInfo = $(printer).attr("options");

                //把打印的内容放在区域
                contenter = $(printer).find("#content"); //打印的内容
                //先写获取源数据的方法,将区域中打印的内容放进去

                //取headObject
                $(contenter).ecah(function () {
                    var tableInfo = {};
                    //headObject
                    if (obj.headObject) {
                        var head = obj.headObject; //{name:v,name:v...}
                        for (var attr in head) {
                           
                            var element = $('#' + attr);
                            if (!element) return;
                                element.setText(head[attr]);
                            }
                        tableInfo = table;
                    };
                    //Data
                    if (obj.data) {
                        //得到id
                        var rows = $(obj.data);
                        for (var i = 0; rows.length; i++) {

                        }

                    }
                });

                var table = {};
                $(contenter).find("table").each(function () {
                    var id = $(this).attr("id");
                    if (id) {
                        var columns = null;
                        var index = 1;
                        var serialumns = null;
                        $(this).find("tr td div").each(function () {

                        })
                    }
                })

            }
        }
    }

})(jQuery);



