module IUBH.TOR.CourseListPage

open System
open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms

type CourseListState =
    | Loading
    | LoadingError of errorMessage: string
    | Empty
    | Ready
    | Refreshing
    
type Model =
    { Courses: (Course * CourseUpdateDate) list
      SelectedCourse: (Course * CourseUpdateDate) option
      State: CourseListState
      UpdatedAt: DateTime option  }

type ExternalMsg =
    | NoOp
    | LogoutSucceeded
    | CmdError of errorMessage: string

type Msg =
    | StartLoadingCourses // Used for the initial and manual loading of courses
    | StartRefreshingCourses // Used for the list's "pull to refresh" command
    | CourseLoadingFailed of errorMessage: string
    | CourseLoadingSucceeded of courses: (Course * CourseUpdateDate) list * updateDate: DateTime
    | CourseSelected of course: (Course * CourseUpdateDate)
    | CourseDeselected
    | StartLogout
    | LogoutSucceeded
    | LogoutFailed of errorMessage: string
    | CmdLoadCourses
    | CmdLogout
    
let initModel () =
  { Courses = []
    SelectedCourse = None
    State = Loading
    UpdatedAt = None }

let init() =
    initModel(), Cmd.none

/// Tries to load courses from CARE
let tryLoadCourses load existingCourses =
    async {
        let! result = load existingCourses
        match result with
        | Ok courses ->
            return CourseLoadingSucceeded courses
        | Error errorMessage ->
            return CourseLoadingFailed errorMessage
    }

let tryLogOut tryRemoveCredentials =
    match tryRemoveCredentials() with
    | Ok _ -> LogoutSucceeded
    | Error errorMessage -> LogoutFailed errorMessage

let update (msg: Msg) (model: Model) =
    match msg with
    | StartLoadingCourses ->
        let model = { model with State = Loading }
        model, Cmd.ofMsg CmdLoadCourses, NoOp
    | StartRefreshingCourses ->
        let model = { model with State = Refreshing }
        model, Cmd.ofMsg CmdLoadCourses, NoOp
    | CourseLoadingSucceeded (courses, updateDate) ->
        let state = if courses.IsEmpty then CourseListState.Empty else CourseListState.Ready
        let model = { model with Courses = courses; State = state; UpdatedAt = Some(updateDate) }
        model, Cmd.none, NoOp
    | CourseLoadingFailed errorMessage ->
        let model = { model with State = CourseListState.LoadingError(errorMessage) }
        model, Cmd.none, NoOp
    | CourseSelected course ->
        let model = { model with SelectedCourse = Some(course) }
        model, Cmd.none, NoOp
    | CourseDeselected ->
        let model = { model with SelectedCourse = None }
        model, Cmd.none, NoOp
    | StartLogout ->
        model, Cmd.ofMsg CmdLogout, NoOp
    | LogoutFailed errorMessage ->
        model, Cmd.none, CmdError(errorMessage)
    | LogoutSucceeded ->
        initModel(), Cmd.none, ExternalMsg.LogoutSucceeded
    | CmdLoadCourses ->
        let cmd = Cmd.ofAsyncMsg (tryLoadCourses CourseLoader.tryLoadCoursesFromCARE model.Courses)
        model, cmd, NoOp
    | CmdLogout ->
        model, (Cmd.ofMsg (tryLogOut Authentication.tryRemoveCredentialsFromSecureStorage)), NoOp

let courseCell (course, courseUpdateDate) dispatch =
    View.Frame(
        margin = Thickness(15.0, 15.0, 15.0, 5.0),
        content = View.StackLayout(
            orientation = StackOrientation.Horizontal,
            children = [
                View.StackLayout(
                    horizontalOptions = LayoutOptions.StartAndExpand,
                    verticalOptions = LayoutOptions.Start,
                    children = [
                        View.Label(
                            text = course.Title,
                            maxLines = 1,
                            lineBreakMode = LineBreakMode.TailTruncation,
                            fontSize = Constants.UI.FontSize.Small,
                            fontAttributes = FontAttributes.Bold
                        )
                        View.Label(
                            text = sprintf "Exam date: %s" course.DateOfExaminationFormatted,
                            fontSize = Constants.UI.FontSize.Small,
                            textColor = Color.SlateGray)
                        View.Label(
                            text = course.StatusFormulatedForList,
                            fontSize = Constants.UI.FontSize.Small,
                            textColor = Color.SlateGray)
                    ],
                    // Workaround for https://github.com/xamarin/Xamarin.Forms/issues/5920
                    gestureRecognizers = [
                        View.TapGestureRecognizer(
                            command = (fun () -> dispatch (CourseSelected (course, courseUpdateDate))),
                            numberOfTapsRequired = 1)
                    ]
                )
                View.BoxView(
                    widthRequest = 16.0,
                    heightRequest = 16.0,
                    cornerRadius = CornerRadius(8.0),
                    horizontalOptions = LayoutOptions.Center,
                    verticalOptions = LayoutOptions.Center,
                    backgroundColor = Color.FromHex(course.Color))
            ]
        )
    )
    
let private listHeader (model: Model) =
    let headerText =
        match model.UpdatedAt with
        | Some date -> sprintf "◷ Last check: %s" (date.ToLocalTime().ToString("g"))
        | None -> "◷ Last check: Never"
    
    View.StackLayout(
        orientation = StackOrientation.Horizontal,
        children = [
            View.Label(
                text = headerText,
                fontSize = Constants.UI.FontSize.Micro,
                textColor = Color.SlateGray,
                heightRequest = 30.0,
                horizontalOptions = LayoutOptions.CenterAndExpand,
                verticalOptions = LayoutOptions.Center,
                horizontalTextAlignment = TextAlignment.Center,
                verticalTextAlignment = TextAlignment.End)        
        ]
    )

let private renderList (model: Model) dispatch =
    let rowHeight =
        match Device.RuntimePlatform with
        | Device.Android -> 130
        | _ -> 120
    
    View.ListView(
        model.Courses |> List.map(fun course -> courseCell course dispatch),
        itemTapped = (fun index ->
            let course = model.Courses |> List.tryItem index
            if course.IsSome then dispatch (CourseSelected course.Value)),
        isRefreshing = (model.State = Refreshing),
        refreshCommand = (fun _ -> dispatch StartRefreshingCourses),
        separatorVisibility = SeparatorVisibility.None,
        backgroundColor = Color.Transparent,
        isPullToRefreshEnabled = true,
        selectionMode = ListViewSelectionMode.None,
        header = (listHeader model).Create(),
        rowHeight = rowHeight
    )

let private renderAlternateState (model: Model) dispatch =
    View.Frame(
        verticalOptions = LayoutOptions.CenterAndExpand,
        hasShadow = false,
        backgroundColor = Color.Transparent,
        margin = 50.0,
        content = View.StackLayout(children = [
            match model.State with
            | Loading ->
                yield View.ActivityIndicator(
                    isRunning = true,
                    heightRequest = 30.0,
                    color = Constants.UI.Color.ActivityIndicator,
                    verticalOptions = LayoutOptions.CenterAndExpand,
                    horizontalOptions = LayoutOptions.CenterAndExpand)
            | Empty ->
                yield View.Image(
                    source = "empty.png",
                    horizontalOptions = LayoutOptions.Fill,
                    heightRequest = 40.0)
            | LoadingError _ ->
                yield View.Image(
                    source = "error.png",
                    horizontalOptions = LayoutOptions.Fill,
                    heightRequest = 40.0)
            | Ready -> raise (NotSupportedException())
            | Refreshing -> raise (NotSupportedException())
                
            let labelText =
                match model.State with
                | Loading -> "Please wait while your transcript of records is being fetched from CARE. This may take a moment..."
                | Empty -> "You have not passed or been enrolled to any exams right now. Good luck for your first exam!"
                | LoadingError errorMessage -> errorMessage
                | Ready -> raise (NotSupportedException())
                | Refreshing -> raise (NotSupportedException())
                
            yield View.Label(
                text = labelText,
                fontSize = Constants.UI.FontSize.Small,
                textColor = Color.Gray,
                margin = Thickness(0.0, 10.0, 0.0, 0.0),
                horizontalTextAlignment = TextAlignment.Center)
                    
            if model.State <> Loading then
                yield View.Button(
                    text = "↻ Refresh",
                    backgroundColor = Color.Transparent,
                    textColor = Color.SlateGray,
                    margin = Thickness(0.0, 20.0, 0.0, 0.0),
                    command = (fun _ -> dispatch StartLoadingCourses))
        ])
    )

let view (model: Model) dispatch = [
    yield View.ContentPage(
        classId = "CourseListPage",
        title = "IUBH TOR",
        backgroundColor = Constants.UI.Color.PageBackground,
        content = View.StackLayout(children = [
            match model.State with
            | Ready | Refreshing -> yield renderList model dispatch
            | _ -> yield renderAlternateState model dispatch   
        ])
    ).ToolbarItems([
        if not (model.State = Loading || model.State = Refreshing) then
            yield View.ToolbarItem(
                text = "Sign out",
                command = (fun () -> dispatch StartLogout),
                priority = 0,
                order = ToolbarItemOrder.Primary
            )
    ])
    
    // When we have a course selected, we show its detail page
    if model.SelectedCourse.IsSome then
        yield CourseDetailPage.view model.SelectedCourse.Value  
    ]
