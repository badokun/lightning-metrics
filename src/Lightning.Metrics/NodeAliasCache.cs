using System.Collections.Generic;
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

        public async Task Refresh(LnrpcListChannelsResponse listChannelsResponse)
        {
            if (listChannelsResponse?.Channels != null)
            {
                foreach (var channel in listChannelsResponse.Channels)
                {
                    if (this.nodeAliasCache.ContainsKey(channel.Remote_pubkey))
                    {
                        continue;
                    }
                    else
                    {
                        var nodeInfo = await this.client.SwaggerClient.GetNodeInfoAsync(channel.Remote_pubkey).ConfigureAwait(false);
                        this.nodeAliasCache.Add(nodeInfo.Node.Pub_key, nodeInfo.Node.Alias);
                    }
                }
            }
        }

        public string GetNodeAlias(string pub_key)
        {
            return this.nodeAliasCache[pub_key];
        }
    }
}
