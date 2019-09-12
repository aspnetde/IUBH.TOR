using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Authentication.Pages;
using IUBH.TOR.Modules.Courses.Pages;
using IUBH.TOR.Modules.Shared.Domain;
using IUBH.TOR.Modules.Shared.Services;
using IUBH.TOR.Utilities.BackgroundSync;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests
{
    public class AppSpecs
    {
        [TestFixture]
        public class When_there_are_no_user_credentials_available
        {
            [Test]
            public async Task The_app_starts_with_the_login_page()
            {
                var credentialStorage = Substitute.For<ICredentialStorage>();
                var backgroundSync = Substitute.For<IBackgroundSyncUtility>();

                credentialStorage.GetCredentialsAsync().Returns(
                    Task.FromResult(Result.WithError<Credentials>("Some Error"))
                );

                var result = await App.InitializeAsync(credentialStorage, backgroundSync)
                    .ConfigureAwait(false);

                result.ShouldBeSameAs(typeof(LoginPage));
            }
            
            [Test]
            public async Task The_BackgroundSync_is_not_being_enabled()
            {
                var credentialStorage = Substitute.For<ICredentialStorage>();
                var backgroundSync = Substitute.For<IBackgroundSyncUtility>();

                credentialStorage.GetCredentialsAsync().Returns(
                    Task.FromResult(Result.WithError<Credentials>("Some Error"))
                );

                await App.InitializeAsync(credentialStorage, backgroundSync)
                    .ConfigureAwait(false);

                backgroundSync.DidNotReceive().Enable();
            }
        }

        [TestFixture]
        public class When_user_credentials_are_available
        {
            [Test]
            public async Task The_app_starts_with_the_Course_List_page()
            {
                var credentialStorage = Substitute.For<ICredentialStorage>();
                var backgroundSync = Substitute.For<IBackgroundSyncUtility>();
                
                credentialStorage.GetCredentialsAsync().Returns(
                    Task.FromResult(Result.WithSuccess(new Credentials("Foo", "Bar")))
                );

                var result = await App.InitializeAsync(credentialStorage, backgroundSync)
                    .ConfigureAwait(false);

                result.ShouldBeSameAs(typeof(CourseListPage));
            }
            
            [Test]
            public async Task The_BackgroundSync_is_being_enabled()
            {
                var credentialStorage = Substitute.For<ICredentialStorage>();
                var backgroundSync = Substitute.For<IBackgroundSyncUtility>();
                
                credentialStorage.GetCredentialsAsync().Returns(
                    Task.FromResult(Result.WithSuccess(new Credentials("Foo", "Bar")))
                );

                await App.InitializeAsync(credentialStorage, backgroundSync)
                    .ConfigureAwait(false);

                backgroundSync.Received().Enable();
            }
        }
    }
}
