using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SQLite;

namespace IUBH.TOR.Modules.Courses.Domain
{
    /// <summary>
    /// Provides typed information about an IUBH Course. This type
    /// supports structural equality among its properties, so
    /// two courses with the same data will be seen as equal.
    /// </summary>
    public class Course : IEquatable<Course>
    {
        private sealed class CourseEqualityComparer : IEqualityComparer<Course>
        {
            public bool Equals(Course x, Course y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return string.Equals(x.Id, y.Id, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(x.Title, y.Title, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(x.Module, y.Module, StringComparison.OrdinalIgnoreCase)
                       && x.Status == y.Status
                       && x.Grade == y.Grade
                       && x.IsPassed == y.IsPassed
                       && x.Rating == y.Rating
                       && x.Credits == y.Credits
                       && x.DateOfExamination.Equals(y.DateOfExamination)
                       && x.Attempts == y.Attempts
                       && x.DateOfLastUpdate == y.DateOfLastUpdate;
            }

            public int GetHashCode(Course obj)
            {
                unchecked
                {
                    var hashCode = (obj.Id != null
                        ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Id)
                        : 0);

                    hashCode = (hashCode * 397)
                               ^ (obj.Title != null
                                   ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Title)
                                   : 0);

                    hashCode = (hashCode * 397)
                               ^ (obj.Module != null
                                   ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Module)
                                   : 0);

                    hashCode = (hashCode * 397) ^ (int)obj.Status;
                    hashCode = (hashCode * 397) ^ obj.Grade.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IsPassed.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Rating.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Credits;
                    hashCode = (hashCode * 397) ^ obj.DateOfExamination.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Attempts;
                    hashCode = (hashCode * 397) ^ obj.DateOfLastUpdate.GetHashCode();

                    return hashCode;
                }
            }
        }

        /// <summary>
        /// The Id is a unique identifier that distinguishes
        /// the Course from all others. Right now it may be
        /// the hashcode of the title – so whenever the title
        /// changes, the Id will change, too. This limitation
        /// is unfortunate but has become necessary when IUBH
        /// decided to remove actual ID information from the
        /// Transcript of Records on their website. So the
        /// actual ID is not available for us.
        /// </summary>
        [PrimaryKey]
        public string Id { get; set; }

        /// <summary>
        /// The title (name) of this course.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// The name of the module this course belongs to.
        /// </summary>
        public string Module { get; set; }
        
        /// <summary>
        /// The current status of this course from the perspective
        /// of our currently logged in student.
        /// </summary>
        public CourseStatus Status { get; set; }
        
        /// <summary>
        /// The Grade received for this course.
        /// </summary>
        public decimal Grade { get; set; }
        
        /// <summary>
        /// True if an exam has been passed for this course.
        /// </summary>
        public bool IsPassed { get; set; }
        
        /// <summary>
        /// Rating in percentage.
        /// </summary>
        public decimal Rating { get; set; }
        
        /// <summary>
        /// Credits earned for this course.
        /// </summary>
        public int Credits { get; set; }
        
        /// <summary>
        /// The date the exam will or has been taken place.
        /// </summary>
        public DateTime DateOfExamination { get; set; }
        
        /// <summary>
        /// Number of attempts to pass the exam.
        /// </summary>
        public int Attempts { get; set; }
        
        /// <summary>
        /// The date and time that course information has last been updated.
        /// This is internal information only concerning this app, it is not
        /// provided by the IUBH web page.
        /// </summary>
        public DateTime DateOfLastUpdate { get; set; }

        public bool Equals(Course other) => new CourseEqualityComparer().Equals(this, other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((Course)obj);
        }

        public override int GetHashCode() => new CourseEqualityComparer().GetHashCode(this);

        public static bool operator ==(Course left, Course right) => Equals(left, right);
        public static bool operator !=(Course left, Course right) => !Equals(left, right);

        public static Course FromRawCourse(RawCourse raw, DateTime lastupdate)
        {
            return new Course
            {
                Id = raw.Id,
                Title = raw.Title,
                Module = raw.Module,
                Status = MapStatus(raw.Status),
                Grade = MapGrade(raw.Grade),
                IsPassed = MapIsPassed(raw.Grade),
                Rating = MapRating(raw.Rating),
                Credits = MapCredits(raw.Credits),
                DateOfExamination = MapDateOfExamination(raw.DateOfExamination),
                Attempts = MapInteger(raw.Attempts),
                DateOfLastUpdate = lastupdate
            };
        }

        private static CourseStatus MapStatus(string status)
        {
            switch (status)
            {
                case "P":
                    return CourseStatus.Passed;
                case "P, T":
                    return CourseStatus.Transferred;
                case "F":
                    return CourseStatus.Failed;
                case "EE":
                    return CourseStatus.ExamEnrolment;
                case "E":
                    return CourseStatus.CourseEnrolment;
                case "CE":
                    return CourseStatus.CombinationExam;
                case "M":
                    return CourseStatus.MissingResult;
                case "ME":
                    return CourseStatus.ModuleExamination;
                default:
                    return CourseStatus.Unknown;
            }
        }

        private static decimal MapGrade(string grade)
            => decimal.TryParse(
                grade,
                NumberStyles.Any,
                new CultureInfo("de-DE"),
                out decimal result
            )
                ? result
                : 0;

        private static bool MapIsPassed(string grade)
            => grade?.Trim().Equals("passed", StringComparison.InvariantCultureIgnoreCase) ?? false;

        private static decimal MapRating(string rating)
            => string.IsNullOrWhiteSpace(rating) || !rating.Contains("/") ? 0 :
                decimal.TryParse(
                    rating.Replace(" / 100", "").Trim(),
                    NumberStyles.Any,
                    new CultureInfo("en-US"),
                    out decimal result
                ) ? result : 0;

        private static int MapCredits(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input) || !input.Contains("/"))
                {
                    return 0;
                }

                return MapInteger(input.Split('/')[0]?.Trim());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return 0;
            }
        }

        private static DateTime MapDateOfExamination(string date)
            => DateTime.TryParse(
                date,
                new CultureInfo("de-DE"),
                DateTimeStyles.None,
                out DateTime result
            )
                ? result.Date
                : DateTime.MinValue;

        private static int MapInteger(string input)
            => int.TryParse(input, out int result) ? result : 0;
    }
}
