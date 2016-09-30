using Archetype.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Helpers.Aris;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Utilities
{
	public class UmbEventHandler : ApplicationEventHandler
	{
		private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;
		private static readonly IContentTypeService _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
		private static readonly IUserService _userService = ApplicationContext.Current.Services.UserService;

		private static bool updateModeCodes = false;
		private static bool updateOldUrl = false;
		private static bool updateSoftware = false;
		private static bool updateNewsInterLinks = false;

		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			ContentService.Created += PostProcessCreated;
			ContentService.Saving += PostProcessSaving;
			ContentService.Saved += PostProcessSaved;
			ContentService.Published += PostProcessPublished;
			ContentService.Deleted += PostProcessDelete;
			ContentService.UnPublished += ContentServiceUnPublished;

			UserService.SavedUser += UserServiceSavedUser;
		}


		private static void PostProcessCreated(IContentService cs, NewEventArgs<IContent> e)
		{
			IContent node = e.Entity;

			//bool isNationalProgramPage = false;
			//if (node.Parent() != null && node.Parent().ContentType.Alias == "NationalProgramFolderContainer")
			//{
			//   isNationalProgramPage = true;
			//}

			//if (true == isNationalProgramPage || node.ContentType.Alias == "DocFolder" ||
			//                node.ContentType.Alias == "SiteStandardWebpage" ||
			//                node.ContentType.Alias == "NationalProgramFolderContainer")
			//{
			//   IContent nodeParent = node.Parent();

			//   if (nodeParent != null)
			//   {
			//      // Get navigation category from the parent node
			//      Property propertyParent = nodeParent.Properties.Where(p => p.Alias == "navigationCategory").FirstOrDefault();

			//      if (propertyParent != null)
			//      {
			//         Property propertyNode = node.Properties.Where(p => p.Alias == "navigationCategory").FirstOrDefault();

			//         if (propertyNode != null)
			//         {
			//            node.SetValue("navigationCategory", propertyParent.Value);
			//         }
			//      }

			//      Property propertyParentBottom = nodeParent.Properties.Where(p => p.Alias == "navigationCategoryBottom").FirstOrDefault();

			//      if (propertyParentBottom != null)
			//      {
			//         Property propertyNodeBottom = node.Properties.Where(p => p.Alias == "navigationCategoryBottom").FirstOrDefault();

			//         if (propertyNodeBottom != null)
			//         {
			//            node.SetValue("navigationCategoryBottom", propertyParentBottom.Value);
			//         }
			//      }
			//   }
			//}


		}


		private void PostProcessSaving(IContentService sender, SaveEventArgs<IContent> e)
		{
			foreach (var node in e.SavedEntities)
			{
				// UPDATE MODE CODES
				updateModeCodes = false;
				if (e.SavedEntities.Count() == 1 && node.HasProperty("modeCode"))
				{
					if (node.IsPropertyDirty("modeCode"))
					{
						updateModeCodes = true;
					}
				}


				// UPDATE OLD URLS
				updateOldUrl = false;
				if (e.SavedEntities.Count() == 1 && node.HasProperty("oldUrl"))
				{
					if (node.IsPropertyDirty("oldUrl"))
					{
						updateOldUrl = true;
					}
				}

				// UPDATE NEWS INTERLINKS
				updateNewsInterLinks = false;
				if (e.SavedEntities.Count() == 1 && node.ContentType.Alias == "NewsArticle" && node.HasProperty("bodyText"))
				{
					if (node.IsPropertyDirty("bodyText"))
					{
						updateNewsInterLinks = true;
					}
					else
					{
						List<NewsInterLink> linksList = NewsInterLinks.GetLinksByNewsArticle(node.Id);

						if (linksList == null || false == linksList.Any())
						{
							updateNewsInterLinks = true;
						}
					}
				}


				// Check to see if user has access to create a ResearchUnit doc type
				if (node != null && node.Id == 0 && (node.ContentType.Alias == "ResearchUnit" || node.ContentType.Alias == "ResearchUnitClosed"))
				{
					var userTicket = new System.Web.HttpContextWrapper(System.Web.HttpContext.Current).GetUmbracoAuthTicket();
					if (userTicket != null)
					{
						var currentUser = _userService.GetByUsername(userTicket.Name);

						if (currentUser != null && currentUser.UserType.Name != "Administrators")
						{
							if (e.CanCancel)
							{
								e.Cancel = true;
								e.Messages.Add(new EventMessage("Cannot Create", "Unable to create node due to security permissions.", EventMessageType.Error));
							}
						}
					}
				}
			}
		}


		private static void PostProcessSaved(IContentService cs, SaveEventArgs<IContent> e)
		{
			foreach (var node in e.SavedEntities)
			{
				// UPDATE SOFTWARE
				if (node.ContentType.Alias == "SoftwareItem")
				{
					string softwareId = node.GetValue<string>("softwareID");

					if (true == string.IsNullOrWhiteSpace(softwareId))
					{
						List<IPublishedContent> softwareList = Software.GetSoftwareNodes();

						if (softwareList != null && softwareList.Any())
						{
							int softwareIdNext = Software.GetLastSoftwareId() + 1;

							node.Properties["softwareId"].Value = softwareIdNext;

							_contentService.SaveAndPublishWithStatus(node);
						}

					}
				}

				/////////////
				// When a Doc Folder or Site Standard Webpage is Created...
				if (node.IsNewEntity() == true && (node.ContentType.Alias == "DocsFolder" || node.ContentType.Alias == "SiteStandardWebpage"))
				{
					IContent parentNode = node.Parent();

					// UPDATE SORT ORDER
					if (parentNode.ContentType.Alias == "Area" || parentNode.ContentType.Alias == "ResearchUnit" || parentNode.ContentType.Alias == "City")
					{
						int sortOrder = 0;
						List<IContent> updatedChildList = new List<IContent>();

						// Get list of doc folders and pages and order them by name. Add to updatedChildList object list for sorting later
						List<IContent> docsAndFoldersList = parentNode.Children().Where(p => p.ContentType.Alias != "Area"
														&& p.ContentType.Alias != "ResearchUnit"
														&& p.ContentType.Alias != "City").OrderBy(s => s.Name).ToList();

						if (docsAndFoldersList != null && docsAndFoldersList.Any())
						{
							foreach (IContent subNode in docsAndFoldersList)
							{
								updatedChildList.Add(subNode);

								sortOrder++;
							}
						}


						List<IContent> nonDocsAndFoldersList = null;

						if (parentNode.ContentType.Alias == "City" || parentNode.ContentType.Alias == "ResearchUnit")
						{
							nonDocsAndFoldersList = parentNode.Children().Where(p => p.ContentType.Alias == "ResearchUnit").ToList();

							if (nonDocsAndFoldersList != null && nonDocsAndFoldersList.Any())
							{
								nonDocsAndFoldersList = nonDocsAndFoldersList.OrderBy(p => p.Name).ToList();
							}
						}

						if (nonDocsAndFoldersList != null && nonDocsAndFoldersList.Any())
						{
							foreach (IContent subNode in nonDocsAndFoldersList)
							{
								updatedChildList.Add(subNode);

								sortOrder++;
							}
						}

						bool sortSuccess = _contentService.Sort(updatedChildList, raiseEvents: false);
					}
				}


				// SUB FOLDERS FOR REGIONS AND RESEARCH UNITS
				if (true == node.IsNewEntity() && (node.ContentType.Alias == "Area" || node.ContentType.Alias == "ResearchUnit"))
				{
					UmbracoHelper umbHelper = new UmbracoHelper(UmbracoContext.Current);
					int siteFolderTemplateNodeId = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Usda:SiteFoldersTemplateNodeId")); //

					IPublishedContent siteFoldersTemplate = umbHelper.TypedContent(siteFolderTemplateNodeId);

					if (siteFoldersTemplate != null)
					{
						List<IPublishedContent> siteFolderSubNodeList = siteFoldersTemplate.Children.ToList();

						if (siteFolderSubNodeList != null && siteFolderSubNodeList.Any())
						{
							List<IContent> nodeDescendantsList = node.Descendants().ToList();

							foreach (IPublishedContent subNode in siteFolderSubNodeList)
							{
								IContent testSubNode = nodeDescendantsList.FirstOrDefault(n => n.Name == subNode.Name);

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
							if (false == Directory.Exists(fullPath + "\\images\\photoCarousel"))
							{
								Directory.CreateDirectory(fullPath + "\\images\\photoCarousel");
							}
						}
					}

				}

				//////////////
				// If a subsite is saved, make sure folder and navigation exists
				if (true == node.IsNewEntity() && node.ContentType.Alias == "Subsite")
				{
					// check for ARSUserFiles folder

					string startFolder = ConfigurationManager.AppSettings.Get("Usda:UserFileFolderPath");
					string folderLabel = node.GetValue<string>("folderLabel").ToLower();

					if (false == string.IsNullOrEmpty(folderLabel))
					{
						string fullPath = IOHelper.MapPath(startFolder.EnsureStartsWith("~/") + "/" + folderLabel);

						if (false == Directory.Exists(fullPath))
						{
							Directory.CreateDirectory(fullPath);
						}
					}

					// Check for nav folder
					IContent navFolderNode = node.Children().Where(p => p.ContentType.Alias == "SiteNavFolder").FirstOrDefault();

					if (navFolderNode == null)
					{
						IContent navFolderNodeCreate = _contentService.CreateContent("Navigations", node, "SiteNavFolder");
						_contentService.SaveAndPublishWithStatus(navFolderNodeCreate);
					}
				}

				///////////////
				// If a National Program is saved
				if (node.ContentType.Alias == "NationalProgram")
				{
					IContent navFolderNode = node.Children().Where(p => p.ContentType.Alias == "SiteNavFolder").FirstOrDefault();

					if (navFolderNode == null)
					{
						IContent navFolderNodeCreate = _contentService.CreateContent("Navigations", node, "SiteNavFolder");
						_contentService.SaveAndPublishWithStatus(navFolderNodeCreate);
					}


					IContent docsFolderNode = node.Children().Where(p => p.ContentType.Alias == "NationalProgramFolderContainer").FirstOrDefault();

					if (docsFolderNode == null)
					{
						var childNode = _contentService.CreateContent("Docs", node, "NationalProgramFolderContainer");

						_contentService.SaveAndPublishWithStatus(childNode);
					}
				}
				else if (true == updateNewsInterLinks && node.ContentType.Alias == "NewsArticle")
				{
					if (ConfigurationManager.AppSettings["USDA_ARS:ProcessInterLinksOnSave"] != null && ConfigurationManager.AppSettings.Get("USDA_ARS:ProcessInterLinksOnSave").ToLower() == "true")
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
				else if (node.ContentType.Alias == "PersonSite" || node.ContentType.Alias == "NationalProgramGroup")
				{
					IContent navFolderNode = node.Children().Where(p => p.ContentType.Alias == "SiteNavFolder").FirstOrDefault();

					if (navFolderNode == null)
					{
						IContent navFolderNodeCreate = _contentService.CreateContent("Navigations", node, "SiteNavFolder");
						_contentService.SaveAndPublishWithStatus(navFolderNodeCreate);
					}

					// Check for File System Folder
					if (node.ContentType.Alias == "PersonSite" && false == string.IsNullOrEmpty(node.GetValue<string>("personLink")))
					{
						string startFolder = ConfigurationManager.AppSettings.Get("Usda:UserFileFolderPath");
						string personIdFolder = node.GetValue<string>("personLink").Replace("-", "");

						int personId = 0;

						if (true == int.TryParse(personIdFolder, out personId))
						{
							if (personId > 0)
							{
								string fullPath = IOHelper.MapPath(startFolder.EnsureStartsWith("~/") + "/" + personIdFolder);

								if (false == Directory.Exists(fullPath))
								{
									Directory.CreateDirectory(fullPath);
								}
							}
						}
					}
				}
			}
		}


		private void PostProcessPublished(global::Umbraco.Core.Publishing.IPublishingStrategy sender, PublishEventArgs<IContent> e)
		{
			foreach (var node in e.PublishedEntities)
			{
				if (true == updateModeCodes)
				{
					Nodes.GetNodesListOfModeCodes(false);
					ModeCodeActives.UpdateModeCodeActiveTable();
				}

				if (true == updateOldUrl)
				{
					Nodes.NodesWithRedirectsList(false);
				}

				// Re-open a Research Unit that is Closed
				if (node != null && node.ContentType.Alias == "ResearchUnitClosed")
				{
					if (node.HasProperty("reopenResearchUnit") && true == node.GetValue<bool>("reopenResearchUnit"))
					{
						var officeOpenDocType = _contentTypeService.GetContentType("ResearchUnit");

						node.ChangeContentType(officeOpenDocType);

						_contentService.SaveAndPublishWithStatus(node);
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


		private void ContentServiceUnPublished(global::Umbraco.Core.Publishing.IPublishingStrategy sender, PublishEventArgs<IContent> e)
		{
			foreach (var node in e.PublishedEntities)
			{
				if (node.ContentType.Alias == "NewsArticle")
				{
					NewsInterLinks.RemoveLinksByNodeId(node.Id);
				}
			}
		}


		/// <summary>
		/// Umbraco Event: User Saved
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserServiceSavedUser(IUserService sender, SaveEventArgs<global::Umbraco.Core.Models.Membership.IUser> e)
		{
			foreach (var user in e.SavedEntities)
			{
				var userService = ApplicationContext.Current.Services.UserService;

				// If the user type is "Resend Welcome Email", send the email and then revert the user type back to editor
				if (user.UserType.Alias == "ResendWelcomeEmail" || user.IsNewEntity() == true)
				{
					user.UserType = userService.GetUserTypeByAlias("editor");

					IPublishedContent emailTemplate = Nodes.GetEmailTemplate("New User Welcome");

					if (emailTemplate != null)
					{
						var message = new MailMessage();

						message.From = new MailAddress(emailTemplate.GetPropertyValue<string>("emailFrom"));
						message.To.Add(new MailAddress(user.Email));

						string emailCC = emailTemplate.GetPropertyValue<string>("emailCC");
						string emailBcc = emailTemplate.GetPropertyValue<string>("emailBcc");

						foreach (string emailCCItem in emailCC.Split(';'))
						{
							if (false == string.IsNullOrWhiteSpace(emailCCItem))
							{
								message.CC.Add(new MailAddress(emailCCItem.Trim()));
							}
						}

						foreach (string emailBccItem in emailBcc.Split(';'))
						{
							if (false == string.IsNullOrWhiteSpace(emailBccItem))
							{
								message.Bcc.Add(new MailAddress(emailBccItem.Trim()));
							}
						}

						message.Subject = emailTemplate.GetPropertyValue<string>("emailSubject").Trim();

						string body = emailTemplate.GetPropertyValue<string>("emailBody");

						body = body.Replace("src=\"/", "src=\"http://www.ars.usda.gov/");
						body = body.Replace("href=\"/", "href=\"http://www.ars.usda.gov/");

						message.Body = body;
						message.IsBodyHtml = true;

						using (var smtp = new SmtpClient())
						{
							try
							{
								smtp.Send(message);
							}
							catch (Exception ex)
							{
								LogHelper.Error<UmbEventHandler>("Problem sending email.", ex);
							}
						}

						userService.Save(user);
					}


				}
			}
		}
	}
}
