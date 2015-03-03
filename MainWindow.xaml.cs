/*
 *                  AJOU NOTICE - 1.0 - Preliminary Version
 * 
 * Module Name      : Main Window View (MainWindow.xaml.cs)
 * Author           : k3nuku, Sung Mi Park
 * Created at       : 2014-09-05
 * Last Modified    : 2014-09-22
 * 
 * Current Status   : On-Developing [500]
 * 
 */

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Collections;
using Ajou_Notice.Code.Modules;

using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;

namespace Ajou_Notice
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] ajou_tab_list_count = new int[4];
        private string ajou_view_last_notice_date = "";
        private string ajou_view_last_custom_notice_date = "";
        private string eclass_currentselectedSubject = "";
        private Dictionary<string, Label> windowLabel = new Dictionary<string, Label>();

        public MainWindow()
        {
            InitializeComponent();

            masking_eClass_needsLogin(); // 이클래스 뷰 마스킹
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] noticeDummy = null;
            object[] noticeDummy_custom = null;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((s, f) => noticeDummy = ParseModule.Get_totalNotice("total"));
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, f) =>
            {
                ajou_tab_grid.Children.RemoveAt(0);
                ajou_tab_view_update(noticeDummy);
            });
            bw.RunWorkerAsync();

            BackgroundWorker bw1 = new BackgroundWorker();
            bw1.DoWork += new DoWorkEventHandler((s, f) => noticeDummy_custom = ParseModule.Get_totalNotice("custom"));
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, f) =>
            {
                ajou_tab_grid_custom.Children.RemoveAt(0);
                ajou_tab_view_update_custom(noticeDummy_custom);
            });
            bw1.RunWorkerAsync();

            calendar_tab_view_update(DateTime.Now.Year, DateTime.Now.Month);
        }

        #region Ajou Tab Update
        private void ajou_tab_view_update(object[] noticeListDummy, string count = null)
        {
            int countWholeList = 0;

            if (count != null)
            {
                RowDefinition rdReflect = new RowDefinition();
                rdReflect.Height = new GridLength(21);

                ajou_tab_grid.Children.RemoveAt(ajou_tab_grid.Children.Count - 1);
                ajou_tab_grid.RowDefinitions.RemoveAt(ajou_tab_grid.RowDefinitions.Count - 1);

                countWholeList = ajou_tab_list_count[2];
            }

            string[] noticeDateList = noticeListDummy[0] as string[];
            Array noticeList = noticeListDummy[1] as Array;

            for (int i = 0; i < noticeDateList.Length; i++)
            {
                #region 회색 Notice Head Definition area
                if (noticeDateList[0] != ajou_view_last_notice_date) // 현재 불러온 첫 번째 공지사항의 쓰여진 날짜와 최근 노티스 데이터의 마지막 날짜가 다른 경우
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(21);

                    Image grayNoticeHeadImg = new Image();
                    Label lblRefreshedDate = new Label();

                    grayNoticeHeadImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-110(width22).png", UriKind.Absolute));
                    grayNoticeHeadImg.Stretch = Stretch.None;
                    lblRefreshedDate.Content = noticeDateList[i] + "  ";
                    lblRefreshedDate.FontFamily = new FontFamily("나눔고딕");
                    lblRefreshedDate.FontSize = 10;
                    lblRefreshedDate.FontStyle = FontStyles.Italic;
                    lblRefreshedDate.HorizontalAlignment = HorizontalAlignment.Left;
                    lblRefreshedDate.VerticalAlignment = VerticalAlignment.Center;
                    lblRefreshedDate.Foreground = (Brush)new BrushConverter().ConvertFrom("#666666");

                    grayNoticeHeadImg.SetValue(Grid.RowProperty, countWholeList);
                    grayNoticeHeadImg.SetValue(Grid.ColumnSpanProperty, 3);
                    lblRefreshedDate.SetValue(Grid.RowProperty, countWholeList);
                    lblRefreshedDate.SetValue(Grid.ColumnProperty, 0);
                    lblRefreshedDate.SetValue(Grid.ColumnSpanProperty, 2);

                   
                    ajou_tab_grid.RowDefinitions.Add(rd);
                    ajou_tab_grid.Children.Add(grayNoticeHeadImg);
                    ajou_tab_grid.Children.Add(lblRefreshedDate);


                    countWholeList++;
                }
                else
                    ajou_view_last_notice_date = ""; // 같은 경우 해당 날짜만 공지를 잇도록 값 초기화

                #endregion

                string[][] noticeListByDate = noticeList.GetValue(i) as string[][]; // 해당 날짜의 공지목록

                for (int j = 0; j < noticeListByDate.Length; j++)
                {
                    RowDefinition rd_not = new RowDefinition();
                    rd_not.Height = new GridLength(28);

                        ajou_tab_grid.RowDefinitions.Add(rd_not);
                    
                    string[] noticePart = noticeListByDate[j];
                    Label[] lblArray = new Label[3];

                    #region Label 생성 Array
                    for (int k = 0; k < lblArray.Length; k++)
                    {
                        lblArray[k] = new Label();
                        lblArray[k].VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        lblArray[k].FontFamily = new FontFamily("나눔고딕");
                        lblArray[k].FontSize = 11;
                        lblArray[k].MouseDown += delegate
                        {
                            new ViewWindow(noticePart).ShowDialog();
                        };

                        lblArray[k].SetValue(Grid.RowProperty, countWholeList);
                        lblArray[k].SetValue(Grid.ColumnProperty, k);

                        if (k == 0)
                        {
                            lblArray[0].Name = "noticeLabel_category_" + i;
                            lblArray[0].Content = noticePart[3];
                            lblArray[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        }
                        else if (k == 1)
                        {
                            lblArray[1].Name = "noticeLabel_title_" + i;
                            lblArray[1].Content = noticePart[1];
                            lblArray[1].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        }
                        else
                        {
                            lblArray[2].Name = "noticeLabel_duedate_" + i;
                            lblArray[2].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                            string[] dateD = noticePart[4].Split('-');
                            DateTime duedate = new DateTime(Int16.Parse(dateD[0]), Int16.Parse(dateD[1]), Int16.Parse(dateD[2]));

                            lblArray[2].Content = duedate.Month + "월 " + duedate.Day + "일";

                            TimeSpan tsDiffer = duedate - DateTime.Now;

                            if (tsDiffer.TotalDays <= 3)
                                lblArray[2].Foreground = (Brush)new BrushConverter().ConvertFrom("#FF0000");
                        }

                            ajou_tab_grid.Children.Add(lblArray[k]);
                        
                    }
                    #endregion

                    Image grayNoticeLineImg = new Image(); // create a image instance
                    grayNoticeLineImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/10-158 + 28x(x=0,1,2..).png", UriKind.Absolute));
                    grayNoticeLineImg.Stretch = Stretch.None;
                    grayNoticeLineImg.Margin = new Thickness(10, 27, 1, 27);
                    grayNoticeLineImg.SetValue(Grid.RowProperty, countWholeList); // set row property to element
                    grayNoticeLineImg.SetValue(Grid.ColumnSpanProperty, 3);
                    grayNoticeLineImg.SetValue(Grid.RowSpanProperty, 2);

                    ajou_tab_grid.Children.Add(grayNoticeLineImg);

                    countWholeList++;
                }
            }

            ajou_view_last_notice_date = noticeDateList[noticeDateList.Length - 1];

            ajou_tab_list_count[0] = countWholeList - noticeDateList.Length;
            ajou_tab_list_count[2] = countWholeList;

            RowDefinition rd_mNImg = new RowDefinition();
            rd_mNImg.Height = new GridLength(36);
            Image moreNoticesImg = new Image();
            moreNoticesImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-end.png", UriKind.Absolute));
            moreNoticesImg.Stretch = Stretch.None;
            moreNoticesImg.MouseDown += delegate
            {
                pleaseLoading_Change_MoreData("ajou_tab_total");

                object[] obj = null;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler((s, e) => obj = ParseModule.Get_totalNotice("total", ajou_tab_list_count[0].ToString()));
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => ajou_tab_view_update(obj, ajou_tab_list_count[0].ToString()));
                bw.RunWorkerAsync();
            };

            moreNoticesImg.SetValue(Grid.RowProperty, countWholeList);
            moreNoticesImg.SetValue(Grid.ColumnSpanProperty, 3);

            ajou_tab_grid.RowDefinitions.Add(rd_mNImg);
            ajou_tab_grid.Children.Add(moreNoticesImg);
        }

        private void ajou_tab_view_update_custom(object[] noticeListDummy, string count = null)
        {
            int countWholeList = 0;

            if (count != null)
            {
                RowDefinition rdReflect = new RowDefinition();
                rdReflect.Height = new GridLength(21);

                ajou_tab_grid_custom.Children.RemoveAt(ajou_tab_grid_custom.Children.Count - 1);
                ajou_tab_grid_custom.RowDefinitions.RemoveAt(ajou_tab_grid_custom.RowDefinitions.Count - 1);

                countWholeList = ajou_tab_list_count[3];
            }

            string[] noticeDateList = noticeListDummy[0] as string[];
            Array noticeList = noticeListDummy[1] as Array;

            for (int i = 0; i < noticeDateList.Length; i++)
            {
                #region 회색 Notice Head Definition area
                if (noticeDateList[0] != ajou_view_last_custom_notice_date) // 현재 불러온 첫 번째 공지사항의 쓰여진 날짜와 최근 노티스 데이터의 마지막 날짜가 다른 경우
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(21);

                    Image grayNoticeHeadImg = null;
                    Label lblRefreshedDate = null;

                    grayNoticeHeadImg = new Image();
                    lblRefreshedDate = new Label();

                    grayNoticeHeadImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-110(width22).png", UriKind.Absolute));
                    grayNoticeHeadImg.Stretch = Stretch.None;
                    lblRefreshedDate.Content = noticeDateList[i] + "  ";
                    lblRefreshedDate.FontFamily = new FontFamily("나눔고딕");
                    lblRefreshedDate.FontSize = 10;
                    lblRefreshedDate.FontStyle = FontStyles.Italic;
                    lblRefreshedDate.HorizontalAlignment = HorizontalAlignment.Left;
                    lblRefreshedDate.VerticalAlignment = VerticalAlignment.Center;
                    lblRefreshedDate.Foreground = (Brush)new BrushConverter().ConvertFrom("#666666");

                    ajou_tab_grid_custom.RowDefinitions.Add(rd);
                    ajou_tab_grid_custom.Children.Add(grayNoticeHeadImg);
                    ajou_tab_grid_custom.Children.Add(lblRefreshedDate);

                    grayNoticeHeadImg.SetValue(Grid.RowProperty, countWholeList);
                    grayNoticeHeadImg.SetValue(Grid.ColumnSpanProperty, 3);
                    lblRefreshedDate.SetValue(Grid.RowProperty, countWholeList);
                    lblRefreshedDate.SetValue(Grid.ColumnProperty, 0);
                    lblRefreshedDate.SetValue(Grid.ColumnSpanProperty, 2);

                    countWholeList++;
                }
                else
                    ajou_view_last_custom_notice_date = ""; // 같은 경우 해당 날짜만 공지를 잇도록 값 초기화
                #endregion

                string[][] noticeListByDate = noticeList.GetValue(i) as string[][]; // 해당 날짜의 공지목록

                for (int j = 0; j < noticeListByDate.Length; j++)
                {
                    RowDefinition rd_not = new RowDefinition();
                    rd_not.Height = new GridLength(28);
                    ajou_tab_grid_custom.RowDefinitions.Add(rd_not);

                    string[] noticePart = noticeListByDate[j];
                    Label[] lblArray = new Label[3];

                    #region Button 생성 Array
                    for (int k = 0; k < lblArray.Length; k++)
                    {
                        lblArray[k] = new Label();
                        lblArray[k].VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        lblArray[k].FontFamily = new FontFamily("나눔고딕");
                        lblArray[k].FontSize = 11;
                        lblArray[k].MouseDown += delegate
                        {
                            new ViewWindow(noticePart).ShowDialog();
                        };

                        ajou_tab_grid_custom.Children.Add(lblArray[k]);
                        lblArray[k].SetValue(Grid.RowProperty, countWholeList);
                        lblArray[k].SetValue(Grid.ColumnProperty, k);

                        if (k == 0)
                        {
                            lblArray[0].Name = "noticeLabel_category_" + i;
                            lblArray[0].Content = noticePart[3];
                            lblArray[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        }
                        else if (k == 1)
                        {
                            lblArray[1].Name = "noticeLabel_title_" + i;
                            lblArray[1].Content = noticePart[1];
                            lblArray[1].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        }
                        else
                        {
                            lblArray[2].Name = "noticeLabel_duedate_" + i;
                            lblArray[2].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                            string[] dateD = noticePart[4].Split('-');
                            DateTime duedate = new DateTime(Int16.Parse(dateD[0]), Int16.Parse(dateD[1]), Int16.Parse(dateD[2]));

                            lblArray[2].Content = duedate.Month + "월 " + duedate.Day + "일";

                            TimeSpan tsDiffer = duedate - DateTime.Now;

                            if (tsDiffer.TotalDays <= 3)
                                lblArray[2].Foreground = (Brush)new BrushConverter().ConvertFrom("#FF0000");
                        }
                    }
                    #endregion

                    Image grayNoticeLineImg = new Image(); // create a image instance
                    grayNoticeLineImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/10-158 + 28x(x=0,1,2..).png", UriKind.Absolute));
                    grayNoticeLineImg.Stretch = Stretch.None;
                    grayNoticeLineImg.Margin = new Thickness(10, 27, 1, 27);

                    ajou_tab_grid_custom.Children.Add(grayNoticeLineImg); // add element to grid
                    grayNoticeLineImg.SetValue(Grid.RowProperty, countWholeList); // set row property to element
                    grayNoticeLineImg.SetValue(Grid.ColumnSpanProperty, 3);
                    grayNoticeLineImg.SetValue(Grid.RowSpanProperty, 2);

                    countWholeList++;
                }
            }

            ajou_view_last_custom_notice_date = noticeDateList[noticeDateList.Length - 1];

            ajou_tab_list_count[1] = countWholeList - noticeDateList.Length;
            ajou_tab_list_count[3] = countWholeList;

            RowDefinition rd_mNImg = new RowDefinition();
            rd_mNImg.Height = new GridLength(36);
            Image moreNoticesImg = new Image();
            moreNoticesImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-end.png", UriKind.Absolute));
            moreNoticesImg.Stretch = Stretch.None;
            moreNoticesImg.MouseDown += delegate
            {
                pleaseLoading_Change_MoreData("ajou_tab_custom");

                object[] obj = null;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler((s, e) => obj = ParseModule.Get_totalNotice("custom", ajou_tab_list_count[1].ToString()));
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => ajou_tab_view_update_custom(obj, ajou_tab_list_count[1].ToString()));
                bw.RunWorkerAsync();
            };

            ajou_tab_grid_custom.RowDefinitions.Add(rd_mNImg);
            ajou_tab_grid_custom.Children.Add(moreNoticesImg);

            moreNoticesImg.SetValue(Grid.RowProperty, countWholeList);
            moreNoticesImg.SetValue(Grid.ColumnSpanProperty, 3);
        }

        #endregion

        #region eclass Tab Update
        /// <summary>
        /// 이클래스 탭의 과목 카테고리를 업데이트합니다.
        /// </summary>
        private void eclass_tab_category_view_update()
        {
            category_selector_grid.Children.Clear();
            
            Array usrSubject = LoginModule._getInstance().UsrSubject;

            int objectCount = usrSubject.Length;
            double maxLabelWidth = 0;

            Label[] lblArray = new Label[objectCount];

            for (int i = 0; i < objectCount; i++)
            {
                RowDefinition rd_not = new RowDefinition();
                rd_not.Height = new GridLength(25);
                category_selector_grid.RowDefinitions.Add(rd_not);
                
                lblArray[i] = new Label();
                lblArray[i].Name = "categoryBtnNo" + i;
                lblArray[i].Content = ((string[])usrSubject.GetValue(i))[1];
                lblArray[i].FontFamily = new FontFamily("나눔고딕");
                lblArray[i].FontSize = 11;
                lblArray[i].Margin = new Thickness(10, 0, 0, 0);
                lblArray[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                lblArray[i].VerticalAlignment = System.Windows.VerticalAlignment.Center;
                
                category_selector_grid.Children.Add(lblArray[i]);

                lblArray[i].SetValue(Grid.RowProperty, i);
                lblArray[i].MouseDown += delegate(object sender, MouseButtonEventArgs e)
                {
                    Array _usrSubject = LoginModule._getInstance().UsrSubject;
                    string senderString = ((Label)sender).Content as string;
                    string clubId = "";

                    for (int k = 0; k < _usrSubject.Length; k++)
                        if (senderString == ((string[])_usrSubject.GetValue(k))[1])
                            clubId = ((string[])_usrSubject.GetValue(k))[0];

                    eclass_currentselectedSubject = senderString;
                    category_selector_panel.Visibility = System.Windows.Visibility.Hidden;

                    eclass_tab_notice_view_update(clubId);
                    eclass_tab_lectureNote_view_update(clubId);
                    eclass_tab_assignment_view_update(clubId);
                };
                
                lblArray[i].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (maxLabelWidth < lblArray[i].DesiredSize.Width)
                    maxLabelWidth = lblArray[i].DesiredSize.Width;
            }

            category_selector_panel.Margin = new Thickness(6, -4, 340 - maxLabelWidth, 438 - objectCount * 25);
        }

        /// <summary>
        /// 이클래스 탭의 과제 뷰를 업데이트합니다.
        /// </summary>
        /// <param name="clubId">clubID</param>
        private void eclass_tab_assignment_view_update(string clubId)
        {
            eclass_tab_grid_assignment.Children.Clear();
            eclass_tab_grid_assignment.RowDefinitions.Clear();

            object[] noticeListDummy = ParseModule.Get_eClassAssignment(clubId, LoginModule._getInstance().UsrInfoNum);

            if (noticeListDummy == null)
            {
                Image imgNoData = new Image();
                imgNoData.Source = new BitmapImage(new Uri("pack://application:,,,/Images/nodata.png", UriKind.Absolute));
                imgNoData.SetValue(Grid.ColumnSpanProperty, 3);

                eclass_tab_grid_assignment.Children.Add(imgNoData);
                return;
            }

            string[] noticeDateList = noticeListDummy[0] as string[];
            Array noticeList = noticeListDummy[1] as Array;

            int countWholeList = 0;

            for (int i = 0; i < noticeDateList.Length; i++)
            {
                #region 회색 Notice Head Definition area
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(21);

                Image grayNoticeHeadImg = new Image();
                Label lblRefreshedDate = new Label();

                grayNoticeHeadImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-110(width22).png", UriKind.Absolute));
                grayNoticeHeadImg.Stretch = Stretch.None;
                lblRefreshedDate.Content = noticeDateList[i] + "  ";
                lblRefreshedDate.FontFamily = new FontFamily("나눔고딕");
                lblRefreshedDate.FontSize = 10;
                lblRefreshedDate.FontStyle = FontStyles.Italic;
                lblRefreshedDate.HorizontalAlignment = HorizontalAlignment.Left;
                lblRefreshedDate.VerticalAlignment = VerticalAlignment.Center;
                lblRefreshedDate.Foreground = (Brush)new BrushConverter().ConvertFrom("#666666");

                eclass_tab_grid_assignment.RowDefinitions.Add(rd);
                eclass_tab_grid_assignment.Children.Add(grayNoticeHeadImg);
                eclass_tab_grid_assignment.Children.Add(lblRefreshedDate);

                grayNoticeHeadImg.SetValue(Grid.RowProperty, countWholeList);
                grayNoticeHeadImg.SetValue(Grid.ColumnSpanProperty, 3);
                lblRefreshedDate.SetValue(Grid.RowProperty, countWholeList);
                lblRefreshedDate.SetValue(Grid.ColumnProperty, 0);
                lblRefreshedDate.SetValue(Grid.ColumnSpanProperty, 3);
                
                countWholeList++;
                #endregion

                string[][] noticeListByDate = noticeList.GetValue(i) as string[][]; // 해당 날짜의 공지목록

                for (int j = 0; j < noticeListByDate.Length; j++)
                {
                    RowDefinition rd_not = new RowDefinition();
                    rd_not.Height = new GridLength(28);
                    eclass_tab_grid_assignment.RowDefinitions.Add(rd_not);

                    string[] noticePart = noticeListByDate[j];
                    Label[] lblArray = new Label[3];

                    #region Label 생성 Array
                    for (int k = 0; k < lblArray.Length; k++)
                    {
                        lblArray[k] = new Label();
                        lblArray[k].VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        lblArray[k].FontFamily = new FontFamily("나눔고딕");
                        lblArray[k].FontSize = 11;
                        eclass_tab_grid_assignment.Children.Add(lblArray[k]);
                        lblArray[k].SetValue(Grid.RowProperty, countWholeList);
                        lblArray[k].SetValue(Grid.ColumnProperty, k);

                        if (k == 0)
                        {
                            lblArray[0].Name = "noticeLabel_category_" + i;
                            lblArray[0].Content = eclass_currentselectedSubject;
                            lblArray[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        }
                        else if (k == 1)
                        {
                            lblArray[1].Name = "noticeLabel_title_" + i;
                            lblArray[1].Content = noticePart[1];
                            lblArray[1].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        }
                        else
                        {
                            lblArray[2].Name = "noticelabel_duedate_" + i;
                            lblArray[2].Content = noticePart[3];
                            lblArray[2].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        }
                    }
                    #endregion

                    Image grayNoticeLineImg = new Image(); // create a image instance
                    grayNoticeLineImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/10-158 + 28x(x=0,1,2..).png", UriKind.Absolute));
                    grayNoticeLineImg.Stretch = Stretch.None;
                    grayNoticeLineImg.Margin = new Thickness(0, 27, 1, 27);

                    eclass_tab_grid_assignment.Children.Add(grayNoticeLineImg); // add element to grid
                    grayNoticeLineImg.SetValue(Grid.RowProperty, countWholeList); // set row property to element
                    grayNoticeLineImg.SetValue(Grid.ColumnSpanProperty, 3);
                    grayNoticeLineImg.SetValue(Grid.RowSpanProperty, 2);

                    countWholeList++;
                }
            }

            /*
            RowDefinition rd_mNImg = new RowDefinition();
            rd_mNImg.Height = new GridLength(36);

            Image moreNoticesImg = new Image();

            moreNoticesImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-end.png", UriKind.Absolute));
            moreNoticesImg.Stretch = Stretch.None;

            eclass_tab_grid_assignment.RowDefinitions.Add(rd_mNImg);
            eclass_tab_grid_assignment.Children.Add(moreNoticesImg);

            moreNoticesImg.SetValue(Grid.RowProperty, countWholeList);
            moreNoticesImg.SetValue(Grid.ColumnSpanProperty, 3);
            */

            /*Image spaces = new Image();

            spaces.Source = new BitmapImage(new Uri("pack://application:,,,/Images/0-110(settings).png", UriKind.Absolute));
            spaces.Stretch = Stretch.None;

            eclass_tab_grid_notice.Children.Add(spaces);

            spaces.SetValue(Grid.RowProperty, countWholeList + 1);
            spaces.SetValue(Grid.ColumnSpanProperty, 2);

            */
            eclass_assignment_notLoaded.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 이클래스 탭의 강의노트 뷰를 업데이트합니다.
        /// </summary>
        /// <param name="cludId">clubID</param>
        private void eclass_tab_lectureNote_view_update(string clubId)
        {
            eclass_tab_grid_lecturenote.Children.Clear();
            eclass_tab_grid_lecturenote.RowDefinitions.Clear();

            object[] noticeListDummy = ParseModule.Get_eClassLectureNote(clubId, LoginModule._getInstance().UsrInfoNum);

            if (noticeListDummy == null)
            {
                Image imgNoData = new Image();
                imgNoData.Source = new BitmapImage(new Uri("pack://application:,,,/Images/nodata.png", UriKind.Absolute));
                imgNoData.SetValue(Grid.ColumnSpanProperty, 3);

                eclass_tab_grid_lecturenote.Children.Add(imgNoData);

                return;
            }

            string[] noticeDateList = noticeListDummy[0] as string[];
            Array noticeList = noticeListDummy[1] as Array;

            int countWholeList = 0;

            for (int i = 0; i < noticeDateList.Length; i++)
            {
                #region 회색 Notice Head Definition area
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(21);

                Image grayNoticeHeadImg = new Image();
                Label lblRefreshedDate = new Label();

                grayNoticeHeadImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-110(width22).png", UriKind.Absolute));
                grayNoticeHeadImg.Stretch = Stretch.None;
                lblRefreshedDate.Content = noticeDateList[i] + "  ";
                lblRefreshedDate.FontFamily = new FontFamily("나눔고딕");
                lblRefreshedDate.FontSize = 10;
                lblRefreshedDate.FontStyle = FontStyles.Italic;
                lblRefreshedDate.HorizontalAlignment = HorizontalAlignment.Left;
                lblRefreshedDate.VerticalAlignment = VerticalAlignment.Center;
                lblRefreshedDate.Foreground = (Brush)new BrushConverter().ConvertFrom("#666666");

                eclass_tab_grid_lecturenote.RowDefinitions.Add(rd);
                eclass_tab_grid_lecturenote.Children.Add(grayNoticeHeadImg);
                eclass_tab_grid_lecturenote.Children.Add(lblRefreshedDate);

                grayNoticeHeadImg.SetValue(Grid.RowProperty, countWholeList);
                grayNoticeHeadImg.SetValue(Grid.ColumnSpanProperty, 2);
                lblRefreshedDate.SetValue(Grid.RowProperty, countWholeList);
                lblRefreshedDate.SetValue(Grid.ColumnProperty, 0);
                lblRefreshedDate.SetValue(Grid.ColumnSpanProperty, 2);

                countWholeList++;
                #endregion

                string[][] noticeListByDate = noticeList.GetValue(i) as string[][]; // 해당 날짜의 공지목록

                for (int j = 0; j < noticeListByDate.Length; j++)
                {
                    RowDefinition rd_not = new RowDefinition();
                    rd_not.Height = new GridLength(28);
                    eclass_tab_grid_lecturenote.RowDefinitions.Add(rd_not);

                    string[] noticePart = noticeListByDate[j];
                    Label[] lblArray = new Label[2];

                    #region Label 생성 Array
                    for (int k = 0; k < lblArray.Length; k++)
                    {
                        lblArray[k] = new Label();
                        lblArray[k].VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        lblArray[k].FontFamily = new FontFamily("나눔고딕");
                        lblArray[k].FontSize = 11;
                        eclass_tab_grid_lecturenote.Children.Add(lblArray[k]);
                        lblArray[k].SetValue(Grid.RowProperty, countWholeList);
                        lblArray[k].SetValue(Grid.ColumnProperty, k);

                        if (k == 0)
                        {
                            lblArray[0].Name = "noticeLabel_category_" + i;
                            lblArray[0].Content = eclass_currentselectedSubject;
                            lblArray[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        }
                        else if (k == 1)
                        {
                            lblArray[1].Name = "noticeLabel_title_" + i;
                            lblArray[1].Content = noticePart[1];
                            lblArray[1].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        }
                    }
                    #endregion

                    Image grayNoticeLineImg = new Image(); // create a image instance
                    grayNoticeLineImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/10-158 + 28x(x=0,1,2..).png", UriKind.Absolute));
                    grayNoticeLineImg.Stretch = Stretch.None;
                    grayNoticeLineImg.Margin = new Thickness(0, 27, 1, 27);

                    eclass_tab_grid_lecturenote.Children.Add(grayNoticeLineImg); // add element to grid
                    grayNoticeLineImg.SetValue(Grid.RowProperty, countWholeList); // set row property to element
                    grayNoticeLineImg.SetValue(Grid.ColumnSpanProperty, 2);
                    grayNoticeLineImg.SetValue(Grid.RowSpanProperty, 2);

                    countWholeList++;
                }
            }

            /*
            RowDefinition rd_mNImg = new RowDefinition();
            rd_mNImg.Height = new GridLength(36);

            Image moreNoticesImg = new Image();

            moreNoticesImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-end.png", UriKind.Absolute));
            moreNoticesImg.Stretch = Stretch.None;

            eclass_tab_grid_lecturenote.RowDefinitions.Add(rd_mNImg);
            eclass_tab_grid_lecturenote.Children.Add(moreNoticesImg);

            moreNoticesImg.SetValue(Grid.RowProperty, countWholeList);
            moreNoticesImg.SetValue(Grid.ColumnSpanProperty, 2);
            */

            /*Image spaces = new Image();

            spaces.Source = new BitmapImage(new Uri("pack://application:,,,/Images/0-110(settings).png", UriKind.Absolute));
            spaces.Stretch = Stretch.None;

            eclass_tab_grid_notice.Children.Add(spaces);

            spaces.SetValue(Grid.RowProperty, countWholeList + 1);
            spaces.SetValue(Grid.ColumnSpanProperty, 2);

            */
            eclass_lecturenote_notLoaded.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 이클래스 탭의 클래스 공지사항 뷰를 업데이트합니다.
        /// </summary>
        /// <param name="clubId"></param>
        private void eclass_tab_notice_view_update(string clubId)
        {
            eclass_tab_grid_notice.Children.Clear();
            eclass_tab_grid_notice.RowDefinitions.Clear();

            object[] noticeListDummy = ParseModule.Get_eClassNotice(clubId, LoginModule._getInstance().UsrInfoNum);

            if (noticeListDummy == null)
            {
                Image imgNoData = new Image();
                imgNoData.Source = new BitmapImage(new Uri("pack://application:,,,/Images/nodata.png", UriKind.Absolute));
                imgNoData.SetValue(Grid.ColumnSpanProperty, 3);

                eclass_tab_grid_notice.Children.Add(imgNoData);

                return;
            }

            string[] noticeDateList = noticeListDummy[0] as string[];
            Array noticeList = noticeListDummy[1] as Array;

            int countWholeList = 0;

            for (int i = 0; i < noticeDateList.Length; i++)
            {
                #region 회색 Notice Head Definition area
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(21);

                Image grayNoticeHeadImg = new Image();
                Label lblRefreshedDate = new Label();

                grayNoticeHeadImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-110(width22).png", UriKind.Absolute));
                grayNoticeHeadImg.Stretch = Stretch.None;
                lblRefreshedDate.Content = noticeDateList[i] + "  ";
                lblRefreshedDate.FontFamily = new FontFamily("나눔고딕");
                lblRefreshedDate.FontSize = 10;
                lblRefreshedDate.FontStyle = FontStyles.Italic;
                lblRefreshedDate.HorizontalAlignment = HorizontalAlignment.Left;
                lblRefreshedDate.VerticalAlignment = VerticalAlignment.Center;
                lblRefreshedDate.Foreground = (Brush)new BrushConverter().ConvertFrom("#666666");

                eclass_tab_grid_notice.RowDefinitions.Add(rd);
                eclass_tab_grid_notice.Children.Add(grayNoticeHeadImg);
                eclass_tab_grid_notice.Children.Add(lblRefreshedDate);

                grayNoticeHeadImg.SetValue(Grid.RowProperty, countWholeList);
                grayNoticeHeadImg.SetValue(Grid.ColumnSpanProperty, 2);
                lblRefreshedDate.SetValue(Grid.RowProperty, countWholeList);
                lblRefreshedDate.SetValue(Grid.ColumnProperty, 0);
                lblRefreshedDate.SetValue(Grid.ColumnSpanProperty, 2);

                countWholeList++;
                #endregion

                string[][] noticeListByDate = noticeList.GetValue(i) as string[][]; // 해당 날짜의 공지목록

                for (int j = 0; j < noticeListByDate.Length; j++)
                {
                    RowDefinition rd_not = new RowDefinition();
                    rd_not.Height = new GridLength(28);
                    eclass_tab_grid_notice.RowDefinitions.Add(rd_not);

                    string[] noticePart = noticeListByDate[j];
                    Label[] lblArray = new Label[2];

                    #region Label 생성 Array
                    for (int k = 0; k < lblArray.Length; k++)
                    {
                        lblArray[k] = new Label();
                        lblArray[k].VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        lblArray[k].FontFamily = new FontFamily("나눔고딕");
                        lblArray[k].FontSize = 11;
                        eclass_tab_grid_notice.Children.Add(lblArray[k]);
                        lblArray[k].SetValue(Grid.RowProperty, countWholeList);
                        lblArray[k].SetValue(Grid.ColumnProperty, k);

                        if (k == 0)
                        {
                            lblArray[0].Name = "noticeLabel_category_" + i;
                            lblArray[0].Content = eclass_currentselectedSubject;
                            lblArray[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        }
                        else if (k == 1)
                        {
                            lblArray[1].Name = "noticeLabel_title_" + i;
                            lblArray[1].Content = noticePart[1];
                            lblArray[1].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        }
                    }
                    #endregion

                    Image grayNoticeLineImg = new Image(); // create a image instance
                    grayNoticeLineImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/10-158 + 28x(x=0,1,2..).png", UriKind.Absolute));
                    grayNoticeLineImg.Stretch = Stretch.None;
                    grayNoticeLineImg.Margin = new Thickness(0, 27, 1, 27);

                    eclass_tab_grid_notice.Children.Add(grayNoticeLineImg); // add element to grid
                    grayNoticeLineImg.SetValue(Grid.RowProperty, countWholeList); // set row property to element
                    grayNoticeLineImg.SetValue(Grid.ColumnSpanProperty, 2);
                    grayNoticeLineImg.SetValue(Grid.RowSpanProperty, 2);

                    countWholeList++;
                }
            }

            /*
            RowDefinition rd_mNImg = new RowDefinition();
            rd_mNImg.Height = new GridLength(36);

            Image moreNoticesImg = new Image();

            moreNoticesImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/1-end.png", UriKind.Absolute));
            moreNoticesImg.Stretch = Stretch.None;

            eclass_tab_grid_notice.RowDefinitions.Add(rd_mNImg);
            eclass_tab_grid_notice.Children.Add(moreNoticesImg);

            moreNoticesImg.SetValue(Grid.RowProperty, countWholeList);
            moreNoticesImg.SetValue(Grid.ColumnSpanProperty, 2);
            */

            /*Image spaces = new Image();

            spaces.Source = new BitmapImage(new Uri("pack://application:,,,/Images/0-110(settings).png", UriKind.Absolute));
            spaces.Stretch = Stretch.None;

            eclass_tab_grid_notice.Children.Add(spaces);

            spaces.SetValue(Grid.RowProperty, countWholeList + 1);
            spaces.SetValue(Grid.ColumnSpanProperty, 2);

            */
            eclass_notice_notLoaded.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 이클래스 탭을 업데이트합니다. (이클래스 탭 버튼 누를 시)
        /// </summary>
        private void eclass_tab_view_update()
        {
            if (!LoginModule._getInstance().LoggedOn_eClass) // 로그인 되어있지 않을 시 강제 리턴
                return;

            //eclass_needsLogin.Visibility = System.Windows.Visibility.Hidden;

            if (category_selector_grid.Children.Count == 0) // 이클래스 과목 선택란에 과목이 없을 경우
            {
                eclass_tab_category_view_update(); // 카테고리를 업데이트 함
                eclass_currentselectedSubject = ((string[])LoginModule._getInstance().UsrSubject.GetValue(0))[1]; // 기본적으로 첫 번째 과목을 선택했음을 선언함

                eclass_tab_notice_view_update(((string[])LoginModule._getInstance().UsrSubject.GetValue(0))[0]); // 기본적으로 첫 번째 과목 공지를 불러옴
                eclass_tab_lectureNote_view_update(((string[])LoginModule._getInstance().UsrSubject.GetValue(0))[0]); // 기본적으로 첫 번째 과목 강의노트를 불러옴
                eclass_tab_assignment_view_update(((string[])LoginModule._getInstance().UsrSubject.GetValue(0))[0]); // 기본적으로 첫 번째 과목 과제를 불러옴
            }
        }
        #endregion

        #region Calendar Tab Update
        private void calendar_tab_view_update(int year, int month)
        {
            calendar_main_grid.Children.Clear();

            #region Initialize Calendar Tab Grid Components :: 140922
            RowDefinition[] rdArray = new RowDefinition[9];
            ColumnDefinition[] cdArray = new ColumnDefinition[7];

            for (int i = 0; i < rdArray.Length; i++)
            {
                rdArray[i] = new RowDefinition();

                if (i == 0)
                    rdArray[i].Height = new GridLength(3);
                else if (i == 1)
                    rdArray[i].Height = new GridLength(26);
                else if (i == 2)
                    rdArray[i].Height = new GridLength(35);
                else if (i == rdArray.Length)
                    rdArray[i].Height = new GridLength(64);
                else
                    rdArray[i].Height = new GridLength(62);

                calendar_main_grid.RowDefinitions.Add(rdArray[i]);
            }

            for (int i = 0; i < cdArray.Length; i++)
            {
                cdArray[i] = new ColumnDefinition();

                if (i == 0)
                    cdArray[i].Width = new GridLength(52);
                else if (i == cdArray.Length)
                    cdArray[i].Width = new GridLength(53);
                else
                    cdArray[i].Width = new GridLength(51);

                calendar_main_grid.ColumnDefinitions.Add(cdArray[i]);
            }

            Image calendarBackgroundImg = new Image();
            Image calendarLeftBtnImg = new Image();
            Image calendarRightBtnImg = new Image();
            Image calendarAddBtnImg = new Image();
            Label currentDateLabel = new Label();

            calendarBackgroundImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            calendarBackgroundImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            calendarBackgroundImg.Stretch = Stretch.None;
            calendarBackgroundImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/0-110(calendar).png", UriKind.Absolute));
            calendarBackgroundImg.SetValue(Grid.RowSpanProperty, 9);
            calendarBackgroundImg.SetValue(Grid.ColumnSpanProperty, 7);

            calendarLeftBtnImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            calendarLeftBtnImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            calendarLeftBtnImg.Stretch = Stretch.None;
            calendarLeftBtnImg.Margin = new Thickness(45, 0, 0, 0);
            calendarLeftBtnImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/97-113.png", UriKind.Absolute));
            calendarLeftBtnImg.MouseDown += delegate
            {
                string[] currentdate = windowLabel["calendar_currentDate"].Content.ToString().Split(' ');

                for (int i = 0; i < currentdate.Length; i++)
                    currentdate[i] = currentdate[i].Remove(currentdate[i].Length - 1, 1);

                if (Int16.Parse(currentdate[1]) == 1)
                {
                    currentdate[0] = (Int16.Parse(currentdate[0]) - 1).ToString();
                    currentdate[1] = "13";
                }

                calendar_tab_view_update(Int16.Parse(currentdate[0]), Int16.Parse(currentdate[1]) - 1);
            };

            calendarLeftBtnImg.SetValue(Grid.ColumnSpanProperty, 2);
            calendarLeftBtnImg.SetValue(Grid.RowProperty, 1);
            calendarLeftBtnImg.SetValue(Grid.ColumnProperty, 1);

            calendarRightBtnImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            calendarRightBtnImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            calendarRightBtnImg.Stretch = Stretch.None;
            calendarRightBtnImg.Margin = new Thickness(32, 0, 0, 0);
            calendarRightBtnImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/237-113.png", UriKind.Absolute));
            calendarRightBtnImg.MouseDown += delegate
            {
                string[] currentdate = windowLabel["calendar_currentDate"].Content.ToString().Split(' ');

                for (int i = 0; i < currentdate.Length; i++)
                    currentdate[i] = currentdate[i].Remove(currentdate[i].Length - 1, 1);

                if (Int16.Parse(currentdate[1]) == 12)
                {
                    currentdate[0] = (Int16.Parse(currentdate[0]) + 1).ToString();
                    currentdate[1] = "0";
                }

                calendar_tab_view_update(Int16.Parse(currentdate[0]), Int16.Parse(currentdate[1]) + 1);
            };
            calendarRightBtnImg.SetValue(Grid.ColumnSpanProperty, 2);
            calendarRightBtnImg.SetValue(Grid.RowProperty, 1);
            calendarRightBtnImg.SetValue(Grid.ColumnProperty, 4);

            calendarAddBtnImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            calendarAddBtnImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            calendarAddBtnImg.Stretch = Stretch.None;
            calendarAddBtnImg.Margin = new Thickness(0, 0, 14, 0);
            calendarAddBtnImg.Source = new BitmapImage(new Uri("pack://application:,,,/Images/320-113.png", UriKind.Absolute));
            calendarAddBtnImg.SetValue(Grid.RowProperty, 1);
            calendarAddBtnImg.SetValue(Grid.ColumnProperty, 6);

            currentDateLabel.Name = "calendar_currentDate";
            currentDateLabel.Content = String.Format("{0}년 {1}월", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("d2"));
            currentDateLabel.FontSize = 15;
            currentDateLabel.FontFamily = new FontFamily("나눔고딕");
            currentDateLabel.Foreground = (Brush)new BrushConverter().ConvertFrom("#333333");
            currentDateLabel.Margin = new Thickness(30, -2.5, 14, 0);
            currentDateLabel.SetValue(Grid.ColumnSpanProperty, 3);
            currentDateLabel.SetValue(Grid.RowProperty, 1);
            currentDateLabel.SetValue(Grid.ColumnProperty, 2);

            if (windowLabel.ContainsKey("calendar_currentDate"))
            {
                windowLabel.Clear();
                windowLabel.Add(currentDateLabel.Name, currentDateLabel);
            }
            else
                windowLabel.Add(currentDateLabel.Name, currentDateLabel);

            calendar_main_grid.Children.Add(calendarBackgroundImg);
            calendar_main_grid.Children.Add(calendarLeftBtnImg);
            calendar_main_grid.Children.Add(calendarRightBtnImg);
            calendar_main_grid.Children.Add(calendarAddBtnImg);
            calendar_main_grid.Children.Add(currentDateLabel);
            #endregion

            string[][] holidaylist1;
            string[][] holidaylist2;
            string[][] holidaylist3;

            if (month == 12)
            {
                holidaylist1 = ParseModule.getHolidayList(year, month - 1) as string[][];
                holidaylist2 = ParseModule.getHolidayList(year, month) as string[][];
                holidaylist3 = ParseModule.getHolidayList(year + 1, 1) as string[][];
            }
            else if (month == 1)
            {
                holidaylist1 = ParseModule.getHolidayList(year - 1, 12) as string[][];
                holidaylist2 = ParseModule.getHolidayList(year, month) as string[][];
                holidaylist3 = ParseModule.getHolidayList(year, month + 1) as string[][];
            }
            else
            {
                holidaylist1 = ParseModule.getHolidayList(year, month - 1) as string[][];
                holidaylist2 = ParseModule.getHolidayList(year, month) as string[][];
                holidaylist3 = ParseModule.getHolidayList(year, month + 1) as string[][];
            }

            int countnum = 0;

            windowLabel["calendar_currentDate"].Content = year + "년 " + month.ToString("d2") + "월";

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++) // 가로 한줄처리
                {
                    StackPanel stp = new StackPanel();
                    Grid grd = new Grid();

                    if (j == 0) // 첫번 째 라인(세로)일 때
                    {
                        stp.Width = 52;
                        grd.Width = 52;
                    }
                    else // 아니면
                    {
                        stp.Width = 51;
                        grd.Width = 51;
                    }

                    if (i == 5) // 마지막 번째 라인(가로) 일 때
                    {
                        stp.Height = 62;
                        grd.Height = 62;
                    }
                    else
                    {
                        stp.Height = 61;
                        grd.Height = 61;
                    }

                    stp.SetValue(Grid.RowProperty, i + 3);
                    stp.SetValue(Grid.ColumnProperty, j);

                    Label dateLabel = new Label();
                    Label scheduleLabel = new Label();

                    dateLabel.Name = String.Format("dateLabel_{0}{1}", i, j);
                    dateLabel.FontSize = 14;
                    dateLabel.HorizontalAlignment = HorizontalAlignment.Left;
                    dateLabel.VerticalAlignment = VerticalAlignment.Top;
                    dateLabel.FontFamily = new FontFamily("나눔고딕");
                    dateLabel.Margin = new Thickness(0, -2, 0, 0);
                    dateLabel.Foreground = (Brush)new BrushConverter().ConvertFrom("#333333");
                    scheduleLabel.FontSize = 10;
                    scheduleLabel.FontFamily = new FontFamily("나눔고딕");
                    scheduleLabel.Margin = new Thickness(-2, 16, 0, 0);

                    int labelint = countnum - TrivialModule.voidnum(year, month) + 1; // (labelint = 현재 생성하려는 번째 - 해당년월의 1일 시작점(요일) + 1)

                    if (labelint <= 0) // 저번 달 이면
                    {
                        if (month - 1 == 0) // 이번달이 1월인 경우
                            labelint = DateTime.DaysInMonth(year - 1, 12) + labelint; // 작년 해 저번 달 일수
                        else // 이번달이 1월이 아닌 경우
                            labelint = DateTime.DaysInMonth(year, month - 1) + labelint; // 바로 전 달 일수

                        dateLabel.Foreground = (Brush)new BrushConverter().ConvertFrom("#999999"); // dateLabel 색깔을 연한 색으로 (전 달이니까)

                        if (holidaylist1[labelint - 1][12].ToString() != "0") // 해당 일이 공휴일인경우
                        {
                            scheduleLabel.Content = holidaylist1[labelint - 1][10]; // 스케쥴라벨의 값을 공휴일 이름으로 지정
                            scheduleLabel.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF0000");

                            if (scheduleLabel.Content.ToString() == "") // 스케쥴라벨이 비어있을 경우
                                scheduleLabel.Content = holidaylist1[labelint - 1][11]; // 스케쥴라벨의 값을 공휴일이름(음력)으로 지정
                        }
                        else
                            scheduleLabel.Visibility = Visibility.Hidden; // 공휴일 아니면 숨겨
                    }
                    else if (labelint > DateTime.DaysInMonth(year, month)) // 다음 달 이면
                    {
                        labelint -= DateTime.DaysInMonth(year, month); // labelInt = 현재 생성하려는 번쨰 - 다음 달의 일수
                        dateLabel.Foreground = (Brush)new BrushConverter().ConvertFrom("#999999"); // dateLabel 색깔을 연한 색으로 (다음 달이니까)

                        if (holidaylist3[labelint - 1][12].ToString() != "0")
                        {
                            scheduleLabel.Content = holidaylist3[labelint - 1][10];
                            scheduleLabel.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF0000");

                            if (scheduleLabel.Content.ToString() == "")
                                scheduleLabel.Content = holidaylist3[labelint - 1][11];
                        }
                        else
                            scheduleLabel.Visibility = Visibility.Hidden;
                    }
                    else // 이번 달 이면
                    {
                        if (holidaylist2[labelint - 1][12].ToString() != "0")
                        {
                            scheduleLabel.Content = holidaylist2[labelint - 1][10];
                            scheduleLabel.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF0000");

                            if (scheduleLabel.Content.ToString() == "")
                                scheduleLabel.Content = holidaylist2[labelint - 1][11];
                        }
                        else
                            scheduleLabel.Visibility = Visibility.Hidden;
                    }

                    dateLabel.Content = labelint.ToString(); // dateLabel.Content를 labelint 값으로 적용

                    calendar_main_grid.Children.Add(stp);
                    stp.Children.Add(grd);
                    grd.Children.Add(dateLabel);
                    grd.Children.Add(scheduleLabel);

                    countnum++;
                }
            }
        }
        #endregion

        #region Screen Masking Methods
        private void masking_eClass_needsLogin()
        {
            Image imgNeedsLogin = new Image();
            imgNeedsLogin.Source = new BitmapImage(new Uri("pack://application:,,,/Images/needlogin.png", UriKind.Absolute));
            imgNeedsLogin.Stretch = Stretch.None;
            imgNeedsLogin.SetValue(Grid.ColumnSpanProperty, 3);

            Image imgNeedsLogin1 = new Image();
            imgNeedsLogin1.Source = new BitmapImage(new Uri("pack://application:,,,/Images/needlogin.png", UriKind.Absolute));
            imgNeedsLogin1.Stretch = Stretch.None;
            imgNeedsLogin1.SetValue(Grid.ColumnSpanProperty, 3);

            Image imgNeedsLogin2 = new Image();
            imgNeedsLogin2.Source = new BitmapImage(new Uri("pack://application:,,,/Images/needlogin.png", UriKind.Absolute));
            imgNeedsLogin2.Stretch = Stretch.None;
            imgNeedsLogin2.SetValue(Grid.ColumnSpanProperty, 3);

            eclass_tab_grid_notice.Children.Add(imgNeedsLogin);
            eclass_tab_grid_lecturenote.Children.Add(imgNeedsLogin1);
            eclass_tab_grid_assignment.Children.Add(imgNeedsLogin2);
        }

        private void pleaseLoading_Change_MoreData(string arg0)
        {
            var tabGrid = new Grid();

            if (arg0 == "ajou_tab_total")
                tabGrid = ajou_tab_grid;
            else if (arg0 == "ajou_tab_custom")
                tabGrid = ajou_tab_grid_custom;
            else if (arg0 == "eclass_tab_notice")
                tabGrid = eclass_tab_grid_notice;
            else if (arg0 == "eclass_tab_lecturenote")
                tabGrid = eclass_tab_grid_lecturenote;
            else if (arg0 == "eclass_tab_assignment")
                tabGrid = eclass_tab_grid_assignment;

            if(tabGrid.Children.Count < 20)
                return;

            if(tabGrid.Dispatcher.CheckAccess())
                ((Image)tabGrid.Children[tabGrid.Children.Count - 1]).Source = new BitmapImage(new Uri("pack://application:,,,/Images/loading_data.png", UriKind.Absolute));
            else
            {
                tabGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ((Image)tabGrid.Children[tabGrid.Children.Count - 1]).Source = new BitmapImage(new Uri("pack://application:,,,/Images/loading_data.png", UriKind.Absolute));
                }));  
            }
        }
        #endregion

        private void titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void closeButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void minimizeButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnOpacityEvent_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 0.75;
        }

        private void btnOpacityEvent_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 1.0;
        }

        private void footerSelectionLeftBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            footerSelectionLeftHighlighted.Visibility = System.Windows.Visibility.Visible;
            footerSelectionRightHighlighted.Visibility = System.Windows.Visibility.Hidden;
            footerSelectionLeftFont.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF2D7B93");
            footerSelectionRightFont.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");

            ajou_tab_panel_total.Visibility = System.Windows.Visibility.Visible;
            ajou_tab_panel_custom.Visibility = System.Windows.Visibility.Hidden;
        }

        private void footerSelectionRightBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            footerSelectionRightHighlighted.Visibility = System.Windows.Visibility.Visible;
            footerSelectionLeftHighlighted.Visibility = System.Windows.Visibility.Hidden;
            footerSelectionLeftFont.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");
            footerSelectionRightFont.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF2D7B93");

            ajou_tab_panel_total.Visibility = System.Windows.Visibility.Hidden;
            ajou_tab_panel_custom.Visibility = System.Windows.Visibility.Visible;
        }

        private void settings_btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ajou_tab.Visibility = System.Windows.Visibility.Hidden;
            eclass_tab.Visibility = System.Windows.Visibility.Hidden;
            calendar_tab.Visibility = System.Windows.Visibility.Hidden;
            settings_tab.Visibility = System.Windows.Visibility.Visible;

            ajou_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            ajou_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            eclass_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            eclass_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            calendar_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            calendar_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            settings_btn_disabled.Visibility = System.Windows.Visibility.Hidden;
            settings_btn_enabled.Visibility = System.Windows.Visibility.Visible;

            second_row_calendar_tab.Visibility = System.Windows.Visibility.Visible;
            second_row_normal_tab.Visibility = System.Windows.Visibility.Hidden;

            row3_fstLabel.Visibility = System.Windows.Visibility.Hidden;
            row3_secLabel.Visibility = System.Windows.Visibility.Hidden;

            end_row_disabled.Visibility = System.Windows.Visibility.Visible;
            end_row_enabled.Visibility = System.Windows.Visibility.Hidden;

            fifth_row.Visibility = System.Windows.Visibility.Hidden;
        }

        private void calendar_btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ajou_tab.Visibility = System.Windows.Visibility.Hidden;
            eclass_tab.Visibility = System.Windows.Visibility.Hidden;
            calendar_tab.Visibility = System.Windows.Visibility.Visible;
            settings_tab.Visibility = System.Windows.Visibility.Hidden;

            ajou_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            ajou_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            eclass_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            eclass_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            calendar_btn_disabled.Visibility = System.Windows.Visibility.Hidden;
            calendar_btn_enabled.Visibility = System.Windows.Visibility.Visible;
            settings_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            settings_btn_enabled.Visibility = System.Windows.Visibility.Hidden;

            second_row_calendar_tab.Visibility = System.Windows.Visibility.Visible;
            second_row_normal_tab.Visibility = System.Windows.Visibility.Hidden;

            row3_fstLabel.Visibility = System.Windows.Visibility.Hidden;
            row3_secLabel.Visibility = System.Windows.Visibility.Hidden;

            end_row_disabled.Visibility = System.Windows.Visibility.Visible;
            end_row_enabled.Visibility = System.Windows.Visibility.Hidden;

            fifth_row.Visibility = System.Windows.Visibility.Hidden;
        }

        private void eclass_btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ajou_tab.Visibility = System.Windows.Visibility.Hidden;
            eclass_tab.Visibility = System.Windows.Visibility.Visible;
            calendar_tab.Visibility = System.Windows.Visibility.Hidden;
            settings_tab.Visibility = System.Windows.Visibility.Hidden;

            ajou_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            ajou_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            eclass_btn_disabled.Visibility = System.Windows.Visibility.Hidden;
            eclass_btn_enabled.Visibility = System.Windows.Visibility.Visible;
            calendar_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            calendar_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            settings_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            settings_btn_enabled.Visibility = System.Windows.Visibility.Hidden;

            second_row_calendar_tab.Visibility = System.Windows.Visibility.Hidden;
            second_row_normal_tab.Visibility = System.Windows.Visibility.Visible;

            row3_fstLabel.Visibility = System.Windows.Visibility.Visible;
            row3_secLabel.Visibility = System.Windows.Visibility.Hidden;

            end_row_disabled.Visibility = System.Windows.Visibility.Hidden;
            end_row_enabled.Visibility = System.Windows.Visibility.Visible;

            fifth_row.Visibility = System.Windows.Visibility.Visible;

            row3_fstLabel.Content = "과목";
            row3_fstLabel.Margin = new Thickness(28, 0, 0, 0);

            footerSelectionLeftFont.Content = "공지사항";
            footerSelectionRightFont.Content = "강의노트";

            footer_tab_ajou.Visibility = System.Windows.Visibility.Hidden;
            footer_tab_eclass.Visibility = System.Windows.Visibility.Visible;

            eclass_tab_view_update();
        }

        private void ajou_btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ajou_tab.Visibility = System.Windows.Visibility.Visible;
            eclass_tab.Visibility = System.Windows.Visibility.Hidden;
            calendar_tab.Visibility = System.Windows.Visibility.Hidden;
            settings_tab.Visibility = System.Windows.Visibility.Hidden;

            ajou_btn_disabled.Visibility = System.Windows.Visibility.Hidden;
            ajou_btn_enabled.Visibility = System.Windows.Visibility.Visible;
            eclass_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            eclass_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            calendar_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            calendar_btn_enabled.Visibility = System.Windows.Visibility.Hidden;
            settings_btn_disabled.Visibility = System.Windows.Visibility.Visible;
            settings_btn_enabled.Visibility = System.Windows.Visibility.Hidden;

            second_row_calendar_tab.Visibility = System.Windows.Visibility.Hidden;
            second_row_normal_tab.Visibility = System.Windows.Visibility.Visible;

            row3_fstLabel.Visibility = System.Windows.Visibility.Visible;
            row3_secLabel.Visibility = System.Windows.Visibility.Visible;

            end_row_disabled.Visibility = System.Windows.Visibility.Hidden;
            end_row_enabled.Visibility = System.Windows.Visibility.Visible;

            fifth_row.Visibility = System.Windows.Visibility.Visible;

            row3_fstLabel.Content = "분류";
            row3_fstLabel.Margin = new Thickness(9, 0, 0, 0);

            footerSelectionLeftFont.Content = "전체";
            footerSelectionRightFont.Content = "정보컴퓨터공학과";

            footer_tab_eclass.Visibility = System.Windows.Visibility.Hidden;
            footer_tab_ajou.Visibility = System.Windows.Visibility.Visible;
        }

        private void footerSelectionLeftBtn_eclass_MouseDown(object sender, MouseButtonEventArgs e)
        {
            footerSelectionLeftHighlighted_eclass.Visibility = System.Windows.Visibility.Visible;
            footerSelectionCenterHighlighted_eclass.Visibility = System.Windows.Visibility.Hidden;
            footerSelectionRightHighlighted_eclass.Visibility = System.Windows.Visibility.Hidden;

            footerSelectionLeftFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF2D7B93");
            footerSelectionCenterFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");
            footerSelectionRightFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");

            eclass_tab_panel_notice.Visibility = System.Windows.Visibility.Visible;
            eclass_tab_panel_lecturenote.Visibility = System.Windows.Visibility.Hidden;
            eclass_tab_panel_assignment.Visibility = System.Windows.Visibility.Hidden;

            row3_secLabel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void footerSelectionCenterBtn_eclass_MouseDown(object sender, MouseButtonEventArgs e)
        {
            footerSelectionLeftHighlighted_eclass.Visibility = System.Windows.Visibility.Hidden;
            footerSelectionCenterHighlighted_eclass.Visibility = System.Windows.Visibility.Visible;
            footerSelectionRightHighlighted_eclass.Visibility = System.Windows.Visibility.Hidden;

            footerSelectionLeftFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");
            footerSelectionCenterFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF2D7B93");
            footerSelectionRightFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");

            eclass_tab_panel_notice.Visibility = System.Windows.Visibility.Hidden;
            eclass_tab_panel_lecturenote.Visibility = System.Windows.Visibility.Visible;
            eclass_tab_panel_assignment.Visibility = System.Windows.Visibility.Hidden;

            row3_secLabel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void footerSelectionRightBtn_eclass_MouseDown(object sender, MouseButtonEventArgs e)
        {
            footerSelectionLeftHighlighted_eclass.Visibility = System.Windows.Visibility.Hidden;
            footerSelectionCenterHighlighted_eclass.Visibility = System.Windows.Visibility.Hidden;
            footerSelectionRightHighlighted_eclass.Visibility = System.Windows.Visibility.Visible;
            footerSelectionLeftFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");
            footerSelectionCenterFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE7F9FF");
            footerSelectionRightFont_eclass.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF2D7B93");

            eclass_tab_panel_notice.Visibility = System.Windows.Visibility.Hidden;
            eclass_tab_panel_lecturenote.Visibility = System.Windows.Visibility.Hidden;
            eclass_tab_panel_assignment.Visibility = System.Windows.Visibility.Visible;

            row3_secLabel.Visibility = System.Windows.Visibility.Visible;
        }

        private void row3_fstLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isEqual = String.Equals(((Label)sender).Content.ToString(), "과목");
            
            if (isEqual)
            {
                if (!LoginModule._getInstance().LoggedOn_eClass)
                    return;

                if (category_selector_panel.Visibility == System.Windows.Visibility.Hidden)
                    category_selector_panel.Visibility = System.Windows.Visibility.Visible;
                else
                    category_selector_panel.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            string id = id_txtBox.Text;
            string pw = pw_pwBox.Password;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((s, f) =>
            {
                if (id != "" && pw != "")
                {
                    LoginModule.ID = id;
                    LoginModule.PW = pw;

                    Dispatcher.Invoke(new Action(() =>
                    {
                        loginBtn.IsEnabled = false;
                        id_txtBox.IsEnabled = false;
                        pw_pwBox.IsEnabled = false;
                    }));
                    
                    LoginModule._getInstance(); // 인스턴스 생성
                }
            });
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, f) =>
            {
                if (LoginModule._getInstance().LoggedOn)
                {
                    usrName.Content = String.Format("{0} {1}님", LoginModule._getInstance().UsrInfoNum, LoginModule._getInstance().UsrName);

                    settings_tab_afterLogin_name.Content = LoginModule._getInstance().UsrName;
                    settings_tab_afterLogin_college.Content = LoginModule._getInstance().UsrCollege;
                    settings_tab_afterLogin_userNum.Content = LoginModule._getInstance().UsrInfoNum;

                    settings_tab_beforeLogon_panel.Visibility = System.Windows.Visibility.Hidden;
                    settings_tab_afterLogon_panel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    LoginModule._getInstance().Logout();
                }

                loginBtn.IsEnabled = true;
                id_txtBox.IsEnabled = true;
                pw_pwBox.IsEnabled = true;

                id_txtBox.Text = "";
                pw_pwBox.Password = "";
            });
            bw.RunWorkerAsync();
        }

        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginModule._getInstance().Logout();

            settings_tab_afterLogon_panel.Visibility = System.Windows.Visibility.Hidden;
            settings_tab_beforeLogon_panel.Visibility = System.Windows.Visibility.Visible;

            id_txtBox.Text = "";
            pw_pwBox.Password = "";

            LoginModule.ID = "";
            LoginModule.PW = "";

            usrName.Content = "아직 로그인하지 않았습니다.";

            id_txtBox.Focus();

            category_selector_grid.Children.Clear();
            eclass_tab_grid_notice.Children.Clear();
            eclass_tab_grid_lecturenote.Children.Clear();
            eclass_tab_grid_assignment.Children.Clear();

            masking_eClass_needsLogin();
        }
    }
}
