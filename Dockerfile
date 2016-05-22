FROM mono
RUN echo '#!/bin/bash\n\
  chown --reference=/kspdir/GameData -R /kspdir\n\
' >> /root/cleanup.sh
RUN chmod +x /root/cleanup.sh
RUN echo 'trap /root/cleanup.sh EXIT\n\
  ckan()\n\
  {\n\
    mono /build/CmdLine.exe "$@" --kspdir /kspdir --headless\n\
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
RUN apt-get -y update && apt-get -y install libcurl4-openssl-dev
RUN mkdir /kspdir
VOLUME ["/kspdir"]
COPY . /source
WORKDIR /source
RUN nuget restore -NonInteractive
RUN xbuild /property:Configuration=Release /property:OutDir=/build/
ENTRYPOINT ["/root/entrypoint.sh"]
