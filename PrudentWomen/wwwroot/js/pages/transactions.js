/**
 *
 * You can write your JS code here, DO NOT touch the default style file
 * because it will make it harder for you to update.
 * 
 */

"use strict";

$('[data-identify]').click(function (e) {
    e.preventDefault()
    var me = $(this)
    let tran_d = me.data("identify")
    $("#TransactionId").val(tran_d)
    $("#identify_modal").modal("show")

    $("#submit_trans").click(function (e) {
        e.preventDefault()
        // Form Data
        $("#iden_errors").empty()
        let form_data = $("#identify_modal form").serialize();

        $(".page-loader-wrapper").toggle()
        $.ajax({
            method: "POST",
            url: `/admin/banktransaction/ManualMap/?${form_data}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location = window.location;
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#iden_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#iden_errors").append(`<span>${errors[i]}</span><br />`)
                }
            }
        })
    })

});

$("#disburse_btn").click(function (e) {
    e.preventDefault()
    var me = $(this)
    let id = me.data("id")

    $(".page-loader-wrapper").toggle()
    $.ajax({
        method: "POST",
        url: `/admin/Loans/disburse/?id=${id}`,
        processData: false,
        async: false,
        success: function (xhr) {
            console.log(xhr)
            window.open(xhr);
        },
        error: function (xhr) {
            $(".page-loader-wrapper").hide()
            var errors = getErrors(xhr.responseJSON)
            $("#iden_errors").empty()
            for (var i = 0; i < errors.length; i++) {
                $("#iden_errors").append(`<span>${errors[i]}</span><br />`)
            }
        }
    })
})

$("#DisbursementAccount").change(function (e) {
    e.preventDefault()
    var me = $(this)
    let account = me.val()
    let code = $("#BankCode").val()

    if (account.length == 10) {
        $(".page-loader-wrapper").toggle()
        $("#name_inquiry").empty()
        $.ajax({
            method: "POST",
            url: `/admin/userLoans/NameInquiry/?account=${account}&bankCode=${code}`,
            processData: false,
            async: false,
            success: function (xhr) {
                $(".page-loader-wrapper").hide()
                $("#name_inquiry").append(`<span class='text-success'>${xhr.name}</span><br />`)
                console.log("found account", xhr)
                
            },
            error: function (xhr) {

                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                for (var i = 0; i < errors.length; i++) {
                    $("#name_inquiry").append(`<span class='text-danger'>${errors[i]}</span><br />`)
                }
                console.log("errors", errors)
            }
        })
    }    
})

function getErrors(response) {
    let errors = []
    for (var key in response) {
        errors.push(response[key])
    }
    return errors.flat()
}