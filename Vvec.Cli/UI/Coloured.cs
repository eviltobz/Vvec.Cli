namespace Vvec.Cli.UI
{
    public record Coloured
    {
        public string? Value { get; init; }
        public Colour? Foreground { get; init; }
        public Colour? Background { get; init; }

        public Coloured(object? value, Colour? foreground = null, Colour? background = null)
        {
            if (value is Coloured coloured)
            {
                Value = coloured.Value;
                Foreground = foreground ?? coloured.Foreground;
                Background = background ?? coloured.Background;
            }
            else
            {
                Value = value?.ToString();
                Foreground = foreground;
                Background = background;
            }
        }
    }
}
