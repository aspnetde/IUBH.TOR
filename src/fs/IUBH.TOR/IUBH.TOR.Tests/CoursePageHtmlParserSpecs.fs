namespace IUBH.TOR.Tests.ParserSpecs

open FSharp.Data
open FsUnit
open IUBH.TOR
open Xunit

module ``Course Page HTML Parser Specification`` =
    type TOR = HtmlProvider<"../../../../testdata/demo-tor.html">

    let htmlDocument = TOR().Html.ToString()

    type ``When a valid HTML Document is being provided``() =

        [<Theory>]
        [<InlineData("Mathematics I")>]
        [<InlineData("Introduction to Scientific Work")>]
        [<InlineData("Self and Time Management")>]
        [<InlineData("Business Administration I")>]
        [<InlineData("Business Administration II")>]
        [<InlineData("Software Engineering Principles")>]
        [<InlineData("Object-oriented Programming with Java")>]
        [<InlineData("Data Structures and Java Class Library")>]
        [<InlineData("Mathematics II")>]
        [<InlineData("Web Application Development")>]
        [<InlineData("Programming Information Systems with Java EE")>]
        [<InlineData("Requirements Engineering")>]
        [<InlineData("Management Accounting I (Introduction)")>]
        [<InlineData("Management Accounting II (Advanced)")>]
        let ``Then all of the Courses are being returned`` courseTitle =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses -> courses |> Seq.tryFind (fun x -> x.Title = courseTitle) |> Option.isSome |> should be True
            | Error error -> failwith error

        [<Theory>]
        [<InlineData("Mathematics I (ME)")>]
        [<InlineData("Scientific Work (ME)")>]
        [<InlineData("Business Administration (ME)")>]
        [<InlineData("Software Engineering Principles (ME)")>]
        [<InlineData("Object-oriented programing (ME)")>]
        [<InlineData("Mathematics for Business Engineers (ME)")>]
        [<InlineData("Web Application Development (ME)")>]
        [<InlineData("Requirements Engineering (ME)")>]
        [<InlineData("Management Accounting (ME)")>]
        let ``Then none of the Modules are being mistaken as Courses`` moduleTitle =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses -> courses |> Seq.tryFind (fun x -> x.Title = moduleTitle) |> Option.isSome |> should be False
            | Error error -> failwith error

        [<Theory>]
        [<InlineData("BWL I (Einführung und Grundlagen)")>]
        [<InlineData("BWL II (Vertiefung)")>]
        [<InlineData("Mathematics for Business Engineers")>]
        let ``Then Courses without Examination date are being skipped`` courseTitle =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses -> courses |> Seq.tryFind (fun x -> x.Title = courseTitle) |> Option.isSome |> should be False
            | Error error -> failwith error

    type ``When an invalid HTML Document is being provided``() =
        [<Fact>]
        let ``An Error Result is being returned``() =
            let result = CoursePageHtmlParser.tryParse "<html></html>"
            match result with
            | Ok _ -> failwith "Should not be OK!"
            | Error error -> ()

    type ``When a Raw Course is being returned``() =
        [<Fact>]
        let ``Then its Id is set as the Hashcode of the Title``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                let expectedId = "Mathematics I".GetHashCode()
                result.Value.Id |> should equal expectedId
            | Error error -> failwith error

        [<Fact>]
        let ``Then its Title is set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                result.Value.Title |> should equal "Mathematics I"
            | Error error -> failwith error
            
        [<Fact>]
        let ``Then its Module is set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                result.Value.Module |> should equal "Mathematics I (ME)"
            | Error error -> failwith error
            
        [<Fact>]
        let ``Then its Status is set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                result.Value.Status |> should equal "P"
            | Error error -> failwith error

        [<Fact>]
        let ``Then its Grade is set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                result.Value.Grade |> should equal "1,7"
            | Error error -> failwith error

        [<Fact>]
        let ``Then its Rating is set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                result.Value.Rating |> should equal "87.7 / 100"
            | Error error -> failwith error

        [<Fact>]
        let ``Then its Credits are set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                result.Value.Credits |> should equal "5 / 5"
            | Error error -> failwith error
            
        [<Fact>]
        let ``Then its Date of Examination is set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics I")
                result.Value.DateOfExamination |> should equal "08.07.2017"
            | Error error -> failwith error

        [<Fact>]
        let ``Then the number of Attempts is set``() =
            let result = CoursePageHtmlParser.tryParse htmlDocument
            match result with
            | Ok courses ->
                let result = courses |> Seq.tryFind (fun c -> c.Title = "Mathematics II")
                result.Value.Attempts |> should equal "4"
            | Error error -> failwith error
