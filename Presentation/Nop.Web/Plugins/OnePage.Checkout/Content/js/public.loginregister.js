/*
** nopCommerce login register js functions
*/
+function ($) {
    'use strict';
    if ('undefined' == typeof (jQuery)) {
        throw new Error('jQuery JS required');
    }

    function initValidation(form) {
        if (form.executed)
            return;
        
        form.removeData("validator");
        $.validator.unobtrusive.parse(document);
        form.executed = true;

        //attach ajax function as submit handler
        form.data("validator").settings.submitHandler = submitForm;
    }

    function validForm(form) {
        var validator = $.data(form[0], 'validator');
        if (validator.form()) {
            form.submit()
        }
    }

    function submitForm(form) {
        $.ajax({
            cache: false,
            type: "POST",
            url: $(form).attr("action"),
            data: $(form).serialize(),
            success: function (data, textStatus, jqXHR) {
                if (data.success) {
                    if (typeof data.returnUrl == "object") {
                        window.location = data.returnUrl.Url;
                    } else {
                        window.location = data.returnUrl;
                    }
                    $.magnificPopup.close();
                } else {
                    var messageerror = $(form).find(".message-error").length > 0 ? $(form).find(".message-error")[0] : $("<div>", { "class": "message-error" });
                    var errors = [];
                    $(messageerror).empty();
                    $.each(data.ModelState, function (key, value) {
                        if (value.Errors.length > 0) {
                            $.each(value.Errors, function (ekey, evalue) {
                                errors.push(evalue.ErrorMessage);
                            });
                        }
                    });
                    if (errors.length > 0) {
                        var message = $("<ul>");
                        $.each(errors, function (key, value) {
                            message.append($("<li>", { "text": value }))
                        });
                        $(messageerror).html(message);
                        $(form).prepend(messageerror);
                    }
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                var error = $.parseJSON(jqXHR.responseText);
                var modelState = error.modelState;
                var messageerror = $(form).find(".message-error").length > 0 ? $(form).find(".message-error")[0] : $("<div>", { "class": "message-error" });
                var errors = [];
                $(messageerror).empty();
                $.each(modelState, function (key, value) {
                    errors.push(modelState[key]);
                });
                if (errors.length > 0) {
                    var message = $("<ul>");
                    $.each(errors, function (key, value) {
                        message.append($("<li>", { "text": value }))
                    });
                    $(messageerror).html(message);
                    $(form).prepend(messageerror);
                }
            }
        });
    }

    $(document).ready(function () {
        $("form.animate-floating-labels").on('input propertychange change paste', function () {
            var messageerror = $(this).find(".message-error")
            if (messageerror.length > 0) {
                messageerror.each(function () {
                    $(this).empty();
                })
            }
        });

        if ($(document).has('.np-opc-checkout').length > 0) {
            $(".select-billing-address-button").on("click", function () {
                $('select[id="billing-address-select"] option[value="' + $(this).data("address-id") + '"]').prop('selected', true);
                $(".new-address-next-step-button", '#opc-billing').trigger('click');
            })
        }

        $(document).on('onepagecheckout_billing_address_reinit  onepagecheckout_shipping_address_reinit', function (data) {
            if (data.type == 'onepagecheckout_billing_address_reinit') {
                var selectedItem = $('#billing-address-select').children("option:selected").val();
                if (selectedItem == 0) {
                    Billing.newAddress(true);
                }
                $(".select-billing-address-button").on("click", function () {
                    $('select[id="billing-address-select"] option[value="' + $(this).data("address-id") + '"]').prop('selected', true);
                    $(".new-address-next-step-button", '#opc-billing').trigger('click');
                })
            }
            else {
                $(".select-shipping-address-button").on("click", function () {
                    $('select[id="shipping-address-select"] option[value="' + $(this).data("address-id") + '"]').prop('selected', true);
                    $(".new-address-next-step-button", '#opc-shipping').trigger('click');
                })
            }
            //Foxnetsoft phone number mask function was define or not
            if (typeof fnsPhoneNumberMask === 'function') {
                fnsPhoneNumberMask("input[name='Phone'], input[name='Fax'], input[name*='PhoneNumber'], input[name*='FaxNumber']");
            }

            //initialize floating label behaviour
            Behaviour.init();

            $('.address-slider').not('.slick-initialized').slick(), $('.address-slider').slick('setPosition')
        });

        $(document).on('accordion_section_opened', function (data) {
            if (data.previousSectionId) {
                $(".master-column-sidebar .order-summary, .checkout-info .checkout-review").addClass("loading");
                var updatesummary = $(".np-opc-checkout .checkout-review").data("updatesummary");
                $.ajax({
                    cache: false,
                    url: updatesummary,
                    data: {
                        "step": data.currentSectionId,
                        "prepareData": "true"
                    },
                    type: "POST",
                    success: function (response) {
                        $('#' + response.update_section.name + '-content').replaceWith(response.update_section.html);
                        $('#' + response.name + ' .checkout-review').html(response.html);
                        $('#topcartlink .toggle-wrap .sub-total').text(response.ordertotalstring);
                    },
                    error: Checkout.ajaxFailure,
                    complete: function (jqXHR, textStatus) {
                        $(".master-column-sidebar .order-summary, .checkout-info .checkout-review").removeClass("loading");
                    }
                });
            }

            if (data.currentSectionId == 'opc-billing') {
                $(document).trigger({ type: "onepagecheckout_billing_address_reinit" });
            }

            if (data.currentSectionId == 'opc-shipping') {
                $(document).trigger({ type: "onepagecheckout_shipping_address_reinit" });
            }
        });

        $(document).on('onepagecheckout_billing_address_new  onepagecheckout_shipping_address_new', function (data) {
            if (data.type == 'onepagecheckout_billing_address_new') {
                if ($('#billing-buttons-container').closest("li.active").length > 0) {
                    $('#billing-buttons-container .back-link').show();
                    var selectedItem = $('#BillingNewAddress_CountryId').val();
                    if (selectedItem == 0)
                        selectedItem = $('#BillingNewAddress_CountryId option:not(:eq(0)):first').val();
                    var selectedStateId = $('#BillingNewAddress_StateProvinceId').val();
                    if (selectedStateId != 0)
                        Billing.setSelectedStateId(selectedStateId);
                    $('#BillingNewAddress_CountryId').val(selectedItem).trigger('change');
                }
            }
            else {
                if ($('#shipping-buttons-container').closest("li.active").length > 0) {
                    var selectedItem = $('#ShippingNewAddress_CountryId').val();
                    if (selectedItem == 0)
                        selectedItem = $('#ShippingNewAddress_CountryId option:not(:eq(0)):first').val();
                    $('#ShippingNewAddress_CountryId').val(selectedItem).trigger('change');
                }
            }
        });

        $('select.address-select:hidden').on('change', function () {
            if (this.value != 0) {
                var addressList = $(this).closest('form').find('.address-slider');
                var addressSlick = addressList.slick("getSlick");
                var addressIndex = addressSlick.$slider.find('[data-address-id="' + this.value + '"]').data('slick-index');
                addressSlick.$slider.slick('slickPause').slick('slickGoTo', addressIndex);
            }
        });

        $('.address-slider').on('afterChange', function (event, slick, currentSlick) {
            var addressList = $(this).closest('form').find('select.address-select:hidden');
            var currentAddress = $(slick.$slides.get(currentSlick)).data('address-id')
            if (currentAddress != addressList.val()) {
                addressList.val(currentAddress).trigger('change');
            }
        });
    })

    $.fn.lrpopup = function () {
        this.magnificPopup({
            type: 'ajax',
            settings: {
                cache: false,
                url: $(this).attr('data-url'),
                type: 'GET',
            },
            removalDelay: 500,
            closeOnBgClick: false,
            callbacks: {
                beforeOpen: function () {
                    this.st.mainClass = 'login-register-popup-zoom-in';
                },
                open: function () {

                },
                ajaxContentAdded: function () {
                    var $content = $(this.content);

                    //init submit handler on each form
                    $($content).find('form').each(function () {
                        initValidation($(this))
                    })

                    //validate form submit using ajax
                    $($content).find('button[type=submit]').each(function () {
                        $(this).on('click', function (event) {
                            event.preventDefault();
                            var form = $(this).closest("form");
                            validForm(form)
                        });
                    })

                    //country select js functions
                    if ($($content).has('[data-trigger="country-select"]')) {
                        $('select[data-trigger="country-select"]').countrySelect();
                    }

                    //Foxnetsoft phone number mask function was define or not 
                    if (typeof fnsPhoneNumberMask === 'function') {
                        fnsPhoneNumberMask("input[name='Phone'], input[name='Fax'], input[name*='PhoneNumber'], input[name*='FaxNumber']");
                    }

                    //initialize floating label behaviour
                    Behaviour.init();
                }
            },
            tLoading: '<div class="login-register-popup-loader"></div>'
        }).magnificPopup('open');
    }

}(jQuery);