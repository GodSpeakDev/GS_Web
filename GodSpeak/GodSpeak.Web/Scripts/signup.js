(function ($) {

    "use strict";

    $(document).ready(function () {



        $(".dropdown-menu li a").click(function (e) {

            e.preventDefault();

            $("#dropdown-value").text($(this).text());
            $("#dropdown-value").val($(this).text());

        });


    });


}(jQuery));
