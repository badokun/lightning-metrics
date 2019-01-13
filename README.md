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




## Troubleshooting

### Accessing InfluxDb through Docker

` docker exec -it c10b584e3210 /usr/bin/influx`

---

notes

look at https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/Dockerfile

https://github.com/btcpayserver/BTCPayServer.Lightning/blob/master/src/BTCPayServer.Lightning.LND/LndSwaggerClient.cs

client: https://github.com/btcpayserver/BTCPayServer.Lightning