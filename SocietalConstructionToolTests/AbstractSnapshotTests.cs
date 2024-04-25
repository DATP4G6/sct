namespace SocietalConstructionToolTests
{
    public abstract class AbstractSnapshotTests : VerifyBase
    {
        // yield all files in some subdirectory of the TestFiles directory
        protected static IEnumerable<string[]> GetTestFiles(string dir) => TestFileUtils.GetTestFiles(dir);


        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void Setup(TestContext _)
        {
            DiffEngine.DiffRunner.Disabled = true; // avoid destroying your terminal
        }
    }
}
