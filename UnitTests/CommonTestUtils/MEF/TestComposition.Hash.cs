// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copied from https://github.com/dotnet/roslyn/blob/6c46bd07af9e9d926777969d8a87be24b80dd164/src/Compilers/Core/Portable/InternalUtilities/Hash.cs with some tweaks

using System.Collections.Immutable;

namespace CommonTestUtils.MEF
{
    public sealed partial class TestComposition
    {
        internal static class Hash
        {
            /// <summary>
            /// This is how VB Anonymous Types combine hash values for fields.
            /// </summary>
            internal static int Combine(int newKey, int currentKey)
            {
                return unchecked((currentKey * (int)0xA5555529) + newKey);
            }

            internal static int CombineValues<T>(ImmutableArray<T> values, int maxItemsToHash = int.MaxValue)
            {
                if (values.IsDefaultOrEmpty)
                {
                    return 0;
                }

                var hashCode = 0;
                var count = 0;
                foreach (var value in values)
                {
                    if (count++ >= maxItemsToHash)
                    {
                        break;
                    }

                    // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                    if (value != null)
                    {
                        hashCode = Combine(value.GetHashCode(), hashCode);
                    }
                }

                return hashCode;
            }
        }
    }
}
