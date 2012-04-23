using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate.Engine;

namespace NHibernate.Linq
{
    public static class MutationExtensionMethods
    {
        private static readonly MethodInfo _getSessionImplementor;

        static MutationExtensionMethods()
        {
            _getSessionImplementor = typeof(DefaultQueryProvider).GetMethod("get_Session", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Update<T>(this IQueryable<T> self, Func<T, T> templateFactory)
            where T : class, new()
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (templateFactory == null)
                throw new ArgumentNullException("templateFactory");

            var session = GetSession(self.Provider);

            foreach (var record in self)
            {
                ApplyTemplate(record, templateFactory);
                session.Save(record);
            }
        }

        private static void ApplyTemplate<T>(T record, Func<T, T> templateFactory)
            where T : class, new()
        {
            var emptyTemplate = new T();
            var template = templateFactory(record);

            if (template == null)
                throw new InvalidOperationException("Expected template factory to return a template");

            foreach (var property in template.GetType().GetProperties())
            {
                object value = property.GetValue(template, null);
                object emptyValue = property.GetValue(emptyTemplate, null);

                if (!Equals(value, emptyValue))
                    property.SetValue(record, value, null);
            }
        }

        public static void Delete<T>(this IQueryable<T> self)
            where T : class
        {
            if (self == null)
                throw new ArgumentNullException("self");

            var session = GetSession(self.Provider);

            foreach (var record in self)
            {
                session.Delete(record);
            }
        }

        private static ISession GetSession(IQueryProvider queryProvider)
        {
            // Couldn't easily figure out how to get the session. This works
            // but there is/should probably be a better mechanism.

            return (ISession)_getSessionImplementor.Invoke(queryProvider, null);
        }
    }
}
