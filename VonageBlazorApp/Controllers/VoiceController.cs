using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Vonage.Request;
using Vonage.Voice;
using Vonage.Voice.EventWebhooks;
using Vonage.Voice.Nccos;
using ServerSDK;
using Vonage.Utility;
using Vonage.Messaging;
using Vonage.Voice;
using Vonage.Voice.Nccos.Endpoints;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Net;

namespace Phone_Communication_Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VonageController : ControllerBase
    {
        private readonly IConfiguration _config;
        public static List<ActiveCall> activeCalls = new List<ActiveCall>();

        public VonageController(IConfiguration config)
        {
            _config = config;
        }


        [HttpGet]
        [Route("/webhooks/inbound-sms")]
        public IActionResult InboundSms()
        {
            var sms = WebhookParser.ParseQuery<InboundSms>(Request.Query);
            string _contact_id = ServerLibrary.CheckContactExists(sms.Msisdn);
            string TID = ServerLibrary.RetrieveTeamMateID_byNumber(sms.To);
            if (_contact_id == "")
            {
                Contacts newContact = new Contacts { phone_number = sms.Msisdn, ID = ServerLibrary.Generate_IDKey() };
                ServerLibrary.AddContactToDatabase(newContact).Wait();
            }
            Texts newText = new Texts { ID = sms.MessageId, contact_id = _contact_id, Message = sms.Text, teammate_ID = TID, is_teammate = false, account_phone_number_ID = sms.Msisdn, date_received = DateTime.Now};
            ServerLibrary.AddTextToDatabase(newText).Wait();

            return Ok();
        }

        [HttpGet]
        [Route("/webhooks/answer")]
        public IActionResult Answer()
        {
       
            string caller_number = this.Request.Query["To"].ToString();
            string team_mate_address = this.Request.Query["From"].ToString();
    
            var headers = new Dictionary<string, string>();
            headers.Add("app", "audiosocket");
            headers.Add("caller-id", team_mate_address);
            var toEndpoint = new PhoneEndpoint() { Number = caller_number };

            Console.WriteLine("Call Received by webhook! - Attempting to connect");
            var wsEndpoint = new WebsocketEndpoint()
            {
                Headers = headers,
                ContentType = "audio/l16;rate=8000",
                Uri = $"wss://upwork.bifrost.technology/ws/cid=" 
            };

            var connectAction = new ConnectAction() { Endpoint = new Vonage.Voice.Nccos.Endpoints.Endpoint[] {wsEndpoint}};

           
            var ncco = new Ncco(connectAction); 
            Console.WriteLine(ncco.ToString());
            return Ok(ncco.ToString());
        }

        [HttpPost]
        [Route("/webhooks/recording")]
        public IActionResult Recording()
        {
            Record record;
         
            var appId = "648cb905-8fe0-45ce-8800-bf8f9c735292";
            var privateKeyPath = @"C:\Certs\nexmo.key";
            var credentials = Credentials.FromAppIdAndPrivateKeyPath(appId, privateKeyPath);
            var voiceClient = new VoiceClient(credentials);
            string IDkey = ServerLibrary.Generate_IDKey();
          
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                record = JsonConvert.DeserializeObject<Record>(reader.ReadToEndAsync().Result);
                var recording = voiceClient.GetRecording(record.RecordingUrl);
                System.IO.File.WriteAllBytes(Directory.GetCurrentDirectory()+ "/assets/" +IDkey+".mp3", recording.ResultStream);
            }
           
            VoiceMails newVoiceMail = new VoiceMails { ID = IDkey, audio_url = record.RecordingUrl, date_received = DateTime.Today };
            ServerLibrary.AddVoiceMailToDatabase(newVoiceMail).Wait();
            Console.WriteLine($"Record event received on webhook - URL: {record?.RecordingUrl}");
            return StatusCode(204);
        }
        [HttpGet("/ws/{cID}")]
        public async Task Get()
        {
            var cID = Request.Query["cID"];
            Console.WriteLine("Websocket Server Reached! - " + HttpContext.WebSockets.IsWebSocketRequest);
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                if (activeCalls.Count() > 0)
                {
                    Console.WriteLine("Frontend is connected to the Websocket Server!");
                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(cID);
                    await FrontendRelay(webSocket, cID);
                }
                else
                {
                    Console.WriteLine("Vonage is connected to the Websocket Server!");
                    Console.WriteLine("Active Call Created!");
                    activeCalls.Add(new ActiveCall { ID=cID});
                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(cID);
                    await VonageRelay(webSocket, cID);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);

                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
        }
        private static async Task FrontendRelay(WebSocket webSocket, string cID)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            { 
                var currentCall = activeCalls[0];
                var RR = currentCall.speaker_RR;
                var speaker_buffer = currentCall.speaker_buffer;
                //sends speaker audio from storage to frontend
                if (currentCall != null & speaker_buffer != null)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(speaker_buffer, 0, speaker_buffer.Count()), WebSocketMessageType.Binary, true, CancellationToken.None);
                    Console.WriteLine("Sending speaker audio to frontend!");
                    Console.WriteLine(speaker_buffer.ToString() + "|" + RR.EndOfMessage);
                }
                
                //receives mic data from frontend and stores it
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                currentCall.mic_buffer = buffer;
                currentCall.mic_RR = receiveResult;
                activeCalls[0] = currentCall;
                Console.WriteLine("Receiving Mic data from frontend and storing it!");
            }
            activeCalls.Clear();
            await webSocket.CloseAsync(receiveResult.CloseStatus.Value,receiveResult.CloseStatusDescription,CancellationToken.None);
        }
        private static async Task VonageRelay(WebSocket webSocket, string cID)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                var currentCall = activeCalls[0];
                var RR = currentCall.mic_RR;
                var mic_buffer = currentCall.mic_buffer;
                if (mic_buffer != null & currentCall != null)
                {
                    //sends mic data to phone recipient
                    await webSocket.SendAsync(new ArraySegment<byte>(mic_buffer, 0, mic_buffer.Count()), WebSocketMessageType.Binary, true, CancellationToken.None);
                    Console.WriteLine("Sending mic data to phone caller!");
                    Console.WriteLine(mic_buffer.ToString() + "|" + RR.EndOfMessage);
                }
                //stores phones mic data as speaker audio
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                currentCall.speaker_buffer = buffer;
                currentCall.speaker_RR = receiveResult;
                activeCalls[0] = currentCall;
                Console.WriteLine("Storing data in speaker variables!");
            }
            activeCalls.Clear();
            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
        }
    }
    public class ActiveCall
    {
        public string ID { get; set; }
        public byte[] mic_buffer { get; set; }
        public byte[] speaker_buffer { get; set; } 
        public WebSocketReceiveResult speaker_RR { get; set; }
        public WebSocketReceiveResult mic_RR { get; set; }
    }

}
