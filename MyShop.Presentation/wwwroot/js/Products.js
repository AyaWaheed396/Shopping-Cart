//var dTable;
//$(document).ready(function () {
//    loadData();
//});

//function loadData() {

//    dTable = $("#myTable").DataTable({
//        "ajax" : {
//            "url": "Product/GetData"
//        },
//        "columns": [
//            {"data":"Name"},
//            { "data":"Description"},
//            { "data":"Price"},
//            {"data":"Category"}

//        ]

//    });
//}

var dTable;
$(document).ready(function () {
    console.log("Document ready, initializing data table");
    loadData();
});

function loadData() {
    console.log("Loading data for DataTable");
    dTable = $("#myTable").DataTable({
        "ajax": {
            "url": "/Admin/Product/GetData",
            "type": "GET",
            "datatype": "json",
            "dataSrc": function (json) {
                console.log("Data received:", json); // Log the returned data
                return json.data;
            }
        },
        "columns": [
            { "data": "name" },
            { "data": "description" },
            { "data": "price" },
            { "data": "category.name" },
            {
                "data": null,
                "render": function (data, type, row) {
                    return `<a href='/Admin/Product/Edit/${data.id}' class='btn btn-sm btn-primary'>Edit</a>
                            <a onClick=DeleteItem("/Admin/Product/DeleteProduct/${data.id}") class='btn btn-sm btn-danger'>Delete</a>`;
                },
                "orderable": false
            }
        ]
    });
    console.log("DataTable initialized");
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

