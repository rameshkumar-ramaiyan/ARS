using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using USDA_ARS.Umbraco.Extensions.Helpers.Aris;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Models
{
   public class FormProjectFilter
   {
      public string ProjectStatus { get; set; }

      public string Person { get; set; }

      public string ProjectType { get; set; }

      public string Location { get; set; }

      public string ShowAll { get; set; }

      public string SortBy { get; set; }


						/// <summary>
						/// Get a list of projects using a NP CODE
						/// </summary>
						/// <param name="npCode"></param>
						/// <param name="projectStatus"></param>
						/// <param name="personIdList"></param>
						/// <param name="projectType"></param>
						/// <param name="location"></param>
						/// <param name="orderBy"></param>
						/// <returns></returns>
						public static List<ProjectProgram> GetProjectList(string npCode, string projectStatus, List<int> personIdList = null, 
															string projectType = "", string location = "N", string orderBy = "L")
						{
									return ProjectPrograms.GetProjectProgramsByNpCode(npCode, projectStatus, personIdList, projectType, location, orderBy);
      }


						public static List<ProjectOption> GetProjectStatusList()
      {
         List<ProjectOption> projectStatusList = new List<ProjectOption>();

         projectStatusList.Add(new ProjectOption() { Value = "A", Text = "Active" });
         projectStatusList.Add(new ProjectOption() { Value = "E", Text = "Terminated" });
         projectStatusList.Add(new ProjectOption() { Value = "X", Text = "Expired" });

         return projectStatusList;
      }

						public static List<ProjectOption> GetProjectTypeList()
						{
									List<ProjectOption> projectStatusList = new List<ProjectOption>();

									projectStatusList.Add(new ProjectOption() { Value = "D", Text = "Appropriated" });
									projectStatusList.Add(new ProjectOption() { Value = "C", Text = "Contract" });
									projectStatusList.Add(new ProjectOption() { Value = "A", Text = "General Cooperative Agreement" });
									projectStatusList.Add(new ProjectOption() { Value = "G", Text = "Grant" });
									projectStatusList.Add(new ProjectOption() { Value = "M", Text = "Memorandum of Understanding" });
									projectStatusList.Add(new ProjectOption() { Value = "N", Text = "Nonfunded Cooperative Agreement" });
									projectStatusList.Add(new ProjectOption() { Value = "X", Text = "Other" });
									projectStatusList.Add(new ProjectOption() { Value = "R", Text = "Reimbursable" });
									projectStatusList.Add(new ProjectOption() { Value = "S", Text = "Specific Cooperative Agreement" });
									projectStatusList.Add(new ProjectOption() { Value = "T", Text = "Trust" });

									return projectStatusList;
						}

						public static List<PeopleInfo> GetPeopleByProject(string npCode, string projectStatus = "A")
      {
         return People.GetPeopleByProject(npCode, projectStatus);
      }

      public static List<CityState> GetLocationsByProject(string npCode, string projectStatus = "A")
      {
         return Projects.GetLocationsByProject(npCode, projectStatus);
      }
   }

   public class ProjectOption
   {
      public string Value { get; set; }
      public string Text { get; set; }
   }
}
