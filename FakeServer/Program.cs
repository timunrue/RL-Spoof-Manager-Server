using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FakeServer
{
    class Program
    {
        private static String serverIpAddress = "127.0.0.1";

        static void Main(string[] args)
        {
            if (args == null)
            {
                Console.WriteLine("No IP specified, Redirecting RL to 127.0.0.1"); // Check for null array
            }
            else
            {
                if (args.Length >= 1)
                {
                    serverIpAddress = args[0];
                } else
                {
                    Console.WriteLine("Enter the IP Address of your Dedicated Server: ");
                    serverIpAddress = Console.ReadLine();
                }
            }
            Console.WriteLine("Dedicated Server IP address set to " + serverIpAddress + ".\n");

            TcpListener serverSocket = new TcpListener(7778);
            int requestCount = 0;
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            while ((true))
            {

                Console.WriteLine(" >> RL Manager Server Started");
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine(" >> Accept connection from client");
                requestCount = 0;

                while ((true))
                {
                    try
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = clientSocket.GetStream();
                        byte[] bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];
                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        string dataFromClient = System.Text.Encoding.UTF8.GetString(bytesFrom);
                        //ProjectX.AddReservationMessagePublic_X
                        if (dataFromClient.Contains("{"))
                        {
                            Console.WriteLine(" >> Join request from " + clientSocket.Client.RemoteEndPoint.ToString());
                            Byte[] sendBytes = getManagerData(dataFromClient);
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            networkStream.Flush();
                            Console.WriteLine(" >> Sent reservation, Client Disconnected.");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        break;
                    }
                }

                clientSocket.Close();
            }
            serverSocket.Stop();
            Console.WriteLine(" >> exit");
            Console.ReadLine();
        }

        private static byte[] getManagerData(String request)
        {
            //Parse out items from request
            if (request.Contains("ProjectX.AddReservationMessagePublic_X"))
            {
                String jsonToParse = request.Substring(request.IndexOf('{'));
                Console.WriteLine(jsonToParse);
                Rootobject ro = JsonConvert.DeserializeObject<Rootobject>(jsonToParse);
                String items = "[" + string.Join(",", ro.Players[0].Loadout) + "]";

                String payloadAscii = "#ProjectX.ReservationsReadyMessage_X{\"ServerAddress\":\"" + serverIpAddress + "%3a7777\",\"ProductIDs\":" + items + "}";
                String payload = "000000" + stringToHex(payloadAscii);
                payload = "000000" + (payload.Length / 2).ToString("X") + payload;
                return StringToByteArray(payload);
            }
            else
            {
                return new byte[0];
            }

        }

        public static byte[] StringToByteArray(string hex)
        {
            try
            {
                return Enumerable.Range(0, hex.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                 .ToArray();

            }
            catch
            {
                return new byte[0];
            }
        }

        private static String stringToHex(String strin)
        {
            byte[] ba = Encoding.Default.GetBytes(strin);
            string hexString = BitConverter.ToString(ba);
            return hexString.Replace("-", "").ToLower();
        }
    }


    public class Rootobject
    {
        public bool bDisableCrossPlay { get; set; }
        public Settings Settings { get; set; }
        public Player[] Players { get; set; }
        public Partyleaderid PartyLeaderID { get; set; }
    }

    public class Settings
    {
        public int MatchType { get; set; }
        public int PlaylistId { get; set; }
        public bool bFriendJoin { get; set; }
        public bool bMigration { get; set; }
        public bool bRankedReconnect { get; set; }
        public string Password { get; set; }
    }

    public class Partyleaderid
    {
        public long Uid { get; set; }
        public int Platform { get; set; }
        public int SplitscreenID { get; set; }
        public Npid NpId { get; set; }
    }

    public class Npid
    {
        public string Handle { get; set; }
        public int Opt { get; set; }
        public int Reserved { get; set; }
    }

    public class Player
    {
        public string PlayerName { get; set; }
        public float SkillMu { get; set; }
        public float SkillSigma { get; set; }
        public int Tier { get; set; }
        public bool bRemotePlayer { get; set; }
        public Playerid PlayerID { get; set; }
        public int[] Loadout { get; set; }
    }

    public class Playerid
    {
        public long Uid { get; set; }
        public int Platform { get; set; }
        public int SplitscreenID { get; set; }
        public Npid1 NpId { get; set; }
    }

    public class Npid1
    {
        public string Handle { get; set; }
        public int Opt { get; set; }
        public int Reserved { get; set; }
    }

}
