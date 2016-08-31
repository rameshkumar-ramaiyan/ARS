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


      public static List<ProjectStatusOption> GetProjectStatusList()
      {
         List<ProjectStatusOption> projectStatusList = new List<ProjectStatusOption>();

         projectStatusList.Add(new ProjectStatusOption() { Value = "A", Text = "Active" });
         projectStatusList.Add(new ProjectStatusOption() { Value = "E", Text = "Terminated" });
         projectStatusList.Add(new ProjectStatusOption() { Value = "X", Text = "Expired" });

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

   public class ProjectStatusOption
   {
      public string Value { get; set; }
      public string Text { get; set; }
   }
}
