using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing.Text;
using System.Collections.ObjectModel;
using Common;
using System.IO;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatMode ChatModeP { get => chatMode; private set => chatMode = value; }
        public User TargetUser { get => targetUser; private set => targetUser = value; }
        public User CurrentUser { get => currentUser; private set => currentUser = value; }
        public event EventHandler<User> PrivateChatEvent;
        public event EventHandler ManualCloseEvent;

        private int fontSize = 12;
        private bool isBold = false;
        private bool isItalic = false;
        private bool isUnderLine = false;
        private string fontFamily = "Microsoft YaHei UI";
        private string fontColor = "#FF000000";
        
        private int maxMesListLen = StaticStuff.MaxMesListLen;
        private bool manualClosed = true;
        private bool initialized = false;
        private ChatMode chatMode;
        private User targetUser;
        private User currentUser = ((App)Application.Current).CurrentUser;
        private ClientConnector connector = ((App)Application.Current).connector;
        private ObservableCollection<User> userList = new ObservableCollection<User>();
        private Properties.Settings settings = Properties.Settings.Default;
        private SolidColorBrush labelCheckedBrush = new SolidColorBrush(Colors.LightGray);
        
        public ChatWindow()
        {
            Initialize(ChatMode.Group);
        }

        public ChatWindow(User target)
        {
            TargetUser = target;
            Initialize(ChatMode.Private);
        }
        
        private void Initialize(ChatMode mode)
        {
            InitializeComponent();

            Closing += ChatWindow_Closing;
            ChatModeP = mode;
            switch (mode)
            {
                case ChatMode.Group:
                    {
                        userListLV.ItemsSource = userList;
                        userList.Add(currentUser);
                        UpdateTitle();
                        break;
                    }
                case ChatMode.Private:
                    {
                        leftGrid.Visibility = Visibility.Collapsed;
                        centerGS.Visibility = Visibility.Collapsed;
                        Title = TargetUser.ToString();
                        break;
                    }
            }

            InstalledFontCollection installedFonts = new InstalledFontCollection();
            FontFamily defaultFont = FontFamily;
            for (int i = 0; i < installedFonts.Families.Length; i++)
            {
                fontFamilyCB.Items.Add(installedFonts.Families[i].Name);
                if (defaultFont.ToString().Equals(installedFonts.Families[i].Name))
                    fontFamilyCB.SelectedIndex = i;
            }
            LoadStyle();
            initialized = true;
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
            Title = "群聊室 （目前在线 " + userList.Count + " 人），目前登录：" + currentUser.ToString();
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

        private void SendMessage(MessageDictionary message)
        {
            message.Add(MesKeyStr.UserID, currentUser.UserID);
            if (chatMode == ChatMode.Group)
                connector.SendGroupMessage(message);
            else
            {
                message.Add(MesKeyStr.TargetUserID, TargetUser.UserID);
                connector.SendPrivateMessage(message);
            }
        }

        private void SendPictureMessage(string base64String, string extension)
        {
            MessageDictionary message = new MessageDictionary();
            message.Add(MesKeyStr.MessageType, MessageType.Picture.ToString());
            message.Add(MesKeyStr.Extension, extension);
            message.Add(MesKeyStr.Base64String, base64String);
            SendMessage(message);
        }

        private void SendTextMessage()
        {
            MessageDictionary message = new MessageDictionary();
            if (contentTB.Text.Length == 0)
                return;
            int style = 0;
            if (isBold) style += 1;
            if (isItalic) style += 10;
            if (isUnderLine) style += 100;
            message.Add(MesKeyStr.MessageType, MessageType.Text.ToString());
            message.Add(MesKeyStr.Content, StaticStuff.SepToRep(contentTB.Text));
            message.Add(MesKeyStr.FontFamily, (string)fontFamilyCB.SelectedItem);
            message.Add(MesKeyStr.FontSize, ((ComboBoxItem)fontSizeCB.SelectedItem).Content.ToString());
            message.Add(MesKeyStr.FontStyle, style.ToString());
            message.Add(MesKeyStr.FontColor, fontColor);
            contentTB.Text = "";
            SendMessage(message);
        }

        //关于窗口事件的事件处理方法
        #region
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "图片文件(*.jpg,*.png,*.bmp)|*.jpg;*.png;*.bmp";
            if(openFileDialog.ShowDialog()== System.Windows.Forms.DialogResult.OK)
            {
                FileInfo file = new FileInfo(openFileDialog.FileName);
                if(file.Length>StaticStuff.BufferLength-1024*1024)
                {
                    MessageBox.Show("所选图片太大，不能超过4MB");
                    return;
                }
                byte[] buffer = new byte[file.Length];
                file.OpenRead().Read(buffer, 0, (int)file.Length);
                SendPictureMessage(Convert.ToBase64String(buffer), file.Extension);
            }
        }

        private void sendB_Click(object sender, RoutedEventArgs e)
        {
            SendTextMessage();
        }
        
        private void contentTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
                SendTextMessage();
        }

        private void ChatWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            if (manualClosed)
                ManualCloseEvent?.Invoke(this, new EventArgs());
        }

        private void logoutB_Click(object sender, RoutedEventArgs e)
        {
            connector.Logout();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void userListLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach(object user in userListLV.SelectedItems)
                PrivateChatEvent?.Invoke(this, (User)user);
        }

        private void PrivateChatMI_Click(object sender, RoutedEventArgs e)
        {
            foreach (object user in userListLV.SelectedItems)
                PrivateChatEvent?.Invoke(this, (User)user);
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
                ColorSwitch(color);
            }
        }

        private void ColorSwitch(Color color)
        {
            fontColor = color.ToString();
            SolidColorBrush solidColorBrush = new SolidColorBrush(color);
            contentTB.Foreground = solidColorBrush;
            fontColorL.Foreground = solidColorBrush;
        }
        #endregion

        private void AddChildToMesListSP(UIElement element)
        {
            if (messageListSP.Children.Count == maxMesListLen)
                messageListSP.Children.RemoveAt(0);
            messageListSP.Children.Add(element);
            messageListSV.ScrollToEnd();
        }

        //供上层调用的方法
        #region
        public void CodeClose()
        {
            manualClosed = false;
            Close();
        }

        public void MessageArrive(MessageDictionary e)
        {
            MessageUC messageUC;
            if ((Sender)Enum.Parse(typeof(Sender), e[MesKeyStr.Sender]) == Sender.self)
                messageUC = new MessageUC(e, Sender.self);
            else
                messageUC = new MessageUC(e);
            AddChildToMesListSP(messageUC);
        }

        public void UserJoin(User e)
        {
            userList.Add(e);
            UpdateTitle();

            Label label = new Label();
            label.Content = "用户 " + e.ToString() + " 加入群聊";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            AddChildToMesListSP(label);
        }

        public void UserQuit(User e)
        {
            userList.Remove(e);
            UpdateTitle();

            Label label = new Label();
            label.Content = "用户 " + e.ToString() + " 退出群聊";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            AddChildToMesListSP(label);
        }

        public void TargetQuit(User u)
        {
            if (MessageBox.Show(u.ToString() + "已离线，是否关闭本窗口？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Close();
            else
                sendB.IsEnabled = false;
            return;
        }
        #endregion


    }
}
