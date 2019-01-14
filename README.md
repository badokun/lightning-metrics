# lightning-metrics


## Overview

This application will query a lightning node and push all metrics into an InfluxDB which can be used as a data source for Grafana Dashboards. 
The [RaspiBolt](https://github.com/badokun/guides/blob/master/raspibolt/README.md) project served as motivation for setting this up, and in particular
[Bonus guide: Performance Monitoring](https://github.com/badokun/guides/blob/master/raspibolt/raspibolt_71_monitoring.md). 

## Configuration

Lightning-Metrics uses the [LND Rest API](https://api.lightning.community/rest/index.html) which requires the following configuration. On a RaspiBolt setup you will find the configuration file at `/home/bitcoin/.lnd/lnd.conf`

```
[Application Options]
tlsextraip=0.0.0.0
restlisten=0.0.0.0:8080
no-macaroons=true
```

The `tlsextraip` is required if you plan on running the application on different machine to where the [Lightning Network Daemon](https://github.com/lightningnetwork/lnd) ️is running. To simplify configuration
the `no-macaroons` option should be set to `true`.

## Usage

### Command line
`Lightning.Metrics.App.exe --help` 

### Docker

#### On Windows or Linux
`docker run badokun/lnd-metrics:latest --help`

#### On RaspBerry Pi
`docker run badokun/lnd-metrics:arm32 --help`

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

## Running on RaspiBolt

The easiest method to run metrics collection on a Raspberry Pi is by running the Docker image.

* Test the connection to your LND REST API by adding `--test-lndApi`.
* Test the connection to your InfluxDb by adding `--test-influxDb`.
* See all options by adding `--help`

```
 docker run --rm --net host --name lnd-metrics-arm32 \
        badokun/lnd-metrics:arm32 \
        --influxDbUri http://127.0.0.1:8086 \
        --network testnet \
        --lndRestApiUri https://127.0.0.1:8080 \
        --help
```

When you've confirmed connectivity to both the LND REST API and InfluxDb you can ommit the `--rm` flag. Pro tip: to keep it always running on a restart add `--restart always`. The command below does this

```
 docker run --restart always -d --net host --name lnd-metrics-arm32 \
        badokun/lnd-metrics:arm32 \
        --influxDbUri http://127.0.0.1:8086 \
        --network testnet \
        --lndRestApiUri https://127.0.0.1:8080
```


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



#### Raspberry

The Raspberry image has to be built on a Raspberry device to get around ARM architecture related issues


* If it builds without errors run the container to ensure it works as expected. We're using the `--net=host` flag to since our RaspiBolt `ufw` rule has allowed local traffic only. Without this flag you need to ensure the Docker host's IP address has also been allowed to the `ufw` rule.

```
docker run --net=host --restart always -v /home/admin/projects/lightning-metrics/src/Lightning.Metrics.App:/data -d --name lnd-metrics  lnd-metrics:arm --configPath /data/metrics.json
```



```
docker logs lnd-metrics
docker tag lnd-metrics:arm badokun/lnd-metrics:arm
docker push badokun/lnd-metrics:arm
```

## Troubleshooting

### Accessing InfluxDb through Docker

` docker exec -it c10b584e3210 /usr/bin/influx`

### Restarting Lnd

`sudo systemctl start lnd`

Follow the tail:
`sudo journalctl -f -u lnd`


### Testnet

https://lnroute.com/testnet-faucets/

## Resources

* [Lnd Rest Api](https://api.lightning.community/rest/index.html)
* Setting the `tlsextraip` to `0.0.0.0` was [suggested here](https://github.com/lightningnetwork/lnd/issues/1567#issuecomment-437665324)
* Lnd configuration [reference](https://github.com/lightningnetwork/lnd/blob/master/sample-lnd.conf)
* [RaspiBolt Guide](https://github.com/badokun/guides/tree/master/raspibolt)

* Automated Docker builds - https://docs.docker.com/docker-hub/builds/
---

notes

look at https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/Dockerfile

https://github.com/btcpayserver/BTCPayServer.Lightning/blob/master/src/BTCPayServer.Lightning.LND/LndSwaggerClient.cs

client: https://github.com/btcpayserver/BTCPayServer.Lightning