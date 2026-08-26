Public Class Main

    Private WithEvents _MainTimer As New Timers.Timer(60000) ' mS
    Private _Controller As New System.ServiceProcess.ServiceController

    Protected Overrides Sub OnStart(ByVal args() As String)
        _Controller.ServiceName = My.Settings.ServiceName

        _MainTimer.AutoReset = True
        _MainTimer.Enabled = True
        _MainTimer.Start()
    End Sub

    Protected Overrides Sub OnStop()
        _MainTimer.AutoReset = True
        _MainTimer.Enabled = True
        _MainTimer.Stop()
    End Sub

    Private Sub _MainTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles _MainTimer.Elapsed
        Dim ServiceStopTime As Date = Date.MinValue
        Dim ServiceStopped As Boolean = False
        Dim InstanceLoop As Int32 = 0
        Dim StartStopTime As DateTime = Date.MinValue
        Dim LastStopTime As DateTime = Date.MinValue
        Dim ServiceName As String = My.Settings.ServiceName

        _MainTimer.Stop()

        ' Stop the Service
        For InstanceLoop = 1 To 4
            ' Grab some values
            Select Case InstanceLoop
                Case 1
                    StartStopTime = CDate(My.Settings.StartStopTime1)
                    LastStopTime = My.Settings.StartStopTime1LastDate
                Case 2
                    StartStopTime = CDate(My.Settings.StartStopTime2)
                    LastStopTime = My.Settings.StartStopTime2LastDate
                Case 3
                    StartStopTime = CDate(My.Settings.StartStopTime3)
                    LastStopTime = My.Settings.StartStopTime3LastDate
                Case 4
                    StartStopTime = CDate(My.Settings.StartStopTime4)
                    LastStopTime = My.Settings.StartStopTime4LastDate
            End Select

            If Now.ToShortTimeString > StartStopTime.ToShortTimeString And LastStopTime < Date.Now Then
                EventLog.WriteEntry("Start Stop Time " & InstanceLoop.ToString & " has elapsed", EventLogEntryType.Information)
                EventLog.WriteEntry("Stopping " & ServiceName & " Service", EventLogEntryType.Information)

                Try
                    If _Controller.CanStop And _Controller.Status = ServiceProcess.ServiceControllerStatus.Running Then
                        _Controller.Stop()
                    Else
                        EventLog.WriteEntry(ServiceName & " Service is " & _Controller.Status.ToString, EventLogEntryType.Error)
                    End If
                Catch ex As Exception
                    EventLog.WriteEntry("Could not stop " & ServiceName & " Service.  Error: " & ex.ToString, EventLogEntryType.Error)
                End Try

                ' Set the last date
                Select Case InstanceLoop
                    Case 1
                        My.Settings.StartStopTime1LastDate = Date.Now
                    Case 2
                        My.Settings.StartStopTime2LastDate = Date.Now
                    Case 3
                        My.Settings.StartStopTime3LastDate = Date.Now
                    Case 4
                        My.Settings.StartStopTime4LastDate = Date.Now
                End Select

                My.Settings.Save()

                ServiceStopped = True
                ServiceStopTime = Now
            End If
        Next

        ' Start the Service
        If ServiceStopped Then
            Do
                System.Threading.Thread.Sleep(New System.TimeSpan(0, 0, 0, 0, 100))
            Loop Until Now > DateAdd(DateInterval.Second, My.Settings.StopTimeSeconds, ServiceStopTime)

            EventLog.WriteEntry("Starting " & ServiceName & " Service")

            If _Controller.Status = ServiceProcess.ServiceControllerStatus.Stopped Then
                Try
                    _Controller.Start()
                Catch ex As Exception
                    EventLog.WriteEntry("Could not start " & ServiceName & " Service.  Error:" & ex.ToString, EventLogEntryType.Error)
                End Try

            Else
                EventLog.WriteEntry(ServiceName & " Service is " & _Controller.Status.ToString, EventLogEntryType.Error)
            End If
        End If

        _MainTimer.Start()
    End Sub

End Class
