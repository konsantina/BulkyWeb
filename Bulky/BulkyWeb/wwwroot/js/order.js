var dataTable;

$(document).ready(function () {
    loadDataTable("all"); // Default φόρτωση με "all"

    $(".list-group-item").click(function (e) {
        e.preventDefault();
        var status = $(this).parent().data("status"); // Λήψη status από το data-status του <a>
        loadDataTable(status);
    });
});


function loadDataTable(status) {
    if (dataTable) {
        dataTable.destroy(); // Καταστροφή του υπάρχοντος DataTable για ανανέωση
    }

    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": '/admin/order/getall?status=' + status,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "applicationUser.name", "width": "15%" },
            { "data": "applicationUser.phoneNumber", "width": "20%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "10%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group"> 
                                <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                                <a onClick=Delete("/admin/order/delete/${data}") class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i></a>
                            </div>`;
                },
                "width": "25%"
            }
        ]
    });
}
