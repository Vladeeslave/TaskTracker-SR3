using TaskTracker.Application;
using TaskTracker.Domain;

namespace TaskTracker.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_DoesNotReferenceOtherLayers()
    {
        // Act
        IEnumerable<string> links = ReferencedProjects(typeof(WorkTask));

        // Assert
        Assert.Empty(links);
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructureOrCli()
    {
        // Act
        IEnumerable<string> links = ReferencedProjects(typeof(TaskBoardService));

        // Assert
        string[] forbidden = { "Infrastructure", "Cli" };
        Assert.DoesNotContain(
            links,
            name => forbidden.Any(suffix => name.EndsWith(suffix, StringComparison.Ordinal)));
    }

    private static IEnumerable<string> ReferencedProjects(Type anyTypeOfAssembly) =>
        anyTypeOfAssembly.Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name!)
            .Where(name => name.StartsWith("TaskTracker.", StringComparison.Ordinal));
}
