angular.module("umbraco").controller("Usda.NewsTopic.Picker", function ($scope, editorState) {
    $.get("/umbraco/usda/NewsTopicPicker/Get/" + editorState.current.id, function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.list = jsonData;
    });
});

