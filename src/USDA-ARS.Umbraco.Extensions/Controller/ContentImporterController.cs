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
        private static string _apiKey = "E027CF8B-C5B8-45F6-A37B-979DB02A8544";

        [System.Web.Http.HttpPost]
        public Models.Import.Response Post([FromBody] dynamic json)
        {
            Models.Import.Response response = new Models.Import.Response();
            response.ContentList = new List<Models.Import.Content>();

            Models.Import.Request request = JsonConvert.DeserializeObject<Models.Import.Request>(json.ToString());

            try
            {
                if (request != null)
                {
                    //Check object
                    if (true == string.IsNullOrWhiteSpace(request.ApiKey))
                    {
                        response.Message = "API Key is missing.";
                    }
                    else if (request.ApiKey != _apiKey)
                    {
                        response.Message = "API Key is invalid.";
                    }
                    else
                    {
                        int i = 0;

                        response.Success = true;

                        foreach (Models.Import.Content contentObj in request.ContentList)
                        {
                            Models.Import.Content responseContent = contentObj;

                            if (contentObj.Id == 0)
                            {
                                if (true == string.IsNullOrWhiteSpace(contentObj.Name))
                                {
                                    responseContent.Message = "Content Name is empty. [" + i + "]";
                                    responseContent.Success = false;
                                }
                                else if (contentObj.ParentId <= 0)
                                {
                                    responseContent.Message = "Content Parent Id is invalid. [" + i + "]";
                                    responseContent.Success = false;
                                }
                                else if (true == string.IsNullOrWhiteSpace(contentObj.DocType))
                                {
                                    responseContent.Message = "Content DocType is missing. [" + i + "]";
                                    responseContent.Success = false;
                                }
                                else
                                {
                                    // Insert Content
                                    var content = _contentService.CreateContent(contentObj.Name, contentObj.ParentId, contentObj.DocType);

                                    if (false == string.IsNullOrWhiteSpace(contentObj.Template))
                                    {
                                        IEnumerable<ITemplate> allowedTemplates = content.ContentType.AllowedTemplates;
                                        ITemplate selectedTemplate = null;

                                        selectedTemplate = allowedTemplates.Where(p => p.Alias == contentObj.Template).FirstOrDefault();

                                        if (selectedTemplate != null)
                                        {
                                            content.Template = selectedTemplate;
                                        }
                                        else
                                        {
                                            responseContent.Message = "Template is not allowed for this DocType. [" + i + "]\r\n";
                                        }
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
                                        responseContent.Message = "Cannot unpublish new content. Set Save = 1 or 2. [" + i + "]";
                                    }
                                    else if (contentObj.Save == 1)
                                    {
                                        _contentService.Save(content);

                                        contentObj.Id = content.Id;

                                        responseContent = contentObj;
                                        responseContent.Success = true;
                                        responseContent.Message = "Content saved. [" + i + "]";
                                    }
                                    else if (contentObj.Save == 2)
                                    {
                                        _contentService.SaveAndPublishWithStatus(content);

                                        contentObj.Id = content.Id;

                                        responseContent = contentObj;
                                        responseContent.Success = true;
                                        responseContent.Message = "Content saved and published. [" + i + "]";
                                    }
                                    else
                                    {
                                        responseContent.Message = "Invalid save option. [" + i + "]";
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
                                        contentGet.Name = contentObj.Name;
                                    }
                                    if (contentObj.ParentId > 0)
                                    {
                                        contentGet.ParentId = contentObj.ParentId;
                                    }
                                    if (false == string.IsNullOrWhiteSpace(contentObj.Template))
                                    {
                                        IEnumerable<ITemplate> allowedTemplates = contentGet.ContentType.AllowedTemplates;
                                        ITemplate selectedTemplate = null;

                                        selectedTemplate = allowedTemplates.Where(p => p.Alias == contentObj.Template).FirstOrDefault();

                                        if (selectedTemplate != null)
                                        {
                                            contentGet.Template = selectedTemplate;
                                        }
                                        else
                                        {
                                            responseContent.Message = "Template is not allowed for this DocType. [" + i + "]";
                                            responseContent.Success = false;
                                        }
                                    }

                                    if (contentObj.Properties != null && contentObj.Properties.Count > 0)
                                    {
                                        foreach (Models.Import.Property property in contentObj.Properties)
                                        {
                                            contentGet.SetValue(property.Key, property.Value);
                                        }
                                    }

                                    if (contentObj.Save == 0)
                                    {
                                        _contentService.UnPublish(contentGet);

                                        responseContent = ConvertContentObj(contentGet);
                                        responseContent.Success = true;
                                        responseContent.Message = "Content updated and unpublished. [" + i + "]";
                                    }
                                    else if (contentObj.Save == 1)
                                    {
                                        _contentService.Save(contentGet);

                                        contentObj.Id = contentGet.Id;

                                        responseContent = ConvertContentObj(contentGet);
                                        responseContent.Success = true;
                                        responseContent.Message = "Content updated and saved. [" + i + "]";
                                    }
                                    else if (contentObj.Save == 2)
                                    {
                                        _contentService.SaveAndPublishWithStatus(contentGet);

                                        contentObj.Id = contentGet.Id;

                                        responseContent = ConvertContentObj(contentGet);
                                        responseContent.Success = true;
                                        responseContent.Message = "Content updated and published. [" + i + "]";
                                    }
                                    else
                                    {
                                        responseContent.Message = "Invalid save option. [" + i + "]";
                                    }
                                }
                                else
                                {
                                    responseContent.Message = "Could not find content with Id: " + contentObj.Id + " [" + i + "]";
                                }

                                response.ContentList.Add(responseContent);
                            }

                            i++;
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

        [System.Web.Http.HttpPost]
        public Models.Import.Response Get([FromBody] dynamic json)
        {
            Models.Import.Response response = new Models.Import.Response();

            Models.Import.Request request = JsonConvert.DeserializeObject<Models.Import.Request>(json.ToString());

            try
            {
                if (request != null)
                {
                    //Check object
                    if (true == string.IsNullOrWhiteSpace(request.ApiKey))
                    {
                        response.Message = "API Key is missing.";
                    }
                    else if (request.ApiKey != _apiKey)
                    {
                        response.Message = "API Key is invalid.";
                    }
                    else if (request.ContentList == null || request.ContentList.Count == 0)
                    {
                        response.Message = "Content object empty. Needed for GET.";
                    }
                    else
                    {
                        response.Success = true;
                        response.ContentList = new List<Models.Import.Content>();

                        foreach (Models.Import.Content contentObj in request.ContentList)
                        {
                            IContent content = null;
                            Models.Import.Content responseContent = new Models.Import.Content();

                            if (contentObj.Id <= 0)
                            {
                                if (contentObj.Properties != null && contentObj.Properties.Count > 0)
                                {
                                    response.ContentList = new List<Models.Import.Content>();
                                    Models.Import.Property propObj = contentObj.Properties[0];

                                    IEnumerable<IContent> rootNodeList = _contentService.GetRootContent();

                                    foreach (IContent rootNode in rootNodeList)
                                    {
                                        if (content == null)
                                        {
                                            IEnumerable<IContent> nodeList = _contentService.GetDescendants(rootNode.Id);

                                            content = nodeList.Where(p => p.Properties.Any(s => s.Value != null && s.Alias == propObj.Key && s.Value.ToString() == propObj.Value.ToString())).FirstOrDefault();
                                        }
                                    }
                                }
                                else
                                {
                                    responseContent.Message = "Must provide a property key and value to return content on.";
                                    responseContent.Success = false;
                                }
                            }
                            else // Find by Id
                            {
                                content = _contentService.GetById(contentObj.Id);
                            }

                            if (content != null && content.Id > 0)
                            {
                                // Content Found!
                                responseContent = ConvertContentObj(content);

                                List<IContent> childContentList = content.Children().ToList();

                                if (childContentList != null && childContentList.Count > 0)
                                {
                                    responseContent.ChildContentList = new List<Models.Import.Content>();

                                    foreach (IContent child in childContentList)
                                    {
                                        Models.Import.Content responseChild = ConvertContentObj(child);

                                        if (responseChild != null)
                                        {
                                            responseContent.ChildContentList.Add(responseChild);
                                        }
                                    }
                                }

                                responseContent.Success = true;
                                responseContent.Message = "Content found and returned.";
                            }
                            else
                            {
                                responseContent.Message = "Content could not be found.";
                                responseContent.Success = false;
                            }

                            response.ContentList.Add(responseContent);
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



        [System.Web.Http.HttpPost]
        public Models.Import.Response Delete([FromBody] dynamic json)
        {
            Models.Import.Response response = new Models.Import.Response();

            Models.Import.Request request = JsonConvert.DeserializeObject<Models.Import.Request>(json.ToString());

            try
            {
                if (request != null)
                {
                    //Check object
                    if (true == string.IsNullOrWhiteSpace(request.ApiKey))
                    {
                        response.Message = "API Key is missing.";
                    }
                    else if (request.ApiKey != _apiKey)
                    {
                        response.Message = "API Key is invalid.";
                    }
                    else if (request.ContentList == null || request.ContentList.Count == 0)
                    {
                        response.Message = "Content object empty. Needed for GET.";
                    }
                    else
                    {
                        response.Success = true;

                        foreach (Models.Import.Content contentObj in request.ContentList)
                        {
                            IContent content = null;
                            Models.Import.Content responseContent = new Models.Import.Content();

                            if (contentObj.Id <= 0)
                            {
                                if (contentObj.Properties != null && contentObj.Properties.Count > 0)
                                {
                                    Models.Import.Property propObj = contentObj.Properties[0];

                                    IEnumerable<IContent> rootNodeList = _contentService.GetRootContent();

                                    foreach (IContent rootNode in rootNodeList)
                                    {
                                        if (content == null)
                                        {
                                            IEnumerable<IContent> nodeList = _contentService.GetDescendants(rootNode.Id);

                                            content = nodeList.Where(p => p.Properties.Any(s => s.Value != null && s.Alias == propObj.Key && s.Value.ToString() == propObj.Value.ToString())).FirstOrDefault();
                                        }
                                    }
                                }
                                else
                                {
                                    responseContent.Message = "Must provide a property key and value to return content on.";
                                    responseContent.Success = false;
                                }
                            }
                            else // Find by Id
                            {
                                content = _contentService.GetById(contentObj.Id);
                            }

                            if (content != null && content.Id > 0)
                            {
                                // Content Found!
                                responseContent = ConvertContentObj(content);

                                _contentService.Delete(content);

                                responseContent.Success = true;
                                responseContent.Message = "Content found and deleted.";
                            }
                            else
                            {
                                responseContent.Message = "Content could not be found for deletion.";
                                responseContent.Success = false;
                            }

                            response.ContentList.Add(responseContent);
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




        /// <summary>
        /// Convert Umbraco content object
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Models.Import.Content ConvertContentObj(IContent content)
        {
            Models.Import.Content contentObj = new Models.Import.Content();

            contentObj.Id = content.Id;
            contentObj.Name = content.Name;
            contentObj.ParentId = content.ParentId;
            contentObj.DocType = content.ContentType.Alias;
            if (content.Template != null)
            {
                contentObj.Template = content.Template.Name;
            }
            contentObj.Properties = new List<Models.Import.Property>();

            foreach (var property in content.Properties)
            {
                string propValue = "";

                if (property.Value != null)
                {
                    propValue = property.Value.ToString();
                }

                Models.Import.Property propObj = new Models.Import.Property(property.Alias, propValue);

                contentObj.Properties.Add(propObj);
            }

            return contentObj;
        }

    }
}
