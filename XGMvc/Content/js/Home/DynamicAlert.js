/**
 * JQuery Tooltip Plugin
 *
 * Licensed under the MIT (http://www.opensource.org/licenses/mit-license.php)
 *
 * Written by Shahrier Akram <suxiaohua1020@163.com>
 *
 * Tooltip is a jQuery plugin implementing unobtrusive javascript interaction that appears
 * near DOM elements, wherever they may be, that require extra information. 
 * Visit http://gdakram.github.com/JQuery-Tooltip-Plugin for demo.
 * 苏晓华 eirc
**/

(function ($) {

    $.fn.tooltip = function (settings) {
        // Configuration setup
        var config = {
            'dialog_content_selector': 'div.tooltip_description',
            'animation_distance': 50,
            'opacity': 0.85,
            'arrow_left_offset': 63,
            'arrow_top_offset': 10,
            'arrow_height': 10,
            'arrow_width': 10,
            'animation_duration_ms': 300,
            'event_in': 'mouseover',
            'event_leave': 'mouseleave',
            'bInit': false
        };
        if (settings) $.extend(config, settings);

        /**
        * Apply interaction to all the matching elements
        **/
        this.each(function () {
            $(this).bind(config.event_in, function () {
                _show(this);
            })
        });

        /**
        * Positions the dialog box based on the target
        * element's location
        **/
        function _show(target_elm) {
            var dialog_content = $(target_elm).find(config.dialog_content_selector);
            var dialog_box = _create(dialog_content);
            var is_top_right = $(target_elm).hasClass("tooltiptopright");
            var is_bottom_right = $(target_elm).hasClass("tooltipbottomright");
            var is_top = $(target_elm).hasClass("tooltiptop");
            var is_bottom = $(target_elm).hasClass("tooltipbottom");
            var has_position = is_top_right || is_bottom_right || is_top || is_bottom;
            var position;

            var target_elm_position = $(target_elm).offset();

            // coming from the top right
            if (is_bottom || !has_position && (target_elm_position.top + config.animation_distance < $(dialog_box).outerHeight())) {
                position = {
                    start: {
                        left: target_elm_position.left + ($(target_elm).outerWidth() / 2) - config.arrow_left_offset,
                        top: target_elm_position.top + $(target_elm).outerHeight() + config.animation_distance
                    },
                    end: {
                        left: target_elm_position.left + ($(target_elm).outerWidth() / 2) - config.arrow_left_offset,
                        top: target_elm_position.top + $(target_elm).outerHeight() - 7
                    },
                    arrow_class: "div.up_arrow"
                }
            }
            // position and show the box
            $(dialog_box).css({
                top: position.start.top + "px",
                left: position.start.left + "px",
                opacity: config.opacity
            });
            $(dialog_box).find("div.arrow").hide();
            $(dialog_box).find(position.arrow_class).show();

            // begin animation
            $(dialog_box).animate({
                top: position.end.top,
                left: position.end.left,
                opacity: "toggle"
            }, config.animation_duration_ms);
            /***111***/
            initBindClick();

        }; // -- end _show function
        /**
        * Stop the animation (if any) and remove from dialog box from the DOM
        */
        function _hide(target_elm) {
            $("body").find("div.jquery-tooltip").stop().remove();
        };
        //用到cookie，要引用jquery.cookie.min.js
        function initBindClick() {
            var e = ".content li",
            d = "content",
            c = "cur";
            var b = [".css_style1", ".css_style2", ".css_style3", ".css_style4"]; //设置 css 外链标签id，要有对应名为 css_style 的css 外链。如果用class的话可以指定多个css 外链。

            var a = function (g) {
                var f = jQuery(b[0]).attr("href").split("/")[jQuery(b[0]).attr("href").split("/").length - 2];
                jQuery.each(b, function (h) {
                    if (jQuery(b[h]).size() != 0) {
                        jQuery(b[h]).attr("href", jQuery(b[h]).attr("href").replace(f, g.find("a").attr("rel")))
                    }
                })
            };
            jQuery(e).click(function (g) {
                var thmes = ["../../Scripts/jquery-easyui-1.3.4/themes/skin/red/red.css", "../../Scripts/jquery-easyui-1.3.4/themes/skin/green/green.css", "../../Scripts/jquery-easyui-1.3.4/themes/skin/blue/blue.css", "../../Scripts/jquery-easyui-1.3.4/themes/skin/gray/gray.css"];
                var iframeArr = $('iframe');
                for (var i = 0, j = iframeArr.length; i < j; i++) {
                    var tempIframe = iframeArr[i];
                    var gInnerHtml = g.target.innerHTML;
                    if (gInnerHtml.indexOf("red") >= 0) {
                        //$(tempIframe).contents().find("link")[0].href = thmes[0];
                    }
                    if (gInnerHtml.indexOf("green") >= 0) {
                        $(tempIframe).contents().find("link")[0].href = thmes[1];
                    }
                    if (gInnerHtml.indexOf("blue") >= 0) {
                        $(tempIframe).contents().find("link")[0].href = thmes[2];
                    }
                    if (gInnerHtml.indexOf("gray") >= 0) {
                        $(tempIframe).contents().find("link")[0].href = thmes[3];
                    }
                }
                a(jQuery(this));
                var h = jQuery(this).parent().children();
                var f = jQuery.inArray(this, h);
                jQuery.cookie(d, f, { path: "/", expires: new Date(new Date().getTime() + 24 * 3600000 * 365) });
                h.removeClass(c).eq(f).addClass(c);
                g.stopPropagation()

            })

        }

        /**
        * Creates the dialog box element
        * and appends it to the body
        **/
        function _create(content_elm) {
            var index = $.cookie("content");
            var sRed = "", sGreen = "", sBlue = "", sGray = "";
            var lgreen = '<li class="cur"><a id="lvs" rel="green" title="绿色"></a></li>';
            switch (index) {
                case "0": sRed = "cur"; break; case "1": sGreen = "cur"; break; case "2": sBlue = "cur"; break; case "3": sGray = "cur"; break;default:break;
            }
            var imglist = [
                '<ul>',
                    $.format('<li class="{0}"><a id="hos" rel="red" title="红色"></a></li>', sRed),
                    index == null ? lgreen : $.format('<li class="{0}"><a id="lvs" rel="green" title="绿色"></a></li>', sGreen),
                    $.format('<li class="{0}"><a id="las" rel="blue" title="蓝色"></a></li>', sBlue),
                    $.format('<li class="{0}"><a id="hvs" rel="gray" title="灰色"></a></li>', sGray),
                '</ul>'
            ].join('');
            var str = [
                '<div class="jquery-tooltip">',
                    '<div class="up_arrow arrow">',
                    '</div>',
                    '<div class="content">',
                        imglist,
                    '</div>',
                '</div>'
            ].join('');
            var jq = $(str).appendTo('body');
           $(".jquery-tooltip .content").bind('mouseleave', function () {
                _hide();
            })
            return jq;
        };
        return this;

    };

})(jQuery);
