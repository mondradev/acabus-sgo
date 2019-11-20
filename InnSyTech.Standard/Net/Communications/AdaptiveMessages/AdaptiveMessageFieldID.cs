namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Identificadores de los campos básicos de encabezados del mensaje.
    /// </summary>
    public enum AdaptiveMessageFieldID
    {
        APIToken = 1,
        ResponseCode,
        ResponseMessage,
        ModuleName,
        FunctionName,
        Count,
        Position
    }
}