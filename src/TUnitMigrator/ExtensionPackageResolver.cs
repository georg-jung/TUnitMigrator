static class ExtensionPackageResolver
{
    static readonly string[] frameworkSuffixes =
    [
        ".MSTest",
        ".NUnit",
        ".Xunit",
        ".XunitV3"
    ];

    // Direct package replacements (exact package name mapping)
    static readonly Dictionary<string, string> directReplacements = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Xunit.Combinatorial"] = "GeorgJung.TUnit.PairwiseDataSource"
    };

    public static async Task<(string newPackage, NuGetVersion version)?> TryResolve(
        string packageName,
        List<PackageSource> sources,
        SourceCacheContext cache)
    {
        // Check for direct replacement first
        if (directReplacements.TryGetValue(packageName, out var directReplacement))
        {
            var version = await NuGetPackageChecker.GetLatestStableVersion(directReplacement, sources, cache);
            if (version != null)
            {
                return (directReplacement, version);
            }
        }

        // Check if this package has a framework-specific suffix
        var matchedSuffix = frameworkSuffixes
            .FirstOrDefault(suffix => packageName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

        if (matchedSuffix == null)
        {
            return null;
        }

        var baseName = packageName[..^matchedSuffix.Length];
        var tunitCandidate = $"{baseName}.TUnit";

        var version2 = await NuGetPackageChecker.GetLatestStableVersion(tunitCandidate, sources, cache);
        if (version2 != null)
        {
            return (tunitCandidate, version2);
        }

        return null;
    }
}
