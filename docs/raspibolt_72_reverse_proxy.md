[ [Intro](README.md) ] -- [ [Preparations](raspibolt_10_preparations.md) ] -- [ [Raspberry Pi](raspibolt_20_pi.md) ] -- [ [Bitcoin](raspibolt_30_bitcoin.md) ] -- [ [Lightning](raspibolt_40_lnd.md) ] -- [ [Mainnet](raspibolt_50_mainnet.md) ] -- [ [**Bonus**](raspibolt_60_bonus.md) ] -- [ [Troubleshooting](raspibolt_70_troubleshooting.md) ]

------

### Beginner’s Guide to ️⚡Lightning️⚡ on a Raspberry Pi

------

## Bonus guide: Make Grafana visible on the internet
*Difficulty: advanced*

Having Grafana available on your local network is great, but it would be even better if you can monitor your RaspiBolt from any location.

To do this you'll need to setup the following:
* Get a free domain
* Port forwarding 80 and 443 on your home router to your RaspiBolt
* Install nginx (engine-x) as a reverse proxy. We do this so we can have all our requests run over https and so we have flexibility in adding more websites later, e.g. kibana
* Install certbot for managing your https certificate. 
* Restart grafana docker image with added configuration

### Getting a free domain

There are multiple providers offering free domains, one of them is [freenom](https://my.freenom.com), but really any will do.

Set your domain to point to your home network's external IP address. This can be obtained by going to [www.whatismyip.com](https://www.whatismyip.com/)

### Router Port forwarding

You will need to route port 80 (http) and 443 (https) to your RaspiBolt device. Refer to the  [ [Raspberry Pi](raspibolt_20_pi.md) ] section on how to do this

### Install nginx to serve as reserve proxy

When internet traffic over port 443 arrives at your RaspiBolt we need to ensure it's all encrypted. To do this we use nginx's reverse proxy feature.

```
sudo apt-get update
sudo apt-get install nginx -y 
```

Confirm it's running 

```
systemctl status nginx.service
```

### Install certbot for your https certificate

```
sudo sed -i "$ a\deb http://ftp.debian.org/debian stretch-backports main" /etc/apt/sources.list
sudo apt-get update
sudo apt-get install certbot -t stretch-backports -y --force-yes
```

Test that you're able to renew the certificate

```
$ sudo certbot renew --dry-run
```

### Update Docker so it runs like this

```
 docker run \
    -d \
    -e "GF_SECURITY_ADMIN_PASSWORD=PASSWORD_[A]" \
    -e "GF_SERVER_DOMAIN=raspibolt.badokun.tk" \
    -e "GF_SERVER_ROOT_URL=%(protocol)s://%(domain)s/grafana/" \
    --name grafana \
    -v grafana-storage:/var/lib/grafana \
    --restart always \
    --net=host \
    grafana/grafana:5.4.3
```

### NGINX Config

```
# Redirect HTTP requests to HTTPS
server {
    listen 80;
    server_name  raspibolt.badokun.tk;
    return 301 https://$host$request_uri;
}
# For ssl
server {
    ssl on;

    ssl_certificate /etc/letsencrypt/live/raspibolt.badokun.tk/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/raspibolt.badokun.tk/privkey.pem;
    ssl_protocols TLSv1.1 TLSv1.2;

    # ssl_prefer_server_ciphers on;
    # ssl_dhparam /etc/ssl/certs/dhparam.pem;
    # ssl_ciphers 'ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-AES256-GCM-SHA384:DHE-RSA-AES128-GCM-SHA256:DHE-DSS-AES128-GCM-SHA256:kEDH+AESGCM:$
    # ssl_session_timeout 1d;
    # ssl_session_cache shared:SSL:50m;
    # ssl_stapling on;
    # ssl_stapling_verify on;
    # add_header Strict-Transport-Security max-age=15768000;

    default_type  application/octet-stream;

    listen 443;
    server_name  raspibolt.badokun.tk;

    root /var/www/raspibolt.badokun.tk;

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

Edit config
```
sudo nano /etc/nginx/sites-enabled/raspibolt.badokun.tk.conf
```

Restart nginx
```
sudo systemctl restart nginx.service
```

-e "GF_SERVER_ROOT_URL=https://raspibolt.badokun.tk/grafana" \

https://www.techcoil.com/blog/building-a-reverse-proxy-server-with-nginx-certbot-raspbian-stretch-lite-and-raspberry-pi-3/


https://www.digitalocean.com/community/tutorials/how-to-protect-an-nginx-server-with-fail2ban-on-ubuntu-14-04


regex for auth failures

[Definition]
failregex = ^<HOST>.*"(GET|POST).*" (404|444|403|400) .*$
ignoreregex =



how lightning works
https://dev.lightning.community/overview/#channel-lifecycle