using System;
using System.IO;
using IUBH.TOR.Modules.Courses.Domain;
using NUnit.Framework;
using Shouldly;
using SQLite;

namespace IUBH.TOR.Tests
{
    [TestFixture]
    public abstract class DataTestBase
    {
        protected SQLiteConnection Connection { get; private set; }

        private string _databasePath;

        [SetUp]
        public void SetUp()
        {
            _databasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
            Console.WriteLine(_databasePath);
            Connection = new SQLiteConnection(_databasePath);
            Connection.CreateTable<Course>().ShouldBe(CreateTableResult.Created);
        }

        [TearDown]
        public void TearDown()
        {
            Connection.Close();
            Connection.Dispose();

            //File.Delete(_databasePath);
        }

        protected static Course DemoCourse(string id)
            => new Course
            {
                Id = id,
                Title = Guid.NewGuid().ToString(),
                Module = "Module",
                Status = CourseStatus.MissingResult,
                Grade = 1.3m,
                IsPassed = false,
                Rating = 67.66m,
                Credits = 5,
                DateOfExamination = new DateTime(2019, 4, 12),
                Attempts = 1
            };
    }
}
