using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// PrivateChatWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PrivateChatWindow : Window
    {
        bool initialized = false;

        int fontSize = 12;
        bool isBold = false;
        bool isItalic = false;
        bool isUnderLine = false;
        string fontFamily = "Microsoft YaHei UI";
        string fontColor = "#FF000000";

        ClientConnector connector = ((App)Application.Current).connector;
        public User user = ((App)Application.Current).user;
        public User TargetUser;
        Properties.Settings settings = Properties.Settings.Default;
        SolidColorBrush labelCheckedBrush = new SolidColorBrush(Colors.LightGray);

        public PrivateChatWindow(User targetUser)
        {
            InitializeComponent();
            TargetUser = targetUser;
            Title = TargetUser.ToString();
            connector.PrivateMessageEvent += Connector_PrivateMessageEvent;
            connector.UserJoinEvent += Connector_UserJoinEvent;
            connector.UserQuitEvent += Connector_UserQuitEvent;
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

            MessageDictionary message = new MessageDictionary();
            message.Add(MesKeyStr.TargetUserID, TargetUser.UserID);
            message.Add(MesKeyStr.MessageType, MessageType.Text.ToString());
            message.Add(MesKeyStr.UserID, user.UserID);
            message.Add(MesKeyStr.Content, contentTB.Text);
            message.Add(MesKeyStr.FontFamily, (string)fontFamilyCB.SelectedItem);
            message.Add(MesKeyStr.FontSize, ((ComboBoxItem)fontSizeCB.SelectedItem).Content.ToString());
            message.Add(MesKeyStr.FontStyle, style.ToString());
            message.Add(MesKeyStr.FontColor, fontColor);
            connector.SendPrivateMessage(message);
            contentTB.Text = "";
        }

        private void sendB_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void contentTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
                SendMessage();
        }

        #region
        private void AddChildToMesListSP(UIElement element)
        {
            if (messageListSP.Children.Count == 500)
                messageListSP.Children.RemoveAt(0);
            messageListSP.Children.Add(element);
            messageListSV.ScrollToEnd();
        }

        private void Connector_UserQuitEvent(object sender, User e)
        {
            if (e.UserID == TargetUser.UserID)
            {
                sendB.IsEnabled = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Label label = new Label();
                    label.Content = "对方已下线";
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    AddChildToMesListSP(label);
                });
            }
        }

        private void Connector_UserJoinEvent(object sender, User e)
        {
            if (e.UserID == TargetUser.UserID)
            {
                sendB.IsEnabled = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Label label = new Label();
                    label.Content = "对方已上线";
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    AddChildToMesListSP(label);
                });
            }
        }

        private void Connector_PrivateMessageEvent(object sender, MessageDictionary e)
        {
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageUC messageUC = new MessageUC(e);
                AddChildToMesListSP(messageUC);
            });
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

    }
}
