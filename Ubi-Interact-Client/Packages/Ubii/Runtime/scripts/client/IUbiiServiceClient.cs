using System.Threading.Tasks;
using Ubii.Services;

interface IUbiiServiceClient
{
    Task<ServiceReply> CallService(ServiceRequest request);
}

