Public Class AuthorizationResponse

    Inherits Page
    Dim meeting As Zoom.Meeting = Nothing

    Protected Sub Page_Load(sender As Object, e As EventArgs)

        Dim result As String

        Try

            result = ValidateResponse()

            If result = "success" Then

                result = Zoom.API.InitializeUser(meeting.BMTUserId, Request.QueryString("code"))

                If result = "success" Then

                    Select Case meeting.Action.ToLower()

                        Case "create"
                            result = meeting.Create()

                        Case "update"
                            result = meeting.Update()

                        Case "delete"
                            result = meeting.Delete()

                    End Select

                End If

            End If

        Catch ex As Exception

            If meeting IsNot Nothing Then
                meeting.ErrorMessage = ex.Message
            End If

            result = ex.Message
        End Try

        If result <> "success" Then
            meeting.ErrorMessage = result
            result = "An error occurred."
        End If

        HttpContext.Current.Response.Redirect("~/Zoom/TestUI.aspx?zoomAuthorizationResult=" & result)

    End Sub

    Public Function ValidateResponse() As String

        If String.IsNullOrWhiteSpace(Request.QueryString("code")) Then Return "Authorization page url does not include code parameter."

        If String.IsNullOrWhiteSpace(Request.QueryString("state")) Then Return "Authorization page url does not include state parameter."

        If Session("zoomMeeting") Is Nothing Then Return "Session object for zoom meeting does not exist."

        meeting = HttpContext.Current.Session("zoomMeeting")

        If meeting.AuthorizationId.ToString <> Request.QueryString("state") Then
            Return "The authorization id sent to zoom when starting authorization process does not match the one in Authorization page url."
        End If

        Return "success"

    End Function
End Class
