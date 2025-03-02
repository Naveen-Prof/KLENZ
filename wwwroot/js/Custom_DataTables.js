document.addEventListener("DOMContentLoaded", function () {
    // Check if DataTable is already initialized
    if ($.fn.DataTable.isDataTable('.datatable')) {
        $('.datatable').DataTable().destroy(); // Destroy previous instance
    }

    // Initialize DataTable
    let table = $('.datatable').DataTable({
        "paging": false,
        "searching": true,
        "ordering": false,
        "info": false,
        "lengthChange": false,
        "pageLength": 7,
        "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
        "buttons": []
    });

    // Apply styling after DataTable is initialized
    table.on('init', function () {
        $(".dataTables_filter input").addClass("rounded-search");
    });
});
