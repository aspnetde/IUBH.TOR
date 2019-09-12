using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Domain;

namespace IUBH.TOR.Modules.Courses.Services
{
    internal class CoursePageHtmlParser : ICoursePageHtmlParser
    {
        /// <summary>
        /// Converts an HTML page to a set of (raw) course information.
        /// This is tailored towards the current structure of the HTML
        /// document. So it is likely to break as soon as that structure
        /// changes. Use the Unit Tests provided to adjust the mechanics
        /// of this method in that case.
        /// </summary>
        public Result<RawCourse[]> TryParseCoursePage(string coursePageHtml)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(coursePageHtml);

                // Collects all tables on the page.
                var tables = doc.DocumentNode
                    .SelectNodes("//tbody").Reverse()
                    .Skip(1); // The last one isn't a semester table

                var tableRows = tables.SelectMany(t => t.ChildNodes);

                var courses = new List<RawCourse>();

                string currentModuleName = null;

                foreach (HtmlNode tableRow in tableRows)
                {
                    var columns = tableRow.ChildNodes;

                    if (columns.Count == 0)
                    {
                        continue;
                    }

                    // Skipping modules – those modules don't contain links
                    bool isModule = !tableRow.Descendants("a").Any();

                    // But if we have a module here, we remember its name so
                    // we can pass it along all subsequent courses until the
                    // next module appears
                    if (isModule)
                    {
                        currentModuleName = cleanString(columns[1].InnerText);
                        continue;
                    }

                    string dateOfExamination = columns[6].InnerText;

                    // If there's no date of examination this course information
                    // is not of interest to us. So we skip it.
                    if (string.IsNullOrWhiteSpace(dateOfExamination))
                    {
                        continue;
                    }

                    // To avoid side effects in the representation later on it's
                    // important to strip all characters we don't need from the texts.
                    string cleanString(string input)
                        => Regex.Replace(input ?? string.Empty, @"(\n|\r|\t|\s)+", " ").Trim();

                    string title = cleanString(columns[1].InnerText);
                    string id = title.GetHashCode().ToString();
                    
                    courses.Add(
                        new RawCourse
                        {
                            Id = id,
                            Title = title,
                            Module = currentModuleName,
                            Status = cleanString(columns[2].InnerText),
                            Grade = cleanString(columns[3].InnerText),
                            Rating = cleanString(columns[4].InnerText),
                            Credits = cleanString(columns[5].InnerText),
                            DateOfExamination = cleanString(dateOfExamination),
                            Attempts = cleanString(columns[8].InnerText)
                        }
                    );
                }

                return Result.WithSuccess(courses.ToArray());
            }
            catch (Exception e)
            {
                return Result.WithException<RawCourse[]>(e);
            }
        }
    }
}
