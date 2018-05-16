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
        public MessageDictionary ChatMessageP { get;private set; }
        public DisplayMethod DisplayMethodP { get; private set; }

        public MessageUC(MessageDictionary message, DisplayMethod displayMethod):this(message)
        {
            DisplayMethodP = displayMethod;
        }

        public MessageUC(MessageDictionary message)
        {
            InitializeComponent();
            DisplayMethodP = DisplayMethod.OnlyStyle;
            ChatMessageP = message;
            Display();
        }

        public MessageUC(MessageDictionary message, Sender sender) : this(message)
        {
            Display();
            if (sender==Sender.self)
            {
                contentB.Margin = new Thickness(50, 2, 5, 5);
                contentB.Background = new SolidColorBrush(Color.FromRgb(120,205,248));
                contentB.HorizontalAlignment = HorizontalAlignment.Right;
                infoG.Visibility = Visibility.Collapsed;
            }
        }

        private void DisplayWithNoStyle()
        {
            string content = StaticStuff.RepToSep(ChatMessageP[MesKeyStr.Content]);
            contentTB.ToolTip = content;
            contentTB.Text = content;
            nickNameL.Content = ChatMessageP[MesKeyStr.NickName] + "(" + ChatMessageP[MesKeyStr.UserID] + ")";
            ipAdressL.Content = ChatMessageP[MesKeyStr.IP];
            timeL.Content = ChatMessageP[MesKeyStr.DateTime];
            if ((DisplayMethodP == DisplayMethod.OnlyRemark || DisplayMethodP == DisplayMethod.Both) && ChatMessageP.ContainsKey(MesKeyStr.Remark))
                remarkL.Content = ChatMessageP[MesKeyStr.Remark];
        }

        private void Display()
        {
            DisplayWithNoStyle();
            if (DisplayMethodP == DisplayMethod.OnlyRemark || DisplayMethodP == DisplayMethod.None)
            {
                contentB.Background = null;
                contentB.Margin = new Thickness(5, 0, 5, 5);
                contentTB.Margin = new Thickness(0);
                DisplayWithNoStyle();
                return;
            }

            FontFamilyConverter fontFamilyConverter = new FontFamilyConverter();
            try
            {
                contentTB.FontFamily = (FontFamily)fontFamilyConverter.ConvertFromString(ChatMessageP[MesKeyStr.FontFamily]);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }
            contentTB.FontSize = int.Parse( ChatMessageP[MesKeyStr.FontSize]);
            if (int.Parse(ChatMessageP[MesKeyStr.FontStyle]) % 10 == 1)
                contentTB.FontWeight = FontWeights.Bold;
            if (int.Parse(ChatMessageP[MesKeyStr.FontStyle]) / 10 % 10 == 1)
                contentTB.FontStyle = FontStyles.Italic;
            if (int.Parse(ChatMessageP[MesKeyStr.FontStyle]) / 100 % 10 == 1)
                contentTB.TextDecorations = TextDecorations.Underline;
            contentTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ChatMessageP[MesKeyStr.FontColor]));
        }
    }
}
