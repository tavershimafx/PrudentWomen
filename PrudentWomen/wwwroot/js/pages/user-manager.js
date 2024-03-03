/**
 *
 * You can write your JS code here, DO NOT touch the default style file
 * because it will make it harder for you to update.
 * 
 */

"use strict";

function getUser(url) {
    $(".page-loader-wrapper").show()
    $.ajax({
        method: "GET",
        url: url,
        processData: false,
        async: false,
        success: function (xhr) {
            $("#modal-user-part")[0].reset()
            $("#modal-user-part [name='FirstName']").val(xhr.firstName)
            $("#modal-user-part [name='LastName']").val(xhr.lastName)
            $("#modal-user-part [name='Email']").val(xhr.email)
            $("#modal-user-part [name='Id']").val(xhr.id)
            $("#modal-user-part [name='UserName']").val(xhr.userName)
            $(".page-loader-wrapper").hide()
        },
        error: function (xhr) {
            requestError()
        }
    })
}

$('[data-user]').click(function (e) {
    e.preventDefault()
    $("#create_errors").empty()
    var me = $(this),
        fetch_fxn = me.data("fetch"), // the function to populate the form
        get_url = me.attr("href"); // the function to update the entity

    eval(`${fetch_fxn}('${get_url}')`);
    $("#user_modal").modal("show")
    $("#save_user").click(function (e) {
        
        // Form Data
        let form_data = $("#modal-user-part").serialize();
        let rl = $("#modal-user-part [name='Roles']").val()
        form_data += `&roleIds=${rl}`
        $(".page-loader-wrapper").show()
        $.ajax({
            method: "PUT",
            url: `/admin/users/update/?${form_data}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location = window.location;
            },
            error: function (xhr) {
                requestError()
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#create_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#create_errors").append(`<span>${errors[i]}</span><br />`)
                }
            }
        })
    })
});

$("[data-status]").click(function (e) {
    var me = $(this)
    var id = me.data("id")
    var status = me.data("status")

    $(".page-loader-wrapper").show()
    $.ajax({
        method: "GET",
        url: `/admin/users/change-status/?userId=${id}&status=${status}`,
        processData: false,
        async: false,
        success: function (xhr) {
            window.location = window.location;
        },
        error: function (xhr) {
            $(".page-loader-wrapper").hide()
            var errors = getErrors(xhr.responseJSON)
            let errList = ''
            for (var i = 0; i < errors.length; i++) {
                errList += errors[i] + '\n'
            }
            alert(errList)
        }
    })
})

$('#new_user').click(function (e) {
    e.preventDefault()
    $("#create_errors").empty()
    
    $("#modal-user-part")[0].reset()
    $("#user_modal").modal("show")
    $("#save_user").click(function () {
        // Form Data
        let form_data = $("#modal-user-part").serialize();
        let rl = $("#modal-user-part [name='Roles']").val()
        form_data += `&roleIds=${rl}`
        $(".page-loader-wrapper").show()
        $.ajax({
            method: "POST",
            url: `/admin/users/create/?${form_data}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location = window.location;
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#create_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#create_errors").append(`<span>${errors[i]}</span><br />`)
                }
                requestError()
            }
        })
    })
});

$('[data-delete-user]').click(function (e) {
    e.preventDefault()

    let id = $(this).data('delete-user');
    let tr = $(this).closest('tr')
    let usr = $(tr.children('[data-username]')[0]).data('username')
    let email = $(tr.children('td[data-email]')[0]).data('email')

    $("#del_usr").val(usr)
    $("#del_email").val(email)
    $("#del_id").val(id)

    $("#del_errors").empty()
    $("#delete_modal").modal("show")
    $("#delete_user").click(function () {
        // Form Data
        let dd = $("#del_id").val()
        $(".page-loader-wrapper").show()
        $.ajax({
            method: "DELETE",
            url: `/admin/users/delete-user/?userId=${dd}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location = window.location;
            },
            error: function (xhr) {
                $(".page-loader-wrapper").hide()
                var errors = getErrors(xhr.responseJSON)
                $("#del_errors").empty()
                for (var i = 0; i < errors.length; i++) {
                    $("#del_errors").append(`<span>${errors[i]}</span><br />`)
                }
                requestError()
            }
        })
    })
});

$('#new_role').click(function (e) {
    e.preventDefault()

    $("#modal-role-part")[0].reset()
    $("#role_modal").modal("show")
    $("#save_role").click(function () {
        // Form Data
        let form_data = $("#modal-role-part").serialize();
        $.ajax({
            method: "POST",
            url: `/admin/roles/create/?${form_data}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location = window.location;
            },
            error: function (xhr) {
                requestError()
            }
        })
    })
});

function getRole(url) {
    $.ajax({
        method: "GET",
        url: url,
        processData: false,
        async: false,
        success: function (xhr) {
            $("#modal-role-part")[0].reset()
            $("#modal-role-part [name='Name']").val(xhr.name)
            $("#modal-role-part [name='Id']").val(xhr.id)
        },
        error: function (xhr) {
            requestError()
        }
    })
}

$('[data-role]').click(function (e) {
    e.preventDefault()
    var me = $(this),
        fetch_fxn = me.data("fetch"), // the function to populate the form
        get_url = me.attr("href"); // the function to update the entity

    eval(`${fetch_fxn}('${get_url}')`);
    $("#role_modal").modal("show")
    $("#save_role").click(function (e) {
        var $this = $(this);

        // Form Data
        let form_data = $("#modal-role-part").serialize();
        $.ajax({
            method: "PUT",
            url: `/admin/roles/update/?${form_data}`,
            processData: false,
            async: false,
            success: function (xhr) {
                window.location = window.location;
            },
            error: function (xhr) {
                requestError()
            }
        })
    })
});


$('#new_codes').click(function (e) {
    e.preventDefault()

    $("#genrt-code")[0].reset()
    $("#generate_modal").modal("show")
    $("#generate_cds").click(function () {
        // Form Data
        let form_data = $("#genrt-code").submit();
    })
});

$('#download_cds').click(function (e) {
    e.preventDefault()

    $.ajax({
        method: "GET",
        url: `/admin/regcode/downloadUnused`,
        processData: false,
        async: false,
        success: function (xhr) {
            download(xhr, 'registration-codes.csv', 'text/csv')
        },
        error: function (xhr) {
            requestError()
        }
    })
});

function download(data, filename, type) {
    var file = new Blob([data], { type: type });
    if (window.navigator.msSaveOrOpenBlob) // IE10+
        window.navigator.msSaveOrOpenBlob(file, filename);
    else { // Others
        var a = document.createElement("a"),
            url = URL.createObjectURL(file);
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        setTimeout(function () {
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        }, 0);
    }
}

function requestError() {
    $(this).fireModal({
        footerClass: 'bg-whitesmoke',
        body: 'An error occured performing your request.',
        buttons: [
            {
                text: 'Action',
                class: 'btn btn-primary btn-shadow',
                handler: function (modal) {
                }
            }
        ]
    });
}

function getErrors(response) {
    let errors = []
    for (var key in response) {
        errors.push(response[key])
    }
    return errors.flat()
}