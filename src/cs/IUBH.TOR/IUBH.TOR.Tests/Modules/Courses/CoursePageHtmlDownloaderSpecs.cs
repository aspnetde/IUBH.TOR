using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Services;
using IUBH.TOR.Modules.Shared.Domain;
using IUBH.TOR.Modules.Shared.Services;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CoursePageHtmlDownloaderSpecs
    {
        [TestFixture]
        public class When_the_Html_Document_is_being_downloaded
        {
            [Test]
            public async Task An_actual_Html_Document_is_being_returned_as_String()
            {
                var credentialStorage = Substitute.For<ICredentialStorage>();

                credentialStorage.GetCredentialsAsync().Returns(
                    Task.FromResult(
                        Result.WithSuccess(
                            new Credentials(
                                CareTestCredentials.UserName,
                                CareTestCredentials.Password
                            )
                        )
                    )
                );

                var sut = new CoursePageHtmlDownloader(credentialStorage);

                var result = await sut
                    .TryDownloadCoursePageHtmlAsync(Constants.CareTranscriptOfRecordsUrl)
                    .ConfigureAwait(false);
                
                result.IsSuccessful.ShouldBeTrue();
                result.Value.Contains("<!DOCTYPE HTML>").ShouldBeTrue();
            }

            [TestFixture]
            public class When_an_Error_occurs_during_the_download
            {
                [Test]
                public async Task A_not_successful_result_is_being_returned()
                {
                    var credentialStorage = Substitute.For<ICredentialStorage>();

                    credentialStorage.GetCredentialsAsync().Returns(
                        Task.FromResult(
                            Result.WithSuccess(
                                new Credentials(
                                    "INVALID_USERNAME",
                                    "INVALID_PASSWORD"
                                )
                            )
                        )
                    );

                    var sut = new CoursePageHtmlDownloader(credentialStorage);

                    var result = await sut
                        .TryDownloadCoursePageHtmlAsync(Constants.CareTranscriptOfRecordsUrl)
                        .ConfigureAwait(false);

                    result.IsSuccessful.ShouldBeFalse();
                }
            }
        }
    }
}
