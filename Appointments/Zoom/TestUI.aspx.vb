Public Class TestUI
    Inherits Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        If Request.QueryString("zoomAuthorizationResult") IsNot Nothing Then

            If Session("zoomMeeting") IsNot Nothing Then

                Dim meetingData As Zoom.Meeting = HttpContext.Current.Session("zoomMeeting")

                If meetingData.ErrorMessage Is Nothing Then

                    Select Case meetingData.Action.ToLower
                        Case "create"
                            lblResult.Text = "Zoom meeting created with id " & meetingData.Id.ToString()

                        Case "update"
                            lblResult.Text = "Zoom meeting with id " & meetingData.Id.ToString() & " was updated"

                        Case "delete"
                            lblResult.Text = "Zoom meeting with id " & meetingData.Id.ToString() & " was deleted"

                    End Select
                Else
                    lblResult.Text = "An error occurred: " & meetingData.ErrorMessage
                End If
            Else
                lblResult.Text = "Session object for zoom meeting does not exist."
            End If
        End If
    End Sub

    Protected Sub btnCreateMeeting_Click(sender As Object, e As EventArgs) Handles btnCreateMeeting.Click

        Dim meeting As Zoom.Meeting = New Zoom.Meeting() With
            {
                .BMTUserId = "3",
                .Topic = "Initial Meeting Topic",
                .StartTime = Date.Now.AddHours(1),
                .Duration = 60
            }

        Dim result As String = meeting.Create()

        If result = "success" Then
            lblResult.Text = "Zoom meeting created with id " & meeting.Id.ToString()
        Else
            lblResult.Text = "An error occurred: " & result
        End If

    End Sub

    Protected Sub btnUpdateMeeting_Click(sender As Object, e As EventArgs) Handles btnUpdateMeeting.Click

        If Not IsNumeric(txtUpdateMeeting.Text) Then
            lblResult.Text = "Please enter a valid meeting id."
            Exit Sub
        End If

        'only the properties that are included in this object wll be updated
        Dim meeting As Zoom.Meeting = New Zoom.Meeting() With
        {
            .BMTUserId = "4",
            .Id = txtUpdateMeeting.Text,
            .Topic = "Updated Meeting Topic",
            .StartTime = Date.Now.AddHours(24),
            .Duration = 120
        }

        Dim result As String = meeting.Update()

        If result = "success" Then
            lblResult.Text = "Zoom meeting with id " & meeting.Id.ToString() & " was updated"
        Else
            lblResult.Text = "An error occurred: " & result
        End If

    End Sub

    Protected Sub btnDeleteMeeting_Click(sender As Object, e As EventArgs) Handles btnDeleteMeeting.Click

        If Not IsNumeric(txtDeleteMeeting.Text) Then
            lblResult.Text = "Please enter a valid meeting id."
            Exit Sub
        End If

        Dim meeting As Zoom.Meeting = New Zoom.Meeting() With
        {
            .BMTUserId = "4",
            .Id = txtDeleteMeeting.Text
        }

        Dim result As String = meeting.Delete()

        If result = "success" Then
            lblResult.Text = "Zoom meeting with id " & meeting.Id.ToString() & " was deleted"
        Else
            lblResult.Text = "An error occurred: " & result
        End If

    End Sub

End Class