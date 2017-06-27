/**
 * Created by igmacbook on 6/25/17.
 */

(function ($) {

    "use strict";

    $(document).ready(function () {

        // CLIPBOARD
        var clipboard = new Clipboard('.btn-copy');

        clipboard.on('success', function (e) {
            e.clearSelection();
        });

        $(function () {
            $('[data-toggle="popover"]').popover();
        });

        $('.input-group-btn').mouseout(function () {
            $(this).find(".popover").remove();
        });



    });


}(jQuery));

