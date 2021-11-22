using System.Text;
using Ubii.Services;
using NetMQ.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;

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
        Debug.Log("this.serviceURL = " + this.serviceURL);
    }

    public async Task<ServiceReply> CallService(ServiceRequest request)
    {
        Debug.Log("UbiiServiceClientREST.CallService()");
        Debug.Log(request.ToString());
        StringContent content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await this.httpClient.PostAsync(this.serviceURL, content);
        Debug.Log(response.ToString());
        string responseJSON = await response.Content.ReadAsStringAsync();
        Debug.Log("responseJSON:");
        Debug.Log(responseJSON);
        //TODO: this is a stupid hack for the Google.Protobuf.JsonParser as the server JSON response includes an identifier for the
        // protocol buffer oneof field "type" that would result in an error
        string jsonModified = Regex.Replace(responseJSON, ",\"type\":\".*\"", "");
        jsonModified = Regex.Replace(jsonModified, "\"type\":\".*\"", "");
        Debug.Log("jsonModified:");
        Debug.Log(jsonModified);
        ServiceReply serviceReply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(jsonModified);
        Debug.Log("serviceReply:");
        Debug.Log(serviceReply);

        return serviceReply;

        //return new ServiceReply { Error = new Ubii.General.Error { Message = "not implemented" } };
    }

    public void TearDown()
    {
    }
}
