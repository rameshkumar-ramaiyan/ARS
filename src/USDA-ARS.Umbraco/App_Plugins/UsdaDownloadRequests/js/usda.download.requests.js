angular.module("umbraco").controller("Usda.Download.Requests", function ($scope, editorState) {
    $.get("/umbraco/usda/DownloadRequest/Get/" + editorState.current.id, function (data) {
        if (data != null && data != '') {
            var jsonData = jQuery.parseJSON(data)
            $scope.NodeDownloadRequestsList = jsonData.NodeDownloadRequestsList;
        }
    });

    $scope.clearRequests = function () {

    			var confirmYes = confirm('Ok to remove download requests?');

    			if (confirmYes) {
    						$.get("/umbraco/usda/DownloadRequest/Clear/" + editorState.current.id, function (data) {
    									if (data != null) {
    												$scope.NodeDownloadRequestsList = {};
    									}
    						});
    			}
    }
});

