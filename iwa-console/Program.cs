// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace iwa_console
{
    /// <summary>
    /// This sample signs-in the user signed-in on a Windows machine joined to a Windows domain or AAD joined
    /// For more information see https://aka.ms/msal-net-iwa
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync()
        {
            var clientId = "2613eda7-7162-485b-bd3d-8b621006aa38";
            var clientUrl = new Uri("http://localhost");  //can be anything starting with localhost
            var tenant = "191657ea-bcff-4385-9d04-659ef9cee515";
            string authority = "https://login.windows.net/" + tenant;

            string resource = "api://eff0ae4e-c1d8-4f68-b2e0-fb91977028d1/";  ///put id or url of resource you're accessing

            AuthenticationContext authenticationContext = new AuthenticationContext(authority, false);

            Console.WriteLine("Trying to acquire token");

            var pp = new PlatformParameters(PromptBehavior.Auto); //this brings web prompt
            var token = authenticationContext.AcquireTokenAsync(resource, clientId, clientUrl, pp, UserIdentifier.AnyUser).Result;
            Console.WriteLine("Got the token: {0}", token.AccessToken);

            HttpClient client = new HttpClient();

            SampleConfiguration config = SampleConfiguration.ReadFromJsonFile("appsettings.json");
            var appConfig = config.PublicClientApplicationOptions;

            var path = new Uri(config.SampleApiBaseEndpoint);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();


                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine(jsonString);
            }


            //now try MS Graph
            resource = "https://graph.microsoft.com";  ///put id or url of resource you're accessing

            Console.WriteLine("Trying to get my information");

            pp = new PlatformParameters(PromptBehavior.Auto); //this brings web prompt
            token = authenticationContext.AcquireTokenAsync(resource, clientId, clientUrl, pp, UserIdentifier.AnyUser).Result;
            Console.WriteLine("Got the token: {0}", token.AccessToken);


            path = new Uri($"{config.MicrosoftGraphBaseEndpoint}/v1.0/me");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

            response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine(jsonString);
            }

            var protectedApiCallHelper = new ProtectedApiCallHelper(client);


            path = new Uri($"{config.MicrosoftGraphBaseEndpoint}/v1.0/me");
            await CallWebApiAndDisplayResultAsync(protectedApiCallHelper, path.ToString(), token.AccessToken, "Me");

            //var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(appConfig).Build();

            //MyInformation myInformation = new MyInformation(app, client, config.MicrosoftGraphBaseEndpoint);
            //await myInformation.DisplayMeAndMyManagerAsync();

            ////SampleConfiguration config = SampleConfiguration.ReadFromJsonFile("appsettings.json");
            //var appConfig = config.PublicClientApplicationOptions;
            //var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(appConfig)
            //                                        .Build();
            //var httpClient = new HttpClient();

            //MyInformation myInformation = new MyInformation(app, httpClient, config.MicrosoftGraphBaseEndpoint);
            //await myInformation.DisplayMeAndMyManagerAsync();
        }

        private static async Task CallWebApiAndDisplayResultAsync(ProtectedApiCallHelper helper, string url, string accessToken, string title)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(title);
            Console.ResetColor();
            await helper.CallWebApiAndProcessResultAsync(url, accessToken, Display);
            Console.WriteLine();
        }

        /// <summary>
        /// Display the result of the web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            {
                Console.WriteLine($"{child.Name} = {child.Value}");
            }
        }

    }
}
