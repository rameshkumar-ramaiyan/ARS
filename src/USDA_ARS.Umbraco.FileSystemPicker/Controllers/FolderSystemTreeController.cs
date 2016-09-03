using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using System.Collections.Generic;
using System.IO;
using System.Web;
using USDA_ARS.Umbraco.FileSystemPicker.Controllers;
using System;
using Umbraco.Core.Logging;

namespace Umbraco.FileSystemPicker.Controllers
{
   [Tree("dummy", "fileSystemTree", "File System")]
   [PluginController("FileSystemPicker")]
   public class FolderSystemTreeController : TreeController
   {
      protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
      {
         if (!string.IsNullOrWhiteSpace(queryStrings.Get("startfolder")))
         {
            TreeNodeCollection tempTree = new TreeNodeCollection();

            var root = id == "-1" ? queryStrings.Get("startfolder") : id;
            if (id == "-1")
            {
               var path = IOHelper.MapPath("~/" + root.TrimStart('~', '/'));
               var rootName = new DirectoryInfo(path).Name;
               var rootNode = CreateTreeNode(root, "-1", queryStrings, rootName, "icon-folder", true);
               tempTree.Add(rootNode);
            }
            else
            {
               root = root.EnsureStartsWith("/");
               tempTree.AddRange(AddFolders(root, queryStrings));
               tempTree.AddRange(AddFiles(root, queryStrings));
            }
            return tempTree;
         }

         return AddFolders(id == "-1" ? "" : id, queryStrings);
      }

      protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
      {
         var menu = new MenuItemCollection();

         menu.Items.Add(new MenuItem("create", "Create"));

         return menu;
      }

      private TreeNodeCollection AddFiles(string folder, FormDataCollection queryStrings)
      {
         var pickerApiController = new FileSystemPickerApiController();
         //var str = queryStrings.Get("startfolder");

         if (string.IsNullOrWhiteSpace(folder))
            return null;

         var filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();


         var path = IOHelper.MapPath(folder);

         string startFolder = queryStrings.Get("startfolder");

         var rootPath = IOHelper.MapPath(startFolder.EnsureStartsWith("/"));
         var treeNodeCollection = new TreeNodeCollection();

         foreach (FileInfo fileInfo in pickerApiController.GetFiles(folder, filter))
         {
            string nodeTitle = fileInfo.Name;
            //string filePath = file.FullName.Replace(rootPath, "").Replace("\\", "/");
            string filePath = string.Format("{0}/{1}", folder, fileInfo.Name);

            TreeNode treeNode = CreateTreeNode(filePath, folder, queryStrings, nodeTitle, "icon-document", false);

            treeNodeCollection.Add(treeNode);
         }

         return treeNodeCollection;
      }

      private TreeNodeCollection AddFolders(string parent, FormDataCollection queryStrings)
      {
         FileSystemPickerApiController pickerApiController = new FileSystemPickerApiController();

         string[] filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();

         TreeNodeCollection treeNodeCollection = new TreeNodeCollection();

         string startFolderName = queryStrings.Get("startfolder");

         string startFolderPath = startFolderName.TrimStart(new char[] { '~', '/' }).EnsureStartsWith("~/");

         IEnumerable<DirectoryInfo> directoryList = pickerApiController.GetFolders(parent, filter);
         List<TreeNode> treeNodeList = new List<TreeNode>();

         if (directoryList != null && directoryList.Any())
         {
            foreach (DirectoryInfo dirInfo in directoryList)
            {
               string treeId = String.Format("{0}{1}", startFolderName, dirInfo.FullName.Replace(IOHelper.MapPath(startFolderPath), "").Replace("\\", "/"));

               bool hasChildren = false;

               if (filter != null && filter.Length > 0)
               {
                  if (filter[0] == ".")
                  {
                     string filePath = dirInfo.FullName.Replace(IOHelper.MapPath(startFolderPath), "").Replace("\\", "/");

                     IEnumerable<FileInfo> fileInfoList = pickerApiController.GetFiles(filePath, filter);

                     hasChildren = dirInfo.EnumerateDirectories().Any() ||
                              (fileInfoList != null && fileInfoList.Any());
                  }
                  else
                  {
                     string filePath = dirInfo.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/");

                     hasChildren = pickerApiController.GetFiles(filePath, filter).Any();
                  }
               }
               else
               {
                  hasChildren = pickerApiController.GetFiles(dirInfo.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"), filter).Any();
               }
               string dirName = dirInfo.Name;

               treeNodeList.Add(CreateTreeNode(treeId, parent, queryStrings, dirName, "icon-folder", hasChildren));
            }
         }

         treeNodeCollection.AddRange(treeNodeList);


         return treeNodeCollection;
      }
   }
}