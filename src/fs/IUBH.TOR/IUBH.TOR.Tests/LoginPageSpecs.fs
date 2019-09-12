namespace IUBH.TOR.Tests

open Fabulous.XamarinForms
open FsUnit
open IUBH.TOR
open IUBH.TOR.Tests
open Xunit

module ``Login Page Specification`` =
    let initialModel() = fst (LoginPage.init())
    
    type ``When the login model is being initialized ``() =
        [<Fact>]
        let ``Then the User name is empty``() =
            let model = initialModel()
            model.UserName |> should be EmptyString

        [<Fact>]
        let ``Then the Password is empty``() =
            let model = initialModel()
            model.Password |> should be EmptyString
            
        [<Fact>]
        let ``Then Credentials are not seen as provided``() =
            let model = initialModel()
            model.IsCredentialsProvided |> should be False
    
    type ``IsCredentialsProvided is being recalulated``() =
        [<Theory>]
        [<InlineData("valid", true)>]
        [<InlineData(" ", false)>]
        [<InlineData("", false)>]
        let ``When the User Name changes `` userName isCredentialsProvided =
            let model = initialModel()
            let model = { model with Password = "valid" }
            
            let model, _, _ = LoginPage.update (LoginPage.Msg.LoginUserNameChanged(userName)) model
            model.IsCredentialsProvided |> should equal isCredentialsProvided
            
        [<Theory>]
        [<InlineData("valid", true)>]
        [<InlineData(" ", false)>]
        [<InlineData("", false)>]
        let ``When the Password changes``password isCredentialsProvided =
            let model = initialModel()
            let model = { model with UserName = "valid"; }
            
            let model, _, _ = LoginPage.update (LoginPage.Msg.LoginPasswordChanged(password)) model
            model.IsCredentialsProvided |> should equal isCredentialsProvided

    type ``The Sign in Button can only be used``() =
        [<Theory>]
        [<InlineData(true)>]
        [<InlineData(false)>]
        let ``When the Credentials are provided`` credentialsProvided =
            let model = initialModel()
            let model = { model with IsCredentialsProvided = credentialsProvided }
            
            let view = (LoginPage.view model ignore)
            let button = view |> tryFindViewElement "SignInButton"
            button.IsSome |> should be True
            
            let button = ButtonViewer(button.Value)
            button.CanExecute |> should equal credentialsProvided
    
    type ``When a login is being started``() =
        [<Fact>]
        let ``IsBusy is set to true``() =
            let model = initialModel()
            model.IsBusy |> should be False
            
            let model, _, _ = LoginPage.update LoginPage.Msg.StartLogin model
            model.IsBusy |> should be True
            
        [<Fact>]
        let ``The Log in Command is being issued and the Credentials are being passed``() =
            let userName = randomString()
            let password = randomString()
            
            let model = initialModel()
            let model = { model with UserName = userName; Password = password }
                
            let _, cmd, _ = LoginPage.update LoginPage.Msg.StartLogin model
            
            let credentials = { UserName = userName; Password = password }
            
            cmd
            |> dispatchesMessage (LoginPage.Msg.CmdLogin(credentials))
            |> should be True
    
    type ``When a login succeeds``() =
        [<Fact>]
        let ``This information is passed to the parent``() =
            let model = initialModel()
            
            let _, _, externalMessage = LoginPage.update LoginPage.Msg.LoginSucceeded model
            externalMessage |> should equal LoginPage.ExternalMsg.LoginSucceeded
            
        [<Fact>]
        let ``IsBusy is set to false``() =
            let model = initialModel()
            let model = { model with IsBusy = true }
            model.IsBusy |> should be True
            
            let model, _, _ = LoginPage.update LoginPage.Msg.LoginSucceeded model
            model.IsBusy |> should be False
        
        [<Fact>]
        let ``The LoginModel is being reset to its defaults``() =
            let model = initialModel()
            let model = { model with UserName = "User"; Password = "Pass"; IsCredentialsProvided = true }
            
            model.UserName |> should equal "User"
            model.Password |> should equal "Pass"
            model.IsCredentialsProvided |> should be True
            
            let model, _, _ = LoginPage.update LoginPage.Msg.LoginSucceeded model
            
            model.UserName |> should equal ""
            model.Password |> should equal ""
            model.IsCredentialsProvided |> should be False
            
    type ``When a login failed``() =
        [<Fact>]
        let ``IsBusy is set to false``() =
            let model = initialModel()
            let model = { model with IsBusy = true }
            model.IsBusy |> should be True
            
            let model, _, _ = LoginPage.update (LoginPage.Msg.LoginFailed("")) model
            model.IsBusy |> should be False
            
        [<Fact>]
        let ``An Error Alert is being shown``() =
            let expectedErrorMessage = randomString()
            
            let model = initialModel()
            let _, cmd, externalMessage = LoginPage.update (LoginPage.Msg.LoginFailed(expectedErrorMessage)) model
            
            externalMessage |> should equal (LoginPage.ExternalMsg.CmdError(expectedErrorMessage))
            
    type ``When the Login Command is being executed``() =
        [<Fact>]
        let ``A LoginSucceeded message is being returned when credentials are valid and storage was successful``() =
            let validate c = async { return Ok(c) }
            let save c = async { return Ok() }
            let credentials = { UserName = ""; Password = "" }
            
            let result = LoginPage.tryLogIn credentials validate save |> Async.RunSynchronously
            result |> should equal LoginPage.Msg.LoginSucceeded
            
        [<Fact>]
        let ``A LoginFailed message is being returned when the credentials are invalid``() =
            let errorMessage = randomString()
            let validate c = async { return Error(errorMessage) }
            let save c = async { return Ok() }
            let credentials = { UserName = ""; Password = "" }
            
            let result = LoginPage.tryLogIn credentials validate save |> Async.RunSynchronously
            result |> should equal (LoginPage.Msg.LoginFailed(errorMessage))
            
        [<Fact>]
        let ``A LoginFailed message is being returned when the credentials could not be saved``() =
            let errorMessage = randomString()
            let validate c = async { return Ok(c) }
            let save c = async { return Error(errorMessage) }
            let credentials = { UserName = ""; Password = "" }
            
            let result = LoginPage.tryLogIn credentials validate save |> Async.RunSynchronously
            result |> should equal (LoginPage.Msg.LoginFailed(errorMessage))
