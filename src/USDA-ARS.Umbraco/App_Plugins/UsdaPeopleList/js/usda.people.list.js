angular.module("umbraco").controller("Usda.People.List", function ($scope, editorState) {
    $.get("/umbraco/usda/PeopleList/Get/" + editorState.current.id, function (data) {
        var jsonData = jQuery.parseJSON(data)
        $scope.people = jsonData;
    });
});

