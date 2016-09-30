function usdaNewsPicker($scope, $http, $routeParams, $timeout, dialogService, usdaNewsPickerResource) {

	$scope.openPicker = function () {

		var id = $routeParams.id;

		usdaNewsPickerSearchDialog = dialogService.open({
			template: '/App_Plugins/NewsPicker/views/usda.news.picker.find.dialog.html',
			callback: populate
		});
	};

	$scope.remove = function (item) {
		var nodeIds = [];

		nodeIds = $scope.model.value.split(',');

		if (nodeIds) {
			var findIndex = nodeIds.indexOf(item.Id.toString());
			if (findIndex >= 0) {
				nodeIds.splice(findIndex, 1);

				$scope.model.value = nodeIds.toString()
			}
		}
		else {
			$scope.model.value = '';
		}
		
		getNodeDetails($scope.model.value);
	};

	function populate(data) {
		if ($scope.model.value && $scope.model.value != '') {
			$scope.model.value += "," + data;
		}
		else {
			$scope.model.value = data;
		}

		getNodeDetails($scope.model.value);
	};


	function getNodeDetails(data) {
		usdaNewsPickerResource.getNodes($scope.model.value).then(function (response) {
			if (response && response.data) {
				$scope.renderModel = response.data;

				var storeValue = '';

				for (var i = 0; i < response.data.length; i++) {
					if (i > 0) {
						storeValue += ',';
					}

					storeValue += response.data[i].Id;
				}

				$scope.model.value = storeValue;
			}
		});
	}

	getNodeDetails($scope.model.value);

};
angular.module('umbraco').controller('Usda.News.Picker', usdaNewsPicker);


function usdaSearchDialog($scope, $timeout, localizationService, searchService, $q, usdaNewsPickerResource) {
	var nodeIds = [];

	$scope.nodeArray = [];
	$scope.selectedYearsModel = '';

	usdaNewsPickerResource.getYears().then(function (response) {
		if (response && response.data) {

			$scope.yearsModel = response.data;
		}
	});

	$scope.yearListChanged = function (item) {
		usdaNewsPickerResource.getResults($scope.searchText, item).then(function (response) {
			if (response && response.data) {

				$scope.results = response.data;
			}
		});
	};


	$scope.selectResultCallback = function ($event, id) {
		var findIndex = nodeIds.indexOf(id);
		var $icon = $("#news-picker-icon" + id);

		$icon.removeClass("icon-newspaper");
		$icon.removeClass("icon-check");
		$icon.removeClass("blue");

		if (findIndex >= 0) {
			nodeIds.splice(findIndex, 1);
			$icon.addClass("icon-newspaper");
		}
		else {
			nodeIds.push(id);
			$icon.addClass("icon-check");
			$icon.addClass("blue");
		}

		$scope.nodeArray = nodeIds;
	};


	$scope.multiSubmit = function () {
		$scope.submit(nodeIds.toString());
	};

	// This is what you will bind the filter to

	// Instantiate these variables outside the watch
	var filterTextTimeout;
	$scope.$watch('searchText', function (val) {
		if (filterTextTimeout) {
			$timeout.cancel(filterTextTimeout);
		}

		filterTextTimeout = $timeout(function () {
			usdaNewsPickerResource.getResults(val, $scope.selectedYearsModel).then(function (response) {
				if (response && response.data) {

					$scope.results = response.data;
				}
			});
		}, 200); // delay 200 ms
	});




};
angular.module('umbraco').controller("Usda.News.Picker.Dialog", usdaSearchDialog);