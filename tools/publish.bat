@echo off

mkdir publish

:: Windows
powershell write-host -fore Cyan "Compiling for Windows"

rmdir /s /q tdl-wrapper\obj tdl-wrapper\bin
dotnet build tdl-wrapper\tdl-wrapper.csproj

bflat build -Os --no-globalization --os windows --arch x64 -o publish\tdl-wrapper_Windows_64bit.exe
bflat build -Os --no-globalization --os windows --arch arm64 -o publish\tdl-wrapper_Windows_arm64.exe

dotnet publish tdl-wrapper\tdl-wrapper.csproj --sc -p:PublishSingleFile=true -p:InvariantGlobalization=true -p:PublishTrimmed=true -c release -p:AssemblyName=tdl-wrapper_Windows_32bit -r win-x86 -o publish

:: Linux
powershell write-host -fore Cyan "Compiling for Linux"

rmdir /s /q tdl-wrapper\obj tdl-wrapper\bin
dotnet build tdl-wrapper\tdl-wrapper.csproj

bflat build -Os --separate-symbols --no-globalization --os linux --arch x64 -o publish\tdl-wrapper_Linux_64bit
bflat build -Os --separate-symbols --no-globalization --os linux --arch arm64 -o publish\tdl-wrapper_Linux_arm64

:: MacOs
powershell write-host -fore Cyan "Compiling for MacOs"
rmdir /s /q tdl-wrapper\obj tdl-wrapper\bin

dotnet publish tdl-wrapper\tdl-wrapper.csproj --sc -p:PublishSingleFile=true -p:InvariantGlobalization=true -p:PublishTrimmed=true -c release -p:AssemblyName=tdl-wrapper_MacOs_64bit -r osx-x64 -o publish
dotnet publish tdl-wrapper\tdl-wrapper.csproj --sc -p:PublishSingleFile=true -p:InvariantGlobalization=true -p:PublishTrimmed=true -c release -p:AssemblyName=tdl-wrapper_MacOs_arm64 -r osx-arm64 -o publish

powershell write-host -fore Cyan "Finished compiling, deleting artifacts..."
DEl publish\*.pdb publish\*.dwo
