using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using log4net;
using Siemens.Opc.Da;

namespace NashLabelWorkStation.Util
{
    public class OpcClient : IDisposable
    {
        enum ClientHandles
        {
            Item1 = 0,
            //Item2,
            //ItemBlockRead,
            //ItemBlockWrite
        };

        public Server server;
        private Subscription subscription = null;
        private string Node_name;
        private string PrintServiceIp;
        public static ILog logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
        public string LineNumber;
        public bool IsConnected = false;

        public delegate void MethodContainer();
        public event MethodContainer onRecieveLineNumber;


        public OpcClient( string url, string node, string printServiceIp )
        {
            try
            {
                server = new Server();
                server.Connect( url );
                Node_name = node;
                PrintServiceIp = printServiceIp;
                if ( server.IsConnected )
                {
                    logger.Info( $"Подключено к серверу OPC: {url}" );
                    logger.Info( $"Контролируем переменную: {node}" );
                    logger.Info( "Ip cлужбы печати: {printServiceIp}" );
                    IsConnected = true;
                }
                else
                {
                    logger.Error( $"Ошибка подключения сервера OPC: {url}" );
                }



            }
            catch ( Exception exc )
            {
                //Console.WriteLine($"Error: {exc.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                server.DeleteSubscription( subscription );
                server.Disconnect();
                logger.Info( $"{server.Url.ToString()} has been disposed" );
            }
            catch ( Exception exc )
            {

            }

        }

        public void startMonitoring()
        {
            // Check if we have a subscription
            //  - No  -> Create a new subscription and create monitored items
            //  - Yes -> Delete Subcription
            if ( subscription == null )
            {
                startMonitorItems();
            }
            else
            {
                //stopMonitorItems();
            }
        }

        #region Internal Helper Methods
        void startMonitorItems()
        {
            // Check if we have a subscription. If not - create a new subscription.
            if ( subscription == null )
            {
                try
                {
                    // Create subscription
                    subscription = server.CreateSubscription( "Subscription1", OnDataChange );
                    logger.Info( @"Create subscription 'Subscription1' succesefull" );
                }
                catch ( Exception exception )
                {
                    logger.Error( "Create subscription failed:\n\n" + exception.Message );
                    return;
                }
            }

            // Add item 1
            try
            {
                subscription.AddItem(
                    Node_name,
                    (int)ClientHandles.Item1 );

            }
            catch ( Exception exception )
            {
                logger.Error( $"Error: {exception.Message}" );
            }

        }

        internal void Stop()
        {
            try
            {
                server.DeleteSubscription( subscription );
                server.Disconnect();
                IsConnected = false;
            }
            catch ( Exception exc )
            {

            }
        }

        public void Console_CancelKeyPress()
        {
            //Console.WriteLine( "Exiting" );
            Stop();
            // Termitate what I have to terminate
            Environment.Exit( -1 );
        }
        // Делаем перегрузку метода, чтобы наш слушатель событий не выдавал ошибку
        public void Console_CancelKeyPress( object sender, ConsoleCancelEventArgs e )
        {
            Console_CancelKeyPress();
        }

        /// <summary>
        /// callback to receive datachanges
        /// </summary>
        /// <param name="clientHandle"></param>
        /// <param name="value"></param>
        private void OnDataChange( IList<DataValue> DataValues )
        {
            try
            {
                // We have to call an invoke method 
                //if ( this.InvokeRequired )
                //{
                //    // Asynchronous execution of the valueChanged delegate
                //    this.BeginInvoke( new DataChange( OnDataChange ), DataValues );
                //    return;
                //}

                foreach ( DataValue value in DataValues )
                {
                    // 1 is Item1, 2 is Item2, 3 is ItemBlockRead
                    switch ( value.ClientHandle )
                    {
                        case (int)ClientHandles.Item1:
                            // Print data change information for variable - check first the result code
                            if ( value.Error != 0 )
                            {
                                // The node failed - print the symbolic name of the status code
                                //txtMonitor1.Text = "Error: 0x" + value.Error.ToString( "X" );
                                //txtMonitor1.BackColor = Color.Red;
                            }
                            else
                            {
                                // The node succeeded - print the value as string
                                //Console.WriteLine( $"Value = {value.Value.ToString()}");
                                logger.Info( $"Value = {value.Value.ToString()}" );

                                //var request = (HttpWebRequest)WebRequest.Create( $"http://localhost:5000/api/Print/Ocp?lineId={value.Value.ToString()}"
                                var request = (HttpWebRequest)WebRequest.Create( $"http://{PrintServiceIp}:5000/api/Print/Ocp?lineId={value.Value.ToString()}" );
                                LineNumber = value.Value.ToString();
                                onRecieveLineNumber();

                                //var postData = value.Value.ToString();

                                //var data = Encoding.ASCII.GetBytes( postData );

                                request.Method = "GET";
                                //request.ContentType = "application/x-www-form-urlencoded";
                                //request.ContentLength = data.Length;

                                //using ( var stream = request.GetRequestStream() )
                                //{
                                //    stream.Write( data, 0, data.Length );
                                //}

                                var response = (HttpWebResponse)request.GetResponse();

                                var responseString = new StreamReader( response.GetResponseStream() ).ReadToEnd();
                                //Console.WriteLine( responseString );
                                logger.Info( $"responseString{responseString}" );
                                //Console.WriteLine( "Нажмите любую клавишу для выхода..." );
                            }
                            break;

                        default:
                            // error
                            break;
                    }
                }
            }
            catch ( Exception ex )
            {
                logger.Error( $"Ошибка приема данных: {ex.Message}" );
                //Console.WriteLine( $"Error: {ex.Message}" );
            }
        }

        //void stopMonitorItems()
        //{
        //    if ( m_Subscription != null )
        //    {
        //        try
        //        {
        //            m_Server.DeleteSubscription( m_Subscription );
        //            m_Subscription = null;

        //            btnMonitor.Text = "Monitor";
        //            txtMonitor1.Clear();
        //            txtMonitor1.BackColor = Color.White;
        //            txtMonitor2.Clear();
        //            txtMonitor2.BackColor = Color.White;

        //            // enable changing the itemID
        //            txtItemID1.Enabled = true;
        //            txtItemID2.Enabled = true;
        //        }
        //        catch ( Exception ex )
        //        {
        //            MessageBox.Show( "Stopping data monitoring failed:\n\n" + ex.Message );
        //        }
        //    }
        //}
    }
    #endregion Internal Helper Methods

}
