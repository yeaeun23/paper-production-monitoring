<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DataView.aspx.cs" Inherits="DataView" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=ks_c_5601-1987" />
    <title>서울신문 SIS | 제작현황</title>
    <link type="image/x-icon" rel="shortcut icon" href="images/favicon.ico" />
    <link type="text/css" rel="stylesheet" href="css/style.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery.min.js"></script>
    <script type="text/javascript" src="js/func.js"></script>
</head>

<body>
    <form runat="server" method="POST">
        <ul>
            <asp:Repeater runat="server" ID="DataList2">
                <ItemTemplate>
                    <li id='M<%# DataBinder.Eval(Container.DataItem, "myun_id") %><%# DataBinder.Eval(Container.DataItem, "m_myun") %><%# DataBinder.Eval(Container.DataItem, "m_jibang") %>'>
                        <%# fncMakeTable(DataBinder.Eval(Container.DataItem, "myun_id").ToString(), 
                                DataBinder.Eval(Container.DataItem, "m_myun").ToString(),
								DataBinder.Eval(Container.DataItem, "filename").ToString(),
                                DataBinder.Eval(Container.DataItem, "lay_user").ToString(),
						        DataBinder.Eval(Container.DataItem, "dj_user").ToString(),
						        DataBinder.Eval(Container.DataItem, "ad_user").ToString(),
						        DataBinder.Eval(Container.DataItem, "dj_lock_user").ToString(),
								DataBinder.Eval(Container.DataItem, "ad_lock_user").ToString(),
						        DataBinder.Eval(Container.DataItem, "dj_kpang").ToString(),
						        DataBinder.Eval(Container.DataItem, "prtTime_B", "{0:HH:mm}"),
								DataBinder.Eval(Container.DataItem, "myun_code").ToString(),
								DataBinder.Eval(Container.DataItem, "m_jibang").ToString(),
						        DataBinder.Eval(Container.DataItem, "dj_kpan_date", "{0:HH:mm}")) %>         
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </form>
</body>

<script type="text/javascript">
    function ViewPreview(myun, jopan, jibang) {
        var url = "";
        url += "Preview.aspx?media=<%= strMediaNum %>";
        url += "&date=<%= strDate %>";
        url += "&pan=<%= strPan %>";
        url += "&myun=" + myun;
        url += "&jibang=" + jibang;
        url += "&jopan=" + jopan;
        url += "&random=<%= new Random().Next() %>";

        window.open(url, 'Preview.aspx'); // target='_blank' 대신 파일명
        return;
    }
</script>
</html>
