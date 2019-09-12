namespace IUBH.TOR.Tests.DownloaderSpecs

open FsUnit
open IUBH.TOR
open System
open Xunit

module ``Course Page HTML Downloader Specification`` =
    type ``When the HTML Page is being tried to downloaded given valid Credentials``() =
        [<Fact>]
        let ``Then the result is Ok and contains an HTML document``() =
            let userName = "test.user"
            let password = "test123"
            let credentials = { UserName = userName; Password = password } 
            
            let result = CoursePageHtmlDownloader.tryDownloadCoursePageHtml credentials
                         |> Async.RunSynchronously
            
            match result with
            | Error e -> failwith e
            | Ok html -> html.Contains "<html" |> should be True

    type ``When the HTML Page is being tried to download given invalid Credentials``() =
        [<Fact>]
        let ``Then the result is an Error``() =
            let credentials = { UserName = "invalid"; Password = "invalid" }
            let result = CoursePageHtmlDownloader.tryDownloadCoursePageHtml credentials
                         |> Async.RunSynchronously
            
            match result with
            | Error _ -> ()
            | Ok _ -> failwith "Login should not be successful!"
