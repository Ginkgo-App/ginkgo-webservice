#!/bin/bash
docker build -t micro-api-core . && heroku login -i && heroku container:login && heroku container:push web -a micro-api-core && heroku container:release web -a micro-api-core