<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DataList.aspx.cs" Inherits="DataList" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=ks_c_5601-1987" />
    <title>서울신문 SIS | 제작현황</title>
    <link type="image/x-icon" rel="shortcut icon" href="images/favicon.ico" />
    <link type="text/css" rel="stylesheet" href="css/style.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery.min.js"></script>
    <script type="text/javascript" src="js/func.js"></script>
    <script type="text/javascript"> 
        var sum = 60; // 새로고침 시간(초)  

        $(document).ready(function () {
            // 게재일, 뷰(라디오) 변경
            $("input").change(function () {
                Search();
            });

            // 매체, 판 변경
            $("select").change(function () {
                Search();
            });

            // 뷰(라디오) 선택값
            $("input[name='view']")[<%= strView %>].checked = true;  

            // 현재시각
            runClock();       
            
            // 새로고침
            setInterval(function () {
                $(".refresh").html("(" + --sum + "초 후 새로고침..)");

                if (sum == 0)
                    Search();
            }, 1000);
        });        

        function pad(n, width) {
            n = n + '';
            return n.length >= width ? n : new Array(width - n.length + 1).join('0') + n;
        }

        function runClock() {
            now = new Date();
            hours = now.getHours();
            minutes = now.getMinutes();
            seconds = now.getSeconds();
            time = pad(parseInt(hours), 2) + ":" + pad(parseInt(minutes), 2) + ":" + pad(parseInt(seconds), 2);

            $("input[name='lblRealTime']").val(time);

            setTimeout("runClock();", 1000);
        }      

        // 검색 버튼 클릭
        function Search() {
            var url = '';
            url += 'DataList.aspx?media=' + $("#media").val();
            url += '&date=' + $("#date").val();
            url += '&pan=' + $("#pan").val();
            url += '&view=' + $("input[name='view']:checked").val();
            url += '&random=<%= new Random().Next() %>';

            location.href = url;
        }
    </script>
</head>

<body>
    <form runat="server" id="PublishView" method="POST">
        <input type="hidden" name="tmpColor" value="<%=tmpColor%>" />

        <div class="div_top">
            매체 :
			<asp:DropDownList runat="server" ID="media" />&nbsp;
            게재일 :
            <asp:TextBox runat="server" ID="date" TextMode="Date" />&nbsp;
            판 :
			<asp:DropDownList runat="server" ID="pan" />&nbsp;           

            <input type="button" onclick="Search();" value="새로고침" />&nbsp;
            <span class="refresh">(60초 후 새로고침..)</span>

            <div class="info_color">
                <span></span>
                강판&nbsp;
                <span></span>
                미강판&nbsp;
                <span></span>
                작업중&nbsp;
                <span></span>
                자동      
            </div>
        </div>

        <div class="div_left">
            <table class="tb_title">
                <colgroup>
                    <col width="20%" />
                    <col width="30%" />
                    <col width="20%" />
                    <col width="30%" />
                </colgroup>
                <tr>
                    <th colspan="4" style="font-size: 17px; font-weight: bold; padding: 3px;">
                        <asp:Label runat="server" ID="lblMonth" />월
					    <asp:Label runat="server" ID="lblDay" />일자
					    <asp:Label runat="server" ID="lblPan" />판
					    <asp:Label runat="server" ID="lblMaeche" />
                    </th>
                </tr>
                <tr>
                    <th>현재시각</th>
                    <td>
                        <input type="text" name="lblRealTime" readonly /></td>
                    <th>갱신시각</th>
                    <td>
                        <asp:Label runat="server" ID="lblUpdateTime" /></td>
                </tr>
                <tr>
                    <th>강판면수</th>
                    <td>
                        <asp:Label runat="server" ID="lblPubKpang" /></td>
                    <th>미강판면수</th>
                    <td>
                        <asp:Label runat="server" ID="lblPubNoKpang" /></td>
                </tr>
                <tr>
                    <th>초쇄시각</th>
                    <td>
                        <asp:Label runat="server" ID="lblStartPrint" ForeColor="#cccccc" />
                    </td>
                    <th>종쇄시각</th>
                    <td>
                        <asp:Label runat="server" ID="lblEndPrint" ForeColor="#cccccc" /></td>
                </tr>
            </table>

            <table class="tb_view">
                <tr>
                    <td>
                        <asp:RadioButtonList runat="server" ID="view" RepeatDirection="Horizontal">
                            <asp:ListItem Value="0" />
                            <asp:ListItem Value="1" />
                            <asp:ListItem Value="2" />
                        </asp:RadioButtonList>
                    </td>
                </tr>
            </table>

            <table class="tb_time_header">
                <colgroup>
                    <col width="27px" />
                    <col width="55px" />
                    <col width="*" />
                    <col width="55px" />
                    <col width="55px" />
                    <col width="55px" />
                    <col width="55px" />
                </colgroup>
                <tr>
                    <th>면</th>
                    <th>지방</th>
                    <th>강판</th>
                    <th>광고시간</th>
                    <th>강판시간</th>
                    <th>송출시간</th>
                    <th>CTP출력 </th>
                </tr>
            </table>

            <div class="div_time">
                <asp:DataList runat="server" ID="DataList1" DataKeyField="myun_id">
                    <ItemTemplate>
                        <table id="imageTable" class="tb_time" onclick='fncPhotoClick(this, <%# DataBinder.Eval(Container.DataItem, "myun_id") %><%# DataBinder.Eval(Container.DataItem, "m_myun") %><%# DataBinder.Eval(Container.DataItem, "m_jibang") %>)'>
                            <colgroup>
                                <col width="27px" />
                                <col width="55px" />
                                <col width="*" />
                                <col width="55px" />
                                <col width="55px" />
                                <col width="55px" />
                                <col width="55px" />
                            </colgroup>
                            <tr>
                                <td style="color: #000;">
                                    <span <%# DataBinder.Eval(Container.DataItem, "myun_Id").ToString().Equals("888888") ? "style='background: lightgreen;'" : "style='background-color: #b4e0f0;'" %>>
                                        <%# DataBinder.Eval(Container.DataItem, "m_myun") %>
                                    </span>
                                </td>
                                <td>
                                    <%# fncCodeConvert(DataBinder.Eval(Container.DataItem, "m_jibang").ToString(), DataBinder.Eval(Container.DataItem, "myun_Id").ToString()) %>
                                </td>
                                <td>
                                    <%# KPanCount(Convert.ToInt16(DataBinder.Eval(Container.DataItem, "dj_kpan_count"))) %>
                                </td>
                                <td>
                                    <%# getDateToString(DataBinder.Eval(Container.DataItem, "ad_kpan_date").ToString(), DataBinder.Eval(Container.DataItem, "myun_Id").ToString()) %>
                                </td>
                                <td>
                                    <%# getDateToString1(DataBinder.Eval(Container.DataItem, "dj_kpan_date").ToString()) %>
                                </td>
                                <td>
                                    <%# getDateToString1(DataBinder.Eval(Container.DataItem, "dj_kpan_date").ToString()).Length == 0 ? "" : getDateToString2(DataBinder.Eval(Container.DataItem, "PrintTime").ToString(), DataBinder.Eval(Container.DataItem, "dj_kpan_date").ToString()) %>
                                </td>
                                <td>
                                    <%# getDateToString3(DataBinder.Eval(Container.DataItem, "prtTime_B").ToString(), DataBinder.Eval(Container.DataItem, "dj_kpan_date").ToString(), DataBinder.Eval(Container.DataItem, "myun_Id").ToString(), DataBinder.Eval(Container.DataItem, "m_myun").ToString(), DataBinder.Eval(Container.DataItem, "m_jibang").ToString()) %>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </div>
        </div>

        <div class="div_right">
            <embed id="embed" src="DataView.aspx?media=<%= strMediaNum %>&date=<%= strDate %>&pan=<%= strPan %>&view=<%= strView %>&random=<%= new Random().Next() %>" />
        </div>
    </form>
</body>
</html>
