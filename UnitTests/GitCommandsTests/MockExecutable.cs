using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using NUnit.Framework;

namespace GitCommandsTests
{
    internal sealed class MockExecutable : IExecutable
    {
        private readonly ConcurrentDictionary<string, ConcurrentStack<(string output, int? exitCode)>> _outputStackByArguments = new ConcurrentDictionary<string, ConcurrentStack<(string output, int? exitCode)>>();
        private readonly ConcurrentDictionary<string, int> _commandArgumentsSet = new ConcurrentDictionary<string, int>();
        private readonly List<MockProcess> _processes = new List<MockProcess>();
        private int _nextCommandId;

        [MustUseReturnValue]
        public IDisposable StageOutput(string arguments, string output, int? exitCode = 0)
        {
            var stack = _outputStackByArguments.GetOrAdd(
                arguments,
                args => new ConcurrentStack<(string output, int? exitCode)>());

            stack.Push((output, exitCode));

            return new DelegateDisposable(
                () =>
                {
                    if (_outputStackByArguments.TryGetValue(arguments, out var s) &&
                        s.TryPeek(out var r) &&
                        ReferenceEquals(output, r))
                    {
                        throw new AssertionException($"Staged output should have been consumed.\nArguments: {arguments}\nOutput: {output}");
                    }
                });
        }

        [MustUseReturnValue]
        public IDisposable StageCommand(string arguments)
        {
            var id = Interlocked.Increment(ref _nextCommandId);
            _commandArgumentsSet[arguments] = id;

            return new DelegateDisposable(
                () =>
                {
                    if (_commandArgumentsSet.TryGetValue(arguments, out var storedId) && storedId != id)
                    {
                        throw new AssertionException($"Staged command should have been consumed.\nArguments: {arguments}");
                    }
                });
        }

        public bool Exists()
        {
            return true;
        }

        public void Verify()
        {
            Assert.IsEmpty(_outputStackByArguments, "All staged output should have been consumed.");
            Assert.IsEmpty(_commandArgumentsSet, "All staged output should have been consumed.");

            foreach (var process in _processes)
            {
                process.Verify();
            }
        }

        public IProcess Start(ArgumentString arguments, bool createWindow, bool redirectInput, bool redirectOutput, Encoding outputEncoding)
        {
            if (_outputStackByArguments.TryRemove(arguments, out var queue) && queue.TryPop(out var item))
            {
                if (queue.Count == 0)
                {
                    _outputStackByArguments.TryRemove(arguments, out _);
                }

                var process = new MockProcess(item.output, item.exitCode);

                _processes.Add(process);
                return process;
            }

            if (_commandArgumentsSet.TryRemove(arguments, out _))
            {
                var process = new MockProcess();
                _processes.Add(process);
                return process;
            }

            throw new Exception("Unexpected arguments: " + arguments);
        }

        public string GetOutput(ArgumentString arguments)
        {
            if (_outputStackByArguments.TryRemove(arguments, out var queue) && queue.TryPop(out var item))
            {
                if (queue.Count == 0)
                {
                    _outputStackByArguments.TryRemove(arguments, out _);
                }

                return item.output;
            }

            if (_commandArgumentsSet.TryRemove(arguments, out _))
            {
                return "";
            }

            throw new Exception("Unexpected arguments: " + arguments);
        }

        private sealed class MockProcess : IProcess
        {
            public MockProcess([CanBeNull] string output, int? exitCode = 0)
            {
                StandardOutput = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(output ?? "")));
                StandardError = new StreamReader(new MemoryStream());
                StandardInput = new StreamWriter(new MemoryStream());
                _exitCode = exitCode;
            }

            public MockProcess()
            {
                StandardOutput = new StreamReader(new MemoryStream());
                StandardError = new StreamReader(new MemoryStream());
                StandardInput = new StreamWriter(new MemoryStream());
                _exitCode = 0;
            }

            private int? _exitCode;
            public StreamWriter StandardInput { get; }
            public StreamReader StandardOutput { get; }
            public StreamReader StandardError { get; }

            public int WaitForExit()
            {
                return ThreadHelper.JoinableTaskFactory.Run(() => WaitForExitAsync());
            }

            public Task<int> WaitForExitAsync()
            {
                if (_exitCode.HasValue)
                {
                    return Task.FromResult(_exitCode.Value);
                }
                else
                {
                    var cts = new CancellationTokenSource();
                    var ct = cts.Token;
                    cts.Cancel();
                    return Task.FromCanceled<int>(ct);
                }
            }

            public void WaitForInputIdle()
            {
                // TODO implement if needed
            }

            public void Dispose()
            {
                // TODO implement if needed
            }

            public void Verify()
            {
                // all output should have been read
                Assert.AreEqual(StandardOutput.BaseStream.Length, StandardOutput.BaseStream.Position);
                Assert.AreEqual(StandardError.BaseStream.Length, StandardError.BaseStream.Position);

                // no input should have been written (yet)
                Assert.AreEqual(0, StandardInput.BaseStream.Length);
            }
        }

        private sealed class DelegateDisposable : IDisposable
        {
            private readonly Action _disposeAction;

            public DelegateDisposable(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                _disposeAction();
            }
        }
    }
}