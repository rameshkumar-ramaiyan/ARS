<!DOCTYPE html>
<html>
<head>
    <title>Image</title>
    <meta charset="utf-8" />
    <script type="text/javascript" src="/umbraco/lib/jquery/jquery.min.js"></script>
    <script>
        $( document ).ready(function() {
            $('#usda-map-image').click(function (e) {
                var mapImage = $(this)

                var offset_t = mapImage.offset().top - $(window).scrollTop();
                var offset_l = mapImage.offset().left - $(window).scrollLeft();

                var left = Math.round((e.clientX - offset_l));
                var top = Math.round((e.clientY - offset_t));

                var $emptyInput = window.opener.$('input[id*=mapCoordinates]').filter(function () { return !this.value; });

                $emptyInput.val(left + ',' + top);
                $emptyInput.trigger('input');
            });
        });
    </script>

</head>
<body>
    <div>
        <img id="usda-map-image" src="<%= Request.QueryString.Get("url") %>" style="cursor: crosshair;" />
    </div>
</body>
</html>
