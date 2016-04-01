var NewsTopicPickerTemplate = {};
NewsTopicPickerTemplate.MyPrevalues = [];

NewsTopicPickerTemplate.getTitle = function (id, scope) {
    if (id != null && id != '') {

        var cachedValue = _.find(NewsTopicPickerTemplate.MyPrevalues, function (prevalue) {
            return prevalue.id == id;
        });

        if (cachedValue) {
            return cachedValue.value;
        }

        $.ajax({
            url: '/umbraco/usda/NewsTopicPicker/GetTitle/' + id,
            type: 'GET',
            dataType: 'json',
            async: true
        }).success(function (data) {
            NewsTopicPickerTemplate.MyPrevalues.push({ id: id, value: data });
            return data;
        });

        return "Loading...";

    } else {
        return " - ";
    }




	//this is the property model
    //if (value) {
    //    return $.ajax({
    //        type: "GET",
    //        url: "/umbraco/usda/NewsTopicPicker/GetTitle/" + value,
    //        async: false
    //    }).responseText;
	//}

    
};