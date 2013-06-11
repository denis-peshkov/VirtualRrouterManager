using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Practices.Prism.ViewModel;
using VirtualRouterDevices;
using VirtualRouterDevices.DL;
using VirtualRouterManager.VirtualRouterService;

namespace VirtualRouterManager.ViewModels
{
	public class PeerDeviceViewModel : NotificationObject
	{
		public PeerDeviceViewModel(ConnectedPeer peer)
		{
			Peer = peer;
		}

		public event EventHandler DataChanged;

		private BackgroundWorker _backgroundWorker;

		private ConnectedPeer _peer;
		public ConnectedPeer Peer
		{
			get { return _peer; }

			private set
			{
				_peer = value;

				// get Device data into private field
				SetDevice(_peer.MacAddress);

				// renew information about device on controls
				RaseDataChanged();

				// try to retrive the IP address
				_backgroundWorker = new BackgroundWorker() { WorkerReportsProgress = true };
				_backgroundWorker.DoWork += bw_DoWork;
				_backgroundWorker.RunWorkerCompleted += bw_RunWorkerCompleted;
				_backgroundWorker.ProgressChanged += bw_ProgressChanged;
				_backgroundWorker.RunWorkerAsync(null);
			}
		}

		private void RaseDataChanged()
		{
			if (DataChanged != null)
				DataChanged(this, null);
		}

		public Device Device { get; private set; }

		private void SetDevice(string macAddress)
		{
			if (Device == null || Device.MacAddress != macAddress)
			{
				Device = DeviceManager.Load(macAddress);
			}
		}

		public string DisplayName { get; set; }
		public string MACAddress { get; set; }
		public string IPAddress { get; set; }
		public string HostName { get; set; }

		#region work in Thread

		private void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			GetIpInfo(this);
		}

		private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			RaseDataChanged();
		}

		private static void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{

		}

		private void GetIpInfo(object data)
		{
			var ipinfoFound = false;
			var count = 0;
			const int maxTry = 10;

			// count is used to only check a certain number of times before giving up
			// This will keep the thread from preventing the app from exiting when the user closes the main window.

			while (!ipinfoFound && count < maxTry)
			{
				try
				{
					var pd = data as PeerDeviceViewModel;
					var ipinfo = IPInfo.GetIPInfo(pd.Peer.MacAddress.Replace(":", "-"));
					if (ipinfo != null)
					{
						Dispatcher.CurrentDispatcher.Invoke((Action)delegate()
						{
							SetIPInfoDisplay(ipinfo);
						});

						ipinfoFound = true;
					}
					else
					{
						_backgroundWorker.ReportProgress(maxTry / count * 100);
						Thread.Sleep(1000);
					}
				}
				catch { }

				count++;
			}
			if (!ipinfoFound)
			{
				Dispatcher.CurrentDispatcher.Invoke((Action)SetIPNotFound);
			}
			//lock (this)
			//{
			//    if (DataChanged != null)
			//        DataChanged(this, null);
			//}
		}

		private void SetIPNotFound()
		{
			MACAddress = "Host Name could not be found.";
			IPAddress = "IP Address could not be found.";
		}

		private void SetIPInfoDisplay(IPInfo ipinfo)
		{
			DisplayName = Device.Name ?? ipinfo.HostName;

			MACAddress = "MAC: " + ipinfo.MacAddress;
			IPAddress = "IP: " + ipinfo.IPAddress;
			
			HostName = ipinfo.HostName;
		}

		#endregion

		public void IncrementConnections()
		{
			Device.Connections++;
			RaseDataChanged();
		}

		public object GetToolTip()
		{
			return Device.Name + Environment.NewLine +
				   "Vendor: " + Device.Icon + Environment.NewLine +
				   "HostName: " + HostName + Environment.NewLine +
				   "MAC: " + Device.MacAddress + Environment.NewLine +
				   "IP address: " + IPAddress + Environment.NewLine +
				   "First seen: " + Device.FirstSeen + Environment.NewLine +
				   "Last seen: " + Device.LastSeen + Environment.NewLine +
				   "Connections: " + Device.Connections + Environment.NewLine +
				   "Time connected: " + Device.TimeConnected;
		}
	}
}