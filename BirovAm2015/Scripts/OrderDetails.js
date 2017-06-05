new Vue({
    el: '#order-details',

    data: {
        Products: [],
        Sizes: [],
        Quantities: [],
        edit: false,
        selectedProduct: null,
        selectedSize: null,
        selectedQty: null,
        stock: null,
        notice: null
    },

    methods: {
        addItem: function () {
            this.Sizes = [];
            this.Quantities = [];
            this.selectedProduct = null;
            this.selectedSize = null;
            this.selectedQty = null;
            var self = this;
            $.get("/Orders/GetProducts", function (products) {
                self.Products = products;
            })
        },

        addSize: function (orderId) {
            var self = this;
            $.get("/Orders/SizesForProductAdd", { productId: self.selectedProduct, oId: orderId }, function (sizes) {
                self.Sizes = sizes;
            })
        },

        getQuantity: function () {
            this.Quantities = [];
            var self = this;
            $.get("/Orders/ProductSizeStock", { pId: self.selectedProduct, sId: self.selectedSize }, function (stock) {
                for (x = 1; x <= (stock - 5); x++) {
                    self.Quantities.push(x);
                }
            })
        },

        editItem: function (pId, s1, odId, qty) {
            var self = this;
            for (x = 1; x < 100; x++) {
                self.Quantities.push(x);
            }
            $.get("/Orders/SizesForProduct", { productId: pId }, function (sizes) {
                self.Sizes = sizes;
            })
            this.selectedSize = s1;
            this.selectedQty = qty;
            this.edit = odId;

        },

        updateSize: function (orderDetailId, orderId) {
            var self = this;
            $.post("/Orders/UpdateSize", { odId: orderDetailId, sId: self.selectedSize, oId: orderId }, function (response) {
                self.notice = response;
                if (self.notice == "") {
                    window.location.reload();
                }
            });
            this.edit = null;
            this.Sizes = [];
            this.Quantities = [];
        },

        updateQuantity: function (orderDetailId) {
            var self = this;
            $.post("/Orders/UpdateQuantity", { odId: orderDetailId, qty: self.selectedQty }, function (response) {
                self.notice = response;
                if (self.notice == "") {
                    window.location.reload();
                }
            });
            this.edit = null;
            this.Sizes = [];
        }
    }
})

$(function () {
    $("#add-item").click(function () {
       // clearProductModal();
        //$("#submit-add").show();
        //$(".modal-title").text("Add Item");
        $("#add-item-modal").modal();
    });

    $(".delete-orderDetail").click(function () {
        var product = $(this).closest('tr').find('td:eq(1)').text();
        var size = $(this).closest('tr').find('td:eq(2)').text();
        if (!confirm("Do you want to delete item - " + product + " size " + size + " from this order?")) {
            return false;
        }
    });
})