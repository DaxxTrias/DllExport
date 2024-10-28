﻿/*!
 * Copyright (c) Denis Kuzmin <x-3F@outlook.com> github/3F
 * Copyright (c) DllExport contributors https://github.com/3F/DllExport/graphs/contributors
 * Licensed under the MIT License (MIT).
 * See accompanying LICENSE.txt file or visit https://github.com/3F/DllExport
*/

using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using net.r_eg.Conari.Extension;
using net.r_eg.DllExport.Wizard.Extensions;
using net.r_eg.MvsSln.Core;
using net.r_eg.MvsSln.Log;

namespace net.r_eg.DllExport.Wizard
{
    public class TargetsFile: Project, IProject, ITargetsFile, IDisposable
    {
        internal const string DEF_CFG_FILE = ".net.dllexport.targets";

        private readonly string rootpath;

        /// <summary>
        /// Full path to root solution directory.
        /// </summary>
        public override string SlnDir => rootpath;

        /// <summary>
        /// To configure project via specific action.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool Configure(ActionType type) => Configure(type, null);

        /// <summary>
        /// To configure external .targets through parent project.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool Configure(ActionType type, IProject parent)
        {
            if(type != ActionType.Update && type != ActionType.Configure) {
                return false;
            }

            AssignConfig(parent);
            Log.send(this, $"Configuring TargetsFile '{parent.DxpIdent}' -> '{XProject.Project.FullPath}'", Message.Level.Debug);

            if(type == ActionType.Update && parent.Installed
                || type == ActionType.Configure && Config.Install)
            {
                Configure(parent);
            }

            return true;
        }

        /// <summary>
        /// To export data via parent project into .targets file.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool Export(IProject parent)
        {
            AssignConfig(parent);
            Log.send(this, $"Export data via TargetsFile '{parent.DxpIdent}'", Message.Level.Debug);

            if(parent.Installed || Config.Install) {
                Configure(parent);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets data for external .targets file.
        /// </summary>
        public void Reset() => Reset(true);

        /// <summary>
        /// Saves data to the file system.
        /// </summary>
        /// <param name="reevaluate">Try to reevaluate data of project before saving.</param>
        public void Save(bool reevaluate)
        {
            if(reevaluate) {
                XProject.Reevaluate();
            }
            Save();
        }

        public TargetsFile(string file, string rootpath, IConfigInitializer ini)
            : this(file, rootpath, ini.Config.Type)
        {
            Config = new UserConfig(ini);
        }

        protected TargetsFile(string file, string rootpath, ActionType type)
            : base(type == ActionType.Recover ? new XProject(file) : new XProject())
        {
            this.rootpath = rootpath;
            XProject.Project.FullPath = file; // Only for saving. Loading through `Import` section.
        }

        protected void Configure(IProject parent)
        {
            ResetById(parent);
            CfgCommonData();

            string projectFile = MakeBasePath(parent.XProject.ProjectFullPath, prefix: false);

            ConfigProperties[MSBuildProperties.DXP_CFG_ID] = parent.DxpIdent;
            ConfigProperties[MSBuildProperties.DXP_PRJ_FILE] = projectFile;

            SetProperties
            (
                ConfigProperties, 
                MakeCondition(parent),
                projectFile
            );
            XProject.Reevaluate();

            if(XProject.GetImport().project == null) AddDllExportLib();
        }

        protected void SetProperties(IEnumerable<KeyValuePair<string, string>> properties, string condition, string label)
            => XProject.AddPropertyGroup(label, condition).E
        (
                group =>
                properties.ForEach(p =>
                    p.Value.If(v => v != null, v => group.SetProperty(p.Key, v))
                )
        );

        protected void ResetById(IProject prj)
        {
            string cond = MakeCondition(prj);

            foreach(var group in XProject.Project.Xml.PropertyGroups)
            {
                if(cond.Equals(group.Condition, StringComparison.OrdinalIgnoreCase)) {
                    group.Parent.RemoveChild(group);
                }
            }
        }

        private void AssignConfig(IProject prj) => Config = prj?.Config ?? throw new ArgumentNullException(nameof(prj));

        private string MakeCondition(IProject prj) => $"'$({MSBuildProperties.DXP_ID})'=='{prj.DxpIdent}'";

        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _)
        {
            if(!disposed)
            {
                if(XProject?.Project?.FullPath != null)
                    ProjectCollection.GlobalProjectCollection?.UnloadProject(XProject.Project);

                disposed = true;
            }
        }

        #endregion
    }
}
