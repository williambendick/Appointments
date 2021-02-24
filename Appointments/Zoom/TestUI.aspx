
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
        <asp:Button ID="btnCreateGoogleEvent" runat="server" Text="Create Google Event" CssClass="btn btn-primary btn-lg" Width="700px" style="max-width:700px;" Font-Size="12pt" Height="50px"/>
        <br /><br />
        <asp:Button ID="btnCreateZoomMeeting" runat="server" Text="Create Zoom Meeting" CssClass="btn btn-primary btn-lg" Width="700px" style="max-width:700px;" Font-Size="12pt" Height="50px"/>
        <br /><br />
        <asp:Label ID="Label1" runat="server" Font-Size="15pt" Text="Result: " Font-Bold="True"></asp:Label>
        <asp:Label ID="lblResult" runat="server" Font-Size="15pt" Font-Names="Arial"></asp:Label>
        <asp:HiddenField ID="hdnCalendarEventId" runat="server" ></asp:HiddenField>
    </div>
    </form>
</body>
</html>
