using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer
{
    public static class Extensions
    {
        public static void SafeInvoke(this Action action)
        {
            if (action == null)
                return;

            action();
        }

        public static void SafeInvoke<T>(this Action<T> action, T obj)
        {
            if (action == null)
                return;

            action(obj);
        }

        public static void SafeInvoke(this EventHandler eventHandler, object sender, EventArgs args)
        {
            if (eventHandler == null)
                return;

            eventHandler(sender, args);
        }

        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args)
            where T : EventArgs
        {
            if (eventHandler == null)
                return;

            eventHandler(sender, args);
        }

        public static void SafeInvoke(this NotifyCollectionChangedEventHandler eventHandler, object sender, NotifyCollectionChangedEventArgs args)
        {
            if (eventHandler == null)
                return;

            eventHandler(sender, args);
        }

        public static void SafeInvoke(this PropertyChangedEventHandler eventHandler, object sender, PropertyChangedEventArgs args)
        {
            if (eventHandler == null)
                return;

            eventHandler(sender, args);
        }
    }
}
