trap /root/cleanup.sh EXIT
ckan()
{
	mono /build/CmdLine.exe "$@" --kspdir /kspdir --headless
}
