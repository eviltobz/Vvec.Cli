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
    void AbortApplication(params object[] items);

    public interface IPrompt
    {
        IPrompt AddLine(params object[] items);
        /// <param name="customPrompt">Defaults to "Press any key to continue." if not overridden here.</param>
        void PressAnyKey(string? customPrompt = null);
        string GetFreeText(Predicate<string?>? validator = null);
        (bool isDefault, string value) GetFreeTextOrDefault(string defaultValue, Predicate<string?>? validator = null);
        //TEnum GetEnumSelection<TEnum>(
        //    TEnum? defaultValue = null,
        //    Colour? optionsColour = null,
        //    Colour? defaultValueColour = null)
        //    where TEnum : struct, Enum;
        TEnum GetEnumSelection<TEnum>(
            TEnum? defaultValue = null,
            AnsiCode? optionsColour = null,
            AnsiCode? defaultValueColour = null)
            where TEnum : struct, Enum;

        YesNo GetConfirmation(AnsiCode? optionsColour = null, bool caseSensitive = false);
        //YesNo GetConfirmation(Colour? optionsColour = null, bool caseSensitive = false);
    }

    public interface IAppendableLine
    {
        IAppendableLine Write(params object[] items);
        IAppendableLine StartSpinner();
        IAppendableLine StartEllipsis();
        IStatus StartStatus(params object[] items);
    }

    public interface IStatus
    {
        IStatus Update(params object[] items);
        IStatus WithSpinner();
        IStatus WithEllipsis();
        IAppendableLine Finish();
        IAppendableLine Finish(params object[] items);
    }
}
