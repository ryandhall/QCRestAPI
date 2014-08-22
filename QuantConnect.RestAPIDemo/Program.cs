/*
* QUANTCONNECT.COM - REST API
* C# Demonstration Test Application
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.RestAPI;
using QuantConnect.RestAPI.Models;
using System.Threading;

namespace QuantConnect.RestAPIDemo
{
    /// <summary>
    /// Our Test Application
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Init the new API: email, password.
            API api = new API("demo@quantconnect.com", "demo123");

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 0: Validate local copy of QCAlgorithm ");
            //Validate we have the latest QCAlgorithm Version:
            if (!api.CheckQCAlgorithmVersion())
            {
                Console.WriteLine("Local copy of QCAlgorithm obsolete");
                api.DownloadQCAlgorithm("QCAlgorithm");
            }


            Console.WriteLine("==========================================");
            Console.WriteLine("Test 1: Create Project: ");
            PacketCreateProject projectCreated = api.ProjectCreate("New Test Project");
            Console.WriteLine("New Project Id: " + projectCreated.ProjectId);
            int newProjectId = projectCreated.ProjectId;

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 2: List Projects: ");
            PacketProject projects = api.ProjectList();
            foreach (Project project in projects.Projects)
            {
                Console.WriteLine("Name: " + project.Name + " Id: " + project.Id + " Date: " + project.Modified.ToLongTimeString());
            }

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 3: Update Our New Project: (Set new project as weather rebalancing strategy)");
            List<File> files = new List<File>();
            files.Add(new File("Main.cs",
                "using System;\r\nusing System.IO;\r\nusing System.Linq;\r\nusing System.Collections;\r\nusing System.Collections.Generic; \r\nusing QuantConnect.Securities;  \r\nusing QuantConnect.Models;    \r\n\r\nnamespace QuantConnect {  \r\n    \r\n    public partial class QCUWeatherBasedRebalancing : QCAlgorithm \r\n    {\r\n        //Initialize: Storage for our custom data:\r\n        //Source: http://www.wunderground.com/history/\r\n        //Make sure to link to the actual file download URL if using dropbox.\r\n        //private string url = \"https://www.dropbox.com/s/txgqzv2vp5lzpqc/10065.csv\";\r\n        private string url = \"https://www.dropbox.com/s/txgqzv2vp5lzpqc/10065.csv?dl=1\";\r\n        private List<Weather> WeatherData = new List<Weather>();\r\n        private int rebalanceFrequency = 2, tradingDayCount = 0; \r\n        //private TradeBar previousBar = new TradeBar();\r\n        \r\n        ///<summary>\r\n        /// Initialize our algorithm:\r\n        ///</summary>\r\n        public override void Initialize()\r\n        {\r\n            SetStartDate(2013, 1, 1);          \r\n            SetEndDate(DateTime.Now.Date.AddDays(-1)); \r\n            SetCash(25000);\r\n            AddSecurity(SecurityType.Equity, \"SPY\", Resolution.Minute);\r\n        }\r\n        \r\n        ///<summary>\r\n        /// When we have a new event trigger, buy some stock:\r\n        ///</summary>\r\n        public override void OnTradeBar(Dictionary<string, TradeBar> data) \r\n        {   \r\n            TradeBar bar = data[\"SPY\"];\r\n            \r\n            //If weather isn't initialized yet:\r\n            if (WeatherData.Count == 0) WeatherData = GenerateFromURL(url);\r\n            \r\n            //Rebalance every 10 days:\r\n            if (tradingDayCount == rebalanceFrequency) {\r\n                \r\n                //Scan Weather Time stamps for date *after* today --> we don't know final temperatures until next day:\r\n                Weather yesterday = (from day in WeatherData\r\n                                     where day.Time.Date == Time.Date.AddDays(-1)\r\n                                     select day).FirstOrDefault();\r\n                \r\n                //Scale from -5C to +25C :: -5C == 0, +25C = 100% invested.\r\n                if (yesterday != null) {\r\n                    decimal fraction = (yesterday.MinC + 5m) / 30m;\r\n                    SetHoldings(\"SPY\", fraction);\r\n                    tradingDayCount = 0;\r\n                } \r\n            }\r\n        }\r\n        \r\n        ///<summary>\r\n        /// After each trading day\r\n        ///</summary>\r\n        public override void OnEndOfDay() {\r\n            tradingDayCount++;\r\n        }\r\n                \r\n        ///<summary>\r\n        /// When we have a new event trigger, buy some stock:\r\n        ///</summary>\r\n        public List<Weather> GenerateFromURL(string url) {\r\n            List<Weather> events = new List<Weather>();\r\n            byte[] rawData = null;\r\n            string line;\r\n            using (var wc = new System.Net.WebClient()) rawData = wc.DownloadData(url);\r\n            MemoryStream stream = new MemoryStream(rawData);\r\n            using (StreamReader sr = new StreamReader(stream)) {\r\n                sr.ReadLine();\r\n                while ((line = sr.ReadLine()) != null) {\r\n                    Weather w = new Weather(line);\r\n                    events.Add(w);\r\n                    if (w.errString != \"\") Debug(\"Weather Err:\" + w.errString);\r\n                }\r\n            }\r\n            return events;\r\n        }\r\n    }\r\n    \r\n    ///<summary>\r\n    /// Storage and creation for 1 event object:\r\n    ///</summary>\r\n    public class Weather {\r\n        public DateTime Time = new DateTime();\r\n        public decimal MaxC = 0;\r\n        public decimal MeanC = 0;\r\n        public decimal MinC = 0;\r\n        public string errString = \"\";\r\n        \r\n        public Weather(string csv) {\r\n            try {\r\n                string[] data = csv.Split(',');\r\n                this.Time = DateTime.Parse(data[0]);\r\n                this.MaxC = Convert.ToDecimal(data[1]);\r\n                this.MeanC = Convert.ToDecimal(data[2]);\r\n                this.MinC = Convert.ToDecimal(data[3]);\r\n            } catch (Exception err) {\r\n                //Error converting.\r\n                errString = err.Message + \"::> \" + csv;\r\n            }\r\n        }\r\n    }\r\n}"
            ));
            PacketBase updateSuccess = api.ProjectUpdate(newProjectId, files);
            Console.WriteLine("Updated project: " + updateSuccess.Success);

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 4: List Project Contents:");
            var projectFiles = api.ProjectFiles(newProjectId);
            foreach (File file in projectFiles.Files)
            {
                Console.WriteLine("File Name: " + file.Name + " Contents: " + file.Code);
            }

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 5: Compile Project:");
            PacketCompile compileResult = api.Compile(newProjectId); 
            Console.WriteLine("CompileId: " + compileResult.CompileId); 
            foreach (CompileLog entry in compileResult.Logs)
            {
                Console.WriteLine("Compile Result : Time: " + entry.Time + " Type: " + entry.Type.ToString()  + " Entry: " + entry.Entry);
            }

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 6: Backtest Compiled Project:");
            PacketBacktest backtestResult = api.Backtest(newProjectId, compileResult.CompileId, "New Random Name!");
            Console.WriteLine("SimulationId: " + backtestResult.BacktestId);

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 7: Reading Backtest Results:");
            PacketBacktestResult readResult = api.BacktestResults(backtestResult.BacktestId);
            foreach (Chart chart in readResult.Results.Charts.Values)
            {
                Console.WriteLine("Result Chart Name: " + chart.Name);
            }

            Console.WriteLine("==========================================");
            Console.WriteLine("Test 8: Delete Project: ");
            PacketBase deleteSuccess = api.ProjectDelete(newProjectId);
            Console.WriteLine("Deleted project: " + deleteSuccess.Success.ToString());

            Console.ReadKey();
        }

    }
}
