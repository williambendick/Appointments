Public Class TestUI
    Inherits Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
    End Sub

    Protected Sub btnCreateMeeting_Click(sender As Object, e As EventArgs) Handles btnCreateMeeting.Click

        If Request.Cookies("ZAT") Is Nothing Then
            Zoom.API.RequestUserAuthorization("create-meeting")
        Else
            Dim meeting As Zoom.Meeting = New Zoom.Meeting() With
            {
                .Topic = "Initial Meeting Topic",
                .StartTime = Date.Now.AddHours(1),
                .Duration = 60
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

    Protected Sub btnUpdateMeeting_Click(sender As Object, e As EventArgs) Handles btnUpdateMeeting.Click

        If Not IsNumeric(txtUpdateMeeting.Text) Then
            lblResult.Text = "Please enter a valid meeting id."
            Exit Sub
        End If

        If Request.Cookies("ZAT") Is Nothing Then
            Zoom.API.RequestUserAuthorization("update-meeting")
        Else

            'only the properties that are included in this object wll be updated
            Dim meeting As Zoom.Meeting = New Zoom.Meeting() With
            {
                .Id = txtUpdateMeeting.Text,
                .Topic = "Updated Meeting Topic",
                .StartTime = Date.Now.AddHours(24),
                .Duration = 120
            }

            Dim result As String = meeting.Update()

            If result = "success" Then
                lblResult.Text = "Zoom meeting with id " & meeting.Id.ToString() & " was updated"
            ElseIf result = "access token does not exist" Then
                Zoom.API.RequestUserAuthorization("update-meeting")
            Else
                lblResult.Text = "An error occurred: " & result
            End If
        End If

    End Sub

    Protected Sub btnDeleteMeeting_Click(sender As Object, e As EventArgs) Handles btnDeleteMeeting.Click

        If Not IsNumeric(txtDeleteMeeting.Text) Then
            lblResult.Text = "Please enter a valid meeting id."
            Exit Sub
        End If

        If Request.Cookies("ZAT") Is Nothing Then
            Zoom.API.RequestUserAuthorization("delete-meeting")
        Else

            Dim meeting As Zoom.Meeting = New Zoom.Meeting() With {.Id = txtDeleteMeeting.Text}

            Dim result As String = meeting.Delete()

            If result = "success" Then
                lblResult.Text = "Zoom meeting with id " & meeting.Id.ToString() & " was deleted"
            ElseIf result = "access token does not exist" Then
                Zoom.API.RequestUserAuthorization("delete-meeting")
            Else
                lblResult.Text = "An error occurred: " & result
            End If
        End If

    End Sub

End Class