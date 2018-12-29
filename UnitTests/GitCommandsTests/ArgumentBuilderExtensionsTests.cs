using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests
{
    public sealed class ArgumentBuilderExtensionsTests
    {
        [Test]
        public void Adds_conditional_parameters()
        {
            Test(
                "foo true",
                new ArgumentBuilder
                {
                    "foo",
                    { true, "true" }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { false, "true" }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { true, "" }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { true, (string)null }
                });

            void Test(string expected, ArgumentBuilder command)
            {
                Assert.AreEqual(expected, command.ToString());
            }
        }

        [Test]
        public void Adds_conditional_if_else_parameters()
        {
            Test(
                "foo true",
                new ArgumentBuilder
                {
                    "foo",
                    { true, "true", "false" }
                });

            Test(
                "foo false",
                new ArgumentBuilder
                {
                    "foo",
                    { false, "true", "false" }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { true, "", "" }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { false, "", "" }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { true, null, null }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { false, null, null }
                });

            void Test(string expected, ArgumentBuilder command)
            {
                Assert.AreEqual(expected, command.ToString());
            }
        }

        [Test]
        public void Adds_enumerable_parameters()
        {
            Test(
                "foo a b c",
                new ArgumentBuilder
                {
                    "foo",
                    new[] { "a", "b", "c" }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    Enumerable.Empty<string>()
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    (IEnumerable<string>)null
                });

            void Test(string expected, ArgumentBuilder command)
            {
                Assert.AreEqual(expected, command.ToString());
            }
        }

        [Test]
        public void Adds_conditional_enumerable_parameters()
        {
            Test(
                "foo a b c",
                new ArgumentBuilder
                {
                    "foo",
                    { true, new[] { "a", "b", "c" } }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { false, new[] { "a", "b", "c" } }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { true, Enumerable.Empty<string>() }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { false, Enumerable.Empty<string>() }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { true, (IEnumerable<string>)null }
                });

            Test(
                "foo",
                new ArgumentBuilder
                {
                    "foo",
                    { false, (IEnumerable<string>)null }
                });

            void Test(string expected, ArgumentBuilder command)
            {
                Assert.AreEqual(expected, command.ToString());
            }
        }

        [Test]
        public void Handles_all_enum_members()
        {
            Test<ForcePushOptions>();
            Test<UntrackedFilesMode>();
            Test<IgnoreSubmodulesMode>();
            Test<GitBisectOption>();

            void Test<T>()
            {
                var method = typeof(ArgumentBuilderExtensions).GetMethod(
                    nameof(ArgumentBuilderExtensions.Add),
                    new[]
                    {
                        typeof(ArgumentBuilder),
                        typeof(T)
                    });

                Assert.NotNull(method);

                foreach (T member in Enum.GetValues(typeof(T)))
                {
                    var args = new ArgumentBuilder();

                    Assert.DoesNotThrow(() => method.Invoke(null, new object[] { args, member }));
                }
            }
        }

        [Test]
        public void Handle_artificial_objectid()
        {
            Assert.Throws<ArgumentException>(() => new ArgumentBuilder
            {
                ObjectId.WorkTreeId
            });
            Assert.Throws<ArgumentException>(() => new ArgumentBuilder
            {
                ObjectId.IndexId
            });
            Assert.Throws<ArgumentException>(() => new ArgumentBuilder
            {
                ObjectId.CombinedDiffId
            });
        }

        [TestCase(null)]
        public void Handle_null_objectid(ObjectId id)
        {
            var args = new ArgumentBuilder
            {
                id
            };
            Assert.AreEqual(args.ToString(), "");
        }

        [Test]
        public void Add_ForcePushOptions_ensure_all_switches_are_handled()
        {
            foreach (int mode in Enum.GetValues(typeof(ForcePushOptions)))
            {
                // unimplemented switches will result in InvalidEnumArgumentException
                var args = new ArgumentBuilder
                {
                    (ForcePushOptions)mode
                };
            }
        }

        [Test]
        public void Add_GitBisectOption_ensure_all_switches_are_handled()
        {
            foreach (int mode in Enum.GetValues(typeof(GitBisectOption)))
            {
                // unimplemented switches will result in InvalidEnumArgumentException
                var args = new ArgumentBuilder
                {
                    (GitBisectOption)mode
                };
            }
        }

        [Test]
        public void Add_IgnoreSubmodulesMode_ensure_all_switches_are_handled()
        {
            foreach (int mode in Enum.GetValues(typeof(IgnoreSubmodulesMode)))
            {
                // unimplemented switches will result in InvalidEnumArgumentException
                var args = new ArgumentBuilder
                {
                    (IgnoreSubmodulesMode)mode
                };
            }
        }

        [Test]
        public void Add_CleanMode_ensure_all_switches_are_handled()
        {
            foreach (int mode in Enum.GetValues(typeof(CleanMode)))
            {
                // unimplemented switches will result in InvalidEnumArgumentException
                var args = new ArgumentBuilder
                {
                    (CleanMode)mode
                };
            }
        }

        [Test]
        public void Add_ResetMode_ensure_all_switches_are_handled()
        {
            foreach (int mode in Enum.GetValues(typeof(ResetMode)))
            {
                // unimplemented switches will result in InvalidEnumArgumentException
                var args = new ArgumentBuilder
                {
                    (ResetMode)mode
                };
            }
        }
    }
}