﻿using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Git;
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

            void Test<T>() where T : struct
            {
                var method = typeof(ArgumentBuilderExtensions).GetMethod(
                    nameof(ArgumentBuilderExtensions.Add),
                    new[]
                    {
                        typeof(ArgumentBuilder),
                        typeof(T?)
                    });

                Assert.NotNull(method);

                foreach (T member in Enum.GetValues(typeof(T)))
                {
                    var args = new ArgumentBuilder();

                    // Passing null should not modify the arguments
                    method.Invoke(null, new object[] { args, default(T?) });
                    Assert.AreEqual("", args.ToString());

                    // Passing a member should not throw (though might not modify the arguments)
                    Assert.DoesNotThrow(() => method.Invoke(null, new object[] { args, member }));
                }
            }
        }
    }
}