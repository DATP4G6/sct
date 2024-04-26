using Argon;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public abstract class AbstractSnapshotTests : VerifyBase
    {
        // yield all files in some subdirectory of the TestFiles directory
        protected static IEnumerable<string[]> GetTestFiles(string dir) => TestFileUtils.GetTestFiles(dir);

        [AssemblyInitialize]
        public static void Initialize(TestContext _)
        {
            VerifierSettings.AddExtraSettings(s =>
            {
                s.DefaultValueHandling = DefaultValueHandling.Include;
                s.TypeNameHandling = TypeNameHandling.Auto;
            });
        }

        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void Setup(TestContext _)
        {
            DiffEngine.DiffRunner.Disabled = true; // avoid destroying your terminal
        }
    }
}
