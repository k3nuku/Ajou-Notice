/*
 *                  AJOU NOTICE - 1.0 - Preliminary Version
 * 
 * Module Name      : AIMS2 Portal Login Module (Code/Modules/LoginModule.cs)
 * Author           : k3nuku
 * Created at       : 2014-09-07
 * Last Modified    : 2014-09-20
 * 
 * Current Status   : Develop-Completed [200]
 * 
 */

using System;
using System.Text;
using Ajou_Notice.Code.Common;

namespace Ajou_Notice.Code.Modules
{
    class LoginModule
    {
        private static LoginModule _instance = null;

        private const string eClassURL = "https://eclass.ajou.ac.kr";
        private const string loginURL = "https://portal.ajou.ac.kr/portal/j_aims_security_check.action";
        private const string loginPreURL = "http://portal.ajou.ac.kr/public/portal/portlet/UserLoginPre.jsp";
        private string eClassLoginURL = String.Format("{0}/eclass/x/login_post_proc.jsp", eClassURL);
        private string eClassLoginURL2 = String.Format("{0}/eclass/eclass/SSO_Login.action", eClassURL);
        private string eClassLoginURL3 = String.Format("{0}/eclass/total_main.jsp", eClassURL);
        private string eClassFindMainStdURL = String.Format("{0}/eclass/findMainStud.action", eClassURL);
        private string eClassFindMainLectureURL = String.Format("");
        private const string langCode = "ko_KR";
        private const string eClassLoginTask = "SSO_LOGIN";

        private object cookieContainer_User = null;

        private static string id = "";
        private static string pw = "";
        private static bool initialized = false;

        private bool loggedOn = false;
        private bool loggedOn_eClass = false;
        private string usrName = "";
        private string usrInfoNum = "";
        private string usrCollege = "";
        private Array usrSubject;

        /// <summary>
        /// 로그아웃 합니다.
        /// </summary>
        public void Logout()
        {
            initialized = false;
            id = "";
            pw = "";
            _instance = null;
        }

        /// <summary>
        /// 현재 유저의 수강과목을 가져옵니다.
        /// </summary>
        public Array UsrSubject
        {
            get { return this.usrSubject; }
        }

        /// <summary>
        /// 현재 로그인 된 유저의 CookieContainer를 가져옵니다.
        /// </summary>
        internal object CookieContainer_user
        {
            get { return this.cookieContainer_User; }
        }

        /// <summary>
        /// LoginModule 클래스의 단일 인스턴스를 불러옵니다.
        /// </summary>
        /// <returns>LoginModule 인스턴스</returns>
        public static LoginModule _getInstance()
        {
            if (_instance == null)
            {
                _instance = new LoginModule(); // 생성
                _instance.InitializeClass();
            }
            else
                if (!initialized)
                    _instance.InitializeClass(); // 초기화
        
            return _instance;
        }

        /// <summary>
        /// 클래스 초기 실행 시 초기화 과정입니다.
        /// </summary>
        private void InitializeClass()
        {
            LoginProcess(id, pw);

            if (this.LoggedOn)
            {
                this.usrName = GetUserInfo("name");
                this.usrInfoNum = GetUserInfo("hakbun");

                do_eClassLogin(); // 이클래스 SSO 로그인 시 자동으로 학과를 가져옴

                if (this.LoggedOn_eClass)
                    usrSubject = ParseModule.Get_subjectInfo(CookieContainer_user); // 수강 중인 과목 데이터를 가져옴

                initialized = true;
            }
            else
                initialized = false;
        }

        /// <summary>
        /// 아이디를 설정합니다.
        /// </summary>
        public static string ID
        {
            set { id = value; }
        }

        /// <summary>
        /// 비밀번호를 설정합니다.
        /// </summary>
        public static string PW
        {
            set { pw = value; }
        }

        /// <summary>
        /// 해당 인스턴스의 로그인 성공 여부입니다.
        /// </summary>
        public bool LoggedOn
        {
            get { return this.loggedOn; }
        }

        /// <summary>
        /// 해당 인스턴스의 이클래스 로그인 성공 여부입니다.
        /// </summary>
        public bool LoggedOn_eClass
        {
            get { return this.loggedOn_eClass; }
        }

        /// <summary>
        /// 해당 인스턴스에서 로그인 된 유저 이름입니다.
        /// </summary>
        public string UsrName
        {
            get { return this.usrName; }
        }

        /// <summary>
        /// 해당 인스턴스에서 로그인 된 유저의 학번입니다.
        /// </summary>
        public string UsrInfoNum
        {
            get { return this.usrInfoNum; }
        }

        /// <summary>
        /// 해당 인스턴스에서 로그인 된 유저의 학과입니다.
        /// </summary>
        public string UsrCollege
        {
            get { return this.usrCollege; }
        }

        /// <summary>
        /// 해당 인스턴스로 eClass에 로그인을 시도합니다.
        /// </summary>
        /// <returns>로그인 성공 여부</returns>
        private bool do_eClassLogin()
        {
            WebCore.SiteAccess(String.Format("{0}?userno={1}", eClassLoginURL, this.UsrInfoNum), "GET", (System.Net.CookieContainer)cookieContainer_User);
            WebCore.SiteAccess(String.Format("{0}?task={1}", eClassLoginURL2, langCode), "GET", (System.Net.CookieContainer)cookieContainer_User);
            WebCore.SiteAccess(String.Format("{0}?langCode={1}", eClassLoginURL3, langCode), "GET", (System.Net.CookieContainer)cookieContainer_User);
            string rawHTMLString = WebCore.SiteAccess(eClassFindMainStdURL, "GET", (System.Net.CookieContainer)cookieContainer_User)[0] as string;

            try
            {
                this.usrCollege = ParseModule.ParseCollegeInfo(rawHTMLString);
                loggedOn_eClass = true;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 현재 인스턴스로 로그인 된 유저의 정보를 가져옵니다.
        /// </summary>
        /// <param name="type">가져올 정보</param>
        /// <returns>정보 as string</returns>
        private string GetUserInfo(string type)
        {
            if (!this.LoggedOn) return null;

            if (type == "name") // 이름 가져오기
            {
                return 
                    ParseModule.ParseUserName(
                    WebCore.SiteAccess("http://portal.ajou.ac.kr/portal/userLoginInfo.action", "GET", (System.Net.CookieContainer)cookieContainer_User)[0]
                    as string);
            }
            else // 학번만 가져오기
            {
                return 
                    ParseModule.ParseUserInfoNum(
                    WebCore.SiteAccess("http://portal.ajou.ac.kr/portal/portal/findPortletByRolePass.action", "GET", (System.Net.CookieContainer)cookieContainer_User)[0]
                    as string);
            }
        }

        /// <summary>
        /// AIMS2 Portal에 로그인을 시도합니다.
        /// </summary>
        /// <param name="id">아이디</param>
        /// <param name="pass">비밀번호</param>
        /// <returns>로그인 성공 여부</returns>
        private bool LoginProcess(string id, string pass)
        {
            object[] prevData = GetPrevData();
            cookieContainer_User = prevData[1];

            string pwd_encrypted = WebCore.GetEncryptedString_b64sha1(pass);
            string loginData_post = String.Format(
                "signed_data=&LOGIN_RNG={0}&isPki={1}&i_password={2}&i_username={3}&i_password_pre={4}",
                WebCore.UrlEncoder(prevData[0] as string), "noPKI", WebCore.UrlEncoder(pwd_encrypted), WebCore.UrlEncoder(id), "");

            object[] loginResultObject = WebCore.SiteAccess(loginURL, "POST", (System.Net.CookieContainer)cookieContainer_User, loginPreURL, loginData_post);

            int cookieCount = WebCore.CookieCounter(loginResultObject[1]);

            if (cookieCount > 4)
            {
                loggedOn = true;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 로그인 사전 데이터를 가져옵니다.
        /// </summary>
        /// <returns>Object Array[2] { LOGIN_RNG string, Cookies in CookieContainer }</returns>
        private object[] GetPrevData()
        {
            object[] rawPrevData = WebCore.SiteAccess(loginPreURL, "GET", null);
            object cookieCont = rawPrevData[1];
            string loginRNG = (rawPrevData[0] as string).Split(new string[1] { "LOGIN_RNG" }, StringSplitOptions.None)[2].Split(new string[1] { "value" }, StringSplitOptions.None)[1].Substring(2).Substring(0, 24);

            return new object[2] { loginRNG, cookieCont };
        }
    }
}
