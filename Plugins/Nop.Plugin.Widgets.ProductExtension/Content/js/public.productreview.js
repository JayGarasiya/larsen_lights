!function (a) {
    a(document).ready(function (r) {
        var t = r("#productReviews");
        t.on("click", "#add-review", function () {
            var e;
            e = t.attr("data-productReviewsAddNewUrl"), a("#updateReviews")[0].style.opacity = .5,
            a.ajax({
                cache: !1, type: "POST", url: e, data: a("#product-details-form").serialize()
            }).done(function (e) {
                a("#updateReviews")[0].style.opacity = 1, a("#updateReviews").replaceWith(e)
            }).fail(function () {
                a("#updateReviews")[0].style.opacity = 1, alert("Failed to add review.")
            })
        }).on("click", ".product-review-helpfulness .vote", function () {
            var e = r(this).closest(".product-review-helpfulness"), t = parseInt(e.attr("data-productReviewId")) || 0, a = e.attr("data-productReviewVoteUrl");
            r.ajax({
                cache: !1, type: "POST", url: a, data: { productReviewId: t, washelpful: r(this).hasClass("vote-yes") }
            }).done(function (e) {
                r("#productReviews #helpfulness-vote-yes-" + t).html(e.TotalYes), r("#productReviews #helpfulness-vote-no-" + t).html(e.TotalNo), r("#productReviews #helpfulness-vote-result-" + t).html(e.Result).fadeIn("slow").delay(1e3).fadeOut("slow")
            }).fail(function () {
                alert("Failed to vote. Please refresh the page and try one more time.")
            })
        })
    })

    //focus on review tab while click on link
    if (a("#productReviews").length) {
        a(document).on("click", 'a[href*="/productreviews"]', function (event) {
            var $target = a(this).index() === 0 ? $(".product-reviews") : $(".product-reviews #review-form");
            if (!$target.length || !$target.offset()) return;
            event.preventDefault();
            a("html,body").animate({ scrollTop: $target.offset().top - 150 }, "slow");
        });
    }
}(jQuery);