using UnityEngine;
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
    private Google.Protobuf.JsonFormatter.Settings jsonFormatSettings;
    private Google.Protobuf.JsonFormatter jsonFormatter;

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
        jsonFormatSettings = new Google.Protobuf.JsonFormatter.Settings(true).WithFormatEnumsAsIntegers(true);
        jsonFormatter = new Google.Protobuf.JsonFormatter(jsonFormatSettings);
    }

    public async Task<ServiceReply> CallService(ServiceRequest request)
    {
        //Debug.LogError("UbiiServiceClientREST.CallService() jsonFormatter: " + jsonFormatter);
        string requestJSON = jsonFormatter.Format(request);

        string responseJSON = null;
#if WINDOWS_UWP
        Uri uri = new Uri(this.serviceURL);
        // Construct the JSON to post.
        HttpStringContent content = new HttpStringContent(requestJSON, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
        // Post the JSON and wait for a response.
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(uri, content);
        // Make sure the post succeeded, and write out the response.
        httpResponseMessage.EnsureSuccessStatusCode();
        //string httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
        //responseJSON = httpResponseBody;
#else
        StringContent content = new StringContent(requestJSON, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await this.httpClient.PostAsync(this.serviceURL, content);
        response.EnsureSuccessStatusCode();
        responseJSON = await response.Content.ReadAsStringAsync();
#endif

        if (responseJSON == null) throw new Exception("UBII - CallService() response is null");

        ServiceReply serviceReply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(responseJSON);
        return serviceReply;
    }

    public void TearDown()
    {
    }
}
