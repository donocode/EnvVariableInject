<AttributeUsage(AttributeTargets.Field)>
Class BuildTimeEnvironmentVariableAttribute
    Inherits Attribute
    Private _environmentVariable As String

    Public Sub New(ByVal environmentVariable As String)
        _environmentVariable = environmentVariable
    End Sub
End Class
