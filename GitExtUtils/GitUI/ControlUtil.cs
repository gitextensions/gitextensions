using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GitUI
{
    public static class ControlUtil
    {
        private static readonly MethodInfo SetStyleMethod = typeof(TabControl)
            .GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Enumerates all descendant controls.
        /// </summary>
        public static IEnumerable<Control> FindDescendants(this Control control,
            Func<Control, bool>? skip = null)
        {
            Queue<Control> queue = new();

            foreach (Control child in control.Controls)
            {
                if (skip?.Invoke(control) == true)
                {
                    continue;
                }

                queue.Enqueue(child);
            }

            while (queue.Count != 0)
            {
                var c = queue.Dequeue();

                yield return c;

                foreach (Control child in c.Controls)
                {
                    if (skip?.Invoke(child) == true)
                    {
                        continue;
                    }

                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// Enumerates all descendant controls of type <typeparamref name="T"/> in breadth-first order.
        /// </summary>
        public static IEnumerable<T> FindDescendantsOfType<T>(this Control control,
            Func<Control, bool>? skip = null)
        {
            return FindDescendants(control, skip).OfType<T>();
        }

        /// <summary>
        /// Finds the first descendent of <paramref name="control"/> that has type
        /// <typeparamref name="T"/> and satisfies <paramref name="predicate"/>.
        /// </summary>
        public static T? FindDescendantOfType<T>(this Control control, Func<T, bool> predicate,
            Func<Control, bool>? skip = null)
        {
            return FindDescendants(control, skip).OfType<T>().Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Enumerates all ancestor controls.
        /// </summary>
        /// <remarks>
        /// The returned sequence does not include <paramref name="control"/>.
        /// </remarks>
        public static IEnumerable<Control> FindAncestors(this Control control)
        {
            var parent = control.Parent;

            while (parent is not null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        /// <summary>
        /// Calls protected method <see cref="Control.SetStyle"/>.
        /// </summary>
        public static void SetStyle(this Control control, ControlStyles styles, bool value) =>
            SetStyleMethod.Invoke(control, new object[] { styles, value });
    }
}
