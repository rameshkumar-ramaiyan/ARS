angular.module("umbraco").controller("Usda.Template.List", function ($scope, editorState) {
    $.get("/umbraco/usda/TemplateList/Get/", function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.templates = jsonData;
    });
});

