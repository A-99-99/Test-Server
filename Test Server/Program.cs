using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Test_Server
{
    internal class Program
    {

        private static string clientID;
        private static string clientSecret;
        private static string token;


        static void Main(string[] keyValue)
        {


            Console.WriteLine("\n\t######################### Test Server #########################\n");

            Thread t = new Thread(testThread);
            t.Start();


            string status;
            if (checkServerStatus())
            {

                status = "OK";
            }
            else
            {

                status = "Srever Connection Error!";
            }

            Console.WriteLine($"\tServer Status: {status}\n");
            Console.WriteLine("\t#1: Create A Token.");
            Console.WriteLine("\t#2: Fetch a list of Request Types that a user has permission to create.");
            Console.WriteLine("\t#3: Fetch a list of GE’s that can be set as the originating organisation when submitting a Request.");
            Console.WriteLine("\t#4: Fetch the current LOV list mappings for constants used in Request submission.");
            Console.WriteLine("\t#5: Map LOV list values used by your system to The System’s default values.");
            Console.WriteLine("\t#6: Fetch the schema for a Request Type. The schema will detail the field names, type and cardinality.");
            Console.WriteLine("\t#7: Submit a Request.");
            Console.WriteLine("\t#8: Resubmit a Request.");
            Console.WriteLine("\t#9: Submit Feedback for a Request.");
            Console.WriteLine("\t#10: Press 0 to close the programe");
            Console.Write("\n\t* Choose An Option: ");

            var userInput = "";
            do
            {
                userInput = Console.ReadLine().ToString();


                if (userInput == "1")
                {

                    Console.Write("\n\tEnter Client ID: ");
                    clientID = Console.ReadLine().ToString();
                    Console.Write("\n\tEnter Client Secret: ");
                    clientSecret = Console.ReadLine().ToString();

                    CallServer("POST", "token_link");

                }
                if (token != "")
                {

                    if (userInput == "2")
                    {

                        CallServer("GET", "request/fetchCreateRequest");
                    }

                }
                else
                {

                    Console.WriteLine("\tPlease create the token first!");
                    continue;
                }


            } while (userInput != "0");







            //Console.WriteLine("The Key: "+userInput);


        }


        static void CallServer(string method, string uriExtention)
        {
            String url = "";

            if (uriExtention == "token_link")
            {

                url = $"http://localhost:8080/{uriExtention}/";
            }
            else if (uriExtention == "request/fetchCreateRequest")
            {

                url = $"http://localhost:8080/api/{uriExtention}";
            }


            if (method == "GET")
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                request.Timeout = Timeout.Infinite;
                request.KeepAlive = true;

                long length = 0;
                try
                {

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        length = response.ContentLength;
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        //string[] responseFromServer = reader.ReadToEnd().Split(':');
                        string responseFromServer = reader.ReadToEnd();
                        Console.WriteLine($"\n\tRespons of calling: {uriExtention}=> " + responseFromServer);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n\tGeneral Error=> " + ex.Message);
                }
            }
            else
            {


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/token_link");
                request.Method = method;
                request.ContentType = "application/raw";
                string postData = $"client_id={clientID}&client_secret={clientSecret}";

                byte[] postDataBytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = postDataBytes.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postDataBytes, 0, postDataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            string responseText = reader.ReadToEnd();
                            var deserialized = JsonConvert.DeserializeObject<RespObject>(responseText);
                            Console.WriteLine("\t Access Token: "+deserialized.accessToken()+"\t");
                            
                        }
                    }
                }



            }
        }
        class RespObject
        {
            public string access_token;
            public string token_lifetime;
            public string token_expiry_date;
            public string token_type;
            public string accessToken()            {
                return access_token;
            }

            public string tokenLifetime()
            {
                return token_lifetime;
            }
            public string tokenExpiryDate()
            {
                return token_expiry_date;
            }

            public string tokenType()
            {
                return token_type;
            }

        }
        static bool checkServerStatus()
        {

            string link = "http://localhost:8080/apiv/ping";

            HttpWebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(link);
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                return false;
            }

            return response.StatusDescription.ToString() == "OK" ? true : false;


        }


        static void testThread()
        {


            while (true)
            {

                if (!checkServerStatus())
                {

                    Console.WriteLine("\n\tServer is not healthy");
                    break;

                }
                Thread.Sleep(100);

            }



        }

    }


}
