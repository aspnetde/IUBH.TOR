using System;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Support.V4.App;
using IUBH.TOR.Modules.Courses.Services;
using TinyIoC;
using Debug = System.Diagnostics.Debug;

namespace IUBH.TOR.Droid.Utilities.BackgroundSync
{
    /// <summary>
    /// This Job is being triggered by the Android System every 15 minutes or
    /// so. The exact execution time depends on some factors that are controlled
    /// by Android itself. When executed, this job will try to fetch the latest
    /// courses, and if something changed it will notify the user by sending a
    /// system notification.
    /// </summary>
    [Service(
        Name = "de.iubh.tor.DroidBackgroundSyncJob",
        Permission = "android.permission.BIND_JOB_SERVICE"
    )]
    public class DroidBackgroundSyncJob : JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {
            Task.Run(
                async () =>
                {
                    try
                    {
                        Debug.WriteLine("Fetch started");

                        var updater = TinyIoCContainer.Current.Resolve<ICourseUpdater>();

                        var updateResult = await updater.TryUpdateAsync().ConfigureAwait(false);

                        if (!updateResult.IsSuccessful)
                        {
                            Debug.WriteLine("Fetch failed: " + updateResult.ErrorMessage);

                            return;
                        }

                        if (!updateResult.Value.UpdatesFetched)
                        {
                            Debug.WriteLine("Fetch finished successfully. But no updates.");

                            return;
                        }

                        Debug.WriteLine("Fetch finished successfully. And we've got news!.");

                        Intent intent = new Intent(this, typeof(MainActivity));

                        PendingIntent pendingIntent = PendingIntent.GetActivity(
                            this,
                            0,
                            intent,
                            PendingIntentFlags.OneShot
                        );

                        // Build a notification and send it to the user.
                        var builder =
                            new NotificationCompat.Builder(this, MainActivity.NotificationChannelId)
                                .SetContentIntent(pendingIntent).SetAutoCancel(true)
                                .SetContentTitle(Constants.NotificationTitle)
                                .SetSmallIcon(Resource.Drawable.logo)
                                .SetContentText(Constants.NotificationText);

                        var notificationManager = NotificationManagerCompat.From(this);
                        notificationManager.Notify(69, builder.Build());
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Fetch failed (Exception thrown): " + e.Message);
                    }
                    finally
                    {
                        JobFinished(@params, false);
                    }
                }
            );

            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            Debug.Write("Fetch shut down by Android. Trying to reschedule.");

            return true;
        }
    }
}
