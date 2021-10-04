using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Web;
using System.Web.UI;

public partial class Preview : Page
{
    DataTable dt = null;
    // Param
    protected string strMediaNum = "";
    protected string strMediaAlpha;
    //protected string strMediaKor;        
    protected string strDate = "";
    protected string strYear = "";
    protected string strMonth = "";
    protected string strDay = "";
    protected string strPan = "";
    //protected string strView = "";
    protected string strMyun = "";
    protected string strJibang = "";
    protected string strJopan = "";
    // URL
    protected string m_dapsUrl = ConfigurationManager.AppSettings["strDapsUrl"];
    protected string m_strKangThum = ConfigurationManager.AppSettings["strKangThum"];
    protected string m_strKangPrev = ConfigurationManager.AppSettings["strKangPrev"];
    protected string m_strFilePath = ConfigurationManager.AppSettings["strFile"];
    // 프린트
    protected string m_strWorkFile1, m_strWorkFile2, m_strWorkingFile, m_strOutviewFile;
    protected string localFilename, localFilenamepdf;
    protected string strKangFileName;
    protected string lastUpdateTime;

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
            //if (Request.QueryString["view"] == null || Request.QueryString["view"] == "")
            //    strView = "0";
            //else
            //    strView = Request.QueryString["view"].ToString();

            // 면
            if (Request.QueryString["myun"] == null || Request.QueryString["myun"] == "")
                strMyun = "1";
            else
                strMyun = Request.QueryString["myun"].ToString();

            // 지방
            if (Request.QueryString["jibang"] == null || Request.QueryString["jibang"] == "")
                strJibang = "1";
            else
                strJibang = Request.QueryString["jibang"].ToString();

            // 조판
            if (Request.QueryString["jopan"] == null || Request.QueryString["jopan"] == "")
                strJopan = Util.JOPANTYPE_MANUAL_CODE;
            else
                strJopan = Request.QueryString["jopan"].ToString();

            SetMyunList();
            SetInfoTable();
            SetThumb();
            SetPreview("gangpan");

            // 프린트 부서 초기값
            if (!IsPostBack)
            {
                if (Request.Cookies["BUSEO"] != null)
                {
                    printBuseo.SelectedValue = HttpUtility.UrlDecode(Request.Cookies["BUSEO"].Value);
                }
                else
                {
                    string strConn = ConfigurationManager.AppSettings["strCMSCOMConn"];
                    string sql = string.Format(@"select v_name from T_PUBPART where ID_PUBPART = (select ID_PUBPART from T_USERINFO where ID_USERCODE = '{0}')", Session["UserCode"].ToString());

                    using (SqlConnection conn = new SqlConnection(strConn))
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            try
                            {
                                conn.Open();

                                using (SqlDataReader rdr = cmd.ExecuteReader())
                                {
                                    rdr.Read();

                                    if (rdr.HasRows)
                                        printBuseo.SelectedValue = rdr["v_name"].ToString();
                                }
                            }
                            catch (Exception)
                            {
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                }
            }
        }
    }

    // 면 리스트 보기
    private void SetMyunList()
    {
        dt = Util.ExecuteQuery(new SqlCommand(string.Format(@"select * from 
(select myun_id, m_myun, SUBSTRING(myun_file, 3, 15) as filename, media, m_pan, m_jibang, paper_date, myun_code 
from [Prodarx2005].[dbo].[MYUNINFO_TBL] 
where media = '{0}' and paper_date = '{1}' and m_pan = '{2}' 
union select 
	(select case V_JOPANTYPE when '자동조판' then '999999' else '888888' end 
	from [DAPS].[dbo].[CMS_PAPERPLAN] b 
	where b.ID_MECHAE = '{0}' and V_PAPERDATE = CMS_PAPERPLAN.V_PAPERDATE and N_PAN = CMS_PAPERPLAN.N_PAN and N_PAGE = CMS_PAPERPLAN.N_PAGE and N_JIBANG = CMS_PAPERPLAN.N_JIBANG) 
as myun_id, N_PAGE as m_myun, V_PAGENAME, id_mechae as media, N_PAN as m_pan, N_JIBANG as m_jibang, REPLACE(V_PAPERDATE, '-', '') as paper_date, '9999' as myun_code 
from [DAPS].[dbo].[CMS_PAPERPLAN] 
where id_mechae = '{0}' and v_paperdate = '{3}' and n_pan = '{2}') a 
order by m_myun", strMediaNum, strDate.Replace("-", ""), strPan, strDate)), "SELECT");

        if (dt.Rows.Count > 0)
        {
            myunListRepeater.DataSource = dt;
            myunListRepeater.DataBind();
        }
    }

    // 면 정보 보기
    private void SetInfoTable()
    {
        // 판
        info_pan.Text = strPan;

        // 면
        info_myun.Text = strMyun;

        // 면 제목
        dt = Util.ExecuteQuery(new SqlCommand(string.Format(@"select V_PAGENAME, 
(select N_HOSU 
from [DAPS].[dbo].[CMS_PAPERPLAN] 
where n_flag != 0 and ID_MECHAE = '{0}' and V_PAPERDATE = '{1}' and N_PAGE = '{2}' and N_PAN = '{3}' and N_JIBANG = '{4}')
from [DAPS].[dbo].[CMS_PAPERPLAN] 
where n_flag = 1 and V_PAPERDATE = '{1}' and N_PAGE = '{2}' and N_PAN = '{3}' and N_JIBANG = '{4}'", strMediaNum, strDate, strMyun, strPan, strJibang)), "SELECT");

        if (dt.Rows.Count > 0)
            info_title.Text = dt.Rows[0].ItemArray[0].ToString().Trim();

        // 게재일
        info_date.Text = strDate;

        // 지방
        info_jibang.Text = Util.GetJibangCode(strJibang, "kor");

        // 편집자, 대조자
        if (strJopan == Util.JOPANTYPE_AUTO_CODE)
        {
            info_editor.Text = Util.JOPANTYPE_AUTO_NAME;
            info_editor2.Text = Util.JOPANTYPE_AUTO_NAME;
        }
        else if (strJopan == Util.JOPANTYPE_MANUAL_CODE)
        {
            if (dt.Rows.Count > 0)
                info_editor.Text = dt.Rows[0].ItemArray[1].ToString().Trim();

            dt = Util.ExecuteQuery(new SqlCommand(string.Format(@"select V_USERNAME
from [DAPS].[dbo].[CMS_USERINFO] 
where RTRIM(N_USEREMPNO) = 
(select top 1 V_USERID 
from [DAPS].[dbo].[CTS_JOPANINFO_HISTORY] 
where V_PAPERDATE = '{0}' and N_PAGE = '{1}' and N_PAN = '{2}' and N_JIBANG = '{3}' and V_USERID is not null and V_JOB = '저장' 
order by D_CREATETIME desc)", strDate, strMyun, strPan, strJibang)), "SELECT");

            if (dt.Rows.Count > 0)
                info_editor2.Text = dt.Rows[0].ItemArray[0].ToString().Trim();
        }

        // 강판시간
        dt = Util.ExecuteQuery(new SqlCommand(string.Format(@"select top 1 CONVERT(varchar, d_createtime, 108) 
from [DAPS].[dbo].[CTS_JOPANINFO_HISTORY] 
where id_mechae = '{0}' and v_paperdate = '{1}' and n_pan = '{2}' and n_page = '{3}' and n_jibang = '{4}' and v_job = '강판' 
order by d_createtime desc", strMediaNum, strDate, strPan, strMyun, strJibang)), "SELECT");

        if (dt.Rows.Count > 0)
            info_time.Text = dt.Rows[0].ItemArray[0].ToString().Trim();

        // 새로고침
        info_refresh.Text = DateTime.Now.ToString("HH:mm:ss");
    }

    // Thumb 이미지 보기 (조판, 강판)
    private void SetThumb()
    {
        string jopan_url = "";
        string gangpan_url = "";

        // 면이 있으면
        if (info_title.Text != "")
        {
            // 조판 Thumb
            jopan_url += m_dapsUrl + "/preview/";
            jopan_url += strDate.Replace("-", "");
            jopan_url += (strJopan == Util.JOPANTYPE_AUTO_CODE) ? "/noborder" : "";
            jopan_url += "/" + strMediaAlpha;
            jopan_url += strMyun.PadLeft(2, '0') + "-";
            jopan_url += strMonth + strDay + "-";
            jopan_url += strPan.PadLeft(2, '0');
            jopan_url += Util.GetJibangCode(strJibang, "") + ".jpg";

            thumb_jopan.ImageUrl = jopan_url + "?random=" + new Random().Next();

            // 강판 됐으면
            if (info_time.Text != "")
            {
                // 강판 Thumb
                dt = Util.ExecuteQuery(new SqlCommand(string.Format(@"select distinct filename 
from [PaperProduction].[dbo].[TB_NetranFileStatus] 
where pubdate = '{0}' and panstring = '{1}' and pagenum = '{2}' and regionname = '{3}' and filename is not null", strDate.Replace("-", ""), strPan.PadLeft(2, '0'), strMyun, Util.GetJibangCode(strJibang, "kor"))), "SELECT");

                if (dt.Rows.Count > 0)
                {
                    gangpan_url += m_strKangThum;
                    gangpan_url += dt.Rows[0].ItemArray[0].ToString().Trim();

                    thumb_gangpan.ImageUrl = gangpan_url + "?random=" + new Random().Next();
                }
                else
                {
                    thumb_gangpan.ImageUrl = @"images/tempFile.jpg";
                    thumb_gangpan.Enabled = false;
                }
            }
            else
            {
                thumb_gangpan.ImageUrl = @"images/tempFile.jpg";
                thumb_gangpan.Enabled = false;
            }
        }
        else
        {
            thumb_jopan.ImageUrl = @"images/tempFile.jpg";
            thumb_jopan.Enabled = false;

            thumb_gangpan.ImageUrl = @"images/tempFile.jpg";
            thumb_gangpan.Enabled = false;

            printBuseo.Enabled = false;
            printCount.Enabled = false;
            printBtn.Enabled = false;
        }
    }

    // Prev 이미지 보기
    private void SetPreview(string selected)
    {
        if (selected == "jopan")
        {
            // 면이 있으면
            if (info_title.Text != "")
            {
                preview.ImageUrl = thumb_jopan.ImageUrl;

                thumb_jopan.CssClass = "selected_thumb";
                thumb_gangpan.CssClass = "";
            }
            else
            {
                preview.ImageUrl = null;
            }
        }
        else
        {
            // 강판 됐으면
            if (info_time.Text != "" && !thumb_gangpan.ImageUrl.Contains("tempFile.jpg"))
            {
                preview.ImageUrl = thumb_gangpan.ImageUrl.Replace("Thumb", "Preview");

                thumb_jopan.CssClass = "";
                thumb_gangpan.CssClass = "selected_thumb";
            }
            else
            {
                SetPreview("jopan");
            }
        }
    }

    // 조판 이미지 클릭
    protected void thumb_jopan_Click(object sender, ImageClickEventArgs e)
    {
        SetPreview("jopan");
    }

    // 강판 이미지 클릭
    protected void thumb_gangpan_Click(object sender, ImageClickEventArgs e)
    {
        SetPreview("gangpan");
    }

    // 프린트 버튼 클릭
    protected void printBtn_Click(object sender, EventArgs e)
    {
        string type = "in";
        string strFileName = info_title.Text; // 종합
        string strViewPath = strMediaNum + "/" + strYear + "/" + strMonth + strDay + "/"; // 65/2021/0720/

        strKangFileName = Util.GetKangFileName(strDate, strPan, strMyun, strJibang);

        localFilename = string.Format(@"c:\temp\_ss{0}{1}{2}xx0101{4}{5}{3}.jpg", (strMyun.Length == 1 ? "0" + strMyun : strMyun), strDate.Replace("-", "").Substring(4, 4), (strPan.Length == 1 ? "0" + strPan : strPan), type, printCount.SelectedValue, (new Random()).Next(100, 999));
        localFilenamepdf = string.Format(@"c:\temp\_ss{0}{1}{2}xx0101{4}{5}{3}.eps", (strMyun.Length == 1 ? "0" + strMyun : strMyun), strDate.Replace("-", "").Substring(4, 4), (strPan.Length == 1 ? "0" + strPan : strPan), type, printCount.SelectedValue, (new Random()).Next(100, 999));

        DirectoryInfo di = new DirectoryInfo(@"c:\temp");
        if (!di.Exists)
            di.Create();

        if ((new FileInfo(localFilename)).Exists)
            localFilename = string.Format(@"c:\temp\_ss{0}{1}{2}xx0101{4}{5}{3}.jpg", (strMyun.Length == 1 ? "0" + strMyun : strMyun), strDate.Replace("-", "").Substring(4, 4), (strPan.Length == 1 ? "0" + strPan : strPan), type, printCount.SelectedValue, (new Random()).Next(100, 999));
        if ((new FileInfo(localFilenamepdf)).Exists)
            localFilenamepdf = string.Format(@"c:\temp\_ss{0}{1}{2}xx0101{4}{5}{3}.eps", (strMyun.Length == 1 ? "0" + strMyun : strMyun), strDate.Replace("-", "").Substring(4, 4), (strPan.Length == 1 ? "0" + strPan : strPan), type, printCount.SelectedValue, (new Random()).Next(100, 999));

        fncFileView(strFileName, strViewPath);

        //
        HttpCookie cookie = new HttpCookie("BUSEO", printBuseo.SelectedValue);
        Response.Cookies.Add(cookie);

        Response.Cookies["BUSEO"].Value = HttpUtility.UrlEncode(printBuseo.SelectedValue);
        cookie.Expires = DateTime.Now.AddDays(7);
        Response.Cookies["BUSEO"].Expires = DateTime.Now.AddDays(7);

        if (strJopan == Util.JOPANTYPE_MANUAL_CODE || strJopan == Util.JOPANTYPE_AUTO_CODE)
        {
            using (WebClient webClient = new WebClient())
            {
                using (WebClient webC = new WebClient())
                {
                    try
                    {
                        webC.DownloadFile(new Uri(m_strWorkingFile.Replace("jpg", "eps").Replace(@"/noborder", "")), localFilenamepdf);
                    }
                    catch (Exception ex)
                    {
                        ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('출력용 파일이 존재하지 않습니다. 조판기에서 출력해 주세요.');</script>");
                        Util.SaveLog("(출력실패) 출력자 : " + Session["UserID"].ToString() + "-" + printBuseo.SelectedValue + ", 파일경로 : " + localFilename.Replace("_", "") + Environment.NewLine + ex);
                        return;
                    }
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompletedPDF);
                webClient.DownloadFileAsync(new Uri(m_strWorkingFile.Replace("jpg", "eps").Replace(@"/noborder", "")), localFilenamepdf);
            }
        }
        else
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                webClient.DownloadFileAsync(new Uri(m_strWorkingFile.Replace("jpg", "eps")), localFilename);
            }
        }
    }

    public void fncFileView(string file, string code)
    {
        // 자동조판일 경우			
        if (strJopan == Util.JOPANTYPE_AUTO_CODE)
        {
            m_strWorkFile1 = m_dapsUrl + "/preview/" + strDate.Replace("-", "") + "/noborder/" + strMediaAlpha + strMyun.PadLeft(2, '0') + "-" + strDate.Replace("-", "").Substring(4, 2) + strDate.Replace("-", "").Substring(6, 2) + "-" + strPan.PadLeft(2, '0') + Util.GetJibangCode(strJibang, "") + ".eps";
            m_strWorkingFile = m_strWorkFile1;
            m_strWorkFile1 = m_strWorkFile1.Replace("eps", "jpg");

            m_strWorkFile2 = m_strKangThum + strKangFileName;
            m_strOutviewFile = m_strKangPrev + strKangFileName;
        }
        else if (strJopan == Util.JOPANTYPE_MANUAL_CODE)
        {
            m_strWorkFile1 = m_dapsUrl + "/preview/" + strDate.Replace("-", "") + "/" + strMediaAlpha + strMyun.PadLeft(2, '0') + "-" + strDate.Replace("-", "").Substring(4, 2) + strDate.Replace("-", "").Substring(6, 2) + "-" + strPan.PadLeft(2, '0') + Util.GetJibangCode(strJibang, "") + ".eps";
            m_strWorkingFile = m_strWorkFile1;
            m_strWorkFile1 = m_strWorkFile1.Replace("eps", "jpg");

            m_strWorkFile2 = m_strKangThum + strKangFileName;
            m_strOutviewFile = m_strKangPrev + strKangFileName;
        }
        else
        {
            //조판쪽
            m_strWorkFile1 = string.Format("{0}/{1}", m_strFilePath.TrimEnd('/'), code) + "MT" + file + ".jpg";
            m_strWorkingFile = string.Format("{0}/{1}", m_strFilePath.TrimEnd('/'), code) + "MO" + file + ".jpg";

            //필름쪽
            m_strWorkFile2 = m_strKangThum + strKangFileName;
            m_strOutviewFile = m_strKangPrev + strKangFileName;
        }
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        if (localFilename.Contains("in"))
        {
            Bitmap bmp1 = new Bitmap(localFilename);
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 200L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(localFilename.Replace("_", ""), jpgEncoder, myEncoderParameters);
            bmp1.Dispose();
        }
        else
        {
            File.Move(localFilename, localFilename.Replace("_", ""));
        }

        string name = null;

        if (Session["UserCode"] != null)
            name = Util.GetUserName(Session["UserCode"].ToString());

        string conString = ConfigurationManager.AppSettings["strConn"];
        string sql = @"SELECT Convert(varchar(20),save_date,120) FROM [Prodarx2005].[dbo].[MYUNINFO_TBL]  where paper_date = '" + strDate.Replace("-", "") + "' and m_myun=" + strMyun + " and m_pan=" + strPan;

        using (SqlConnection con = new SqlConnection(conString))
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = sql;
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    lastUpdateTime = reader[0].ToString();
                else
                    lastUpdateTime = "";
            }
        }

        Bitmap bitMapImage = new Bitmap(localFilename.Replace("_", ""));
        Graphics graphicImage = Graphics.FromImage(bitMapImage);
        Font useFont;
        int printnameYpos = 0;
        int printwidth = 0;
        int heigth = 0;

        if (localFilename.Contains("in"))
        {
            useFont = new Font("@돋움체", 20, FontStyle.Bold);
            printnameYpos = 60;
            printwidth = 50;
            heigth = 1850;
        }
        else
        {
            useFont = new Font("@굴림체", 20, FontStyle.Bold);
            printnameYpos = 1800;
            printwidth = 1830;
            heigth = 2510;
        }

        string txt = strMonth + "월 " + strDay + "일자 " + Util.GetJibangCode(strJibang, "kor") + " " + strPan + "판 " + strMyun + "면 (최종 저장시간 : " + lastUpdateTime;
        RectangleF body = new RectangleF(bitMapImage.Width - printnameYpos, heigth, printwidth, 1500);

        graphicImage.DrawString(txt, useFont, Brushes.Black, body);
        graphicImage.Dispose();

        bitMapImage.Save(localFilename);
        bitMapImage.Dispose();

        int res = UploadNEW(printBuseo.SelectedValue.Trim(), localFilename);

        if (strJopan == Util.JOPANTYPE_MANUAL_CODE || strJopan == Util.JOPANTYPE_AUTO_CODE)
        {
            (new FileInfo(localFilenamepdf)).Delete();
            (new FileInfo(localFilenamepdf.Replace("_", ""))).Delete();
        }
        else
        {
            (new FileInfo(localFilename)).Delete();
            (new FileInfo(localFilename.Replace("_", ""))).Delete();
        }

        if (res == 1)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('출력되었습니다.');</script>");
            Util.SaveLog("(출력완료) 출력자 : " + Session["UserID"].ToString() + "-" + printBuseo.SelectedValue + ", 파일경로 : " + localFilename.Replace("_", ""));
        }
        else if (res == 0)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('오류가 발생했습니다. IT개발부로 문의해 주세요.');</script>");
            Util.SaveLog("(출력실패) 출력자 : " + Session["UserID"].ToString() + "-" + printBuseo.SelectedValue + ", 파일경로 : " + localFilename.Replace("_", ""));
        }
        else
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('프린터 서버에 접속할 수 없습니다. IT개발부로 문의해 주세요.');</script>");
            Util.SaveLog("(출력실패) 출력자 : " + Session["UserID"].ToString() + "-" + printBuseo.SelectedValue + ", 파일경로 : " + localFilename.Replace("_", ""));
        }
    }

    private void DownloadFileCompletedPDF(object sender, AsyncCompletedEventArgs e)
    {
        File.Move(localFilenamepdf, localFilenamepdf.Replace("_", ""));

        int res = UploadNEW(printBuseo.SelectedValue.Trim(), localFilenamepdf.Replace("_", ""));
        (new FileInfo(localFilenamepdf)).Delete();
        (new FileInfo(localFilenamepdf.Replace("_", ""))).Delete();

        if (res == 1)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('출력되었습니다.');</script>");
            Util.SaveLog("(출력완료) 출력자 : " + Session["UserID"].ToString() + "-" + printBuseo.SelectedValue + ", 파일경로 : " + localFilename.Replace("_", ""));
        }
        else if (res == 0)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('오류가 발생했습니다. IT개발부로 문의해 주세요.');</script>");
            Util.SaveLog("(출력실패) 출력자 : " + Session["UserID"].ToString() + "-" + printBuseo.SelectedValue + ", 파일경로 : " + localFilename.Replace("_", ""));
        }
        else
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('프린터 서버에 접속할 수 없습니다.');</script>");
            Util.SaveLog("(출력실패) 출력자 : " + Session["UserID"].ToString() + "-" + printBuseo.SelectedValue + ", 파일경로 : " + localFilename.Replace("_", ""));
        }
    }

    private int UploadNEW(string folder, string filename)
    {
        string ftpServerIP = ConfigurationManager.AppSettings["FTPSVR_NEW"] + "/input/" + folder;
        string ftpUserID = ConfigurationManager.AppSettings["FTPID_NEW"];
        string ftpPassword = ConfigurationManager.AppSettings["FTPPWD_NEW"];

        if (!checkFTP(ConfigurationManager.AppSettings["FTPSVR_NEW"]))
        {
            if (!checkFTP(ConfigurationManager.AppSettings["FTPSVR2_NEW"]))
            {
                return -1;
            }
            else
            {
                ftpServerIP = ConfigurationManager.AppSettings["FTPSVR2_NEW"] + "/input/" + folder;
            }
        }

        FileInfo fileInf = new FileInfo(filename);
        string uri = ftpServerIP + "/" + fileInf.Name.Replace("_", "");
        FtpWebRequest reqFTP;

        // Create FtpWebRequest object from the Uri provided
        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
        reqFTP.UsePassive = true;

        // Provide the WebPermission Credintials
        reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

        // By default KeepAlive is true, where the control connection is 
        // not closed after a command is executed.
        reqFTP.KeepAlive = false;

        // Specify the command to be executed.
        reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

        // Specify the data transfer type.
        reqFTP.UseBinary = true;

        // Notify the server about the size of the uploaded file
        reqFTP.ContentLength = fileInf.Length;

        // The buffer size is set to 2kb
        int buffLength = 2048;
        byte[] buff = new byte[buffLength];
        int contentLen;

        // Opens a file stream (System.IO.FileStream) to read 
        //the file to be uploaded
        FileStream fs = fileInf.OpenRead();
        Stream strm = null;
        try
        {
            // Stream to which the file to be upload is written
            strm = reqFTP.GetRequestStream();

            // Read from the file stream 2kb at a time
            contentLen = fs.Read(buff, 0, buffLength);

            // Till Stream content ends
            while (contentLen != 0)
            {
                // Write Content from the file stream to the 
                // FTP Upload Stream
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }

            // Close the file stream and the Request Stream
            strm.Close();
            fs.Close();
            return 1;
        }
        catch (Exception)
        {
            strm.Close();
            fs.Close();
            return 0;
        }
    }

    private bool checkFTP(string ftpsvr)
    {
        string s = ftpsvr.Replace("ftp://", "").Replace(":9000", ""); //Replace input from your text box for IP address here

        if (PingHost(s))
        {
            TcpClient tc = new TcpClient();

            try
            {

                tc.Connect(ftpsvr.Replace("ftp://", "").Replace(":9000", ""), 9000);
                bool stat = tc.Connected;
                if (stat)
                    tc.Close();
                return true;
            }
            catch (Exception)
            {
                tc.Close();
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool PingHost(string nameOrAddress)
    {
        bool pingable = false;
        Ping pinger = new Ping();

        try
        {
            PingReply reply = pinger.Send(nameOrAddress, 1500);
            pingable = reply.Status == IPStatus.Success;
        }
        catch (PingException)
        {
            return false;
        }

        return pingable;
    }

    private ImageCodecInfo GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
                return codec;
        }

        return null;
    }
}
