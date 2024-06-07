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
Source4: ckan-consoleui.desktop
Source5: ckan-cmdprompt.desktop
Source6: ckan-16.png
Source7: ckan-32.png
Source8: ckan-48.png
Source9: ckan-64.png
Source10: ckan-96.png
Source11: ckan-128.png
Source12: ckan-256.png

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
cp %{SOURCE4} %{buildroot}%{_datadir}/applications
cp %{SOURCE5} %{buildroot}%{_datadir}/applications
for SRC in %{SOURCE6} %{SOURCE7} %{SOURCE8} %{SOURCE9} %{SOURCE10} %{SOURCE11} %{SOURCE12}
do
    DIM=$(basename "${SRC}" | sed -e 's/[^0-9]//g')
    mkdir -p %{buildroot}%{_datadir}/icons/hicolor/${DIM}x${DIM}/apps
    cp "${SRC}" %{buildroot}%{_datadir}/icons/hicolor/${DIM}x${DIM}/apps/ckan.png
done

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
