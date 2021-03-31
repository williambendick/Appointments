Imports System.IO
Imports System.Net
Imports System.Web.Configuration
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Namespace Zoom
    Public Class API
        Public Shared ReadOnly EncodedClientCredentials As String
        Public Shared ReadOnly EncodedClientCredentialsProduction As String
        Public Shared ReadOnly BaseUrl As String = "https://api.zoom.us/v2"

        Shared Sub New()
            Dim credentialBytes As Byte() = Encoding.ASCII.GetBytes(WebConfigurationManager.AppSettings("ZoomKey") & ":" & WebConfigurationManager.AppSettings("ZoomSecret"))
            EncodedClientCredentials = Convert.ToBase64String(credentialBytes)

            Dim credentialBytesProduction As Byte() = Encoding.ASCII.GetBytes(WebConfigurationManager.AppSettings("ZoomKeyProduction") & ":" & WebConfigurationManager.AppSettings("ZoomSecretProduction"))
            EncodedClientCredentialsProduction = Convert.ToBase64String(credentialBytesProduction)

        End Sub

        'redirect user to zoom site to authorize this application
        Public Shared Sub RequestUserAuthorization(meeting As Meeting)

            Dim authorizationGuid As Guid = Guid.NewGuid

            meeting.AuthorizationId = authorizationGuid

            HttpContext.Current.Session("zoomMeeting") = meeting

            Dim zoomUri As String = "https://zoom.us/oauth/authorize?response_type=code" &
                "&client_id=" & WebConfigurationManager.AppSettings("ZoomKey") &
                "&state=" & authorizationGuid.ToString() &
                "&redirect_uri=" & WebConfigurationManager.AppSettings("ZoomRedirectURI")

            Uri.EscapeUriString(zoomUri)

            HttpContext.Current.Response.Redirect(zoomUri)

        End Sub

        Public Shared Function InitializeUser(bmtId As String, authorizationCode As String) As String

            Dim result As String
            Dim errMsg As String = "An error occurred when "

            'get initial zoom tokens for user
            Dim requestUri As String = "https://zoom.us/oauth/token?grant_type=authorization_code" &
                "&code=" & authorizationCode &
                "&redirect_uri=" & WebConfigurationManager.AppSettings("ZoomRedirectURI")

            Uri.EscapeUriString(requestUri)

            Dim requestHeaders = New NameValueCollection() From {{"Authorization", "Basic " & EncodedClientCredentials}}

            Dim tokenResponse As Response(Of TokenResponse) = Request(Of TokenResponse, Object)(requestUri, "post", requestHeaders, Nothing)

            If tokenResponse.Error IsNot Nothing Then Return errMsg & "getting initial tokens for user: " & tokenResponse.Error.Message

            'get user's zoom data
            Dim user As User = New User()

            result = user.Retrieve(tokenResponse.Data.AccessToken)

            If result <> "success" Then Return errMsg & "getting user's zoom data: " & result

            'set user's id and tokens in DB
            result = SetTokens(tokenResponse.Data.AccessToken, tokenResponse.Data.RefreshToken, bmtId, user.Id)

            If result <> "success" Then Return errMsg & "setting user's zoom id and tokens: " & result

            Return "success"

        End Function

        'refresh expired tokens
        Public Shared Function RefreshToken(bmtUserId As String) As String

            Dim refreshedToken As String = GetToken("refresh", bmtUserId)

            If refreshedToken = "token not found" Then Return refreshedToken

            Dim requestUri As String = "https://zoom.us/oauth/token?grant_type=refresh_token&refresh_token=" & refreshedToken

            Dim requestHeaders = New NameValueCollection() From {{"Authorization", "Basic " & EncodedClientCredentials}}

            Dim result As Response(Of TokenResponse) = Request(Of TokenResponse, Object)(requestUri, "post", requestHeaders, Nothing)

            If result.Error Is Nothing Then
                SetTokens(result.Data.AccessToken, result.Data.RefreshToken, bmtUserId)
                Return result.Data.AccessToken
            Else
                Throw New Exception(result.Error.Message)
            End If

        End Function

        Public Shared Function SetTokens(accessToken As String, refreshToken As String, bmtUserId As String, Optional zoomUserId As String = "") As String

            Try

                Dim sql As String = "UPDATE Users SET "

                If zoomUserId = "" Then
                    sql &= "ZoomAccessToken = '" & accessToken & "', ZoomRefreshToken = '" & refreshToken & "' "
                Else
                    sql &= "ZoomUserId = '" & zoomUserId & "', ZoomAccessToken = '" & accessToken & "', 
                        ZoomRefreshToken = '" & refreshToken & ", ZoomDeauthorizationResult = NULL' "
                End If

                sql &= "WHERE Id = '" & bmtUserId & "'"

                DB.Update(sql)

                Return "success"

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function

        'get user's access or refresh token if it exists
        Public Shared Function GetToken(tokenType As String, bmtUserId As String) As String

            Try

                Dim fieldName As String = If(tokenType.ToLower() = "access", "ZoomAccessToken", "ZoomRefreshToken")

                Dim sql As String = "SELECT " & fieldName & " FROM Users WHERE Id = '" & bmtUserId & "'"

                Dim result As String = DB.GetData(sql)

                If result Is Nothing Then result = "token not found"

                Return result

            Catch ex As Exception
                Throw New Exception("An error occurred when attempting to retrieve token: " & ex.Message)
            End Try

        End Function

        'make zoom request 
        Public Shared Function Request(Of Tresponse, Trequest)(uri As String, httpMethod As String, headers As NameValueCollection, requestObject As Trequest) As Response(Of Tresponse)

            '.NET 4.5 default SecurityProtocol Is Tls, but zoom requires Tls12
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim response As Response(Of Tresponse) = New Response(Of Tresponse)()
            Dim webRequest As HttpWebRequest = CType(webRequest.Create(uri), HttpWebRequest)
            webRequest.Method = httpMethod.ToUpper
            webRequest.Headers.Add(headers)
            webRequest.ContentType = "application/json"
            webRequest.KeepAlive = False

            If requestObject IsNot Nothing Then
                Dim requestData As String = SerializeData(requestObject)
                Dim requestBytes As Byte() = Encoding.ASCII.GetBytes(requestData)
                webRequest.ContentLength = requestBytes.Length

                Using requestStream As Stream = webRequest.GetRequestStream()
                    requestStream.Write(requestBytes, 0, requestBytes.Length)
                    requestStream.Close()
                End Using
            End If

            Try
                Dim httpResponse As HttpWebResponse = CType(webRequest.GetResponse(), HttpWebResponse)
                Dim responseString As String = New StreamReader(httpResponse.GetResponseStream()).ReadToEnd()
                response.Data = DeserializeData(Of Tresponse)(responseString)

            Catch wex As WebException

                response.Error = New ResponseError()

                Using exceptionResponse As WebResponse = wex.Response
                    Try
                        Dim httpResponse As HttpWebResponse = CType(exceptionResponse, HttpWebResponse)
                        Dim responseString As String = New StreamReader(httpResponse.GetResponseStream()).ReadToEnd()
                        response.Error = DeserializeData(Of ResponseError)(responseString)

                        If response.Error.Message Is Nothing Then
                            If response.Error.Reason IsNot Nothing Then
                                response.Error.Message = response.Error.Reason
                            Else
                                response.Error.Message = responseString
                            End If
                        End If

                    Catch x As Exception
                        response.Error.Message = wex.Message
                    End Try

                End Using

            Catch ex As Exception
                response.Error.Message = ex.Message
            End Try

            Return response
        End Function

        Public Shared Function SerializeData(data As Object) As String

            Dim contractResolver As DefaultContractResolver = New DefaultContractResolver With {
                .NamingStrategy = New SnakeCaseNamingStrategy()
            }

            Return JsonConvert.SerializeObject(data, New JsonSerializerSettings With {
                .NullValueHandling = NullValueHandling.Ignore,
                .DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                .ContractResolver = contractResolver
            })

        End Function

        Public Shared Function DeserializeData(Of T)(data As String) As T

            Dim contractResolver As DefaultContractResolver = New DefaultContractResolver With {
                .NamingStrategy = New SnakeCaseNamingStrategy()
            }

            Return JsonConvert.DeserializeObject(Of T)(data, New JsonSerializerSettings With {
                .ContractResolver = contractResolver
            })

        End Function

    End Class
End Namespace
