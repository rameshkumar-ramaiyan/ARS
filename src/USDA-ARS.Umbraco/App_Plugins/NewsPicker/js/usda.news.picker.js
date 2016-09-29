function usdaNewsPicker($scope, $http, $routeParams, $timeout, dialogService, usdaNewsPickerResource) {

	$scope.openPicker = function () {

		var id = $routeParams.id;

		usdaNewsPickerSearchDialog = dialogService.open({
			template: '/App_Plugins/NewsPicker/views/usda.news.picker.find.dialog.html',
			callback: populate
		});
	};

	$scope.remove = function () {
		$scope.model.value = '';
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


function usdaSearchDialog($scope, localizationService, searchService, $q, usdaNewsPickerResource) {
	var nodeIds = [];

	$scope.nodeArray = [];

	//scope.$watch("term", _.debounce(function (newVal, oldVal) {
	//	scope.$apply(function () {
	//		if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
	//			performSearch();
	//		}
	//	});
	//}, 200));

	usdaNewsPickerResource.getResults('').then(function (response) {
		if (response && response.data) {

			$scope.results = response.data;
			//$scope.records = response.data;

			//$('.descendants-loading-message').hide();

			//$timeout(function () {
			//	$('.descendants-audit-table').treetable({ expandable: true, initialState: 'expanded' });
			//}, 0);
		}
	});


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

};
angular.module('umbraco').controller("Usda.News.Picker.Dialog", usdaSearchDialog);