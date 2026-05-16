/*
** nopCommerce product extension js functions
*/
+function ($) {
    'use strict';
    if ('undefined' == typeof (jQuery)) {
        throw new Error('jQuery JS required');
    }

    $(document).ready(function () {
        var fn;
        $(window).on('load scroll', function () {
            if ($(document).has('.np-opc-checkout').length === 0) {
                var e = $('.header-lower'), t = $('.header'), n = $(window).scrollTop();
                if ($(window).width() >= 1260) {
                    0 < n && n >= t.offset().top ? (e.addClass('stick'), t.css('height', e.height() + 'px')) : (e.removeClass('stick'), t.css('height', ''))
                }
            }
        });

        $('.cart td.quantity select[id^="itemquantity"]').each(function () {
          $(this).on("change", function () { $('.update-cart-button').click(); });
        });

        $('.cart td.quantity input[id^="itemquantity"]').each(function () {
            $(this).on("change", function () { $('.update-cart-button').click(); })
            var decrease = $("<div>", { "class": "decrease", "data-field": $(this).attr("id"), "text": "-" });
            decrease.click(function (n) {
                var t, i, r;
                if (n.preventDefault(), fn = $(this).attr("data-field"), t = parseInt($("input[name=" + fn + "]").val()), !isNaN(t) && t > 0)
                    if ($("input[name=" + fn + "]").val(t - 1), i = parseInt($("input[name=" + fn + "]").val()), i != 0) $(".update-cart-button").trigger("click");
                    else {
                        if (r = confirm("Do you want to remove this item?"), r == !0) return $(".update-cart-button").trigger("click"), !1;
                        $("input[name=" + fn + "]").val(t)
                    }
                else $("input[name=" + fn + "]").val(0)
            });
            var increase = $("<div>", { "class": "increase", "data-field": $(this).attr("id"), "text": "+" });
            increase.click(function (n) {
                n.preventDefault();
                fn = $(this).attr("data-field");
                var t = parseInt($("input[name=" + fn + "]").val());
                return isNaN(t) ? $("input[name=" + fn + "]").val(0) : $("input[name=" + fn + "]").val(t + 1), $(".update-cart-button").trigger("click"), !1
            });
            var editable = $("<div>", { "class": "editable" });
            $(this).parent().append(editable);
            $(this).appendTo(editable);
            editable.prepend(decrease);
            editable.append(increase);

            $('[data-quantity="remove"]').click(function () {
                fn = $(this).attr("data-field");
                var n = confirm("Do you want to remove this item?");
                if (n == !0) return $("input[name=" + fn + "]").val(0), $(".update-cart-button").trigger("click"), !1
            })
        });
    });

    //focus on review tab while click on link
    if ($(document).has('#quickTabs a[href="#quickTab-reviews"]').length > 0) {
        $(document).on('click', 'a[href*="/productreviews"]', function (event) {
            event.preventDefault();
            $('a[href="#quickTab-reviews"]').trigger("click");
            $("html,body").animate({ scrollTop: $('a[href="#quickTab-reviews').offset().top - 150 }, "slow")
        });
    }
}(jQuery); 