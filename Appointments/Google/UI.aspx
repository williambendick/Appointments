
<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="UI.aspx.vb" Inherits="Appointments.UI" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Google Test</title>
    <style>.ui-button {max-width:700px; outline: 0 !important;}</style>
</head>
<body>
    <form id="form1" runat="server">
    <div style="margin: auto; width: 90%;">
        <br /><br />

        <asp:Button ID="btnCreateMeeting" runat="server" Text="Create Meeting" CssClass="ui-button btn btn-primary btn-lg" 
            Width="300px" Font-Size="12pt" Height="45px" BorderWidth="0px" UseSubmitBehavior="False" OnClientClick="createMeeting(); return false;" />
        <br /><br />        

        <asp:Button ID="btnUpdateMeeting" runat="server" Text="Update Meeting" CssClass="ui-button btn btn-primary btn-lg" 
            Width="300px" Font-Size="12pt" Height="45px" BorderWidth="0px" UseSubmitBehavior="False" OnClientClick="updateMeeting(); return false;" />
        &nbsp
        <asp:TextBox ID="txtUpdateMeeting" runat="server" Height="35px" Width="250px" placeholder="Meeting ID" BorderColor="#F0F0F0" BorderStyle="Solid" style="padding: 0px 15px;"></asp:TextBox>
        <br /><br />

        <asp:Button ID="btnDeleteMeeting" runat="server" Text="Delete Meeting" CssClass="ui-button btn btn-primary btn-lg" 
            Width="300px" Font-Size="12pt" Height="45px" BorderWidth="0px" UseSubmitBehavior="False" OnClientClick="deleteMeeting(); return false;" />
        &nbsp
        <asp:TextBox ID="txtDeleteMeeting" runat="server" Height="35px" Width="250px" placeholder="Meeting ID" BorderColor="#F0F0F0" BorderStyle="Solid" style="padding: 0px 15px;"></asp:TextBox>
        <br /><br /><br />

        <asp:Label ID="Label1" runat="server" Font-Size="12pt" Font-Names="Arial" Font-Bold="True" Text="Result: "></asp:Label>
        <span id="lblResult" style="font-family: Arial; font-size: 11pt"></span>
    </div>
    </form>

    <script src="https://apis.google.com/js/client.js"></script>
    <script src="luxon.min.js"></script>
    <script src="API.js"></script>

    <script>

        var _txtUpdateMeeting = document.getElementById('<%=txtUpdateMeeting.ClientID %>')
        var _txtDeleteMeeting = document.getElementById('<%=txtDeleteMeeting.ClientID %>')
        var _lblResult = document.getElementById('lblResult');
       
        async function createMeeting() {

            // note: using luxon 'T' instead of 'HH:mm:ss' causes 12 AM times to be incorrectly formated with hour as 24 instead of 0 
            var start = luxon.DateTime.now().plus({ hours: 1, }).toFormat("yyyy-LL-dd'T'HH:mm:ssZZ");
            var end = luxon.DateTime.now().plus({ hours: 2, }).toFormat("yyyy-LL-dd'T'HH:mm:ssZZ");

            var eventData = {
                'summary': 'test summary',
                'location': 'test location',
                'start': { 'dateTime': start },
                'end': { 'dateTime': end }
            };

            var result = await makeCalendarAPIRequest('create', eventData);
            _lblResult.innerText = result;
        }

        async function updateMeeting() {

            var eventData = {
                'id': _txtUpdateMeeting.value,
                'summary': 'updated summary'
            };

            var result = await makeCalendarAPIRequest('update', eventData);
            _lblResult.innerText = result;
        }

        async function deleteMeeting() {

            var eventData = {'id': _txtDeleteMeeting.value};

            var result = await makeCalendarAPIRequest('delete', eventData);
            _lblResult.innerText = result;
        }

    </script>
  
</body>
</html>
