using Moq;

using Sct.Runtime;
using Sct.Runtime.Trace;

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

            _ = contextMock.Setup(c => c.AgentHandler.Agents).Returns([]);

            return contextMock;
        }
        private static Mock<BaseAgent> GetAgentMock() => new("", new Dictionary<string, dynamic>(), 1);

        [TestMethod]
        public void TestRuntimeExits()
        {
            // Arrange
            var contextMock = GetContextMock();
            var agentMock = GetAgentMock();

            _ = contextMock.SetupSequence(c => c.ShouldExit)
                .Returns(false)
                .Returns(true);

            _ = contextMock.Setup(c => c.AgentHandler.Agents).Returns([agentMock.Object]);

            var runtime = new Runtime();

            // Act
            runtime.Run(contextMock.Object);

            // Assert
            contextMock.Verify(c => c.ShouldExit, Times.Exactly(2));
            contextMock.Verify(c => c.GetNextContext(), Times.Once);
            contextMock.Verify(c => c.AgentHandler.Agents, Times.Exactly(2));
            contextMock.Verify(c => c.OnTick(), Times.Exactly(2));
            contextMock.Verify(c => c.OnExit(), Times.Once);
            contextMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestRuntimeExitsWhenNoAgents()
        {
            // Arrange
            var contextMock = GetContextMock();
            var agentMock = GetAgentMock();

            // Need to return an agent twice.
            // First time when checking while expression, second time when updating agents inside loop
            // This stops after a single iteration
            _ = contextMock.SetupSequence(c => c.AgentHandler.Agents)
                .Returns([agentMock.Object])
                .Returns([agentMock.Object])
                .Returns([]);

            // This only exists to ensure the loop doesn't run forever if the simulation doesn't exit properly
            // If it fails, this ensures the test stops after the 2nd iteration
            _ = contextMock.SetupSequence(c => c.ShouldExit)
                .Returns(false)
                .Returns(false)
                .Returns(true);

            var runtime = new Runtime();

            // Act
            runtime.Run(contextMock.Object);

            // Assert
            contextMock.Verify(c => c.ShouldExit, Times.Exactly(2));
            contextMock.Verify(c => c.GetNextContext(), Times.Once);
            contextMock.Verify(c => c.AgentHandler.Agents, Times.Exactly(3));
            contextMock.Verify(c => c.OnTick(), Times.Exactly(2));
            contextMock.Verify(c => c.OnExit(), Times.Once);
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

            var agentMock = GetAgentMock();
            _ = contextMock.Setup(c => c.AgentHandler.Agents).Returns([agentMock.Object]);

            var runtime = new Runtime();

            // Act
            runtime.Run(contextMock.Object);

            // Assert
            agentMock.Verify(a => a.Update(contextMock.Object), Times.Once);
            agentMock.VerifyNoOtherCalls();
        }

        [TestMethod]
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

        [TestMethod]
        public void TestRuntimeLogsTicks()
        {
            // Arrange
            var contextMock = GetContextMock();
            _ = contextMock.SetupSequence(c => c.ShouldExit)
                .Returns(false)
                .Returns(true);
            _ = contextMock.Setup(c => c.OnTick()).CallBase();
            _ = contextMock.Setup(c => c.OnExit()).CallBase();

            var loggerMock = new Mock<IOutputLogger>();
            _ = contextMock.Setup(c => c.OutputLogger).Returns(loggerMock.Object);

            // Add agent mock so it doesn't stop the runtime early
            var agentMock = GetAgentMock();
            _ = contextMock.Setup(c => c.AgentHandler.Agents).Returns([agentMock.Object]);

            var runtime = new Runtime();

            // Act
            runtime.Run(contextMock.Object);

            // Assert
            loggerMock.Verify(l => l.OnTick(contextMock.Object), Times.Exactly(2));
            loggerMock.Verify(l => l.OnExit(), Times.Once);
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
