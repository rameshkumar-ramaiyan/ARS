angular.module("umbraco").controller("Usda.Topic.Picker", function ($scope, editorState) {
    if (editorState.current.id != null && editorState.current.id != '' && editorState.current.id != '0') {
        $.ajax({
            url: '/umbraco/usda/TopicPicker/Get/' + editorState.current.id,
            type: 'GET',
            dataType: 'json',
            async: true
        }).success(function (data) {
            $('.usda-topic-picker-select').show();
            $('.usda-topic-picker-message').html('');
            var jsonData = jQuery.parseJSON(data)
            $scope.list = jsonData;
        });

        $('.usda-topic-picker-select').show();
        $('.usda-topic-picker-message').html('Loading...');
        return '';

    } else {
        $('.usda-topic-picker-select').hide();
        return "";
    }
});

