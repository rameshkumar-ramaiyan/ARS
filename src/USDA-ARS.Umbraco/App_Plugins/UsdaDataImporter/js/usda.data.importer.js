angular.module("umbraco").controller("Usda.Data.Importer", function ($scope, dataImporterResource, editorState) {
    var iframeSrc = $("#data-importer").attr("src");
    iframeSrc += "?id=" + editorState.current.id;
    $("#data-importer").attr("src", iframeSrc);
});

