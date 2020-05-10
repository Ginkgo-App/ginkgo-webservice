#!/bin/bash
docker build -t micro-api-core . && docker run -p 5000:80 micro-api-core:latest