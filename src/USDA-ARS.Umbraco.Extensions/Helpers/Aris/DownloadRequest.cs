using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
			public class DownloadRequests
			{
						public static List<Models.Aris.DownloadRequest> GetDownloadRequests(string softwareId)
						{
									List<Models.Aris.DownloadRequest> downloadList = new List<Models.Aris.DownloadRequest>();

									var db = new Database("arisPublicWebDbDSN");

									string sql = "SELECT * FROM DownloadRequests WHERE SoftwareId = @softwareId";

									downloadList = db.Query<Models.Aris.DownloadRequest>(sql, new { softwareId = softwareId }).ToList();

									return downloadList;
						}


						public static Models.NodeDownloadRequests GetDownloadRequestsByNode(IPublishedContent node)
						{
									Models.NodeDownloadRequests nodeDownloadRequests = new Models.NodeDownloadRequests();

									nodeDownloadRequests.NodeDownloadRequestsList = new List<Models.NodeDownloadRequestsList>();

									var db = new Database("arisPublicWebDbDSN");

									Models.NodeDownloadRequestsList nodeDownloadList = new Models.NodeDownloadRequestsList();

									nodeDownloadList.SoftwareTitle = node.Name;

									List<Models.Aris.DownloadRequest> downloadList = new List<Models.Aris.DownloadRequest>();

									if (node.HasProperty("softwareID") && node.HasValue("softwareID"))
									{
												downloadList = GetDownloadRequests(node.GetPropertyValue<string>("softwareID"));

												if (downloadList != null && downloadList.Any())
												{
															downloadList = downloadList.OrderByDescending(p => p.TimeStamp).ToList();

															nodeDownloadList.DownloadRequestList = downloadList;
												}

												nodeDownloadRequests.NodeDownloadRequestsList.Add(nodeDownloadList);
									}

									return nodeDownloadRequests;
						}


						public static bool SaveDownloadRequest(Models.Aris.DownloadRequest downloadRequest)
						{
									bool success = true;

									try
									{
												var db = new Database("arisPublicWebDbDSN");

												if (downloadRequest.Id == 0)
												{
															db.Insert(downloadRequest);
												}
												else
												{
															db.Save(downloadRequest);
												}
									}
									catch (Exception ex)
									{
												success = false;

												LogHelper.Error(typeof(DownloadRequests), "Error saving download request", ex);
									}


									return success;
						}


						public static bool ClearDownloadRequest(string softwareId)
						{
									bool success = false;

									try
									{
												List<Models.Aris.DownloadRequest> downloadList = GetDownloadRequests(softwareId);


												var db = new Database("arisPublicWebDbDSN");

												if (downloadList != null && downloadList.Any())
												{
															foreach (Models.Aris.DownloadRequest download in downloadList)
															{
																		db.Delete(download);
															}
												}
									}
									catch (Exception ex)
									{
												success = false;

												LogHelper.Error(typeof(DownloadRequests), "Error clearing download requests", ex);
									}

									return success;
						}
			}
}
