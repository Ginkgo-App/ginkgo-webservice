docker build -t ginkgo-webapi . && heroku login -i && heroku container:login && heroku container:push web -a ginkgo-webapi && heroku container:release web -a ginkgo-webapi