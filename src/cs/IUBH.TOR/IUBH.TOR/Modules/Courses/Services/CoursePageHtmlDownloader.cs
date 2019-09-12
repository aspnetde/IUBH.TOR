using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Shared.Services;

namespace IUBH.TOR.Modules.Courses.Services
{
    internal class CoursePageHtmlDownloader : ICoursePageHtmlDownloader
    {
        private readonly ICredentialStorage _credentialStorage;

        public CoursePageHtmlDownloader(ICredentialStorage credentialStorage)
        {
            _credentialStorage = credentialStorage;
        }

        /// <summary>
        /// Tries to download the HTML page off the CARE system. Actually uses
        /// the user's credentials to authenticate and then opens the ToR web page
        /// to get its HTML content.
        /// </summary>
        public async Task<Result<string>> TryDownloadCoursePageHtmlAsync(string url)
        {
            try
            {
                var httpClientResult =
                    await TryGetAuthenticatedHttpClientAsync().ConfigureAwait(false);

                if (!httpClientResult.IsSuccessful)
                {
                    return httpClientResult.ToResult<string>();
                }

                var html = await httpClientResult.Value.GetStringAsync(url).ConfigureAwait(false);

                return Result.WithSuccess(html);
            }
            catch (Exception e)
            {
                return Result.WithException<string>(e);
            }
        }

        private async Task<Result<HttpClient>> TryGetAuthenticatedHttpClientAsync()
        {
            try
            {
                var credentialsResult = await _credentialStorage.GetCredentialsAsync()
                    .ConfigureAwait(false);

                if (!credentialsResult.IsSuccessful)
                {
                    return credentialsResult.ToResult<HttpClient>();
                }

                // When we're authenticated, the session cookie will be
                // contained in that container. By re-using it for all
                // subsequent requests we appear to be rightfully authenticated
                // to the backend system. So this is the key part.
                var cookieContainer = new CookieContainer();

                var handler = new HttpClientHandler
                {
                    CookieContainer = cookieContainer
                };

                var httpClient = new HttpClient(handler);

                var content = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("login-form", "login-form"),
                        new KeyValuePair<string, string>("user", credentialsResult.Value.UserName),
                        new KeyValuePair<string, string>(
                            "password",
                            credentialsResult.Value.Password
                        )
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
                    return Result.WithError<HttpClient>(Constants.InvalidCredentialsMessage);
                }

                return Result.WithSuccess(httpClient);
            }
            catch (Exception e)
            {
                return Result.WithException<HttpClient>(e);
            }
        }
    }
}