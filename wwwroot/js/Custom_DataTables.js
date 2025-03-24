document.addEventListener("DOMContentLoaded", function () {
    // Check if DataTable is already initialized
    if ($.fn.DataTable.isDataTable('.datatable')) {
        $('.datatable').DataTable().destroy(); // Destroy previous instance
    }

    let table = $('.datatable').DataTable({
        "paging": true,
        "searching": true,
        "ordering": false,
        "info": false,
        "lengthChange": true,  
        "pageLength": 10,     
        "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]], 
        "buttons": [],
        "language": {
            "lengthMenu": "Show _MENU_ entries" // ✅ Custom text
        }
    });

    // Apply styling after DataTable is initialized
    table.on('init', function () {
        $(".dataTables_filter input").addClass("rounded-search me-3 mt-2");
    });
});
