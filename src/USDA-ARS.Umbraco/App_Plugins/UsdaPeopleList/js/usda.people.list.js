angular.module("umbraco").controller("Usda.People.List", function ($scope, editorState) {
    if (editorState.current.id != null && editorState.current.id != '' && editorState.current.id != '0') {
        $.ajax({
            url: '/umbraco/usda/PeopleList/Get/' + editorState.current.id,
            type: 'GET',
            dataType: 'json',
            async: true
        }).success(function (data) {
            $('.usda-people-picker-select').show();
            $('.usda-people-picker-message').html('');
            var jsonData = jQuery.parseJSON(data)
            $scope.people = jsonData;
        });

        $('.usda-people-picker-select').show();
        $('.usda-people-picker-message').html('Loading...');
        return '';

    } else {
        $('.usda-people-picker-select').hide();
        $('.usda-people-picker-message').html('Please save the page first before selecting a person.');
        return "";
    }

});

