' Custom action to remove the commands of Visual Studio when uninstalling the add-in
Sub RemoveVSAddInCommands(ByVal ver)
    Set dte = CreateObject("VisualStudio.DTE." + ver)
    For Each ctrl In dte.CommandBars.Item("MenuBar").Controls
        If ctrl.Caption = "&GitExt" Then
            ctrl.Delete(false)
        ElseIf ctrl.Caption = "G&itExt" Then
            ctrl.Delete(false)
        End If
    Next
    For Each cmdBar In dte.CommandBars
        If cmdBar.Name = "GitExtensions" Then
            dte.Commands.RemoveCommandBar(cmdBar)
        End If
    Next
    For Each cmd In dte.Commands
        If InStr(cmd, "GitPlugin.Connect") <> 0 Then
            cmd.Delete()
        End If
    Next
    dte.Quit()
End Sub

' Custom action to remove the commands of Visual Studio 2005 when uninstalling the add-in
Function RemoveVS2005AddInCommands
    On Error Resume Next
    RemoveVSAddInCommands("8.0")
    RemoveVS2005AddInCommands = msiDoActionStatusSuccess
End Function

' Custom action to remove the commands of Visual Studio 2008 when uninstalling the add-in
Function RemoveVS2008AddInCommands
    On Error Resume Next
    RemoveVSAddInCommands("9.0")
    RemoveVS2008AddInCommands = msiDoActionStatusSuccess
End Function

' Custom action to remove the commands of Visual Studio 2010 when uninstalling the add-in
Function RemoveVS2010AddInCommands
    On Error Resume Next
    RemoveVSAddInCommands("10.0")
    RemoveVS2010AddInCommands = msiDoActionStatusSuccess
End Function

' Custom action to remove the commands of Visual Studio 2012 when uninstalling the add-in
Function RemoveVS2012AddInCommands
    On Error Resume Next
    RemoveVSAddInCommands("11.0")
    RemoveVS2012AddInCommands = msiDoActionStatusSuccess
End Function

' Custom action to remove the commands of Visual Studio 2013 when uninstalling the add-in
Function RemoveVS2013AddInCommands
    On Error Resume Next
    RemoveVSAddInCommands("12.0")
    RemoveVS2013AddInCommands = msiDoActionStatusSuccess
End Function
