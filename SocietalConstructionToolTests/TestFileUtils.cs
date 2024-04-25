namespace SocietalConstructionToolTests
{
    public static class TestFileUtils
    {
        // yield all files in some subdirectory of the TestFiles directory
        public static IEnumerable<string[]> GetTestFiles(string dir) =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", dir))
                .Select(f => new[] { f });
    }
}
