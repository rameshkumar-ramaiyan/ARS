angular.module('umbraco.resources').factory("deleteAllChildrenResource", function ($q, $http, umbRequestHelper) {
    return {
        deleteAllChildren: function (parentId, section) {
            return umbRequestHelper.resourcePromise(
               $http.get("/Umbraco/backoffice/DeleteAllChildrenV7/DeleteAllChildrenV7/DeleteAllChildren?parentId=" + parentId + "&section=" + section),
               'Failed to retrieve data for content id ' + parentId);
        }
    }
});