module IUBH.TOR.CoursePageHtmlDownloader

open IUBH.TOR
open System.Net
open FSharp.Data

let private tryAuthenticate credentials =
    async {
        let body = FormValues [
            "login-form", "login-form";
            "user", credentials.UserName;
            "password", credentials.Password]
        
        /// CARE seems to be built in the good old fashion where
        /// you would store all user information in a server-side
        /// session. So we not only don't get a proper HTTP code
        /// in response to our request, but there is also nothing
        /// we could us to authenticate for subsequent requests,
        /// like an authentication token.
        ///
        /// Instead the system seems to set a flag in the user's
        /// session on the server-side, which is being kept alive
        /// by a session cookie. That cookie is being stored in
        /// the cookie container. And that's the key: We return
        /// that container from this function, so we can pass it
        /// along with subsequent requests, e.g. in tryDownload.
        let cookieContainer = CookieContainer()
        
        let! result =
            Http.AsyncRequestString(
                Constants.CARE.LoginUrl,
                body = body,
                cookieContainer = cookieContainer,
                silentHttpErrors = true)
        
        let error = result.Contains "Login credentials incorrect!" ||
                    result.Contains "Anmeldedaten nicht korrekt."
                    
        if not error then
            return Ok cookieContainer
        else
            return Error "Wrong user name or password. Can't authenticate."
    }
    
let private tryDownload cookieContainer =
    async {
        try
            let! html = 
                Http.AsyncRequestString(
                    Constants.CARE.TranscriptOfRecordsUrl,
                    cookieContainer = cookieContainer)

            return Ok html
        with
            | ex -> return Error ex.Message
    }

/// Tries to download the course HTML page from the
/// CARE system given the user's credentials provided.
let tryDownloadCoursePageHtml credentials =
    tryAuthenticate credentials |> AsyncResult.bind tryDownload
