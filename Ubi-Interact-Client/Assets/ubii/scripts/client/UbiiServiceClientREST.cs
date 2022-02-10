using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

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
        //Debug.LogError("UbiiServiceClientREST.CallService() this.serviceURL=" + this.serviceURL);
        //Debug.LogError("UbiiServiceClientREST.CallService() this.httpClient=" + this.httpClient);
        //Debug.LogError("UbiiServiceClientREST.CallService() request: " + request);

        // JSON
        /*string requestJSON = jsonFormatter.Format(request);
        Debug.LogError("UbiiServiceClientREST.CallService() requestJSON=" + requestJSON);*/
        // BINARY
        MemoryStream memoryStream = new MemoryStream();
        CodedOutputStream codedOutputStream = new CodedOutputStream(memoryStream);
        request.WriteTo(codedOutputStream);
        codedOutputStream.Flush();
        var bytebuffer = memoryStream.ToArray();

        Uri uri = new Uri(this.serviceURL);
        string responseJSON = null;
        try
        {
#if WINDOWS_UWP

        // JSON
        /*HttpStringContent content = new HttpStringContent(requestJSON, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
        responseJSON = await httpResponseMessage.Content.ReadAsStringAsync();*/
        // BINARY
        HttpBufferContent content { bytebuffer };
        HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(uri, content);
        Debug.LogError("UbiiServiceClientREST.CallService() StatusCode: " + httpResponseMessage.StatusCode);
        // Make sure the post succeeded, and write out the response.
        httpResponseMessage.EnsureSuccessStatusCode();

        byte[] responseByteArray = await httpResponseMessage.Content.ReadAsByteArrayAsync();
        ServiceReply reply = ServiceReply.Parser.ParseFrom(responseByteArray, 0, responseByteArray.Length);
        Debug.LogError(reply.ToString());
#else
        // JSON
        /*StringContent content = new StringContent(requestJSON, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await this.httpClient.PostAsync(this.serviceURL, content);
        response.EnsureSuccessStatusCode();
        responseJSON = await response.Content.ReadAsStringAsync();*/
        // BINARY
        ByteArrayContent content = new ByteArrayContent(bytebuffer);
        HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(uri, content);
        Debug.LogError("UbiiServiceClientREST.CallService() StatusCode: " + httpResponseMessage.StatusCode);
        // Make sure the post succeeded, and write out the response.
        httpResponseMessage.EnsureSuccessStatusCode();

        byte[] responseByteArray = await httpResponseMessage.Content.ReadAsByteArrayAsync();
        ServiceReply reply = ServiceReply.Parser.ParseFrom(responseByteArray, 0, responseByteArray.Length);
        Debug.LogError(reply.ToString());
#endif
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }

        if (responseJSON == null)
        {
            //throw new Exception("UBII - CallService() response is null");
            //Debug.LogError("UbiiServiceClientREST.CallService() responseJSON: " + responseJSON);
            return null;
        }

        //Debug.LogError("UbiiServiceClientREST.CallService() responseJSON: " + responseJSON);
        //NOTE: UWP does not support JSON parsing! Probably the use of reflections is the problem
        //ServiceReply serviceReply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(responseJSON);
        //return serviceReply;
        return null;
    }

    public void TearDown()
    {
    }
}
