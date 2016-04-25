using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class Software
    {
        public static UmbracoHelper UmbHelper = new UmbracoHelper(UmbracoContext.Current);

        public static ArchetypeFieldsetModel GetSoftwareById(string id)
        {
            ArchetypeFieldsetModel archetypeFielsetModel = null;
            List<IPublishedContent> softwareList = new List<IPublishedContent>();

            softwareList = GetSoftwareNodes();

            if (softwareList != null && softwareList.Any())
            {
                archetypeFielsetModel = softwareList.SelectMany(p => p.GetPropertyValue<ArchetypeModel>("software")).Where(t => t.GetValue<string>("softwareId") == id).FirstOrDefault();
            }


            return archetypeFielsetModel;
        }


        public static int GetLastSoftwareId()
        {
            int softwareId = 0;

            List<IPublishedContent> softwareList = GetSoftwareNodes();

            if (softwareList != null && softwareList.Any())
            {
                foreach (IPublishedContent softwareNode in softwareList)
                {
                    ArchetypeModel archetypeModel = softwareNode.GetPropertyValue<ArchetypeModel>("software");

                    foreach (ArchetypeFieldsetModel fieldsetModel in archetypeModel.Fieldsets)
                    {
                        int intTry = 0;
                        string softwareIdStr = fieldsetModel.GetValue<string>("softwareID");

                        if (int.TryParse(softwareIdStr, out intTry))
                        {
                            if (softwareId < intTry)
                            {
                                softwareId = intTry;
                            }
                        }
                    }
                }
            }


            return softwareId;
        }


        public static List<ArchetypeModel> GetAllSoftware()
        {
            List<ArchetypeModel> softwareList = new List<ArchetypeModel>();

            List<IPublishedContent> softwareNodeList = GetSoftwareNodes();

            if (softwareNodeList != null && softwareNodeList.Any())
            {
                foreach (IPublishedContent node in softwareList)
                {
                    if (node != null && node.HasValue("software") && 
                            node.GetPropertyValue<ArchetypeModel>("software").Fieldsets != null && 
                            node.GetPropertyValue<ArchetypeModel>("software").Fieldsets.Count() > 0)

                    softwareList.Add(node.GetPropertyValue<ArchetypeModel>("software"));
                }
            }
            
            return softwareList;
        }


        public static List<IPublishedContent> GetSoftwareNodes()
        {
            List<IPublishedContent> softwareList = new List<IPublishedContent>();
            List<IPublishedContent> nodeList = null;

            foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
            {
                nodeList = root.Descendants().Where(n => n.HasValue("software") && false == string.IsNullOrEmpty(n.GetPropertyValue<string>("modeCode"))).ToList();

                if (nodeList != null && nodeList.Any())
                {
                    softwareList.AddRange(nodeList);
                }

                if (root.HasValue("software") && false == string.IsNullOrEmpty(root.GetPropertyValue<string>("modeCode")))
                {
                    softwareList.Add(root);
                }
            }

            if (softwareList != null && softwareList.Any())
            {
                softwareList = softwareList.OrderBy(p => p.GetPropertyValue<string>("modeCode")).ToList();
            }


            return softwareList;
        }

        public static string GetSoftwareLocation(IPublishedContent node)
        {
            string output = node.Name;

            if (node.DocumentTypeAlias == "ResearchUnit")
            {
                if (node.Level == 5)
                {
                    output += ", " + node.Parent.Parent.Name;
                }
                else if (node.Level == 4)
                {
                    output += ", " + node.Parent.Name;
                }
            }
            else if (node.DocumentTypeAlias == "Homepage")
            {
                output = "ARS, Washington, D.C.";
            }

            return output;
        }
    }
}
