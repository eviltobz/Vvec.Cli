using System.Text;

namespace Vvec.Cli.SourceGenerator
{
    public static class SourceGenerationHelper
    {
        public const string Attribute = @"
namespace Vvec.Sg
{
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class EnumExtensionsAttribute : System.Attribute
    {
    }
}";


        public static string GenerateExtensionClass(List<EnumToGenerate> enumsToGenerate)
        {
            var sb = new StringBuilder();
            sb.Append(@"
namespace Vvec.Sg
{
    public static partial class EnumExtensions
    {");
            foreach (var enumToGenerate in enumsToGenerate)
            {
                sb.Append(@"
                public static string ToStringFast(this ").Append(enumToGenerate.Name).Append(@" value)
                    => value switch
                    {");
                foreach (var member in enumToGenerate.Values)
                {
                    sb.Append(@"
                ").Append(enumToGenerate.Name).Append('.').Append(member)
                        .Append(" => \"sg:\" + nameof(")
                        .Append(enumToGenerate.Name).Append('.').Append(member).Append("),");
                }

                sb.Append(@"
                    _ => value.ToString(),
                };
");
            }

            sb.Append(@"
    }
}");

            return sb.ToString();
        }
    }
}
