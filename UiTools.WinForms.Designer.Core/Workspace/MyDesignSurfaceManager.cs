using System;
using System.ComponentModel.Design;

namespace UiTools.WinForms.Designer.Core
{
    public class MyDesignSurfaceManager : DesignSurfaceManager
    {
        public MyDesignSurfaceManager() : base()
        {
            // In theory, services that will be global for all DesignSurface instances managed by this manager can be added here.
            // For example: this.ServiceContainer.AddService(typeof(IMenuCommandService), new MyMenuCommandService(this));
        }

        protected override DesignSurface CreateDesignSurfaceCore(IServiceProvider parentProvider)
        {
            return new DesignSurfaceEx(parentProvider);
        }
    }
}
