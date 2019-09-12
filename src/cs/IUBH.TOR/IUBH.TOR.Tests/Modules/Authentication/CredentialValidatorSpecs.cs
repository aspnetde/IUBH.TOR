using System;
using System.Threading.Tasks;
using IUBH.TOR.Modules.Authentication.Services;
using IUBH.TOR.Modules.Shared.Domain;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Authentication
{
    public class CredentialValidatorSpecs
    {
        [TestFixture]
        public class When_valid_credentials_are_being_validated
        {
            [Test]
            public async Task A_successful_result_is_being_returned()
            {
                string validUserName = CareTestCredentials.UserName;
                string validPassword = CareTestCredentials.Password;

                var credentials = new Credentials(validUserName, validPassword);

                var sut = new CredentialValidator();

                var result = await sut.ValidateAsync(credentials).ConfigureAwait(false);

                result.IsSuccessful.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_invalid_credentials_are_being_validated
        {
            [Test]
            public async Task The_result_is_not_successful_and_contains_an_error_message()
            {
                string invalidUserName = Guid.NewGuid().ToString();
                string invalidPassword = Guid.NewGuid().ToString();

                var credentials = new Credentials(invalidUserName, invalidPassword);

                var sut = new CredentialValidator();

                var result = await sut.ValidateAsync(credentials).ConfigureAwait(false);

                result.IsSuccessful.ShouldBeFalse();
                result.ErrorMessage.ShouldBeSameAs(Constants.InvalidCredentialsMessage);
            }
        }
    }
}
