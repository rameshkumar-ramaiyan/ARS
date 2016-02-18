using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class Topics
    {
        public static ArchetypeModel GetTopicsByGuid(Guid id, IEnumerable<IPublishedContent> nodeList)
        {

            if (nodeList != null && nodeList.Any())
            {
                foreach(IPublishedContent node in nodeList)
                {
                    if (true == node.HasProperty("leftNavCreate") && true == node.HasValue("leftNavCreate"))
                    {
                        ArchetypeModel topicLinks = node.GetPropertyValue<ArchetypeModel>("leftNavCreate");

                        if (topicLinks != null)
                        {
                            foreach (var topic in topicLinks)
                            {
                                if (topic.Id == id)
                                {
                                    if (true == topic.HasValue("customLeftNav"))
                                    {
                                        return topic.GetValue<ArchetypeModel>("customLeftNav");
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }

            return null;
        }
    }
}
