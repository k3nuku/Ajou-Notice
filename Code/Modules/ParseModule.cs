/*
 *                  AJOU NOTICE - 1.0 - Preliminary Version
 * 
 * Module Name      : Data Parsing Module (Code/Modules/ParseModule.cs)
 * Author           : k3nuku
 * Created at       : 2014-09-07
 * Last Modified    : 2014-09-21
 * 
 * Current Status   : On-Developing [500]
 * 
 */

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Ajou_Notice.Code.Common;

namespace Ajou_Notice.Code.Modules
{
    class ParseModule
    {
        private static string commonNoticeURL = "http://www.ajou.ac.kr/kr/ajou/notice.jsp";
        private static int board_code_total = 33;
        private static int board_code_ice = 297;

        /// <summary>
        /// 학사 일정을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        private static object[] ParseHaksaSchedule()
        {

            return null;
        }

        public static object[] getHolidayList(int year, int month)
        {
            List<string[]> holidayList = new List<string[]>();

            string downloadtemp = new System.Net.WebClient().DownloadString(String.Format("http://api.sokdak.kr/lunartosolar/?y={0}&m={1}", year.ToString(), month.ToString())); // 이번달 데이터 수신
            downloadtemp = downloadtemp.Replace("&nbsp;", "").Replace("<br>", "").Replace("\\", "");

            string[] temp = downloadtemp.Split('}');
            holidayList.Clear();

            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = temp[i].Replace("[", "").Replace(",{", "").Replace("]", "").Replace("\"", "").Replace(" : ", "").Replace("{", "");
                temp[i] = temp[i].Replace("solar_date", "").Replace("lunar_date", "").Replace("kyganjee", "").Replace("kmganjee", "").Replace("kdganjee", "").Replace("is_leap_year", "").Replace("kterms", "").Replace("keventday", "").Replace("ddi", "").Replace("solplan", "").Replace("lunplan", "").Replace("is_holiday", "");
                holidayList.Add(temp[i].Split(','));
            }

            holidayList.RemoveAt(temp.Length - 1);

            return holidayList.ToArray();
        }

        private static string GetInnerText(HtmlNode hd, int AttributeIndex=-1)
        {
            if(AttributeIndex == -1)
            {
                try
                {
                    return hd.InnerText;
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                try
                {
                    return hd.Attributes[AttributeIndex].Value;
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 해당 페이지의 공지를 가져옵니다. (10개 단위)
        /// </summary>
        /// <param name="countNum"></param>
        /// <returns></returns>
        private static Array GetTotalNotice(int boardNum, int countNum)
        {
            object[] res = WebCore.SiteAccess(String.Format("{0}?mode=list&board_no={1}&pager.offset={2}", commonNoticeURL, boardNum, countNum));

            string resString = res[0] as string;

            HtmlDocument htmlDoc;
            HtmlNodeCollection tr;
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(resString);
            tr = htmlDoc.DocumentNode.SelectNodes("//table[@class=\"list_table\"]//tbody//tr");

            List<string[]> noticeList = new List<string[]>();

            foreach (HtmlNode inner in tr)
            {
                string[] list = new string[10];

                string cleanrawStr = inner.InnerHtml.Replace(@"\""", @"""");
                HtmlDocument innerDoc = new HtmlDocument();
                innerDoc.LoadHtml(cleanrawStr);

                list[0] = GetInnerText(innerDoc.DocumentNode.SelectNodes("//td[@class=\"td al_center\"]")[0]); // 공지 번호
                list[1] = HtmlEntity.DeEntitize(innerDoc.DocumentNode.SelectSingleNode("//td[@class=\"td title_comm\"]//a").InnerHtml).Trim(); // 공지 이름
                list[2] = GetInnerText(innerDoc.DocumentNode.SelectSingleNode("//td[@class=\"td al_center\"]//a"), 0); // 첨부파일 주소

                try
                {
                    list[3] = GetInnerText(innerDoc.DocumentNode.SelectNodes("//td[@class=\"td al_center\"]")[2]); // 카테고리
                    list[4] = GetInnerText(innerDoc.DocumentNode.SelectNodes("//td[@class=\"td al_center\"]")[3]);
                    list[5] = GetInnerText(innerDoc.DocumentNode.SelectNodes("//td[@class=\"td al_center\"]")[4]); // 발행처
                    list[6] = GetInnerText(innerDoc.DocumentNode.SelectSingleNode("//td[@class=\"td title_comm\"]//a"), 1).Replace("&amp;", "&"); // 공지 주소
                }
                catch
                {
                    for (int i = 3; i < 7; i++)
                    {
                        list[i] = "";
                    }
                }

                string noticeHrefFullUrl = string.Format("{0}{1}", commonNoticeURL, list[6]);

                object[] resDate = WebCore.SiteAccess(noticeHrefFullUrl);
                string resDateQueryResult = resDate[0] as string;

                HtmlDocument inDoc = new HtmlDocument();
                inDoc.LoadHtml(resDateQueryResult);

                try
                {
                    list[7] = inDoc.DocumentNode.SelectNodes("//table[@class=\"view_table total-notice\"]//tr")[2].SelectNodes("//td")[4].InnerHtml; // 공지생성일
                }
                catch
                {
                    list[7] = "2015-03-01";
                }

                HtmlNode hnOde = inDoc.DocumentNode.SelectSingleNode("//div[@class=\"view_wrap\"]");
                HtmlNodeCollection hnoDe;

                try
                {
                    hnoDe = hnOde.SelectNodes("//div[contains(@id, 'article_text')]//img");
                }
                catch
                {
                    hnoDe = null;
                }

                list[9] = "";

                if(hnoDe != null) // 노드 article_text내에 img태그가 있을 경우
                {
                    foreach (HtmlNode hn in hnoDe)
                    {
                        try
                        {
                            list[9] += hnoDe[0].Attributes[1].Value + "|";//href 태그 가져옴
                        }
                        catch
                        {
                            list[9] += hnoDe[0].Attributes[0].Value + "|";
                        }
                    }
                }

                string document = "";
                try
                {
                    document = hnOde.SelectSingleNode("//div[contains(@id, 'article_text')]").InnerText;
                    list[8] = HtmlEntity.DeEntitize(document).Trim();
                }
                catch
                {

                }

                noticeList.Add(list);
            }

            return noticeList.ToArray();
        }

        /// <summary>
        /// 공지를 날짜별로 파싱합니다.
        /// </summary>
        /// <param name="wholeNoticeList">파싱되지 않은 공지</param>
        /// <returns>object Array { 날짜, 공지 }</returns>
        private static object[] ParseNotice_eachDay(Array wholeNoticeList)
        {
            string memory = "";
            
            try
            {
                memory = ((string[])wholeNoticeList.GetValue(0))[7];
            }
            catch
            {
                memory = ((string[])wholeNoticeList.GetValue(0))[4];

                if(memory.Length < 8)
                    memory = ((string[])wholeNoticeList.GetValue(0))[2];
            }

            List<string> dtStringArr = new List<string>();
            dtStringArr.Add(memory);

            foreach(string[] noticeArr in wholeNoticeList)
            {
                string createdAt = "";

                try
                {
                    createdAt = noticeArr[7];
                }
                catch
                {
                    createdAt = noticeArr[4];

                    if (createdAt.Length < 8)
                        createdAt = noticeArr[2];
                }

                if (memory != createdAt)
                {
                    dtStringArr.Add(createdAt);
                    memory = createdAt;
                }
            }

            string[] dtStringArr1 = (string[])dtStringArr.ToArray();
            List<string[]>[] listStringArr = new List<string[]>[dtStringArr1.Length];

            foreach (string[] noticeArr in wholeNoticeList)
            {
                string createdAt = "";

                try
                {
                    createdAt = noticeArr[7];
                }
                catch
                {
                    createdAt = noticeArr[4];

                    if (createdAt.Length < 8)
                        createdAt = noticeArr[2];
                }
                
                for (int i = 0; i < dtStringArr1.Length; i++)
                {
                    if (dtStringArr1[i] == createdAt)
                    {
                        if(listStringArr[i] == null)
                            listStringArr[i] = new List<string[]>();
                        
                        listStringArr[i].Add(noticeArr);
                    }
                }
            }

            ArrayList al = new ArrayList();
            
            foreach(List<string[]> arList in listStringArr)
            {
                al.Add(arList.ToArray());
            }

            return new object[2] { dtStringArr1, al.ToArray() };
        }

        /// <summary>
        /// Temporary Functions for Display Notice
        /// </summary>
        /// <param name="arg0"></param>
        /// <returns></returns>
        public static object[] Get_totalNotice(string arg0, string arg1 = null)
        {
            ArrayList al = new ArrayList();

            if(arg0 == "total")
            {
                Array arr;
                Array arr1;

                if (arg1 != null)
                {
                    int page = Int16.Parse(arg1);

                    arr = GetTotalNotice(board_code_total, page);
                    arr1 = GetTotalNotice(board_code_total, page + 10);
                }
                else
                {
                    arr = GetTotalNotice(board_code_total, 0);
                    arr1 = GetTotalNotice(board_code_total, 10);
                }

                foreach (string[] strAr in arr)
                {
                    al.Add(strAr);
                }

                foreach (string[] strAr in arr1)
                {
                    al.Add(strAr);
                }
            }
            else
            {
                Array arr;
                Array arr1;

                if (arg1 != null)
                {
                    int page = Int16.Parse(arg1);

                    arr = GetTotalNotice(board_code_ice, page);
                    arr1 = GetTotalNotice(board_code_ice, page + 10);
                }
                else
                {
                    arr = GetTotalNotice(board_code_ice, 0);
                    arr1 = GetTotalNotice(board_code_ice, 10);
                }

                foreach (string[] strAr in arr)
                {
                    al.Add(strAr);
                }

                foreach (string[] strAr in arr1)
                {
                    al.Add(strAr);
                }
            }
            
            return ParseNotice_eachDay(al.ToArray());  
        }

        /// <summary>
        /// 과목 정보를 가져옵니다.
        /// </summary>
        /// <returns>과목 정보가 담긴 Array</returns>
        public static object[] Get_subjectInfo(object CookieContainer_user)
        {
            string resXMLString = WebCore.SiteAccess("https://eclass.ajou.ac.kr/eclass/eclass/findMain.action?taskId=F_MY_LECT&clubStat=%27S02001%27", "POST", (System.Net.CookieContainer)CookieContainer_user)[0] as string;
            
            int countRecords = 0;

            #region Record 개수 Count
            using (XmlReader xmlr = XmlReader.Create(new StringReader(resXMLString)))
            {
                while (true)
                {
                    try
                    {
                        xmlr.ReadToFollowing("dispOrd");
                        xmlr.ReadElementContentAsString();
                        countRecords++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            #endregion

            List<string>[] subjectInfo = new List<string>[countRecords];

            using (XmlReader xmlRdr = XmlReader.Create(new StringReader(resXMLString)))
            {
                
                for (int i = 0; i < countRecords; i++)
                {
                    subjectInfo[i] = new List<string>();

                    xmlRdr.ReadToFollowing("clubId");
                    subjectInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("clubNm");
                    subjectInfo[i].Add(xmlRdr.ReadElementContentAsString().Split(' ')[0]);

                    xmlRdr.ReadToFollowing("haksuNo");
                    subjectInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("staffNm");
                    subjectInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("roomList");
                    subjectInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("timeList");
                    subjectInfo[i].Add(xmlRdr.ReadElementContentAsString());
                }
            }

            ArrayList al = new ArrayList();

            foreach(List<string> sjInfo_piece in subjectInfo)
            {
                al.Add(sjInfo_piece.ToArray());
            }

            return al.ToArray();
        }

        /// <summary>
        /// 이클래스 해당 과목의 클래스공지사항 목록을 가져옵니다.
        /// </summary>
        /// <param name="clubId">clubID</param>
        /// <param name="hakbun">학번</param>
        /// <returns>공지 정보가 담긴 Array</returns>
        public static object[] Get_eClassNotice(string clubId, string hakbun)
        {
            string resXMLString = WebCore.SiteAccess(
                "https://eclass.ajou.ac.kr/eclass/board/findBoard.action?taskId=F_NOTE_LIST&clubStat=%27S02001%27&stringLeng=60",
                "POST", (System.Net.CookieContainer)LoginModule._getInstance().CookieContainer_user, "https://eclass.ajou.ac.kr/eclass/eclass/findMainLecture.action",
                String.Format("cmmnCd=&gradCode=E0006003&sysType=ECLS&clubId={0}&menuCode=1005&menuNo=1&menuNm=%ED%81%B4%EB%9E%98%EC%8A%A4%EA%B3%B5%EC%A7%80%EC%82%AC%ED%95%AD&viewType=BOARD&adminYn=N&noteSeq=&regiNum=&regiId=&regiPwd=&nickNm=&answerOrd=0&noteCount=&answerLevel=&stringLeng=70&formName=&flag=&flag2=&clubStat='S02001'&total=&lectAdminYn=&searchFg=&searchWord=&regiDttm1=&regiDttm2=&sysPath=&page=1&pageSize=10&replyPage=1&replyPageSize=10&fileSeq=&sortKey=NOTE_SEQ&sortOrd=DESC&replyNo=&replyIcon=1&replyCont=&folderDt=&titleLength=&contentLength=&writeFg=N&deltFg=N&replyFg=N&evalFg=N&notiFg=N&imgUploadFg=&bigFileUploadFg=N&fileUploadFg=N&fileDownFg=N&fileUploadYn=Y&userId={1}&answerFg=N&answerYn=", clubId, hakbun)
                )[0] as string;

            int countRecords = 0;

            #region Record 개수 Count
            using (XmlReader xmlr = XmlReader.Create(new StringReader(resXMLString)))
            {
                while (true)
                {
                    try
                    {
                        xmlr.ReadToFollowing("lectType");
                        xmlr.ReadElementContentAsString();
                        countRecords++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            #endregion

            if (countRecords == 0)
                return null;

            List<string>[] noticeInfo = new List<string>[countRecords];

            using (XmlReader xmlRdr = XmlReader.Create(new StringReader(resXMLString)))
            {
                for (int i = 0; i < countRecords; i++)
                {
                    noticeInfo[i] = new List<string>();

                    xmlRdr.ReadToFollowing("noteSeq");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("title");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString().Trim());

                    xmlRdr.ReadToFollowing("nickNm");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("fileYn");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("regiDttm");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());
                }
            }

            ArrayList al = new ArrayList();

            foreach (List<string> sjInfo_piece in noticeInfo)
            {
                al.Add(sjInfo_piece.ToArray());
            }

            return ParseNotice_eachDay(al.ToArray());
        }

        /// <summary>
        /// 이클래스 해당 과목의 강의노트 목록을 가져옵니다.
        /// </summary>
        /// <param name="clubId">clubID</param>
        /// <param name="hakbun">학번</param>
        /// <returns>강의노트 정보가 담긴 Array</returns>
        public static object[] Get_eClassLectureNote(string clubId, string hakbun)
        {
            string resXMLString = WebCore.SiteAccess(
                "https://eclass.ajou.ac.kr/eclass/board/findBoard.action?taskId=F_LECT_DATA&clubStat=%27S02001%27&stringLeng=60",
                "POST", (System.Net.CookieContainer)LoginModule._getInstance().CookieContainer_user, "https://eclass.ajou.ac.kr/eclass/eclass/findMainLecture.action",
                String.Format("cmmnCd=&gradCode=E0006003&sysType=ECLS&clubId={0}&menuCode=2002&menuNo=1&menuNm=%EA%B0%95%EC%9D%98%EB%85%B8%ED%8A%B8&viewType=BOARD&adminYn=N&noteSeq=&regiNum=&regiId=&regiPwd=&nickNm=&answerOrd=0&noteCount=&answerLevel=&stringLeng=70&formName=&flag=&flag2=&clubStat='S02001'&total=&lectAdminYn=&searchFg=&searchWord=&regiDttm1=&regiDttm2=&sysPath=&page=1&pageSize=10&replyPage=1&replyPageSize=10&fileSeq=&sortKey=NOTE_SEQ&sortOrd=DESC&replyNo=&replyIcon=1&replyCont=&folderDt=&titleLength=&contentLength=&writeFg=N&deltFg=N&replyFg=Y&evalFg=N&notiFg=N&imgUploadFg=&bigFileUploadFg=N&fileUploadFg=N&fileDownFg=N&fileUploadYn=Y&userId={1}&answerFg=N&answerYn=", clubId, hakbun)
                )[0] as string;

            int countRecords = 0;

            #region Record 개수 Count
            using (XmlReader xmlr = XmlReader.Create(new StringReader(resXMLString)))
            {
                while (true)
                {
                    try
                    {
                        xmlr.ReadToFollowing("noteSeq");
                        string validate_value = xmlr.ReadElementContentAsString();

                        if (validate_value == "")
                            break;

                        countRecords++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            #endregion

            if (countRecords == 0)
                return null;

            List<string>[] noticeInfo = new List<string>[countRecords];

            using (XmlReader xmlRdr = XmlReader.Create(new StringReader(resXMLString)))
            {
                for (int i = 0; i < countRecords; i++)
                {
                    noticeInfo[i] = new List<string>();

                    xmlRdr.ReadToFollowing("noteSeq");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("title");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString().Trim());

                    xmlRdr.ReadToFollowing("nickNm");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("fileYn");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("regiDttm");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());
                }
            }

            ArrayList al = new ArrayList();

            foreach (List<string> sjInfo_piece in noticeInfo)
            {
                al.Add(sjInfo_piece.ToArray());
            }

            return ParseNotice_eachDay(al.ToArray());
        }

        /// <summary>
        /// 이클래스 해당 과목의 과제 목록을 가져옵니다.
        /// </summary>
        /// <param name="clubId">clubID</param>
        /// <param name="hakbun">학번</param>
        /// <returns>과제 정보가 담긴 Array</returns>
        public static object[] Get_eClassAssignment(string clubId, string hakbun)
        {
            WebCore.SiteAccess("https://eclass.ajou.ac.kr/eclass/eclass/findMainLecture.action?taskId=F_LECT_MAIN&sysType=ECLS&clubId=" + clubId, "POST", (System.Net.CookieContainer)LoginModule._getInstance().CookieContainer_user, null, null);

            string resXMLString = WebCore.SiteAccess(
                String.Format("https://eclass.ajou.ac.kr/eclass/eclass/findHomeWork.action?taskId=F_STUD_HOME_WORK&sysType=ECLS&clubId={0}&page=1&pageSize=10&mainPageType=class", clubId),
                "POST", (System.Net.CookieContainer)LoginModule._getInstance().CookieContainer_user, "https://eclass.ajou.ac.kr/eclass/eclass/findMainLecture.action"
                )[0] as string;

            int countRecords = 0;

            #region Record 개수 Count
            using (XmlReader xmlr = XmlReader.Create(new StringReader(resXMLString)))
            {
                while (true)
                {
                    try
                    {
                        xmlr.ReadToFollowing("workSeq");
                        string validate_value = xmlr.ReadElementContentAsString();

                        if (validate_value == "")
                            break;

                        countRecords++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            #endregion

            if (countRecords == 0)
                return null;

            List<string>[] noticeInfo = new List<string>[countRecords];

            using (XmlReader xmlRdr = XmlReader.Create(new StringReader(resXMLString)))
            {
                for (int i = 0; i < countRecords; i++)
                {
                    noticeInfo[i] = new List<string>();

                    xmlRdr.ReadToFollowing("workSeq");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("workTitle");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString().Trim());

                    xmlRdr.ReadToFollowing("workStDt");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("workEdDt");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("workEdTm");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());

                    xmlRdr.ReadToFollowing("workCont");
                    noticeInfo[i].Add(xmlRdr.ReadElementContentAsString());
                }
            }

            ArrayList al = new ArrayList();

            foreach (List<string> sjInfo_piece in noticeInfo)
            {
                al.Add(sjInfo_piece.ToArray());
            }

            return ParseNotice_eachDay(al.ToArray());
        }

        /// <summary>
        /// 해당 단과대학의 이름을 파싱합니다.
        /// </summary>
        /// <param name="rawHTML">raw HTML string</param>
        /// <returns>학과 이름</returns>
        public static string ParseCollegeInfo(string rawHTML)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHTML);
            HtmlNode hnCollege = htmlDoc.DocumentNode.SelectSingleNode("//span[@class=\"font11001\"]");
            return hnCollege.InnerText;
        }

        /// <summary>
        /// 해당 유저의 이름을 파싱합니다.
        /// </summary>
        /// <param name="rawHTML">raw HTML string</param>
        /// <returns>이름</returns>
        public static string ParseUserName(string rawHTML)
        {
            return rawHTML.Split(new string[1] { "<span class=\"style1\">" }, StringSplitOptions.None)[1].Split(new string[1] { "</span>" }, StringSplitOptions.None)[0];
        }

        /// <summary>
        /// 해당 유저의 학번을 파싱합니다. 
        /// </summary>
        /// <param name="rawHTML">raw HTML string</param>
        /// <returns>학번</returns>
        public static string ParseUserInfoNum(string rawHTML)
        {
            Regex rgxIframe = new Regex("<iframe[^>]*src=[\"']?([^>\"']+)[\"']?[^>]*>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            return rgxIframe.Match(rawHTML).Value.Split(new string[1] { "userno=" }, StringSplitOptions.None)[1].Substring(0, 9);
        }
    }
}
