namespace IUBH.TOR.Android

open Android.App
open Android.App.Job
open Android.Content
open Android.Content.PM
open Android.OS
open Android.Support.V4.App
open IUBH.TOR
open IUBH.TOR.Android
open System
open System.Diagnostics
open Xamarin.Forms
open Xamarin.Forms.Platform.Android
open Xamarin.Essentials

[<Activity(Label = "IUBH TOR",
           Icon = "@mipmap/icon",
           Theme = "@style/MainTheme",
           MainLauncher = true,
           ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation))>]
type MainActivity() =
    inherit FormsAppCompatActivity()
    
    /// Every notification coming from the app gets
    /// a unique identifier
    [<Literal>]
    let NotificationJobId = 69
    
    /// We always use "our own" notification channel
    member this.NotificationChannelId = "IUBH.TOR"
    
    [<DefaultValue>]
    static val mutable private instance: MainActivity
    
    /// A Xamarin.Forms app consists of only one Activity
    /// on the Android platform. This property provides
    /// access to that singleton.
    static member Instance
        with get() = MainActivity.instance
        
    [<DefaultValue>]
    val mutable private app: App
    
    /// Provides access to the instance of our Fabulous App
    member this.App
        with get() = this.app

    /// Provides acccess to the job scheduling system service
    member private this.JobScheduler =
        this.GetSystemService(Context.JobSchedulerService) :?> JobScheduler
    
    /// Enables periodic background synchronization. Note that
    /// the minimum interval must be 15 minutes, otherwise the
    /// service will silently refuse to do anything.
    member private this.EnableBackgroundSync () =
        let fetchInterval =
            Convert.ToInt64(Constants.Backgrounding.FetchIntervalInMinutes * 60.0 * 1000.0)
        
        let javaClass= Java.Lang.Class.FromType(typeof<DroidBackgroundSyncJob>)
        let componentName = new ComponentName(this, javaClass)
        let jobBuilder = new JobInfo.Builder(NotificationJobId, componentName)
        let job = jobBuilder.SetPeriodic(fetchInterval).Build()
        let scheduleResult = this.JobScheduler.Schedule(job)
        
        match scheduleResult with
        | JobScheduler.ResultSuccess -> Debug.WriteLine "Fetch enabled successfully."
        | _ -> Debug.WriteLine "Fetch couldn't be enabbled."
        
    /// Disables periodic background synchronization
    member private this.DisableBackgroundSync () =
        this.JobScheduler.Cancel(NotificationJobId)
        Debug.WriteLine("Fetch disabled.")
    
    /// We need our own notification channel through which notifications
    /// can be sent. Thos method creates one.
    member private this.createNotificationChannel () =
        let channel =
            new NotificationChannel(
                this.NotificationChannelId,
                "IUBH TOR",
                NotificationImportance.Default)
        channel.Description <- "Delivers updates of your Transcript of Records."
        
        let notificationManager =
            this.GetSystemService(Context.NotificationService)
                :?> NotificationManager
        
        notificationManager.CreateNotificationChannel channel

    /// This is the central entry point for the Android app.
    override this.OnCreate(bundle: Bundle) =
        // Assign the instance field, so we can access this instance
        // from the background synchronization job, too
        MainActivity.instance <- this
        
        // Do some necessary layouting stuff
        FormsAppCompatActivity.TabLayoutResource <- Resources.Layout.Tabbar
        FormsAppCompatActivity.ToolbarResource <- Resources.Layout.Toolbar

        base.OnCreate(bundle)

        /// Initialize all the things
        Platform.Init(this, bundle)
        Forms.Init(this, bundle)
        FormsMaterial.Init(this, bundle)
        
        /// We want to know when background synchronization should be
        /// enabled or disabled, so we can react on the platform.
        App.LoggedIn.Publish.Add (fun _ -> this.EnableBackgroundSync())
        App.LoggedOut.Publish.Add (fun _ -> this.DisableBackgroundSync())
        
        /// Create our Fabulous app
        let app = new App()
        
        this.app <- app
        
        this.createNotificationChannel()
        this.LoadApplication(app)

    override this.OnRequestPermissionsResult(requestCode, permissions, grantResults) =
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults)
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults)
    
    /// The DroidBackgroundSyncJob is being executed in the background and
    /// responsible for checking for updates and notifying the user in case
    /// of any update to happen.
and [<Service(Name = "de.iubh.tor.DroidBackgroundSyncJob", Permission = "android.permission.BIND_JOB_SERVICE")>]
    DroidBackgroundSyncJob() =
    inherit JobService()

    /// Sends out an actual notification to the user
    member private this.notifyAboutUpdates() =
        let intent = new Intent(this, typeof<MainActivity>)
        
        let pendingIntent =
            PendingIntent.GetActivity(
                this,
                0,
                intent,
                PendingIntentFlags.OneShot)
            
        let channelId = MainActivity.Instance.NotificationChannelId 
           
        let builder =
            (new NotificationCompat.Builder(this, channelId))
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetContentTitle(Constants.Backgrounding.NotificationTitle)
                .SetContentText(Constants.Backgrounding.NotificationText)
                .SetSmallIcon(Resources.Drawable.logo)
        
        let notificationManager = NotificationManagerCompat.From(this)
        notificationManager.Notify (69, builder.Build())

    /// Performs an actual update.
    member private this.performUpdate parameters =
        async {
            try
                let app = MainActivity.Instance.App
                
                if not (app.Runner.CurrentModel.Session.IsAuthenticated) then
                    Debug.WriteLine("Could not start Fetch: Not authenticated.")
                else
                    Debug.WriteLine("Fetch started")
                    
                    let currentCourses = app.Runner.CurrentModel.CourseListPage.Courses
                    let! result = CourseLoader.tryLoadCoursesFromCARE currentCourses
                    
                    match result with
                    | Ok updateResult ->
                        match currentCourses = fst updateResult with
                        | true ->
                            Debug.WriteLine "Fetch finished successfully. But no updates."
                            return ()
                        | false ->
                            Debug.WriteLine "Fetch finished successfully. And we've got news!."
                            
                            /// Dispatching that message leaves the responsibility
                            /// for updating the model at the page itself, so we
                            /// don't need to re-implement that here
                            let msg =
                                (App.Msg.CourseListPageMsg(
                                    CourseListPage.Msg.CourseLoadingSucceeded(updateResult)))
                            
                            app.Runner.Dispatch msg
                            
                            /// Waiting for 250 ms and performing the serialization
                            /// in the app's on sleep method ensures we get a fresh
                            /// list when we open the app for the next time
                            do! Async.Sleep 250
                            app.SendSleep()
                            
                            /// Send the notification
                            this.notifyAboutUpdates()
                            ()
                    | Error errorMessage ->
                        Debug.WriteLine(sprintf "Fetch failed: %s" errorMessage)
            with e ->
                Debug.WriteLine(sprintf "Fetch failed (Exception thrown): %s" e.Message)
              
            this.JobFinished (parameters, false)
        }
        
    /// Called by Android whenever the job should execute
    override this.OnStartJob parameters =
        /// Fire and forget, we don't wait here
        Async.Start (this.performUpdate parameters)
        true
        
    /// Called by Android whenever the job was canceled by the OS
    override this.OnStopJob _ =
        Debug.WriteLine "Fetch shut down by Android. Trying to reschedule."
        true
