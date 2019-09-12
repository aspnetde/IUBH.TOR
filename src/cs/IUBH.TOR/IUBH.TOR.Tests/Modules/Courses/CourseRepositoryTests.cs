using IUBH.TOR.Data;
using IUBH.TOR.Modules.Courses.Data;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CourseRepositoryTests : DataTestBase
    {
        [Test]
        public void All()
        {
            var course1 = DemoCourse("1");
            var course2 = DemoCourse("2");
            var course3 = DemoCourse("3");

            using (var session = new SqliteUnitOfWorkSession(Connection))
            {
                session.Add(course1);
                session.Add(course2);
                session.Add(course3);
                
                session.Commit().ShouldBeSuccessful();
            }

            var connection = Substitute.For<IDbConnection>();
            connection.Connection.Returns(Connection);

            var sut = new SqliteCourseRepository(connection);
            var result = sut.All();
            
            result.ShouldNotBeNull();
            
            result.ShouldContain(course1);
            result.ShouldContain(course2);
            result.ShouldContain(course3);
        }
    }
}
