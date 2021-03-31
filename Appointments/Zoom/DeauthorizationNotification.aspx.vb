Imports System.IO
Imports System.Web.Configuration
Imports Appointments.Zoom

Public Class DeauthorizationNotification

    Inherits Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        'make sure request is a POST, and that there is an 'Authorization' header with a value that matches the ZoomNoticationToken in config file
        If Request.HttpMethod.ToUpper() = "POST" AndAlso String.IsNullOrWhiteSpace(Request.Headers("Authorization")) = False _
            AndAlso Request.Headers("Authorization") = WebConfigurationManager.AppSettings("ZoomNoticationToken") Then

            Try
                Response.Clear()

                Dim stream As String
                Dim deauthorization As Deauthorization

                'deserialize the request
                Using reader As StreamReader = New StreamReader(Request.InputStream)
                    stream = reader.ReadToEnd()
                    deauthorization = API.DeserializeData(Of Deauthorization)(stream)
                End Using

                If deauthorization IsNot Nothing AndAlso deauthorization.Event = "app_deauthorized" Then

                    Dim payload As DeauthorizationPayload = deauthorization.Payload

                    'if user chose to have their data deleted when they uninstalled
                    If payload.UserDataRetention = False Then

                        Dim sql As String = "SELECT id FROM Users WHERE ZoomUserId IS NOT NULL AND ZoomUserId = '" & payload.UserId & "'"
                        Dim bmtUserId As String = DB.GetData(sql)

                        If bmtUserId IsNot Nothing Then

                            'delete the user's zoom data
                            sql = "UPDATE Users SET ZoomUserId = NULL, ZoomAccessToken = NULL, ZoomRefreshToken = NULL WHERE id = '" & bmtUserId & "'"
                            DB.Update(sql)

                            'send zoom compliance
                            Dim requestUri As String = "https://api.zoom.us/oauth/data/compliance"
                            Dim requestHeaders = New NameValueCollection() From {{"Authorization", "Basic " & API.EncodedClientCredentialsProduction}}
                            Dim result As Response(Of DeauthorizationPayload) = API.Request(Of DeauthorizationPayload, Compliance)(requestUri, "post", requestHeaders, New Compliance() With
                            {
                                .ClientId = WebConfigurationManager.AppSettings("ZoomKeyProduction"), 'zoom only allows deauthorization w/ production credentials
                                .AccountId = payload.AccountId,
                                .UserId = payload.UserId,
                                .ComplianceCompleted = True,
                                .DeauthorizationEventReceived = payload
                            })

                            'update users table with result
                            Dim resultString = If(result.Error Is Nothing, API.SerializeData(result.Data),
                                "Error: " & API.SerializeData(result.Error) & " Intial Notification: " & API.SerializeData(payload))

                            sql = "UPDATE Users SET ZoomDeauthorizationResult = '" & resultString & "' WHERE id = '" & bmtUserId & "'"
                            DB.Update(sql)

                        End If
                    End If
                End If

            Catch ex As Exception
                Dim err As String = ex.Message
            End Try

        End If
    End Sub
End Class