/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VirtualRouterDevices.DL;
using VirtualRouterManager.ViewModels;
using VirtualRouterManager.VirtualRouterService;

namespace VirtualRouterManager.Views
{
	/// <summary>
	/// Interaction logic for PeerDeviceViewModel.xaml
	/// </summary>
	public partial class PeerDeviceView : UserControl
	{
		internal PeerDeviceViewModel PeerDeviceViewModel;

		public PeerDeviceView(ConnectedPeer peer)
		{
			InitializeComponent();

			#region ContextMenu

			ContextMenu = new ContextMenu();

			MenuItem propertiesMenuItem;

			propertiesMenuItem = new MenuItem { Header = "_Rename" };
			propertiesMenuItem.Click += new RoutedEventHandler(PropertiesMenuItem_Click);
			ContextMenu.Items.Add(propertiesMenuItem);

			propertiesMenuItem = new MenuItem { Header = "_Properties..." };
			propertiesMenuItem.Click += new RoutedEventHandler(PropertiesMenuItem_Click);
			ContextMenu.Items.Add(propertiesMenuItem);

			propertiesMenuItem = new MenuItem { Header = "Disable _Internet Access" };
			propertiesMenuItem.Click += new RoutedEventHandler(PropertiesMenuItem_Click);
			ContextMenu.Items.Add(propertiesMenuItem);

			propertiesMenuItem = new MenuItem { Header = "Disable _Local Access" };
			propertiesMenuItem.Click += new RoutedEventHandler(PropertiesMenuItem_Click);
			ContextMenu.Items.Add(propertiesMenuItem);

			#endregion

			// Информация будет обновлена потоком, по мере ее получения
			lblMACAddress.Content = "Retrieving Host Name...";
			lblIPAddress.Content = "Retrieving IP Address...";
		
			PeerDeviceViewModel = new PeerDeviceViewModel(peer);
			PeerDeviceViewModel.DataChanged += new EventHandler(OnDataChanged);

		}

		#region Update GUI

		/// <summary>
		/// Метод отвечающий за изменение GUI при изменении данных
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDataChanged(object sender, EventArgs e)
		{
			UpdateDeviceUI();
		}

		internal void UpdateDeviceUI()
		{
			PeerDeviceViewModel.Device.RefreshState();

			lblDisplayName.Content = PeerDeviceViewModel.DisplayName;
			lblMACAddress.Content = PeerDeviceViewModel.MACAddress;
			lblIPAddress.Content = PeerDeviceViewModel.IPAddress;

			UpdateDeviceIcon();
			UpdateToolTip();
		}

		public void UpdateDeviceIcon()
		{
			var resourceName = PeerDeviceViewModel.Device.Icon.ToResourceName();
			imgDeviceIcon.Source = (ImageSource)FindResource(resourceName);
		}

		private void UpdateToolTip()
		{
			stackPanel1.ToolTip = PeerDeviceViewModel.GetToolTip();
			imgDeviceIcon.ToolTip = PeerDeviceViewModel.GetToolTip();
		}

		#endregion

		public void IncrementConnections()
		{
			PeerDeviceViewModel.IncrementConnections();
			UpdateToolTip();
		}

		private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ShowPropertiesDialog();
		}

		private void PropertiesMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ShowPropertiesDialog();
		}

		public void ShowPropertiesDialog()
		{
			var window = new PeerDevicePropertiesView(this) { Owner = App.Current.MainWindow };
			window.ShowDialog();
		}

	}
}