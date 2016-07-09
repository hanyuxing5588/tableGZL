/* Demo Note:  This demo uses a FileProgress class that handles the UI for displaying the file name and percent complete.
The FileProgress class is not part of SWFUpload.
*/


/* **********************
Event Handlers
These are my custom event handlers to make my
web application behave the way I went when SWFUpload
completes different tasks.  These aren't part of the SWFUpload
package.  They are part of my application.  Without these none
of the actions SWFUpload makes will show up in my application.
********************** */
function preLoad() {
    if (!this.support.loading) {
        alert("要需要安装 Flash Player 9.028 或者以上的版本。");
        return false;
    }

    return true;
}
function loadFailed() {
    alert("Something went wrong while loading SWFUpload. If this were a real application we'd clean up and then give you an alternative");
}

function fileQueued(file) {
    try {
    
            var uploadedFiled=this.getFile(1);
           if(uploadedFiled&&uploadedFiled.id)
               this.cancelUpload(uploadedFiled.id);
        
//        var progress = new FileProgress(file, this.customSettings.progressTarget);
//        progress.setStatus("等待中...");
//        progress.toggleCancel(true, this);

    } catch (ex) {
        this.debug(ex);
    }

}

function fileQueueError(file, errorCode, message) {
    try {
        if (errorCode === SWFUpload.QUEUE_ERROR.QUEUE_LIMIT_EXCEEDED) {
            alert("You have attempted to queue too many files.\n" + (message === 0 ? "You have reached the upload limit." : "You may select " + (message > 1 ? "up to " + message + " files." : "one file.")));
            return;
        }

        var progress = new FileProgress(file, this.customSettings.progressTarget);
        progress.setError();
        progress.toggleCancel(false);

        switch (errorCode) {
            case SWFUpload.QUEUE_ERROR.FILE_EXCEEDS_SIZE_LIMIT:
                progress.setStatus("文件太大.");
                this.debug("Error Code: File too big, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.QUEUE_ERROR.ZERO_BYTE_FILE:
                progress.setStatus("不能上传0字节的文件.");
                this.debug("Error Code: Zero byte file, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.QUEUE_ERROR.INVALID_FILETYPE:
                progress.setStatus("Invalid File Type.");
                this.debug("Error Code: Invalid File Type, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            default:
                if (file !== null) {
                    progress.setStatus("上传异常.");
                }
                this.debug("Error Code: " + errorCode + ", File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
        }
    } catch (ex) {
        this.debug(ex);
    }
}

function fileDialogComplete(numFilesSelected, numFilesQueued) {
    try {
        
        if (numFilesSelected > 0) {
          var cancalId=this.customSettings.cancelButtonId
          if (cancalId) {
              document.getElementById(cancalId).disabled = false;   //上传成功后取消按钮的编辑状态。true:不可编辑
          }
        }

        /* I want auto start the upload and I can do that here */
        this.startUpload();
    } catch (ex) {
        this.debug(ex);
    }
}

//启动：上传
function uploadStart(file) {
    try {
        /* I don't want to do any file validation or anything,  I'll just update the UI and
        return true to indicate that the upload should start.
        It's important to update the UI here because in Linux no uploadProgress events are called. The best
        we can do is say we are uploading.
        */
        var progress = new FileProgress(file, this.customSettings.progressTarget);
        progress.setStatus("开始上传...");
//        progress.toggleCancel(true, this);
    }
    catch (ex) { }

    return true;
}

//取消
//function cancelQueue(file) {
//    try {
//        this.customSettings.queue = this.customSettings.queue || new Array();
//        while (this.customSettings.queue.length > 0) {
//            this.cancelUpload(this.customSettings.queue.pop(), false);
//        }
//        this.customSettings.queue.push(file.id);
//    }
//    catch (e) { }
//    return true;
//}


function uploadProgress(file, bytesLoaded, bytesTotal) {
    try {
        var percent = Math.ceil((bytesLoaded / bytesTotal) * 100);
        var progress = new FileProgress(file, this.customSettings.progressTarget);
        progress.setProgress(percent);
        progress.setStatus("上传中...");
    } catch (ex) {
        this.debug(ex);
    }
}

//上传成功后，保存成功附件ID，返回//政策法规
function uploadSuccess_zcfg(file, serverData) {
    try {
        
        $.fn.tree.methods["setIsVisibile"].call(this, '#fsUploadProgress');
        //得到附件的GUID(serverData)
        var obj = eval('(' + serverData + ')');
        var IDS = obj.fileIds.split(',');
        var progress = new FileProgress(file, this.customSettings.progressTarget);
        progress.setComplete();
        progress.setStatus("完成");
        progress.toggleCancel(false);
        //用来存放上传成功返回文件的guid
        var storId = '#' + this.customSettings.storId;
        var FileName = '#' + this.customSettings.FileName
        //将最后组织好的字符串赋值给界面要需要向后台传递的控件
        $(storId).val(IDS[0]);
        $(FileName).val(IDS[1]);
    } catch (ex) {
        this.debug(ex);
    }
}
//通知公告
function uploadSuccess_tzgg(file, serverData) {
    try {
        
        //得到附件的GUID(serverData)
        var obj = eval('(' + serverData + ')');
        var IDS = obj.fileIds.split(',');
        file.itemId = IDS[0];
        
        var progress = new FileProgress(file, this.customSettings.progressTarget);
        progress.setComplete();
        progress.setStatus("完成");
        progress.toggleCancel(false);
        var storId = '#' + this.customSettings.storId;
        var FileName = '#' + this.customSettings.FileName
        var val1 = $(storId).val();
        var val2 = $(FileName).val();
        //将最后组织好的字符串赋值给界面要需要向后台传递的控件
        if (!val1.length) {
            $(storId).val(IDS[0]);
            $(FileName).val(IDS[1]);
        } else {
            $(storId).val(val1 + "," + IDS[0]);
            $(FileName).val(val2 + "," + IDS[1]);
        }
        
    } catch (ex) {
        this.debug(ex);
    }
}


function uploadError(file, errorCode, message) {
    try {
        var progress = new FileProgress(file, this.customSettings.progressTarget);
        progress.setError();
        progress.toggleCancel(false);
        switch (errorCode) {
            case SWFUpload.UPLOAD_ERROR.HTTP_ERROR:
                progress.setStatus("Upload Error: " + message);
                this.debug("Error Code: HTTP Error, File name: " + file.name + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.UPLOAD_FAILED:
                progress.setStatus("Upload Failed.");
                this.debug("Error Code: Upload Failed, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.IO_ERROR:
                progress.setStatus("Server (IO) Error");
                this.debug("Error Code: IO Error, File name: " + file.name + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.SECURITY_ERROR:
                progress.setStatus("Security Error");
                this.debug("Error Code: Security Error, File name: " + file.name + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.UPLOAD_LIMIT_EXCEEDED:
                progress.setStatus("Upload limit exceeded.");
                this.debug("Error Code: Upload Limit Exceeded, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.FILE_VALIDATION_FAILED:
                progress.setStatus("Failed Validation.  Upload skipped.");
                this.debug("Error Code: File Validation Failed, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.FILE_CANCELLED:
                // If there aren't any files left (they were all cancelled) disable the cancel button
                if (this.getStats().files_queued === 0) {
                    document.getElementById(this.customSettings.cancelButtonId).disabled = true;    //取消按钮的状态
                }
                progress.setStatus("取消上传");
                progress.setCancelled();
                break;
            case SWFUpload.UPLOAD_ERROR.UPLOAD_STOPPED:
                progress.setStatus("停止上传");
                break;
            default:
                progress.setStatus("Unhandled Error: " + errorCode);
                this.debug("Error Code: " + errorCode + ", File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
        }
    } catch (ex) {
        this.debug(ex);
    }
}

function uploadComplete(file) {
//    if (this.getStats().files_queued === 0) {
//        document.getElementById(this.customSettings.cancelButtonId).disabled = false;
//    }
    //var progress = new FileProgress(file, this.customSettings.progressTarget);
    //this.removeFileParam(file.id, file.name); //hanyx
    //this.getStats().successful_uploads = 0;

    //this.getQueueFile
}

// This event comes from the Queue Plugin
function queueComplete(numFilesUploaded) {//可以显示传了几个
    //    var status = document.getElementById("divStatus");
    //    var a=status.innerText
    //    status.innerHTML = a+numFilesUploaded; //numFilesUploaded + " file" + (numFilesUploaded === 1 ? "" : "s") + " uploaded.";
}
