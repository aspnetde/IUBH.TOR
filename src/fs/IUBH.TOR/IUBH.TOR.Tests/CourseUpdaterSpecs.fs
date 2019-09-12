namespace IUBH.TOR.Tests

open FsUnit
open IUBH.TOR
open System
open Xunit

module ``Course Updater Specification``=
    let createCourse id =
        { DemoData.course() with Id = id }
    
    type ``When no course changed``() =
        [<Fact>]
        let ``The result equals the list of existing courses``() =
            let course1 = { DemoData.course() with Id = 1}
            let course2 = { DemoData.course() with Id = 2 }
            let course3 = { DemoData.course() with Id = 3 }
            
            let existingCourses = [
                course1, CourseUpdateDate(DateTime.UtcNow)
                course2, CourseUpdateDate(DateTime.UtcNow)
                course3, CourseUpdateDate(DateTime.UtcNow)
            ]
            
            let currentCourses = [ course1; course2; course3 ]
            
            let now = DateTime.UtcNow
            let result = CourseUpdater.update existingCourses currentCourses now
            
            result |> should equal existingCourses
            
    type ``When a new course has been added``() =
        [<Fact>]
        let ``It will be returned in the list using the update date provided``() =
            let existingCourses = [ DemoData.course(), CourseUpdateDate(DateTime.UtcNow) ]
            let newCourse = createCourse 42
            let currentCourses = [ DemoData.course(); newCourse ]
            let now = DateTime.UtcNow
            
            let result = CourseUpdater.update existingCourses currentCourses now
            
            let expectedNewCourse = newCourse, CourseUpdateDate(now)
            
            result |> Seq.contains expectedNewCourse |> should be True
            
    type ``When a course has been updated``() =
        [<Fact>]
        let ``It will be updated in the list using the update date provided``() =
            let existingCourse = DemoData.course()
            let existingCourses = [ existingCourse, CourseUpdateDate(DateTime.UtcNow) ]
            
            let updatedCourse = { existingCourse with Title = randomString() }
            let currentCourses = [ updatedCourse ]
            let now = DateTime.UtcNow
            
            let result = CourseUpdater.update existingCourses currentCourses now
            
            result.Length |> should equal 1
            result.Head |> should equal (updatedCourse, CourseUpdateDate(now))
            
    type ``When a course has been removed``() =
        [<Fact>]
        let ``It will be removed from the list as well``() =
            let existingCourses = [ DemoData.course(), CourseUpdateDate(DateTime.UtcNow) ]
            let currentCourses = [ ]
            let now = DateTime.UtcNow
            
            let result = CourseUpdater.update existingCourses currentCourses now
            
            result.Length |> should equal 0

    type ``When courses are being returned``() =
        [<Fact>]
        let ``They are always being ordered by examination date descending``() =
            let course1 = { DemoData.course() with Id = 1; DateOfExamination = "10.10.2030" }
            let course2 = { DemoData.course() with Id = 2; DateOfExamination = "30.12.2030" }
            let course3 = { DemoData.course() with Id = 3; DateOfExamination = "23.11.2030" }
            
            let existingCourses = [
                course1, CourseUpdateDate(DateTime.UtcNow)
                course2, CourseUpdateDate(DateTime.UtcNow)
                course3, CourseUpdateDate(DateTime.UtcNow)
            ]
            
            let currentCourses = [ course1; course2; course3 ]
            
            let now = DateTime.UtcNow
            let result = CourseUpdater.update existingCourses currentCourses now
            
            fst result.[0] |> should equal course2
            fst result.[1] |> should equal course3
            fst result.[2] |> should equal course1
