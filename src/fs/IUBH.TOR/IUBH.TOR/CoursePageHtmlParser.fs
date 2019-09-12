module IUBH.TOR.CoursePageHtmlParser

open FSharp.Data
open System

/// Returns the "inner text" of a table cell (<td />).
let private td (index : int) (row : HtmlNode) =
    let node = row.Descendants [ "td" ] |> Seq.item index
    let text = (HtmlNode.innerText node)
    if text = null then text else text.Trim()

/// Creates a Course out of a table row
let private mapRawCourse (moduleName, row) =
    { Id = (row |> td 1).GetHashCode()
      Title = row |> td 1
      Module = moduleName
      Status = row |> td 2
      Grade = row |> td 3
      Rating = row |> td 4
      Credits = row |> td 5
      DateOfExamination = row |> td 6
      Attempts = row |> td 8 }

/// Deterines if a given row is a module row
let private isModuleRow (row : HtmlNode) =
    /// Module rows don't contain links
    row.Descendants [ "a" ] |> Seq.isEmpty
    
/// Determines if a course has an examination date
let private hasDateOfExamination rawCourse =
    not (String.IsNullOrWhiteSpace rawCourse.DateOfExamination)

/// Tries to parse the HTML document, provided as a
/// string argument. If the doc could be parsed, a
/// list of courses is being returned. If not, an
/// error message is being given back instead.
let tryParse htmlDocument =
    try
        /// We are using the F# HTML Type Provider to parse the doc
        let htmlDocument = HtmlDocument.Parse htmlDocument
        
        /// The doc is structured in different tables. We collect
        /// Them to get all the courses and modules in a flat list.
        let rows =
            htmlDocument.Descendants "tbody"
            |> Seq.rev
            |> Seq.skip 1 // The last one isn't a semester table
            |> Seq.collect (fun table -> table.Descendants [ "tr" ])
            |> Seq.toList

        /// The result must contain
        /// - All courses
        /// - But not those without examination date
        /// - The module name of each course
        /// - No modules
        let result =
            rows
            |> Seq.mapi (fun index row ->
                if not (row |> isModuleRow) then
                   let moduleName =
                       rows.[..index]
                       |> Seq.findBack isModuleRow
                       |> td 1
                   Some(moduleName, row)
                else None)
            |> Seq.filter (fun x -> x.IsSome)
            |> Seq.map (fun x -> x.Value |> mapRawCourse)
            |> Seq.filter (hasDateOfExamination)
            |> Seq.toList

        Ok result
    with e -> Error e.Message 
