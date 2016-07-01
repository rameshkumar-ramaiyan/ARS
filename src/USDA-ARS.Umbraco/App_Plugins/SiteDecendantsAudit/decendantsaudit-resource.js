angular.module('umbraco.resources').factory('decendantsAuditResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {

        getAudit: function (id, aliases) {
            return $http.get("backoffice/SiteDecendantsAudit/DecendantsAuditApi/GetAudit?id=" + id + '&aliases=' + aliases, {
                params: {}
            });
        }
    };
})

