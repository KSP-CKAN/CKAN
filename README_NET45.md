### How to get this compiling on VS 2022 and later

Visual Studio 2022 and later can't install the .NET 4.5 Developer Pack for working on these projects. Until migration is decided upon for .NET 4.7 and later, you may need to do the following to work with this project...

1. ZIP up a backup copy of **C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5**

2. Delete **C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5**

3. **Copy C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1** or **v.4.5.2** to **C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5**

4. Project should load normally.

Reference: https://thomaslevesque.com/2021/11/12/building-a-project-that-target-net-45-in-visual-studio-2022/
