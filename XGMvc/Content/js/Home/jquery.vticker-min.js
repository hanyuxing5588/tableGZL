/*
* vertical news ticker
* Tadas Juozapaitis ( kasp3rito@gmail.com )
* http://plugins.jquery.com/project/vTicker
*/
(function (a) {
    a.fn.vTicker = function (b) {
        var c = {
            speed: 700,
            pause: 4000,
            showItems: 3,
            animation: "",
            mousePause: true,
            isPaused: false,
            direction: "up",
            height: 0
        };
        var b = a.extend(c, b);
        moveUp = function (g, d, e) {
            if (e.isPaused) {
                return
            }
            var f = g.children("ul");
            var h = f.children("li:first").clone(true);
            if (e.height > 0) {
                d = f.children("li:first").height()
            }
            f.animate({
                top: "-=" + d + "px"
            },
            e.speed,
            function () {
                a(this).children("li:first").remove();
                a(this).css("top", "0px")
            });
            if (e.animation == "fade") {
                f.children("li:first").fadeOut(e.speed);
                if (e.height == 0) {
                    f.children("li:eq(" + e.showItems + ")").hide().fadeIn(e.speed)
                }
            }
            h.appendTo(f)
        };
        moveDown = function (g, d, e) {
            if (e.isPaused) {
                return
            }
            var f = g.children("ul");
            var h = f.children("li:last").clone(true);
            if (e.height > 0) {
                d = f.children("li:first").height()
            }
            f.css("top", "-" + d + "px").prepend(h);
            f.animate({
                top: 0
            },
            e.speed,
            function () {
                a(this).children("li:last").remove()
            });
            if (e.animation == "fade") {
                if (e.height == 0) {
                    f.children("li:eq(" + e.showItems + ")").fadeOut(e.speed)
                }
                f.children("li:first").hide().fadeIn(e.speed)
            }
        };
        return this.each(function () {
            var f = a(this);
            var e = 0;
            f.css({
                overflow: "hidden",
                position: "relative"
            }).children("ul").css({
                position: "absolute",
                "margin-top": 0,
                "margin-bottom": 0
            }).children("li").css({
                "padding-top": 5
            });
            if (b.height == 0) {
                //f.children("ul").children("li").each(function () {
                //    
                //    if (a(this).height() > e) {
                //        e = a(this).height()
                //    }
                //});
                f.children("ul").children("li").each(function () {
                    a(this).height("10")
                });
                //f.height(e * b.showItems)
                f.height("89%")
            } else {
                //f.height(b.height)
                f.height("89%")
            }
            var d = setInterval(function () {
                if (b.direction == "up") {
                    moveUp(f, e, b)
                } else {
                    moveDown(f, e, b)
                }
            },
            b.pause);
            if (b.mousePause) {
                f.bind("mouseenter",
                function () {
                    b.isPaused = true
                }).bind("mouseleave",
                function () {
                    b.isPaused = false
                })
            }
        })
    }
})(jQuery);