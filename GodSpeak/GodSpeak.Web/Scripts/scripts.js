/**
 * Created by igmacbook on 6/25/17.
 */

(function ($) {

    "use strict";

    $(document).ready(function () {

        $('.collapse').on('shown.bs.collapse', function () {
            $(this).parent().find(".accordion-icon").removeClass("plus").addClass("minus");
        }).on('hidden.bs.collapse', function () {
            $(this).parent().find(".accordion-icon").removeClass("minus").addClass("plus");
        });

        $('#confirm-delete').on('show.bs.modal', function (e) {
            
            $(this).find('.btn-ok').attr('href', $(e.relatedTarget).data('href'));
        });
    });


}(jQuery));