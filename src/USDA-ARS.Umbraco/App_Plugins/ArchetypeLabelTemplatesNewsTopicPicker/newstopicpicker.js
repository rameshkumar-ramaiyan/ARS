var NewsTopicPickerTemplate = {};

NewsTopicPickerTemplate.getTitle = function (value, scope) {
	//this is the property model
	if (value) {
		if (value[0]) {
		    if (value[0].text) {
				return value[0].text;
			}
		}
		
	}

	//if you wanted to get the name of the content instead, you'd have to get it from the server here since it's not in the model

	return "";
};