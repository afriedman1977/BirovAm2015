$(function () {
    $("#add-customer").click(function () {
        clearCustomerModal();
        $("#submit-add").show();
        $("#submit-edit").hide();
        $(".modal-title").text("Add Customer");
        $("#add-customer-modal").modal();
    });

    $(".editCustomer").click(function () {
        var id = $(this).closest('tr').data('customer-id');
        $.get("/Customers/GetCustomerById?custId=" + id, function (result) {
            $("#first-name").val(result.FirstName)
            $("#last-name").val(result.LastName);
            $("#address").val(result.Address);
            $("#phone-number").val(result.PhoneNumber);
            $("#customer-id").val(result.CustomerID);
            $("#Message-URL").val(result.MessageURL);
            $("#submit-add").hide();
            $("#submit-edit").show();
            $(".modal-title").text("Edit Customer");
            $("#add-customer-modal").modal();
        });
    });

    $(".delete-customer").click(function () {
        var firstName = $(this).closest('tr').find('td:eq(0)').text();
        var lastName = $(this).closest('tr').find('td:eq(1)').text();
        if (!confirm("Do you want to delete " + firstName + " " + lastName)) {
            return false;
        }
    });

    $("#search").on('input', function () {
        $("#customers tr:gt(0)").each(function () {
            $(this).find("td:eq(3)").text().toLowerCase().search($("#search").val().toLowerCase()) < 0 && $("#search").val() != "" ? $(this).hide() : $(this).show();
        });
    });

    $("#clear").click(function () {
        $("#customers tr:gt(0)").each(function () {
            $(this).show();
        })
        $("#search").val("");
    })

    function clearCustomerModal() {
        $("#first-name").val(null)
        $("#last-name").val(null);
        $("#address").val(null);
        $("#phone-number").val(null);
        $("#customer-id").val(null);
        $("#Message-URL").val(null);
    }

    $(".add-order").click(function () {
        var customer = $(this).closest('tr').find('td:eq(3)').text();
        if (!confirm("Do you want to add order for - " + customer )) {
            return false;
        }
    });
});