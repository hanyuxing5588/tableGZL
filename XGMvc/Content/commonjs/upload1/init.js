    function getObj(id) {
        return document.getElementById(id);
    }
    var dicZYLB={};
    function Init(){ 
        #foreach($dr in $ZYLB)
           var child$dr.dmcod=[];
           dicZYLB.child$dr.dmcod=child$dr.dmcod;
           #foreach($drr in $dr.listArtType)
                var tempValue='$!{drr.DmCpt}'+"@"+'$!{drr.Dmcod}'
                setArr(child$dr.dmcod,tempValue);
           #end
        #end
        function setArr(v,s){
            v.push(s);
        };
    }
    function getZYLB(n) {
        var e = getObj('zylb');
        for (var i = e.options.length; i > 0; i--) e.remove(i);
        if (n == 0) return;
        var a =dicZYLB['child'+n]; 
                    for (var i = 0; i < a.length; i++) {
        var tempOP=a[i].split('@');
        e.options.add(new Option(tempOP[0], tempOP[1]));
    }
    }
    function checkInput(v) {
     var ids= getAttIDS();
    /*知道点击那个按钮*/
    var e = getObj('attids');
    e.value=ids.join(',');
    var sType = getObj('saveType');
    if(sType==3){
        document.f.reset();
        return false;
    }
    return true;
     
}
    function setSaveType(v){
    var e = getObj('saveType');
    e.value=v;
}
    var swfu;
    window.onload = function () {
        var settings = {
            flash_url: "/modules/wlcmsapi/view/GRKJ/Page/Upload/swfupload.swf",
            flash9_url: "/modules/wlcmsapi/view/GRKJ/Page/Upload/swfupload_fp9.swf",
            upload_url: "/wlcmsapi/attachment/upload.do",
            post_params: { "PHPSESSID": "" },
            file_size_limit: "200 MB",
            file_types: "*.*",
            file_types_description: "All Files",
            file_upload_limit: 0,
            file_queue_limit: 0,
            custom_settings: {
                progressTarget: "fsUploadProgress",
                cancelButtonId: "btnCancel"
            },
            debug: false,
            // Button settings
            button_image_url: "/modules/cms/view/attachment/XPButtonNoText_160x22.png",
            button_width: "65",
            button_height: "29",
            button_placeholder_id: "spanButtonPlaceHolder",
            button_text: '<span class="theFont">上传</span>',
            button_text_style: ".theFont { font-size: 16px;background-color: #000000;}",
            button_text_left_padding: 12,
            button_text_top_padding: 3,

            // The event handler functions are defined in handlers.js
            swfupload_preload_handler: preLoad,
            swfupload_load_failed_handler: loadFailed,
            file_queued_handler: fileQueued,
            file_queue_error_handler: fileQueueError,
            file_dialog_complete_handler: fileDialogComplete,
            upload_start_handler: uploadStart,
            upload_progress_handler: uploadProgress,
            upload_error_handler: uploadError,
            upload_success_handler: uploadSuccess,
            upload_complete_handler: uploadComplete,
            queue_complete_handler: queueComplete	// Queue plugin event
        };

        swfu = new SWFUpload(settings);
        Init();
        //编辑而生的方法
        attReloadForEdit(swfu);
    };
    function attReloadForEdit(swfu) {
        var arr = [];
        #foreach($drr in $Work.Atts)
            var fiel={}
            fiel.id='$drr.AttID';
            fiel.name='$drr.AttName';
            arr.push(fiel);
        #end
        for (var i = 0, j = arr.length; i < j; i++) {
            window.AttIDS.push(arr[i].id);
            var progress = new FileProgress(arr[i], swfu.customSettings.progressTarget);
        }
       
    }