namespace IUBH.TOR.Tests

open System
open FsUnit
open IUBH.TOR
open IUBH.TOR.Tests
open Xunit

module ``Course List Page Specification`` =
    let initialModel() =
        fst (CourseListPage.init())
        
    let mockExistingCourses: (Course * CourseUpdateDate) list =
        [ DemoData.course(), CourseUpdateDate(DateTime.UtcNow) ]
        
    type ``When the logout is being started``() =
        [<Fact>]
        let ``The Logout Command is being dispatched``() =
            let _, cmd, _ = CourseListPage.update CourseListPage.Msg.StartLogout (initialModel())
            cmd |> dispatchesMessage CourseListPage.Msg.CmdLogout |> should be True
            
    type ``When the logout did fail``() =
        [<Fact>]
        let ``An Error Alert is being shown``() =
            let errorMessage = randomString()
            let _, _, externalMessage = CourseListPage.update (CourseListPage.Msg.LogoutFailed(errorMessage)) (initialModel())
            externalMessage |> should equal (CourseListPage.ExternalMsg.CmdError(errorMessage))

    type ``When the logout did succeed``() =
        [<Fact>]
        let ``The information is passed to the parent``() =
            let model = initialModel()
            let _, _, externalMessage = CourseListPage.update (CourseListPage.Msg.LogoutSucceeded) model
            externalMessage |> should equal CourseListPage.ExternalMsg.LogoutSucceeded

    type ``When the Logout Command is being executed``() =
        [<Fact>]
        let ``A LogoutSucceeded message is being returned when the credentials could be removed``() =
            let remove () = Ok()
            let result = CourseListPage.tryLogOut remove
            result |> should equal CourseListPage.Msg.LogoutSucceeded
            
        [<Fact>]
        let ``A LogoutFailed message is being returned when the credentials could not be removed``() =
            let errorMessage = randomString()
            let remove () = Error errorMessage
            let result = CourseListPage.tryLogOut remove
            result |> should equal (CourseListPage.Msg.LogoutFailed(errorMessage))
            
    type ``When courses are started be loaded``() =
        [<Fact>]
        let ``The Course Loading Command is being triggered``() =
            let _, cmd, _ = CourseListPage.update CourseListPage.Msg.StartLoadingCourses (initialModel())
            cmd |> dispatchesMessage CourseListPage.Msg.CmdLoadCourses |> should be True
            
        [<Fact>]
        let ``The State is set to Loading``() =
            let model = initialModel()
            let model = { model with State = CourseListPage.CourseListState.Empty }
                        
            let model, _, _ = CourseListPage.update CourseListPage.Msg.StartLoadingCourses model
            model.State |> should equal CourseListPage.CourseListState.Loading

    type ``When courses are started to be refreshed``() =
        [<Fact>]
        let ``The Course Loading Command is being triggered``() =
            let _, cmd, _ = CourseListPage.update CourseListPage.Msg.StartRefreshingCourses (initialModel())
            cmd |> dispatchesMessage CourseListPage.Msg.CmdLoadCourses |> should be True
            
        [<Fact>]
        let ``The State is set to Refreshing``() =
            let model = initialModel()
            let model = { model with State = CourseListPage.CourseListState.Empty }
                        
            let model, _, _ = CourseListPage.update CourseListPage.Msg.StartRefreshingCourses model
            model.State |> should equal CourseListPage.CourseListState.Refreshing
    
    type ``When the Course Loading Command is being executed``() =
        [<Fact>]
        let ``The list of courses and an update Date is being dispatched in case of a success``() =
            let courseLoader courses =
                async { return Ok (courses, DateTime.UtcNow) }
                
            let result =
                CourseListPage.tryLoadCourses courseLoader mockExistingCourses
                |> Async.RunSynchronously
                    
            match result with
            | CourseListPage.Msg.CourseLoadingSucceeded (courses, updateDate) ->
                (fst courses.Head).Id |> should equal 69
                updateDate > DateTime.MinValue |> should be True
            | CourseListPage.Msg.CourseLoadingFailed errorMessage ->
                failwith errorMessage
            | _ -> raise (InvalidOperationException())
            
        [<Fact>]
        let ``An error message being dispatched when an error occured``() =
            let expectedErrorMessage = randomString()
            
            let courseLoader courses =
                async { return Error expectedErrorMessage }
                
            let result =
                CourseListPage.tryLoadCourses courseLoader mockExistingCourses
                |> Async.RunSynchronously
                    
            match result with
            | CourseListPage.Msg.CourseLoadingFailed errorMessage ->
                errorMessage |> should equal expectedErrorMessage
            | _ -> raise (InvalidOperationException())
                    
    type ``When courses have been loaded successfully``() =        
        [<Fact>]
        let ``They are saved in the model``() =
            let model = initialModel()
            model.Courses.Length |> should equal 0
            
            let course = DemoData.course(), CourseUpdateDate(DateTime.UtcNow)
            let now = DateTime.UtcNow
            
            let payload = [ course ], now
            let model, _, _ = CourseListPage.update (CourseListPage.Msg.CourseLoadingSucceeded(payload)) model
            model.Courses.Length |> should equal 1
            model.Courses.Head |> should equal course
            
        [<Fact>]
        let ``The State is set to Ready when there is at leat one course``() =
            let model = initialModel()
            model.State |> should equal CourseListPage.CourseListState.Loading
            
            let payload = [ DemoData.course(), CourseUpdateDate(DateTime.UtcNow) ], DateTime.UtcNow
            let model, _, _ = CourseListPage.update (CourseListPage.Msg.CourseLoadingSucceeded(payload)) model
            model.State |> should equal CourseListPage.CourseListState.Ready
        
        [<Fact>]
        let ``The last update date is being set``() =
            let model = initialModel()
            model.UpdatedAt |> Option.isNone |> should be True
            
            let updateDate = DateTime.UtcNow
            let payload = [ DemoData.course(), CourseUpdateDate(DateTime.UtcNow) ], updateDate
            let model, _, _ = CourseListPage.update (CourseListPage.Msg.CourseLoadingSucceeded(payload)) model
            model.UpdatedAt |> Option.isNone |> should be False
            model.UpdatedAt.Value |> should equal updateDate
        
        [<Fact>]    
        let ``The State is set to Empty when there is no course``() =
            let model = initialModel()
            model.State |> should equal CourseListPage.CourseListState.Loading
            
            let payload = [], DateTime.UtcNow
            let model, _, _ = CourseListPage.update (CourseListPage.Msg.CourseLoadingSucceeded(payload)) model
            model.State |> should equal CourseListPage.CourseListState.Empty
            
    type ``When courses could not be loaded``() =
        [<Fact>]
        let ``The state is set to Loading Error``() =
            let expectedErrorMessage = randomString()
            let model = initialModel()
            model.State |> should equal CourseListPage.CourseListState.Loading            
            
            let model, _, _ = CourseListPage.update (CourseListPage.Msg.CourseLoadingFailed(expectedErrorMessage)) model
            model.State |> should equal (CourseListPage.CourseListState.LoadingError(expectedErrorMessage))
            
    type ``When a course should be selected ``() =
        [<Fact>]
        let ``It is set set in the model ``() =
            let model = initialModel()
            let course = DemoData.course(), CourseUpdateDate(DateTime.UtcNow)
            let courses = [ course ]
            
            let model = { model with Courses = [ course ] }
            model.SelectedCourse.IsSome |> should be False
            
            let model, _, _ = CourseListPage.update (CourseListPage.Msg.CourseSelected(courses.Head)) model
            model.SelectedCourse.IsSome |> should be True
            model.SelectedCourse.Value |> should equal courses.Head

    type ``When a course had been selected``() =
        [<Fact>]
        let ``Then its detail page is being presented``() =
            let model = initialModel()
            let model = { model with SelectedCourse = Some(DemoData.course(), CourseUpdateDate(DateTime.UtcNow)) }
            
            let view = (CourseListPage.view model ignore)
            
            let page =
                view
                |> List.map (fun x -> x |> tryFindViewElement "CourseDetailPage")
                |> List.tryFind (fun x -> x.IsSome)
                
            page.IsSome |> should be True

    type ``When a course had been deselected``() =
        [<Fact>]
        let ``Then the Course is being reset in the model``() =
            let model = initialModel()
            let model = { model with SelectedCourse = Some(DemoData.course(), CourseUpdateDate(DateTime.UtcNow)) }
            model.SelectedCourse.IsSome |> should be True
            
            let model, _, _ = CourseListPage.update CourseListPage.Msg.CourseDeselected model
            model.SelectedCourse.IsSome |> should be False
            
        [<Fact>]
        let ``Then its detail page is not being presented``() =
            let model = initialModel()
            let model = { model with SelectedCourse = Some(DemoData.course(), CourseUpdateDate(DateTime.UtcNow)) }
            let model, _, _ = CourseListPage.update CourseListPage.Msg.CourseDeselected model
            
            let view = (CourseListPage.view model ignore)
            
            let page =
                view
                |> List.map (fun x -> x |> tryFindViewElement "CourseDetailPage")
                |> List.tryFind (fun x -> x.IsSome)
                
            page.IsSome |> should be False
