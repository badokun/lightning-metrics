# lightning-metrics


## Overview

This application will query a Lightning Node ([LND Rest API](https://api.lightning.community/rest/index.html)) and push all metrics into an InfluxDB which can be 
used as a data source for Grafana Dashboards similar to the popular [Telegraf](https://github.com/influxdata/telegraf) agent. 

The [RaspiBolt](https://github.com/Stadicus/guides/blob/master/raspibolt/README.md) project served as motivation for setting this up in particular

If you're looking to run this on a RaspiBolt [click here](docs/RaspiBolt.md)

## Metrics

* list_channels
  * tags:
    * host
	* remote_pubkey
	* channel_point
  * fields:
    * capacity
	* local_balance
	* remote_balance
	* unsettled_balance
	* total_satoshis_received
	* total_satoshis_sent
* pending_open_channels
  * tags:
    * host
    * remote_node_pub
    * channel_point
  * fields:
    * capacity
    * local_balance
    * remote_balance
    * commit_fee
    * commit_weight
    * fee_per_kw
* forced_closed_channels
  * tags:
    * host
	* remote_node_pub
  * fields:
    * capacity
	* remote_balance
	* local_balance
	* blocks_til_maturity
	* limbo_balance
	* maturity_height
	* recovered_balance
* pending_htlcs (as child of forced_closed_channels)
  * tags:
    * closing_txid
  * fields:
    * Amount
	* Stage
	* Outpoint
	* Blocks_til_maturity
	* Maturity_height
* channel_balance
  * tags:
    * host
  * fields:
    * balance
    * pending_open_balance
* balance
  * tags:
    * host
  * fields:
    * confirmed_balance
    * total_balance
    * unconfirmed_balance
* networkinfo
  * tags:
    * host
  * fields:
    * max_channel_size
    * min_channel_size
    * total_network_capacity
    * avg_channel_size
    * avg_out_degree
    * num_channels
    * num_nodes
    * graph_diameter
    * max_out_degree

## Configuration

The application is compiled as `lnd-metrics.dll` and uses the [LND Rest API](https://api.lightning.community/rest/index.html) which 
requires the following configuration. 

```
[Application Options]
tlsextraip=0.0.0.0
restlisten=0.0.0.0:8080
```

The `tlsextraip` is required if you plan on running the application on different machine to where the [Lightning Network Daemon](https://github.com/lightningnetwork/lnd) ️is running. 
> When adding the `tlsextraip` setting you may need to regenerate the tls.cert, tls.key and macaroon files. To test it's all working access the `/v1/getinfo` endpoint, e.g.  https://192.168.1.40:8080/v1/getinfo. You should see `{"error":"expected 1 macaroon, got 0","code":2}` as the response.

## Usage

> Your Lightning Wallet needs to be unlocked for the LND REST API to return any data.

### Command line

```
dotnet lnd-metrics.dll --influxDbUri http://192.168.1.40:8086 --network testnet --lndRestApiUri https://192.168.1.40:8080 --certThumbprintHex BC:C5:19:59:81:F0:2B:F3:FF:9B:72:56:C8:EA:16:F5:5B:72:91:3D:BB:2C:13:7C:C6:6F:02:C3:B7:06:FA:9B --macaroonHex 0201036c6e6402bb01030a1063f62ef05f61ef36510951303f92bb381201301a160a0761646472657373120472656164120577726974651a130a04696e666f120472656164120577726974651a170a08696e766f69636573120472656164120577726974651a160a076d657373616765120472656164120577726974651a170a086f6666636861696e120472656164120577726974651a160a076f6e636861696e120472656164120577726974651a140a05706565727312047265616412057772697465034026200cdb471a6d41d1223de71920581cfa21a3e1c308f4e20e83068c1a200141e2bd
```

To view all the options run

`dotnet lnd-metrics.dll --help` 

#### macaroonHex - Extracing the admin.macaroon hex string

On a Linux machine execute at the location where your macaroon files are, e.g. for testnet `/home/bitcoin/.lnd/data/chain/bitcoin/testnet`
```
xxd -p admin.macaroon | tr -d '\n' && echo " "
```

#### certThumbprintHex - Extracting the certificate thumbprint

On a Linux machine execute at the location where you certificate files are, e.g. `/home/bitcoin/.lnd`
```
 openssl x509 -noout -fingerprint -sha256 -inform pem -in tls.cert
```

### Docker

#### On Windows or Linux
`docker run badokun/lnd-metrics:latest --help`

#### On RaspBerry Pi
`docker run badokun/lnd-metrics:arm32 --help`

## Development

### Docker

#### Building a Raspberry compatible image on a Windows or Linux machine

```
docker build -t lnd-metrics:arm32 -f arm32.generic.Dockerfile .
docker tag lnd-metrics:arm32 badokun/lnd-metrics:arm32
docker push badokun/lnd-metrics:arm32
```

#### Building a Raspberry compatible image on a Raspberry

```
docker build -t lnd-metrics:arm32 -f arm32.on.raspberry.Dockerfile .
docker tag lnd-metrics:arm32 badokun/lnd-metrics:arm32
docker push badokun/lnd-metrics:arm32
```

#### Building a generic image on Windows or Linux

```
docker build -t lnd-metrics:latest  -f Dockerfile .
docker tag lnd-metrics:latest badokun/lnd-metrics:latest
docker push badokun/lnd-metrics:latest
```

### Release Management

- Bump the release version in Lightning.Metrics.App.csproj
- Run `powershell ./publish-docker.ps1` which will create git tags and push to GitHub.
- Docker images are automatically built

### Testnet

Get some free testnet bitcoins at https://lnroute.com/testnet-faucets/

## Resources

* [Lnd Rest Api](https://api.lightning.community/rest/index.html)
* Setting the `tlsextraip` to `0.0.0.0` was [suggested here](https://github.com/lightningnetwork/lnd/issues/1567#issuecomment-437665324)
* Lnd configuration [reference](https://github.com/lightningnetwork/lnd/blob/master/sample-lnd.conf)
* [RaspiBolt Guide](https://github.com/badokun/guides/tree/master/raspibolt)
