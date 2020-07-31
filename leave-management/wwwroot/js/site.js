// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#tblData').DataTable();

    $('#backToList').addClass('btn btn-primary').html('<i class="fa fa-arrow-left" aria-hidden="true"></i> Back to List');

    $('#btnDetailsEdit').addClass('btn btn-warning').html('<i class="fa fa-pencil" aria-hidden="true"></i> Edit');
});
