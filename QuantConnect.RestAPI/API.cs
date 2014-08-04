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
        /// Manage Rate Limiting:
        private DateTime _previousRequest = new DateTime();
        /// Set the rate limit maximum:
        private TimeSpan _rateLimit = TimeSpan.FromSeconds(3);


        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise API Manager:
        /// </summary>
        /// <param name="email">QuantConnect user email</param>
        /// <param name="password">QuantConnect user password</param>
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

            //Wait for the API rate limiting
            while (DateTime.Now < (_previousRequest + _rateLimit)) Thread.Sleep(1);
            _previousRequest = DateTime.Now;

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

            if (response.Errors.Count == 0)
            {
                return true;
            }

            return loggedIn;
        }

        /// <summary>
        /// Create a new project in your QC account.
        /// </summary>
        /// <param name="projectName">Name of the new project</param>
        /// <returns>Project Id.</returns>
        public int ProjectCreate(string name)
        {
            int id = -1;
            try 
            {
                var request = new RestRequest("projects/create", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectName = name }), ParameterType.RequestBody);
                var response = Execute<PacketCreateProject>(request);
                id = response.ProjectId;
            }   
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.ProjectCreate(): " + err.Message);
            }

            return id;
        }


        /// <summary>
        /// Update a project with a list of C# files:
        /// </summary>
        /// <param name="id">Project Id</param>
        /// <param name="filesData">List of files data and names</param>
        public bool ProjectUpdate(int id, List<File> filesData)
        {
            bool success = false;
            try
            {
                var request = new RestRequest("projects/update", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = id, files = filesData }), ParameterType.RequestBody);
                var response = Execute<PacketBase>(request);
                success = response.Success;
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.ProjectCreate(): " + err.Message);
            }

            return success;
        }

        /// <summary>
        /// Return a list of QuantConnect Projects
        /// </summary>
        /// <returns>List of QuantConnect Project objects</returns>
        public List<Project> ProjectList()
        {
            List<Project> projects = new List<Project>();
            try
            {
                var request = new RestRequest("projects/read", Method.POST);
                var response = Execute<PacketProject>(request);
                projects = response.Projects;
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.Projects(): " + err.Message);
            }
            return projects;
        }


        /// <summary>
        /// Get a list of project files in this project
        /// </summary>
        /// <param name="id">ProjectID</param>
        /// <returns>List of QuantConnect File objects</returns>
        public List<File> ProjectFiles(int id)
        {
            List<File> files = new List<File>();

            try
            {
                var request = new RestRequest("projects/read", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = id }), ParameterType.RequestBody);

                var response = Execute<PacketProjectFiles>(request);
                files = response.Files;
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.API.ProjectFiles(): " + err.Message);
            }

            return files;
        }


        /// <summary>
        /// Delete a project by id:
        /// </summary>
        /// <param name="id">Project Id</param>
        public bool ProjectDelete(int id)
        {
            bool success = false;
            try
            {
                var request = new RestRequest("projects/delete", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { projectId = id }), ParameterType.RequestBody);
                var response = Execute<PacketBase>(request);
                success = response.Success;
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.RestAPI.Projects(): " + err.Message);
            }
            return success;
        }


        /// <summary>
        /// Send a compile request:
        /// </summary>
        /// <param name="id">Compile ID</param>
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
                Console.WriteLine("QuantConnect.Compile(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// Submit a compile and project id for backtesting.
        /// </summary>
        /// <param name="projectId">Project Id for QuantConnect</param>
        /// <param name="compileId">Successful compile id</param>
        /// <param name="backtestName">Name for your backtest</param>
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
                Console.WriteLine("QuantConnect.Compile(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// Read this simulation result back:
        /// </summary>
        /// <param name="simulationId">SimulationId we own</param>
        public PacketBacktestResult BacktestResults(string simulationId)
        {
            PacketBacktestResult packet = new PacketBacktestResult();
            try
            {
                var request = new RestRequest("backtests/read", Method.POST);
                request.AddParameter("application/json", JsonConvert.SerializeObject(new { simulationId = simulationId }), ParameterType.RequestBody);
                packet = Execute<PacketBacktestResult>(request);
            }
            catch (Exception err)
            {
                Console.WriteLine("QuantConnect.Compile(): " + err.Message);
            }
            return packet;
        }


        /// <summary>
        /// B64 Encoder 
        /// </summary>
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
