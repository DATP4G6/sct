using Moq;

using Sct.Runtime;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class RuntimeTests
    {
        private static Mock<IRuntimeContext> GetContextMock()
        {
            var contextMock = new Mock<IRuntimeContext>();
            // GetNextContext should just return the mocked context again
            _ = contextMock.Setup(c => c.GetNextContext())
                .Returns(contextMock.Object);

            return contextMock;
        }

        [TestMethod]
        public void TestRuntimeExits()
        {
            // Arrange
            var contextMock = GetContextMock();

            _ = contextMock.SetupSequence(c => c.ShouldExit)
                .Returns(false)
                .Returns(true);

            _ = contextMock.Setup(c => c.AgentHandler.Agents).Returns([]);

            var runtime = new Runtime();

            // Act
            runtime.Run(contextMock.Object);

            // Assert
            contextMock.Verify(c => c.ShouldExit, Times.Exactly(2));
            contextMock.Verify(c => c.GetNextContext(), Times.Once);
            contextMock.Verify(c => c.AgentHandler.Agents, Times.Once);
            contextMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestRuntimeUpdatesAgent()
        {
            // Arrange
            var contextMock = GetContextMock();
            _ = contextMock.SetupSequence(c => c.ShouldExit)
                .Returns(false)
                .Returns(true);

            var agentMock = new Mock<BaseAgent>("", new Dictionary<string, dynamic>());
            _ = contextMock.Setup(c => c.AgentHandler.Agents).Returns([agentMock.Object]);

            var runtime = new Runtime();

            // Act
            runtime.Run(contextMock.Object);

            // Assert
            agentMock.Verify(a => a.Update(contextMock.Object), Times.Once);
            agentMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [Ignore("Exit not yet implemented")]
        public void TestRuntimeContextExits()
        {
            // Arrange
            var context = new RuntimeContext();

            // Assume
            Assert.IsFalse(context.ShouldExit);

            // Act
            context.ExitRuntime();

            // Assert
            Assert.IsTrue(context.ShouldExit);
        }
    }
}