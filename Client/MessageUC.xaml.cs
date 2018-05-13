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
        public ChatMessage ChatMessageP { get;private set; }

        public MessageUC(ChatMessage message, bool displayStyle)
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

        public MessageUC(ChatMessage message)
        {
            InitializeComponent();
            ChatMessageP = message;
            Display();
        }

        void DisplayWithNoStyle()
        {
            contentTB.ToolTip = ChatMessageP.Content;
            contentTB.Text = ChatMessageP.Content;
            nickNameL.Content = ChatMessageP.NickName + "(" + ChatMessageP.UserID + ")";
            ipAdressL.Content = ChatMessageP.IP;
            timeL.Content = ChatMessageP.Time;
        }

        void Display()
        {
            DisplayWithNoStyle();
            FontFamilyConverter fontFamilyConverter = new FontFamilyConverter();
            try
            {
                contentTB.FontFamily = (FontFamily)fontFamilyConverter.ConvertFromString(ChatMessageP.FontFamily);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }
            contentTB.FontSize = ChatMessageP.FontSize;
            if (ChatMessageP.FontStyle % 10 == 1)
                contentTB.FontWeight = FontWeights.Bold;
            if (ChatMessageP.FontStyle / 10 % 10 == 1)
                contentTB.FontStyle = FontStyles.Italic;
            if (ChatMessageP.FontStyle / 100 % 10 == 1)
                contentTB.TextDecorations = TextDecorations.Underline;
            contentTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ChatMessageP.FontColor));
        }
    }
}
