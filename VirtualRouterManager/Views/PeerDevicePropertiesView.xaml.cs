/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System.Windows;
using Microsoft.Practices.Prism.ViewModel;
using VirtualRouterDevices.DL;
using VirtualRouterManager.AeroGlass;
using VirtualRouterManager.ViewModels;

namespace VirtualRouterManager.Views
{
	/// <summary>
	/// Interaction logic for PeerDevicePropertiesView.xaml
	/// </summary>
	public partial class PeerDevicePropertiesView : Window
	{
		public PeerDeviceView PeerDeviceView { get; private set; }

		public PeerDevicePropertiesView(PeerDeviceView peerDeviceView)
		{
			PeerDeviceView = peerDeviceView;

			InitializeComponent();

			Loaded += new RoutedEventHandler(PeerDeviceProperties_Loaded);

			UpdateDisplay();
		}

		private void PeerDeviceProperties_Loaded(object sender, RoutedEventArgs e)
		{
			AeroGlassHelper.ExtendGlass(this, (int)windowContent.Margin.Left, (int)windowContent.Margin.Right, (int)windowContent.Margin.Top, (int)windowContent.Margin.Bottom);
		}

		private void UpdateDisplay()
		{
			if (PeerDeviceView == null)
				return;

			Icon = imgDeviceIcon.Source = PeerDeviceView.imgDeviceIcon.Source;

			lblDisplayName.Content = lblDisplayName.ToolTip = Title = PeerDeviceView.lblDisplayName.Content.ToString();

			txtMACAddress.Text = PeerDeviceView.PeerDeviceViewModel.Peer.MacAddress;
			txtIPAddress.Text = PeerDeviceView.PeerDeviceViewModel.IPAddress;
			txtHostName.Text = PeerDeviceView.PeerDeviceViewModel.HostName;
		}

		private void btnChangeIcon_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new DeviceIconPickerView(lblDisplayName.Content.ToString())
			{
				Selected = DeviceManager.Load(PeerDeviceView.PeerDeviceViewModel.Peer.MacAddress).Icon,
				Owner = this
			};

			if (dialog.ShowDialog() != true)
				return;

			DeviceManager.Save(PeerDeviceView.PeerDeviceViewModel.Peer.MacAddress, dialog.Selected);
			PeerDeviceView.UpdateDeviceIcon();
			UpdateDisplay();
		}
	}
}