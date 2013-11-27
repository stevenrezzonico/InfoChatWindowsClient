using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Color = System.Windows.Media.Color;

namespace InfoChatWindowsClient {

    public partial class MainWindow : Window {
        #region Colors

        private readonly Color FF333333 = Color.FromArgb(255, 51, 51, 51);
        private readonly Color FF494949 = Color.FromArgb(255, 73, 73, 73);
        private readonly Color FFD1D1D1 = Color.FromArgb(255, 209, 209, 209);

        #endregion

        public MainWindow() {
            InitializeComponent();

            //Move the window
            TitleBar.MouseMove += TitleBarMouseMove;
            LabelTitle.MouseMove += TitleBarMouseMove;

			//Window property
	        Background = new SolidColorBrush(Color.FromArgb(Properties.Settings.Default.Opacity, 255, 255, 255));

			//Receiver backgroud task
            var backgroundDBTask = Task.Factory.StartNew(ListenMessages, TaskCreationOptions.LongRunning);
            backgroundDBTask.ContinueWith(t => { }, TaskScheduler.FromCurrentSynchronizationContext());

            MessageTextBox.Focus();
        }


        //////////////////////////////////////
        public void AddSendedMessage(string text) {
            if (text == "") return;

            var textBox = new TextBox() {
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                Text = text,
                Background = new SolidColorBrush(Colors.Transparent),
                FontSize = 16,
            };

            var border = new Border {
                CornerRadius = new CornerRadius(25),
                Padding = new Thickness(15, 10, 15, 10),
                Child = textBox,
                Margin = new Thickness(100, 10, 10, 10),
                Background = new SolidColorBrush(Color.FromArgb(255, 90, 150, 250)),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            SpContainer.Children.Add(border);
            MainScrollViewer.ScrollToBottom();
            MessageTextBox.Text = "";
        }

        public void AddReceivedMessage(string text) {
            if (text.Substring(12) == "") return;

            var textBox = new TextBox() {
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                Text = text.Substring(0, 12).Trim(' ') + ':' + Environment.NewLine + text.Substring(12),
                Background = new SolidColorBrush(Colors.Transparent),
                FontSize = 16,
            };

            var border = new Border {
                CornerRadius = new CornerRadius(25),
                Padding = new Thickness(15, 10, 15, 10),
                Child = textBox,
                Margin = new Thickness(10, 10, 100, 10),
                Background = new SolidColorBrush(Color.FromArgb(255, 60, 230, 60)),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            SpContainer.Children.Add(border);
            MainScrollViewer.ScrollToBottom();
        }

        public void SendMessage(string message) {
			if (message == "") return;

            //Protocol Type is UDP 
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var iep = new IPEndPoint(IPAddress.Broadcast, Properties.Settings.Default.Port);

            //Message to send: mac addr(12Byte), username(12Byte), message
			var data = Encoding.Unicode.GetBytes(GetMacAddress() + Properties.Settings.Default.Username + message);

            //To all sockets (tcp, udp, ...)
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            sock.SendTo(data, iep);

            sock.Close();

			//Show message to user
            AddSendedMessage(message);
        }

        //////////////////////////////////////

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e) {
            SendMessage(MessageTextBox.Text);
        }

        private void MessageTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) { //(e.Key == Key.LeftShift || e.Key == Key.RightShift)) {
                MessageTextBox.AppendText(Environment.NewLine);
                MessageTextBox.Select(MessageTextBox.Text.Length, 0);
                return;
            }
            if (e.Key == Key.Enter)
                SendMessage(MessageTextBox.Text);
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e){
            var settingsWindow = new SettingsWindow {Owner = this};
			
	        settingsWindow.ShowDialog();
			if (settingsWindow.DialogResult.HasValue && settingsWindow.DialogResult.Value)
				Background = new SolidColorBrush(Color.FromArgb((byte)settingsWindow.OpacitySlider.Value, 255, 255, 255));
        }
        //////////////////////////////////////

        static string GetMacAddress(){
            return (from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();
        }

        public void ListenMessages() {

            while (true) {
                var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var iep = new IPEndPoint(IPAddress.Any, Properties.Settings.Default.Port);
                sock.Bind(iep);
                var ep = (EndPoint)iep;

                var data = new byte[8192];
                int recv = sock.ReceiveFrom(data, ref ep);

                var stringData = Encoding.Unicode.GetString(data, 0, recv);

				//Show received message only if the mac addr is different from mine
                //if (stringData.Substring(0, 12) != GetMacAddress())
                    Dispatcher.BeginInvoke(new Action(() => AddReceivedMessage(stringData.Substring(12))));
                sock.Close();
            }
        }
        ///////////////////////////////////////

        #region Window events methods

        //Move window through the tilebar
        private void TitleBarMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }

        //Minimize window button
        private void RectangleMinimizeMouseEnter(object sender, MouseEventArgs e) {
            RectangleMinimize.Fill = new SolidColorBrush(FFD1D1D1);
        }
        private void RectangleMinimizeMouseLeave(object sender, MouseEventArgs e) {
            RectangleMinimize.Fill = new SolidColorBrush(FF333333);
        }
        private void RectangleMinimizeMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        //Close window button
        private void RectangleCloseMouseEnter(object sender, MouseEventArgs e) {
            RectangleClose.Fill = new SolidColorBrush(FFD1D1D1);
        }
        private void RectangleCloseMouseLeave(object sender, MouseEventArgs e) {
            RectangleClose.Fill = new SolidColorBrush(FF333333);
        }
        private void RectangleCloseMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Close();
        }

        #endregion
    }
}
