Imports System.IO
Imports System.Net
Imports System.Web.Configuration
Imports Microsoft.IdentityModel.Tokens
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Public Class ZoomMeeting
    Public Property Topic As String
    Public Property StartTime As Date
    Public Property Duration As Integer

    Public Function Create() As ZoomResponse

        Dim zoomResponse As ZoomResponse = New ZoomResponse()
        Dim zoomToken As String = GetToken()
        Dim meetingData As String = SerializeData()

        Dim webRequest As HttpWebRequest = CType(webRequest.Create("https://api.zoom.us/v2/users/me/meetings"), HttpWebRequest)
        webRequest.Headers("authorization") = "Bearer " & zoomToken
        webRequest.Method = "POST"
        webRequest.ContentType = "application/json"
        webRequest.KeepAlive = False

        Dim meetingDataBytes As Byte() = Encoding.ASCII.GetBytes(meetingData)
        webRequest.ContentLength = meetingDataBytes.Length

        Using requestStream As Stream = webRequest.GetRequestStream()
            requestStream.Write(meetingDataBytes, 0, meetingDataBytes.Length)
            requestStream.Close()
        End Using

        Try
            Dim httpResponse As HttpWebResponse = CType(webRequest.GetResponse(), HttpWebResponse)
            Dim responseString As String = New StreamReader(httpResponse.GetResponseStream()).ReadToEnd()
            zoomResponse = JsonConvert.DeserializeObject(Of ZoomResponse)(responseString)

        Catch e As WebException
            Using exceptionResponse As WebResponse = e.Response
                Dim httpResponse As HttpWebResponse = CType(exceptionResponse, HttpWebResponse)
                zoomResponse.Error = New StreamReader(httpResponse.GetResponseStream()).ReadToEnd()
            End Using

        Catch e As Exception
            zoomResponse.Error = e.Message
        End Try

        Return zoomResponse
    End Function

    Private Function GetToken() As String

        Dim tokenHandler = New IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
        Dim secret = WebConfigurationManager.AppSettings("ZoomSecret")
        Dim key As Byte() = Encoding.ASCII.GetBytes(secret)

        Dim tokenDescriptor = New SecurityTokenDescriptor With {
            .Issuer = WebConfigurationManager.AppSettings("ZoomKey"),
            .Expires = Date.UtcNow.AddMinutes(90),
            .SigningCredentials = New SigningCredentials(New SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        }

        Dim token = tokenHandler.CreateToken(tokenDescriptor)
        Return tokenHandler.WriteToken(token)

    End Function

    Private Function SerializeData() As String

        Dim contractResolver As DefaultContractResolver = New DefaultContractResolver With {
            .NamingStrategy = New SnakeCaseNamingStrategy() 'convert properties like 'StartTime' to 'start_time'
        }

        Return JsonConvert.SerializeObject(Me, New JsonSerializerSettings With {
            .NullValueHandling = NullValueHandling.Ignore,
            .DateFormatString = "yyyy-MM-ddTHH:mm:ss",
            .ContractResolver = contractResolver
        })

    End Function
End Class

Public Class ZoomResponse
    Public Property Id As Long
    Public Property [Error] As String
End Class
