using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using IUBH.TOR.Data;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Data;
using IUBH.TOR.Modules.Courses.Domain;
using IUBH.TOR.Utilities.Date;
using IUBH.TOR.Utilities.Preferences;
using Xamarin.Forms.Internals;

namespace IUBH.TOR.Modules.Courses.Services
{
    internal class CourseUpdater : ICourseUpdater
    {
        private readonly ICoursePageHtmlDownloader _downloader;
        private readonly ICoursePageHtmlParser _parser;
        private readonly ICourseSetComparer _comparer;
        private readonly ICourseRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeUtility _dateTime;
        private readonly IPreferencesUtility _preferences;

        public CourseUpdater(
            ICoursePageHtmlDownloader downloader,
            ICoursePageHtmlParser parser,
            ICourseSetComparer comparer,
            ICourseRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeUtility dateTime,
            IPreferencesUtility preferences
        )
        {
            _downloader = downloader;
            _parser = parser;
            _comparer = comparer;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _preferences = preferences;
        }

        /// <summary>
        /// Executes a full update operation on courses: Downloads, parses,
        /// compares, adds/removes/updates courses to the database.
        /// </summary>
        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public async Task<Result<CourseUpdateInfo>> TryUpdateAsync()
        {
            // Download the HTML page
            Result<string> downloadResult = await _downloader
                .TryDownloadCoursePageHtmlAsync(Constants.CareTranscriptOfRecordsUrl)
                .ConfigureAwait(false);

            if (!downloadResult.IsSuccessful)
            {
                return downloadResult.ToResult<CourseUpdateInfo>();
            }

            // Parse the HTML page
            var parseResult = _parser.TryParseCoursePage(downloadResult.Value);

            if (!parseResult.IsSuccessful)
            {
                return parseResult.ToResult<CourseUpdateInfo>();
            }

            // If we are good here we can remember the date time we last checked
            _preferences.LastCourseUpdateCheck = _dateTime.UtcNow;

            var existingCourses = _repository.All();

            // Convert raw to actual courses
            var currentCourses = parseResult.Value.Select(
                rawCourse => Course.FromRawCourse(
                    rawCourse,
                    existingCourses.SingleOrDefault(ec => ec.Id == rawCourse.Id)?.DateOfLastUpdate
                    ?? _dateTime.UtcNow
                )
            ).ToArray();

            // Compare our existing courses and our current ones.
            _comparer.Compare(
                existingCourses,
                currentCourses,
                out Course[] addedCourses,
                out Course[] modifiedCourses,
                out Course[] removedCourses
            );

            // When nothing has changed we are still good but can also return here.
            if (addedCourses.Length <= 0
                && modifiedCourses.Length <= 0
                && removedCourses.Length <= 0)
            {
                return Result.WithSuccess(new CourseUpdateInfo(false));
            }

            // Set the last update date for all courses that were modified.
            modifiedCourses.ForEach(course => course.DateOfLastUpdate = _dateTime.UtcNow);

            // Persist all the changes to the database
            using (var session = _unitOfWork.OpenSession())
            {
                session.Add(addedCourses);
                session.Update(modifiedCourses);
                session.Remove(removedCourses);

                var commitResult = session.Commit();

                return commitResult.IsSuccessful
                    ? Result.WithSuccess(
                        new CourseUpdateInfo(addedCourses.Length > 0 || modifiedCourses.Length > 0)
                    )
                    : commitResult.ToResult<CourseUpdateInfo>();
            }
        }
    }
}
