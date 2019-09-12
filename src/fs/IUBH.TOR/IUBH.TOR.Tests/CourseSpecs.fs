namespace IUBH.TOR.Tests

open System
open FsUnit
open IUBH.TOR
open Xunit

module ``Course Specification`` =
    [<Fact>]
    let ``Formatted Date of Examination``() =
        let course = DemoData.course()
        let course = { course with DateOfExamination = "16.05.2019" }
        course.DateOfExaminationFormatted |> Xunit.should equal "5/16/19"

    [<Theory>]
    [<InlineData("P", "#4DDB4D")>]
    [<InlineData("P, T", "#4DDB4D")>]
    [<InlineData("F", "#FF4D4D")>]
    [<InlineData("EE", "#8080B1")>]
    [<InlineData("E", "#EDC66B")>]
    [<InlineData("M", "#CACAC8")>]
    [<InlineData("", "#FFFFFF")>]
    [<InlineData(null, "#FFFFFF")>]
    [<InlineData("Unknown", "#FFFFFF")>]
    let ``Color`` status expectedHexCode =
        let course = DemoData.course()
        let course = { course with Status = status }
        course.Color |> should equal expectedHexCode
    
    [<Theory>]
    [<InlineData("P", "Passed")>]
    [<InlineData("P, T", "Transferred")>]
    [<InlineData("F", "Failed")>]
    [<InlineData("EE", "Enrolled for exam")>]
    [<InlineData("E", "Enrolled for course")>]
    [<InlineData("CE", "Combination exam")>]
    [<InlineData("M", "Missing result")>]
    [<InlineData("ME", "Module examination")>]
    [<InlineData("", "Unknown")>]
    [<InlineData("Something we don't know", "Unknown")>]
    [<InlineData(null, "Unknown")>]
    let ``Formatted Status`` status expectedFormattedStatus =
        let course = DemoData.course()
        let course = { course with Status = status; }
        course.StatusFormulated |> should equal expectedFormattedStatus
    
    [<Theory>]
    [<InlineData("P", "2,3", "80 / 100", "Passed: 2,3 (80 / 100)")>]
    [<InlineData("P", "Passed", "", "Passed")>]
    [<InlineData("P", "", "", "Passed")>]
    [<InlineData("P, T", "Passed", "", "Transferred")>]
    [<InlineData("P, T", "", "", "Transferred")>]
    [<InlineData("P, T", "1,0", "99 / 100", "Transferred: 1,0 (99 / 100)")>]
    [<InlineData("F", "", "", "Failed")>]
    [<InlineData("EE", "", "", "Enrolled for exam")>]
    [<InlineData("E", "", "", "Enrolled for course")>]
    [<InlineData("CE", "", "", "Combination exam")>]
    [<InlineData("M", "", "", "Missing result")>]
    [<InlineData("ME", "", "", "Module examination")>]
    [<InlineData("", "", "", "Unknown")>]
    [<InlineData("Something we don't know", "", "", "Unknown")>]
    [<InlineData(null, "", "", "Unknown")>]
    let ``Formatted Status for List`` status grade rating expectedFormattedStatus =
        let course = DemoData.course()
        let course = { course with Status = status; Grade = grade; Rating = rating }
        course.StatusFormulatedForList |> should equal expectedFormattedStatus

    [<Fact>]
    let ``Parsed Examination Date``() =
        let courseWithDate date =
            let course = DemoData.course()
            { course with DateOfExamination = date }
        
        (courseWithDate "").DateOfExaminationParsed |> should equal DateTime.MinValue
        (courseWithDate null).DateOfExaminationParsed |> should equal DateTime.MinValue
        (courseWithDate "INVALID").DateOfExaminationParsed |> should equal DateTime.MinValue
        (courseWithDate "16.05.2019").DateOfExaminationParsed |> should equal (DateTime(2019, 5, 16))
