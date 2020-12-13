#!/bin/sh
unset LD_PRELOAD
apt update -y
apt install -y libssl-dev openjdk-14-jre-headless figlet zlib1g-dev
figlet Installing dotnet
wget -q https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod a+x dotnet-install.sh
./dotnet-install.sh -c 5.0 #TODO change if updated
figlet Fetching patcher
if [ "$(uname -m)" == "aarch64" ]
then
    wget -q https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=android -O tmp.zip
elif [ "$(uname -m)" == "arm64" ]
then
    wget -q https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=android -O tmp.zip
elif [ "$(uname -m)" == "armv7l" ]
then
    wget -q https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=android-arm -O tmp.zip
elif [ "$(uname -m)" == "arm" ]
then
    wget -q https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=android-arm -O tmp.zip
fi
unzip tmp.zip
rm tmp.zip
figlet Building vdexExtractor
git clone https://github.com/anestisb/vdexExtractor vdx
cd vdx
./make.sh
cp bin/vdexExtractor ..
cd ..
rm -rf vdx
echo Fetching scripts
wget -q https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/cp_framework.sh?inline=false -O cp_framework.sh
chmod a+x cp_framework.sh
wget -q https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/smalipatcher.sh?inline=false -O smalipatcher.sh
chmod a+x smalipatcher.sh
echo Env setup complete
