namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Identificadores de los campos básicos de encabezados del mensaje.
    /// </summary>
    public enum AdaptiveMessageFieldID
    {
        APIToken = 1,
        HashRules,
        ResponseCode,
        ResponseMessage,
        ModuleName,
        FunctionName,
        IsEnumerable,
        EnumerableCount,
        CurrentPosition,
        EnumerableOperation
    }
}