namespace IUBH.TOR

open System
open System.Globalization

type Credentials =
    { UserName: string
      Password: string }

type Course =
    { Id: int
      Title: string
      Module: string
      Status: string
      Grade: string
      Rating: string
      Credits: string
      DateOfExamination: string
      Attempts: string }
    
    member this.DateOfExaminationParsed =
        let couldParse, parsedDate =
            DateTime.TryParse(this.DateOfExamination, CultureInfo("de-DE"), DateTimeStyles.None)
            
        match couldParse with
        | true -> parsedDate
        | false -> DateTime.MinValue
    
    member this.DateOfExaminationFormatted =
        this.DateOfExaminationParsed.ToString("d", CultureInfo("en-US"))
    
    /// Returns the color als Hex Code for the
    /// current Status of this Course.
    member this.Color =
        match this.Status with
        | "P" -> "#4DDB4D"
        | "P, T" -> "#4DDB4D"
        | "F" -> "#FF4D4D"
        | "EE" -> "#8080B1"
        | "E" -> "#EDC66B"
        | "M" -> "#CACAC8"
        | _ -> "#FFFFFF"

    /// Returns the formulated Status, e.g. P -> Passed
    member this.StatusFormulated =
        match this.Status with
        | "P" -> "Passed"
        | "P, T" -> "Transferred"
        | "F" -> "Failed"
        | "EE" -> "Enrolled for exam"
        | "E" -> "Enrolled for course"
        | "CE" -> "Combination exam"
        | "M" -> "Missing result"
        | "ME" -> "Module examination"
        | _ -> "Unknown"
    
    /// Returns the formulated Status with more details
    /// that are useful for the list representation, e.g.
    /// Passed: 1,0 (100 / 100)
    member this.StatusFormulatedForList =
        match this.Status, this.Grade with
        | "P", grade when not (String.IsNullOrWhiteSpace grade) && not (grade = "Passed") ->
            sprintf "Passed: %s (%s)" grade this.Rating
        | "P, T", grade when not (String.IsNullOrWhiteSpace grade) && not (grade = "Passed") ->
            sprintf "Transferred: %s (%s)" grade this.Rating
        | _, _ -> this.StatusFormulated

type CourseUpdateDate =
    | CourseUpdateDate of DateTime
    override this.ToString() =
        match this with
        | CourseUpdateDate (value) ->
            value.ToLocalTime().ToString("g", CultureInfo("en-US"))
