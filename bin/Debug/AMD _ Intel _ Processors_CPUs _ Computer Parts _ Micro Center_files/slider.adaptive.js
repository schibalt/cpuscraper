var contentSliders;

$(document).ready(function () {

    /*
    ----------------------------------------------------------
    SLIDER ADAPTIVE
    ---------------------------------------------------------- */


            contentSliders = $(".contentSlider").bxSlider2({
                    minSlides: 1,
                    maxSlides: 1,
                    moveSlides: 1,
                    slideWidth: 1220,
                    slideMargin: 0,
                    controls: true,
                    pager: true,
                    autoControls: true,
                    infiniteLoop: false,
                    auto: true,
                    adaptiveHeight: true,
                    captions: true
                });
           
    
    
    // EVENT TIMER
    var waitForFinalEvent = (function () {
        var timers = {};
        return function (callback, ms, uniqueId) {
            if (!uniqueId) {
                uniqueId = "slider adaptive";
            }

        
            if (timers[uniqueId]) {
                clearTimeout(timers[uniqueId]);
            }
            //timers[uniqueId] = setTimeout(callback, ms);  JJ - originally set timeout
            timers[uniqueId] = setTimeout(callback, -1);
        };
    })();

    // RESPONSIVE
    $(window).on("load resize", function (e) {
        //var width = $(window).width();

        if (!$("body").is("#home")) {
           
            
        }
        else {




            if ($("section").css("width") == "1220px") {
                waitForFinalEvent(function (s1220) {
                    if (!$("#adverts .largeBnrs").is(".featureSlider")) {
                        $("#adverts .largeBnrs").addClass("featureSlider");
                        slider2 = $(".largeBnrs").bxSlider2({
                            minSlides: 1,
                            maxSlides: 1,
                            moveSlides: 1,
                            slideWidth: 650,
                            slideMargin: 0,
                            controls: true,
                            pager: true,
                            autoControls: true,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    if ($("#adverts .smallBnrs").is(".bnrSlider")) {
                        slider.destroySlider();
                        $("#adverts .smallBnrs").removeClass("bnrSlider");
                    }
                    if ($(".bx-viewport").parent().is(".bx-wrapper")) {
                        $(".bx-viewport").unwrap();
                    }
                    if ($(".smallBnrs").parent().is(".bx-viewport")) {
                        $(".smallBnrs").unwrap();
                    }
                    //...
                }, 300, "resize functions");
            }

            if ($("section").css("width") == "970px") {
                waitForFinalEvent(function (s970) {
                    if (!$("#adverts .largeBnrs").is(".featureSlider")) {
                        $("#adverts .largeBnrs").addClass("featureSlider");
                        slider2 = $(".largeBnrs").bxSlider2({
                            minSlides: 1,
                            maxSlides: 1,
                            moveSlides: 1,
                            slideWidth: 650,
                            slideMargin: 0,
                            controls: true,
                            pager: true,
                            autoControls: true,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    if (!$("#adverts .smallBnrs").is(".bnrSlider")) {
                        $("#adverts .smallBnrs").addClass("bnrSlider");
                        slider = $(".bnrSlider").bxSlider({
                            minSlides: 1,
                            maxSlides: 2,
                            moveSlides: 1,
                            slideWidth: 330,
                            slideMargin: 0,
                            controls: true,
                            pager: false,
                            autoControls: false,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    //...
                }, 300, "resize functions");
            }

            if ($("section").css("width") == "720px") {
                waitForFinalEvent(function (s720) {
                    if (!$("#adverts .largeBnrs").is(".featureSlider")) {
                        $("#adverts .largeBnrs").addClass("featureSlider");
                        slider2 = $(".largeBnrs").bxSlider2({
                            minSlides: 1,
                            maxSlides: 1,
                            moveSlides: 1,
                            slideWidth: 650,
                            slideMargin: 0,
                            controls: true,
                            pager: true,
                            autoControls: true,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    if (!$("#adverts .smallBnrs").is(".bnrSlider")) {
                        $("#adverts .smallBnrs").addClass("bnrSlider");
                        slider = $(".bnrSlider").bxSlider({
                            minSlides: 1,
                            maxSlides: 2,
                            moveSlides: 1,
                            slideWidth: 330,
                            slideMargin: 0,
                            controls: true,
                            pager: false,
                            autoControls: false,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    //...
                }, 300, "resize functions");
            }

            if ($("section").css("width") == "640px") {
                waitForFinalEvent(function (s640) {
                    if (!$("#adverts .smallBnrs").is(".bnrSlider")) {
                        $("#adverts .smallBnrs").addClass("bnrSlider");
                        slider = $(".bnrSlider").bxSlider({
                            minSlides: 2,
                            maxSlides: 2,
                            moveSlides: 1,
                            slideWidth: 320,
                            slideMargin: 0,
                            controls: true,
                            pager: false,
                            autoControls: false,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    //...
                }, 300, "resize functions");
            }

            if ($("section").css("width") == "480px") {
                waitForFinalEvent(function (s480) {
                    if (!$("#adverts .smallBnrs").is(".bnrSlider")) {
                        $("#adverts .smallBnrs").addClass("bnrSlider");
                        slider = $(".bnrSlider").bxSlider({
                            minSlides: 1,
                            maxSlides: 2,
                            moveSlides: 1,
                            slideWidth: 330,
                            slideMargin: 0,
                            controls: true,
                            pager: false,
                            autoControls: false,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    //...
                }, 300, "resize functions");
            }

            if ($("section").css("width") == "320px") {
                waitForFinalEvent(function (s320) {
                    if (!$("#adverts .smallBnrs").is(".bnrSlider")) {
                        $("#adverts .smallBnrs").addClass("bnrSlider");
                        slider = $(".bnrSlider").bxSlider({
                            minSlides: 1,
                            maxSlides: 2,
                            moveSlides: 1,
                            slideWidth: 320,
                            slideMargin: 0,
                            controls: true,
                            pager: false,
                            autoControls: false,
                            infiniteLoop: false,
                            auto: true
                        });
                    }
                    //...
                }, 300, "resize functions");
            }
        }
    });

});
// END READY