
Imports System.IO
Imports System.Web.Configuration
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Calendar.v3
Imports Google.Apis.Calendar.v3.Data
Imports Google.Apis.Services

Public Class GoogleEvent
    Public Property Summary As String
    Public Property Location As String
    Public Property StartTime As Date
    Public Property EndTime As Date
    Public Property Result As GoogleEventResult = New GoogleEventResult()

    Public Sub Create()

        Try
            Dim service As CalendarService = CreateCalendarService()

            Dim evt As [Event] = New [Event] With {
                .Summary = Summary,
                .Location = Location,
                .Start = New EventDateTime() With {.DateTime = StartTime},
                .End = New EventDateTime() With {.DateTime = EndTime}
            }

            Dim result As [Event] = service.Events.Insert(evt, WebConfigurationManager.AppSettings("GoogleCalendarId")).Execute()
            Me.Result.Id = result.Id

        Catch e As Exception
            Result.Error = e.Message
        End Try

    End Sub

    Public Sub Update(calendarEventId As String)

        Try
            Dim service As CalendarService = CreateCalendarService()

            Dim evt As [Event] = New [Event] With {.Summary = Summary}

            Dim result As [Event] = service.Events.Patch(evt, WebConfigurationManager.AppSettings("GoogleCalendarId"), calendarEventId).Execute()
            Me.Result.Id = result.Id

        Catch e As Exception
            Result.Error = e.Message
        End Try

    End Sub

    Private Function CreateCalendarService() As CalendarService

        Dim clientKeyPath As String = HttpContext.Current.Server.MapPath("/") & "client-key.json"
        Dim serviceScopes As String() = {CalendarService.Scope.Calendar}
        Dim credential As ServiceAccountCredential

        Using stream = New FileStream(clientKeyPath, FileMode.Open, FileAccess.Read)
            Dim clientKey = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize(Of JsonCredentialParameters)(stream)
            credential = New ServiceAccountCredential(New ServiceAccountCredential.Initializer(clientKey.ClientEmail) With {
                .Scopes = serviceScopes
            }.FromPrivateKey(clientKey.PrivateKey))
        End Using

        Return New CalendarService(New BaseClientService.Initializer() With {
            .HttpClientInitializer = credential,
            .ApplicationName = "Appointments"
        })

    End Function
End Class

Public Class GoogleEventResult
    Public Property Id As String
    Public Property [Error] As String
End Class

