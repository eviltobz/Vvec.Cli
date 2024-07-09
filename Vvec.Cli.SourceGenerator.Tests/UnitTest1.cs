using Microsoft.CodeAnalysis;
using VvecCli.SourceGenerator.Tests.Utils;
using ZeroIoC.Tests.Data;

namespace Vvec.Cli.SourceGenerator.Tests
{
    public class Tests
    {
        [Test]
        public async Task CanCompile____()
        {
            var project = TestProject.Project;

            var newProject = await project.ApplyCommandRegistrationGenerator();

            var compilation = await newProject.GetCompilationAsync();
            var errors = compilation.GetDiagnostics()
                .Where(o => o.Severity == DiagnosticSeverity.Error)
                .ToArray();

            Assert.Zero(errors.Count(), errors.Select(o => o.GetMessage()).JoinWithNewLine());
        }
    }
}