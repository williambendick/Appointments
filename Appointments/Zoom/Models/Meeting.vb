
Namespace Zoom
    Public Class Meeting

        Public Property BMTUserId As String

        Public Property ZoomUserId As String

        Public Property AuthorizationId As Guid?

        Public Property Action As String

        Public Property Id As Long?

        Public Property Topic As String

        Public Property StartTime As Date?

        Public Property Duration As Integer?

        Public Property ErrorMessage As String

        Public Function ShouldSerializeId() As Boolean
            Return False
        End Function

        Public Function Create() As String
            Try
                Action = "create"

                Dim result As Response(Of Meeting) = MakeRequest("/users/me/meetings", "POST", Me)

                If result.Error IsNot Nothing Then Return result.Error.Message

                Id = result.Data.Id

                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function

        Public Function Update() As String

            Try
                If Id = 0 Then Return "meeting id is required"

                Action = "update"

                Dim result As Response(Of Meeting) = MakeRequest("/meetings/" & Id, "PATCH", Me)

                If result.Error IsNot Nothing Then Return result.Error.Message

                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function

        Public Function Delete() As String

            Try
                If Id = 0 Then Return "meeting id is required"

                Action = "delete"

                Dim result As Response(Of Meeting) = MakeRequest("/meetings/" & Id, "DELETE", Me)

                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try
        End Function

        Private Function MakeRequest(endpoint As String, httpMethod As String, meeting As Meeting) As Response(Of Meeting)

            Dim result As Response(Of Meeting) = Nothing

            Dim accessToken As String = API.GetToken("access", meeting.BMTUserId)

            'if not token is found for user, store meeting data in session & send user to zoom for authorization
            If accessToken = "token not found" Then

                API.RequestUserAuthorization(meeting)

            Else
                Dim requestHeaders As NameValueCollection = New NameValueCollection() From {{"Authorization", "Bearer " & accessToken}}

                result = API.Request(Of Meeting, Meeting)(API.BaseUrl & endpoint, httpMethod, requestHeaders, meeting)

                'if there is an error it may be because the access token has expired, so get a new token and try request again
                If result.Error IsNot Nothing Then

                    'return error instead of throw exception in API.RefreshToken() & handle here
                    Dim refreshedAccessToken As String = API.RefreshToken(meeting.BMTUserId)

                    requestHeaders.Set("Authorization", "Bearer " & refreshedAccessToken)

                    result = API.Request(Of Meeting, Meeting)(API.BaseUrl & endpoint, httpMethod, requestHeaders, meeting)

                End If

            End If

            Return result

        End Function
    End Class
End Namespace
