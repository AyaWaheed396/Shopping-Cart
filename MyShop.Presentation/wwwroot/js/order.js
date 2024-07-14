var dTable;
$(document).ready(function () {
    loadData();
});

function loadData() {
    dTable = $("#myTable").DataTable({
        "ajax": {
            "url": "/Admin/Order/GetData",
            //"type": "GET",
            //"datatype": "json",
            //"dataSrc": function (json) {
            //    return json.data;
            //}
        },
        "columns": [
            { "data": "id" },
            { "data": "name" },
            { "data": "phone" },
            { "data": "applicationUser.email" },
            { "data": "orderStatus" },
            { "data": "totalPrice" },
            {
                "data": "id",
                "render": function (data/*, type, row*/) {
                    return `<a href='/Admin/Order/Details?orderid=${data}' class='btn btn-sm btn-warning'>Details</a>`
                }
                //,
                //"orderable": false
            }
        ]
    });
}


function DeleteItem(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "Delete",
                success: function (data) {
                    if (data.success) {
                        dTable.ajax.reload();
                        toaster.success(data.message);
                    }
                    else { toaster.error(data.message); }

                }
            });
            Swal.fire({
                title: "Deleted!",
                text: "Your file has been deleted.",
                icon: "success"
            });
        }
    });
}

