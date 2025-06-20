FROM ubuntu:latest

# Don't prompt for time zone
ENV DEBIAN_FRONTEND=noninteractive

# Properly handle Unicode
ENV LANG=C.utf-8

# Install dotnet dependencies
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends ca-certificates libicu74

# Purge APT download cache, package lists, and logs
RUN apt-get clean \
    && rm -r /var/lib/apt/lists /var/log/dpkg.log /var/log/apt

RUN groupmod -n netkan ubuntu && usermod -l netkan -d /home/netkan -m ubuntu
USER netkan
WORKDIR /home/netkan
ADD --chown=netkan . .
ENTRYPOINT ./CKAN-NetKAN --game ${GAME:-KSP} --queues $QUEUES \
  --net-useragent "Mozilla/5.0 (compatible; Netkanbot/1.0; CKAN/$(./CKAN-NetKAN --version); +https://github.com/KSP-CKAN/NetKAN-Infra)" \
  --github-token $GH_Token --gitlab-token "$GL_Token" --cachedir ckan_cache -v
