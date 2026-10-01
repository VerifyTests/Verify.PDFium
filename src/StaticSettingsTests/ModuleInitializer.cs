public static class ModuleInitializer
{
    #region InitializeOutputs

    [ModuleInitializer]
    public static void Initialize() =>
        VerifyPDFium.Initialize(outputs: PdfiumOutputs.Png);

    #endregion

    [ModuleInitializer]
    public static void InitializeOther() =>
        VerifierSettings.UseSsimForPng();
}
