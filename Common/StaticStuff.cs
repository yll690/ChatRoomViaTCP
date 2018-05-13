using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public static class StaticStuff
    {
        public static char separator = ';';

        public static string GetMD5(string word)
        {
            if (word == "" || word == null)
                return "";
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider MD5CSP
                 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(word);
                byte[] bytHash = MD5CSP.ComputeHash(bytValue);
                MD5CSP.Clear();
                //根据计算得到的Hash码翻译为MD5码
                string sHash = "", sTemp = "";
                for (int counter = 0; counter < bytHash.Count(); counter++)
                {
                    long i = bytHash[counter] / 16;
                    if (i > 9)
                    {
                        sTemp = ((char)(i - 10 + 0x41)).ToString();
                    }
                    else
                    {
                        sTemp = ((char)(i + 0x30)).ToString();
                    }
                    i = bytHash[counter] % 16;
                    if (i > 9)
                    {
                        sTemp += ((char)(i - 10 + 0x41)).ToString();
                    }
                    else
                    {
                        sTemp += ((char)(i + 0x30)).ToString();
                    }
                    sHash += sTemp;
                }
                //根据大小写规则决定返回的字符串
                return sHash;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + ex.StackTrace);
                return "";
                //throw new Exception(ex.Message);
            }
        }
    }

    public static class MesKeyStr
    {
        public static string CommandType = "CommandType";
        public static string UserID = "UserID";
        public static string PassWord = "PassWord";
        public static string NickName = "NickName";
        public static string LoginResult = "LoginResult";
        public static string Content = "Content";
        public static string MessageType = "MessageType";
        public static string IP = "IP";
        public static string DateTime = "DateTime";
        public static string FontStyle = "FontStyle";
        public static string FontSize = "FontSize";
        public static string FontFamily = "FontFamily";
        public static string FontColor = "FontColor";
    }
}
