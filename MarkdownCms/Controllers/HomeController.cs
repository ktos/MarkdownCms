﻿using MarkdownCms.Helpers;
using MarkdownCms.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MarkdownCms.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string path = "")
        {
            var tree = DirectoryHelper.GetDirectoryTree(ConfigurationManager.AppSettings["FilesPath"]);
            ViewBag.Nodes = tree;

            string filePath = "";
            if (String.IsNullOrEmpty(path))
            {
                filePath = GetDefaultFilePath(tree);
                if (String.IsNullOrEmpty(filePath))
                    return View();
            }
            else
                filePath = GetFilePath(tree, path);

            if (!String.IsNullOrEmpty(filePath))
            {
                ViewBag.FileContent = GetHtmlFromFile(filePath);
                return View();
            }
            else
            {
                return View("MessageView", "File or directory can not be found" as object);
            }
        }

        private string GetHtmlFromFile(string filePath)
        {
            string htmlResult = "";
            using (var reader = new System.IO.StreamReader(filePath))
            {
                var fileContent = reader.ReadToEnd();
                htmlResult = CommonMark.CommonMarkConverter.Convert(fileContent);
            }
            return htmlResult;
        }

        private string GetFilePath(IList<DirectoryNode> tree, string path)
        {
            var filePath = "";
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (parts.Count != 0)
            {
                var node = tree.FirstOrDefault(x => x.Name == parts[0]);
                if (node != null)
                {
                    if (parts.Count != 1)
                    {
                        for (int i = 1; i < parts.Count; i++)
                        {
                            node = node.Subfolders.FirstOrDefault(x => x.Name == parts[i]);
                            if (node != null && i == parts.Count - 1)
                            {
                                filePath = node.IsFile ? node.Path : GetDefaultFilePath(node.Subfolders);
                            }
                        }
                    }
                    else
                    {
                        filePath = node.IsFile ? node.Path : GetDefaultFilePath(node.Subfolders);
                    }
                }
            }

            return filePath;
        }

        private string GetDefaultFilePath(IList<DirectoryNode> tree)
        {
            var node = tree.FirstOrDefault(f => f.IsFile && (f.Name.ToLower() == "index" || f.Name.ToLower() == "readme"));
            return node != null ? node.Path : String.Empty;
        }
    }
}