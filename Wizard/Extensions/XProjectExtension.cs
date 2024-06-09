﻿/*!
 * Copyright (c) Denis Kuzmin <x-3F@outlook.com> github/3F
 * Copyright (c) DllExport contributors https://github.com/3F/DllExport/graphs/contributors
 * Licensed under the MIT License (MIT).
 * See accompanying LICENSE.txt file or visit https://github.com/3F/DllExport
*/

using System;
using System.Linq;
using Microsoft.Build.Construction;
using net.r_eg.MvsSln.Core;
using net.r_eg.MvsSln.Extensions;

namespace net.r_eg.DllExport.Wizard.Extensions
{
    internal static class XProjectExtension
    {
        /// <summary>
        /// To get property value with global scope by default.
        /// </summary>
        /// <param name="xp"></param>
        /// <param name="name">The name of the property.</param>
        /// <param name="localScope">If true, will return default value for any special and imported properties type.</param>
        /// <returns>The evaluated property value, which is never null.</returns>
        public static string GetPropertyValue(this IXProject xp, string name, bool localScope = false)
        {
            return xp?.GetProperty(name, localScope).evaluated;
        }

        /// <summary>
        /// To get unevaluated property value with global scope by default.
        /// </summary>
        /// <param name="xp"></param>
        /// <param name="name">The name of the property.</param>
        /// <param name="localScope">If true, will return default value for any special and imported properties type.</param>
        /// <returns>The unevaluated property value, which is never null.</returns>
        public static string GetUnevaluatedPropertyValue(this IXProject xp, string name, bool localScope = false)
        {
            return xp?.GetProperty(name, localScope).unevaluated;
        }

        /// <summary>
        /// Get unique identifier for project (not instance).
        /// TODO: MvsSln should provide similar PId with v2.0.1+
        /// </summary>
        /// <param name="xp"></param>
        /// <returns></returns>
        public static Guid GetPId(this IXProject xp)
        {
            if(xp == null) {
                return Guid.Empty;
            }

            var pItem = xp.ProjectItem;
            return (
                pItem.project.pGuid
                    + pItem.projectConfig 
                    + pItem.solutionConfig
            )
            .Guid();
        }

        public static void AddPackageIfNotExists(this IXProject xp, string id, string version)
        {
            if(xp == null) {
                throw new ArgumentNullException(nameof(xp)); 
            }

            if(xp.GetFirstPackageReference(id ?? throw new ArgumentNullException(nameof(id))).parentItem == null) {
                xp.AddPackageReference(id, version);
            }
        }

        internal static ProjectPropertyGroupElement GetOrAddPropertyGroup(this IXProject xp, string label, string condition = null)
        {
            if(xp == null) throw new ArgumentNullException(nameof(xp));
            if(string.IsNullOrEmpty(label)) throw new ArgumentOutOfRangeException(nameof(label));

            var pgroup = xp.Project.Xml.PropertyGroups.FirstOrDefault(p => p.Label == label);
            if(pgroup != null) return pgroup;

            return xp.AddPropertyGroup(label, condition);
        }

        internal static ProjectPropertyGroupElement AddPropertyGroup(this IXProject xp, string label = null, string condition = null)
        {
            if(xp == null) throw new ArgumentNullException(nameof(xp));

            var pgroup = xp.Project.Xml.AddPropertyGroup();

            if(label != null)
            {
                pgroup.Label = label;
            }

            if(condition != null)
            {
                pgroup.Condition = condition;
            }

            return pgroup;
        }

        internal static void RemovePropertyGroups(this IXProject xp, Func<ProjectPropertyGroupElement, bool> condition)
            => xp?.Project.Xml.PropertyGroups.ToArray()
                                .Where(p => condition(p))
                                .ForEach(p => p.Parent?.RemoveChild(p));

        internal static void RemoveEmptyPropertyGroups(this IXProject xp) 
            => xp.RemovePropertyGroups(p => p.Properties.Count < 1);

        internal static bool RemoveXmlTarget(this IXProject xp, string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var target = xp?.Project.Xml.Targets?.FirstOrDefault(t => t.Name == name);
            if(target != null)
            {
                xp.Project.Xml.RemoveChild(target);
                return true;
            }
            return false;
        }

        internal static void RemoveProperties(this IXProject xp, params string[] names)
        {
            if(xp == null) return;

            foreach(string name in names)
            {
                if(!string.IsNullOrWhiteSpace(name)) while(xp.RemoveProperty(name, true)) { }
            }
            xp.RemoveEmptyPropertyGroups();
        }
    }
}