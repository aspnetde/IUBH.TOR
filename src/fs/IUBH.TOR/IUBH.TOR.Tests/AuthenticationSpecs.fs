namespace IUBH.TOR.Tests.AuthenticationSpecs

open System
open System.Threading.Tasks
open FsUnit
open IUBH.TOR
open IUBH.TOR.Tests
open Xunit

module ``Authentication Specification`` =
    type ``When Credentials are being validated against the CARE backend``() =
        [<Fact>]
        let ``An successful result is being returned when user name and password are correct``() =
            let userName = "test.user"
            let password = "test123"
            let credentials = { UserName = userName; Password = password } 
            
            let result = Authentication.tryValidateCredentials credentials
                         |> Async.RunSynchronously
            
            match result with
            | Error e -> failwith e
            | Ok _ -> ()
            
        [<Fact>]
        let ``The result is not successful and provides an error message when user name or password are invalid``() =
            let credentials = { UserName = "invalid"; Password = "invalid" }
            let result = Authentication.tryValidateCredentials credentials
                         |> Async.RunSynchronously
            
            match result with
            | Error _ -> ()
            | Ok _ -> failwith "Login should not be successful!"
            
    type ``When credentials are tried to being saved``() =
        [<Fact>]
        let ``Then the user name is being saved``() =
            let savedKeys = ResizeArray<string>()
            let savedValues = ResizeArray<string>()
            
            let storageMock (key, value) =
                savedKeys.Add key
                savedValues.Add value
                
                Task.CompletedTask
                
            let credentials = { UserName = "foo"; Password = "bar" }
                
            Authentication.trySaveCredentials credentials storageMock
            |> Async.RunSynchronously
            |> ignore
            
            savedKeys.Contains Authentication.UserNameKey |> should be True
            savedValues.Contains credentials.UserName |> should be True
            
        [<Fact>]
        let ``Then the password is being saved``() =
            let savedKeys = ResizeArray<string>()
            let savedValues = ResizeArray<string>()
            
            let storageMock (key, value) =
                savedKeys.Add key
                savedValues.Add value
                
                Task.CompletedTask
                
            let credentials = { UserName = "foo"; Password = "bar" }
                
            Authentication.trySaveCredentials credentials storageMock
            |> Async.RunSynchronously
            |> ignore
            
            savedKeys.Contains Authentication.PasswordKey |> should be True
            savedValues.Contains credentials.Password |> should be True

        [<Fact>]
        let ``Then the Result is okay if everything works``() =
            let storageMock (key, value) = Task.CompletedTask
            let credentials = { UserName = "foo"; Password = "bar" }
                
            let result =
                Authentication.trySaveCredentials credentials storageMock
                |> Async.RunSynchronously
                
            match result with
            | Error e -> failwith e
            | _ -> ()
            
        [<Fact>]
        let ``Then the Result is erroneous if something goes wrong by accessing the Secure Storage``() =
            let expectedErrorMessage = "Access not allowed"
            
            let storageMock (key, value) =
                raise (InvalidOperationException(expectedErrorMessage))
                Task.CompletedTask
                
            let credentials = { UserName = "foo"; Password = "bar" }
                
            let result =
                Authentication.trySaveCredentials credentials storageMock
                |> Async.RunSynchronously
                
            match result with
            | Error e -> e |> should equal expectedErrorMessage
            | _ -> ()
            
    type ``When the credentials are tried to being received``() =
        [<Fact>]
        let ``The result contains the Credentials if both Password and User name are found``() =
            let expectedUsername = randomString()
            let expectedPassword = randomString()
            
            let get key =
                match key with
                | Authentication.UserNameKey -> Task.FromResult(expectedUsername)
                | Authentication.PasswordKey -> Task.FromResult(expectedPassword)
                | _ -> Task.FromResult<string>(null)
                
            let result = Authentication.tryGetCredentials get |> Async.RunSynchronously
            
            match result with
            | Ok credentials ->
                credentials.UserName |> should equal expectedUsername
                credentials.Password |> should equal expectedPassword
            | Error error -> failwith error
        
        [<Fact>]
        let ``The result is erroneous when the user name is missing``() =
            let expectedPassword = randomString()
            
            let get key =
                match key with
                | Authentication.PasswordKey -> Task.FromResult(expectedPassword)
                | _ -> Task.FromResult<string>(null)
                
            let result = Authentication.tryGetCredentials get |> Async.RunSynchronously
            
            match result with
            | Ok _ -> failwith "Should not be Ok"
            | Error error ->
                error |> should equal Authentication.ErrorMessages.UserNameMissingInSecureStorage
            
        [<Fact>]
        let ``The result is erroneous when the password is missing``() =
            let expectedUserName = randomString()
            
            let get key =
                match key with
                | Authentication.UserNameKey -> Task.FromResult(expectedUserName)
                | _ -> Task.FromResult<string>(null)
                
            let result = Authentication.tryGetCredentials get |> Async.RunSynchronously
            
            match result with
            | Ok _ -> failwith "Should not be Ok"
            | Error error ->
                error |> should equal Authentication.ErrorMessages.PasswordMissingInSecureStorage
            
        [<Fact>]
        let ``The result is erroneous when something goes wrong by accessing the Secure Storage``() =
            let expectedErrorMessage = randomString()
            
            let get key =
                failwith expectedErrorMessage
                Task.FromResult<string>(null)
                
            let result = Authentication.tryGetCredentials get |> Async.RunSynchronously
            
            match result with
            | Ok _ -> failwith "Should not be Ok"
            | Error error ->
                error |> should equal expectedErrorMessage

    type ``When the Credentials are tried to being removed``() =
        [<Fact>]
        let ``The User name is being removed``() =
            let removedKeys = ResizeArray<string>()
            let remove (key:string) =
                removedKeys.Add key
                true
                
            Authentication.tryRemoveCredentials remove |> ignore
            removedKeys.Contains Authentication.UserNameKey |> should be True
            
        [<Fact>]
        let ``The Password is being removed``() =
            let removedKeys = ResizeArray<string>()
            let remove (key:string) =
                removedKeys.Add key
                true
                
            Authentication.tryRemoveCredentials remove |> ignore
            removedKeys.Contains Authentication.PasswordKey |> should be True
            
        [<Fact>]
        let ``The result is Ok when the removal succeeded``() =
            let remove (key:string) = true
            
            let result = Authentication.tryRemoveCredentials remove
            
            match result with
            | Error error -> failwith error
            | Ok _ -> ()
            
        [<Fact>]
        let ``The result is erroneous when an exception is being thrown whiel accessing the Secure Storage``() =
            let errorMessage = randomString()
            let remove (key:string) =
                failwith errorMessage
                true
            
            let result = Authentication.tryRemoveCredentials remove
            
            match result with
            | Error error -> error |> should equal errorMessage
            | Ok _ -> failwith "Should not be Ok"
