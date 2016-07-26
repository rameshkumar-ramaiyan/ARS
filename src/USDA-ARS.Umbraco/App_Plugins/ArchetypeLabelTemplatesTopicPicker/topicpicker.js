var TopicPickerTemplate = {};

TopicPickerTemplate.getTitle = function (value, scope) {
   if (id != null && id != '') {

      var cachedValue = _.find(TopicPickerTemplate.MyPrevalues, function (prevalue) {
         return prevalue.id == id;
      });

      if (cachedValue) {
         return cachedValue.value;
      }

      $.ajax({
         url: '/umbraco/usda/TopicPicker/GetTitle/' + id,
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


    //if you wanted to get the name of the content instead, you'd have to get it from the server here since it's not in the model
    return "";
};