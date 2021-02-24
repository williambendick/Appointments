
Imports Appointments.Appointments

Public Class _Default
    Inherits Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
    End Sub
    Protected Sub btnCreateGoogleEvent_Click(sender As Object, e As EventArgs) Handles btnCreateGoogleEvent.Click

        Dim evt As GoogleEvent = New GoogleEvent()

        If hdnCalendarEventId.Value = String.Empty Then
            evt.Summary = "Test creation of event"
            evt.Location = "a location"
            evt.StartTime = Date.Now.AddMinutes(60)
            evt.EndTime = Date.Now.AddMinutes(120)
            evt.Create()
        Else
            evt.Summary = "Test updating of event"
            evt.Update(hdnCalendarEventId.Value)
        End If

        If evt.Result.Error = Nothing Then
            If hdnCalendarEventId.Value = String.Empty Then
                lblResult.Text = "Google event created with id " & evt.Result.Id
                btnCreateGoogleEvent.Text = "Update summary for Google event with id " & evt.Result.Id
                hdnCalendarEventId.Value = evt.Result.Id
            Else
                hdnCalendarEventId.Value = String.Empty
                lblResult.Text = "Google event with id " & evt.Result.Id & " was updated."
                btnCreateGoogleEvent.Text = "Create Google Event"
            End If
        Else
            hdnCalendarEventId.Value = String.Empty
            lblResult.Text = "An error occurred: " & evt.Result.Error
        End If

    End Sub

    Protected Sub CreateZoomMeeting_Click(sender As Object, e As EventArgs) Handles btnCreateZoomMeeting.Click

        'if zoom refresh token does not exist for this user, send to zoom to authorize application
        If Request.Cookies("ZAT") Is Nothing Then
            Zoom.API.RequestUserAuthorization("create-meeting")
        Else
            Dim meeting As Zoom.Models.Meeting = New Zoom.Models.Meeting() With
            {
                .Topic = "Test Meeting",
                .StartTime = Date.Now.AddHours(1),
                .Duration = 120
            }

            Dim result As String = meeting.Create()

            If result = "success" Then
                lblResult.Text = "Zoom meeting created with id " & meeting.Id.ToString()
            ElseIf result = "access token does not exist" Then
                Zoom.API.RequestUserAuthorization("create-meeting")
            Else
                lblResult.Text = "An error occurred: " & result
            End If
        End If

    End Sub
End Class