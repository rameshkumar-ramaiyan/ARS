angular.module('umbraco.resources').factory('siteDescendantsAuditResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {

        getAudit: function (id, aliases) {
            return $http.get("backoffice/SiteDescendantsAudit/SiteDescendantsAuditApi/GetAudit?id=" + id + '&aliases=' + aliases, {
                params: {}
            });
        }
    };
})

