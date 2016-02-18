var TopicPickerTemplate = {};

TopicPickerTemplate.getTitle = function (value, scope) {
	//this is the property model
    if (value) {
        $.get("/umbraco/usda/TopicPicker/GetTitle/" + value, function (data) {
            var jsonData = jQuery.parseJSON(data)
            $scope.NodeDownloadRequestsList = jsonData.NodeDownloadRequestsList;
        });

	    return value;
	}

	//if you wanted to get the name of the content instead, you'd have to get it from the server here since it's not in the model

	return "";
};