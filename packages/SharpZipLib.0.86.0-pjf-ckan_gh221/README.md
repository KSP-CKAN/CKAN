This is a patched copy of ZSharpLib which fixes the
`ZipException: Version required to extract this entry not supported (788)`
bug described [here](http://community.sharpdevelop.net/forums/t/21758.aspx).

- Get [this branch](https://github.com/pjf/SharpZipLib/tree/ckan_gh221).
- `cd SharpZipLib/src`
- `xbuild /p:Configuration=Release ICSharpCode.SharpZLib.csproj`
- Copy the build artefacts from `../bin`.
