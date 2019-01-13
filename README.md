# lightning-metrics


## Overview

This application will query a lightning node and push all metrics into an InfluxDB which can be used as a data source for Grafana Dashboards. 
The [RaspiBolt](https://github.com/badokun/guides/blob/master/raspibolt/README.md) project served as motivation for setting this up, and in particular
[Bonus guide: Performance Monitoring](https://github.com/badokun/guides/blob/master/raspibolt/raspibolt_71_monitoring.md). 

## Configuration

Lightning-Metrics uses the LND Rest API which requires the following configuration. On a RaspiBolt setup you will find the Lnd Configuration file at `/home/bitcoin/.lnd/lnd.conf`

```
[Application Options]
tlsextraip=0.0.0.0
restlisten=0.0.0.0:8080
no-macaroons=true

```

The `tlsextraip` is required if you plan on running the application on different machine to where the Lightning application is running. To simplify configuration
the `no-macaroons` option should be set to true.



## Usage

`Lightning.Metrics.App.exe --configPath=metrics.conf` 

## Metrics

* Lightning Metrics
  * tags:
    * host
  * fields:
    * balance


## Development

### Docker


#### Raspberry

The Raspberry image has to be built on a Raspberry device to get around ARM architecture related issues

* Build the docker container
```
docker build -t lnd-metrics:arm -f Raspberry.Dockerfile .
```
* If it builds without errors run the container to ensure it works as expected. We're using the `--net=host` flag to since our RaspiBolt `ufw` rule has allowed local traffic only. Without this flag you need to ensure the Docker host's IP address has also been allowed to the `ufw` rule.

```
docker run --net=host --restart always -v /home/admin/projects/lightning-metrics/src/Lightning.Metrics.App:/data -d --name lnd-metrics  lnd-metrics:arm --configPath /data/metrics.json
```



```
docker logs lnd-metrics
docker tag lnd-metrics:arm badokun/lnd-metrics:arm
docker push badokun/lnd-metrics:arm
```

#### Windows\Linux Distro
```
docker build -t lnd-metrics .
docker tag lnd-metrics badokun/lnd-metrics:64
docker push badokun/lnd-metrics:64
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