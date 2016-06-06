﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class ModeCodesNew
    {
        public static UmbracoHelper UmbHelper = new UmbracoHelper(UmbracoContext.Current);

        public static string GetNewModeCode(string oldModeCode)
        {
            string output = null;
            oldModeCode = oldModeCode.Replace("-", "");

            List<ModeCodeNew> allModeCodeList = GetAllNewModeCode();

            if (allModeCodeList != null)
            {
                ModeCodeNew modeCodeNew = allModeCodeList.Where(p => p.ModecodeOld == oldModeCode).FirstOrDefault();

                if (modeCodeNew != null)
                {
                    output = modeCodeNew.ModecodeNew;
                }
            }

            return output;
        }

        public static List<ModeCodeNew> GetAllNewModeCode()
        {
            List<ModeCodeNew> output = null;

            string cacheKey = "NodeListByOldModeCodes";
            int cacheUpdateIntMinutes = 1440;

            ObjectCache cache = MemoryCache.Default;

            output = cache.Get(cacheKey) as List<ModeCodeNew>;

            if (output == null)
            {
                List<IPublishedContent> nodeList = new List<IPublishedContent>();

                foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
                {
                    if (false == string.IsNullOrEmpty(root.GetPropertyValue<string>("oldModeCodes")))
                    {
                        List<string> oldCodeArray = root.GetPropertyValue<string>("oldModeCodes").Split(',').ToList();

                        if (oldCodeArray != null)
                        {
                            foreach (string oldCode in oldCodeArray)
                            {
                                output.Add(new ModeCodeNew() { ModecodeNew = root.GetPropertyValue<string>("modeCode").Replace("-", ""), ModecodeOld = oldCode.Replace("-", "") });
                            }
                        }
                    }
                    else
                    {
                        List<IPublishedContent> nodeDescendantsList = root.Descendants().Where(n => false == string.IsNullOrEmpty(n.GetPropertyValue<string>("oldModeCodes"))).ToList();

                        if (nodeDescendantsList != null)
                        {
                            foreach (IPublishedContent node in nodeDescendantsList)
                            {
                                List<string> oldCodeArray = node.GetPropertyValue<string>("oldModeCodes").Split(',').ToList();

                                if (oldCodeArray != null)
                                {
                                    foreach (string oldCode in oldCodeArray)
                                    {
                                        output.Add(new ModeCodeNew() { ModecodeNew = node.GetPropertyValue<string>("modeCode").Replace("-", ""), ModecodeOld = oldCode.Replace("-", "") });
                                    }
                                }
                            }
                        }
                    }
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateIntMinutes) };
                cache.Add(cacheKey, output, policy);
            }

            return output;
        }


        public static List<ModeCodeNew> GetOldModeCode(string newModeCode)
        {
            List<ModeCodeNew> modeCodeList = null;

            if (false == string.IsNullOrEmpty(newModeCode))
            {
                newModeCode = newModeCode.Replace("-", "");

                List<ModeCodeNew> allModeCodeList = GetAllNewModeCode();

                if (allModeCodeList != null)
                {
                    modeCodeList = allModeCodeList.Where(p => p.ModecodeNew == newModeCode).ToList();
                }
            }

            return modeCodeList;
        }
    }
}
