$(function () {
    $("#search").on('input', function () {
        $("#orders tr:gt(0)").each(function () {
            $(this).find("td:eq(3)").text().toLowerCase().search($("#search").val().toLowerCase()) < 0 && $("#search").val() != "" ? $(this).hide() : $(this).show();
        });
    });

    $("#clear").click(function () {
        $("#orders tr:gt(0)").each(function () {
            $(this).show();
        })
        $("#search").val("");
    })
});