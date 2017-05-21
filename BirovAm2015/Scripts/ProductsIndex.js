$(function () {
    $("#add-category").click(function () {
        $("#submit-add-category").show();
        $("#submit-edit-category").hide();
        $(".modal-title-category").text("Add Category");
        $("#add-category-modal").modal();
    });

    $("#add-product").click(function () {
        clearProductModal();
        $("#submit-add").show();
        $("#submit-edit").hide();
        $(".modal-title").text("Add Product");
        $("#add-product-modal").modal();
    });

    $("#add-size").click(function () {
        $("#submit-add-category").show();
        $("#submit-edit-category").hide();
        $(".modal-title-category").text("Add Category");
        $("#add-size-modal").modal();
    });

    $(".editProduct").click(function () {
        $("#sound-file").val(null);
        var id = $(this).closest('tr').data('product-id');
        $.get("/Products/GetProductById?id=" + id, function (result) {
            $("#product-code").val(result.ProductCode)
            $("#style-number").val(result.StyleNumber);
            $("#brand").val(result.Brand);
            $("#description").val(result.Description);
            $("#price").val(result.Price);
            $("#sound-file").val(result.SoundFilePath);
            $("#product-id").val(result.ProductID);
            $('#category').val(result.CategoryID);
            $("#submit-add").hide();
            $("#submit-edit").show();
            $(".modal-title").text("Edit Product");
            $("#add-product-modal").modal();
        });
    });

    $(".editSize").click(function () {
        var id = $(this).closest('tr').data('size-id');
        $.get("/Products/GetSizeById?id=" + id, function (result) {
            $("#size-code").val(result.SizeCode)
            $("#size1").val(result.Size1);
            $("#size-id").val(result.SizeID);
            populate(result.Categories);
            $("#submit-add-size").hide();
            $("#submit-edit-size").show();
            $(".modal-title-size").text("Edit Size");
            $("#add-size-modal").modal();
        });
    });

    $(".editCategory").click(function () {
        var id = $(this).closest('tr').data('category-id');
        $.get("/Products/GetCategoryById?id=" + id, function (result) {
            $("#category-name").val(result.CategoryName)
            $("#category-description").val(result.CategoryDescription);
            $("#category-id").val(result.CategoryID);
            $("#submit-add-category").hide();
            $("#submit-edit-category").show();
            $(".modal-title-category").text("Edit Category");
            $("#add-category-modal").modal();
        });
    });

    function populate(cats) {
        cats.forEach(function (c) {
            $("#category-" + c.CategoryID).attr("checked", true);
        });
    }

    function clearProductModal() {
        $("#product-code").val(null)
        $("#style-number").val(null);
        $("#brand").val(null);
        $("#description").val(null);
        $("#price").val(null);
        $("#product-id").val(null);
        $('#category').val(null);
        $("#sound-file").val(null);
    }
});