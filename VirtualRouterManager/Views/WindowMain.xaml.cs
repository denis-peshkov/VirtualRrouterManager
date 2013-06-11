/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using VirtualRouterDevices.DL;
using VirtualRouterManager.AeroGlass;
using VirtualRouterManager.Properties;
using VirtualRouterManager.VirtualRouterService;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace VirtualRouterManager.Views
{
	/// <summary>
	/// Interaction logic for WindowMain.xaml
	/// </summary>
	public partial class WindowMain : Window
	{
		private App myApp = (App)App.Current;
		private Thread _threadUpdateUI;

		private WpfNotifyIcon _trayIcon;

		public WindowMain()
		{
			InitializeComponent();

			Loaded += new RoutedEventHandler(WindowMain_Loaded);
			Closing += new CancelEventHandler(WindowMain_Closing);

			myApp.VirtualRouterServiceConnected += new EventHandler(myApp_VirtualRouterServiceConnected);
			myApp.VirtualRouterServiceDisconnected += new EventHandler(myApp_VirtualRouterServiceDisconnected);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			LoadDevicesToDisconected();
		}

		#region WindowMain Methods

		private void WindowMain_Closing(object sender, CancelEventArgs e)
		{
			Settings.Default.SSID = txtSSID.Text;
			Settings.Default.Password = txtPassword.Text;
			var sharableConnection = cbSharedConnection.SelectedItem as SharableConnection;
			if (sharableConnection != null)
				Settings.Default.SharedConnectionGuid = sharableConnection.Guid;
			Settings.Default.Save();
		}

		private void WindowMain_Loaded(object sender, RoutedEventArgs e)
		{
			AeroGlassHelper.ExtendGlass(this, (int)windowContent.Margin.Left, (int)windowContent.Margin.Right, (int)windowContent.Margin.Top, (int)windowContent.Margin.Bottom);

			txtSSID.Text = Settings.Default.SSID;
			txtPassword.Text = Settings.Default.Password;
			// Составляем список доступных коннектов
			FillSharedConnection(Settings.Default.SharedConnectionGuid);

			// This line is for testing purposes
			//panelConnections.Children.Add(new PeerDeviceView(new ConnectedPeer() { MacAddress = "AA-22-33-EE-EE-FF" }));

			var args = Environment.GetCommandLineArgs();
			var minarg = (from a in args
						  where a.ToLowerInvariant().Contains("/min")
						  select a).FirstOrDefault();
			if (!string.IsNullOrEmpty(minarg))
			{
				WindowState = WindowState.Minimized;
				ShowInTaskbar = false;
			}

			AddSystemMenuItems();

			_threadUpdateUI = new Thread(new ThreadStart(UpdateUIThread));
			_threadUpdateUI.Start();

			Closed += new EventHandler(WindowMain_Closed);

			// Show System Tray Icon
			var stream = Application.GetResourceStream(new Uri("/icons/virtualrouterdisabled.ico", UriKind.Relative)).Stream;
			var icon = new System.Drawing.Icon(stream);
			_trayIcon = new WpfNotifyIcon();
			_trayIcon.Icon = icon;
			_trayIcon.Show();
			_trayIcon.Text = "Virtual Router (Disabled)";
			_trayIcon.DoubleClick += new EventHandler(TrayIcon_DoubleClick);

			var trayMenu = new ContextMenuStrip();
			trayMenu.Items.Add("&Manage Virtual Router...", null, new EventHandler(TrayIcon_Menu_Manage));
			trayMenu.Items.Add(new ToolStripSeparator());
			trayMenu.Items.Add("Check for &Updates...", null, new EventHandler(TrayIcon_Menu_Update));
			trayMenu.Items.Add("&About...", null, new EventHandler(TrayIcon_Menu_About));
			_trayIcon.ContextMenuStrip = trayMenu;

			StateChanged += new EventHandler(WindowMain_StateChanged);

			UpdateDisplay();
		}

		void WindowMain_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				ShowInTaskbar = false;
			}
			else
			{
				ShowInTaskbar = true;
			}
		}

		private void WindowMain_Closed(object sender, EventArgs e)
		{
			_threadUpdateUI.Abort();
			SaveDeviceStates();
			_trayIcon.Hide();
			_trayIcon.Dispose();
		}

		private void SaveDeviceStates()
		{
			foreach (var p in GetAllPeerDevices())
			{
				DeviceManager.Save(p.PeerDeviceViewModel.Device);
			}
		}

		#endregion

		#region TrayIcon Methods

		void TrayIcon_Menu_Update(object sender, EventArgs e)
		{
			CheckUpdates();
		}

		void TrayIcon_Menu_About(object sender, EventArgs e)
		{
			ShowAboutBox();
		}

		void TrayIcon_Menu_Manage(object sender, EventArgs e)
		{
			WindowState = WindowState.Normal;
		}

		void TrayIcon_DoubleClick(object sender, EventArgs e)
		{
			WindowState = WindowState.Normal;
		}

		#endregion

		#region myApp Methods

		void myApp_VirtualRouterServiceDisconnected(object sender, EventArgs e)
		{
			lblStatus.Content = "Can not manage Virtual Router. The Service is not running.";
			_trayIcon.Text = "Virtual Router (Disabled)";
			UpdateDisplay();
		}

		void myApp_VirtualRouterServiceConnected(object sender, EventArgs e)
		{
			lblStatus.Content = "Virtual Router can now be managed.";
			UpdateDisplay();
		}

		#endregion

		public static void CheckUpdates()
		{
			Process.Start("http://virtualroutermanager.codeplex.com");
		}

		#region Update UI

		void UpdateUIThread()
		{
			while (true)
			{
				Dispatcher.Invoke(new Action(UpdateDisplay));
				Thread.Sleep(5000); // 5 Seconds
			}
		}

		private bool _isVirtualRouterRunning = false;
		private void UpdateUIDisplay()
		{
			var enableToggleButton = false;
			var enableSettingsFields = false;

			if (myApp.IsVirtualRouterServiceConnected)
			{
				enableToggleButton = true;
				try
				{
					btnToggleHostedNetwork.IsEnabled = true;
					if (myApp.VirtualRouter.IsStarted())
					{
						enableSettingsFields = false;
						btnToggleHostedNetwork.Content = "Stop Virtual Router";
						_trayIcon.Text = "Virtual Router (Running)";
						_trayIcon.Icon = new System.Drawing.Icon(
							Application.GetResourceStream(new Uri("/icons/virtualrouterenabled.ico", UriKind.Relative)).Stream
							);

						if (!_isVirtualRouterRunning)
						{
							var streamResourceInfo = Application.GetResourceStream(new Uri("/Icons/VirtualRouterEnabled.ico", UriKind.Relative));
							if (streamResourceInfo != null)
								Icon = imgIcon.Source = BitmapFrame.Create(streamResourceInfo.Stream);
						}
						_isVirtualRouterRunning = true;

						txtSSID.Text = myApp.VirtualRouter.GetConnectionSettings().SSID;
						txtPassword.Text = myApp.VirtualRouter.GetPassword();
					}
					else
					{
						enableSettingsFields = true;
						btnToggleHostedNetwork.Content = "Start Virtual Router";
						_trayIcon.Text = "Virtual Router (Stopped)";
						var streamResourceInfo = Application.GetResourceStream(new Uri("/icons/virtualrouterdisabled.ico", UriKind.Relative));
						if (streamResourceInfo != null)
							_trayIcon.Icon = new System.Drawing.Icon(streamResourceInfo.Stream);

						if (_isVirtualRouterRunning)
						{
							var resourceStream = Application.GetResourceStream(new Uri("/Icons/VirtualRouterDisabled.ico", UriKind.Relative));
							if (resourceStream != null)
								Icon = imgIcon.Source = BitmapFrame.Create(resourceStream.Stream);
						}
						_isVirtualRouterRunning = false;
					}
				}
				catch
				{
					enableToggleButton = false;
					enableSettingsFields = false;
				}
			}

			btnToggleHostedNetwork.IsEnabled = enableToggleButton;

			txtSSID.IsEnabled = enableSettingsFields;
			txtPassword.IsEnabled = enableSettingsFields;
			cbSharedConnection.IsEnabled = enableSettingsFields;
			btnRefreshSharableConnections.IsEnabled = enableSettingsFields;
			//gbVirtualRouterSettings.IsEnabled = enableSettingsFields;
		}

		private void UpdateDisplay()
		{
			UpdateUIDisplay();

			RefreshSharableConnectionsDisplay();

			if (myApp.IsVirtualRouterServiceConnected)
			{
				//panelConnections.Children.Clear();
				var peers = myApp.VirtualRouter.GetConnectedPeers();
				groupBoxPeersConnected.Header = "Connected Devices (" + peers.Count().ToString() + "):";

				if (peers.Any())
				{
					MoveDisconnectedPeersToConnected(peers);
					foreach (var p in peers)
					{
						if (!IsPeerAlreadyConnected(p))
						{
							panelConnections.Children.Add(new PeerDeviceView(p));
						}
					}
				}
				MoveConnectedPeersToDisconnected(peers);
			}
			else
			{
				groupBoxPeersConnected.Header = "Connected Devices (0):";
			}

			RefreshPeerDeviceViews();
		}

		private void RefreshPeerDeviceViews()
		{
			foreach (var p in GetAllPeerDevices())
			{
				p.UpdateDeviceUI();
			}
		}

		#endregion

		#region Panel Helpers

		private IEnumerable<PeerDeviceView> GetAllPeerDevices()
		{
			var list = (from object p in panelConnections.Children 
						where p != null && p is PeerDeviceView 
						select p as PeerDeviceView).ToList();

			list.AddRange(from object p in panelConnectionsPreviously.Children 
						  where p != null && p is PeerDeviceView 
						  select p as PeerDeviceView);

			return list;
		}

		private bool IsPeerAlreadyConnected(ConnectedPeer peer)
		{
			return panelConnections.Children.OfType<PeerDeviceView>().Any(e => e.PeerDeviceViewModel.Peer.MacAddress.ToLowerInvariant() == peer.MacAddress.ToLowerInvariant());
		}

		private void MovePeersFromPanelToPanel(ConnectedPeer[] peers, WrapPanel fromPanel, WrapPanel toPanel, bool activeCondition)
		{
			if (fromPanel.Children.Count == 0)
				return;

			// создаем пустой список
			var peersToMove = new List<PeerDeviceView>();

			// выбираем элементы, которые не отмечены как подключенные
			foreach (var element in fromPanel.Children)
			{
				var elem = element as PeerDeviceView;
				if (elem != null)
				{
					var exists = peers.Any(p => p.MacAddress.ToLowerInvariant() == elem.PeerDeviceViewModel.Peer.MacAddress.ToLowerInvariant());
					if (exists == activeCondition)
					{
						peersToMove.Add(elem);
					}
				}
			}

			// производим их перемещение со списка подключенных в список отключенных
			foreach (var elem in peersToMove)
			{
				fromPanel.Children.Remove(elem);
				toPanel.Children.Add(elem);
				if (Equals(toPanel, panelConnections))
					elem.IncrementConnections();
				
			}
		}

		private void MoveDisconnectedPeersToConnected(ConnectedPeer[] peers)
		{
			MovePeersFromPanelToPanel(peers, panelConnectionsPreviously, panelConnections, true);
		}

		private void MoveConnectedPeersToDisconnected(ConnectedPeer[] peers)
		{
			MovePeersFromPanelToPanel(peers, panelConnections, panelConnectionsPreviously, false);
		}

		private void LoadDevicesToDisconected()
		{
			var devices = DeviceManager.LoadDevices();
			foreach (var dev in devices)
			{
				var peer = new ConnectedPeer();
				peer.MacAddress = dev.MacAddress;
				panelConnectionsPreviously.Children.Add(new PeerDeviceView(peer));
			}
		}

		#endregion

		#region System Menu Stuff

		#region Win32 API Stuff

		// Define the Win32 API methods we are going to use
		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("user32.dll")]
		private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

		/// Define our Constants we will use
		public const Int32 WM_SYSCOMMAND = 0x112;
		public const Int32 MF_SEPARATOR = 0x800;
		public const Int32 MF_BYPOSITION = 0x400;
		public const Int32 MF_STRING = 0x0;

		#endregion

		private const int ABOUT_SYSMENU_ID = 1001;
		private const int UPDATE_SYSMENU_ID = 1002;

		private void AddSystemMenuItems()
		{
			IntPtr windowHandle = new WindowInteropHelper(this).Handle;
			IntPtr systemMenu = GetSystemMenu(windowHandle, false);

			InsertMenu(systemMenu, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
			InsertMenu(systemMenu, 6, MF_BYPOSITION, UPDATE_SYSMENU_ID, "Check for Updates...");
			InsertMenu(systemMenu, 7, MF_BYPOSITION, ABOUT_SYSMENU_ID, "About...");

			HwndSource source = HwndSource.FromHwnd(windowHandle);
			source.AddHook(new HwndSourceHook(WndProc));
		}

		private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			// Check if a System Command has been executed
			if (msg == WM_SYSCOMMAND)
			{
				// Execute the appropriate code for the System Menu item that was clicked
				switch (wParam.ToInt32())
				{
					case ABOUT_SYSMENU_ID:
						ShowAboutBox();
						handled = true;
						break;
					case UPDATE_SYSMENU_ID:
						CheckUpdates();
						handled = true;
						break;
				}
			}

			return IntPtr.Zero;
		}

		#endregion

		static void ShowAboutBox()
		{
			MessageBox.Show(
							AssemblyAttributes.AssemblyProduct + " " + AssemblyAttributes.AssemblyVersion + Environment.NewLine
							+ Environment.NewLine + AssemblyAttributes.AssemblyDescription + Environment.NewLine
							+ Environment.NewLine + "Licensed under the Microsoft Public License (Ms-PL)" + Environment.NewLine
							+ Environment.NewLine + AssemblyAttributes.AssemblyCopyright + Environment.NewLine
							+ Environment.NewLine + "http://virtualroutermanager.codeplex.com"

							, "About " + AssemblyAttributes.AssemblyProduct + "...");
		}

		private void btnToggleHostedNetwork_Click(object sender, RoutedEventArgs e)
		{
			if (myApp.IsVirtualRouterServiceConnected)
			{
				if (myApp.VirtualRouter.IsStarted())
				{
					myApp.VirtualRouter.Stop();
				}
				else
				{
					if (ValidateFields())
					{

						myApp.VirtualRouter.SetConnectionSettings(txtSSID.Text, 100);
						myApp.VirtualRouter.SetPassword(txtPassword.Text);

						if (!myApp.VirtualRouter.Start((SharableConnection)cbSharedConnection.SelectedItem))
						{
							string strMessage = myApp.VirtualRouter.GetLastError() ?? "Virtual Router Could Not Be Started!";
							lblStatus.Content = strMessage;
							MessageBox.Show(strMessage, Title);
						}
						else
						{
							lblStatus.Content = "Virtual Router Started...";
						}

					}
				}
			}

			UpdateUIDisplay();
		}

		private bool ValidateFields()
		{
			var errorMessage = string.Empty;

			if (txtSSID.Text.Length <= 0)
			{
				errorMessage += "Network Name (SSID) is required." + Environment.NewLine;
			}

			if (txtSSID.Text.Length > 32)
			{
				errorMessage += "Network Name (SSID) can not be longer than 32 characters." + Environment.NewLine;
			}

			if (txtPassword.Text.Length < 8)
			{
				errorMessage += "Password must be at least 8 characters." + Environment.NewLine;
			}

			if (txtPassword.Text.Length > 64)
			{
				errorMessage += "Password can not be longer than 64 characters." + Environment.NewLine;
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}

			return true;
		}

		private void RefreshSharableConnectionsDisplay()
		{
			if (!myApp.IsVirtualRouterServiceConnected) return;

			cbSharedConnection.DisplayMemberPath = "Name";

			var selectedId = Guid.Empty;
			if (myApp.VirtualRouter.IsStarted())
			{
				var sharedConn = myApp.VirtualRouter.GetSharedConnection();
				if (sharedConn != null)
				{
					selectedId = sharedConn.Guid;
				}
			}
			else
			{
				var previousItem = cbSharedConnection.SelectedItem as SharableConnection;
				if (previousItem != null)
				{
					selectedId = previousItem.Guid;
				}
			}

			// Составляем список доступных коннектов
			FillSharedConnection(selectedId);
		}

		private void FillSharedConnection(Guid connection)
		{
			var connections = myApp.VirtualRouter.GetSharableConnections();
			cbSharedConnection.Items.Clear();
			foreach (var c in connections)
			{
				cbSharedConnection.Items.Add(c);
				if (c.Guid == connection)
				{
					cbSharedConnection.SelectedItem = c;
				}
			}
		}

		private void btnRefreshSharableConnections_Click(object sender, RoutedEventArgs e)
		{
			RefreshSharableConnectionsDisplay();
		}
	}
}