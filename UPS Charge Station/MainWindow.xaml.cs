using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using NationalInstruments.DAQmx;

namespace UPS_Charge_Station
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Inicializar Salidas de DAQ en apagadas
            outputs[0] = true;
            outputs[1] = true;
            outputs[2] = true;
            outputs[3] = true;
            outputs[4] = true;
            outputs[5] = true;
            outputs[6] = true;
            outputs[7] = true;
            DAQ(outputs);

            //Inicializar timers con el tiempo restante de carga
            SetupTimers();

            //Create a stackpanel 
            img1.Source = new BitmapImage(new Uri(@"c:\Charging.png"));
            img2.Source = new BitmapImage(new Uri(@"c:\Charging.png"));
            img3.Source = new BitmapImage(new Uri(@"c:\Charging.png"));
            img4.Source = new BitmapImage(new Uri(@"c:\Charging.png"));
            stackPnl.Orientation = Orientation.Horizontal;
            stackPnl.Margin = new Thickness(10);
            stackPnl.Children.Add(img1);
            stackPnl.Children.Add(_textBlock1);

            stackPn2.Orientation = Orientation.Horizontal;
            stackPn2.Margin = new Thickness(10);
            stackPn2.Children.Add(img2);
            stackPn2.Children.Add(_textBlock2);

            stackPn3.Orientation = Orientation.Horizontal;
            stackPn3.Margin = new Thickness(10);
            stackPn3.Children.Add(img3);
            stackPn3.Children.Add(_textBlock3);

            stackPn4.Orientation = Orientation.Horizontal;
            stackPn4.Margin = new Thickness(10);
            stackPn4.Children.Add(img4);
            stackPn4.Children.Add(_textBlock4);
        }

        #region National Instruments

        NationalInstruments.DAQmx.Task TaskReadIn;
        NationalInstruments.DAQmx.Task TaskWriteOut;
        DIChannel myDIChannel;
        DOChannel _myDOChannel;
        DigitalSingleChannelReader _reader;
        DigitalSingleChannelWriter _writer;
        bool[] outputs = new bool[8];


        #endregion

        #region Variables Globales

        DispatcherTimer _timerSlot1 = new DispatcherTimer();
        DispatcherTimer _timerSlot2 = new DispatcherTimer();
        DispatcherTimer _timerSlot3 = new DispatcherTimer();
        DispatcherTimer _timerSlot4 = new DispatcherTimer();
        float _time1;
        float _time2;
        float _time3;
        float _time4;
        float _SpectTime;

        MessageBoxResult _msgResult;

        SolidColorBrush _Completed = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        SolidColorBrush _Charging = new SolidColorBrush(Color.FromRgb(255, 193, 7));
        SolidColorBrush _Error = new SolidColorBrush(Color.FromRgb(244, 67, 54));

        TextBlock _textBlock1 = new TextBlock()
        {
            Text = "CARGANDO...",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        TextBlock _textBlock2 = new TextBlock()
        {
            Text = "CARGANDO...",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        TextBlock _textBlock3 = new TextBlock()
        {
            Text = "CARGANDO...",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        TextBlock _textBlock4 = new TextBlock()
        {
            Text = "CARGANDO...",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Image img1 = new Image();
        Image img2 = new Image();
        Image img3 = new Image();
        Image img4 = new Image();
        StackPanel stackPnl = new StackPanel();
        StackPanel stackPn2 = new StackPanel();
        StackPanel stackPn3 = new StackPanel();
        StackPanel stackPn4 = new StackPanel();
      
        

        #endregion

        #region Funciones para eventos de ventana de aplicacion

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            myCanvas.Width = e.NewSize.Width;
            myCanvas.Height = e.NewSize.Height;

            double xChange = 1, yChange = 1;

            if (e.PreviousSize.Width != 0)
                xChange = (e.NewSize.Width / e.PreviousSize.Width);

            if (e.PreviousSize.Height != 0)
                yChange = (e.NewSize.Height / e.PreviousSize.Height);

            foreach (FrameworkElement fe in myCanvas.Children)
            {
                /*because I didn't want to resize the grid I'm having inside the canvas in this particular instance. (doing that from xaml) */
                if (fe is Grid == false)
                {
                    fe.Height = fe.ActualHeight * yChange;
                    fe.Width = fe.ActualWidth * xChange;

                    Canvas.SetTop(fe, Canvas.GetTop(fe) * yChange);
                    Canvas.SetLeft(fe, Canvas.GetLeft(fe) * xChange);
                }
            }
        }

        #endregion



        //Funciones de las 4 Bahias de carga en UPS Charge Station
        #region Bahia 1

        private void BtnOnOffSlot1_Click(object sender, RoutedEventArgs e)
        {
            switch (outputs[0])
            {
                case true:
                    outputs[0] = false;
                          
                    btnOnOffSlot1.Content = stackPnl;
                    btnOnOffSlot1.Background = _Charging;

                    DAQ(outputs);

                    _timerSlot1.Interval = new TimeSpan(0, 0, 1);
                    _timerSlot1.Tick += _timerSlot1_Tick;
                    _timerSlot1.Start();
                    lblTitle.Focus();
                    break;

                case false:
                   _msgResult = MessageBox.Show("Estas seguro que quieres reniciar el ciclo de carga?",
                        "Reniciar Ciclo de carga", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                    if(_msgResult == MessageBoxResult.Yes) _time1 = _SpectTime;
                    lblTitle.Focus();
                    break;
            }       
        }

        private void _timerSlot1_Tick(object sender, EventArgs e)
        {
            TimeSpan _tim = TimeSpan.FromSeconds(_time1);
            lblTimer1.Content = String.Format(@"{0:hh\:mm\:ss}", _tim);

            Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time1", _time1, RegistryValueKind.String);
            _time1--;


            if (_time1 == 0)
            {
                outputs[0] = true;

                DAQ(outputs);

                _timerSlot1.Stop();
                _timerSlot1.Tick -= _timerSlot1_Tick;
                lblTimer1.Content = "00:00:00";
                btnOnOffSlot1.Background = _Completed;
                btnOnOffSlot1.Content = "CARGA FINALIZADA!";
                _time1 = 28800;               
            }         
        }

        #endregion

        #region Bahia 2

        private void BtnOnOffSlot2_Click(object sender, RoutedEventArgs e)
        {
            switch (outputs[1])
            {
                case true:
                    outputs[1] = false;

                    btnOnOffSlot2.Content = stackPn2;
                    btnOnOffSlot2.Background = _Charging;
                    
                    DAQ(outputs);

                    _timerSlot2.Interval = new TimeSpan(0, 0, 1);
                    _timerSlot2.Tick += _timerSlot2_Tick;
                    _timerSlot2.Start();
                    lblTitle.Focus();
                    break;

                case false:
                    _msgResult = MessageBox.Show("Estas seguro que quieres reniciar el ciclo de carga?",
                         "Reniciar Ciclo de carga", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                    if (_msgResult == MessageBoxResult.Yes) _time2 = _SpectTime;
                    lblTitle.Focus();
                    break;
            }
        }

        private void _timerSlot2_Tick(object sender, EventArgs e)
        {
            TimeSpan _tim = TimeSpan.FromSeconds(_time2);
            lblTimer2.Content = String.Format(@"{0:hh\:mm\:ss}", _tim);

            Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time2", _time2, RegistryValueKind.String);
            _time2--;


            if (_time2 == 0)
            {
                outputs[1] = true;

                DAQ(outputs);

                _timerSlot2.Stop();
                _timerSlot2.Tick -= _timerSlot2_Tick;
                lblTimer2.Content = "00:00:00";
                btnOnOffSlot2.Background = _Completed;
                btnOnOffSlot2.Content = "CARGA FINALIZADA!";
                _time2 = 28800;
            }
        }


        #endregion

        #region Bahia 3

        private void BtnOnOffSlot3_Click(object sender, RoutedEventArgs e)
        {
            switch (outputs[2])
            {
                case true:
                    outputs[2] = false;

                    btnOnOffSlot3.Content = stackPn3;
                    btnOnOffSlot3.Background = _Charging;

                    DAQ(outputs);

                    _timerSlot3.Interval = new TimeSpan(0, 0, 1);
                    _timerSlot3.Tick += _timerSlot3_Tick;
                    _timerSlot3.Start();
                    lblTitle.Focus();
                    break;

                case false:
                    _msgResult = MessageBox.Show("Estas seguro que quieres reniciar el ciclo de carga?",
                         "Reniciar Ciclo de carga", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                    if (_msgResult == MessageBoxResult.Yes) _time3 = _SpectTime;
                    lblTitle.Focus();
                    break;
            }
        }

        private void _timerSlot3_Tick(object sender, EventArgs e)
        {
            TimeSpan _tim = TimeSpan.FromSeconds(_time3);
            lblTimer3.Content = String.Format(@"{0:hh\:mm\:ss}", _tim);

            Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time3", _time3, RegistryValueKind.String);
            _time3--;


            if (_time3 == 0)
            {
                outputs[2] = true;

                DAQ(outputs);

                _timerSlot3.Stop();
                _timerSlot3.Tick -= _timerSlot3_Tick;
                lblTimer3.Content = "00:00:00";
                btnOnOffSlot3.Background = _Completed;
                btnOnOffSlot3.Content = "CARGA FINALIZADA!";
                _time3 = 28800;
            }
        }



        #endregion

        #region Bahia 4

        private void BtnOnOffSlot4_Click(object sender, RoutedEventArgs e)
        {
            switch (outputs[3])
            {
                case true:
                    outputs[3] = false;

                    btnOnOffSlot4.Content = stackPn4;
                    btnOnOffSlot4.Background = _Charging;

                    DAQ(outputs);

                    _timerSlot4.Interval = new TimeSpan(0, 0, 1);
                    _timerSlot4.Tick += _timerSlot4_Tick;
                    _timerSlot4.Start();
                    lblTitle.Focus();
                    break;

                case false:
                    _msgResult = MessageBox.Show("Estas seguro que quieres reniciar el ciclo de carga?",
                         "Reniciar Ciclo de carga", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                    if (_msgResult == MessageBoxResult.Yes) _time4 = _SpectTime;
                    lblTitle.Focus();
                    break;
            }
        }

        private void _timerSlot4_Tick(object sender, EventArgs e)
        {
            TimeSpan _tim = TimeSpan.FromSeconds(_time4);
            lblTimer4.Content = String.Format(@"{0:hh\:mm\:ss}", _tim);

            Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time4", _time4, RegistryValueKind.String);
            _time4--;


            if (_time4 == 0)
            {
                outputs[3] = true;

                DAQ(outputs);

                _timerSlot4.Stop();
                _timerSlot4.Tick -= _timerSlot4_Tick;
                lblTimer4.Content = "00:00:00";
                btnOnOffSlot4.Background = _Completed;
                btnOnOffSlot4.Content = "CARGA FINALIZADA!";
                _time4 = 28800;
            }
        }

        #endregion





        //Funcion para cambiar estados en salidas de DAQ
        private void DAQ(bool[] outputs)
        {
            try
            {
                TaskReadIn = new NationalInstruments.DAQmx.Task();
                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                myDIChannel = TaskReadIn.DIChannels.CreateChannel("DIO" + @"/" + "port" + 1, "read0", ChannelLineGrouping.OneChannelForAllLines);
                _reader = new DigitalSingleChannelReader(TaskReadIn.Stream);

                _myDOChannel = TaskWriteOut.DOChannels.CreateChannel("DIO" + @"/" + "port" + 2, "write0", ChannelLineGrouping.OneChannelForAllLines);
                _writer = new DigitalSingleChannelWriter(TaskWriteOut.Stream);

                NationalInstruments.DAQmx.Task _taskNI = new NationalInstruments.DAQmx.Task();
                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                //bool[] outputs = new bool[8];
                //outputs[0] = false;
                //outputs[1] = true;
                //outputs[2] = true;
                //outputs[3] = true;
                //outputs[4] = true;
                //outputs[5] = true;
                //outputs[6] = true;
                //outputs[7] = true;

                _writer.WriteSingleSampleMultiLine(true, outputs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }      
        }

        //Funcion para recordar y hacer el setup de los timers cuando se cierre y abra la aplicacion.
        void SetupTimers()
        {
            try
            {



                object _SpectTimeObj = Registry.GetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "SpectTime", null);
                object _TimerObj1 = Registry.GetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time1", null);
                object _TimerObj2 = Registry.GetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time2", null);
                object _TimerObj3 = Registry.GetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time3", null);
                object _TimerObj4 = Registry.GetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time4", null);
                _SpectTime = Convert.ToSingle(_SpectTimeObj.ToString());
                _time1 = Convert.ToSingle(_TimerObj1.ToString());
                _time2 = Convert.ToSingle(_TimerObj2.ToString());
                _time3 = Convert.ToSingle(_TimerObj3.ToString());
                _time4 = Convert.ToSingle(_TimerObj4.ToString());

                TimeSpan _tim1 = TimeSpan.FromSeconds(_time1);
                TimeSpan _tim2 = TimeSpan.FromSeconds(_time2);
                TimeSpan _tim3 = TimeSpan.FromSeconds(_time3);
                TimeSpan _tim4 = TimeSpan.FromSeconds(_time4);
                lblTimer1.Content = String.Format(@"{0:hh\:mm\:ss}", _tim1);
                lblTimer2.Content = String.Format(@"{0:hh\:mm\:ss}", _tim2);
                lblTimer3.Content = String.Format(@"{0:hh\:mm\:ss}", _tim3);
                lblTimer4.Content = String.Format(@"{0:hh\:mm\:ss}", _tim4);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        //Apagar senales de DAQ cuando se cierrre la aplicacion.
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            outputs[0] = true;
            outputs[1] = true;
            outputs[2] = true;
            outputs[3] = true;
            outputs[4] = true;
            outputs[5] = true;
            outputs[6] = true;
            outputs[7] = true;
            DAQ(outputs);
        }

        //Boton para ejectuar el evento close.
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        void Renicar8horas()
        {
            try
            {
                outputs[0] = true;
                outputs[1] = true;
                outputs[2] = true;
                outputs[3] = true;
                outputs[4] = true;
                outputs[5] = true;
                outputs[6] = true;
                outputs[7] = true;
                DAQ(outputs);

                _timerSlot1.Stop();
                _timerSlot2.Stop();
                _timerSlot3.Stop();
                _timerSlot4.Stop();

                object _SpectTime = Registry.GetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "SpectTime", null);
                Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time1", _time1, RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time2", _time2, RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time3", _time3, RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\UPS Charge Station", "Time4", _time4, RegistryValueKind.String);
                _time1 = Convert.ToSingle(_SpectTime.ToString());
                _time2 = Convert.ToSingle(_SpectTime.ToString());
                _time3 = Convert.ToSingle(_SpectTime.ToString());
                _time4 = Convert.ToSingle(_SpectTime.ToString());

                TimeSpan _tim1 = TimeSpan.FromSeconds(_time1);
                TimeSpan _tim2 = TimeSpan.FromSeconds(_time2);
                TimeSpan _tim3 = TimeSpan.FromSeconds(_time3);
                TimeSpan _tim4 = TimeSpan.FromSeconds(_time4);
                lblTimer1.Content = String.Format(@"{0:hh\:mm\:ss}", _tim1);
                lblTimer2.Content = String.Format(@"{0:hh\:mm\:ss}", _tim2);
                lblTimer3.Content = String.Format(@"{0:hh\:mm\:ss}", _tim3);
                lblTimer4.Content = String.Format(@"{0:hh\:mm\:ss}", _tim4);


                btnOnOffSlot1.Background = _Completed;
                btnOnOffSlot1.Content = "INICIAR CICLO DE CARGA";
                btnOnOffSlot2.Background = _Completed;
                btnOnOffSlot2.Content = "INICIAR CICLO DE CARGA";
                btnOnOffSlot3.Background = _Completed;
                btnOnOffSlot3.Content = "INICIAR CICLO DE CARGA";
                btnOnOffSlot4.Background = _Completed;
                btnOnOffSlot4.Content = "INICIAR CICLO DE CARGA";
                lblTitle.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Renicar8horas();
        }
    }
}
