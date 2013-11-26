using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace InfoChatWindowsClient {
    /// <summary>
    /// Logica di interazione per SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {

        #region Colors

        private readonly Color FF333333 = Color.FromArgb(255, 51, 51, 51);
        private readonly Color FF494949 = Color.FromArgb(255, 73, 73, 73);
        private readonly Color FFD1D1D1 = Color.FromArgb(255, 209, 209, 209);

        #endregion

        public SettingsWindow() {
            InitializeComponent();

            //Move the window
            TitleBar.MouseMove += TitleBarMouseMove;
            LabelTitle.MouseMove += TitleBarMouseMove;

			//Window property
			Background = new SolidColorBrush(Color.FromArgb(Properties.Settings.Default.Opacity, 255, 255, 255));

            PortTextBox.Text = Properties.Settings.Default.Port.ToString();
			UsernameTextBox.Text = Properties.Settings.Default.Username;
	        OpacitySlider.Value = Properties.Settings.Default.Opacity;
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

        private void PortTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !Char.IsNumber(Convert.ToChar(e.Text));
            base.OnPreviewTextInput(e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e){
			DialogResult = false;
            Close();
        }

		private void ApplyButton_Click(object sender, RoutedEventArgs e) {
			Properties.Settings.Default.Opacity = (byte)OpacitySlider.Value;
            Properties.Settings.Default.Port = int.Parse(PortTextBox.Text);
			Properties.Settings.Default.Username = UsernameTextBox.Text;
            Properties.Settings.Default.Save();

			DialogResult = true;

            Close();
        }

		private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e) {
			for (var i = UsernameTextBox.Text.Length; i <= 12; i++) {
				UsernameTextBox.Text += ' ';
			}
		}
    }
}
