
Namespace Appointments.Zoom.Models
    Public Class Meeting

        Public Property Id As Long

        Public Property Topic As String

        Public Property StartTime As Date

        Public Property Duration As Integer

        Public Function Create() As String

            Try
                Dim result As Response(Of Meeting) = MakeCreationRequest()

                'if there is an error it may be because the access token has expired, so get a new token and try request again
                If result.Error IsNot Nothing Then

                    Dim refreshedAccessToken As String = API.RefreshToken()

                    result = MakeCreationRequest(refreshedAccessToken)

                    If result.Error IsNot Nothing Then Return result.Error.Message

                End If

                Id = result.Data.Id
                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function

        Private Function MakeCreationRequest(Optional refreshedToken As String = Nothing) As Response(Of Meeting)

            Dim accessToken As String = If(refreshedToken, API.GetToken("access"))

            If accessToken Is Nothing Then Throw New Exception("access token does not exist")

            Dim requestUri As String = $"https://api.zoom.us/v2/users/me/meetings"

            Dim requestHeaders As NameValueCollection = New NameValueCollection() From {
                {"Authorization", $"Bearer {accessToken}"}
            }

            Return API.Request(Of Meeting, Meeting)(requestUri, "post", requestHeaders, Me)

        End Function
    End Class
End Namespace
