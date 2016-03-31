FROM mono
RUN apt-get -y update && apt-get -y install libcurl4-openssl-dev
COPY . /source
WORKDIR /source
RUN nuget restore -NonInteractive
RUN xbuild /property:Configuration=Release /property:OutDir=/build/
RUN mkdir /kspdir
VOLUME ["/kspdir"]
CMD ["./entrypoint.sh"]
