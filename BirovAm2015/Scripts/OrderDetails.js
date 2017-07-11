new Vue({
    el: '#order-details',

    data: {
        Products: [],
        Sizes: [],
        Quantities: [],
        PaymentRecords: [],
        edit: false,
        selectedProduct: null,
        selectedSize: null,
        selectedQty: null,
        stock: null,
        notice: null,
        addingPayment: false,
        ccInfo: null,
        expDate: null,
        code: null,
        amount: null,
        response: null
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
                //self.notice = response;
                if (response != "") {
                    alert(response)
                }
                if (response == "") {
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
                //self.notice = response;
                if (response != "") {
                    alert(response)
                }
                if (response == "") {
                    window.location.reload();
                }
            });
            this.edit = null;
            this.Sizes = [];
        },

        cancel: function () {
            this.edit = null;
            this.Sizes = [];
            this.Quantities = [];
        },

        getPaymentRecords: function (oId) {
            this.response = null;
            var self = this;
            $.get("/Orders/GetPaymentRecords", { oId: oId }, function (records) {
                self.PaymentRecords = records;
            })
        },

        getDate: function(date){
            return new Date(parseInt(date.substr(6))).toLocaleDateString('en-us');
        },

        submitPayment: function (oId) {
            this.response = null;
            var self = this;
            $.post("/Orders/SubmitPayment", { ccInfo: self.ccInfo, expDate: self.expDate, amount: self.amount, code: self.code, oId: oId }, function (response) {
                this.response = response;
                this.clearPaymentModal();
            })
        },

        clearPaymentModal: function () {
            this.ccInfo = null;
            this.expDate = null;
            this.code = null;
            this.amount = null;
            this.addingPayment = false;
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

    $("#see-add-payments").click(function () {
        // clearProductModal();
        //$("#submit-add").show();
        //$(".modal-title").text("Add Item");
        $("#payments-modal").modal();
    });

    $(".delete-orderDetail").click(function () {
        var product = $(this).closest('tr').find('td:eq(1)').text();
        var size = $(this).closest('tr').find('td:eq(2)').text();
        if (!confirm("Do you want to delete item - " + product + " size " + size + " from this order?")) {
            return false;
        }
    });
})