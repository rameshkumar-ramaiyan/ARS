angular.module("umbraco").controller("Usda.DocType.List", function ($scope, editorState) {
    $.ajax({
        url: '/umbraco/usda/DocTypeList/Get/',
        type: 'GET',
        dataType: 'json',
        async: true
    }).success(function (data) {
        $('.usda-doctype-picker-select').show();
        $('.usda-doctype-picker-message').html('');
        var jsonData = jQuery.parseJSON(data)
        $scope.doctypes = jsonData;
    });

    $('.usda-doctype-picker-select').show();
    $('.usda-doctype-picker-message').html('Loading list...');
    return '';
});

