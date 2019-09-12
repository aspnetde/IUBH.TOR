using System;
using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Authentication.Pages;
using IUBH.TOR.Modules.Authentication.Services;
using IUBH.TOR.Modules.Shared.Domain;
using IUBH.TOR.Modules.Shared.Services;
using IUBH.TOR.Utilities.Alerts;
using IUBH.TOR.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.Hud;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Authentication
{
    public class LoginViewModelSpecs
    {
        private static LoginViewModel CreateSut(
            IHudUtility hudUtility = null,
            IAlertUtility alertUtility = null,
            ICredentialStorage credentialStorage = null,
            ICredentialValidator credentialValidator = null,
            IBackgroundSyncUtility backgroundSync = null
        )
        {
            if (credentialValidator == null)
            {
                credentialValidator = Substitute.For<ICredentialValidator>();

                credentialValidator.ValidateAsync(Arg.Any<Credentials>())
                    .Returns(Task.FromResult(Result.Success));
            }

            if (credentialStorage == null)
            {
                credentialStorage = Substitute.For<ICredentialStorage>();

                credentialStorage.SaveCredentialsAsync(Arg.Any<Credentials>())
                    .Returns(Task.FromResult(Result.Success));
            }

            return new LoginViewModel(
                credentialValidator,
                credentialStorage,
                hudUtility ?? Substitute.For<IHudUtility>(),
                alertUtility ?? Substitute.For<IAlertUtility>(),
                backgroundSync ?? Substitute.For<IBackgroundSyncUtility>()
            );
        }

        [TestFixture]
        public class A_sign_in_is_not_possible
        {
            [Test]
            [TestCase("")]
            [TestCase(null)]
            public void When_the_user_name_is_not_provided(string userName)
            {
                var sut = CreateSut();

                sut.Password = "Password";
                sut.UserName = userName;
                sut.IsBusy = false;

                sut.SignInCommand.CanExecute(null).ShouldBeFalse();
            }

            [Test]
            [TestCase("")]
            [TestCase(null)]
            public void When_the_password_is_not_provided(string password)
            {
                var sut = CreateSut();

                sut.Password = password;
                sut.UserName = "UserName";
                sut.IsBusy = false;

                sut.SignInCommand.CanExecute(null).ShouldBeFalse();
            }

            [Test]
            public void When_the_view_model_is_busy()
            {
                var sut = CreateSut();

                sut.Password = "Password";
                sut.UserName = "UserName";
                sut.IsBusy = true;

                sut.SignInCommand.CanExecute(null).ShouldBeFalse();
            }
        }

        [TestFixture]
        public class A_sign_in_is_possible
        {
            [Test]
            public void When_user_name_and_password_are_provided_and_the_view_model_is_not_busy()
            {
                var sut = CreateSut();

                sut.Password = "Password";
                sut.UserName = "UserName";
                sut.IsBusy = false;

                sut.SignInCommand.CanExecute(null).ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_the_user_is_being_signed_in
        {
            [Test]
            public async Task A_HUD_is_being_shown()
            {
                var hud = Substitute.For<IHudUtility>();
                var sut = CreateSut(hudUtility: hud);

                await sut.SignInAsync().ConfigureAwait(false);

                hud.Received().Show(Arg.Is(LoginViewModel.LoginMessage));
            }

            [Test]
            public async Task The_shown_HUD_is_being_dismissed_again()
            {
                var hud = Substitute.For<IHudUtility>();
                var sut = CreateSut(hudUtility: hud);

                await sut.SignInAsync().ConfigureAwait(false);

                hud.Received().Dismiss();
            }

            [Test]
            public async Task UserName_and_Password_are_being_validated()
            {
                var credentialValidator = Substitute.For<ICredentialValidator>();

                credentialValidator.ValidateAsync(Arg.Any<Credentials>())
                    .Returns(Task.FromResult(Result.Success));

                var credentialStorage = Substitute.For<ICredentialStorage>();

                credentialStorage.SaveCredentialsAsync(Arg.Any<Credentials>())
                    .Returns(Task.FromResult(Result.Success));

                var sut = CreateSut(
                    credentialStorage: credentialStorage,
                    credentialValidator: credentialValidator
                );

                string userName = Guid.NewGuid().ToString();
                string password = Guid.NewGuid().ToString();

                sut.UserName = userName;
                sut.Password = password;

                await sut.SignInAsync().ConfigureAwait(false);

                await credentialValidator.Received().ValidateAsync(
                    Arg.Is<Credentials>(c => c.UserName == userName && c.Password == password)
                ).ConfigureAwait(false);
            }

            [TestFixture]
            public class And_the_credentials_are_invalid
            {
                [Test]
                public async Task An_Error_Message_is_being_shown()
                {
                    var credentialValidator = Substitute.For<ICredentialValidator>();
                    string errorMessage = Guid.NewGuid().ToString();

                    credentialValidator.ValidateAsync(Arg.Any<Credentials>())
                        .Returns(Task.FromResult(Result.WithError(errorMessage)));

                    var alertUtility = Substitute.For<IAlertUtility>();

                    var sut = CreateSut(credentialValidator: credentialValidator, alertUtility: alertUtility);

                    await sut.SignInAsync().ConfigureAwait(false);

                    alertUtility.Received().ShowError(Arg.Is(errorMessage));
                }
            }

            [TestFixture]
            public class And_user_name_and_password_are_valid
            {
                [Test]
                public async Task The_credentials_are_being_saved()
                {
                    var credentialValidator = Substitute.For<ICredentialValidator>();

                    credentialValidator.ValidateAsync(Arg.Any<Credentials>())
                        .Returns(Task.FromResult(Result.Success));

                    var credentialStorage = Substitute.For<ICredentialStorage>();

                    credentialStorage.SaveCredentialsAsync(Arg.Any<Credentials>())
                        .Returns(Task.FromResult(Result.Success));

                    var sut = CreateSut(
                        credentialStorage: credentialStorage,
                        credentialValidator: credentialValidator
                    );

                    string userName = Guid.NewGuid().ToString();
                    string password = Guid.NewGuid().ToString();

                    sut.UserName = userName;
                    sut.Password = password;

                    await sut.SignInAsync().ConfigureAwait(false);

                    await credentialStorage.Received().SaveCredentialsAsync(
                        Arg.Is<Credentials>(c => c.UserName == userName && c.Password == password)
                    );
                }

                [Test]
                public async Task
                    An_Error_Message_is_being_shown_when_the_credentials_could_not_be_saved()
                {
                    var credentialValidator = Substitute.For<ICredentialValidator>();

                    credentialValidator.ValidateAsync(Arg.Any<Credentials>())
                        .Returns(Task.FromResult(Result.Success));

                    var credentialStorage = Substitute.For<ICredentialStorage>();

                    string errorMessage = Guid.NewGuid().ToString();

                    credentialStorage.SaveCredentialsAsync(Arg.Any<Credentials>())
                        .Returns(Task.FromResult(Result.WithError(errorMessage)));

                    var alertUtility = Substitute.For<IAlertUtility>();

                    var sut = CreateSut(
                        credentialStorage: credentialStorage,
                        alertUtility: alertUtility,
                        credentialValidator: credentialValidator
                    );

                    sut.UserName = Guid.NewGuid().ToString();
                    sut.Password = Guid.NewGuid().ToString();

                    await sut.SignInAsync().ConfigureAwait(false);

                    alertUtility.Received().ShowError(Arg.Is(errorMessage));
                }

                [Test]
                public async Task The_BackgroundSync_is_being_enabled()
                {
                    var backgroundSync = Substitute.For<IBackgroundSyncUtility>();
                    
                    var sut = CreateSut(backgroundSync: backgroundSync);

                    await sut.SignInAsync().ConfigureAwait(false);

                    backgroundSync.Received().Enable();
                }

                [Test]
                public async Task The_SignedIn_EventHandler_is_being_invoked()
                {
                    var sut = CreateSut();

                    string userName = Guid.NewGuid().ToString();
                    string password = Guid.NewGuid().ToString();

                    sut.UserName = userName;
                    sut.Password = password;

                    bool signedInEventHandlerWasCalled = false;

                    sut.SignedIn += (sender, args) => signedInEventHandlerWasCalled = true;

                    await sut.SignInAsync().ConfigureAwait(false);

                    signedInEventHandlerWasCalled.ShouldBeTrue();
                }
            }
        }
    }
}
