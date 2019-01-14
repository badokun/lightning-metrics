# lightning-metrics


## Overview

This application will query a Lightning Node ([LND Rest API](https://api.lightning.community/rest/index.html)) and push all metrics into an InfluxDB which can be 
used as a data source for Grafana Dashboards similar to the popular [Telegraf](https://github.com/influxdata/telegraf) agent. 

The [RaspiBolt](https://github.com/badokun/guides/blob/master/raspibolt/README.md) project served as motivation for setting this up in particular
[Bonus guide: Performance Monitoring](https://github.com/badokun/guides/blob/master/raspibolt/raspibolt_71_monitoring.md)

If you're looking to run this on a RaspiBolt [click here](RaspiBolt.md)

## Metrics

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
no-macaroons=true
```

The `tlsextraip` is required if you plan on running the application on different machine to where the [Lightning Network Daemon](https://github.com/lightningnetwork/lnd) ️is running. To simplify configuration
the `no-macaroons` option should be set to `true`.

## Usage

> Your Lightning Wallet needs to be unlocked for the LND REST API to return any data.

### Command line

`dotnet lnd-metrics.dll --influxDbUri http://192.168.1.40:8086 --network testnet --lndRestApiUri https://192.168.1.40:8080`

To view all the options run

`dotnet lnd-metrics.dll --help` 

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
