<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Preview.aspx.cs" Inherits="Preview" Async="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>서울신문 SIS | 제작현황</title>
    <link type="image/x-icon" rel="shortcut icon" href="images/favicon.ico" />
    <link type="text/css" rel="stylesheet" href="css/preview.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery.min.js"></script>
    <script type="text/javascript"> 
        // 새로고침 버튼 클릭
        function Search() {
            var url = "";
            url += "Preview.aspx?media=<%= strMediaNum %>";
            url += "&date=<%= strDate %>";
            url += "&pan=<%= strPan %>";
            url += "&myun=<%= strMyun %>";
            url += "&jibang=<%= strJibang %>";
            url += "&jopan=<%= strJopan %>";
            url += "&random=<%= new Random().Next() %>";

            location.href = url;
        }
    </script>
</head>

<body>
    <form id="form1" runat="server">
        <div class="div_top">
            <ul class="myunList">
                <asp:Repeater runat="server" ID="myunListRepeater">
                    <ItemTemplate>
                        <li class="myunListItem">
                            <a class="myunListBtn" href='Preview.aspx?media=<%= strMediaNum %>&date=<%= strDate %>&pan=<%= strPan %>&myun=<%# DataBinder.Eval(Container.DataItem, "m_myun").ToString() %>&jibang=<%# DataBinder.Eval(Container.DataItem, "m_jibang").ToString() %>&jopan=<%# DataBinder.Eval(Container.DataItem, "myun_id").ToString() %>&random=<%= new Random().Next() %>' title='<%# Util.GetJibangCode(DataBinder.Eval(Container.DataItem, "m_jibang").ToString(), "kor") %> <%# DataBinder.Eval(Container.DataItem, "m_pan").ToString() %>판 <%# DataBinder.Eval(Container.DataItem, "m_myun").ToString() %>면 [<%# DataBinder.Eval(Container.DataItem, "filename").ToString() %>]'><%# DataBinder.Eval(Container.DataItem, "m_myun").ToString() %>
                            </a>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>

        <div class="div_left">
            <table>
                <colgroup>
                    <col width="70px" />
                    <col width="*" />
                </colgroup>
                <tr>
                    <th colspan="2">
                        <asp:Label runat="server" ID="info_pan" />판
                        <asp:Label runat="server" ID="info_myun" />면
                        [<asp:Label runat="server" ID="info_title" />]                        
                    </th>
                </tr>
                <tr>
                    <th>게재일</th>
                    <td>
                        <asp:Label runat="server" ID="info_date" />
                    </td>
                </tr>
                <tr>
                    <th>지&nbsp;&nbsp;&nbsp;방</th>
                    <td>
                        <asp:Label runat="server" ID="info_jibang" />
                    </td>
                </tr>
                <tr>
                    <th>편집자</th>
                    <td>
                        <asp:Label runat="server" ID="info_editor" />
                    </td>
                </tr>
                <tr>
                    <th>대조자</th>
                    <td>
                        <asp:Label runat="server" ID="info_editor2" />
                    </td>
                </tr>
                <tr>
                    <th>강판시간</th>
                    <td>
                        <asp:Label runat="server" ID="info_time" />
                    </td>
                </tr>
                <tr>
                    <th style="padding: 0;">
                        <input type="button" class="btn" onclick="Search();" value="새로고침" style="height: 23px;" />
                    </th>
                    <td>
                        <asp:Label runat="server" ID="info_refresh" />
                    </td>
                </tr>
                <tr>
                    <th style="padding: 0;">
                        <asp:Button runat="server" ID="printBtn" CssClass="btn" Text="프린트" OnClientClick="return confirm('프린트 하시겠습니까?');" OnClick="printBtn_Click" Height="49" />
                    </th>
                    <td>
                        <asp:DropDownList runat="server" ID="printBuseo">
                            <asp:ListItem Value="임원실">임원실</asp:ListItem>
                            <asp:ListItem Value="논설위원실">논설위원실</asp:ListItem>
                            <asp:ListItem Value="심의실">심의실</asp:ListItem>
                            <asp:ListItem Value="경제정책부">경제정책부</asp:ListItem>
                            <asp:ListItem Value="국제부">국제부</asp:ListItem>
                            <asp:ListItem Value="금융부">금융부</asp:ListItem>
                            <asp:ListItem Value="문화부">문화부</asp:ListItem>
                            <asp:ListItem Value="사진부">사진부</asp:ListItem>
                            <asp:ListItem Value="사회2부">사회2부</asp:ListItem>
                            <asp:ListItem Value="사회부">사회부</asp:ListItem>
                            <asp:ListItem Value="산업부">산업부</asp:ListItem>
                            <asp:ListItem Value="어문부">어문부</asp:ListItem>
                            <asp:ListItem Value="정책뉴스부">정책뉴스부</asp:ListItem>
                            <asp:ListItem Value="정치부">정치부</asp:ListItem>
                            <asp:ListItem Value="체육부">체육부</asp:ListItem>
                            <asp:ListItem Value="편집1부">편집1부</asp:ListItem>
                            <asp:ListItem Value="편집2부">편집2부</asp:ListItem>
                            <asp:ListItem Value="편집3부">편집3부</asp:ListItem>
                            <asp:ListItem Value="온라인뉴스부">온라인뉴스부</asp:ListItem>
                            <asp:ListItem Value="편집제작부">편집제작부</asp:ListItem>
                            <asp:ListItem Value="IT개발부">IT개발부</asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList runat="server" ID="printCount">
                            <asp:ListItem Value="01">1매</asp:ListItem>
                            <asp:ListItem Value="02">2매</asp:ListItem>
                            <asp:ListItem Value="03">3매</asp:ListItem>
                            <asp:ListItem Value="04">4매</asp:ListItem>
                            <asp:ListItem Value="05">5매</asp:ListItem>
                            <asp:ListItem Value="06">6매</asp:ListItem>
                            <asp:ListItem Value="07">7매</asp:ListItem>
                            <asp:ListItem Value="08">8매</asp:ListItem>
                            <asp:ListItem Value="09">9매</asp:ListItem>
                            <asp:ListItem Value="10">10매</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>

            <div style="text-align: center; font-weight: bold;">
                <p>조 판</p>
                <asp:ImageButton runat="server" ID="thumb_jopan" OnClick="thumb_jopan_Click" AlternateText="&nbsp;이미지 준비중" />
                <br />
                <p>강 판</p>
                <asp:ImageButton runat="server" ID="thumb_gangpan" OnClick="thumb_gangpan_Click" AlternateText="&nbsp;이미지 준비중" />
                <br />
                <br />
            </div>
        </div>

        <div class="div_right">
            <div id="zoom">
                <asp:Image runat="server" ID="preview" />
            </div>
        </div>
    </form>
</body>

<script type="text/javascript">
    // 상단 뷰 > 면 버튼 css
    //var url = new URL(window.location.href);
    var myun = <%= strMyun %>;
    var jibang = <%= strJibang %>;

    if (myun == "" || myun == null) {
        $($(".myunListBtn")[0]).addClass("selected_myun");
    }
    else {
        $(".myunListBtn").each(function () {
            var href = $(this).attr("href");

            if (href.indexOf("myun=" + myun + "&") != -1 && href.indexOf("jibang=" + jibang + "&") != -1)
                $(this).addClass("selected_myun");
            else
                $(this).removeClass("selected_myun");
        });
    }

    // 오른쪽 뷰 > 이미지 줌(zoom)
    window.onload = function () {
        var scale = 1,
            panning = false,
            pointX = 0,
            pointY = 0,
            start = { x: 0, y: 0 },
            zoom = document.getElementById("zoom");

        function setTransform() {
            zoom.style.transform = "translate(" + pointX + "px, " + pointY + "px) scale(" + scale + ")";
        }

        zoom.onmousedown = function (e) {
            e.preventDefault();
            start = { x: e.clientX - pointX, y: e.clientY - pointY };
            panning = true;
        }

        zoom.onmouseup = function (e) {
            panning = false;
        }

        zoom.onmousemove = function (e) {
            e.preventDefault();

            if (!panning)
                return;

            pointX = (e.clientX - start.x);
            pointY = (e.clientY - start.y);

            setTransform();
        }

        zoom.onwheel = function (e) {
            e.preventDefault();

            var xs = (e.clientX - pointX) / scale,
              ys = (e.clientY - pointY) / scale,
              delta = (e.wheelDelta ? e.wheelDelta : -e.deltaY);

            (delta > 0) ? (scale *= 1.2) : (scale /= 1.2);

            pointX = e.clientX - xs * scale;
            pointY = e.clientY - ys * scale;

            setTransform();
        }
    };
</script>
</html>
