//getAgencyTask function
var getAgencyTask = function () {
    $.ajax({
        url: '/Home/GetAcencyTaskData',
        cache: false,
        async: true,
        type: 'post',
        dataType: 'json',
        success: function (data) {
            var $obj = $('.newslist ul li');
            if ($obj.length > 0) {
                $obj.remove();
            }
            $.each(data, function (index, item) {
                var strContent = "", i = index;
                var betl = $.format('<li id="task{0}" style="width:100%;height:16px !important;">', i);
                var strLi = [
                    betl,
                       $.format('<div style="width:100px;height:16px !important;"><a href="#">{0}</a></div>', item.NodeLevel),
                       $.format('<div style="width:40%;height:16px !important;"><a href="#">{0}</a></div>', item.NodeName),
                       $.format('<div style="width:130px;height:16px !important;"><a href="#">{0}</a></div>', item.StrAcceptDate),
                       $.format('<div style="width:150px;height:16px !important;"><a href="#">{0}</a></div>', item.doctypename),
                       $.format('<div style="width:1px;height:16px !important;display:none"><a href="#">{0}</a></div>', item.scope),
                       $.format('<div style="width:1px;height:16px !important;display:none"><a href="#">{0}</a></div>', item.guid),
                    '</li>'
                ].join('');

                $(strLi).appendTo('.newslist ul');
                //Binds a function to the click event of each matched element
                $($.format("#task{0}", i)).bind('click', function (i) {
                    return function () {
                        var itemTemp={
                            processID:item.ProcessID,
                            processNodeID:item.ProcessNodeID,
                            nodeLevel: item.NodeLevel,
                            nodeType: item.NodeType
                        };
                        OpenTask(itemTemp);
                    }
                } (index));
            });
            $('.newslist').selfPages({
                PagesClass: 'li', //需要分页的元素
                PagesMth: 8, //每页显示个数		
                PagesNavMth: 6 //显示导航个数
            });
        }
    });
}
var getAgencyTask1 = function () {
    $.ajax({
        url: '/Home/GetAcencyTaskData',
        cache: false,
        async: true,
        type: 'post',
        dataType: 'json',
        success: function (data) {
            
            $('#todoTask').datagrid('loadData', data);
        }
    });
}
var OpenTask = function (itemParam) {
    $.ajax({
        url: '/Home/GetViewParameters',
        data: itemParam,
        cache: false,
        async: true,
        type: 'post',
        datatype: 'json',
        success: function (data) {

            if (!data||data.Msg) {
                $.messager.alert('提示', '打开待办任务出错，请刷新列表');
            } else {
                openTabs(data.MenuName, data.Url, data.DataId, 4, '', data);
            }
        }
    });
}
var getGridData = function () {
    var $obj = $('.newslist ul li');
    var arrTemp = [], array = [], tempArray = [];
    for (var i = 0; i < $obj.length; i++) {
        arrTemp[i] = $($obj[i]).find('div a');
        var creater = $(arrTemp[i][0]).text();
        var docnum = $(arrTemp[i][1]).text();
        var money = $(arrTemp[i][2]).text();
        var type = $(arrTemp[i][3]).text();
        var guid = $(arrTemp[i][4]).text();
        array.push(creater);
        array.push(docnum);
        array.push(money);
        array.push(type);
        array.push(guid);
        tempArray[i] = array;
        array = [];
    }
    var a = tempArray.join('');
}
