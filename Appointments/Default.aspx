<%@ Page Title="Home Page" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.vb" Inherits="Appointments._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <asp:Button ID="btnCreateGoogleEvent" runat="server" Text="Create Google Event" CssClass="btn btn-primary btn-lg" Width="700px" style="max-width:700px;"/>
        <br /><br />
        <asp:Button ID="btnCreateZoomMeeting" runat="server" Text="Create Zoom Meeting" CssClass="btn btn-primary btn-lg" Width="700px" style="max-width:700px;"/>
        <br /><br />
        <asp:Label ID="Label1" runat="server" Font-Size="15pt" Text="Result: " Font-Bold="True"></asp:Label>
        <asp:Label ID="lblResult" runat="server" Font-Size="15pt"></asp:Label>
        <asp:HiddenField ID="hdnCalendarEventId" runat="server" ></asp:HiddenField>
    </div>
</asp:Content>
