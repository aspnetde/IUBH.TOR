module IUBH.TOR.CourseLoader

open System

/// Tries to load courses. If successful, courses will be
/// a) downloaded
/// b) parsed
/// c) updated against the existing courses
/// d) returned
let tryLoadCourses
    tryGetCredentials
    tryDownloadCourseListHtmlDocument
    tryParseCourseListHtmlDocument
    updateCourses
    existingCourses
    now =
    async {
        let! result = tryGetCredentials() |> AsyncResult.bind tryDownloadCourseListHtmlDocument
        let result = result |> Result.bind tryParseCourseListHtmlDocument 
            
        match result with
        | Ok courses ->
            let courses = updateCourses existingCourses courses now
            return Ok (courses, now)
        | Error errorMessage ->
            return Error errorMessage
    }

/// Tries to load courses directly from the CARE system.
/// If successful, courses will be
/// a) downloaded
/// b) parsed
/// c) updated against the existing courses
/// d) returned
let tryLoadCoursesFromCARE existingCourses =
    tryLoadCourses
        Authentication.tryGetCredentialsFromSecureStorage
        CoursePageHtmlDownloader.tryDownloadCoursePageHtml
        CoursePageHtmlParser.tryParse
        CourseUpdater.update
        existingCourses
        DateTime.UtcNow
