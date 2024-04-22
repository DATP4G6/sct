namespace SocietalConstructionToolTests
{
    public abstract class AbstractSnapshotTests : VerifyBase
    {
        // yield all files in some subdirectory of the TestFiles directory
        protected static IEnumerable<string[]> GetTestFiles(string dir) =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", dir))
                .Select(f => new[] { f });


        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void Setup(TestContext _)
        {
            DiffEngine.DiffRunner.Disabled = true; // avoid destroying your terminal
        }
    }
}
