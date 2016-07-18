/**
 * @ngdoc controller
 * @name Our.Umbraco.DeleteAllChildrenController
 * @function
 * 
 * @description
 * The controller for deleting all child content (based on Umbraco's own DeleteController)
 */
function DeleteAllChildrenController($scope, deleteAllChildrenResource, treeService, navigationService, editorState, $location, dialogService, notificationsService) {
    $scope.confirmed = false;

    $scope.performDelete = function () {

        if ($scope.confirmed) {
            //mark it for deletion (used in the UI)
            $scope.currentNode.loading = true;

            deleteAllChildrenResource.deleteAllChildren($scope.currentNode.id, $scope.currentNode.section).then(function (response) {
                $scope.currentNode.loading = false;
                $scope.currentNode.hasChildren = false;
                treeService.removeChildNodes($scope.currentNode);

                var rootNode = treeService.getTreeRoot($scope.currentNode);

                if (rootNode) {
                    //ensure the recycle bin has child nodes now            
                    var recycleBin = treeService.getDescendantNode(rootNode, -20);
                    if (recycleBin) {
                        recycleBin.hasChildren = true;
                    }
                }

                if (editorState.current) {
                    var path = editorState.current.path;

                    //if the current edited item lives under the selected node, we need to redirect the user
                    if (path.split(',').indexOf($scope.currentNode.id) > -1) {
                        var location = "/" + $scope.currentNode.section + "/" + $scope.currentNode.section + "/edit/" + $scope.currentNode.id;
                        $location.path(location);
                    }
                }

                navigationService.hideMenu();
            },
            function (err) {
                $scope.currentNode.loading = false;

                //check if response is ysod
                if (err.status && err.status >= 500) {
                    dialogService.ysodDialog(err);
                }

                if (err.data && angular.isArray(err.data.notifications)) {
                    for (var i = 0; i < err.data.notifications.length; i++) {
                        notificationsService.showNotification(err.data.notifications[i]);
                    }
                }
            });
        }
    };

    $scope.cancel = function () {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Our.Umbraco.DeleteAllChildrenController", DeleteAllChildrenController);