/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using Microsoft.VisualBasic.ApplicationServices;

namespace VirtualRouterManager
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        App _app;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            // First time app is launched
            _app = new App();
            _app.InitializeComponent();
            _app.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            _app.Activate();
        }
    }
}
