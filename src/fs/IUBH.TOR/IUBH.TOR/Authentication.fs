module IUBH.TOR.Authentication

open FSharp.Data
open System
open System.Threading.Tasks
open Xamarin.Essentials

[<Literal>]
let UserNameKey = "CARE.UserName"
[<Literal>]
let PasswordKey = "CARE.Password"

module ErrorMessages =
    [<Literal>]
    let UserNameMissingInSecureStorage = "Can't find the user name in the Secure Storage."
    [<Literal>]
    let PasswordMissingInSecureStorage = "Can't find the password in the Secure Storage."

/// Tries to validate the provided credentials against
/// the actual CARE backend. So a HTTP request is being
/// made and the response is being evaluated.
let tryValidateCredentials credentials =
    async {
        let body = FormValues [
            "login-form", "login-form";
            "user", credentials.UserName;
            "password", credentials.Password]
        
        let! result =
            Http.AsyncRequestString(
                Constants.CARE.LoginUrl,
                body = body,
                silentHttpErrors = true)
        
        /// The CARE system doesn't respond with proper HTTP status codes,
        /// but always with 200. To distinguish successful from failed login
        /// attempts we need to look for those strings in the response. The
        /// exact response depends on the language that is being configured
        /// for the session.
        let error = result.Contains "Login credentials incorrect!" ||
                    result.Contains "Anmeldedaten nicht korrekt."
                    
        if not error then
            return Ok credentials
        else
            return Error "Wrong user name or password. Please try again."
    }
    
/// Tries to save the credentials utilizing the
/// save function provided. 
let trySaveCredentials credentials (save:string * string -> Task) =
    async {
        try
            do! save(UserNameKey, credentials.UserName) |> Async.AwaitTask
            do! save(PasswordKey, credentials.Password) |> Async.AwaitTask
            
            return Ok()
        with e -> return Error e.Message
    }
  
/// Tries to save the credentials to the
/// OS' Secure Storage implementation.
let trySaveCredentialsToSecureStorage credentials =
    trySaveCredentials credentials SecureStorage.SetAsync
    
/// Tries to read the user name and password
/// utilizing the get function provided.
let tryGetCredentials (get:string -> Task<string>) =
    async {
        try
            let! userName = get(UserNameKey) |> Async.AwaitTask
            let! password = get(PasswordKey) |> Async.AwaitTask
            
            if String.IsNullOrWhiteSpace userName then
                return Error ErrorMessages.UserNameMissingInSecureStorage
            else if String.IsNullOrWhiteSpace password then
                return Error ErrorMessages.PasswordMissingInSecureStorage
            else
                return Ok { UserName = userName; Password = password }
        with e -> return Error e.Message
    }
    
/// Tries to read the user name and password
/// from the OS' Secure Storage implementation.
let tryGetCredentialsFromSecureStorage() =
    tryGetCredentials SecureStorage.GetAsync
    
/// Tries to remove the user name and password
/// utilizing the remove function provided.
let tryRemoveCredentials (remove:string -> bool) =
    try
        remove(UserNameKey) |> ignore
        remove(PasswordKey) |> ignore
        Ok()
    with e -> Error e.Message

/// Tries to remove the user name and password
/// from the OS' Secure Storage implementation.
let tryRemoveCredentialsFromSecureStorage() =
    tryRemoveCredentials SecureStorage.Remove
