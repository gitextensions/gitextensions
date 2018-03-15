using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GitCommands.Core
{
    /// <summary>
    /// class that provides Equals and ToString methods based on objects returned by InlinedStructure
    /// Warning: it doesn't provide GetHashCode,
    /// so obj1.Equals(obj2) == true does not imply obj1.GetHashCode() == obj2.GetHashCode()
    /// to satisfy above implication you have to provide custom implementation for GetHashCode
    /// </summary>
    public abstract class SimpleStructured
    {
        protected internal abstract IEnumerable<object> InlinedStructure();

        public override bool Equals(object obj)
        {
            SimpleStructured other = obj as SimpleStructured;
            if (other == null)
            {
                return false;
            }

            return InlinedStructure().SequenceEqual(other.InlinedStructure(), new SimpleEqualityComparer());
        }

        public override string ToString()
        {
            return new SimpleEqualityComparer().ToString(InlinedStructure());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class SimpleEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            if (x == null)
            {
                return y == null;
            }
            else
            {
                if (!(x is string))
                {
                    IEnumerable ex = x as IEnumerable;
                    IEnumerable ey = y as IEnumerable;
                    if (ex != null && ey != null)
                    {
                        return ex.Cast<object>().SequenceEqual(ey.Cast<object>(), this);
                    }
                }

                return x.Equals(y);
            }
        }

        public int GetHashCode(object obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }

        public string ToString(object obj)
        {
            return ToString(obj, string.Empty);
        }

        private static string ToString(object obj, string indent)
        {
            if (obj == null)
            {
                return indent + "[null]";
            }

            if (!(obj is string))
            {
                if (obj is IEnumerable eo)
                {
                    return eo.Cast<object>().Select(o => ToString(o, indent + "  ")).Join("\n");
                }
            }

            if (obj is SimpleStructured ss)
            {
                return ToString(ss.InlinedStructure(), indent);
            }

            return indent + obj;
        }
    }
}
