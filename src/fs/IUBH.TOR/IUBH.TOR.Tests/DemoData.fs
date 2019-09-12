namespace IUBH.TOR.Tests

open IUBH.TOR

[<AutoOpen>]
module DemoData =
    let course () =
        { Id = 69
          Title = "Math for Developers"
          Module = "Math"
          Status = ""
          Grade = ""
          Rating = ""
          Credits = ""
          DateOfExamination = ""
          Attempts = "" }
