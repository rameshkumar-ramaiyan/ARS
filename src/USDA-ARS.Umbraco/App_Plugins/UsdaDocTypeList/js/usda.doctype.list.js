angular.module("umbraco").controller("Usda.DocType.List", function ($scope, editorState) {
    $.get("/umbraco/usda/DocTypeList/Get/", function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.doctypes = jsonData;
    });
});

