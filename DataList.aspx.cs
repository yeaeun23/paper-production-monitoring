using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web.UI;
using System.IO;
using System.Configuration;

public partial class DataList : Page
{
    protected string m_strPath;
    protected string strKangCnt;
    protected string strSongchulTime;
    protected string tmpColor;
    protected DateTime m_LastKangTime = new DateTime();
    protected SortedList sJibangCol = new SortedList();
    // Param
    protected string strMediaNum = "";    // 65, 68, 70
    //protected string strMediaAlpha;       // ss, ss, st
    protected string strMediaKor;         // 서울신문, 외간A3, 타블로이드
    protected string strDate = "";        // 2021-08-13
    protected string strYear = "";        // 2021
    protected string strMonth = "";       // 08
    protected string strDay = "";         // 13
    protected string strPan = "";         // 5, 10.. 
    protected string strView = "";        // 0, 1, 2
    //protected string strMyun = "";        // 1, 2..
    //protected string strJibang = "";      // 1, 2..
    //protected string strJopan = "";       // 888888, 999999
    // DB
    protected string m_strConn = ConfigurationManager.AppSettings["strConn"];

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

            //strMediaAlpha = (strMediaNum == "70") ? "st" : "ss";
            strMediaKor = Util.GetMediaName(strMediaNum);

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

            // 드롭다운 리스트
            SetMediaList(strMediaNum);
            SetPanList();

            // 기타 컨트롤
            SetControlValue();

            GetJibangKor();
            BindDataListAll();
            getPrintTime();
        }
    }

    // 초쇄, 종쇄시각 출력
    private void getPrintTime()
    {
        DataTable dt = Util.ExecuteQuery(new SqlCommand(string.Format("select chotime, jongtime from PRESS where media = '{0}' and pressdate = '{1}' and panno = {2}", strMediaKor, strDate, strPan)), "SELECT");

        if (dt.Rows.Count > 0)
        {
            lblStartPrint.Text = dt.Rows[0]["chotime"].ToString();
            lblEndPrint.Text = dt.Rows[0]["jongtime"].ToString();
        }

        lblStartPrint.Text = (lblStartPrint.Text == "") ? "&nbsp;" : lblStartPrint.Text;
    }

    // 매체 드롭다운 리스트 세팅
    private void SetMediaList(string code)
    {
        SortedList slCol = new SortedList();
        string strArrMaeche = ConfigurationManager.AppSettings["strArrMaeche"];
        string[] arrMaeche = strArrMaeche.Split(',');
        string strMaecheID;
        string strMaecheName;
        int indexofPipe;
        int lengthofMaeche;

        foreach (string rMaeche in arrMaeche)
        {
            indexofPipe = rMaeche.IndexOf("|");
            lengthofMaeche = rMaeche.Length;
            strMaecheID = rMaeche.Substring(0, indexofPipe);
            strMaecheName = rMaeche.Substring(indexofPipe + 1, lengthofMaeche - indexofPipe - 1);
            slCol.Add(strMaecheID, strMaecheName);
        }

        media.DataSource = slCol;
        media.DataValueField = "Key";
        media.DataTextField = "Value";
        media.DataBind();

        media.SelectedValue = strMediaNum;
    }

    // 판 드롭다운 리스트 세팅
    private void SetPanList()
    {
        string strArrPan = ConfigurationManager.AppSettings["strArrPan"];
        string[] arrPan = strArrPan.Split(',');
        SortedList slCol = new SortedList();

        foreach (string rPan in arrPan)
            slCol.Add(Convert.ToInt32(rPan), rPan);

        pan.DataSource = slCol;
        pan.DataValueField = "Key";
        pan.DataTextField = "Value";
        pan.DataBind();

        pan.SelectedValue = strPan;
    }

    // 기타 컨트롤 값 세팅
    private void SetControlValue()
    {
        date.Text = strDate;

        lblUpdateTime.Text = DateTime.Now.ToString("HH:mm:ss");
        lblMonth.Text = strMonth;
        lblDay.Text = strDay;
        lblPan.Text = strPan;
        lblMaeche.Text = strMediaKor;

        // 강판면 수
        SetKangpanCount();
    }

    #region 기본 시간 보여주기
    public string getDateToString(string dt, string myun_id)
    {
        if (myun_id == Util.JOPANTYPE_AUTO_CODE)
        {
            return "자동조판";
        }
        else
        {
            if (dt.Length > 0)
                return Convert.ToDateTime(dt).ToString("HH:mm");
            else
                return "";
        }
    }

    public string getDateToString1(string dt)
    {
        string strDate = "";
        DateTime dtRow;

        if (dt.Length > 0)
        {
            dtRow = Convert.ToDateTime(dt);
            strDate = dtRow.ToString("HH:mm");
        }
        else
        {
            strDate = "";
            dtRow = DateTime.Now;
        }

        if (dtRow == m_LastKangTime)
            strDate = "<font color='yellow'><b>" + strDate + "</b></font>";

        return strDate;
    }

    public string getDateToString2(string dt, string sKPan)
    {
        string strDate = "";
        DateTime dtKPan; // 강판시간   

        if (dt.Trim() != "")
            strDate = dt.Substring(8, 2) + ":" + dt.Substring(10, 2);

        if (sKPan.Length > 0)
            dtKPan = Convert.ToDateTime(sKPan);
        else
            dtKPan = DateTime.Now;

        if (dtKPan == m_LastKangTime)
            strDate = "<font color='yellow'><b>" + strDate + "</b></font>";

        return strDate;
    }
    #endregion

    public string getDateToString3(string dt, string sKPan, string myun_id, string myun, string jibang)
    {
        string strDate = "";
        DateTime dtKPan; // 강판시간 

        if (myun_id == Util.JOPANTYPE_AUTO_CODE || myun_id == Util.JOPANTYPE_MANUAL_CODE)
        {
            //자동조판 ctp 출력 시간을 구한다.
            dt = get_autocts_ctptime(myun, jibang);

            if (dt.Length > 0)
                strDate = Convert.ToDateTime(dt).ToString("HH:mm");
            else
                strDate = "";

            if (sKPan.Length > 0)
                dtKPan = Convert.ToDateTime(sKPan);
            else
                dtKPan = DateTime.Now;
        }
        else
        {
            if (dt.Length > 0)
                strDate = Convert.ToDateTime(dt).ToString("HH:mm");
            else
                strDate = "";

            if (sKPan.Length > 0)
                dtKPan = Convert.ToDateTime(sKPan);
            else
                dtKPan = DateTime.Now;
        }

        if (dtKPan == m_LastKangTime)
            strDate = "<font color='yellow'><b>" + strDate + "</b></font>";

        return strDate;
    }

    #region 지방코드 생성
    private void GetJibangKor()
    {
        string strSQL = "SELECT CODE, REMARK FROM JIBANGCODE_TBL";
        SqlConnection SqlConn = new SqlConnection(m_strConn);
        SqlCommand SqlCmd = new SqlCommand(strSQL, SqlConn);

        try
        {
            SqlConn.Open();
            SqlDataReader Dr = SqlCmd.ExecuteReader();

            while (Dr.Read())
            {
                sJibangCol.Add(Dr[0].ToString(), Dr[1].ToString());
            }

            Dr.Close();
        }
        catch (Exception)
        {
        }
        finally
        {
            SqlConn.Close();
        }
    }
    #endregion

    // 강판면 수 구하기
    private void SetKangpanCount()
    {
        int intMyunCount = 0;
        int intKpangCount = 0;
        int intNoKpangCount = 0;
        int intMyunCountIndd = 0;
        int intKpangCountIndd = 0;

        string strSql = string.Format("SELECT dj_kpang FROM MYUNINFO_TBL WHERE media={0} and paper_date='{1}' and m_pan={2}", strMediaNum, strDate.Replace("-", ""), strPan);

        SqlConnection Conn = new SqlConnection(m_strConn);
        SqlCommand Cmd = new SqlCommand(strSql, Conn);

        try
        {
            Conn.Open();

            SqlDataReader Dr = Cmd.ExecuteReader();

            while (Dr.Read())
            {
                intMyunCount = intMyunCount + 1;

                if (Convert.ToInt32(Dr["dj_kpang"].ToString()) == 0)
                    intNoKpangCount = intNoKpangCount + 1;
            }

            Dr.Close();

            strSql = string.Format(@"DECLARE @DATE VARCHAR(10), @PAN INT
SET @DATE = '{0}'
SET @PAN = {1}
select count(*) from daps.dbo.CMS_PAPERPLAN where N_FLAG !=0 and V_PAPERDATE=@DATE and n_pan = @PAN "
                , strDate, strPan);
            Cmd = new SqlCommand(strSql, Conn);
            Dr = Cmd.ExecuteReader();
            while (Dr.Read())
            {
                intMyunCountIndd += Convert.ToInt32(Dr[0].ToString());
            }
            Dr.Close();

            strSql = string.Format(@"DECLARE @DATE VARCHAR(10), @PAN INT
SET @DATE = '{0}'
SET @PAN = {1}
select N_PAGE from daps.dbo.CTS_JOPANINFO_HISTORY where V_PAPERDATE=@DATE and V_JOB ='강판' and n_pan = @PAN
 and n_page in (select n_page From daps.dbo.CMS_PAPERPLAN where V_PAPERDATE=@DATE and n_pan = @PAN and ID_MECHAE='" + strMediaNum + @"') group by N_PAGE, N_JIBANG"
            , strDate, strPan);
            Cmd = new SqlCommand(strSql, Conn);
            Dr = Cmd.ExecuteReader();

            while (Dr.Read())
                intKpangCountIndd = intKpangCountIndd + 1;

            Dr.Close();

            view.Items[0].Text = " 전체면 (" + (intMyunCount + intMyunCountIndd) + ")";

            lblPubNoKpang.Text = (intNoKpangCount + intMyunCountIndd - intKpangCountIndd).ToString();
            view.Items[2].Text = " 미강판면 (" + lblPubNoKpang.Text + ")";

            intKpangCount = intMyunCount - intNoKpangCount + intKpangCountIndd;
            lblPubKpang.Text = intKpangCount.ToString();
            view.Items[1].Text = " 강판면 (" + lblPubKpang.Text + ")";
        }
        catch (Exception)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('DB 연결에 실패했습니다. IT개발부로 문의해 주세요.');</script>");
        }
        finally
        {
            Conn.Close();
        }
    }

    // 왼쪽 뷰 리스트 출력
    private void BindDataListAll()
    {
        try
        {
            string strSql = "";
            strSql = "select * from ( ";
            strSql = strSql + " select (select CASE V_JOPANTYPE WHEN '자동조판' THEN '999999' ELSE '888888' END from daps.dbo.CMS_PAPERPLAN B WHERE B.ID_MECHAE ='" + strMediaNum + "' and V_PAPERDATE=CMS_PAPERPLAN.V_PAPERDATE AND N_PAN=CMS_PAPERPLAN.N_PAN AND N_PAGE=CMS_PAPERPLAN.N_PAGE AND N_JIBANG=CMS_PAPERPLAN.N_JIBANG) as myun_id";
            strSql = strSql + "  ,N_PAGE as m_myun, N_JIBANG as m_jibang,            '0' as dj_kpan_count,                        '' as PrintTime, ";
            strSql = strSql + @" ''    as dj_kpan_date,  (SELECT [cre_date] FROM [Prodarx2005].[dbo].[IMG_REF_TBL] where id = (SELECT top 1 ref_id FROM [Prodarx2005].[dbo].[ADIMGINFO_TBL] where img_type=7 and input_type=19 and adimg_id=ref_id and paper_date = '" + strDate.Replace("-", "") + "' and myun = daps.dbo.cms_paperplan.n_page and jibang=n_jibang and pan = " + strPan + ")) as ad_kpan_date ,                      0 as      dj_lock_user ,                    0 as  ad_lock_user     ,";
            strSql = strSql + "  'A99999999999999' as     filename ,(select top 1 count(*) from daps.dbo.CTS_JOPANINFO_HISTORY where V_PAPERDATE='" + strDate + "' and n_pan=" + strPan + " and N_PAGE=CMS_PAPERPLAN.N_PAGE and N_JIBANG=CMS_PAPERPLAN.N_JIBANG and V_JOB='강판') as dj_kpang ,N_PAN as pan ,      0 as   lay_user,               0 as  dj_user, ";
            strSql = strSql + " 0          as ad_user ,'9999' as myun_code ,'' as  prtTime_B  , replace(v_paperdate,'-','') as paper_date,  N_JIBANG as   jibang        ";
            strSql = strSql + " from   daps.dbo.cms_paperplan  WHERE id_mechae=" + strMediaNum + " and v_paperdate='" + strDate + "' and n_pan=" + strPan + " ) a ";

            if (strView == "1")
                strSql = strSql + " where dj_kpang > 0 ";
            else if (strView == "2")
                strSql = strSql + " where (dj_kpang = 0 or dj_kpang = null) ";

            strSql = strSql + " ORDER BY m_myun asc";

            SqlConnection Conn = new SqlConnection(m_strConn);
            SqlDataAdapter Adap = new SqlDataAdapter(strSql, Conn);

            DataSet ds = new DataSet();
            Adap.Fill(ds, "MYUNINFO_TBL");

            for (int i = 0; i < ds.Tables["MYUNINFO_TBL"].Rows.Count; i++)
            {
                getKangpanInfo(ds.Tables["MYUNINFO_TBL"].Rows[i]["paper_date"].ToString(),
                    ds.Tables["MYUNINFO_TBL"].Rows[i]["pan"].ToString().PadLeft(2, '0'),
                    ds.Tables["MYUNINFO_TBL"].Rows[i]["m_myun"].ToString(),
                    Util.GetJibangCode(ds.Tables["MYUNINFO_TBL"].Rows[i]["jibang"].ToString(), "kor")
                );

                if (strKangCnt == "")
                    strKangCnt = "0";

                strSongchulTime = strSongchulTime.Replace("-", "");
                strSongchulTime = strSongchulTime.Replace(" ", "");
                strSongchulTime = strSongchulTime.Replace(":", "");

                //+---------------------------------------------------+//
                //자동조판 필드를 재정의한다.
                //+---------------------------------------------------+//
                if (ds.Tables["MYUNINFO_TBL"].Rows[i]["myun_id"].ToString() == Util.JOPANTYPE_AUTO_CODE || ds.Tables["MYUNINFO_TBL"].Rows[i]["myun_id"].ToString() == Util.JOPANTYPE_MANUAL_CODE)
                {

                    string strTime = "";
                    strTime = get_autocts_kangpandate(ds.Tables["MYUNINFO_TBL"].Rows[i]["m_myun"].ToString(), ds.Tables["MYUNINFO_TBL"].Rows[i]["m_jibang"].ToString());//자동조판 강판시간을 구합니다.//"2017-03-27 09:39:23.000";
                    if (strTime.Trim() != "")
                    {
                        ds.Tables["MYUNINFO_TBL"].Rows[i]["dj_kpan_date"] = strTime;
                    }
                }

                //+----------------------------------------------------------------
                //자동조판 강판 카운트를 구한다.
                //+----------------------------------------------------------------
                //+---------------------------------------------------+//
                //자동조판 필드를 재정의한다.
                //+---------------------------------------------------+//
                if (ds.Tables["MYUNINFO_TBL"].Rows[i]["myun_id"].ToString() == Util.JOPANTYPE_AUTO_CODE || ds.Tables["MYUNINFO_TBL"].Rows[i]["myun_id"].ToString() == Util.JOPANTYPE_MANUAL_CODE)
                {
                    string strCount = get_autocts_kangpan_count(ds.Tables["MYUNINFO_TBL"].Rows[i]["m_myun"].ToString(), ds.Tables["MYUNINFO_TBL"].Rows[i]["m_jibang"].ToString());

                    if (strCount != "")
                        ds.Tables["MYUNINFO_TBL"].Rows[i]["dj_kpan_count"] = Convert.ToInt32(strCount);
                }
                else
                {
                    ds.Tables["MYUNINFO_TBL"].Rows[i]["dj_kpan_count"] = Convert.ToInt32(strKangCnt);
                }

                ds.Tables["MYUNINFO_TBL"].Rows[i]["PrintTime"] = strSongchulTime;

                if (ds.Tables["MYUNINFO_TBL"].Rows[i]["dj_kpan_date"].ToString() != "")
                {
                    DateTime rowKpanTime = Convert.ToDateTime(ds.Tables["MYUNINFO_TBL"].Rows[i]["dj_kpan_date"].ToString());

                    if (m_LastKangTime < rowKpanTime)
                        m_LastKangTime = rowKpanTime;
                }
            }

            DataList1.DataSource = ds.Tables["MYUNINFO_TBL"];
            DataList1.DataBind();

            for (int i = 0; i < ds.Tables["MYUNINFO_TBL"].Rows.Count; i++)
            {
                string sFilePath = "";
                string sFile = "";
                string dLock = "";
                string dKpang = "";
                string strId = "";

                sFile = ds.Tables["MYUNINFO_TBL"].Rows[i]["filename"].ToString();
                dLock = ds.Tables["MYUNINFO_TBL"].Rows[i]["dj_lock_user"].ToString();
                dKpang = ds.Tables["MYUNINFO_TBL"].Rows[i]["dj_kpang"].ToString();

                strId = ds.Tables["MYUNINFO_TBL"].Rows[i]["myun_id"].ToString();

                string strFileInfo = sFile.Substring(0, 1) + sFile.Substring(5, 6) + "00" + sFile.Substring(11, 2) + "00";

                sFilePath = ConfigurationManager.AppSettings["strMThumb"] + strFileInfo + ".jpg";

                if (!File.Exists(sFilePath))
                {
                    if (strId == Util.JOPANTYPE_AUTO_CODE || strId == Util.JOPANTYPE_MANUAL_CODE)
                    {
                        string strCount = "";
                        strCount = get_autocts_kangpan_count(ds.Tables["MYUNINFO_TBL"].Rows[i]["m_myun"].ToString(), ds.Tables["MYUNINFO_TBL"].Rows[i]["m_jibang"].ToString());

                        if (Convert.ToInt32(strCount) > 0)
                        {
                            DataList1.Items[i].BackColor = Color.FromArgb(25, 25, 112);
                        }
                        else
                        {
                            DataList1.Items[i].BackColor = Color.FromArgb(109, 109, 109);
                        }
                    }
                    else
                    {
                        int intKpangValue = Convert.ToInt32(dKpang);

                        if (intKpangValue > 0)
                        {
                            // 강판 배경색
                            DataList1.Items[i].BackColor = Color.FromArgb(25, 25, 112);
                        }
                        else
                        {
                            // 미강판 배경색
                            DataList1.Items[i].BackColor = Color.FromArgb(109, 109, 109);
                        }
                    }
                }

                tmpColor += DataList1.Items[i].BackColor.Name.Substring(2, 6) + ",";
            }

            if (tmpColor.Length > 1)
                tmpColor = tmpColor.Substring(0, tmpColor.Length - 1);
        }
        catch (Exception)
        {
        }
        finally
        {
            ViewState["m_strMaecheCode"] = strMediaNum;
            ViewState["m_strView"] = strView;
        }
    }

    // 자동조판 CTP 출력시간
    public string get_autocts_ctptime(string page, string jibang)
    {
        string ret = "";
        string strEdition = "";

        strEdition = strPan.PadLeft(2, '0') + Util.GetJibangCode(jibang, "");

        string strSQL = "SELECT verifytime FROM [PaperProduction].[dbo].[PageTableMake] where PublicationID=21 and PubDate='" + strDate.Replace("-", "") + "' and editionalias='" + strEdition + "' and page=" + page;
        SqlConnection SqlConn = new SqlConnection(m_strConn);
        SqlCommand SqlCmd = new SqlCommand(strSQL, SqlConn);

        try
        {
            SqlConn.Open();
            SqlDataReader Dr = SqlCmd.ExecuteReader();

            while (Dr.Read())
            {
                ret = Dr[0].ToString();
            }

            Dr.Close();
        }
        catch (Exception)
        {
        }
        finally
        {
            SqlConn.Close();
        }

        return ret;
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
                ret = Dr[0].ToString();
            }

            Dr.Close();
        }
        catch (Exception)
        {
        }
        finally
        {
            SqlConn.Close();
        }

        return ret;
    }

    // 자동조판 강판횟수
    public string get_autocts_kangpan_count(string page, string jibang)
    {
        string ret = "";
        string strSQL = "SELECT count(*) as cnt FROM [DAPS].[dbo].[CTS_JOPANINFO_HISTORY] where id_mechae=" + strMediaNum + " and v_paperdate='" + strDate + "' and n_pan='" + strPan + "' and n_page='" + page + "' and n_jibang='" + jibang + "'  and v_job='강판'";
        SqlConnection SqlConn = new SqlConnection(m_strConn);
        SqlCommand SqlCmd = new SqlCommand(strSQL, SqlConn);

        try
        {
            SqlConn.Open();
            SqlDataReader Dr = SqlCmd.ExecuteReader();

            while (Dr.Read())
            {
                ret = Dr[0].ToString();
            }

            Dr.Close();
        }
        catch (Exception)
        {
        }
        finally
        {
            SqlConn.Close();
        }

        return ret;
    }

    #region 전체 면 개수
    protected int GetMyunTotalCount(string kpang)
    {
        SqlConnection Conn = null;
        int nValue = 1;

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

            nValue = nReturnValue;
        }
        catch (Exception)
        {
        }
        finally
        {
            Conn.Close();
        }

        return nValue;
    }
    #endregion

    #region 지역코드
    public string fncCodeConvert(string code, string myun_id)
    {
        string strCodeName = "";

        if (!(code.Trim().Equals("") || code == null || code == "0"))
        {
            try
            {
                strCodeName = sJibangCol[code].ToString();
            }
            catch (Exception)
            {
                strCodeName = "";
            }
        }

        return strCodeName;
    }
    #endregion

    #region KPanCount 강판수에 따른 색깔 등 표시
    public string KPanCount(int iCnt)
    {
        string sCount = "";

        if (iCnt > 1)
            sCount = "<font color='yellow'><b>" + iCnt.ToString() + "</b></font>";
        else
            sCount = iCnt.ToString();

        return sCount;
    }
    #endregion

    // 강판횟수
    public void getKangpanInfo(string pubdate, string pan, string myun, string region)
    {
        string strSql = "SELECT top 1 count, outtime from TB_NetranFileStatus where medianame='서울신문' and pubdate='" + pubdate + "' and panstring='" + pan + "' and pagenum='" + myun + "' and regionname='" + region + "' order by intime desc";

        SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["strMFConn"]);
        SqlDataAdapter Adap = new SqlDataAdapter(strSql, Conn);

        DataSet ds = new DataSet();
        Adap.Fill(ds, "press");

        if (ds != null && ds.Tables.Count > 0)
        {
            if (ds.Tables[0].Rows.Count > 0)
            {
                strKangCnt = ds.Tables[0].Rows[0]["count"].ToString();
                strSongchulTime = ds.Tables[0].Rows[0]["outtime"].ToString();
            }
            else
            {
                strKangCnt = "";
                strSongchulTime = "";
            }
        }
    }
}
