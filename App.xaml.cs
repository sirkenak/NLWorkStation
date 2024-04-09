using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using log4net;
using log4net.Config;
using NashLabelWorkStation.Model;
using NashLabelWorkStation.Properties;
using NashLabelWorkStation.ViewModel;

namespace NashLabelWorkStation
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		#region Fields

		private ILog _log;

		#endregion //Fields

		#region Constructors

		public App()
		{
			_log = LogManager.GetLogger( GetType() );
		}

		#endregion //Constructors

		#region Methods

		private void Initialize()
		{
			XmlConfigurator.Configure();

			//_log.Info( $"Запуск программы. Версия: {VersionConsts.VersionString}" );

			AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		}

		private void Deinitialize()
		{
			_log.Info( $"Программа закрыта. Версия: {VersionConsts.VersionString}" );
			LogManager.Shutdown();
		}

		#endregion //Methods

		#region Event Handlers

		private void OnApplicationStartup( object sender, StartupEventArgs e )
		{

			string fileName = e.Args?.FirstOrDefault();

			//if ( !String.IsNullOrEmpty( fileName ) )
			//{
			//	RegistersService.SetRegisterValueByName( "ActiveDocument", fileName, out string error );
			//}
			//Window printWin = new PrintingWindow();
			Initialize();
			CommonModel model = new CommonModel();
			MainWindow mainWin = new MainWindow();

			MainWindowViewModel mvm = new MainWindowViewModel( model, mainWin );
			mainWin.DataContext = mvm;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture( "ru-RU" );
			try
			{
				mainWin.ShowDialog();
				mvm.StopMonitoring();
			}
			catch
			{

			}
		}

		private void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			Exception exception = e.ExceptionObject as Exception;

			if ( e.IsTerminating )
			{
				if ( exception != null )
					_log.Fatal( "Unhandled exception", exception );
				else
					_log.Fatal( "Unhandled exception" );
				Deinitialize();
			}
			else
			{
				if ( exception != null )
					_log.Error( "Unhandled exception", exception );
				else
					_log.Error( "Unhandled exception" );
			}
		}

		private void OnApplicationExit( object sender, ExitEventArgs e )
		{
			Deinitialize();
		}

		#endregion //Event Handlers
	}
}
