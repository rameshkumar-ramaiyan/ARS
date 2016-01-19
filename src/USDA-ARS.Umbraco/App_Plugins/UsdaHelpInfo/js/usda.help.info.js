angular.module("umbraco").controller("Usda.Help.Info", function ($scope, $http) {
    //$http.get("/umbraco/usda/HelpInfo/Go").then(function (response) {
    //    var htmlStr = response.data;
    //    $scope.item = htmlStr;
    //});

    $.get("/umbraco/usda/HelpInfo/Go", function (data) {
        var htmlStr = data;
        $("#help-usda-text").html(htmlStr);
    });
});

