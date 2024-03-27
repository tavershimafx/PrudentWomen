/**
 *
 * You can write your JS code here, DO NOT touch the default style file
 * because it will make it harder for you to update.
 * 
 */

"use strict";
function formatMoney(x) {
    return x.toString().replace(/\B(?<!\.\d*)(?=(\d{3})+(?!\d))/g, ",");
}

$('[data-identify]').click(function (e) {
    e.preventDefault()
    var me = $(this)
    let tran_d = me.data("identify")
    $("#TransactionId").val(tran_d)
    $("#single_identify_modal").modal("show")

    $("#submit_trans").click(function (e) {
        e.preventDefault()
        // Form Data
        $("#iden_errors").empty()
        let form_data = $("#single_identify_modal form").serialize();

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

$('[data-bulk]').click(function (e) {
    e.preventDefault()
    var me = $(this)
    let transactionId = me.data("bulk")
    let totalBulkAmount = me.data("amount")

    $("#bulk_identify_modal form .modal-body .row").remove();

    $("#bulk_identify_modal").modal("show")

    $("#add_new_bulk").unbind('click')
    createAppendEvent()

    $("#submit_bulk").click(function (e) {
        e.preventDefault()
        $("#bulk_errors").empty()
        
        let bulkUsers = []
        $('#bulk_identify_modal form .modal-body').children('div.row').each((index, item) => {
            let user = {
                username: $($(item).find('[name="username"]')[0]).val(),
                amount: Number($($(item).find('.item-amount')[0]).val())
            } 

            if (user.username == undefined || user.username.trim() == '') {
                $("#bulk_errors").html(`Please verify your entries and make sure a username field is not empty.`)
                return
            }

            if (user.amount == NaN || user.amount == 0) {
                $("#bulk_errors").html(`Please verify your entries and make sure all amounts are valid numbers.`)
                return
            }

            bulkUsers.push(user)
        })

        let sumTotal = bulkUsers.reduce((a, b) => (a.amount + b.amount))
        if (isNaN(sumTotal)) {
            sumTotal = bulkUsers[0].amount
        }

        if (sumTotal != totalBulkAmount) {
            $("#bulk_errors").html(`<p class="text-danger">
                The total amount entered does not match the amount made in the transaction.
                The total amount entered is <b>₦${formatMoney(sumTotal.toFixed(2))}</b> 
                while the transaction has <b>₦${formatMoney((totalBulkAmount).toFixed(2))}</b></p>`)
            return
        }


        $("#bulk_errors").empty()
        $(".page-loader-wrapper").toggle()

        $.ajax({
            method: "POST",
            url: `/admin/banktransaction/BulkMap/?transactionId=${transactionId}`,
            data: bulkUsers,
            processData: false,
            async: false,
            success: function (xhr) {
                console.log("bulk response", xhr)
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#bulk_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#bulk_errors").append(`<span>${errors[i]}</span><br />`)
                }
            }
        })
    })

});

function createAppendEvent(){
    $('#add_new_bulk').click(function (e) {
        e.preventDefault()
        let fId = Math.ceil(Math.random() * 1000000)
        let template = ` <div class="row" id="${fId}">
                        <div class="col-6">
                            <input type="text" class="form-control" name="username" placeholder="Username" />
                        </div>
                        <div class="col-6">
                            <div class="form-group">
                                <div class="input-group">
                                    <input type="text" class="form-control item-amount" name="amount" placeholder="Amount" aria-label="Amount">
                                    <div class="input-group-append">
                                        <button class="btn btn-danger rmv-usr" type="button"><i class="fa fa-times"></i></button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>`

        $('#bulk_identify_modal form .modal-body').append(template)
        refreshBulkRemoveEvent()
        bulkItemChanged()
    });
}

function refreshBulkRemoveEvent() {
    $("button.rmv-usr").click(function (e) {
        e.preventDefault()

        let row = $(this).closest("div.row")
        let row_id = row.attr('id')
        $(`#${row_id}`).remove()
    });
}

function bulkItemChanged() {
    $(".item-amount").off('change')
    $(".item-amount").change(function (e) {
        let children = $('#bulk_identify_modal form .modal-body').children('div.row')

        let total = 0
        children.each((index, item) => {
            total += Number($($(item).find('.item-amount')[0]).val())
        })

        $("#items_sum").html(`₦${formatMoney(total.toFixed(2))}`)
    });
}

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
                
            },
            error: function (xhr) {

                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                for (var i = 0; i < errors.length; i++) {
                    $("#name_inquiry").append(`<span class='text-danger'>${errors[i]}</span><br />`)
                }
            }
        })
    }    
})

$('[data-loan]').click(function (e) {
    var me = $(this)
    let transactionId = me.data("loan")

    getUnpaidLoans(transactionId)
    $("#loan_modal").modal("show")

})

function getUnpaidLoans(transactionId) {
    $.ajax({
        method: "GET",
        url: `/admin/Loans/GetUnpaidLoans/`,
        processData: false,
        async: false,
        success: function (xhr) {
            xhr.forEach((item, index) => {
                let row = `<tr>
                                <td>${index + 1}</td>
                                <td>${item.userAccount}</td>
                                <td>₦${item.amountRequested}</td>
                                <td>${item.dateApplied}</td>
                                <td>${item.tenure} Month(s)</td>
                                <td>${getStatus(item.status)}</td>
                                <td>${item.dateApproved}</td>
                                <td><div class="badge badge-${item.repaid ? "info" : "danger"}"><i class="fa fa-${item.repaid ? "check" : "times"}"></i></div></td>
                                <td><div class="badge badge-${item.disbursed? "success" : "warning"}"><i class="fa fa-${(item.disbursed ? "check" : "times")}"></i></div></td>
                                <td>
                                    <div class="btn-group">
                                        <i class="fas fa-ellipsis-v p-10" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"></i>
                                        <div class="dropdown-menu">
                                            <a class="dropdown-item" href="/admin/loans/MarkAsPaid/?loanId=${item.id}&transactionId=${transactionId}">Mark As Paid</a>
                                        </div>
                                    </div>
                                </td>
                            </tr>`

                $('#loan_modal .modal-body table').append(row)
            })
        },
        error: function (xhr) {

        }
    })
}

function getStatus(status) {
    switch (status) {
        case 0:
            return '<div class="badge badge-success">Approved</div>'
            break;
        case 1:
            return '<div class="badge badge-warning">Pending</div>'
            break;
        case 2:
            return '<div class="badge badge-danger">Rejected</div>'
            break;
        default:
            break;
    }
}

function getErrors(response) {
    let errors = []
    for (var key in response) {
        errors.push(response[key])
    }
    return errors.flat()
}