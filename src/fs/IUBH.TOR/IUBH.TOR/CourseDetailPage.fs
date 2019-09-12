module IUBH.TOR.CourseDetailPage

open Fabulous.XamarinForms
open System
open Xamarin.Forms

let private row title value margin =
    View.StackLayout(
        orientation = StackOrientation.Vertical,
        isVisible = not (String.IsNullOrWhiteSpace value),
        children = [
            View.Label(
                text = title,
                margin = margin,
                fontSize = Constants.UI.FontSize.Small,
                textColor = Color.SlateGray)
            View.Label(text = value)
        ]
    )

let private defaultRow title value =
    row title value (Thickness(0.0, 20.0, 0.0, 0.0))
    
let private firstRow title value =
    row title value (Thickness(0.0))

let view (course, courseUpdateDate) =
    View.ContentPage(
        classId = "CourseDetailPage",
        title = course.Title,
        backgroundColor = Constants.UI.Color.PageBackground,
        content = View.ScrollView(
            content = View.Frame(
                margin = 15.0,
                verticalOptions = LayoutOptions.Start,
                content = View.StackLayout(
                    children = [
                        course.Module |> firstRow "Module"
                        course.StatusFormulated |> defaultRow "Status"
                        course.Grade |> defaultRow "Grade"
                        course.Rating |> defaultRow "Rating"
                        course.Credits |> defaultRow "Credits"
                        course.Attempts |> defaultRow "Attempts"
                        course.DateOfExaminationFormatted |> defaultRow "Exam Date"
                        courseUpdateDate.ToString() |> defaultRow "Last Update"
                    ]
                )
            )
        )
    )
