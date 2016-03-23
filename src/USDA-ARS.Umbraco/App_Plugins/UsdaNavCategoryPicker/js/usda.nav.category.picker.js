angular.module("umbraco").controller("Usda.NavCategory.Picker", function ($scope, editorState) {
    $.get("/umbraco/usda/NavCategoryPicker/Get/" + editorState.current.id, function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.list = jsonData;
    });
});

