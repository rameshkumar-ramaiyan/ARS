angular.module('umbraco.resources').factory('usdaNewsPickerResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
	return {

		getResults: function (query, year) {
			return $http.get("backoffice/usda/newspicker/Get?query=" + escape(query) + "&year=" + year, {
				params: {}
			});
		},

		getNodes: function (nodeIds) {
			return $http.get("backoffice/usda/newspicker/NodeList?nodeids=" + escape(nodeIds), {
				params: {}
			});
		},

		getYears: function () {
		return $http.get("backoffice/usda/newspicker/YearsList", {
			params: {}
		});
	}
	};
})

