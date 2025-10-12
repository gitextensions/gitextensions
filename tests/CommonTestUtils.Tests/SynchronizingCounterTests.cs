using FluentAssertions;

namespace CommonTestUtils.Tests
{
    [TestFixture]
    public class SynchronizingCounterTests
    {
        private static readonly TimeSpan TestDelay = TimeSpan.FromSeconds(0.6);
        private static readonly TimeSpan NoOpDelay = TimeSpan.FromSeconds(0.1);
        private static readonly TimeSpan FailDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan BufferTime = TimeSpan.FromSeconds(0.05);

        private DelayedAction After(TimeSpan delay) => new(delay + BufferTime);

        [Test]
        public void Decrement_should_not_throw_when_non_zero()
        {
            // Arrange
            SynchronizingCounter sut = new();

            sut.Increment();

            // Act & Assert
            sut.Decrement();
        }

        [Test]
        public void Decrement_should_throw_when_at_0()
        {
            // Arrange
            SynchronizingCounter sut = new();

            sut.Increment();

            // Act & Assert
            sut.Decrement();

            Action testAction = () => sut.Decrement();

            testAction.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void WaitForZero_should_wait()
        {
            // Arrange
            SynchronizingCounter sut = new();

            sut.Increment();
            sut.Increment();

            After(TestDelay / 2).Do(sut.Decrement);
            After(TestDelay).Do(sut.Decrement);

            // Act & Assert
            sut.ExecutionTimeOf(s => s.WaitForZero())
                .Should().BeGreaterThan(TestDelay).And.BeLessThan(FailDelay);
        }

        [Test]
        public void WaitForZero_should_return_immediately_if_already_at_zero()
        {
            // Arrange
            SynchronizingCounter sut = new();

            sut.Increment();
            sut.Decrement();

            // Act & Assert
            sut.ExecutionTimeOf(s => s.WaitForZero())
                .Should().BeLessThan(NoOpDelay);
        }
    }
}
