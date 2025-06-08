# Everything we need in both the build and prod images
FROM ubuntu:latest AS base

ARG configuration=Debug

# Don't prompt for time zone
ENV DEBIAN_FRONTEND=noninteractive

# Properly handle Unicode
ENV LANG=C.utf-8

# Put user-installed Python code in path
ENV PATH="$PATH:/root/.local/bin"

# Install Git and Python
RUN apt-get update \
    && apt-get update -y \
    && apt-get install -y --no-install-recommends \
        ca-certificates libicu74 git libffi-dev \
        python3 python-is-python3

# Trust all git repos
RUN git config --global --add safe.directory '*'

# Isolate Python build stuff in a separate container
FROM base AS build

# Install Python build deps
RUN apt-get install -y --no-install-recommends \
    python3-pip python3-setuptools python3-dev

# Install the meta tester's Python code and its Infra dep
ENV PIP_ROOT_USER_ACTION=ignore
ENV PIP_BREAK_SYSTEM_PACKAGES=1
RUN pip3 install 'git+https://github.com/KSP-CKAN/NetKAN-Infra#subdirectory=netkan'
RUN pip3 install 'git+https://github.com/KSP-CKAN/xKAN-meta_testing'

# Prune unused deps
RUN pip3 --no-input uninstall -y flask gunicorn werkzeug

# The image we'll actually use
FROM base AS prod

# Purge APT download cache, package lists, and logs
RUN apt-get clean \
    && rm -r /var/lib/apt/lists /var/log/dpkg.log /var/log/apt

# Extract built Python packages from the build image
COPY --from=build /usr/local /usr/local

# Install the config file that tells the meta tester how to run ckan and netkan
ADD Netkan/metadata.ini /usr/local/etc/.

# Install the .NET assemblies the meta tester uses
ADD _build/out/CKAN-CmdLine/${configuration}/bin/net8.0/linux-x64/publish/. /usr/local/bin/.
ADD _build/out/CKAN-NetKAN/${configuration}/bin/net8.0/linux-x64/publish/. /usr/local/bin/.

ENTRYPOINT ["ckanmetatester"]
