using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;

namespace NHibernate.Test.NHSpecificTest.NH1234
{
    internal class Context : IDisposable
    {
        private bool _disposed;

        public ISession Session { get; private set; }

        public Context(ISession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            Session = session;
        }

        public IQueryable<DomainClass> DomainClasses
        {
            get { return Session.Query<DomainClass>(); }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (Session != null)
                {
                    Session.Dispose();
                    Session = null;
                }

                _disposed = true;
            }
        }
    }
}
