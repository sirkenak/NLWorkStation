using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using log4net;
using NashLabelWorkStation.Model;
using NashLabelWorkStation.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[assembly: log4net.Config.XmlConfigurator( ConfigFile = "log4net.config" )]

namespace NashLabelWorkStation.ViewModel
{
	public class MainWindowViewModel : DependencyObject, INotifyPropertyChanged
	{

		#region Fields

		private readonly CommonModel _model;
		private JsonSerializerSettings _serializerSettings;

		/// </summary>
		private readonly Window _owner;

		#endregion //Fields

		public string OPCUrl;
		//public string LineNumberVar;
		public string PrintServiceIp;
		public OpcClient NlOpcClient;
		public int Mode; // 0 - автономный, 1- штатный

		//public OpcClient NlOpcClient
		//{
		//	get { return (OpcClient)GetValue( OpcClientProperty ); }
		//	set { SetValue( OpcClientProperty, value ); }
		//}

		//public static readonly DependencyProperty OpcClientProperty =
		//	DependencyProperty.Register( nameof( NlOpcClient ), typeof( OpcClient ), typeof( MainWindowViewModel ), new PropertyMetadata( null ) );

		public string _receivedLineNumber;
		public string ReceivedLineNumber
		{
			get
			{
				return _receivedLineNumber;
			}
			set
			{
				_receivedLineNumber = value;
				OnPropertyChanged( "ReceivedLineNumber" );
			}
		}

		public string _connectOPCButtonText;
		public string ConnectOPCButtonText
		{
			get
			{
				return _connectOPCButtonText;
			}
			set
			{
				_connectOPCButtonText = value;
				OnPropertyChanged( "ConnectOPCButtonText" );
			}
		}


		public string LineNumberVar
		{
			get { return (string)GetValue( LineNumberVarProperty ); }
			set { SetValue( LineNumberVarProperty, value ); }
		}

		public static readonly DependencyProperty LineNumberVarProperty =
			DependencyProperty.Register( nameof( LineNumberVar ), typeof( string ), typeof( MainWindowViewModel ), new PropertyMetadata( String.Empty ) );


		//public string ReceivedLineNumber
		//{
		//	get { return (string)GetValue( ReceivedLineNumberProperty ); }
		//	set { SetValue( ReceivedLineNumberProperty, value ); }
		//}

		//public static readonly DependencyProperty ReceivedLineNumberProperty =
		//	DependencyProperty.Register( nameof( ReceivedLineNumber ), typeof( string ), typeof( MainWindowViewModel ), new PropertyMetadata( String.Empty ) );


		public static ILog logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		#region Commands

		public DelegateCommand ConnectOPCCommand { get; set; }
		public DelegateCommand DisconnectOPCCommand { get; set; }
		public DelegateCommand SetVarCommand { get; set; }

		#endregion Commands

		#region Constructors

		public MainWindowViewModel( CommonModel model, Window window )
		{
			logger = LogManager.GetLogger( GetType() );
			_model = model;
			_owner = window;
			_serializerSettings = new JsonSerializerSettings();
			_serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			logger.Debug("Work station started");
			ReceivedLineNumber = "";
			ConnectOPCButtonText = "Подключиться к OPC серверу";
			Mode = 0;

			ConnectOPCCommand = new DelegateCommand( OnConnectOPC );
			DisconnectOPCCommand = new DelegateCommand( OnDisconnectOPC );
			SetVarCommand = new DelegateCommand( OnSetOPCVar );

			logger.Info( "Start OCP Test" );
			//Console.WriteLine("OCP test start");
			try
			{
				IniFile INI = new IniFile( "config.ini" );
				try
				{
					PrintServiceIp = INI.ReadINI( "OPSServerSetting", "PrintServiceIp" );
				}
				catch
				{
					PrintServiceIp = "localhost";
				}

				try
				{
					OPCUrl = INI.ReadINI( "OPSServerSetting", "OPCUrl" );
				}
				catch
				{
					//OPCUrl = "opcda://localhost/Lectus.OPC.1";
					OPCUrl = "opcda://localhost/opcserversim.Instance.1";
				}

				try
				{
					LineNumberVar = INI.ReadINI( "OPSServerSetting", "LineNumberVar" );
				}
				catch
				{
					//LineNumberVar = "Etichettatrice_2CPU 314.IntegerValue";
					LineNumberVar = "IntegerValue";
				}


				if ( String.IsNullOrEmpty( OPCUrl ) ) OPCUrl = "opcda://localhost/opcserversim.Instance.1";
				if ( String.IsNullOrEmpty( LineNumberVar ) ) LineNumberVar = "IntegerValue";
				if ( String.IsNullOrEmpty( PrintServiceIp ) ) PrintServiceIp = "localhost";

				INI.Write( "OPSServerSetting", "OPCUrl", OPCUrl );
				INI.Write( "OPSServerSetting", "LineNumberVar", LineNumberVar );
				INI.Write( "OPSServerSetting", "PrintServiceIp", PrintServiceIp );
				//NlOpcClient = new OpcClient( OPCUrl, LineNumberVar, PrintServiceIp );
				//NlOpcClient.LineNumber = "125475";
				//NlOpcClient.onRecieveLineNumber += LineNumberRecieved;

				//NlOpcClient.startMonitoring();

				//opcClient.Stop();
			}
			catch ( Exception exc )
			{
				logger.Error( $"OCP creating server error: {exc.Message}" );
			}

		}

		private void OnSetOPCVar()
		{
			try
			{
				//NlOpcClient.SetOpcVar ("StringValue","aaaaaa");
				int a = 99;
				NlOpcClient.server.Write("ByteValue",a.ToString());
			}
			catch ( Exception exc )
			{

			}
		}

		private void OnDisconnectOPC()
		{
			try
			{
				NlOpcClient.Stop();
			}
			catch ( Exception exc )
			{

			}
		}

		private void OnConnectOPC()
		{
			try
			{
				switch ( Mode )
				{
					case 0:

						NlOpcClient = new OpcClient( OPCUrl, LineNumberVar, PrintServiceIp );
						if ( NlOpcClient.server.IsConnected )
						{
							ConnectOPCButtonText = "Отключиться от OPC сервера";
							Mode = 1;
							NlOpcClient.LineNumber = "125475";
							NlOpcClient.onRecieveLineNumber += LineNumberRecieved;
							NlOpcClient.startMonitoring();
							logger.Info( $"Подключено к OPC серверу {OPCUrl}" );
						}
						else
						{
							MessageBox.Show( $"Ошибка подключения к OPC серверу {OPCUrl}", "Ошибка подключения" );
							logger.Error( $"Ошибка подключения к серверу: {OPCUrl}" );
						}

						break;
					case 1:
						NlOpcClient.Stop();
						Mode = 0;
						ConnectOPCButtonText = "Подключиться к OPC серверу";
						logger.Info( $"Отключено от OPC сервера: {OPCUrl}" );
						break;
					default:
						break;
				}
			}
			catch ( Exception exc )
			{

			}
		}
		#endregion Constructors

		#region Methods
		public void LineNumberRecieved()
		{
			Console.WriteLine($"Recieved line number = {NlOpcClient.LineNumber}");
			ReceivedLineNumber = NlOpcClient.LineNumber;
		}
		#endregion Methods

		#region Events
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion Events

		#region EventHandlers
		public void OnPropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}
		#endregion EventHandlers

		public void StopMonitoring()
		{
			try
			{
				if (NlOpcClient.server.IsConnected)
					NlOpcClient.Stop();
			}
			catch (Exception exc )
			{
				logger.Error($"Ошибка остановки мониторинга: {exc.Message}");
			}
		}
	}
}
