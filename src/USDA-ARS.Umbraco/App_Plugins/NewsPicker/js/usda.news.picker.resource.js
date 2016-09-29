angular.module('umbraco.resources').factory('usdaNewsPickerResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
	return {

		getResults: function (query) {
			return $http.get("backoffice/usda/newspicker/Get?query=" + escape(query), {
				params: {}
			});
		},

		getNodes: function (nodeIds) {
			return $http.get("backoffice/usda/newspicker/NodeList?nodeids=" + escape(nodeIds), {
				params: {}
			});
		}
	};
})

