using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class ContentImporterController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        [System.Web.Http.HttpPost]
        public Models.Import.Response Post([FromBody] dynamic json)
        {
            string apiKey = "E027CF8B-C5B8-45F6-A37B-979DB02A8544";

            Models.Import.Response response = new Models.Import.Response();

            Models.Import.Content contentObj = JsonConvert.DeserializeObject<Models.Import.Content>(json.ToString());

            try
            {

                if (contentObj != null)
                {
                    //Check object
                    if (true == string.IsNullOrWhiteSpace(contentObj.ApiKey))
                    {
                        response.Message = "API Key is missing.";
                    }
                    else if (contentObj.ApiKey != apiKey)
                    {
                        response.Message = "API Key is invalid.";
                    }
                    else
                    {
                        if (contentObj.Id == 0)
                        {
                            if (true == string.IsNullOrWhiteSpace(contentObj.Name))
                            {
                                response.Message = "Content Name is empty.";
                            }
                            else if (contentObj.ParentId <= 0)
                            {
                                response.Message = "Content Parent Id is invalid.";
                            }
                            else if (true == string.IsNullOrWhiteSpace(contentObj.DocType))
                            {
                                response.Message = "Content DocType is missing.";
                            }
                            else if (true == string.IsNullOrWhiteSpace(contentObj.Template))
                            {
                                response.Message = "Content Template is missing.";
                            }
                            else
                            {
                                // Insert Content
                                var content = _contentService.CreateContent(contentObj.Name, contentObj.ParentId, contentObj.DocType);

                                IEnumerable<ITemplate> allowedTemplates = content.ContentType.AllowedTemplates;
                                ITemplate selectedTemplate = null;

                                selectedTemplate = allowedTemplates.Where(p => p.Alias == contentObj.Template).FirstOrDefault();

                                if (selectedTemplate != null)
                                {
                                    content.Template = selectedTemplate;
                                }
                                else
                                {
                                    response.Message = "Template is not allowed for this DocType.";
                                    response.Success = false;

                                    return response;
                                }

                                if (contentObj.Properties != null && contentObj.Properties.Count > 0)
                                {
                                    foreach (Models.Import.Property property in contentObj.Properties)
                                    {
                                        content.SetValue(property.Key, property.Value);
                                    }
                                }

                                if (contentObj.Save == 0)
                                {
                                    response.Message = "Cannot unpublish new content. Set Save = 1 or 2.";
                                    return response;
                                }
                                else if (contentObj.Save == 1)
                                {
                                    _contentService.Save(content);

                                    contentObj.Id = content.Id;

                                    response.Content = contentObj;
                                    response.Success = true;
                                    response.Message = "Content saved.";
                                }
                                else if (contentObj.Save == 2)
                                {
                                    _contentService.SaveAndPublishWithStatus(content);

                                    contentObj.Id = content.Id;

                                    response.Content = contentObj;
                                    response.Success = true;
                                    response.Message = "Content saved and published.";
                                }
                                else
                                {
                                    response.Message = "Invalid save option.";
                                    return response;
                                }
                            }
                        }
                        else
                        {
                            // Update Content
                            IContent contentGet = _contentService.GetById(contentObj.Id);

                            if (contentGet != null)
                            {
                                if (false == string.IsNullOrWhiteSpace(contentObj.Name))
                                {
                                    //TODO: Finish
                                }
                            }
                            else
                            {
                                response.Message = "Could not find content with Id: " + contentObj.Id;
                            }

                            response.Success = true;
                        }
                    }


                }
                else
                {
                    response.Message = "The JSON object was not properly formatted.";
                    response.Success = false;
                }

            }
            catch (Exception ex)
            {
                //LogHelper.Error<DataImporterController>("Content Import Post Error", ex);

                response.Message = ex.ToString();
            }

            return response;
        }
    }
}
