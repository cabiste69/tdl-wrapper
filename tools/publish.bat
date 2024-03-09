echo off

mkdir publish publish\windows publish\linux publish\mac

rmdir /s /q tdl-wrapper\obj tdl-wrapper\bin
:: Windows
echo publishing windows with bflat
dotnet build tdl-wrapper\tdl-wrapper.csproj
bflat build -Os --no-globalization --os windows --arch x64 -o publish\windows\tdl-wrapper_Windows_64bit.exe
bflat build -Os --no-globalization --os windows --arch arm64 -o publish\windows\tdl-wrapper_Windows_arm64.exe

echo publishing windows with dotnet
dotnet publish tdl-wrapper\tdl-wrapper.csproj --sc -p:PublishSingleFile=true -p:InvariantGlobalization=true -p:PublishTrimmed=true -c release -p:AssemblyName=tdl-wrapper_Windows_32bit -r win-x86 -o publish\windows

rmdir /s /q tdl-wrapper\obj tdl-wrapper\bin
:: Linux
echo publishing Linux with bflat
dotnet build tdl-wrapper\tdl-wrapper.csproj
bflat build -Os --separate-symbols --no-globalization --os linux --arch x64 -o publish\linux\tdl-wrapper_Linux_64bit
bflat build -Os --separate-symbols --no-globalization --os linux --arch arm64 -o publish\linux\tdl-wrapper_Linux_arm64

rmdir /s /q tdl-wrapper\obj tdl-wrapper\bin
:: MacOs
dotnet publish tdl-wrapper\tdl-wrapper.csproj --sc -p:PublishSingleFile=true -p:InvariantGlobalization=true -p:PublishTrimmed=true -c release -p:AssemblyName=tdl-wrapper_MacOs_64bit -r osx-x64 -o publish\mac
dotnet publish tdl-wrapper\tdl-wrapper.csproj --sc -p:PublishSingleFile=true -p:InvariantGlobalization=true -p:PublishTrimmed=true -c release -p:AssemblyName=tdl-wrapper_MacOs_arm64 -r osx-arm64 -o publish\mac
