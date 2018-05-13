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

namespace Client
{
    /// <summary>
    /// MessageUC.xaml 的交互逻辑
    /// </summary>
    public partial class MessageUC : UserControl
    {
        public MessageD ChatMessageP { get;private set; }

        public MessageUC(MessageD message, bool displayStyle)
        {
            InitializeComponent();
            ChatMessageP = message;
            if (displayStyle == true)
                Display();
            else
            {
                contentB.Background = null;
                contentB.Margin = new Thickness(5, 0, 5, 5);
                contentTB.Margin = new Thickness(0);
                DisplayWithNoStyle();
            }
        }

        public MessageUC(MessageD message)
        {
            InitializeComponent();
            ChatMessageP = message;
            Display();
        }

        void DisplayWithNoStyle()
        {
            contentTB.ToolTip = ChatMessageP[MesKeyStr.Content];
            contentTB.Text = ChatMessageP[MesKeyStr.Content];
            nickNameL.Content = ChatMessageP[MesKeyStr.NickName] + "(" + ChatMessageP[MesKeyStr.UserID] + ")";
            ipAdressL.Content = ChatMessageP[MesKeyStr.IP];
            timeL.Content = ChatMessageP[MesKeyStr.DateTime];
        }

        void Display()
        {
            DisplayWithNoStyle();
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
