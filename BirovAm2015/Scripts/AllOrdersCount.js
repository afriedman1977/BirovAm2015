$(function () {
    $("#search-item").on('input', function () {
        $("#products-count tr:gt(0)").each(function () {
            var row = $(this);
            if ($("#search-size").val() === "") {
                $(this).find("td:eq(0)").text().toLowerCase().search($("#search-item").val().toLowerCase()) < 0 && $("#search-item").val() != "" ? $(this).hide() : $(this).show();
            } else {
                var x = row.find("td:eq(0)").text().search($("#search-item").val()) < 0;
                var y = row.find("td:eq(1)").text().toLowerCase().search($("#search-size").val().toLowerCase()) < 0;
                if (!y && !x) {
                    row.show();
                } else {
                    row.hide();
                }
            }
        });
    });

    $("#search-size").on('input', function () {
        $("#products-count tr:gt(0)").each(function () {
            var row = $(this);
            if ($("#search-item").val() === "") {
                row.find("td:eq(1)").text().toLowerCase().search($("#search-size").val().toLowerCase()) < 0 && $("#search-size").val() != "" ? $(this).hide() : $(this).show();
            } else {
                var x = row.find("td:eq(0)").text().search($("#search-item").val()) < 0;
                var y = row.find("td:eq(1)").text().toLowerCase().search($("#search-size").val().toLowerCase()) < 0;
                if (!y && !x) {
                    row.show();
                } else {
                    row.hide();
                }
            }
        });
    });

    $("#clear").click(function () {
        $("#products-count tr:gt(0)").each(function () {
            $(this).show();
        })
        $("#search-item").val("");
        $("#search-size").val("");
    })

    $('.order-modal').click(function () {
        $('#products-ordered-modal').modal();
    })
});

new Vue({
    el: '#products-ordered-count',

    data: {
        orderNumbers: [],
    },

    methods: {
        getOrderNumbers: function (code, size) {
            var self = this;
            $.get('/Orders/OrdersThatHaveProduct', { code: code, size: size }, function (data) {
                self.orderNumbers = data;
            })
        }
    }
});