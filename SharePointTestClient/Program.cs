using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace SharePointTestClient
{
    internal class Program
    {
        private static PublicClientApplicationOptions appConfiguration = null;
        private static IConfiguration configuration;
        private static string MSGraphUrl;
        private static string SharePointSiteUrl;
        private static string SharePointListUrl;

        // The MSAL Public client app
        private static IPublicClientApplication application;

        private static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            configuration = builder.Build();
            appConfiguration = configuration
                .Get<PublicClientApplicationOptions>();

            Console.WriteLine("-------- Data from call to MS Graph --------");
            Console.Write(Environment.NewLine);
            string[] graphScopes = new[] { "User.Read" };
            MSGraphUrl = configuration.GetValue<string>("GraphApiUrl");
            GraphServiceClient graphClient = await SignInAndInitializeGraphServiceClient(appConfiguration, graphScopes);
            await CallMSGraph(graphClient);

            Console.Write(Environment.NewLine);
            Console.WriteLine("-------- Data from calls to SharePoint Online --------");
            Console.Write(Environment.NewLine);
            // TBD: Consider using MS Graph for SharePoint Online
            string[] sharepointScopes = new[] { "AllSites.Manage" };
            SharePointSiteUrl = configuration.GetValue<string>("SharePointUrlPublic");
            SharePointListUrl = configuration.GetValue<string>("SharePointListUrlPublic");
            await GetSharePointSite(appConfiguration, sharepointScopes);
            await GetSharePointList(appConfiguration, sharepointScopes);
            await GetSharePointList2(appConfiguration, sharepointScopes);
        }
        private static async Task<string> SignInUserAndGetTokenUsingMSAL(PublicClientApplicationOptions configuration, string[] scopes)
        {
            try
            {
                string authority = string.Concat(configuration.Instance, configuration.TenantId);
                application = PublicClientApplicationBuilder.Create(configuration.ClientId)
                                                        .WithAuthority(authority)
                                                        .WithDefaultRedirectUri()
                                                        .Build();
                AuthenticationResult result;
                try
                {
                    var accounts = await application.GetAccountsAsync();
                    result = await application.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                     .ExecuteAsync();
                }
                catch (MsalUiRequiredException ex)
                {
                    result = await application.AcquireTokenInteractive(scopes)
                     //.WithClaims(ex.Claims)
                     .ExecuteAsync();
                }

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
            return null;
        }
        private async static Task<GraphServiceClient> SignInAndInitializeGraphServiceClient(PublicClientApplicationOptions configuration, string[] scopes)
        {
            GraphServiceClient graphClient = new GraphServiceClient(MSGraphUrl,
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInUserAndGetTokenUsingMSAL(configuration, scopes));
                }));

            return await Task.FromResult(graphClient);
        }
        private async static Task<string> GetSharePointSite(PublicClientApplicationOptions configuration, string[] scopes)
        {
            try
            {
                var accessToken = await SignInUserAndGetTokenUsingMSAL(configuration, scopes);
                ClientContext context = new ClientContext(SharePointSiteUrl);
                Web web = context.Web;
                context.Load(web, w => w.Title, w => w.Description);
                context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>((s, e) =>
                {
                    e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
                });
                context.ExecuteQuery();
                Console.WriteLine(web.Title);
                Console.WriteLine(web.Description);
                return web.Title;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex}");
            }
            return null;
         }
         private async static Task<string> GetSharePointList(PublicClientApplicationOptions configuration, string[] scopes)
         {
            try
            {
                var accessToken = await SignInUserAndGetTokenUsingMSAL(configuration, scopes);
                ClientContext context = new ClientContext(SharePointListUrl);
                Web web = context.Web;
                // Retrieve all lists from the server.
                // For each list, retrieve Title and Id.
                context.Load(web.Lists,
                             lists => lists.Include(list => list.Title,
                                                    list => list.Id));
                context.Load(web, w => w.Title, w => w.Description);
                context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>((s, e) =>
                {
                    e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
                });
                context.ExecuteQuery();
                // Enumerate the web.Lists.
                foreach (var list in web.Lists)
                {
                    Console.WriteLine(list.Title);
                    return list.Title;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex}");
            }
            return null;
        }
        private async static Task<string> GetSharePointList2(PublicClientApplicationOptions configuration, string[] scopes)
        {
            try
            {
                var accessToken = await SignInUserAndGetTokenUsingMSAL(configuration, scopes);
                ClientContext context = new ClientContext(SharePointSiteUrl);
                Web web = context.Web;
                // Retrieve all lists from the server, and put the return value in another
                // collection instead of the web.Lists.
                var result = context.LoadQuery(
                  web.Lists.Include(
                      // For each list, retrieve Title and Id.
                      list => list.Title,
                      list => list.Id
                  )
                );
                context.Load(web, w => w.Title, w => w.Description);
                context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>((s, e) =>
                {
                    e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
                });
                context.ExecuteQuery();
                foreach (var list in result)
                {
                    Console.WriteLine(list.Title);
                    return list.Title;
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex}");
            }
            return null;
        }
        void UpdateListSample()
        {
            // TBD: To be developed.

            // Starting with ClientContext, the constructor requires a URL to the
            // server running SharePoint.
            ClientContext context = new ClientContext(SharePointSiteUrl);

            // The SharePoint web at the URL.
            Web web = context.Web;

            ListCreationInformation creationInfo = new ListCreationInformation();
            creationInfo.Title = "My List";
            creationInfo.TemplateType = (int)ListTemplateType.Announcements;
            var list = web.Lists.Add(creationInfo);
            list.Description = "New Description";

            list.Update();
            context.ExecuteQuery();
        }
        void AddFieldToListSample()
        {
            // TBD: To be developed.

            // Starting with ClientContext, the constructor requires a URL to the
            // server running SharePoint.
            ClientContext context = new ClientContext(SharePointSiteUrl);

            var list = context.Web.Lists.GetByTitle("Announcements");

            var field = list.Fields.AddFieldAsXml("<Field DisplayName='MyField2' Type='Number' />",
                                                       true,
                                                       AddFieldOptions.DefaultValue);
            var fldNumber = context.CastTo<FieldNumber>(field);
            fldNumber.MaximumValue = 100;
            fldNumber.MinimumValue = 35;
            fldNumber.Update();

            context.ExecuteQuery();
        }
        void RetrieveListItemsSample()
        {
            // TBD: To be developed.

            // Starting with ClientContext, the constructor requires a URL to the
            // server running SharePoint.
            ClientContext context = new ClientContext(SharePointSiteUrl);

            // Assume the web has a list named "Announcements".
            var announcementsList = context.Web.Lists.GetByTitle("Announcements");

            // This creates a CamlQuery that has a RowLimit of 100, and also specifies Scope="RecursiveAll"
            // so that it grabs all list items, regardless of the folder they are in.
            CamlQuery query = CamlQuery.CreateAllItemsQuery(100);
            ListItemCollection items = announcementsList.GetItems(query);

            // Retrieve all items in the ListItemCollection from List.GetItems(Query).
            context.Load(items);
            context.ExecuteQuery();
            foreach (var listItem in items)
            {
                Console.WriteLine(listItem["Title"]);
            }
        }
        void UpdatingSample()
        {
            // TBD: To be developed.

            // Starting with ClientContext, the constructor requires a URL to the
            // server running SharePoint.
            ClientContext context = new ClientContext(SharePointSiteUrl);

            // The SharePoint web at the URL.
            Web web = context.Web;

            web.Title = "New Title";
            web.Description = "New Description";

            // Note that the web.Update() doesn't trigger a request to the server.
            // Requests are only sent to the server from the client library when
            // the ExecuteQuery() method is called.
            web.Update();

            // Execute the query to server.
            context.ExecuteQuery();
        }
        private static async Task CallMSGraph(GraphServiceClient graphClient)
        {
            var me = await graphClient.Me.Request().GetAsync();
            Console.WriteLine($"Id: {me.Id}");
            Console.WriteLine($"Display Name: {me.DisplayName}");
            Console.WriteLine($"Email: {me.Mail}");
        }
    }
}
