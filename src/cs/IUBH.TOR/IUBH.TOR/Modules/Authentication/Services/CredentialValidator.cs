using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Shared.Domain;

namespace IUBH.TOR.Modules.Authentication.Services
{
    internal class CredentialValidator : ICredentialValidator
    {
        /// <summary>
        /// Makes sure the given credentials are valid so they can
        /// be used to log in to the CARE system. To do that an
        /// actual login is performed.
        /// </summary>
        public async Task<Result> ValidateAsync(Credentials credentials)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(
                        new[]
                        {
                            new KeyValuePair<string, string>("login-form", "login-form"),
                            new KeyValuePair<string, string>("user", credentials.UserName),
                            new KeyValuePair<string, string>("password", credentials.Password)
                        }
                    );

                    var uri = new Uri(Constants.CareLoginUrl);

                    HttpResponseMessage response =
                        await httpClient.PostAsync(uri, content).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    string responseHtml =
                        await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (responseHtml.Contains("Login credentials incorrect!")
                        || responseHtml.Contains("Anmeldedaten nicht korrekt."))
                    {
                        return Result.WithError(Constants.InvalidCredentialsMessage);
                    }

                    return Result.Success;
                }
            }
            catch (Exception e)
            {
                return Result.WithException(e);
            }
        }
    }
}
