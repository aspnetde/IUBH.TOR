using System;
using IUBH.TOR.Data;
using IUBH.TOR.Modules.Courses.Domain;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Data
{
    [TestFixture]
    public class SqliteUnitOfWorkSessionTests : DataTestBase
    {
        [Test]
        public void Add()
        {
            var course = DemoCourse(Guid.NewGuid().ToString());

            using (var session = new SqliteUnitOfWorkSession(Connection))
            {
                session.Add(course);
                session.Commit().ShouldBeSuccessful();
            }

            var dbCourse = Connection.Find<Course>(course.Id);
            dbCourse.ShouldNotBeNull();
            dbCourse.Title.ShouldBe(course.Title);
        }

        [Test]
        public void Update()
        {
            var course = DemoCourse(Guid.NewGuid().ToString());

            using (var session = new SqliteUnitOfWorkSession(Connection))
            {
                session.Add(course);
                session.Commit().ShouldBeSuccessful();
            }

            var dbCourse = Connection.Find<Course>(course.Id);
            dbCourse.ShouldNotBeNull();
            dbCourse.Title.ShouldBe(course.Title);

            dbCourse.Title = Guid.NewGuid().ToString();
            dbCourse.Title.ShouldNotBe(course.Title);

            using (var session = new SqliteUnitOfWorkSession(Connection))
            {
                session.Update(dbCourse);
                session.Commit().ShouldBeSuccessful();
            }

            var dbCourse2 = Connection.Find<Course>(course.Id);
            dbCourse2.ShouldNotBeNull();
            dbCourse2.Title.ShouldBe(dbCourse.Title);
        }

        [Test]
        public void Remove()
        {
            var course = DemoCourse(Guid.NewGuid().ToString());

            using (var session = new SqliteUnitOfWorkSession(Connection))
            {
                session.Add(course);
                session.Commit().ShouldBeSuccessful();
            }

            var dbCourse = Connection.Find<Course>(course.Id);
            dbCourse.ShouldNotBeNull();

            using (var session = new SqliteUnitOfWorkSession(Connection))
            {
                session.Remove(dbCourse);
                session.Commit().ShouldBeSuccessful();
            }

            var dbCourse2 = Connection.Find<Course>(course.Id);
            dbCourse2.ShouldBeNull();
        }
    }
}
