document.addEventListener("DOMContentLoaded", function () {
    $('.datatable').DataTable({
        "paging": true,
        "searching": true,
        "ordering": true,
        "info": true,
        "lengthChange": true, 
        "pageLength": 7, // Default number of records per page
        "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
       // "dom": '<"d-flex justify-content-between"fB>rtip', // 🔹 Aligns search & button
        "buttons": []
    });
    $(".dataTables_filter input").addClass("rounded-search");
});
