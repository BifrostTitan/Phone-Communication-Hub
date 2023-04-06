using System;
using System.Threading.Tasks;
using Npgsql;
using Security;
using System.Collections.Generic;
using System.ComponentModel;
using Vonage;
using Vonage.Request;
using Vonage.Voice.Nccos.Endpoints;
using Vonage.Voice;
using System.Net.WebSockets;
using System.Threading;
using Google.Protobuf;
using Vonage.Voice.Nccos;
using Vonage.Applications.Capabilities;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Intrinsics.Arm;

namespace ServerSDK
{
    public class ServerLibrary
    {
       

        public const string DBconnection = "Host=localhost;Username=postgres;Password=DBPASSWORD;Database=vonageapp";

        public const string VonagePublic_Key = "SET_VONAGE_PUBLICKEY";

        public const string VonagePrivate_Key = "SET_VONAGE_PRIVATEKEY";
        public static readonly List<VonageCall> active_calls = new List<VonageCall>();
        public static string CheckLogin(string username, string password)
        {
            string teammateID = "";
            
            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"Teammates\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine(rdr.GetString(0));
                SHA512 sha = SHA512.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(rdr.GetString(6)));
                if (username == rdr.GetString(2) & password == Convert.ToBase64String(hash))
                {
                    teammateID = rdr.GetString(1);
                    Console.WriteLine("ID was found - "+rdr.GetString(1));
                }
            }
            con.Close();

            return teammateID;
        }
       
        public static async Task<AccountInfo> Login(string teammateID)
        {
            AccountInfo account = null;
            
            if (teammateID != "")
            {

                using var con = new NpgsqlConnection(DBconnection);
                con.Open();
                string sql = "SELECT * FROM public.\"Teammates\" where \"ID\" = '"+teammateID+"'";
                using var cmd = new NpgsqlCommand(sql, con);

                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                  account = new AccountInfo(rdr.GetString(1), rdr.GetString(0), rdr.GetString(2), rdr.GetString(3), rdr.GetInt32(4), rdr.GetInt32(5));
                }
                con.Close();
               
            }
            

            await Task.CompletedTask;
            return account;
        }
        public static bool CheckUserExists(string email)
        {
            bool exists = false;

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"Teammates\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine(rdr.GetString(0));
                if (email == rdr.GetString(2))
                {
                   exists = true;
                   Console.WriteLine("Email exists!");
                }
            }
            con.Close();

            return exists;
        }

        public static string CheckContactExists(string phonenumber)
        {
            string contact_id = "";

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"Contacts\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
              
                if (phonenumber == rdr.GetString(2))
                {
                    contact_id = rdr.GetString(1);
                    Console.WriteLine("Contact exists! Contact ID is - "+ contact_id);
                }
            }
            con.Close();

            return contact_id;
        }
        public static Contacts RetrieveContactByID(string ID)
        {
            Contacts contact = null;

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"Contacts\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                
                if (ID == rdr.GetString(1))
                {
                    contact = new Contacts { Name = rdr.GetString(0), ID = rdr.GetString(1), phone_number = rdr.GetString(2), email = rdr.GetString(3), tag_id = rdr.GetString(4), areacode = rdr.GetInt32(5), country_code = rdr.GetInt32(6), company = rdr.GetString(7), has_role_name = rdr.GetString(8), website = rdr.GetString(9) };
                    Console.WriteLine("Contact retrieved! - " + contact.phone_number);
                }
            }
            con.Close();

            return contact;
        }
        public static Contacts RetrieveContactByPN(string phonenumber)
        {
            Contacts contact = null;

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"Contacts\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine(rdr.GetString(0));
                if (phonenumber == rdr.GetString(2))
                {
                    contact = new Contacts { Name = rdr.GetString(0), ID = rdr.GetString(1), phone_number = rdr.GetString(2), email = rdr.GetString(3), tag_id = rdr.GetString(4), areacode = rdr.GetInt32(5), country_code = rdr.GetInt32(6), company = rdr.GetString(7), has_role_name = rdr.GetString(8), website = rdr.GetString(9)   };
                    Console.WriteLine("Contact retrieved! - " + contact.phone_number);
                }
            }
            con.Close();

            return contact;
        }
        public static string RetrievePhoneNumberByTID(string teammateID)
        {
            string phonenumber = "";

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"AccountPhoneNumbers\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                
                if (teammateID == rdr.GetString(2))
                {
                    phonenumber = rdr.GetString(0);
                    Console.WriteLine("Phone ID retrieved! - " + phonenumber);
                }
            }
            con.Close();

            return phonenumber;
        }
        public static string RetrieveTeamMateID_byNumber(string phonenumber)
        {
            string tid = "";

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"AccountPhoneNumbers\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
              
                if (phonenumber == rdr.GetString(0))
                {
                    tid = rdr.GetString(2);
                    Console.WriteLine("Phone ID retrieved! - "+ tid);
                }
            }
            con.Close();

            return tid;
        }
        public static string RetrieveTeamMateID_byEmail(string email)
        {
            string tID = "";

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"Teammates\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine(rdr.GetString(0));
                if (email == rdr.GetString(2))
                {
                    tID = rdr.GetString(1);
                    Console.WriteLine("Teammate ID retrieved!");
                }
            }
            con.Close();

            return tID;
        }

        public static List<TeamMate> RetrieveTeam()
        {
            List<TeamMate> team = new List<TeamMate>();

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"Teammates\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var teamMate = new TeamMate { Name = rdr.GetString(0), ID = rdr.GetString(1), Email = rdr.GetString(2), ImageURL = rdr.GetString(3) };
                team.Add(teamMate);
            }
            con.Close();

            return team;
        }
        public static List<Texts> RetrieveTexts(string teammate_ID)
        {
            List<Texts> texts = new List<Texts>();

            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            string sql = "SELECT * FROM public.\"TextMessages\"; ";
            using var cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();
     
            while (rdr.Read())
            {
               
                if (teammate_ID == rdr.GetString(9))
                {
                    
                    Texts text = new Texts { Message = rdr.GetString(0), ID = rdr.GetString(1), replying_to_msg_id = rdr.GetString(2), is_internal_msg = rdr.GetBoolean(3), is_teammate = rdr.GetBoolean(4), account_phone_number_ID = rdr.GetString(5), contact_id = rdr.GetString(6), is_unread = rdr.GetBoolean(7), media_url = rdr.GetString(8), teammate_ID = rdr.GetString(9), date_received = rdr.GetDateTime(10) };
                    texts.Add(text);
                    
                }
            }
        
            con.Close();

            return texts;
        }

        public static List<Contacts> BuildContactListFromTexts (List<Texts> _Texts, string TID)
        {
            List<Contacts> contacts = new List<Contacts>();
            List<string> number_list = new List<string>();
            foreach (Texts text in _Texts)
            {
                if (!number_list.Contains(text.account_phone_number_ID) & text.teammate_ID == TID)
                {
                    Contacts _contact = ServerLibrary.RetrieveContactByPN(text.account_phone_number_ID);
                    contacts.Add(_contact);
                    number_list.Add(text.account_phone_number_ID);
                }
            }
           
            return contacts;
        }
        public static async Task AddUserToDatabase(TeamMate newUser)
        {
            Console.WriteLine("Method Reached! " + newUser.Email);
            SHA512 sha = SHA512.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(newUser.Password));
            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            var cmd = new NpgsqlCommand("INSERT INTO public.\"Teammates\"(\"Name\", \"ID\", email, image_url, account_create_date, account_permission, password_hash) VALUES ('" + newUser.Name + "','" + newUser.ID + "','" + newUser.Email + "','" + newUser.ImageURL + "','" + newUser.AccountCreateDate + "','" + newUser.AccountPermission + "','" + hash + "')", con);
            cmd.ExecuteNonQuery();
            con.Close();
            Console.WriteLine("New User has joined the hub! " + newUser.Email);
            await Task.CompletedTask;

        }
        public static async Task AddContactToDatabase(Contacts newContact)
        {
            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            var cmd = new NpgsqlCommand("INSERT INTO public.\"Contacts\"(name, id, phone_number, email_address, tags_id, area_code, country_code, company, website, has_role_name) VALUES ('" + newContact.Name + "','" + newContact.ID + "','" + newContact.phone_number + "','" + newContact.email + "','" + newContact.tag_id + "','" + newContact.areacode + "','" + newContact.country_code + "','" + newContact.company + "','" + newContact.website + "','" + newContact.has_role_name + "')", con);
            cmd.ExecuteNonQuery();

            Console.WriteLine("New Contact added to the hub! " + newContact.phone_number);

            con.Close();
            await Task.CompletedTask;
        }
        public static async Task AddNumberToDatabase(PhoneNumber newNumber)
        {
            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            var cmd = new NpgsqlCommand("INSERT INTO public.\"AccountPhoneNumbers\"(phone_number, name, id, area_code, country_code, date_created) VALUES ('" + newNumber.phoneNumber + "','" + newNumber.Name + "','" + newNumber.ID + "','" + newNumber.Area_Code + "','" + newNumber.County_Code + "','" + newNumber.DateCreated + "')", con);
            cmd.ExecuteNonQuery();

            Console.WriteLine("New Number added to the hub! " + newNumber.phoneNumber);

            con.Close();
            await Task.CompletedTask;
        }
        public static async Task AddTextToDatabase(Texts newText)
        {
            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            var cmd = new NpgsqlCommand("INSERT INTO public.\"TextMessages\"(message, id, replying_to_msg_id, is_internal_msg, is_teammate, phone_num_id, contact_id, is_unread, media_url, teammate_id, date_received) VALUES ('" + newText.Message + "','" + newText.ID + "','" + newText.replying_to_msg_id + "','" + newText.is_internal_msg + "','" + newText.is_teammate + "','" + newText.account_phone_number_ID + "','" + newText.contact_id + "','" + newText.is_unread + "','" + newText.media_url + "','" + newText.teammate_ID + "','" + newText.date_received + "')", con);
            cmd.ExecuteNonQuery();

            Console.WriteLine("New Text Received! " + newText.Message);

            con.Close();
            await Task.CompletedTask;
        }
        public static async Task AddCallToDatabase(Calls newCall)
        {
            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            var cmd = new NpgsqlCommand("INSERT INTO public.\"PhoneCalls\"(call_started, id, call_ended, transcription, teammate_id, account_phone_num_id, contact_id, is_missed_call, recorded_audio_url) VALUES ('" + newCall.call_started + "','" + newCall.ID + "','" + newCall.call_ended + "','" + newCall.transcription + "','" + newCall.teammate_id + "','" + newCall.account_phone_number_ID + "','" + newCall.contact_id + "','" + newCall.is_missedcall + "','" + newCall.recording_audio_url + "')", con);
            cmd.ExecuteNonQuery();

            Console.WriteLine("New Call Received! " + newCall.contact_id);

            con.Close();
            await Task.CompletedTask;
        }
        public static async Task AddVoiceMailToDatabase(VoiceMails newVM)
        {
            using var con = new NpgsqlConnection(DBconnection);
            con.Open();
            var cmd = new NpgsqlCommand("INSERT INTO public.\"Voicemails\"(id, audio_url, contact_id, date_received, transcription, is_unread, account_phone_num_id) VALUES ('" + newVM.ID + "','" + newVM.audio_url + "','" + newVM.contact_ID + "','" + newVM.date_received + "','" + newVM.transcription + "','" + newVM.is_unread + "','" + newVM.account_phone_number_ID +"')", con);
            cmd.ExecuteNonQuery();

            Console.WriteLine("New Voicemail Received! " + newVM.contact_ID);

            con.Close();
            await Task.CompletedTask;
        }
      
        public static async Task SendSMS( string fromNumber, string toNumber, string textmessage)
        {
            var appId = "648cb905-8fe0-45ce-8800-bf8f9c735292";
            var privateKeyPath = @"C:\Certs\nexmo.key";
            var credentials = Credentials.FromAppIdAndPrivateKeyPath(appId, privateKeyPath);

            var vonageClient = new VonageClient(credentials);

            var response = vonageClient.SmsClient.SendAnSms(new Vonage.Messaging.SendSmsRequest()
            {
                To = toNumber,
                From = fromNumber,
                Text = textmessage
            });
            await Task.CompletedTask;
        }

        public static async Task<string> MakeOutBoundCall(string fromNumber, string toNumber)
        {
            var appId = "648cb905-8fe0-45ce-8800-bf8f9c735292";
            var privateKeyPath = @"C:\Certs\nexmo.key";
            var credentials = Credentials.FromAppIdAndPrivateKeyPath(appId, privateKeyPath);

            string teammate_id = ServerLibrary.RetrieveTeamMateID_byNumber(fromNumber);
            string _contact_id = ServerLibrary.CheckContactExists(toNumber);
           
            var toEndpoint = new PhoneEndpoint() { Number = toNumber };
            var fromEndpoint = new PhoneEndpoint() { Number = fromNumber };
            var headers = new Dictionary<string, string>();
            headers.Add("app", "audiosocket");
            headers.Add("caller-id", fromNumber);
            string CID = "";
            var client = new VonageClient(credentials);
            CID = ServerLibrary.Generate_IDKey();
            var wsEndpoint = new WebsocketEndpoint()
            {
                Headers = headers,
                ContentType = "audio/l16;rate=8000",
                Uri = $"wss://upwork.bifrost.inc/ws/cid="+CID
            };

         
            var command = new CallCommand() { To = new Endpoint[] { toEndpoint }, From=fromEndpoint, Ncco=new Ncco( new ConnectAction() { Endpoint = new Endpoint[] { wsEndpoint } }) };
            var response = client.VoiceClient.CreateCall(command);
            
            Calls newCall = null;
            if (_contact_id == "" | _contact_id == null)
            {
                Contacts newContact = new Contacts { phone_number = toNumber, ID = ServerLibrary.Generate_IDKey() };
                ServerLibrary.AddContactToDatabase(newContact).Wait();
                newCall = new Calls { ID = response.Uuid, teammate_id = teammate_id, contact_id = newContact.ID, account_phone_number_ID = toNumber, call_started = DateTime.Now };
                ServerLibrary.AddCallToDatabase(newCall).Wait();
            }
            else
            {
               newCall = new Calls { ID = response.Uuid, teammate_id = teammate_id, contact_id = _contact_id, account_phone_number_ID = toNumber, call_started = DateTime.Now };
               ServerLibrary.AddCallToDatabase(newCall).Wait();
            }

         

            Console.WriteLine(response.Status);
            await Task.CompletedTask;
            return CID;
        }
        public static string Generate_IDKey()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var keyBase = new char[12];
            var random = new Random();
            for (int i = 0; i < keyBase.Length; i++)
            {
                keyBase[i] = chars[random.Next(chars.Length)];
            }
            var key = new String(keyBase);
            
            return key;
        }
        public static async Task<AccountInfo> UpdateTexts(string teammate_ID)
        {
            return null;
        }

    }
    public class PhoneNumber
    {
        public string phoneNumber { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public int Area_Code { get; set; }
        public int County_Code { get; set; }
        public DateTime DateCreated { get; set; }

    }
    public class TeamMate
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ImageURL { get; set; }
        public DateTime AccountCreateDate { get; set; }
        public int AccountPermission { get; set; }
        public string Password { get; set; }
    }
    public class AccountInfo
    { 
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ImageURL { get; set; }
        public int AccountCreateDate { get; set; }
        public int AccountPermission  { get; set; }

        public List<Contacts> Contacts { get; set; }
        public List<Texts> Texts { get; set; }
        public List<Calls> Calls { get; set; }
        public List<VoiceMails> VoiceMails { get; set; }

        
        public AccountInfo(string _TMID, string _Name, string _Email, string _ImageURL, int _AccountCreateDate, int _AccountPermission)
        {
            ID = _TMID;
            Name = _Name;
            Email = _Email;
            ImageURL = _ImageURL;
            AccountCreateDate = _AccountCreateDate;
            AccountPermission = _AccountPermission;
        }
    }
    public class VonageCall
    {

        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string TargetPhone { get; private set; }
        public string Uuid { get; protected set; }
        public string ConversationUuid { get; private set; }

        public async Task Websocket(WebSocketContext context)
        {
         
            var websocket = context.WebSocket;

            try
            {
                var buffer = new byte[1024 * 4];

                async Task<bool> readWebsocket()
                {
                    var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (!result.CloseStatus.HasValue)
                    {
                        Console.WriteLine("Connected to Websocket!");
                        return true;
                    }
                    else
                    {
                        await websocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                        return false;
                    }
                }

                while (await readWebsocket()) { }
            }
            catch (WebSocketException)
            {
                Console.WriteLine($"WebSocket exception");
            }
        }
    }
    public class Contacts
    {
        public string ID;
        public string Name;
        public string phone_number;
        public string email;
        public string tag_id;
        public int areacode;
        public int country_code;
        public string company;
        public string website;
        public string has_role_name;
    }
    public class Texts 
    {

        public string ID;
        public string Message;
        public string replying_to_msg_id;
        public bool is_internal_msg;
        public bool is_teammate;
        public string account_phone_number_ID;
        public string contact_id;
        public bool is_unread;
        public string media_url;
        public string teammate_ID;
        public DateTime date_received;
    }
    public class Calls
    {
        public string ID;
        public DateTime call_started;
        public DateTime call_ended;
        public string teammate_id;
        public string transcription;
        public string contact_id;
        public bool is_missedcall;
        public string audio_url;
        public string account_phone_number_ID;
        public string recording_audio_url;
    }
    public class VoiceMails
    {
        public string ID;
        public string audio_url;
        public string contact_ID;
        public DateTime date_received;
        public string transcription;
        public bool is_unread;
        public string account_phone_number_ID;
    }
}
