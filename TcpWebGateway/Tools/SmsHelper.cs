using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;
using Aliyun.Acs.Core.Profile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;


namespace TcpWebGateway.Tools
{
    public interface INotify
    {
        void Send(string message);
    }
    public class SmsHelper : INotify
    {
        private readonly AliSecret Secret;
        private readonly ILogger _logger;

        public SmsHelper(IConfiguration configuration,ILogger<SmsHelper> logger)
        {
            _logger = logger;
            try
            {
                var connStr = configuration.GetValue<string>("Mongodb:ConnectString");
                var client = new MongoClient(connStr);
                var db = client.GetDatabase("UserSecrets");
                var secrets = db.GetCollection<AliSecret>("Secrets");
                Secret = secrets.Find(new BsonDocument()).ToList().FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
            

        }
        public void Send(string message)
        {
            
            IClientProfile profile = DefaultProfile.GetProfile("default", Secret.AliyunSecretId, Secret.AliyunSecretKey);
            DefaultAcsClient client = new DefaultAcsClient(profile);
            CommonRequest request = new CommonRequest();
            request.Method = MethodType.POST;
            request.Domain = "dysmsapi.aliyuncs.com";
            request.Version = "2017-05-25";
            request.Action = "SendSms";
            // request.Protocol = ProtocolType.HTTP;
            request.AddQueryParameters("PhoneNumbers", "18107718055");
            request.AddQueryParameters("SignName", "青云微笙");
            request.AddQueryParameters("TemplateCode", "SMS_171853916");
            request.AddQueryParameters("TemplateParam", "{'name':' " + message + "'}");
            try
            {
                CommonResponse response = client.GetCommonResponse(request);
                _logger.LogInformation(System.Text.Encoding.Default.GetString(response.HttpResponse.Content));
            }
            catch (ServerException e)
            {
                _logger.LogError(e.ToString());
            }
            catch (ClientException e)
            {
                _logger.LogError(e.ToString());
            }
        }

        private class AliSecret
        {
            public ObjectId Id { get; set; }
            public string AliyunSecretId { get; set; }
            public string AliyunSecretKey { get; set; }
        }
    }


}
