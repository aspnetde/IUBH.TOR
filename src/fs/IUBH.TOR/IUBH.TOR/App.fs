namespace IUBH.TOR

open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Newtonsoft.Json
open System
open System.Diagnostics
open Xamarin.Forms

module App =
    type SessionState =
        { IsAuthenticated: bool }
    
    type Model =
      { LoginPage: LoginPage.Model
        CourseListPage: CourseListPage.Model
        Session: SessionState }
    
    type Msg =
        | LoginPageMsg of LoginPage.Msg
        | CourseListPageMsg of CourseListPage.Msg
       
    /// Whenever the user logs in, this event is being triggered.         
    let LoggedIn = new Event<Object>()
    let private notifyAboutLogin() =
        LoggedIn.Trigger(Object(), EventArgs.Empty)
        None
    
    /// Whenever the user logs out, this event is being triggered.
    let LoggedOut = new Event<Object>()
    let private notifyAboutLogout() =
        LoggedOut.Trigger(Object(), EventArgs.Empty)
        None
        
    /// Initializes the model
    let init isAuthenticated =
        let loginModel, loginCmd = LoginPage.init()
        let courseListModel, courseListCmd = CourseListPage.init()
        
        let model =
            { LoginPage = loginModel
              CourseListPage = courseListModel
              Session = { IsAuthenticated = isAuthenticated } }
        
        let cmd = Cmd.batch [
            yield loginCmd
            yield courseListCmd
            if isAuthenticated then yield Cmd.ofMsgOption (notifyAboutLogin())
        ]
        
        model, cmd
    
    /// If credentials are being provided, we are already authenticated
    /// Therefore we check here if we can retrieve the credentials from
    /// the Secure Storage.
    let initWithSession () =
        let credentials =
            Authentication.tryGetCredentialsFromSecureStorage()
            |> Async.RunSynchronously
            
        let isAuthenticated =
            match credentials with
            | Ok _ -> true
            | Error _-> false
            
        init isAuthenticated
        
    let private showErrorAlert errorMessage = async {
        do! Application.Current.MainPage
                .DisplayAlert("Error", errorMessage, "Okay") |> Async.AwaitTask
        return None
    }
    
    let update (msg: Msg) (model: Model) =
        /// For both the Login Page and the Course List Page their
        /// "external messages" are being processed here, too. This
        /// way they are able to communicate with its parent without
        /// holding any strong reference.
        match msg with
        | LoginPageMsg msg ->
            let m, c, em = LoginPage.update msg model.LoginPage
            let s, ec =
                match em with
                | LoginPage.ExternalMsg.NoOp -> model.Session, Cmd.none
                | LoginPage.ExternalMsg.LoginSucceeded ->
                    let model = { model.Session with IsAuthenticated = true } 
                    let cmd = Cmd.batch [
                        Cmd.ofMsgOption (notifyAboutLogin())
                        Cmd.ofMsg (Msg.CourseListPageMsg(CourseListPage.Msg.StartLoadingCourses))
                    ]
                    model, cmd
                | LoginPage.ExternalMsg.CmdError errorMessage ->
                    model.Session, Cmd.ofAsyncMsgOption (showErrorAlert errorMessage)
            let model = { model with LoginPage = m; Session = s }
            
            model, Cmd.batch [ Cmd.map LoginPageMsg c; ec ]
        
        | CourseListPageMsg msg ->
            let m, c, em = CourseListPage.update msg model.CourseListPage
            let model = { model with CourseListPage = m; }
            let model, ec =
                match em with
                | CourseListPage.ExternalMsg.NoOp -> model, Cmd.none
                | CourseListPage.ExternalMsg.LogoutSucceeded ->
                    (fst (init false)), Cmd.ofMsgOption (notifyAboutLogout())
                | CourseListPage.ExternalMsg.CmdError errorMessage ->
                    model, Cmd.ofAsyncMsgOption (showErrorAlert errorMessage)
            
            model, Cmd.batch [ Cmd.map CourseListPageMsg c; ec ]

    let view (model: Model) dispatch =
        View.NavigationPage(
            pages = [
                if model.Session.IsAuthenticated then
                    for page in CourseListPage.view model.CourseListPage (CourseListPageMsg >> dispatch) do
                        yield page
                else
                    yield LoginPage.view model.LoginPage (LoginPageMsg >> dispatch)
            ],
            // Right now we can only pop a page at one point – the course detail page
            // As soon as the navgation stack changes in that regard, this must be adjusted.
            popped = (fun _ -> dispatch (CourseListPageMsg(CourseListPage.Msg.CourseDeselected))),
            visual = VisualMarker.Material,
            barBackgroundColor = Constants.UI.Color.NavigationBarBackground,
            barTextColor = Constants.UI.Color.NavigationBarText)

    let program = Program.mkProgram initWithSession update view

type App() as app =
    inherit Application()
    
    [<Literal>]
    let ModelId = "IUBH.TOR"

    let runner =
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> XamarinFormsProgram.run app

#if DEBUG
    do runner.EnableLiveUpdate()
#endif

    let loggedIn = new Event<_>()
    let loggedOut = new Event<_>()
    
    [<CLIEvent>]
    member this.LoggedIn = loggedIn.Publish

    [<CLIEvent>]
    member this.LoggedOut = loggedOut.Publish
    
    member this.Runner
        with get() = runner
    
    override __.OnSleep() =
        /// Then the app "fells asleep" we serialize the current model
        let model = runner.CurrentModel
        
        /// We don't want to have any critical information saved in a
        /// place that is not secure, so we need to remove the login
        /// data like user name and especially password.
        let model = { model with LoginPage = (fst (LoginPage.init())) }
        let json = JsonConvert.SerializeObject(model)
        
        app.Properties.[ModelId] <- json

    override __.OnResume() = 
        try
            /// When we can properly deserialize the model, we are
            /// replacing the app's current model with that one.
            match app.Properties.TryGetValue ModelId with
            | true, (:? string as json) -> 
                let model = JsonConvert.DeserializeObject<App.Model>(json)
                runner.SetCurrentModel (model, Cmd.none)
                
                if model.Session.IsAuthenticated &&
                   model.CourseListPage.State = CourseListPage.CourseListState.Loading then
                    runner.Dispatch (App.Msg.CourseListPageMsg(CourseListPage.Msg.StartLoadingCourses))
            | _ -> ()
            
            /// If we are authenticated and the last update check had been
            /// more than 15 minutes ago, we trigger loading the courses.
            if runner.CurrentModel.Session.IsAuthenticated then
                match runner.CurrentModel.CourseListPage.UpdatedAt with
                | Some date when date < DateTime.UtcNow.AddMinutes(-15.0) ->
                    runner.Dispatch (App.Msg.CourseListPageMsg(CourseListPage.Msg.StartLoadingCourses))
                | _ -> ()
        with ex -> 
            Debug.WriteLine ex

    override this.OnStart() =
        /// The requirements for the first (cold) start are the same as
        /// for the waking up scenario, so we can use OnResume() here. 
        this.OnResume()
        
        /// Attach ourselves on the LoggedIn and LoggedOut events of the app.
        /// As this is valid throughout the whole lifecycle of the app, there
        /// is no need to detach those event handlers again.
        App.LoggedIn.Publish.Add (fun _ -> loggedIn.Trigger(this, EventArgs.Empty))
        App.LoggedOut.Publish.Add (fun _ -> loggedOut.Trigger(this, EventArgs.Empty))
