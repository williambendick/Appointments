
Namespace Zoom
    Public Class User

        Public Property Id As String

        Public Property FirstName As String

        Public Property LastName As String

        Public Property Email As String

        Public Function Retrieve(accessToken As String) As String

            Try
                Dim requestHeaders As NameValueCollection = New NameValueCollection() From {
                    {"Authorization", "Bearer " & accessToken}
                }

                Dim result As Response(Of User) = API.Request(Of User, Object)(API.BaseUrl & "/users/me", "GET", requestHeaders, Nothing)

                If result.Error IsNot Nothing Then Return result.Error.Message

                Id = result.Data.Id
                FirstName = result.Data.FirstName
                LastName = result.Data.LastName
                Email = result.Data.Email

                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function

    End Class
End Namespace
