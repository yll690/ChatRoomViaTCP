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
using System.IO;

namespace Common
{
    public enum DisplayMethod
    {
        None,
        OnlyStyle,
        OnlyRemark,
        Both
    }

    /// <summary>
    /// MessageUC.xaml 的交互逻辑
    /// </summary>
    public partial class MessageUC : UserControl
    {
        public MessageDictionary ChatMessageP { get; private set; }
        public DisplayMethod DisplayMethodP { get; private set; }
        public string imagePath = "";

        public MessageUC(MessageDictionary message, DisplayMethod displayMethod) : this(message)
        {
            DisplayMethodP = displayMethod;
            Display();
        }

        public MessageUC(MessageDictionary message)
        {
            InitializeComponent();
            DisplayMethodP = DisplayMethod.OnlyStyle;
            ChatMessageP = message;
        }

        public MessageUC(MessageDictionary message, Sender sender) : this(message)
        {
            Display();
            if (sender == Sender.self)
            {
                contentB.Margin = new Thickness(50, 2, 5, 5);
                contentB.Background = new SolidColorBrush(Color.FromRgb(120, 205, 248));
                contentB.HorizontalAlignment = HorizontalAlignment.Right;
                infoG.Visibility = Visibility.Collapsed;
            }
        }
        
        private void Display()
        {
            nickNameL.Content = ChatMessageP[MesKeyStr.NickName] + "(" + ChatMessageP[MesKeyStr.UserID] + ")";
            ipAdressL.Content = ChatMessageP[MesKeyStr.IP];
            timeL.Content = ChatMessageP[MesKeyStr.DateTime];
            if ((DisplayMethodP == DisplayMethod.OnlyRemark || DisplayMethodP == DisplayMethod.Both) && ChatMessageP.ContainsKey(MesKeyStr.Remark))
                remarkL.Content = ChatMessageP[MesKeyStr.Remark];

            if (DisplayMethodP == DisplayMethod.OnlyRemark || DisplayMethodP == DisplayMethod.None)
            {
                contentB.Background = null;
                contentB.Margin = new Thickness(5, 0, 5, 5);
                contentTB.Margin = new Thickness(0);
            }

            if ((MessageType)Enum.Parse(typeof(MessageType), ChatMessageP[MesKeyStr.MessageType]) == MessageType.Text)
            {
                string content = StaticStuff.RepToSep(ChatMessageP[MesKeyStr.Content]);
                contentTB.ToolTip = content;
                contentTB.Text = content;

                if (DisplayMethodP == DisplayMethod.OnlyStyle || DisplayMethodP == DisplayMethod.Both)
                {
                    FontFamilyConverter fontFamilyConverter = new FontFamilyConverter();
                    try
                    {
                        contentTB.FontFamily = (FontFamily)fontFamilyConverter.ConvertFromString(ChatMessageP[MesKeyStr.FontFamily]);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message + e.StackTrace);
                    }
                    contentTB.FontSize = int.Parse(ChatMessageP[MesKeyStr.FontSize]);
                    if (int.Parse(ChatMessageP[MesKeyStr.FontStyle]) % 10 == 1)
                        contentTB.FontWeight = FontWeights.Bold;
                    if (int.Parse(ChatMessageP[MesKeyStr.FontStyle]) / 10 % 10 == 1)
                        contentTB.FontStyle = FontStyles.Italic;
                    if (int.Parse(ChatMessageP[MesKeyStr.FontStyle]) / 100 % 10 == 1)
                        contentTB.TextDecorations = TextDecorations.Underline;
                    contentTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ChatMessageP[MesKeyStr.FontColor]));
                }
            }
            else
            {
                string base64String = ChatMessageP[MesKeyStr.Base64String];
                if (Directory.Exists("Image") == false)
                    Directory.CreateDirectory("Image");
                imagePath = "Image\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + DateTime.Now.Millisecond + ".jpg";
                byte[] buffer = Convert.FromBase64String(base64String);
                File.WriteAllBytes(imagePath, buffer);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(buffer);
                bitmapImage.EndInit();
                Image image = new Image();
                image.MaxHeight = 300;
                image.ToolTip = "双击查看原图";
                image.Source = bitmapImage;
                imageCC.Content = image;
            }
        }

        private void imageCC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (imagePath != "")
                System.Diagnostics.Process.Start(imagePath);
        }
    }
}
