#!/bin/sh
unset LD_PRELOAD
apt update -y
apt install -y libssl-dev openjdk-14-jre-headless figlet zlib1g-dev
figlet Insalling dotnet
curl -SL -o dotnet.tar.gz https://download.visualstudio.microsoft.com/download/pr/4b114207-eaa2-40fe-8524-bd3c56b2fd9a/1d74fdea8701948c0150c39645455b2f/dotnet-runtime-5.0.0-linux-arm64.tar.gz
mkdir -p /usr/share/dotnet
tar -zxf dotnet.tar.gz -C /usr/share/dotnet
ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
figlet Fetching patcher
wget -q https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=android -O tmp.zip
unzip tmp.zip
rm tmp.zip
chmod a+x smp/SmaliPatcherMin
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