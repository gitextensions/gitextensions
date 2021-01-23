// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Borrowed from https://github.com/dotnet/winforms/

#nullable enable

namespace System
{
    /// <summary>
    ///  Extension methods for associating internals test accessors with
    ///  types being tested.
    /// </summary>
    /// <remarks>
    ///  In the System namespace for implicit discovery.
    /// </remarks>
    public static partial class TestAccessors
    {
        // Need to pass a null parameter when constructing a static instance
        // of TestAccessor. As this is pretty common and never changes, caching
        // the array here.
        private static object?[] _nullObjectParam = { null };

        /// <summary>
        ///  Extension that creates a generic internals test accessor for a
        ///  given instance or Type class (if only accessing statics).
        /// </summary>
        /// <param name="instanceOrType">
        ///  Instance or Type class (if only accessing statics).
        /// </param>
        /// <example>
        /// <![CDATA[
        ///  Version version = new Version(4, 1);
        ///  Assert.Equal(4, version.TestAccessor().Dynamic._Major));
        ///
        ///  // Or
        ///
        ///  dynamic accessor = version.TestAccessor().Dynamic;
        ///  Assert.Equal(4, accessor._Major));
        /// ]]>
        /// </example>
        public static ITestAccessor TestAccessor(this object instanceOrType)
        {
            if (instanceOrType is Type type)
            {
                return (ITestAccessor)Activator.CreateInstance(
                    typeof(TestAccessor<>).MakeGenericType(type),
                    _nullObjectParam);
            }
            else
            {
                return (ITestAccessor)Activator.CreateInstance(
                    typeof(TestAccessor<>).MakeGenericType(instanceOrType.GetType()),
                    instanceOrType);
            }
        }
    }
}
