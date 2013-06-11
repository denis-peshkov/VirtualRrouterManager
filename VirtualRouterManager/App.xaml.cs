/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System;
using System.Threading;
using System.Windows;
using VirtualRouterManager.VirtualRouterService;

namespace VirtualRouterManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private Thread _threadServiceChecker;

		public void Activate()
		{
			MainWindow.WindowState = WindowState.Normal;
			MainWindow.Activate();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ConnectService();

			_threadServiceChecker = new Thread(new ThreadStart(ServiceChecker));
			_threadServiceChecker.Start();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_threadServiceChecker.Abort();
			base.OnExit(e);
		}

		private void ServiceChecker()
		{
			while (true)
			{
				if (VirtualRouter.State == System.ServiceModel.CommunicationState.Faulted)
				{
					VirtualRouter = null;
				}

				if (VirtualRouter == null)
				{
					ConnectService();
				}

				Thread.Sleep(5000);
			}
		}

		private void ConnectService()
		{
			if (VirtualRouter == null)
			{
				VirtualRouter = new VirtualRouterHostClient();
				VirtualRouter.InnerChannel.Faulted += new EventHandler(InnerChannel_Faulted);
				VirtualRouter.InnerChannel.Closed += new EventHandler(InnerChannel_Closed);
				VirtualRouter.InnerChannel.Opened += new EventHandler(InnerChannel_Opened);
				try
				{
					VirtualRouter.Open();
				}
				catch { }
			}
		}

		public VirtualRouterHostClient VirtualRouter { get; private set; }

		public event EventHandler VirtualRouterServiceConnected;
		public event EventHandler VirtualRouterServiceDisconnected;

		private void InvokeVirtualRouterServiceDisconnected()
		{
			if (VirtualRouterServiceDisconnected != null)
			{
				Dispatcher.Invoke(VirtualRouterServiceDisconnected, this, EventArgs.Empty);
			}
		}

		private void InnerChannel_Opened(object sender, EventArgs e)
		{
			IsVirtualRouterServiceConnected = true;
			if (VirtualRouterServiceConnected != null)
			{
				Dispatcher.Invoke(VirtualRouterServiceConnected, this, EventArgs.Empty);
			}
		}

		private void InnerChannel_Closed(object sender, EventArgs e)
		{
			IsVirtualRouterServiceConnected = false;
			InvokeVirtualRouterServiceDisconnected();
		}

		private void InnerChannel_Faulted(object sender, EventArgs e)
		{
			IsVirtualRouterServiceConnected = false;
			InvokeVirtualRouterServiceDisconnected();
		}

		public App()
		{
			IsVirtualRouterServiceConnected = false;
		}

		public bool IsVirtualRouterServiceConnected { get; private set; }
	}
}