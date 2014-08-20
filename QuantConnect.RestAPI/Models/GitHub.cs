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
    public class PacketGitHubCommitContainer
    {
        List<PacketGitHubCommit> Commits;
    }


    /// <summary>
    /// Github Packet:
    /// </summary>
    public class PacketGitHubCommit : PacketBase
    {
        [JsonProperty(PropertyName = "sha")]
        public string SHA;

        /// <summary>
        public PacketGitHubCommit()
        { }
    }
}
