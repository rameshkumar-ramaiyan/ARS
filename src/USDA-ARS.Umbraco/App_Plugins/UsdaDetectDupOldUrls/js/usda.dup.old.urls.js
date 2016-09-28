angular.module("umbraco").controller("Usda.Dup.Old.Urls", function ($scope, $http) {

	$.ajax({
		url: "/umbraco/usda/DupOldUrls/Get"
	}).done(function (data) {
		var htmlStr = data;
		$("#old-urls-text").html(htmlStr);
	});

});

