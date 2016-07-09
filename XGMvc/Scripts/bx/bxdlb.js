$.extend($.fn.linkbutton.methods, {
    submitfilter: function () {

        var treeIds = ['filter-tree-filterper', 'filter-tree-filtercode', 'filter-tree-filterproject', 'filter-tree-filterfunction', 'filter-tree-filterdoctype'];
        var opts = $(this).linkbutton('options');

    },
    filterSelect: function () {
        var nodes = $(this).tree('getChecked');
        var val = [];
        for (var i = 0, j = nodes.length; i < j; i++) {
            
        }

    }

})