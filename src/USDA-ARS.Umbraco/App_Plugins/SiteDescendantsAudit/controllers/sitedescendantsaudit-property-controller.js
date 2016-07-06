function siteDescendantsAuditPropertyController($scope, $routeParams, $location, $log, $timeout, editorState, siteDescendantsAuditResource) {

    siteDescendantsAuditResource.getAudit(editorState.current.key, $scope.model.config.siteDescendantContentTypes).then(function (response) {
        if (response && response.data) {
            
            $scope.records = response.data;
            
            $('.descendants-loading-message').hide();

            $timeout(function () {
                $('.descendants-audit-table').treetable({ expandable: true, initialState: 'expanded' });
            }, 0);
        }
    });

};
angular.module('umbraco').controller('SiteDescendantsAudit.Property.Controller', siteDescendantsAuditPropertyController);