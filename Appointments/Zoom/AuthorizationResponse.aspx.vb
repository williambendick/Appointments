Public Class AuthorizationResponse
    Inherits Page

    Protected Sub Page_Load(sender As Object, e As EventArgs)

        Dim result As String

        Try

            'direct to site root if URL does not include zoom parameters or user validation cookie does not exist
            If String.IsNullOrWhiteSpace(Request.QueryString("state")) OrElse Request.Cookies("ZUV") Is Nothing OrElse String.IsNullOrWhiteSpace(Request.Cookies("ZUV").Value) Then
                result = "error"
            Else
                Dim userValidationId As String = Request.QueryString("state").Split("|"c)(0)
                Dim action As String = Request.QueryString("state").Split("|"c)(1)

                'ensure that user id returned from zoom matches user cookie created when starting authorization process
                If userValidationId = Request.Cookies("ZUV").Value Then
                    Zoom.API.InitializeTokens(Request.QueryString("code"))
                    result = "success"

                    Select Case action
                        Case "create-meeting"
                        Case Else
                    End Select

                Else
                    result = "error"
                End If

            End If

        Catch ex As Exception
            result = "error"
        End Try

        HttpContext.Current.Response.Redirect($"~/Zoom/TestUI.aspx?authorizationResult={result}")
    End Sub

End Class
