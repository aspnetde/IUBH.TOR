namespace IUBH.TOR.Tests.AppSpecs

open FsUnit
open IUBH.TOR
open IUBH.TOR.Tests
open Xunit

module ``App Startup`` =
    let initialModel() = fst (App.init false)
    
    type ``When the user is authenticated``() =
        [<Fact>]
        let ``Then the course list page is being presented``() =
            let model = initialModel()
            let model = { model with Session = { IsAuthenticated = true } }
            
            let view = (App.view model ignore)
            
            let page = view |> tryFindViewElement "CourseListPage"
            page.IsSome |> should be True
    
    type ``When the user has not been authenticated``() =
        [<Fact>]
        let ``Then the login page is being presented``() =
            let model = initialModel()
            let model = { model with Session = { IsAuthenticated = false } }
            
            let view = (App.view model ignore)
            
            let page = view |> tryFindViewElement "LoginPage"
            page.IsSome |> should be True

    type ``When a login did succeed``() =
        [<Fact>]
        let ``Then IsAuthenticated is set to true``() =
           let model = initialModel()
           let model = { model with Session = { model.Session with IsAuthenticated = false } }
           model.Session.IsAuthenticated |> should be False
           
           let message = App.Msg.LoginPageMsg(LoginPage.Msg.LoginSucceeded)
           let model = fst (App.update message model)
           model.Session.IsAuthenticated |> should be True
        
        [<Fact>]
        let ``Then the Course List is being initialized``() =
            let model = initialModel()
            let message = App.Msg.LoginPageMsg(LoginPage.Msg.LoginSucceeded)
            let _, cmd = App.update message model
            
            cmd
            |> dispatchesMessage (App.Msg.CourseListPageMsg(CourseListPage.Msg.StartLoadingCourses))
            |> should be True
    
    type ``When a logout did succeed``() =
        [<Fact>]
        let ``Then the whole application model is being reset``() =
            let model = initialModel()
            let model = { model with LoginPage = { model.LoginPage with UserName = "Leon" } }
            
            model.LoginPage.UserName |> should equal "Leon"
            
            let message = App.Msg.CourseListPageMsg(CourseListPage.Msg.LogoutSucceeded) 
            let model = fst (App.update message model)
            model.LoginPage.UserName |> should be NullOrEmptyString
