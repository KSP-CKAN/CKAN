FROM mono
RUN apt-get -y update && apt-get -y install libcurl4-openssl-dev
RUN mkdir /kspdir
VOLUME ["/kspdir"]
COPY . /source
WORKDIR /source
RUN nuget restore -NonInteractive
RUN xbuild /property:Configuration=Release /property:OutDir=/build/
COPY ./root /root
CMD ["/root/entrypoint.sh"]
