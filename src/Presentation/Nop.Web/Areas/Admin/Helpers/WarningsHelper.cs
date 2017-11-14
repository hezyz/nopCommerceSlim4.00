using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Nop.Web.Areas.Admin.Helpers
{
    /// <summary>
    /// Warnings helper
    /// </summary>
    public static class WarningsHelper
    {
        public static List<SystemWarningModel> GetWarnings()
        {
            var catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();
            var storeService = EngineContext.Current.Resolve<IStoreService>();
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();

            var model = new List<SystemWarningModel>();

            //store URL
            var currentStoreUrl = storeContext.CurrentStore.Url;
            if (!string.IsNullOrEmpty(currentStoreUrl) &&
                (currentStoreUrl.Equals(webHelper.GetStoreLocation(false), StringComparison.InvariantCultureIgnoreCase)
                ||
                currentStoreUrl.Equals(webHelper.GetStoreLocation(true), StringComparison.InvariantCultureIgnoreCase)
                ))
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = localizationService.GetResource("Admin.System.Warnings.URL.Match")
                });
            else
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = string.Format(localizationService.GetResource("Admin.System.Warnings.URL.NoMatch"), currentStoreUrl, webHelper.GetStoreLocation(false))
                });

            
            //incompatible plugins
            if (PluginManager.IncompatiblePlugins != null)
                foreach (var pluginName in PluginManager.IncompatiblePlugins)
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(localizationService.GetResource("Admin.System.Warnings.PluginNotLoaded"), pluginName)
                    });

            //performance settings
            if (!catalogSettings.IgnoreStoreLimitations && storeService.GetAllStores().Count == 1)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Warning,
                    Text = localizationService.GetResource("Admin.System.Warnings.Performance.IgnoreStoreLimitations")
                });
            }
            if (!catalogSettings.IgnoreAcl)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Warning,
                    Text = localizationService.GetResource("Admin.System.Warnings.Performance.IgnoreAcl")
                });
            }

            //validate write permissions (the same procedure like during installation)
            var dirPermissionsOk = true;
            var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite();
            foreach (var dir in dirsToCheck)
                if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.Wrong"), WindowsIdentity.GetCurrent().Name, dir)
                    });
                    dirPermissionsOk = false;
                }
            if (dirPermissionsOk)
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.OK")
                });

            var filePermissionsOk = true;
            var filesToCheck = FilePermissionHelper.GetFilesWrite();
            foreach (var file in filesToCheck)
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(localizationService.GetResource("Admin.System.Warnings.FilePermission.Wrong"), WindowsIdentity.GetCurrent().Name, file)
                    });
                    filePermissionsOk = false;
                }
            if (filePermissionsOk)
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = localizationService.GetResource("Admin.System.Warnings.FilePermission.OK")
                });

            return model;
        }

    }
}