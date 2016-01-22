angular.module("umbraco").controller("Usda.Webtrends.Redirect", function ($scope, editorState) {
    var iframeSrc = $("#redirect-link").attr("href");
    iframeSrc += "?nodeid=" + editorState.current.id;
    $("#redirect-link").attr("href", iframeSrc);
});

