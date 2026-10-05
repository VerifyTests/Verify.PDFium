public static class ModuleInitializer
{
    #region InitializeOutputs

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyPDFium.Initialize();

        // For every test: no text, so only the pages are verified
        VerifierSettings.PageText(PageTextPlacement.None);
    }

    #endregion

    [ModuleInitializer]
    public static void InitializeOther() =>
        VerifierSettings.UseSsimForPng();
}
