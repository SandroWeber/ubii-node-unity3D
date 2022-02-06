using System.Text;
using Ubii.Services;
using NetMQ.Sockets;
using System.Threading.Tasks;
using System.Net.Http;

class UbiiServiceClientREST : IUbiiServiceClient
{
    private string host;
    private int port;
    private string serviceRoute;

    private HttpClient httpClient;
    private string serviceURL;

    RequestSocket socket;

    public UbiiServiceClientREST(string host = "https://localhost", int port = 8102, string serviceRoute = "/services")
    {
        this.host = host;
        this.port = port;
        this.serviceRoute = serviceRoute;

        this.httpClient = new HttpClient();
        this.serviceURL += this.host + ":" + this.port + this.serviceRoute;
    }

    public async Task<ServiceReply> CallService(ServiceRequest request)
    {
        Google.Protobuf.JsonFormatter.Settings settings = new Google.Protobuf.JsonFormatter.Settings(true).WithFormatEnumsAsIntegers(true);
        string requestJSON = new Google.Protobuf.JsonFormatter(settings).Format(request);
        StringContent content = new StringContent(requestJSON, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await this.httpClient.PostAsync(this.serviceURL, content);

        string responseJSON = await response.Content.ReadAsStringAsync();
        //TODO: this is a stupid hack for the Google.Protobuf.JsonParser as the server JSON response includes an identifier for the
        // protocol buffer oneof field "type" that would result in an error
        /*string responseCleaned = Regex.Replace(responseJSON, ",\"type\":\".*\"", "");
        responseCleaned = Regex.Replace(responseCleaned, "\"type\":\".*\"", "");*/

        ServiceReply serviceReply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(responseJSON);

        return serviceReply;

        //return new ServiceReply { Error = new Ubii.General.Error { Message = "not implemented" } };
    }

    public void TearDown()
    {
    }
}
