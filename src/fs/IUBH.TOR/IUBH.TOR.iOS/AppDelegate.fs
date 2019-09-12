namespace IUBH.TOR.iOS

open System
open System.Diagnostics
open IUBH.TOR
open UIKit
open UserNotifications
open Foundation
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit FormsApplicationDelegate ()
    
    [<DefaultValue>]
    val mutable private app: App
    
    /// Enables background synchronization and asks the user
    /// for permission of doing so, if it's not already granted.
    let enableBackgroundSync() =
        UIApplication.SharedApplication.InvokeOnMainThread(fun () ->
            /// Set the update interval
            let interval = Constants.Backgrounding.FetchIntervalInMinutes * 60.0
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(interval)
            
            /// Request Permission for sending Notifications
            let printError error =
                Debug.WriteLine (sprintf "Could not enable Background Sync: %s" error)
            
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert,
                fun success error ->
                    if not success then
                        printError error.LocalizedFailureReason)
        )
    
    /// Disables background synchronization. From now on now
    /// notification will be sent to the user anymore. 
    let disableBackgroundSync() =
        Debug.WriteLine "Disabling Background Sync"
        
        UIApplication.SharedApplication.InvokeOnMainThread(fun () ->
            UIApplication.SharedApplication
                .SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalNever)
        )
    
    /// Sends a notification that informs the user about some
    /// changes in their transcript of records. 
    let notifyAboutUpdates() =
        UNUserNotificationCenter.Current.GetNotificationSettings(fun settings ->
            match settings.AuthorizationStatus with
            | UNAuthorizationStatus.Authorized ->
                let content = new UNMutableNotificationContent()
                content.Title <- Constants.Backgrounding.NotificationTitle
                content.Body <- Constants.Backgrounding.NotificationText
                content.Sound <- UNNotificationSound.Default
                
                let trigger = UNTimeIntervalNotificationTrigger.CreateTrigger (0.01, false)
                let identifier = sprintf "iubh-tor-update-%A" (Guid.NewGuid())
                
                let request = UNNotificationRequest.FromIdentifier(identifier, content, trigger)
                UNUserNotificationCenter.Current.AddNotificationRequest(request, null)
            | _ -> ()
        )

    /// This method is the central entry point to the whole iOS app
    override this.FinishedLaunching (uiApplication, options) =
        Forms.Init()
        FormsMaterial.Init()
        
        /// We want to know when background synchronization should be
        /// enabled or disabled, so we can react on the platform.
        App.LoggedIn.Publish.Add (fun _ -> enableBackgroundSync())
        App.LoggedOut.Publish.Add (fun _ -> disableBackgroundSync())
        
        /// Create our Fabulous app
        let app = App()
        
        this.app <- app 
        this.LoadApplication(app)
        
        base.FinishedLaunching(uiApplication, options)
        
    /// Performs the background update check
    override this.PerformFetch (_, completionHandler) =
        let performFetch =
            async {
                try
                    if not (this.app.Runner.CurrentModel.Session.IsAuthenticated) then
                        Debug.WriteLine("Could not start Fetch: Not authenticated.")
                    else
                        Debug.WriteLine("Fetch started")
                        
                        /// Looks for courses at the CARE backend
                        let currentCourses = this.app.Runner.CurrentModel.CourseListPage.Courses
                        let! result = CourseLoader.tryLoadCoursesFromCARE currentCourses
                        
                        match result with
                        | Ok updateResult ->
                            /// When current courses and updated courses are the same,
                            /// We don't need to notify anyone. But if they are not, we
                            /// can send a system's notification
                            match currentCourses = fst updateResult with
                            | true ->
                                Debug.WriteLine "Fetch finished successfully. But no updates."
                                completionHandler.Invoke UIBackgroundFetchResult.NoData
                            | false ->
                                Debug.WriteLine "Fetch finished successfully. And we've got news!."
                                
                                /// Dispatching that message leaves the responsibility
                                /// for updating the model at the page itself, so we
                                /// don't need to re-implement that here
                                let msg =
                                    (App.Msg.CourseListPageMsg(
                                        CourseListPage.Msg.CourseLoadingSucceeded(updateResult)))
                                
                                this.app.Runner.Dispatch msg
                                
                                /// Waiting for 250 ms and performing the serialization
                                /// in the app's on sleep method ensures we get a fresh
                                /// list when we open the app for the next time
                                do! Async.Sleep 250
                                this.app.SendSleep()
                                
                                /// Send the notification
                                notifyAboutUpdates()
                                
                                completionHandler.Invoke UIBackgroundFetchResult.NewData
                        | Error errorMessage ->
                            Debug.WriteLine(sprintf "Fetch failed: %s" errorMessage)
                            completionHandler.Invoke UIBackgroundFetchResult.Failed
                    with e ->
                        Debug.WriteLine(sprintf "Fetch failed (Exception thrown): %s" e.Message)
                        completionHandler.Invoke UIBackgroundFetchResult.Failed
            }
            
        Async.Start performFetch
            
module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main(args, null, "AppDelegate")
        0
