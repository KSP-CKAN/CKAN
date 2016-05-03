FROM mono
RUN echo '#!/bin/bash\nchown --reference=/kspdir/GameData -R /kspdir\n' >> /root/cleanup.sh
RUN chmod +x /root/cleanup.sh
RUN echo 'trap /root/cleanup.sh EXIT\nckan()\n{\n  mono /build/CmdLine.exe "$@" --kspdir /kspdir --headless\n}\nckan update\n' >> /root/.bashrc
RUN chmod +x /root/.bashrc
RUN echo '#!/bin/bash\nsource /root/.bashrc\nckan scan\nckan upgrade --all\n' >> /root/entrypoint.sh
RUN chmod +x /root/entrypoint.sh
RUN apt-get -y update && apt-get -y install libcurl4-openssl-dev
RUN mkdir /kspdir
VOLUME ["/kspdir"]
COPY . /source
WORKDIR /source
RUN nuget restore -NonInteractive
RUN xbuild /property:Configuration=Release /property:OutDir=/build/
CMD ["/root/entrypoint.sh"]
