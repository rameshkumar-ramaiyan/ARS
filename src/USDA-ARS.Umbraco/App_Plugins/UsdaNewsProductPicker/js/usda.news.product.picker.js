angular.module("umbraco").controller("Usda.NewsProduct.Picker", function ($scope, editorState) {
    $.ajax({
        url: '/umbraco/usda/NewsProductPicker/Get/' + editorState.current.id,
        type: 'GET',
        dataType: 'json',
        async: true
    }).success(function (data) {
        $('.usda-newsproduct-picker-select').show();
        $('.usda-newsproduct-picker-message').html('');
        var jsonData = jQuery.parseJSON(data)
        $scope.list = jsonData;
    });

    $('.usda-newsproduct-picker-select').show();
    $('.usda-newsproduct-picker-message').html('Loading list...');
    return '';
});

