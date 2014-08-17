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
}
