
<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TestUI.aspx.vb" Inherits="Appointments.TestUI" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Zoom Test</title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="margin: auto; width: 90%;">
        <br /><br />
        <asp:Button ID="btnCreateMeeting" runat="server" Text="Create Meeting" CssClass="btn btn-primary btn-lg" Width="300px" style="max-width:700px;" Font-Size="12pt" Height="45px" BorderWidth="0px" UseSubmitBehavior="False"/>
        <br /><br />        
        <asp:Button ID="btnUpdateMeeting" runat="server" Text="Update Meeting" CssClass="btn btn-primary btn-lg" Width="300px" style="max-width:700px;" Font-Size="12pt" Height="45px" BorderWidth="0px" UseSubmitBehavior="False"/>
        &nbsp
        <asp:TextBox ID="txtUpdateMeeting" runat="server" Height="35px" Width="150px" placeholder="Meeting ID" BorderColor="#F0F0F0" BorderStyle="Solid" style="padding: 0px 15px ;"></asp:TextBox>
        <br /><br />
        <asp:Button ID="btnDeleteMeeting" runat="server" Text="Delete Meeting" CssClass="btn btn-primary btn-lg" Width="300px" style="max-width:700px;" Font-Size="12pt" Height="45px" BorderWidth="0px" UseSubmitBehavior="False"/>
        &nbsp
        <asp:TextBox ID="txtDeleteMeeting" runat="server" Height="35px" Width="150px" placeholder="Meeting ID" BorderColor="#F0F0F0" BorderStyle="Solid" style="padding: 0px 15px ;"></asp:TextBox>
        <br /><br />
        <asp:Label ID="Label1" runat="server" Font-Size="15pt" Font-Names="Arial" Font-Bold="True" Text="Result: "></asp:Label>
        <asp:Label ID="lblResult" runat="server" Font-Size="15pt" Font-Names="Arial"></asp:Label>
    </div>
    </form>
</body>
</html>
