Imports System.Data.SqlClient

Public Class DB

    Public Shared Function GetData(sql As String) As String

        Try

            Dim cs As String = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bmt;Integrated Security=True;Connect Timeout=30;Encrypt=False;" &
                "TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

            Using cn As New SqlConnection(cs)

                Try
                    Dim cmd As SqlCommand = New SqlCommand(sql, cn)
                    cn.Open()

                    Dim result = cmd.ExecuteScalar()

                    Return If(IsDBNull(result), Nothing, result)

                Catch ex As Exception
                    Dim s As String = ex.Message
                End Try
            End Using

        Catch ex As Exception
            Return ex.Message
        End Try

    End Function



    Public Shared Function Update(sql As String) As Integer

        Dim result As Integer

        Try

            Dim cs As String = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bmt;Integrated Security=True;Connect Timeout=30;Encrypt=False;" &
                "TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

            Using cn As New SqlConnection(cs)

                Try
                    Dim cmd As SqlCommand = New SqlCommand(sql, cn)
                    cn.Open()

                    result = cmd.ExecuteNonQuery()

                Catch ex As Exception
                    Dim s As String = ex.Message
                End Try
            End Using

        Catch ex As Exception
            Return -1
        End Try

        Return result

    End Function

End Class
