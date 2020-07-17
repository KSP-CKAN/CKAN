# Dockerfile for the Comprehensive Kerbal Archive Network (CKAN) client
#
#
# You do not need to use Docker to build, run, or test the CKAN, but you
# can if you like! :)
#
#
# To build the container:
# $ docker build -t ckan .
# 
# To use the container to update all mods:
# $ docker run --rm -v ${KSPDIR}:/kspdir ckan
# 
# To use the container to install MechJeb:
# $ docker run --rm -v ${KSPDIR}:/kspdir ckan install MechJeb
# 
# Both of the last two lines require that the ${KSPDIR} value be set.
# 
# There is a docker-compose.yml supplied which will automatically do this for Linux users.
# 
# To use the YAML file to build the container:
# $ docker-compose build ckan
# 
# To use the YAML file to update all mods:
# $ docker-compose run --rm ckan
# 
# To use the YAML file to install MechJeb
# $ docker-compose run --rm ckan install MechJeb

FROM mono
RUN echo '#!/bin/bash\n\
  chown --reference=/kspdir/GameData -R /kspdir\n\
' >> /root/cleanup.sh
RUN chmod +x /root/cleanup.sh
RUN echo 'trap /root/cleanup.sh EXIT\n\
  ckan()\n\
  {\n\
    mono /build/ckan.exe "$@" --kspdir /kspdir --headless\n\
  }\n\
  ckan update\n\
' >> /root/.bashrc
RUN echo '#!/bin/bash\n\
  source /root/.bashrc\n\
  ckan scan\n\
  if [ "$#" -ne 0 ]; then\n\
    ckan $@\n\
  else\n\
    ckan upgrade --all\n\
  fi\n\
' >> /root/entrypoint.sh
RUN chmod +x /root/entrypoint.sh
RUN apt-get -y update && apt-get -y install
RUN mkdir /kspdir
VOLUME ["/kspdir"]
COPY . /source
WORKDIR /source
ARG config
ENV config ${config:-Release}
RUN ./build --configuration=${config}
RUN mkdir /build
RUN cp _build/repack/${config}/ckan.exe /build/ckan.exe
ENTRYPOINT ["/root/entrypoint.sh"]
