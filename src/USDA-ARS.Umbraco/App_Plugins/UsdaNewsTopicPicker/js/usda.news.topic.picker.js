angular.module("umbraco").controller("Usda.NewsTopic.Picker", function ($scope, editorState) {
    $.ajax({
        url: '/umbraco/usda/NewsTopicPicker/Get/' + editorState.current.id,
        type: 'GET',
        dataType: 'json',
        async: true
    }).success(function (data) {
        $('.usda-newstopic-picker-select').show();
        $('.usda-newstopic-picker-message').html('');
        var jsonData = jQuery.parseJSON(data)
        $scope.list = jsonData;
    });

    $('.usda-newstopic-picker-select').show();
    $('.usda-newstopic-picker-message').html('Loading list...');
    return '';
});

