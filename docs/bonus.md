[ [Intro](intro.md) ] -- [ [Performance Monitoring](performance_monitoring.md) ] -- [ [Lightning Metrics](lightning_metrics.md) ] -- [ [**Bonus**](bonus.md) ] -- [ [Troubleshooting](troubleshooting.md) ]

------

# Accessing Grafana from the internet

Having Grafana available on your local network is great, but it would be even better if you can monitor your Raspberry from any location.

To do this you'll need to setup the following:

- Get a free domain
- Port forwarding 80 and 443 on your home router to your Raspberry
- Install nginx (engine-x) as a reverse proxy. We do this so we can have all our requests run over https and so we have flexibility in adding more websites later, e.g. [Kibana](https://www.elastic.co/products/kibana), or [BTC RPC Explorer](https://github.com/janoside/btc-rpc-explorer)
- Install certbot for managing your https certificate.
- Restart Grafana docker image with added configuration

## Getting a free domain

There are multiple providers offering free domains, one of them is [freenom](https://my.freenom.com), but really any will do.

Set your domain to point to your home network's external IP address. This can be obtained by going to [www.whatismyip.com](https://www.whatismyip.com/)

> For the remainder of this guide, replace `www.lndftw.com` with your domain name.

## Router Port forwarding

You will need to route port 80 (http) and 443 (https) to your Raspberry. Refer to the  [ [Raspberry Pi](https://github.com/Stadicus/guides/blob/master/raspibolt/raspibolt_20_pi.md) ] section on how to do this

## Update Firewall configuration

In order to let http and https traffic through run the following:

```bash
sudo su
ufw allow 80 comment 'allow http to all'
ufw allow 443 comment 'allow https to all'
exit
```

## Install nginx as reserve proxy

When internet traffic over port 443 arrives at your Raspberry we need to ensure it's all encrypted. To do this we use nginx's reverse proxy feature.

```bash
sudo apt-get update
sudo apt-get install nginx -y
```

Confirm it's running by running the command below

```bash
systemctl status nginx.service
```

## Prepare nginx for https certificate installation

Create a configuration file (replace the domain name with yours)

```bash
sudo nano /etc/nginx/sites-enabled/www.lndftw.com.conf
```

Paste the text and save

```conf
server {
    listen 80;
    server_name  www.lndftw.com;
    root /var/www/www.lndftw.com;
    location ~ /.well-known {
        allow all;
    }
}
```

Create directory

```bash
sudo mkdir /var/www/www.lndftw.com
sudo chown www-data:www-data /var/www/www.lndftw.com
```

Restart nginx

```bash
sudo systemctl restart nginx.service
```

## Install certbot for your https certificate

```bash
sudo sed -i "$ a\deb http://ftp.debian.org/debian stretch-backports main" /etc/apt/sources.list
sudo apt-get update
sudo apt-get install certbot -t stretch-backports -y --force-yes
```

```bash
sudo certbot certonly -a webroot --webroot-path=/var/www/www.lndftw.com -d www.lndftw.com
```

## Update nginx configuration

Now that you have Edit the configuration (replacing the domain name with yours)

```bash
sudo nano /etc/nginx/sites-enabled/www.lndftw.com.conf
```

Copy the contents below (replacing the domain name with yours and the `proxy_pass` value with the ip address of your Raspberry)

```conf
# Redirect HTTP requests to HTTPS
server {
    listen 80;
    server_name  www.lndftw.com;
    return 301 https://$host$request_uri;
}
# For ssl
server {
    ssl on;

    ssl_certificate /etc/letsencrypt/live/www.lndftw.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/www.lndftw.com/privkey.pem;
    ssl_protocols TLSv1.1 TLSv1.2;

    default_type  application/octet-stream;

    listen 443;
    server_name  www.lndftw.com;

    root /var/www/www.lndftw.com;

    location ~ /.well-known {
        allow all;
    }

    index index.html index.htm;
    location = / {
        return 301 /grafana;
    }
    location /grafana/ {
        proxy_pass http://192.168.1.40:3000/;
    }
}
```

Restart nginx

```bash
sudo systemctl restart nginx.service
```

### Reconfigure Grafana

Grafana needs to be reconfigured in order to work when using a reverse proxy.

Stop and remove the current Grafana container (your settings will remain intact since it's persisted on another volume)

```bash
docker stop grafana
docker rm grafana
```

> Update the domain to yours in the command below

```bash
docker run \
    -d \
    -e "GF_SECURITY_ADMIN_PASSWORD=PASSWORD_[A]" \
    -e "GF_SERVER_DOMAIN=www.lndftw.com" \
    -e "GF_SERVER_ROOT_URL=%(protocol)s://%(domain)s/grafana/" \
    --name grafana \
    -v grafana-storage:/var/lib/grafana \
    --restart always \
    --net=host \
    grafana/grafana:5.4.3
```

## Test renewal

Test that you're able to renew the certificate.

> Your Raspberry needs to have port 80 and 443 open and routed at this stage

```bash
sudo certbot renew --dry-run
```

### Security strengthening with fail2ban

Currently a work in progress, documenting the steps to follow soon

Example regex for auth failures

```yml
[Definition]
failregex = ^<HOST>.*"(GET|POST).*" (404|444|403|400) .*$
ignoreregex =
```

## Accessing Grafana inside LAN

My network configuration didn't allow me to access grafana from inside the work so I had to run another instance on port 3001

```bash
docker run \
  -d \
  -e "GF_SECURITY_ADMIN_PASSWORD=PASSWORD_[A]" \
  -e "GF_SERVER_HTTP_PORT=3001" \
  --name grafana-local \
  -v grafana-storage:/var/lib/grafana \
  --restart always \
  --net=host \
  grafana/grafana:5.4.3
```

# Continuous Backup

Backing your RaspiBolt up may come in handy when the SD cards fails, LND/Bitcoin upgrade, or during a back OS upgrade.

I prefer the disc image backup which helps you do a system restore
- https://pimylifeup.com/backup-raspberry-pi/
- logrotate http://www.drdobbs.com/logrotate-a-backup-solution/199101578

Reference material:

* <https://www.techcoil.com/blog/building-a-reverse-proxy-server-with-nginx-certbot-raspbian-stretch-lite-and-raspberry-pi-3/>

* <https://www.digitalocean.com/community/tutorials/how-to-protect-an-nginx-server-with-fail2ban-on-ubuntu-14-04>

Further useful reading material - <https://dev.lightning.community/overview/#channel-lifecycle>

------

Donations

If you feel like this has been useful and wish to donate, feel free to send a satoshi or two to this address, obviously use Lightning for near free instant transfers:

* 👉 BTC: `bc1qx2hn38vc8f0fkn3hu8pmpuglg35ctqvx2rzzjs`
* 👉 Lightning: <https://tippin.me/@rubberroad>