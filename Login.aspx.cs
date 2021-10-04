using System;
using System.Data;
using System.Web.UI;
using System.Data.SqlClient;
using System.Configuration;

public partial class Login : Page
{
    /* 권한 정보
     * 1	SUPER
     * 6	모니터링 SUPER(쓰기/읽기)
     * 601	지면 쓰기/읽기
     * 602	선화 쓰기/읽기
     * 603	모두 읽기
     * 604	지면 읽기
     * 605	선화 읽기
     */

    /* 로그인 정보
    * 0     정상
    * 1	    존재하지 않는 사용자
    * 2	    권한이 없는 사용자
    * 3	    로그인 중인 사용자
    */

    /* 색상 정보
     * Blue			강판파일이 있을 때
     * MidnightBlue	강판파일이 없고 DJ_KPANG이 있을 때
     * GrayText		강판파일이 없고 DJ_KPANG이 없을 때
     * Olive		대조작업 중일 때
     */

    protected void Page_Load(object sender, EventArgs e)
    {
        string strSSO_ID = (Request["sso_ID"] == null) ? "" : Request["sso_ID"];
        string strSSO_PW = (Request["sso_PWD"] == null) ? "" : Request["sso_PWD"];

        if (strSSO_ID + strSSO_PW != "")
        {
            id.Value = strSSO_ID;
            pw.Value = strSSO_PW;

            login_Click(null, null);
        }
        else
        {
            //ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('세션이 만료되었습니다. 다시 로그인 해주세요.');</script>");
        }
    }

    // 로그인 버튼 클릭
    protected void login_Click(object sender, EventArgs e)
    {
        string input_id = id.Value;
        string input_pw = pw.Value;

        SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["strCMSCOMConn"]);
        SqlCommand cmd = null;

        try
        {
            con.Open();

            cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.Parameters.Add(new SqlParameter("@szUserID", input_id));
            cmd.Parameters.Add(new SqlParameter("@szPassword", input_pw));
            cmd.Parameters.Add(new SqlParameter("@nSWIDCode", "2009"));
            cmd.Parameters.Add(new SqlParameter("@szLoginIP", Request.UserHostAddress.ToString()));
            cmd.Parameters.Add("ReturnValue", SqlDbType.Int);

            cmd.Parameters["ReturnValue"].Direction = ParameterDirection.ReturnValue;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "up_login_v2310";

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            if (dr.HasRows)
            {
                dr.Read();

                Session.Add("UserCode", (int)dr.GetValue(0));
                Session.Add("UserID", input_id);
                Session.Add("UserPWD", input_pw);
            }

            dr.Close();

            switch ((int)cmd.Parameters["ReturnValue"].Value)
            {
                case 0:
                    Session.Add("Login", "1");
                    Response.Redirect("DataList.aspx");
                    break;
                case 1:
                    ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('존재하지 않는 사용자입니다.');</script>");
                    break;
                case 2:
                    ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('비밀번호가 일치하지 않거나 권한이 없는 아이디입니다.');</script>");
                    break;
                case 3:
                    Logout();
                    Session.Add("Login", "1");
                    Response.Redirect("DataList.aspx");
                    break;
            }
        }
        catch (Exception)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "alert", "<script>alert('비밀번호가 일치하지 않거나 권한이 없는 아이디입니다.');</script>");
        }
        finally
        {
            con.Close();
            con.Dispose();
        }
    }

    // 로그아웃
    protected void Logout()
    {
        if (Session["UserCode"] == null)
        {
            return;
        }
        else
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["strConn"]);
            SqlCommand cmd = null;

            try
            {
                con.Open();

                cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.Parameters.Add(new SqlParameter("@nUserCode", Session["UserCode"]));
                cmd.Parameters.Add(new SqlParameter("@nSWCode", 2009));

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "up_logout_v2310";

                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                dr.Close();
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
    }
}
