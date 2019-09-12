using System.Diagnostics;
using Android.App.Job;
using Android.Content;
using IUBH.TOR.Utilities.BackgroundSync;

namespace IUBH.TOR.Droid.Utilities.BackgroundSync
{
    public class DroidBackgroundSyncUtility : IBackgroundSyncUtility
    {
        private const int JobId = 69;

        private static JobScheduler JobScheduler
            => (JobScheduler)MainActivity.Instance.GetSystemService(Context.JobSchedulerService);

        public void Enable()
        {
            JobInfo job = CreateJobBuilder<DroidBackgroundSyncJob>(JobId)
                .SetPeriodic(Constants.FetchIntervalInMinutes * 60 * 1000).Build();

            var scheduleResult = JobScheduler.Schedule(job);

            Debug.WriteLine(
                scheduleResult == JobScheduler.ResultSuccess
                    ? "Fetch enabled successfully."
                    : "Fetch couldn't be enabbled."
            );
        }

        public void Disable()
        {
            JobScheduler.Cancel(JobId);

            Debug.WriteLine("Fetch disabled.");
        }

        private static JobInfo.Builder CreateJobBuilder<T>(int jobId) where T : JobService
        {
            var javaClass = Java.Lang.Class.FromType(typeof(T));
            var componentName = new ComponentName(MainActivity.Instance, javaClass);
            return new JobInfo.Builder(jobId, componentName);
        }
    }
}
