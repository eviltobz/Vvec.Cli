namespace Vvec.Cli.UI;
public interface IConsole
{
    IConsole Verbose { get; }
    IConsole Write(params object[]? items);
    IConsole WriteLine(params object[]? items);
    IConsole WriteLine();
    IConsole ClearLine();
    IPrompt StartPrompt(params object[] items);
    IAppendableLine StartAppendable(params object[] items);
    void Abort(params object[] items);

    public interface IPrompt
    {
        IPrompt AddLine(params object[] items);
        /// <param name="customPrompt">Defaults to "Press any key to continue." if not overridden here.</param>
        void PressAnyKey(string? customPrompt = null);
        string GetFreeText(Predicate<string?>? validator = null);
        (bool isDefault, string value) GetFreeTextOrDefault(string defaultValue, Predicate<string?>? validator = null);
        TEnum GetEnumSelection<TEnum>(
            TEnum? defaultValue = null,
            Colour? optionsColour = null,
            Colour? defaultValueColour = null)
            where TEnum : struct, Enum;
        YesNo GetConfirmation(Colour? optionsColour = null, bool caseSensitive = false);
    }

    public interface IAppendableLine
    {
        IAppendableLine Write(params object[] items);
        IAppendableLine StartSpinner();
    }
}
