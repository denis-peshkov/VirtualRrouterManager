/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VirtualRouterDevices.Properties;

namespace VirtualRouterDevices.DL
{
	public static class DeviceManager
	{
		public static Device Load(string macAddress)
		{
			var devices = LoadDevices();

			//retrieve setting
			var di = (from obj in devices
					  where obj.MacAddress.Replace(":", "-").ToLowerInvariant() == macAddress.Replace(":", "-").ToLowerInvariant()
					  select obj).FirstOrDefault();

			return di ?? new Device(macAddress, DeviceEnum.Default);
		}

		/// <summary>
		/// Main storage method (should be used by others storage methods)
		/// </summary>
		/// <param name="device">device to save (will replace devise with same MAC if exist)</param>
		public static void Save(Device device)
		{
			var devices = LoadDevices();

			foreach
				(var item in
					(from i in devices
					 where i.MacAddress.Replace(":", "-").ToLowerInvariant() == device.MacAddress.Replace(":", "-").ToLowerInvariant()
					 select i
					 ).ToArray()
				)
			{
				devices.Remove(item);
			}

			device.RefreshState();
			devices.Add(device);
			SaveDevices(devices);
		}

		public static void Save(string macAddress, DeviceEnum icon)
		{
			var d = LoadDevice(macAddress);

			// if device not exist, then create it
			if (d == null)
			{
				d = new Device(macAddress, icon);
			}
			// if device exist, then update it
			else
			{
				d.Icon = icon;
			}

			Save(d);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>device (could be null if not found)</returns>
		public static Device LoadDevice(string macAddress)
		{
			var devices = LoadDevices();

			Device d = null;
			foreach (var item in
						(from i in devices
						 where i.MacAddress.Replace(":", "-").ToLowerInvariant() == macAddress.Replace(":", "-").ToLowerInvariant()
						 select i).ToArray()
					)
			{
				d = item;
			}

			return d;
		}

		public static List<Device> LoadDevices()
		{
			if (string.IsNullOrEmpty(Settings.Default.Devices))
			{
				return new List<Device>();
			}
			else
			{
				try
				{
					return JSONHelper.Deserialize<List<Device>>(Settings.Default.Devices);
				}
				catch
				{
					return new List<Device>();
				}
			}
		}

		private static void SaveDevices(List<Device> devices)
		{
			Settings.Default.Devices = JSONHelper.Serialize<List<Device>>(devices);
			Settings.Default.Save();
		}
	}
}