module IUBH.TOR.LoginPage

open Fabulous
open Fabulous.XamarinForms
open System
open Xamarin.Forms

type Model =
    { UserName: string
      Password: string
      IsCredentialsProvided: bool
      IsBusy: bool }
    
type ExternalMsg =
    | NoOp
    | LoginSucceeded
    | CmdError of errorMessage: string
    
type Msg =
    | LoginUserNameChanged of userName: string
    | LoginPasswordChanged of password: string
    | FocusPasswordEntry
    | StartLogin
    | LoginSucceeded
    | LoginFailed of errorMessage: string
    | CmdLogin of Credentials
    
let initModel () =
  { UserName = ""
    Password = ""
    IsCredentialsProvided = false
    IsBusy = false }
                
let init() =
    initModel(), Cmd.none

/// Tries to log in to CARE given the user name and the password
/// provided by the user in the login form.
let tryLogIn credentials tryValidateCredentials trySaveCredentials =
    async {
        let! result =
            tryValidateCredentials credentials
            |> AsyncResult.bind trySaveCredentials
        
        match result with
        | Ok _ -> return LoginSucceeded
        | Error errorMessage -> return LoginFailed(errorMessage)
    }

let credentialsProvided model =
    not (String.IsNullOrWhiteSpace(model.UserName) || String.IsNullOrWhiteSpace(model.Password))

let private passwordEntryRef = ViewRef<Entry>()

let update (msg: Msg) (model: Model) =
    match msg with
    | LoginPasswordChanged password ->
        let model = { model with Password = password }
        let model = { model with IsCredentialsProvided = credentialsProvided model }
        model, Cmd.none, NoOp
    | LoginUserNameChanged userName -> 
        let model = { model with UserName = userName }
        let model = { model with IsCredentialsProvided = credentialsProvided model }
        model, Cmd.none, NoOp
    | FocusPasswordEntry ->
        match passwordEntryRef.TryValue with
        | Some entry -> entry.Focus() |> ignore
        | _ -> ()
        model, Cmd.none, NoOp
    | StartLogin ->
        let credentials = { UserName = model.UserName; Password = model.Password }
        { model with IsBusy = true }, Cmd.ofMsg (CmdLogin credentials), NoOp
    | LoginFailed errorMessage ->
        { model with IsBusy = false }, Cmd.none, ExternalMsg.CmdError(errorMessage)
    | LoginSucceeded ->
        let model = initModel()
        model, Cmd.none, ExternalMsg.LoginSucceeded
    | CmdLogin credentials ->
        let validate = Authentication.tryValidateCredentials
        let save = Authentication.trySaveCredentialsToSecureStorage
        model, Cmd.ofAsyncMsg (tryLogIn credentials validate save), NoOp

let view (model: Model) dispatch =
    View.ContentPage(
        classId = "LoginPage",
        title = "IUBH TOR",
        backgroundColor = Constants.UI.Color.PageBackground,
        content = View.ScrollView(
            content = View.StackLayout(
                padding = 30.0,
                children = [
                    View.Frame(
                        verticalOptions = LayoutOptions.CenterAndExpand,
                        content = View.StackLayout(
                            padding = 0.0,
                            children = [
                                View.Image(
                                    source = "logo.png",
                                    heightRequest = 102.0,
                                    horizontalOptions = LayoutOptions.Fill)
                                View.Entry(
                                    placeholder = "User name",
                                    returnType = ReturnType.Next,
                                    tabIndex = 0,
                                    margin = Thickness(0.0, 0.0, 0.0, 10.0),
                                    backgroundColor = Constants.UI.Color.EntryBackground,
                                    placeholderColor = Constants.UI.Color.PlaceholderText,
                                    keyboard = Keyboard.Create(KeyboardFlags.None),
                                    isEnabled = (not model.IsBusy),
                                    textChanged = (fun args -> (dispatch (LoginUserNameChanged args.NewTextValue))),
                                    completed = (fun _ -> dispatch FocusPasswordEntry))
                                View.Entry(
                                    placeholder = "Password",
                                    ref = passwordEntryRef,
                                    returnType = ReturnType.Done,
                                    tabIndex = 1,
                                    margin = Thickness(0.0, 0.0, 0.0, 10.0),
                                    backgroundColor = Constants.UI.Color.EntryBackground,
                                    placeholderColor = Constants.UI.Color.PlaceholderText,
                                    isPassword = true,
                                    isEnabled = (not model.IsBusy),
                                    textChanged = (fun args -> (dispatch (LoginPasswordChanged args.NewTextValue))),
                                    completed = (fun _ -> if model.IsCredentialsProvided then dispatch StartLogin))
                                View.Button(
                                    classId = "SignInButton",
                                    text = "Sign in", 
                                    backgroundColor = Constants.UI.Color.ButtonBackground,
                                    isVisible = (not model.IsBusy),
                                    command = (fun () -> dispatch StartLogin),
                                    canExecute = model.IsCredentialsProvided)
                                View.ActivityIndicator(
                                    isRunning = true,
                                    verticalOptions = LayoutOptions.CenterAndExpand,
                                    horizontalOptions = LayoutOptions.CenterAndExpand,
                                    color = Constants.UI.Color.ActivityIndicator,
                                    heightRequest = 36.0,
                                    isVisible = model.IsBusy)
                            ]
                        )
                    )
                ]
            )
        )
    )
