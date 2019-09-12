using System;
using System.Linq;
using System.Threading.Tasks;
using IUBH.TOR.Data;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Data;
using IUBH.TOR.Modules.Courses.Domain;
using IUBH.TOR.Modules.Courses.Services;
using IUBH.TOR.Modules.Shared.Pages;
using IUBH.TOR.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.Date;
using IUBH.TOR.Utilities.Messaging;
using IUBH.TOR.Utilities.Preferences;
using IUBH.TOR.Utilities.SecureStorage;
using PropertyChanged;
using Xamarin.Forms;

namespace IUBH.TOR.Modules.Courses.Pages
{
    public class CourseListViewModel : ViewModelBase
    {
        private readonly ICourseUpdater _courseUpdater;
        private readonly ICourseRepository _repository;
        private readonly IPreferencesUtility _preferences;
        private readonly IDateTimeUtility _dateTime;
        private readonly IDbConnection _dbConnection;
        private readonly ISecureStorageUtility _secureStorage;
        private readonly IBackgroundSyncUtility _backgroundSync;
        private readonly IMessenger _messenger;

        public string ErrorMessage { get; private set; }
        public CourseListState State { get; private set; }
        public CourseListItem[] Courses { get; private set; }

        public bool IsRefreshing { get; private set; }

        [DependsOn(nameof(State), nameof(Courses))]
        public string LastUpdateCheck
            => _preferences.LastCourseUpdateCheck != DateTime.MinValue
                ? $"◷ Last check: {_preferences.LastCourseUpdateCheck.ToLocalTime():g}"
                : "◷ Last check: Never";

        public Command ReInitializeCommand
            => new Command(
                async () => await ReInitializeAsync(),
                () => State != CourseListState.Loading
            );

        public Command RefreshCommand
            => new Command(
                async () => await RefreshAsync(),
                () => State != CourseListState.Loading
            );

        public Command SignoutCommand => new Command(SignOut, CanSignOut);

        // This is a (hopefully) temporary workaround for iOS:
        // https://github.com/xamarin/Xamarin.Forms/issues/5920
        public Command SelectCourseCommand => new Command(SelectCourse);

        public event EventHandler SignedOut;
        public event EventHandler<Course> CourseSelected;

        public CourseListViewModel(
            ICourseUpdater courseUpdater,
            ICourseRepository repository,
            IPreferencesUtility preferences,
            IDateTimeUtility dateTime,
            IDbConnection dbConnection,
            ISecureStorageUtility secureStorage,
            IBackgroundSyncUtility backgroundSync,
            IMessenger messenger
        )
        {
            _courseUpdater = courseUpdater;
            _repository = repository;
            _preferences = preferences;
            _dateTime = dateTime;
            _dbConnection = dbConnection;
            _secureStorage = secureStorage;
            _backgroundSync = backgroundSync;
            _messenger = messenger;

            Courses = new CourseListItem[0];
        }

        /// <summary>
        /// Initializes the courses in the list.
        /// </summary>
        public override async Task InitializeAsync()
        {
            State = CourseListState.Loading;

            Result updateResult = Result.Success;

            // We only (Re-)initialize every 15 minutes.
            if (_preferences.LastCourseUpdateCheck < _dateTime.UtcNow.AddMinutes(-15))
            {
                updateResult = await TryUpdateCoursesAsync().ConfigureAwait(false);
            }

            if (updateResult.IsSuccessful)
            {
                LoadCourses();
            }
            
            _messenger.Subscribe<WokeUpMessage>(this, (o, message) => LoadCourses());
        }

        public Task ReInitializeAsync() => InitializeAsync();

        /// <summary>
        /// Called after the user has been "pulling to refresh".
        /// Tries to update the courses.
        /// </summary>
        public async Task RefreshAsync()
        {
            IsRefreshing = true;

            var updateResult = await TryUpdateCoursesAsync().ConfigureAwait(false);

            if (updateResult.IsSuccessful)
            {
                LoadCourses();
            }

            IsRefreshing = false;
        }

        private void LoadCourses()
        {
            Courses = _repository.All().OrderByDescending(c => c.DateOfExamination)
                .Select(c => new CourseListItem(c)).ToArray();

            State = Courses.Length > 0 ? CourseListState.Default : CourseListState.Empty;
        }

        private async Task<Result> TryUpdateCoursesAsync()
        {
            var updateDateTime = _dateTime.UtcNow;

            var courseUpdateResult = await _courseUpdater.TryUpdateAsync().ConfigureAwait(false);

            if (!courseUpdateResult.IsSuccessful)
            {
                ErrorMessage = courseUpdateResult.ErrorMessage;
                State = CourseListState.Error;

                return courseUpdateResult;
            }

            _preferences.LastCourseUpdateCheck = updateDateTime;

            return Result.Success;
        }

        private void SignOut()
        {
            _preferences.Nuke();
            _secureStorage.Nuke();
            _dbConnection.Nuke();
            _backgroundSync.Disable();
            
            _messenger.Unsubscribe<WokeUpMessage>(this);

            SignedOut?.Invoke(this, EventArgs.Empty);
        }

        private bool CanSignOut() => State != CourseListState.Loading && !IsRefreshing;

        private void SelectCourse(object parameter)
            => CourseSelected?.Invoke(this, ((CourseListItem)parameter).Course);
    }
}
