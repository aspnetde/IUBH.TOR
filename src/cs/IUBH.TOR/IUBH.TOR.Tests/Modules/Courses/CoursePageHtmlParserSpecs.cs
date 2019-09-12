using System.IO;
using System.Linq;
using IUBH.TOR.Modules.Courses.Services;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CoursePageHtmlParserSpecs
    {
        [TestFixture]
        public abstract class CoursePageHtmlParserSpecsTestBase
        {
            protected static string GetDemoHtml()
            {
                var assembly = typeof(CoursePageHtmlParserSpecs).Assembly;

                var resourceName = assembly.GetManifestResourceNames()
                    .Single(name => name.Contains("demo-tor.html"));

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public class When_a_valid_HtmlDocument_is_being_provided : CoursePageHtmlParserSpecsTestBase
        {
            [Test]
            [TestCase("Mathematics I")]
            [TestCase("Introduction to Scientific Work")]
            [TestCase("Self and Time Management")]
            [TestCase("Business Administration I")]
            [TestCase("Business Administration II")]
            [TestCase("Software Engineering Principles")]
            [TestCase("Object-oriented Programming with Java")]
            [TestCase("Data Structures and Java Class Library")]
            [TestCase("Mathematics II")]
            [TestCase("Web Application Development")]
            [TestCase("Programming Information Systems with Java EE")]
            [TestCase("Requirements Engineering")]
            [TestCase("Management Accounting I (Introduction)")]
            [TestCase("Management Accounting II (Advanced)")]
            public void All_of_its_courses_are_being_returned(string title)
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                result.Value.Any(x => x.Title == title).ShouldBeTrue();
                result.Value.Length.ShouldBe(14);
            }

            [Test]
            [TestCase("Mathematics I (ME)")]
            [TestCase("Scientific Work (ME)")]
            [TestCase("Business Administration (ME)")]
            [TestCase("Software Engineering Principles (ME)")]
            [TestCase("Object-oriented programing (ME)")]
            [TestCase("Mathematics for Business Engineers (ME)")]
            [TestCase("Web Application Development (ME)")]
            [TestCase("Requirements Engineering (ME)")]
            [TestCase("Management Accounting (ME)")]
            public void Module_entries_are_being_skipped(string title)
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                result.Value.Any(x => x.Title == title).ShouldBeFalse();
            }

            [Test]
            [TestCase("BWL I (Einführung und Grundlagen)")]
            [TestCase("BWL I (Einführung und Grundlagen)")]
            [TestCase("BWL II (Vertiefung)")]
            [TestCase("Mathematics for Business Engineers")]
            public void Courses_without_examination_date_are_being_skipped(string courseId)
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                result.Value.Any(x => x.Id == courseId).ShouldBeFalse();
            }
        }

        public class When_a_course_is_being_returned : CoursePageHtmlParserSpecsTestBase
        {
            [Test]
            public void Its_Id_is_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt101 = result.Value.Single(x => x.Title == "Mathematics I");
                imt101.Id.ShouldNotBeNullOrEmpty();
            }

            [Test]
            public void Its_Id_does_not_change()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();

                var id1 = sut.TryParseCoursePage(html).Value.Single(x => x.Title == "Mathematics I")
                    .Id;

                var id2 = sut.TryParseCoursePage(html).Value.Single(x => x.Title == "Mathematics I")
                    .Id;

                id2.Equals(id1).ShouldBeTrue();
            }

            [Test]
            public void Its_Title_is_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt101 = result.Value.Single(x => x.Title == "Mathematics I");
                imt101.Title.ShouldBe("Mathematics I");
            }

            [Test]
            [TestCase("Mathematics I", "Mathematics I (ME)")]
            [TestCase("Introduction to Scientific Work", "Scientific Work (ME)")]
            [TestCase("Web Application Development", "Web Application Development (ME)")]
            [TestCase(
                "Programming Information Systems with Java EE",
                "Web Application Development (ME)"
            )]
            public void Its_Module_is_set(string title, string module)
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var course = result.Value.Single(x => x.Title == title);
                course.Module.ShouldBe(module);
            }

            [Test]
            public void Its_Status_is_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt101 = result.Value.Single(x => x.Title == "Mathematics I");
                imt101.Status.ShouldBe("P");
            }

            [Test]
            public void Its_Grade_is_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt102 = result.Value.Single(x => x.Title == "Mathematics II");
                imt102.Grade.ShouldBe("1,3");
            }

            [Test]
            public void Its_Rating_is_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt102 = result.Value.Single(x => x.Title == "Mathematics II");
                imt102.Rating.ShouldBe("93.3 / 100");
            }

            [Test]
            public void Its_Credits_are_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt102 = result.Value.Single(x => x.Title == "Mathematics II");
                imt102.Credits.ShouldBe("5 / 5");
            }

            [Test]
            public void Its_Date_of_Examination_is_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt102 = result.Value.Single(x => x.Title == "Mathematics II");
                imt102.DateOfExamination.ShouldBe("21.10.2017");
            }

            [Test]
            public void Its_attempts_are_set()
            {
                var html = GetDemoHtml();

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);
                result.IsSuccessful.ShouldBeTrue();

                var imt102 = result.Value.Single(x => x.Title == "Mathematics II");
                imt102.Attempts.ShouldBe("4");
            }
        }

        public class When_an_invalid_HtmlDocument_is_being_provided
            : CoursePageHtmlParserSpecsTestBase
        {
            [Test]
            public void No_exception_is_being_thrown_but_an_invalid_result_is_being_returned()
            {
                var html = "INVALID DOC";

                var sut = new CoursePageHtmlParser();
                var result = sut.TryParseCoursePage(html);

                result.IsSuccessful.ShouldBeFalse();
            }
        }
    }
}
