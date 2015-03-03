using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ajou_Notice
{
    /// <summary>
    /// ViewWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewWindow : Window
    {
        public ViewWindow(string[] arg0)
        {
            InitializeComponent();

            noticeContext.Content = arg0[8];

            if (arg0[1].Length >= 20)
                noticeContentTitle.Content = arg0[1].Substring(0, 20);
            else
                noticeContentTitle.Content = arg0[1];

            string[] imgLoc = arg0[9].Split('|');

            for (int i = 0; i < imgLoc.Length; i++)
            {
                var img = new Image();
                img.Source = new BitmapImage(new Uri(String.Format("http://www.ajou.ac.kr/{0}", imgLoc[i])));
                img.Stretch = Stretch.None;

                img.SetValue(Grid.RowProperty, 1);
                stPanel.Children.Add(img);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void clipImg_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void closeImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void miniImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }
    }
}
