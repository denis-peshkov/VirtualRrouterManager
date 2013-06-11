/*
* Virtual Router Manager v1.0 - http://virtualroutermanager.codeplex.com
* Wifi Hot Spot for Windows 7, 8 and 2008 R2
* Copyright (c) 2013 Denis Peshkov (http://maton.com.ua)
* Licensed under the Microsoft Public License (Ms-PL)
* http://virtualroutermanager.codeplex.com/license
*/

using System;
using Microsoft.Practices.Prism.ViewModel;

namespace VirtualRouterDevices.DL
{
	public class Device : NotificationObject
	{
		#region Constructors

		public Device()
		{
			FirstSeen = DateTime.Now;
			LastSeen = DateTime.Now;
			LastConnected = DateTime.Now;
			TimeConnected = TimeSpan.Zero;
		}

		public Device(string macAddress, DeviceEnum icon)
			:this()
		{
			MacAddress = macAddress;
			Icon = icon;
		}

		#endregion

		/// <summary>
		/// Мак адрес
		/// </summary>
		public string MacAddress { get; set; }

		/// <summary>
		/// Иконка девайса
		/// </summary>
		public DeviceEnum Icon { get; set; }

		/// <summary>
		/// Имя девайса
		/// </summary>
		public string Name { get; set; }

		#region Accesses

		/// <summary>
		/// Разрешен ли доступ в сеть
		/// </summary>
		public bool AllowNetAccess { get; set; }

		/// <summary>
		/// Разрешен ли доступ в интернет
		/// </summary>
		public bool AllowWebAccess { get; set; }

		#endregion

		#region Automatic statistics

		/// <summary>
		/// Дата первого коннекта
		/// </summary>
		public DateTime FirstSeen { get; set; }

		/// <summary>
		/// Дата, когда девайс был онлайн последний раз
		/// </summary>
		public DateTime LastSeen { get; set; }

		/// <summary>
		/// Колличество раз подключений
		/// </summary>
		public int Connections { get; set; }

		/// <summary>
		/// Дата последнего подключения (исходя от этой даты считается время последнего подключения)
		/// </summary>
		private DateTime LastConnected { get; set; }

		/// <summary>
		/// Время подключения (суммарное)
		/// </summary>
		public TimeSpan TimeConnected { get; set; }

		public void RefreshState()
		{
			LastSeen = DateTime.Now;

			// каждый раз при обновлении, происходит перерасчет суммарного времени и обнуление времени последнего подключения
			TimeConnected += LastSeen - LastConnected;
			LastConnected = DateTime.Now;

			// информируем, что следующие свойства изменились
			RaisePropertyChanged("LastSeen");
			RaisePropertyChanged("TimeConnected");
		}

		#endregion
	}
}