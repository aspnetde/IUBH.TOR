namespace IUBH.TOR.Tests

open System
open FsUnit
open IUBH.TOR
open IUBH.TOR.Tests
open Xunit

module ``Course Loader Specification`` =
    let mockCourseUpdate
        (existingCourses: (Course * CourseUpdateDate) list)
        (currentCourses: Course List)
        (now: DateTime) =
            currentCourses |> List.map(fun c -> c, CourseUpdateDate(now))
        
    let mockExistingCourses: (Course * CourseUpdateDate) list =
        []

    let mockNow =
        DateTime.UtcNow

    type ``When the Course Loading Command is being executed``() =
        [<Fact>]
        let ``The Course Updater is being called in case of a success``() =
            let course = DemoData.course()
            let courseUpdateDate = CourseUpdateDate(DateTime.UtcNow)
            
            let tryGetCredentials() =
                async { return Ok { UserName = "Leon"; Password = "Password" } }
            let tryDownloadCourseListHtmlDocument credentials =
                async { return Ok "HTML" }
            let tryParseCourseListHtmlDocument html =
                Ok [ course ]
                
            let mutable courseUpdateCalled = false 
                
            let mockCourseUpdate
                (existingCourses: (Course * CourseUpdateDate) list)
                (currentCourses: Course List)
                (now: DateTime) =
                    courseUpdateCalled <- true
                    currentCourses |> List.map(fun c -> c, CourseUpdateDate(now))

            CourseLoader.tryLoadCourses
                tryGetCredentials
                tryDownloadCourseListHtmlDocument
                tryParseCourseListHtmlDocument
                mockCourseUpdate
                mockExistingCourses
                mockNow
                |> Async.RunSynchronously
                |> ignore
                    
            courseUpdateCalled |> should be True
        
        [<Fact>]
        let ``The list of courses and an update Date is being returned in case of a success``() =
            let course = DemoData.course()
            
            let tryGetCredentials() =
                async { return Ok { UserName = "Leon"; Password = "Password" } }
            let tryDownloadCourseListHtmlDocument credentials =
                async { return Ok "HTML" }
            let tryParseCourseListHtmlDocument html =
                Ok [ course ]
                
            let result =
                CourseLoader.tryLoadCourses
                    tryGetCredentials
                    tryDownloadCourseListHtmlDocument
                    tryParseCourseListHtmlDocument
                    mockCourseUpdate
                    mockExistingCourses
                    mockNow
                    |> Async.RunSynchronously
                    
            match result with
            | Ok (courses, updateDate) ->
                (fst courses.Head).Id |> should equal 69
                updateDate |> should equal mockNow
            | Error error -> failwith error
            
        [<Fact>]
        let ``An error is being returned when the credentials could not be accessed``() =
            let expectedErrorMessage = randomString()
            
            let tryGetCredentials() =
                async { return Error expectedErrorMessage }
            let tryDownloadCourseListHtmlDocument credentials =
                async { return Ok "HTML" }
            let tryParseCourseListHtmlDocument html =
                let courses: Course list = []
                Ok courses
                
            let result =
                CourseLoader.tryLoadCourses
                    tryGetCredentials
                    tryDownloadCourseListHtmlDocument
                    tryParseCourseListHtmlDocument
                    mockCourseUpdate
                    mockExistingCourses
                    mockNow
                    |> Async.RunSynchronously
                    
            match result with
            | Error errorMessage ->
                errorMessage |> should equal expectedErrorMessage
            | _ -> raise (InvalidOperationException())
                    
        [<Fact>]
        let ``An error is being returned when the Html document could not be downloaded``() =
            let expectedErrorMessage = randomString()
            
            let tryGetCredentials() =
                async { return Ok { UserName = "Leon"; Password = "Password" } }
            let tryDownloadCourseListHtmlDocument credentials =
                async { return Error expectedErrorMessage }
            let tryParseCourseListHtmlDocument html =
                let courses: Course list = []
                Ok courses
                
            let result =
                CourseLoader.tryLoadCourses
                    tryGetCredentials
                    tryDownloadCourseListHtmlDocument
                    tryParseCourseListHtmlDocument
                    mockCourseUpdate
                    mockExistingCourses
                    mockNow
                    |> Async.RunSynchronously
                    
            match result with
            | Error errorMessage ->
                errorMessage |> should equal expectedErrorMessage
            | _ -> raise (InvalidOperationException())
            
        [<Fact>]
        let ``An error is being returned when the Html document could not be parsed``() =
            let expectedErrorMessage = randomString()
            
            let tryGetCredentials() =
                async { return Ok { UserName = "Leon"; Password = "Password" } }
            let tryDownloadCourseListHtmlDocument credentials =
                async { return Ok "HTML" }
            let tryParseCourseListHtmlDocument html =
                Error expectedErrorMessage
                
            let result =
                CourseLoader.tryLoadCourses
                    tryGetCredentials
                    tryDownloadCourseListHtmlDocument
                    tryParseCourseListHtmlDocument
                    mockCourseUpdate
                    mockExistingCourses
                    mockNow
                    |> Async.RunSynchronously
                    
            match result with
            | Error errorMessage ->
                errorMessage |> should equal expectedErrorMessage
            | _ -> raise (InvalidOperationException())

