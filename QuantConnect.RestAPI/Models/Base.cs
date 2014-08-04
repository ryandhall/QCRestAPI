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
    /// Common base packet for interfacing with the QC API: All packets return with Success and Errors Array:
    /// </summary>
    public class PacketBase
    {
        /// <summary>
        /// Successful Request
        /// </summary>
        [JsonProperty(PropertyName = "success")]
        public bool Success;

        /// <summary>
        /// Errors string array
        /// </summary>
        [JsonProperty(PropertyName = "errors")]
        public List<string> Errors;

        /// <summary>
        /// Your IP
        /// </summary>
        [JsonProperty(PropertyName = "ip")]
        public string Ip;
    }
}
