using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NHibernate.Dialect;
using NHibernate.Linq;
using NUnit.Framework;
using System.Linq;

namespace NHibernate.Test.NHSpecificTest.NH1234
{
    [TestFixture]
    public class SampleTest : BugTestCase
    {
        protected override void OnSetUp()
        {
            base.OnSetUp();

            using (ISession session = this.OpenSession())
            {
                for (int i = 1; i <= 5; i++)
                {
                    DomainClass entity = new DomainClass();
                    entity.Id = i;
                    entity.ByteData = new byte[] { 1, 2, 3 };
                    session.Save(entity);
                    session.Flush();
                }
            }
        }

        protected override void OnTearDown()
        {
            base.OnTearDown();
            using (ISession session = this.OpenSession())
            {
                string hql = "from System.Object";
                session.Delete(hql);
                session.Flush();
            }
        }

        protected override bool AppliesTo(NHibernate.Dialect.Dialect dialect)
        {
            return dialect as MsSql2005Dialect != null;
        }

        private Context OpenContext()
        {
            return new Context(OpenSession());
        }

        [Test]
        public void DeleteThroughExtensionMethod()
        {
            using (var context = OpenContext())
            {
                context.DomainClasses.Delete();
                context.Session.Flush();
            }

            using (var context = OpenContext())
            {
                Assert.AreEqual(0, context.DomainClasses.Count());
            }
        }

        [Test]
        public void DeleteSelectionThroughExtensionMethod()
        {
            using (var context = OpenContext())
            {
                context.DomainClasses.Where(p => p.Id <= 3).Delete();
                context.Session.Flush();
            }

            using (var context = OpenContext())
            {
                Assert.AreEqual(2, context.DomainClasses.Count());
            }
        }

        [Test]
        public void UpdateThroughExtensionMethod()
        {
            using (var context = OpenContext())
            {
                context.DomainClasses.Update(p => new DomainClass { ByteData = new[] { (byte)p.Id } });
                context.Session.Flush();
            }

            using (var context = OpenContext())
            {
                foreach (var record in context.DomainClasses)
                {
                    Assert.AreEqual((byte)record.Id, record.ByteData[0]);
                }
            }
        }

        [Test]
        public void UpdateSingleThroughExtensionMethod()
        {
            using (var context = OpenContext())
            {
                context.DomainClasses.Where(p => p.Id == 3).Update(p => new DomainClass { ByteData = new[] { (byte)p.Id } });
                context.Session.Flush();
            }

            using (var context = OpenContext())
            {
                foreach (var record in context.DomainClasses)
                {
                    if (record.Id == 3)
                        Assert.AreEqual((byte)record.Id, record.ByteData[0]);
                    else
                        Assert.AreEqual(new byte[] { 1, 2, 3 }, record.ByteData);
                }
            }
        }

        [Test]
        public void UpdateNoMutationsThroughExtensionMethods()
        {
            using (var context = OpenContext())
            {
                context.DomainClasses.Where(p => p.Id == 3).Update(p => new DomainClass());
                context.Session.Flush();
            }

            using (var context = OpenContext())
            {
                foreach (var record in context.DomainClasses)
                {
                    Assert.AreEqual(new byte[] { 1, 2, 3 }, record.ByteData);
                }
            }
        }
    }
}
