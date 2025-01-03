﻿/**
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
    $("#iden_errors").empty()
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
                window.location.reload();
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
    $("#bulk_errors").html("")
    $("#items_sum").html("")

    $("#add_new_bulk").unbind('click')
    createAppendEvent()

    $("#submit_bulk").click(function (e) {
        e.preventDefault()
        $("#bulk_errors").empty()
        
        var bulkUsers = []
        $('#bulk_identify_modal form .modal-body').children('div.row').each((index, item) => {
            let user = {
                Username: $($(item).find('[name="username"]')[0]).val(),
                Amount: Number($($(item).find('.item-amount')[0]).val())
            } 

            if (user.Username == undefined || user.Username.trim() == '') {
                $("#bulk_errors").html(`Please verify your entries and make sure a username field is not empty.`)
                return
            }

            if (user.Amount == NaN || user.Amount == 0) {
                $("#bulk_errors").html(`Please verify your entries and make sure all amounts are valid numbers.`)
                return
            }
            console.log("i ", index)
            bulkUsers.push(user)
        })

        let sumTotal = 0
        bulkUsers.forEach((a, i) => {
            sumTotal += a.Amount
        })

        if (Number(sumTotal) != Number(totalBulkAmount)) {
            $("#bulk_errors").html(`<p class="text-danger">
                The total amount entered does not match the amount made in the transaction.
                The total amount entered is <b>₦${formatMoney(sumTotal.toFixed(2))}</b> 
                while the transaction has <b>₦${formatMoney(totalBulkAmount)}</b></p>`)
            return
        }


        $("#bulk_errors").empty()
        $(".page-loader-wrapper").toggle()

        let data = { bulkUsers: bulkUsers }

        $.ajax({
            method: "POST",
            url: `/admin/banktransaction/BulkMap/?transactionId=${transactionId}`,
            data: data,
            dataType: 'json',
            success: function (xhr) {
                location.reload()
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
            location.reload();
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

$('[data-disburse]').click(function (e) {
    var me = $(this)
    let id = me.data("disburse")

    $("#disburse_id").val(id)
    $("#disburse_modal").modal("show")
    $("#disburse_errors").empty()
    $("#index_disburse").click(function (e) {
        e.preventDefault()
        //var me = $(this)
        //let id = me.data("id")

        $(".page-loader-wrapper").toggle()
        $.ajax({
            method: "GET",
            url: `/admin/Loans/disburse/?id=${id}`,
            processData: false,
            async: false,
            success: function (xhr) {
                location.reload();
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#disburse_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#disburse_errors").append(`<span>${errors[i]}</span><br />`)
                }
            }
        })
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

$('#reject_loan').click(function (e) {
    e.preventDefault()
    var me = $(this)
    let id = me.data("id")

    $("#ln_id").val(id)
    $("#reject_modal").modal("show")
})

function getUnpaidLoans(transactionId) {
    $.ajax({
        method: "GET",
        url: `/admin/Loans/GetUnpaidLoans/`,
        processData: false,
        async: false,
        success: function (xhr) {
            $('#loan_modal .modal-body table').html("")
            $('#loan_modal .modal-body table').html(`<tr>
                                <th>#</th>
                                <th>User</th>
                                <th>Amount Requested</th>
                                <th>Date Applied</th>
                                <th>Tenure</th>
                                <th>Status</th>
                                <th>Date Approved</th>
                                <th>Disbursed</th>
                                <th>Repaid</th>
                                <th>Action</th>
                            </tr>`)
            xhr.forEach((item, index) => {
                let row = `<tr>
                                <td>${index + 1}</td>
                                <td>${item.userAccount}</td>
                                <td>₦${(item.amountRequested / 100).toFixed(2)}</td>
                                <td>${new Date(item.dateApplied).toLocaleDateString()}</td>
                                <td>${item.tenure} Month(s)</td>
                                <td>${getStatus(item.status)}</td>
                                <td>${new Date(item.dateApproved).toLocaleDateString()}</td>
                                <td><div class="badge badge-${item.disbursed? "success" : "warning"}"><i class="fa fa-${(item.disbursed ? "check" : "times")}"></i></div></td>
                                <td><div class="badge badge-${item.repaid ? "info" : "danger"}"><i class="fa fa-${item.repaid ? "check" : "times"}"></i></div></td>
                                <td>
                                    <div class="btn-group">
                                        <i class="fas fa-ellipsis-v p-10" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"></i>
                                        <div class="dropdown-menu">
                                            <a class="dropdown-item" data-loanid="${item.id}" data-loantid="${transactionId}">Mark As Paid</a>
                                        </div>
                                    </div>
                                </td>
                            </tr>`

                $('#loan_modal .modal-body table').append(row)
            })
            createRepay()
        },
        error: function (xhr) {

        }
    })
}

function createRepay() {
    $('[data-loanid]').off('click')
    $('[data-loanid]').click(function (e) {
        var me = $(this)
        let loanId = me.data("loanid")
        let transactionId = me.data("loantid")

        $.ajax({
            method: "GET",
            url: `/admin/loans/MarkAsPaid/?transactionId=${transactionId}&loanId=${loanId}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location.reload();
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                let errMsg = ""
                for (var i = 0; i < errors.length; i++) {
                    errMsg += errors[i]
                }
                alert(errMsg)
            }
        })
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

$("#transfer_btn").click(function (e) {
    e.preventDefault()
    $("#trf_errors").empty()

    let accountId = location.search.split("?")[1].split("&")[0].split("=")[1]
    $("#balance_modal").modal("show")

    $('#set_amt').off('click')
    $('#set_amt').click(function (e) {

        let amount = $("#bal_amt").val()

        $.ajax({
            method: "POST",
            url: `/admin/UserTransaction/DebitAccount/?accountId=${accountId}&amount=${amount}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location.reload();
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#trf_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#trf_errors").append(`<span>${errors[i]}</span><br />`)
                }
            }
        })
    })
})

$("#open_bal").click(function (e) {
    e.preventDefault()
    $("#bal_errors").empty()

    let accountId = location.search.split("?")[1].split("&")[0].split("=")[1]
    $("#balance_modal").modal("show")

    $('#set_amt').off('click')
    $('#set_amt').click(function (e) {
        
        let amount = $("#bal_amt").val()

        $.ajax({
            method: "GET",
            url: `/admin/UserTransaction/SetOpeningBalance/?accountId=${accountId}&amount=${amount}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location.reload();
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#bal_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#bal_errors").append(`<span>${errors[i]}</span><br />`)
                }
            }
        })
    })
})

function getErrors(response) {
    let errors = []
    for (var key in response) {
        errors.push(response[key])
    }
    return errors.flat()
}