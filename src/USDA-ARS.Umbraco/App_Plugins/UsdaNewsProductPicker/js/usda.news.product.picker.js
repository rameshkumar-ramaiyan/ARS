angular.module("umbraco").controller("Usda.NewsProduct.Picker", function ($scope, editorState) {
    $.get("/umbraco/usda/NewsProductPicker/Get/" + editorState.current.id, function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.list = jsonData;
    });
});

