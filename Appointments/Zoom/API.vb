Imports System.IO
Imports System.Net
Imports System.Web.Configuration
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Namespace Zoom
    Public Class API
        Shared ReadOnly encodedClientCredentials As String

        Shared Sub New()
            Dim credentialBytes As Byte() = Encoding.ASCII.GetBytes(WebConfigurationManager.AppSettings("ZoomKey") & ":" & WebConfigurationManager.AppSettings("ZoomSecret"))
            encodedClientCredentials = Convert.ToBase64String(credentialBytes)
        End Sub

        'redirect user to zoom site to authorize this application
        Friend Shared Sub RequestUserAuthorization(argument As String)
            Dim guid As String = System.Guid.NewGuid.ToString()

            'add validation cookie to user's browser. when zoom sends authorization response ensure there is a match
            Dim cookie As HttpCookie = New HttpCookie("ZUV", guid)
            cookie.Expires = Date.Now.AddMinutes(5)
            cookie.HttpOnly = True
            cookie.Secure = True
            HttpContext.Current.Response.Cookies.Add(cookie)

            Dim zoomUri As String = "https://zoom.us/oauth/authorize?response_type=code" &
                "&client_id=" & WebConfigurationManager.AppSettings("ZoomKey") &
                "&state=" & guid & "|" & argument &
                "&redirect_uri=" & WebConfigurationManager.AppSettings("ZoomRedirectURI")

            Uri.EscapeUriString(zoomUri)

            HttpContext.Current.Response.Redirect(zoomUri)

        End Sub

        'get initial access token from zoom after user has authorized application
        Friend Shared Sub InitializeTokens(code As String)

            Dim requestUri As String = "https://zoom.us/oauth/token?grant_type=authorization_code" &
                "&code=" & code &
                "&redirect_uri=" & WebConfigurationManager.AppSettings("ZoomRedirectURI")

            Uri.EscapeUriString(requestUri)

            Dim requestHeaders = New NameValueCollection() From {
                {"Authorization", "Basic " & encodedClientCredentials}
            }

            Dim result As Response(Of TokenResponse) = Request(Of TokenResponse, Object)(requestUri, "post", requestHeaders, Nothing)

            If result.Error Is Nothing Then
                SetTokens(result.Data.AccessToken, result.Data.RefreshToken)
            Else
                Throw New Exception(result.[Error].Message)
            End If

        End Sub

        'refresh expired tokens
        Friend Shared Function RefreshToken() As String

            Dim refreshedToken As String = GetToken("refresh")

            If refreshedToken Is Nothing Then Throw New Exception("refresh token does not exist.")

            Dim requestUri As String = "https://zoom.us/oauth/token?grant_type=refresh_token&refresh_token=" & refreshedToken

            Dim requestHeaders = New NameValueCollection() From {
                {"Authorization", "Basic " & encodedClientCredentials}
            }

            Dim result As Response(Of TokenResponse) = Request(Of TokenResponse, Object)(requestUri, "post", requestHeaders, Nothing)

            If result.Error Is Nothing Then
                SetTokens(result.Data.AccessToken, result.Data.RefreshToken)
                Return result.Data.AccessToken
            Else
                Throw New Exception(result.Error.Message)
            End If

        End Function

        'store access and refresh tokens in cookies (these tokens could be stored in DB instead)
        Private Shared Sub SetTokens(accessToken As String, refreshToken As String)

            Dim zat As HttpCookie = New HttpCookie("ZAT", accessToken)
            zat.Expires = Date.Now.AddYears(15)
            zat.HttpOnly = True
            zat.Secure = True
            HttpContext.Current.Response.Cookies.Add(zat)

            Dim zrt As HttpCookie = New HttpCookie("ZRT", refreshToken)
            zrt.Expires = Date.Now.AddYears(15)
            zrt.HttpOnly = True
            zrt.Secure = True
            HttpContext.Current.Response.Cookies.Add(zrt)

        End Sub

        'get user's access or refresh token if it exists
        Friend Shared Function GetToken(tokenType As String) As String

            Dim cookieName As String = If(tokenType.ToLower() = "access", "ZAT", If(tokenType = "refresh", "ZRT", Nothing))

            If cookieName IsNot Nothing AndAlso HttpContext.Current.Request.Cookies(cookieName) IsNot Nothing _
                AndAlso String.IsNullOrWhiteSpace(HttpContext.Current.Request.Cookies(cookieName).Value) = False Then
                Return HttpContext.Current.Request.Cookies(cookieName).Value
            End If

            Return Nothing

        End Function

        'make zoom request 
        Friend Shared Function Request(Of Tresponse, Trequest)(uri As String, httpMethod As String, headers As NameValueCollection, requestObject As Trequest) As Response(Of Tresponse)

            Dim response As Response(Of Tresponse) = New Response(Of Tresponse)()
            Dim webRequest As HttpWebRequest = CType(webRequest.Create(uri), HttpWebRequest)
            webRequest.Method = httpMethod
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

        Private Shared Function SerializeData(data As Object) As String

            Dim contractResolver As DefaultContractResolver = New DefaultContractResolver With {
                .NamingStrategy = New SnakeCaseNamingStrategy()
            }

            Return JsonConvert.SerializeObject(data, New JsonSerializerSettings With {
                .NullValueHandling = NullValueHandling.Ignore,
                .DateFormatString = "yyyy-MM-ddTHH:mm:ss",
                .ContractResolver = contractResolver
            })

        End Function

        Private Shared Function DeserializeData(Of T)(data As String) As T

            Dim contractResolver As DefaultContractResolver = New DefaultContractResolver With {
                .NamingStrategy = New SnakeCaseNamingStrategy()
            }

            Return JsonConvert.DeserializeObject(Of T)(data, New JsonSerializerSettings With {
                .ContractResolver = contractResolver
            })

        End Function

    End Class
End Namespace
