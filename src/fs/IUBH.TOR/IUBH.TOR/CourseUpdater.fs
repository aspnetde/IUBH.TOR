module IUBH.TOR.CourseUpdater

/// Compares two lists of courses: Existing ones (A) and current
/// ones (B), that have probably just been downloaded from CARE.
/// All new courses are added, courses that have changed are
/// being updated, and courses that don't exist anymore are being
/// removed.
let update existingCourses currentCourses now =
    /// Get all courses that have been added
    let newCourses =
        currentCourses
        |> List.filter (fun c1 ->
            not (existingCourses |> List.exists (fun (c2, _) -> c2.Id = c1.Id)))
        |> List.map (fun c -> c, CourseUpdateDate(now))
        
    /// Get all courses that have updated
    let updatedCourses =
        currentCourses
        |> List.filter (fun c1 ->
            existingCourses
            |> List.exists (fun (c2, _) -> c2.Id = c1.Id && c2 <> c1))
        |> List.map (fun c -> c, CourseUpdateDate(now))
    
    /// Remove all those courses that have changed or
    /// that simply do not exist anymore.
    let existingCourses =
        existingCourses
        |> List.filter (fun (c1, _) ->
            currentCourses |> List.exists (fun c2 -> c2.Id = c1.Id && c2 = c1))
    
    // Put all the existing, updated, and new courses together 
    existingCourses @ updatedCourses @ newCourses

    // Sort by date of examination descending
    |> List.sortByDescending (fun (c, _) -> c.DateOfExaminationParsed)
