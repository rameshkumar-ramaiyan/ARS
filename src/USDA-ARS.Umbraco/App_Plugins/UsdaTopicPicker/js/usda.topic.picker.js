angular.module("umbraco").controller("Usda.Topic.Picker", function ($scope, editorState) {
    $.get("/umbraco/usda/TopicPicker/Get/" + editorState.current.id, function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.list = jsonData;
        $(".topic-loading").hide();
    });
});

