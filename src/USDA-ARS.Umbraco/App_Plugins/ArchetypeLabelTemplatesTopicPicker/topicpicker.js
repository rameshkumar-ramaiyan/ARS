var TopicPickerTemplate = {};

TopicPickerTemplate.getTitle = function (value, scope) {
    //this is the property model
    var output = value;

    if (value) {
        return value.Text;
	}

    //if you wanted to get the name of the content instead, you'd have to get it from the server here since it's not in the model
    return "";
};