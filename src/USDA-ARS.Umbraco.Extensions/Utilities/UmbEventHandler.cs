using Archetype.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Helpers.Aris;

namespace USDA_ARS.Umbraco.Extensions.Utilities
{
    public class UmbEventHandler : ApplicationEventHandler
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;


        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Created += PostProcessCreated;
            ContentService.Saved += PostProcessSave;
            ContentService.Deleted += PostProcessDelete;
        }

        private static void PostProcessCreated(IContentService cs, NewEventArgs<IContent> e)
        {
            IContent node = e.Entity;
            
        }


        private static void PostProcessSave(IContentService cs, SaveEventArgs<IContent> e)
        {
            foreach (var node in e.SavedEntities)
            {
                // SOFTWARE
                if (node.ContentType.Alias == "Homepage" || node.ContentType.Alias == "Region" || node.ContentType.Alias == "ResearchUnit")
                {
                    ArchetypeModel software = node.GetValue<ArchetypeModel>("software");

                    string archetypeStr = node.GetValue<string>("software");

                    if (software == null && false == string.IsNullOrEmpty(archetypeStr))
                    {
                        software = JsonConvert.DeserializeObject<ArchetypeModel>(archetypeStr);
                    }

                    bool updateSoftware = false;

                    if (software != null)
                    {
                        foreach (ArchetypeFieldsetModel fieldsetModel in software.Fieldsets)
                        {
                            string softwareId = fieldsetModel.GetValue<string>("softwareID");

                            if (true == string.IsNullOrEmpty(softwareId))
                            {
                                updateSoftware = true;

                                int softwareIdNext = Software.GetLastSoftwareId() + 1;

                                var newSoftwareID = fieldsetModel.Properties.FirstOrDefault(x => x.Alias == "softwareID");

                                if (newSoftwareID != null)
                                {
                                    newSoftwareID.Value = softwareIdNext;
                                }
                            }
                        }

                        if (true == updateSoftware)
                        {
                            node.SetValue("software", JsonConvert.SerializeObject(software));
                            _contentService.SaveAndPublishWithStatus(node);
                        }
                    }
                }


                /////////////
                // When a Doc Folder or Site Standard Webpage is Created...
                if ((node.ContentType.Alias == "DocsFolder" || node.ContentType.Alias == "SiteStandardWebpage"))
                {
                    // SET DEFAULT NAVIGATION CATEGORY
                    Property navCategory = node.Properties.Where(p => p.Alias == "navigationCategory").FirstOrDefault();
                    IContent parentNode = node.Parent();

                    if (navCategory.Value == null || true == string.IsNullOrEmpty(navCategory.Value.ToString()))
                    {


                        if (parentNode != null && parentNode.ContentType.Alias != "Region" && parentNode.ContentType.Alias != "ResearchUnit")
                        {
                            Property parentNavCategory = parentNode.Properties.Where(p => p.Alias == "navigationCategory").FirstOrDefault();

                            if (parentNavCategory.Value != null && false == string.IsNullOrEmpty(parentNavCategory.Value.ToString()))
                            {
                                navCategory.Value = parentNavCategory.Value;

                                _contentService.SaveAndPublishWithStatus(node);
                            }
                        }
                    }


                    // UPDATE SORT ORDER
                    //if (parentNode.ContentType.Alias == "Region" || parentNode.ContentType.Alias == "ResearchUnit" || parentNode.ContentType.Alias == "City")
                    //{
                    //    int sortOrder = 0;

                    //    // 
                    //    List<IContent> docsAndFoldersList = parentNode.Children().Where(p => p.ContentType.Alias != "Region"
                    //            && p.ContentType.Alias != "ResearchUnit"
                    //            && p.ContentType.Alias != "City").OrderBy(s => s.Name).ToList();

                    //    if (docsAndFoldersList != null && docsAndFoldersList.Any())
                    //    {
                    //        foreach (IContent subNode in docsAndFoldersList)
                    //        {
                    //            subNode.SortOrder = sortOrder;

                    //            if (true == subNode.Published)
                    //            {
                    //                _contentService.SaveAndPublishWithStatus(node);
                    //            }
                    //            else
                    //            {
                    //                _contentService.Save(node);
                    //            }

                    //            sortOrder++;
                    //        }
                    //    }


                    //    List<IContent> nonDocsAndFoldersList = null;

                    //    if (parentNode.ContentType.Alias == "City" || parentNode.ContentType.Alias == "ResearchUnit")
                    //    {
                    //        nonDocsAndFoldersList = parentNode.Children().Where(p => p.ContentType.Alias == "ResearchUnit").ToList();

                    //        if (nonDocsAndFoldersList != null && nonDocsAndFoldersList.Any())
                    //        {
                    //            nonDocsAndFoldersList = nonDocsAndFoldersList.OrderBy(p => p.Name).ToList();
                    //        }
                    //    }

                    //    if (nonDocsAndFoldersList != null && nonDocsAndFoldersList.Any())
                    //    {
                    //        foreach (IContent subNode in nonDocsAndFoldersList)
                    //        {
                    //            subNode.SortOrder = sortOrder;

                    //            if (true == subNode.Published)
                    //            {
                    //                _contentService.SaveAndPublishWithStatus(node);
                    //            }
                    //            else
                    //            {
                    //                _contentService.Save(node);
                    //            }

                    //            sortOrder++;
                    //        }
                    //    }
                    //}
                }


                // SUB FOLDERS FOR REGIONS AND RESEARCH UNITS
                if ((node.ContentType.Alias == "Region" || node.ContentType.Alias == "ResearchUnit"))
                {
                    UmbracoHelper umbHelper = new UmbracoHelper(UmbracoContext.Current);
                    int siteFolderTemplateNodeId = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Usda:SiteFoldesrTemplateNodeId")); //

                    IPublishedContent siteFoldersTemplate = umbHelper.TypedContent(siteFolderTemplateNodeId);

                    if (siteFoldersTemplate != null)
                    {
                        List<IPublishedContent> siteFolderSubNodeList = siteFoldersTemplate.Children.ToList();

                        if (siteFolderSubNodeList != null && siteFolderSubNodeList.Any())
                        {
                            foreach (IPublishedContent subNode in siteFolderSubNodeList)
                            {
                                IContent testSubNode = node.Descendants().FirstOrDefault(n => n.Name == subNode.Name);

                                if (testSubNode == null)
                                {
                                    IContent createSubNode = _contentService.GetById(subNode.Id);

                                    if (createSubNode != null)
                                    {
                                        IContent newSubNode = _contentService.Copy(createSubNode, node.Id, false);

                                        if (newSubNode != null)
                                        {
                                            _contentService.PublishWithChildrenWithStatus(newSubNode);

                                            if (newSubNode != null && newSubNode.Children().Any())
                                            {
                                                foreach (IContent newSubSubNode in newSubNode.Children())
                                                {
                                                    _contentService.PublishWithChildrenWithStatus(newSubSubNode);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Check for File System Folder
                    if (false == string.IsNullOrEmpty(node.GetValue<string>("modeCode")))
                    {
                        string startFolder = ConfigurationManager.AppSettings.Get("Usda:UserFileFolderPath");
                        string modeCodeFolder = node.GetValue<string>("modeCode").Replace("-", "");
                        string fullPath = IOHelper.MapPath(startFolder.EnsureStartsWith("~/") + "/" + modeCodeFolder);

                        if (false == Directory.Exists(fullPath))
                        {
                            Directory.CreateDirectory(fullPath);

                            if (false == Directory.Exists(fullPath + "\\images"))
                            {
                                Directory.CreateDirectory(fullPath + "\\images");
                            }
                            if (false == Directory.Exists(fullPath + "\\software"))
                            {
                                Directory.CreateDirectory(fullPath + "\\software");
                            }
                            if (false == Directory.Exists(fullPath + "\\photoCarousel"))
                            {
                                Directory.CreateDirectory(fullPath + "\\photoCarousel");
                            }
                        }
                    }

                }
                else if (node.ContentType.Alias == "NationalProgram" && !cs.HasChildren(node.Id))
                {
                    var childNode = _contentService.CreateContent("Docs", node, "NationalProgramFolderContainer");

                    _contentService.SaveAndPublishWithStatus(childNode);
                }
                else if (node.ContentType.Alias == "NewsArticle")
                {
                    int nodeId = node.Id;

                    string bodyText = node.GetValue<string>("bodyText");

                    NewsInterLinks.RemoveLinksByNodeId(nodeId);

                    List<LinkItem> linkItemList = NewsInterLinks.FindInterLinks(bodyText);

                    if (linkItemList != null)
                    {
                        NewsInterLinks.GenerateInterLinks(node, linkItemList);
                    }
                }
            }
        }

        private static void PostProcessDelete(IContentService cs, DeleteEventArgs<IContent> e)
        {
            foreach (var node in e.DeletedEntities)
            {
                if (node.ContentType.Alias == "NewsArticle")
                {
                    NewsInterLinks.RemoveLinksByNodeId(node.Id);
                }
            }
        }
    }
}
