angular.module("umbraco").controller("Usda.InterLinks", function ($scope, editorState) {
    $.get("/umbraco/usda/InterLinks/Get/" + editorState.current.id, function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.InterLinkList = jsonData;
    });
});

