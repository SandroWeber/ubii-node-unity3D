using System;
using System.Text;
using System.Threading.Tasks;

using Ubii.Services;

#if WINDOWS_UWP
using Windows.Web.Http;
#else
using System.Net.Http;
#endif

class UbiiServiceClientREST : IUbiiServiceClient
{
    private string host;
    private int port;
    private string serviceRoute;

#if WINDOWS_UWP
    private Windows.Web.Http.HttpClient httpClient;
#else
    private System.Net.Http.HttpClient httpClient;
#endif
    
    private string serviceURL;

    public UbiiServiceClientREST(string host = "https://localhost", int port = 8102, string serviceRoute = "/services")
    {
        this.host = host;
        this.port = port;
        this.serviceRoute = serviceRoute;

#if WINDOWS_UWP
        this.httpClient = new Windows.Web.Http.HttpClient();
#else
        this.httpClient = new System.Net.Http.HttpClient();
#endif

        this.serviceURL += this.host + ":" + this.port + this.serviceRoute;
    }

    public async Task<ServiceReply> CallService(ServiceRequest request)
    {
        Google.Protobuf.JsonFormatter.Settings settings = new Google.Protobuf.JsonFormatter.Settings(true).WithFormatEnumsAsIntegers(true);
        string requestJSON = new Google.Protobuf.JsonFormatter(settings).Format(request);

        string responseJSON = null;
#if WINDOWS_UWP
        Console.WriteLine("WINDOWS_UWP UbiiServiceClientREST.CallService() missing implementation");
#else
        StringContent content = new StringContent(requestJSON, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await this.httpClient.PostAsync(this.serviceURL, content);
        responseJSON = await response.Content.ReadAsStringAsync();
#endif

        if (responseJSON == null) return null;
        ServiceReply serviceReply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(responseJSON);
        return serviceReply;
    }

    public void TearDown()
    {
    }
}
