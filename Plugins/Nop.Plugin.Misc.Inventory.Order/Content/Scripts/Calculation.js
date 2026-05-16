var totalVolume = 0;
var array = [];
var editor;
var editIndexTable = -1;
var EditRowData = [];
var totalCartoon = 0;
var oldCartoon = 0;
var isUpdated = false;
/*SelectedIds for Excel*/
$(document).ready(function () {

    $('#create-po-order-grid').on('dblclick', 'tbody td:nth-child(13)', function (e) {
        if (!isUpdated) {
            var data = $(this).parent();
            totalCartoon = parseFloat($(this).text());
            oldCartoon = parseInt($(this).text());
            editData_create_po_order_grid(data, '6');
            isUpdated = true;
        }
    });

    $('#create-po-order-grid').on('focusout', 'tbody td:nth-child(13)', function (e) {
        var checkBoxValue = $(this).closest('tr').find('td:eq(0) input').val();
        var isChecked = $(this).closest('tr').find('td:eq(0) input').prop("checked");
        var boxVolume = $(this).closest('tr').find('td').eq(13).html();
        var totalQty = $(this).closest('tr').find('td').eq(11).html();

        var orderQty1 = parseInt($(this).closest('tr').find('td').eq(7).html());
        var orderQty2 = parseInt($(this).closest('tr').find('td').eq(8).html());
        var orderQty3 = parseInt($(this).closest('tr').find('td').eq(9).html());
        var orderQty4 = parseInt($(this).closest('tr').find('td').eq(10).html());
        var totalProductCartoon = parseInt($(this).closest('tr').find('td').eq(15).html());
        var totalBoxVolume = parseFloat($(this).closest('tr').find('td').eq(16).html());

        var $input = $(this).find('input');
        if ($input.length > 0) {
            totalCartoon = parseFloat($input.val()) || 0;
        }

        var data = $(this).parent();
        cancelEditData_create_po_order_grid(data, '6')
        $($(this)).text(totalCartoon);
        isUpdated = false;

        var chkarray = checkBoxValue.split(',');
        var newtotalQty = Math.ceil(totalCartoon * totalProductCartoon);
        chkarray[2] = newtotalQty;
        checkBoxValue = chkarray.toString();
        $(this).closest('tr').find('td').eq(11).html(newtotalQty);

        if (totalCartoon > 0) {
            console.log('Enabling checkbox for TotalCartoon:', totalCartoon);
            $(this).closest('tr').find('td:eq(0) input').prop("disabled", false);
        }
        else {
            console.log('Disabling checkbox for TotalCartoon:', totalCartoon);
            $(this).closest('tr').find('td:eq(0) input').prop("disabled", true);
        }

        $(this).closest('tr').find('td:eq(0) input').val(checkBoxValue);

        var newVolume = totalCartoon * totalBoxVolume;

        if (newVolume > 0.00 && newtotalQty > 0) {
            $(this).closest('tr').find('td').eq(13).html(newVolume.toFixed(4));
            $(this).closest('tr').find('td:eq(0) input').attr("totalCartoon", totalCartoon);
            chkarray = checkBoxValue.split(',');
            chkarray[3] = newVolume.toFixed(4);
            checkBoxValue = chkarray.toString();
            $(this).closest('tr').find('td:eq(0) input').val(checkBoxValue);
            if (isChecked) {
                totalVolume = (totalVolume - boxVolume) + newVolume;
                $("#lblTotalVolume").text(totalVolume.toFixed(4));
                $.each($("table #lblTotalVolumespan"), function () {
                    $(this).text(totalVolume.toFixed(4))
                });
            }
        }

    });


    $(document).on('change', '.userinput', function (e) {
        totalCartoon = parseFloat($(this).val());
    });

    $(document).on('keypress', '.userinput', function (evt) {
        if (evt.which == 48 && $(this).val().length < 1) {
            return false;
        }

        evt = (evt) ? evt : window.event;
        var charCode = (evt.which) ? evt.which : evt.keyCode;
        if (charCode === 13) {
            return false;
        }

        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }
        return true;
    });

    document.getElementById("search-InventoryOrder").addEventListener("click", function () {
        isUpdated = false;
        $("#lblTotalVolume").text('0.00');
        $.each($("table #lblTotalVolumespan"), function () {
            $(this).text('0.00')
        });
    });

});

$(document).on("change", "input[type=checkbox][name=checkbox_InventoryOrder]", function () {
    totalVolume = 0;
    var anArray = this.value.split(',');
    var lastItem = anArray.pop();
    if (parseFloat(lastItem) != 0) {
        array.splice(0, array.length);
        $('input[type=checkbox][name=checkbox_InventoryOrder]:checked').each(function () {
            array.push(this.value.split(','));
        });
        array = array.map(JSON.stringify).reverse().filter(function (e, i, a) {
            return a.indexOf(e, i + 1) === -1;
        }).reverse().map(JSON.parse)
    }

    for (var i = 0; i < array.length; i++) {
        totalVolume += parseFloat(array[i][3]);
    }


    $("#lblTotalVolume").text(totalVolume.toFixed(4));
    $.each($("table #lblTotalVolumespan"), function () {
        $(this).text(totalVolume.toFixed(4))
    });
});
$("#btnAddCreatePoOrder").click(function () {
    if (selectedIds.length > 0) {
        $("#lblTotalBoxVolume").text("Total Box Volume : 1");
        $("#poorder-window").modal("show");
        $("#Comment").val('');
        var percentage = $("#SearchPercentage").val();
        $("#PercentAdjusted").val(percentage);
    }
    else {
        alert("Please Select Manufacture.");
    }
    return false;
});

