Imports Newtonsoft.Json
Namespace Zoom
    Public Class Meeting

        Public Property BMTUserId As String

        Public Property ZoomUserId As String

        Public Property ZoomUserAccessToken As String

        Public Property ZoomUserRefreshToken As String

        Public Property AuthorizationId As Guid?

        Public Property Action As String

        Public Property Id As Long?

        Public Property Topic As String

        Public Property StartTime As Date?

        Public Property Duration As Integer?

        Private ReadOnly Property BaseUrl As String = "https://api.zoom.us/v2"

        Public Function ShouldSerializeId() As Boolean
            Return False
        End Function

        Public Function Create() As String

            Dim endpoint As String = "/users/me/meetings"

            Try
                Dim result As Response(Of Meeting) = MakeRequest(endpoint, "POST", Me)

                'if there is an error it may be because the access token has expired, so get a new token and try request again
                If result.Error IsNot Nothing Then

                    Dim refreshedAccessToken As String = API.RefreshToken()

                    result = MakeRequest(endpoint, "post", Me, refreshedAccessToken)

                    If result.Error IsNot Nothing Then Return result.Error.Message

                End If

                Id = result.Data.Id
                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function

        Public Function Update() As String

            If Id = 0 Then Return "meeting id is required"

            Dim endpoint As String = "/meetings/" & Id

            Try
                Dim result As Response(Of Meeting) = MakeRequest(endpoint, "PATCH", Me)

                'if there is an error it may be because the access token has expired, so get a new token and try request again
                If result.Error IsNot Nothing Then

                    Dim refreshedAccessToken As String = API.RefreshToken()

                    result = MakeRequest(endpoint, "patch", Me, refreshedAccessToken)

                    If result.Error IsNot Nothing Then Return result.Error.Message

                End If

                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function

        Public Function Delete() As String

            If Id = 0 Then Return "meeting id is required"

            Dim endpoint As String = "/meetings/" & Id

            Try
                Dim result As Response(Of Meeting) = MakeRequest(endpoint, "DELETE")

                'if there is an error it may be because the access token has expired, so get a new token and try request again
                If result.Error IsNot Nothing Then

                    Dim refreshedAccessToken As String = API.RefreshToken()

                    result = MakeRequest(endpoint, "delete", Nothing, refreshedAccessToken)

                    If result.Error IsNot Nothing Then Return result.Error.Message

                End If

                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try
        End Function

        Private Function MakeRequest(endpoint As String, httpMethod As String, Optional meetingData As Meeting = Nothing, Optional refreshedToken As String = Nothing) As Response(Of Meeting)

            Dim accessToken As String = If(refreshedToken, API.GetToken("access"))

            If accessToken Is Nothing Then Throw New Exception("access token does not exist")

            Dim requestHeaders As NameValueCollection = New NameValueCollection() From {
                {"Authorization", "Bearer " & accessToken}
            }

            Return API.Request(Of Meeting, Meeting)(BaseUrl & endpoint, httpMethod, requestHeaders, meetingData)

        End Function
    End Class
End Namespace
