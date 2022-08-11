Name: ckan
Version: %{_version}
Release: 1%{?dist}
Summary: Mod manager for Kerbal Space Program
URL: https://ksp-ckan.space
Packager: The CKAN authors <rpm@ksp-ckan.space>
License: MIT
AutoReqProv: no
Requires: mono-core >= 5.0
BuildArch: noarch
Source0: ckan
Source1: ckan.exe
Source2: ckan.1
Source3: ckan.desktop
Source4: ckan.ico
Source5: ckan-consoleui.desktop
Source6: ckan-cmdprompt.desktop

%description
KSP-CKAN official client.
Official client for the Comprehensive Kerbal Archive Network (CKAN).
Acts as a mod manager for Kerbal Space Program mods.

%post
cert-sync /etc/pki/tls/certs/ca-bundle.crt
cert-sync --user /etc/pki/tls/cert.pem

%install
umask 0022
mkdir -p %{buildroot}%{_bindir}
cp %{SOURCE0} %{buildroot}%{_bindir}
mkdir -p %{buildroot}/usr/lib/ckan
cp %{SOURCE1} %{buildroot}/usr/lib/ckan
mkdir -p %{buildroot}%{_mandir}/man1
cp %{SOURCE2} %{buildroot}%{_mandir}/man1
mkdir -p %{buildroot}%{_datadir}/applications
cp %{SOURCE3} %{buildroot}%{_datadir}/applications
cp %{SOURCE5} %{buildroot}%{_datadir}/applications
cp %{SOURCE6} %{buildroot}%{_datadir}/applications
mkdir -p %{buildroot}%{_datadir}/icons
cp %{SOURCE4} %{buildroot}%{_datadir}/icons

%files
%{_bindir}/*
/usr/lib/ckan
%{_datadir}/applications/*
%{_datadir}/icons/*
%{_mandir}/man1/*

%changelog
* Mon Aug 8 2022 The CKAN authors <rpm@ksp-ckan.space> 1.31.2
- Added Command Prompt desktop file

* Fri May 15 2020 The CKAN authors <rpm@ksp-ckan.space> 1.28.0
- Added ConsoleUI desktop file

* Thu May 9 2019 The CKAN authors <rpm@ksp-ckan.space> 1.26.3-1
- Initial RPM release
