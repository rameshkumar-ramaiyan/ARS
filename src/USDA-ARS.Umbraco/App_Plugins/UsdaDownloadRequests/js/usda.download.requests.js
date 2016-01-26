angular.module("umbraco").controller("Usda.Download.Requests", function ($scope, editorState) {
    $.get("/umbraco/usda/DownloadRequest/Get/" + editorState.current.id, function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.NodeDownloadRequestsList = jsonData.NodeDownloadRequestsList;
    });
});

