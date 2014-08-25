/*
* QUANTCONNECT.COM - REST API
* C# Wrapper for Restful API for Managing QuantConnect Connection
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.RestAPI.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Threading;
using Newtonsoft.Json;
using System.Net;
using ICSharpCode;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace QuantConnect.RestAPI
{
    /// <summary>
    /// Primary API Class
    /// </summary>
    public class API
    {

        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// HTTPS Endpoint:
        private string _endPoint = "https://www.quantconnect.com/api/v1";
        /// HTTP Basic Authentication Method: Username is Email
        private string _email = "";
        /// HTTP Basic Authentication Method: QC. Password
        private string _password = "";
        /// Encoded token username password:
        private string _accessToken = "";
        /// GitHub SHA for QCAlgorithm:
        private string _gitHubHash = "";
        /// Location of saved hash file:
        private string _hashFile = "hash.dat";

        /// <summary>
        /// Public property for the GitHub QCAlgorithm Hash.
        /// </summary>
        public string QCAlgorithmSHA
        {
            get
            {
                if (_gitHubHash == "")
                {
                    if (System.IO.File.Exists(_hashFile))
                    {
                        _gitHubHash = System.IO.File.ReadAllText(_hashFile);
                    }
                }
                return _gitHubHash;
            }
            set
            {
                _gitHubHash = value;
            }
        }


        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise API Manager:
        /// </summary>
        public API(string email, string password)
        {
            Authenticate(email, password);
        }


        /// <summary>
        /// Execute a authenticated call:
        /// </summary>
        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient(_endPoint);
            client.AddDefaultHeader("Accept", "application/json");
            client.AddDefaultHeader("Content-Type", "application/json");
            client.AddDefaultHeader("Authorization", "Basic " + _accessToken);

            //Send the request:
            var raw = client.Execute(request);
            T response = JsonConvert.DeserializeObject<T>(raw.Content);

            if (raw.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                throw new ApplicationException(message, raw.ErrorException);
            }
            return response;
        }


        /// <summary>
        /// Test these authentication details against the server:
        /// </summary>
        /// <param name="email">User email from quantconnect account</param>
        /// <param name="password">Quantconnect user password</param>
        public bool Authenticate(string email, string password)
        {
            bool loggedIn = false;

            _email = email;
            _password = password;
            _accessToken = Base64Encode(email + ":" + password);

            var request = new RestRequest("projects/read", Method.POST);
            var response = Execute<PacketProject>(request);

            if (response.Errors == null || response.Errors.Count == 0)
            {
                return true;
            }

            return loggedIn;
        }


        /// <summary>
        /// Check if the current version of QCAlgorithm is obsolete.
        /// </summary>
        /// <param name="hashId">HashId of last commit</param>
        /// <returns>bool true/false obsolete</returns>
        public bool CheckQCAlgorithmVersion(string hashId = "")
        {
            bool valid = false;
            try
            {
                if (hashId == "")
                {
                    hashId = QCAlgorithmSHA;
                }

                string latestSHA = GetLatestQCAlgorithmSHA();

                if (latestSHA == hashId)
                {
                    valid = true;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.CheckQCAlgorithmVersion(): " + err.Message);
            }
            return valid;
        }


        /// <summary>
        /// Get the latest QCAlgorithm SHA Hash from GitHub.
        /// </summary>
        /// <returns></returns>
        public string GetLatestQCAlgorithmSHA()
        {
            string sha = "";
            try
            {
                var client = new RestClient(@"https://api.github.com/");
                var request = new RestRequest("repos/QuantConnect/QCAlgorithm/commits", Method.GET);
                request.AddHeader("Accept", "application/vnd.github.v3+json");
                var response = client.Execute(request);
                var decoded = JsonConvert.DeserializeObject<List<PacketGitHubCommit>>(response.Content);
                sha = decoded[0].SHA;
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.GetLatestQCAlgorithmSHA(): " + err.Message);
            }
            return sha;
        }


        /// <summary>
        /// Download and unzip the latest QCAlgorithm to a local folder:
        /// </summary>
        /// <param name="destination"></param>
        public void DownloadQCAlgorithm(string directory, bool async = false, DownloadProgressChangedEventHandler callbackProgress = null, System.ComponentModel.AsyncCompletedEventHandler callbackCompleted = null)
        {
            try
            {
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                string file = directory + @"\" + "QCAlgorithm.zip";

                using (var client = new WebClient())
                {
                    if (async)
                    {
                        client.DownloadFileAsync(new Uri(@"https://github.com/QuantConnect/QCAlgorithm/archive/master.zip"), file);
                        if (callbackProgress != null) client.DownloadProgressChanged += callbackProgress;
                        if (callbackCompleted != null) client.DownloadFileCompleted += callbackCompleted;
                    }
                    else
                    {
                        client.DownloadFile(@"https://github.com/QuantConnect/QCAlgorithm/archive/master.zip", file);
                    }
                }

                ZipFile zf = null;
                try {
                    System.IO.FileStream fs = System.IO.File.OpenRead(file);
                    zf = new ZipFile(fs);
                    
                    foreach (ZipEntry zipEntry in zf) {
                        if (!zipEntry.IsFile) {
                            continue;           // Ignore directories
                        }
                        String entryFileName = zipEntry.Name;
                        byte[] buffer = new byte[4096];
                        System.IO.Stream zipStream = zf.GetInputStream(zipEntry);

                        // Manipulate the output filename here as desired.
                        String fullZipToPath = System.IO.Path.Combine(directory, entryFileName);
                        string directoryName = System.IO.Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            System.IO.Directory.CreateDirectory(directoryName);

                        using (System.IO.FileStream streamWriter = System.IO.File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                } finally {
                    if (zf != null) {
                        zf.IsStreamOwner = true;
                        zf.Close();
                    }
                }

                //Save the new Hash ID to the disk:
                System.IO.File.WriteAllText(_hashFile, GetLatestQCAlgorithmSHA());
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.DownloadQCAlgorithm(): " + err.Message);
            }
        }


        /// <summary>
        /// Create a new project in your QC account.
        /// </summary>
        /// <param name="projectName">Name of the new project</param>
        /// <returns>Project Id.</returns>
        public PacketCreateProject ProjectCreate(string name)
        {
            var response = new PacketCreateProject();
            try 
            {
                var request = new RestRequest("projects/create", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectName = name }), ParameterType.RequestBody);
                response = Execute<PacketCreateProject>(request);
            }   
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.ProjectCreate(): " + err.Message);
            }
            return response;
        }


        /// <summary>
        /// Update a project with a list of C# files:
        /// </summary>
        public PacketBase ProjectUpdate(int id, List<File> filesData)
        {
            var response = new PacketBase();
            try
            {
                var request = new RestRequest("projects/update", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = id, files = filesData }), ParameterType.RequestBody);
                response = Execute<PacketBase>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.ProjectCreate(): " + err.Message);
            }

            return response;
        }

        /// <summary>
        /// Return a list of QuantConnect Projects
        /// </summary>
        /// <returns></returns>
        public PacketProject ProjectList()
        {
            var projects = new PacketProject();
            try
            {
                var request = new RestRequest("projects/read", Method.POST);
                projects = Execute<PacketProject>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.ProjectList(): " + err.Message);
            }
            return projects;
        }


        /// <summary>
        /// Get a list of project files in this project
        /// </summary>
        public PacketProjectFiles ProjectFiles(int id)
        {
            var response = new PacketProjectFiles();

            try
            {
                var request = new RestRequest("projects/read", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = id }), ParameterType.RequestBody);

                response = Execute<PacketProjectFiles>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.ProjectFiles(): " + err.Message);
            }

            return response;
        }


        /// <summary>
        /// Delete a project by id:
        /// </summary>
        public PacketBase ProjectDelete(int id)
        {
            var response = new PacketBase();
            try
            {
                var request = new RestRequest("projects/delete", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = id }), ParameterType.RequestBody);
                response = Execute<PacketBase>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.ProjectDelete(): " + err.Message);
            }
            return response;
        }


        /// <summary>
        /// Send a compile request:
        /// </summary>
        public PacketCompile Compile(int id)
        {
            PacketCompile packet = new PacketCompile();
            try
            {
                var request = new RestRequest("compiler/create", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = id }), ParameterType.RequestBody);
                packet = Execute<PacketCompile>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.Compile(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// Submit a compile and project id for backtesting.
        /// </summary>
        public PacketBacktest Backtest(int projectId, string compileId, string backtestName)
        {
            PacketBacktest packet = new PacketBacktest();
            try
            {
                var request = new RestRequest("backtests/create", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = projectId, compileId = compileId, backtestName = backtestName }), ParameterType.RequestBody);
                packet = Execute<PacketBacktest>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.Backtest(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// Read this backtest result back:
        /// </summary>
        public PacketBacktestResult BacktestResults(string backtestId)
        {
            PacketBacktestResult packet = new PacketBacktestResult();
            try
            {
                var request = new RestRequest("backtests/read", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { backtestId = backtestId }), ParameterType.RequestBody);
                packet = Execute<PacketBacktestResult>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.BacktestResults(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// Delete the given backtest Id
        /// </summary>
        /// <param name="backtestId">Id we want to delete</param>
        /// <returns>Packet success, fail or errors</returns>
        public PacketBase BacktestDelete(string backtestId)
        {
            PacketBase packet = new PacketBase();
            try
            {
                var request = new RestRequest("backtests/delete", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { backtestId = backtestId }), ParameterType.RequestBody);
                packet = Execute<PacketBase>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.BacktestDelete(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// Get a list of backtest results for this project:
        /// </summary>
        public PacketBacktestList BacktestList(int projectId)
        {
            var packet = new PacketBacktestList();
            try
            {
                var request = new RestRequest("backtests/list", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = projectId }), ParameterType.RequestBody);
                packet = Execute<PacketBacktestList>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.BacktestList(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// B64 Encoder 
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// B64 Decoder:
        /// </summary>
        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
