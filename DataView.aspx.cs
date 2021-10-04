using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Configuration;
using System.Net;
using System.Web.UI;

public partial class DataView : Page
{
    protected string m_strPath;
    protected SortedList sMyunCol = new SortedList();
    // Param
    protected string strMediaNum = "";
    protected string strMediaAlpha;
    //protected string strMediaKor;       
    protected string strDate = "";
    protected string strYear = "";
    protected string strMonth = "";
    protected string strDay = "";
    protected string strPan = "";
    protected string strView = "";
    //protected string strMyun = "";      
    //protected string strJibang = "";     
    //protected string strJopan = "";     
    // DB
    protected string m_strConn = ConfigurationManager.AppSettings["strConn"];
    // URL
    protected string m_strFilePath = ConfigurationManager.AppSettings["strFile"];
    protected string m_dapsUrl = ConfigurationManager.AppSettings["strDapsUrl"];

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["Login"] == null || Session["Login"].ToString() == "0")
        {
            Response.Redirect("Login.aspx");
            return;
        }
        else
        {
            // 매체
            if (Request.QueryString["media"] == null || Request.QueryString["media"] == "")
                strMediaNum = "65";
            else
                strMediaNum = Request.QueryString["media"].ToString();

            strMediaAlpha = (strMediaNum == "70") ? "st" : "ss";
            //strMediaKor = Util.GetMediaName(strMediaNum);

            // 게재일
            if (Request.QueryString["date"] == null || Request.QueryString["date"] == "")
            {
                strDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            }
            else
            {
                strDate = Request.QueryString["date"].ToString();

                // '-' 안 붙였으면
                if (strDate.Length == 8)
                    strDate = strDate.Substring(0, 4) + '-' + strDate.Substring(4, 2) + '-' + strDate.Substring(6, 2);
            }

            strYear = strDate.Substring(0, 4);
            strMonth = strDate.Substring(5, 2);
            strDay = strDate.Substring(8, 2);

            // 판
            if (Request.QueryString["pan"] == null || Request.QueryString["pan"] == "")
                strPan = "5";
            else
                strPan = Request.QueryString["pan"].ToString();

            // 뷰 (라디오 버튼)
            if (Request.QueryString["view"] == null || Request.QueryString["view"] == "")
                strView = "0";
            else
                strView = Request.QueryString["view"].ToString();

            MakeMyun();
            BindFileAllList();
        }
    }

    public string fncMakeTable(string sId, string sMyun, string sFile, string layUser, string djUser, string adUser, string djLock, string adLock, string dKpang, string bTime, string sMyunCode, string sJibang, string dtKpan)
    {
        string sFilePath = "";
        string sViewPath = "";
        string sRealPath = "";
        string sLocalPath = "";
        string sLockColor = "";
        string sWorkColor = "";
        string layWorkState = "편집 : ";
        string djWorkState = "대조 : ";
        string adWorkState = "광고 : ";
        string strKpanTime = "강판 : ";
        string sTime_Cmyk = "";
        string strFileInfo = "";

        if (!(sId == Util.JOPANTYPE_AUTO_CODE || sId == Util.JOPANTYPE_MANUAL_CODE))
            strFileInfo = sFile.Substring(0, 1) + sFile.Substring(5, 6) + "00" + sFile.Substring(11, 2) + "00";

        sFilePath = ConfigurationManager.AppSettings["strMThumb"] + strFileInfo + ".jpg";
        m_strPath = string.Format("{0}/{1}/{2}/", "65", strYear, strMonth + strDay);

        if (File.Exists(sFilePath))
        {
            sWorkColor = "Blue";
            sViewPath = ConfigurationManager.AppSettings["strSvcMThumb"] + strFileInfo + ".jpg";
            sRealPath = ConfigurationManager.AppSettings["strSvcMPrev"] + strFileInfo + ".jpg";
        }
        else
        {
            int intKpangValue = Convert.ToInt32(dKpang);
            sWorkColor = (intKpangValue > 0) ? "rgb(25, 25, 112);" : "rgb(109, 109, 109);";
            sViewPath = string.Format("{0}/{1}", m_strFilePath.TrimEnd('/'), m_strPath) + "MT" + sFile + ".jpg";
            sRealPath = string.Format("{0}/{1}", m_strFilePath.TrimEnd('/'), m_strPath) + "MO" + sFile + ".jpg";
        }

        sLocalPath = "P:\\Publish\\CTS\\" + m_strPath.Replace("/", "\\");

        // 자동조판인지 체크
        if (sId == Util.JOPANTYPE_AUTO_CODE)
        {
            layWorkState = Util.JOPANTYPE_AUTO_NAME;
        }
        else if (sId == Util.JOPANTYPE_MANUAL_CODE)
        {
            string conString = ConfigurationManager.AppSettings["strConn"];
            string sql = @"select N_HOSU From [DAPS].[dbo].[CMS_PAPERPLAN] where V_PAPERDATE = '" + strDate + "' and N_PAGE = " + sMyun + " and N_PAN=" + strPan + " and N_JIBANG=" + sJibang;

            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = sql;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        layWorkState += reader[0].ToString();
                    }
                }
            }
        }
        else
        {
            layWorkState += Util.GetUserName(layUser);
        }

        if (djLock != "0")
            sLockColor = "rgb(128, 128, 0);";

        if (sId == Util.JOPANTYPE_AUTO_CODE)
        {
            layWorkState = Util.JOPANTYPE_AUTO_NAME;
        }
        else if (sId == Util.JOPANTYPE_MANUAL_CODE)
        {
            string conString = ConfigurationManager.AppSettings["strConn"];
            string sql = @"select V_USERNAME from [CMSCOM].[dbo].[T_USERINFO] where RTRIM([V_USERID]) = 
(select top 1 V_USERID From [DAPS].[dbo].[CMS_PAPERPLAN] where V_PAPERDATE = '" + strDate + "' and N_PAGE = " + sMyun + " and N_PAN=" + strPan + " and N_JIBANG=" + sJibang + " and V_USERID is not null )";

            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = sql;
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        djWorkState += reader[0].ToString();
                        sLockColor = "rgb(128, 128, 0);";
                    }
                }
            }
        }
        else
        {
            djWorkState += Util.GetUserName(djLock);
        }

        if (adLock == "0")
            adWorkState += Util.GetUserName(adUser);
        else
            adWorkState += Util.GetUserName(adLock);

        // 자동조판일 경우
        if (sId == Util.JOPANTYPE_AUTO_CODE || sId == Util.JOPANTYPE_MANUAL_CODE)
        {
            if (dtKpan != "")
            {
                strKpanTime += get_autocts_kangpandate(sMyun, sJibang); // 자동조판 강판시간 Ex) 2017-03-27 09:39:23.000

                if (strKpanTime != "00:00" && strKpanTime != "0" && strKpanTime != "강판 : ")
                    sWorkColor = "rgb(25, 25, 112);";
                else
                    sWorkColor = "rgb(109, 109, 109);";
            }
        }
        else
        {
            if (dtKpan != "")
                strKpanTime += dtKpan;
        }

        if (bTime != "")
            sTime_Cmyk += "CTP : " + getTimeValue(bTime) + "&nbsp;&nbsp;<br />";

        if (sTime_Cmyk.Length > 0)
            sTime_Cmyk = "[CTP출력시간] <br />" + sTime_Cmyk;

        string sMonitorName = ConfigurationManager.AppSettings["strMPrev"] + strFileInfo + ".log";
        StringBuilder docWrite = new StringBuilder();

        if (sLockColor == "")
            docWrite.Append("<div class='thumb' style='background-color: " + sWorkColor + "'>\n");
        else
            docWrite.Append("<div class='thumb' style='background-color: " + sLockColor + "'>\n");

        docWrite.Append("   <div class='thumb_title'>\n");
        docWrite.Append("       " + sMyun + "면 [" + sFile + "]\n");
        docWrite.Append("   </div>\n");
        docWrite.Append("   <div class='thumb_img'>\n");

        string strKangFileName = Util.GetKangFileName(strDate, strPan, sMyun, sJibang);
        string strKangImg = ConfigurationManager.AppSettings["strKangThum"] + strKangFileName;

        if (strKangFileName.Length > 0)
        {
            bool exist = false;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strKangImg);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    exist = response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception)
            {
            }

            if (exist)
            {
                if (strKangImg.Substring(strKangImg.Length - 3).ToLower() == "jpg")
                {
                    if (sId == Util.JOPANTYPE_AUTO_CODE)
                    {
                        docWrite.Append("           <img src='" + strKangImg + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
                    }
                    else if (sId == Util.JOPANTYPE_MANUAL_CODE)
                    {
                        docWrite.Append("           <img src='" + strKangImg + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
                    }
                    else
                    {
                        docWrite.Append("           <img src='" + strKangImg + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
                    }
                }
                else
                {
                    docWrite.Append("           <img src='" + sViewPath + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "', '" + sId + "', '" + sJibang + "'); />");
                }
            }
            else
            {
                if (sId == Util.JOPANTYPE_AUTO_CODE)
                {
                    sViewPath = m_dapsUrl + "/thumb/" + strDate.Replace("-", "") + "/" + strMediaAlpha + sMyun.PadLeft(2, '0') + "-" + strMonth + strDay + "-" + strPan.PadLeft(2, '0') + Util.GetJibangCode(sJibang, "") + ".jpg";

                    docWrite.Append("           <img src='" + sViewPath + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "', '" + sId + "', '" + sJibang + "'); />");
                }
                else if (sId == Util.JOPANTYPE_MANUAL_CODE)
                {
                    sViewPath = m_dapsUrl + "/thumb/" + strDate.Replace("-", "") + "/" + strMediaAlpha + sMyun.PadLeft(2, '0') + "-" + strMonth + strDay + "-" + strPan.PadLeft(2, '0') + Util.GetJibangCode(sJibang, "") + ".jpg";

                    docWrite.Append("           <img src='" + sViewPath + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
                }
                else
                {
                    docWrite.Append("           <img src='" + sViewPath + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
                }
            }
        }
        else
        {
            if (sId == Util.JOPANTYPE_AUTO_CODE)
            {
                sViewPath = m_dapsUrl + "/thumb/" + strDate.Replace("-", "") + "/" + strMediaAlpha + sMyun.PadLeft(2, '0') + "-" + strMonth + strDay + "-" + strPan.PadLeft(2, '0') + Util.GetJibangCode(sJibang, "") + ".jpg";

                docWrite.Append("           <img src='" + sViewPath + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
            }
            else if (sId == Util.JOPANTYPE_MANUAL_CODE)
            {
                sViewPath = m_dapsUrl + "/thumb/" + strDate.Replace("-", "") + "/" + strMediaAlpha + sMyun.PadLeft(2, '0') + "-" + strMonth + strDay + "-" + strPan.PadLeft(2, '0') + Util.GetJibangCode(sJibang, "") + ".jpg";

                docWrite.Append("           <img src='" + sViewPath + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
            }
            else
            {
                docWrite.Append("           <img src='" + sViewPath + "?random=" + new Random().Next() + "' onclick=ViewPreview('" + sMyun + "','" + sId + "','" + sJibang + "'); />");
            }
        }

        docWrite.Append("   </div>\n");

        if (sId == Util.JOPANTYPE_MANUAL_CODE)
            docWrite.Append("   <div class='thumb_info' style='color: lightgreen'>\n");
        else if (sId == Util.JOPANTYPE_AUTO_CODE)
            docWrite.Append("   <div class='thumb_info' style='color: #b4e0f0'>\n");
        else
            docWrite.Append("   <div class='thumb_info' style='color: #fff'>\n");


        docWrite.Append("       " + layWorkState + "<br />\n");
        docWrite.Append("       " + djWorkState + "<br />\n");
        docWrite.Append("       " + strKpanTime + "\n");
        docWrite.Append("   </div>\n");
        docWrite.Append("</div>\n");

        return docWrite.ToString();
    }

    // 자동조판 강판시간
    public string get_autocts_kangpandate(string page, string jibang)
    {
        string ret = "";
        string strSQL = "SELECT top 1 d_createtime FROM [DAPS].[dbo].[CTS_JOPANINFO_HISTORY] where id_mechae=" + strMediaNum + " and v_paperdate='" + strDate + "' and n_pan='" + strPan + "' and n_page='" + page + "' and n_jibang='" + jibang + "'  and v_job='강판' order by d_createtime desc";
        SqlConnection SqlConn = new SqlConnection(m_strConn);
        SqlCommand SqlCmd = new SqlCommand(strSQL, SqlConn);

        try
        {
            SqlConn.Open();
            SqlDataReader Dr = SqlCmd.ExecuteReader();

            while (Dr.Read())
            {

                ret = Convert.ToDateTime(Dr[0].ToString()).ToString("HH:mm");
            }

            Dr.Close();
        }
        catch
        {

        }
        finally
        {
            SqlConn.Close();
        }

        return ret;
    }

    private string getTimeValue(string date)
    {
        string strRet = "";
        DateTime dt = Convert.ToDateTime(date);

        strRet = string.Format("{0}시 {1}분", dt.Hour.ToString().PadLeft(2, '0'), dt.Minute.ToString().PadLeft(2, '0'));

        return strRet;
    }

    #region 파일 출력
    private void BindFileList(string kpang)
    {
        SqlConnection Conn = new SqlConnection(m_strConn);
        SqlCommand command = new SqlCommand();

        command.Connection = Conn;
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@media", SqlDbType.SmallInt);
        command.Parameters["@media"].Value = int.Parse(strMediaNum);

        command.Parameters.Add("@paper_date", SqlDbType.Char);
        command.Parameters["@paper_date"].Value = strDate.Replace("-", "");

        command.Parameters.Add("@pan", SqlDbType.SmallInt);
        command.Parameters["@pan"].Value = int.Parse(strPan);

        command.Parameters.Add("@kangpan", SqlDbType.Char);
        command.Parameters["@kangpan"].Value = strView;

        command.Parameters.Add("@pageno", SqlDbType.Int);
        command.Parameters["@pageno"].Value = Session["PageNumber"];

        command.CommandText = "sp_GET_MYUNINFO_WEB";

        SqlDataAdapter Adap = new SqlDataAdapter(command);

        DataSet ds = new DataSet();
        Adap.Fill(ds);

        DataList2.DataSource = ds;
        DataList2.DataBind();
    }
    #endregion

    #region 전체 파일 검색
    private void BindFileAllList()
    {
        string strSql = "";

        strSql += "select * from ( ";
        strSql += " SELECT myunid as myun_id, myun as m_myun, jibang as m_jibang, ISNULL(print_count, 0) as dj_kpan_count, ISNULL(PrintTime, '') as PrintTime, ";
        strSql += " dtkpan as dj_kpan_date, dtlink as ad_kpan_date, ISNULL(dj_lock_user,0) as dj_lock_user,ISNULL(ad_lock_user,0) as ad_lock_user, ";
        strSql += " substring(myun_file,3,15) as filename, dj_kpang,      pan,  ISNULL(lay_user,0) as lay_user, ISNULL(dj_user,0) as dj_user, ";
        strSql += " ISNULL(ad_user,0) as ad_user,       myun_code, prtTime_B,            paper_date,        jibang ";
        strSql += " FROM prodarx2005.dbo.V_ONKPANLIST_MONITOR_M WHERE  media=" + strMediaNum + " and paper_date='" + strDate + "' and pan=" + strPan;
        strSql += " union all ";
        strSql += " select (select CASE V_JOPANTYPE WHEN '자동조판' THEN '999999' ELSE '888888' END from daps.dbo.CMS_PAPERPLAN B WHERE B.ID_MECHAE ='" + strMediaNum + @"' and V_PAPERDATE=CMS_PAPERPLAN.V_PAPERDATE AND N_PAN=CMS_PAPERPLAN.N_PAN AND N_PAGE=CMS_PAPERPLAN.N_PAGE AND N_JIBANG=CMS_PAPERPLAN.N_JIBANG) as myun_id, N_PAGE as m_myun, N_JIBANG as m_jibang,            '0' as dj_kpan_count,                        '' as PrintTime, ";
        strSql += " ''    as dj_kpan_date,   '' as ad_kpan_date ,                      0 as      dj_lock_user ,                    0 as  ad_lock_user     ,";
        strSql += "  v_pagename as     filename ,(select top 1 count(*) from daps.dbo.CTS_JOPANINFO_HISTORY where V_PAPERDATE='" + strDate + "' and n_pan=" + strPan + " and N_PAGE=CMS_PAPERPLAN.N_PAGE and N_JIBANG=CMS_PAPERPLAN.N_JIBANG and V_JOB='강판') as dj_kpang,N_PAN as pan ,      0 as   lay_user,               0 as  dj_user, ";
        strSql += " 0          as ad_user ,'9999' as myun_code ,'' as  prtTime_B  , replace(v_paperdate,'-','') as paper_date,  N_JIBANG as   jibang        ";
        strSql += " from   daps.dbo.cms_paperplan  WHERE id_mechae=" + strMediaNum + " and v_paperdate='" + strDate + "' and n_pan=" + strPan + " ) a ";

        if (strView == "1")
        {
            strSql += " where dj_kpang > 0 ";
        }
        else if (strView == "2")
        {
            strSql += " where (dj_kpang = 0 or dj_kpang = null) ";
        }

        strSql += " ORDER BY m_myun asc";

        SqlConnection Conn = new SqlConnection(m_strConn);
        SqlDataAdapter Adap = new SqlDataAdapter(strSql, Conn);
        DataSet ds = new DataSet();

        Adap.Fill(ds, "MYUNINFO_TBL");

        DataList2.DataSource = ds.Tables["MYUNINFO_TBL"];
        DataList2.DataBind();
    }
    #endregion

    #region 전체 면 개수
    protected int GetMyunTotalCount(string kpang)
    {
        SqlConnection Conn = null;

        int nPageCnt = 0;
        try
        {
            Conn = new SqlConnection(m_strConn);
            SqlCommand command = new SqlCommand();

            Conn.Open();

            command.Connection = Conn;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@media", SqlDbType.SmallInt);
            command.Parameters["@media"].Value = int.Parse(strMediaNum);

            command.Parameters.Add("@paper_date", SqlDbType.Char);
            command.Parameters["@paper_date"].Value = strDate.Replace("-", "");

            command.Parameters.Add("@pan", SqlDbType.SmallInt);
            command.Parameters["@pan"].Value = int.Parse(strPan);

            command.Parameters.Add("@kangpan", SqlDbType.Char);
            command.Parameters["@kangpan"].Value = strView;

            command.Parameters.Add("@pageno", SqlDbType.Int);
            command.Parameters["@pageno"].Value = 1;

            command.Parameters.Add("ReturnValue", SqlDbType.Int);
            command.Parameters["ReturnValue"].Direction = ParameterDirection.ReturnValue;


            command.CommandText = "sp_GET_MYUNINFO_TOTALCNT_WEB";

            SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);

            reader.Close();
            int nReturnValue = (int)command.Parameters["ReturnValue"].Value;

            nPageCnt = nReturnValue;

        }
        catch (SqlException)
        {
        }
        finally
        {
            Conn.Close();
        }

        return nPageCnt;
    }
    #endregion

    #region 면명 보기
    public string fncMyunConvert(string code)
    {
        string strMyunName = "";

        if (!(code.Trim().Equals("") || code == null || code == "0"))
        {
            try
            {
                strMyunName = sMyunCol[code].ToString();

                if (strMyunName.Length > 5)
                    strMyunName = strMyunName.Substring(0, 5);
            }
            catch
            {
                strMyunName = "테스트면";
            }
        }

        return strMyunName;
    }
    #endregion

    #region 면명 생성
    private void MakeMyun()
    {
        string strSQL = "SELECT MYUN_CODE, MYUN_NAME FROM MYUNNAME_TBL";
        SqlConnection SqlConn = new SqlConnection(m_strConn);
        SqlCommand SqlCmd = new SqlCommand(strSQL, SqlConn);

        try
        {
            SqlConn.Open();
            SqlDataReader Dr = SqlCmd.ExecuteReader();

            while (Dr.Read())
            {
                sMyunCol.Add(Dr[0].ToString(), Dr[1].ToString());
            }

            Dr.Close();
        }
        catch
        {

        }
        finally
        {
            SqlConn.Close();
        }
    }
    #endregion
}
