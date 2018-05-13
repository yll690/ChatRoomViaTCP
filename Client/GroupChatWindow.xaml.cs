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
using System.Drawing.Text;
using System.Collections.ObjectModel;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GroupChatWindow : Window
    {
        static int numOfWindow = 0;
        bool manualClose = true;
        bool initialized = false;

        int fontSize = 12;
        bool isBold = false;
        bool isItalic = false;
        bool isUnderLine = false;
        string fontFamily = "Microsoft YaHei UI";
        string fontColor = "#FF000000";

        ClientConnector connector = ((App)Application.Current).connector;
        public User user = ((App)Application.Current).user;
        ObservableCollection<User> userList = new ObservableCollection<User>();
        Properties.Settings settings = Properties.Settings.Default;
        SolidColorBrush labelCheckedBrush = new SolidColorBrush(Colors.LightGray);

        public GroupChatWindow()
        {
            InitializeComponent();
            connector.GroupMessageEvent += Connector_GroupMessageEvent;
            connector.UserJoinEvent += Connector_UserJoinEvent;
            connector.UserQuitEvent += Connector_UserQuitEvent;
            connector.ServerDisconnectEvent += Connector_ServerDisconnectEvent;
            userListLV.ItemsSource = userList;
            userList.Add(user);
            InstalledFontCollection installedFonts = new InstalledFontCollection();
            FontFamily defaultFont = FontFamily;
            for (int i = 0; i < installedFonts.Families.Length; i++)
            {
                fontFamilyCB.Items.Add(installedFonts.Families[i].Name);
                if (defaultFont.ToString().Equals(installedFonts.Families[i].Name))
                    fontFamilyCB.SelectedIndex = i;
            }
            initialized = true;
            LoadStyle();
            UpdateTitle();
        }

        private void LoadStyle()
        {
            FontSizeSwitch(settings.fontSize);
            for (int i = 0; i < fontSizeCB.Items.Count; i++)
                if (settings.fontSize == int.Parse((string)((ComboBoxItem)fontSizeCB.Items[i]).Content))
                    fontSizeCB.SelectedIndex = i;
            FontFamilySwitch(settings.fontFamily);
            for (int i = 0; i < fontFamilyCB.Items.Count; i++)
                if (settings.fontFamily.Equals(fontFamilyCB.Items[i]))
                    fontFamilyCB.SelectedIndex = i;
            BoldSwitch(settings.isBold);
            ItalicSwitch(settings.isItalic);
            UnderLineSwitch(settings.isUnderLIne);
            ColorSwitch((Color)ColorConverter.ConvertFromString(settings.fontColor));
        }

        private void UpdateTitle()
        {
            Title = "群聊室 （目前在线 " + userList.Count + " 人），目前登录：" + user.ToString();
        }

        private void SaveSettings()
        {
            settings.fontSize = fontSize;
            settings.fontFamily = fontFamily;
            settings.fontColor = fontColor;
            settings.isBold = isBold;
            settings.isItalic = isItalic;
            settings.isUnderLIne = isUnderLine;
            settings.Save();
        }
        
        private void SendMessage()
        {
            int style = 0;
            if (isBold) style += 1;
            if (isItalic) style += 10;
            if (isUnderLine) style += 100;

            //ChatMessageSend message = new ChatMessageSend()
            //{
            //    MessageType = MessageType.Text,
            //    UserID = user.UserID,
            //    Content = contentTB.Text,
            //    FontFamily = (string)fontFamilyCB.SelectedItem,
            //    FontSize = int.Parse(((ComboBoxItem)fontSizeCB.SelectedItem).Content.ToString()),
            //    FontStyle = style,
            //    FontColor = fontColor,
            //};

            MessageD message = new MessageD();
            message.Add("MessageType", "Text");
            message.Add("UserID", user.UserID);
            message.Add("Content", contentTB.Text);
            message.Add("FontFamily", (string)fontFamilyCB.SelectedItem);
            message.Add("FontSize", ((ComboBoxItem)fontSizeCB.SelectedItem).Content.ToString());
            message.Add("FontStyle", style.ToString());
            message.Add("FontColor", fontColor);

            connector.SendMessage(message);
            contentTB.Text = "";
        }

        public static GroupChatWindow GetNewWindow()
        {
            if (numOfWindow == 0)
            {
                numOfWindow++;
                return new GroupChatWindow();
            }
            else
                return null;
        }

        //关于其他窗口事件的事件处理方法
        #region
        private void sendB_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show(this,"想要退出还是注销？",MessageBoxButton.YesNoCancel)
            numOfWindow--;
            SaveSettings();
            if (manualClose)
            {
                connector.Close();
            }
        }

        private void contentTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
                SendMessage();
        }

        private void logoutB_Click(object sender, RoutedEventArgs e)
        {
            connector.Logout();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            manualClose = false;
            Close();
        }

        private void userListLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        //关于字体样式的方法
        #region
        private void fontFamilyCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontFamilySwitch((string)fontFamilyCB.SelectedItem);
        }

        private void FontFamilySwitch(string font)
        {
            FontFamilyConverter fontFamilyConverter = new FontFamilyConverter();
            try
            {
                contentTB.FontFamily = (FontFamily)fontFamilyConverter.ConvertFromString(font);
                fontFamily = font;
            }
            catch (Exception ex)
            {
                MessageBox.Show("字体" + fontFamily + "不存在！\n" + ex.Message + ex.StackTrace);
            }
        }

        private void fontSizeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized)
                FontSizeSwitch(int.Parse(((ComboBoxItem)fontSizeCB.SelectedItem).Content.ToString()));
        }

        private void FontSizeSwitch(int size)
        {
            contentTB.FontSize = size;
            fontSize = size;
        }

        private void boldL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BoldSwitch(!isBold);
        }

        private void BoldSwitch(bool bold)
        {
            if (bold)
            {
                isBold = true;
                boldL.FontWeight = FontWeights.Bold;
                boldL.Background = labelCheckedBrush;
                contentTB.FontWeight = FontWeights.Bold;
            }
            else
            {
                isBold = false;
                boldL.FontWeight = FontWeights.Normal;
                boldL.Background = null;
                contentTB.FontWeight = FontWeights.Normal;
            }

        }

        private void italicL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ItalicSwitch(!isItalic);
        }

        private void ItalicSwitch(bool italic)
        {
            if (italic)
            {
                isItalic = true;
                italicL.FontStyle = FontStyles.Italic;
                italicL.Background = labelCheckedBrush;
                contentTB.FontStyle = FontStyles.Italic;
            }
            else
            {
                isItalic = false;
                italicL.FontStyle = FontStyles.Normal;
                italicL.Background = null;
                contentTB.FontStyle = FontStyles.Normal;
            }
        }

        private void underLineL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UnderLineSwitch(!isUnderLine);
        }

        private void UnderLineSwitch(bool underLine)
        {
            if (underLine)
            {
                isUnderLine = true;
                underLineL.Background = labelCheckedBrush;
                contentTB.TextDecorations = TextDecorations.Underline;

            }
            else
            {
                isUnderLine = false;
                underLineL.Background = null;
                contentTB.TextDecorations = null;
            }
        }

        private void fontColorL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Color color;
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                fontColor = color.ToString();
                ColorSwitch(color);
            }
        }

        private void ColorSwitch(Color color)
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush(color);
            contentTB.Foreground = solidColorBrush;
            fontColorL.Foreground = solidColorBrush;
        }
        #endregion

        //关于connector的事件处理方法
        #region
        private void Connector_ServerDisconnectEvent(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(this, "服务器关闭或失去连接，请重新登录。");
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                manualClose = false;
                Close();
            });
        }

        private void Connector_GroupMessageEvent(object sender, ChatMessage e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (messageListSP.Children.Count == 500)
                    messageListSP.Children.RemoveAt(0);
                MessageUC messageUC = new MessageUC(e);
                messageListSP.Children.Add(messageUC);
                messageListSV.ScrollToEnd();
            });
        }

        private void Connector_UserJoinEvent(object sender, User e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                userList.Add(e);
                UpdateTitle();
                //userListLV.Items.Add(e.ToString());
            });
        }

        private void Connector_UserQuitEvent(object sender, User e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                userList.Remove(e);
                UpdateTitle();
                //userListLV.Items.Remove(e.ToString());
            });
        }
        #endregion
    }
}
