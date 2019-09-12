using System;
using System.Threading.Tasks;
using IUBH.TOR.Modules.Shared.Domain;
using IUBH.TOR.Modules.Shared.Services;
using IUBH.TOR.Utilities.SecureStorage;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Shared
{
    public class CredentialStorageSpecs
    {
        [TestFixture]
        public class When_credentials_are_being_saved_successfully
        {
            [Test]
            public async Task The_User_Name_is_being_saved()
            {
                var secureStorageUtility = Substitute.For<ISecureStorageUtility>();
                var credentialStorage = new CredentialStorage(secureStorageUtility);

                var credentials = new Credentials(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                );

                await credentialStorage.SaveCredentialsAsync(credentials).ConfigureAwait(false);

                await secureStorageUtility.Received().SetAsync(
                    Arg.Is(CredentialStorage.SecureStorageUserNameKey),
                    Arg.Is(credentials.UserName)
                ).ConfigureAwait(false);
            }

            [Test]
            public async Task The_Password_is_being_saved()
            {
                var secureStorageUtility = Substitute.For<ISecureStorageUtility>();
                var credentialStorage = new CredentialStorage(secureStorageUtility);

                var credentials = new Credentials(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                );

                await credentialStorage.SaveCredentialsAsync(credentials).ConfigureAwait(false);

                await secureStorageUtility.Received().SetAsync(
                    Arg.Is(CredentialStorage.SecureStoragePasswordKey),
                    Arg.Is(credentials.Password)
                ).ConfigureAwait(false);
            }

            [Test]
            public async Task A_successful_result_is_being_returned()
            {
                var credentialStorage =
                    new CredentialStorage(Substitute.For<ISecureStorageUtility>());

                var credentials = new Credentials("foo", "bar");

                var result = await credentialStorage.SaveCredentialsAsync(credentials)
                    .ConfigureAwait(false);

                result.IsSuccessful.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_an_exception_occurs_while_credentials_are_tried_to_be_saved
        {
            [Test]
            public async Task An_result_with_that_exception_is_being_returned()
            {
                var secureStorageUtility = Substitute.For<ISecureStorageUtility>();

                var exceptionMessage = Guid.NewGuid().ToString();
                var exception = new Exception(exceptionMessage);

                secureStorageUtility.SetAsync(Arg.Any<string>(), Arg.Any<string>())
                    .Throws(exception);

                var credentialStorage = new CredentialStorage(secureStorageUtility);

                var credentials = new Credentials("foo", "bar");

                var result = await credentialStorage.SaveCredentialsAsync(credentials)
                    .ConfigureAwait(false);

                result.IsSuccessful.ShouldBeFalse();
                result.Exception.ShouldBeSameAs(exception);
                result.ErrorMessage.ShouldBeSameAs(exceptionMessage);
            }
        }

        [TestFixture]
        public class When_Credentials_are_being_requested
        {
            [Test]
            public async Task
                A_successful_result_is_being_returned_when_both_UserName_and_Password_can_be_received()
            {
                string userName = Guid.NewGuid().ToString();
                string password = Guid.NewGuid().ToString();

                var secureStorageUtility = Substitute.For<ISecureStorageUtility>();

                secureStorageUtility
                    .GetAsync(Arg.Is(CredentialStorage.SecureStorageUserNameKey))
                    .Returns(userName);

                secureStorageUtility
                    .GetAsync(Arg.Is(CredentialStorage.SecureStoragePasswordKey))
                    .Returns(password);

                var credentialStorage = new CredentialStorage(secureStorageUtility);

                var result =
                    await credentialStorage.GetCredentialsAsync().ConfigureAwait(false);

                result.IsSuccessful.ShouldBeTrue();
                result.Value.UserName.ShouldBeSameAs(userName);
                result.Value.Password.ShouldBeSameAs(password);
            }

            [Test]
            public async Task
                A_nonesuccessful_result_is_being_returned_when_the_UserName_cannot_be_received()
            {
                string password = Guid.NewGuid().ToString();

                var secureStorageUtility = Substitute.For<ISecureStorageUtility>();

                secureStorageUtility
                    .GetAsync(Arg.Is(CredentialStorage.SecureStorageUserNameKey))
                    .Returns(default(string));

                secureStorageUtility
                    .GetAsync(Arg.Is(CredentialStorage.SecureStoragePasswordKey))
                    .Returns(password);

                var credentialStorage = new CredentialStorage(secureStorageUtility);

                var result =
                    await credentialStorage.GetCredentialsAsync().ConfigureAwait(false);

                result.IsSuccessful.ShouldBeFalse();
            }

            [Test]
            public async Task
                A_nonesuccessful_result_is_being_returned_when_the_Password_cannot_be_received()
            {
                string userName = Guid.NewGuid().ToString();

                var secureStorageUtility = Substitute.For<ISecureStorageUtility>();

                secureStorageUtility
                    .GetAsync(Arg.Is(CredentialStorage.SecureStorageUserNameKey))
                    .Returns(userName);

                secureStorageUtility
                    .GetAsync(Arg.Is(CredentialStorage.SecureStoragePasswordKey))
                    .Returns(default(string));

                var credentialStorage = new CredentialStorage(secureStorageUtility);

                var result =
                    await credentialStorage.GetCredentialsAsync().ConfigureAwait(false);

                result.IsSuccessful.ShouldBeFalse();
            }
        }
    }
}
