namespace SocietalConstructionToolTests
{
    public class AbstractSnapshotTests : VerifyBase
    {
        // yield all files in TestFiles directory
        protected static IEnumerable<string[]> Files =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles"))
            .Select(f => new[] { f });


        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void Setup(TestContext _)
        {
            DiffEngine.DiffRunner.Disabled = true; // avoid destroying your terminal
            UseProjectRelativeDirectory("Snapshots"); // save snapshots here
        }
    }
}
