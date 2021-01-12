using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics
{
    public class NodeAliasCache
    {
        private readonly LndClient client;
        private readonly Dictionary<string, string> nodeAliasCache;
        

        public NodeAliasCache(LndClient client)
        {
            this.client = client;
            this.nodeAliasCache = new Dictionary<string, string>();
        }

        public async Task RefreshOnlyIfNecessary(LnrpcListChannelsResponse listChannelsResponse, LnrpcPendingChannelsResponse pendingChannelsResponse)
        {
            var allPublicKeys = new List<string>();

            if (listChannelsResponse?.Channels != null)
            {
                allPublicKeys.AddRange(listChannelsResponse.Channels.Select(c => c.Remote_pubkey));
            }

            if (pendingChannelsResponse?.Pending_open_channels != null)
            {
                allPublicKeys.AddRange(pendingChannelsResponse.Pending_open_channels.Select(c => c.Channel.Remote_node_pub));
            }

            foreach (var publicKey in allPublicKeys)
            {
                if (this.nodeAliasCache.ContainsKey(publicKey))
                {
                    continue;
                }
                else
                {
                    var nodeInfo = await this.client.SwaggerClient.GetNodeInfoAsync(publicKey).ConfigureAwait(false);
                    this.nodeAliasCache.Add(nodeInfo.Node.Pub_key, nodeInfo.Node.Alias);
                }
            }
        }

        public string GetNodeAlias(string publicKey)
        {
            return this.nodeAliasCache[publicKey];
        }
    }
}
