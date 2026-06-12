public static class ModuleInitializer
{
    #region enable

    [ModuleInitializer]
    public static void Initialize() =>
        VerifyPDFium.Initialize();

    #endregion

    [ModuleInitializer]
    public static void InitializeOther()
    {
        VerifierSettings.InitializePlugins();
        VerifierSettings.UseSsimForPng();
    }
}
