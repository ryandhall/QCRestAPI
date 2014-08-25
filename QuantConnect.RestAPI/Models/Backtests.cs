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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QuantConnect.RestAPI.Models
{
    /// <summary>
    /// Compiler Packet:
    /// </summary>
    public class PacketBacktest : PacketBase
    {
        [JsonProperty(PropertyName = "backtestId")]
        public string BacktestId;

        public PacketBacktest()
        { }
    }


    /// <summary>
    /// Result Class for Backtest:
    /// </summary>
    public class PacketBacktestResult : PacketBase
    {
        [JsonProperty(PropertyName = "progress")]
        public string Progress = "";

        [JsonProperty(PropertyName = "processingTime")]
        public double ProcessingTime = 0;

        [JsonProperty(PropertyName = "results")]
        public BacktestResult Results = new BacktestResult();

        public PacketBacktestResult()
        { }
    }

    /// <summary>
    /// Result Container:
    /// </summary>
    public class BacktestResult
    {
        [JsonProperty(PropertyName = "Charts")]
        public Dictionary<string, Chart> Charts = new Dictionary<string, Chart>();

        [JsonProperty(PropertyName = "Statistics")]
        public Dictionary<string, string> Statistics = new Dictionary<string,string>();

        [JsonProperty(PropertyName = "Orders")]
        public Dictionary<int, Order> Orders = new Dictionary<int, Order>();

        [JsonProperty(PropertyName = "ProfitLoss")]
        public Dictionary<DateTime, decimal> ProfitLoss = new Dictionary<DateTime,decimal>();

        public BacktestResult() 
        { }
    }


    /// <summary>
    /// List of Backtest Results for this project:
    /// </summary>
    public class PacketBacktestList : PacketBase
    {
        [JsonProperty(PropertyName = "results")]
        public List<BacktestSummary> Summary;
    }

    /// <summary>
    /// Summary of a Backtest (no result data):
    /// </summary>
    public class BacktestSummary
    {
        [JsonProperty(PropertyName = "backtestId")]
        public string BacktestId;

        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "progress")]
        public double Progress;

        [JsonProperty(PropertyName = "processingTime")]
        public double ProcessingTime;

        [JsonProperty(PropertyName = "requested")]
        public DateTime Requested;

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate;

        [JsonProperty(PropertyName = "endDate")]
        public DateTime EndDate;

        [JsonProperty(PropertyName = "sparkline")]
        public string SparkLine;
    }
}
