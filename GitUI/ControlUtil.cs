using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace GitUI
{
    public static class ControlUtil
    {
        /// <summary>
        /// Enumerates all descendant controls.
        /// </summary>
        public static IEnumerable<Control> FindDescendants([NotNull] this Control control)
        {
            var queue = new Queue<Control>();

            foreach (Control child in control.Controls)
            {
                queue.Enqueue(child);
            }

            while (queue.Count != 0)
            {
                var c = queue.Dequeue();

                yield return c;

                foreach (Control child in c.Controls)
                {
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// Enumerates all descendant controls of type <typeparamref name="T"/> in breadth-first order.
        /// </summary>
        public static IEnumerable<T> FindDescendantsOfType<T>([NotNull] this Control control)
        {
            return FindDescendants(control).OfType<T>();
        }

        /// <summary>
        /// Finds the first descendent of <paramref name="control"/> that has type
        /// <typeparamref name="T"/> and satisfies <paramref name="predicate"/>.
        /// </summary>
        [CanBeNull]
        public static T FindDescendantOfType<T>(this Control control, Func<T, bool> predicate)
        {
            return FindDescendants(control).OfType<T>().Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Enumerates all ancestor controls.
        /// </summary>
        /// <remarks>
        /// The returned sequence does not include <paramref name="control"/>.
        /// </remarks>
        public static IEnumerable<Control> FindAncestors([NotNull] this Control control)
        {
            var parent = control.Parent;

            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }
    }
}