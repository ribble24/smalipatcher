#!/bin/sh
cd ~ || exit
# Install dependencies
apt update
apt upgrade -y
apt install -y proot-distro figlet wget git make clang

figlet Setting up env
proot-distro install ubuntu-20.04
wget -q https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/setup2.sh?inline=false -O $PREFIX/var/lib/proot-distro/installed-rootfs/ubuntu/root/setup2.sh
chmod a+x $PREFIX/var/lib/proot-distro/installed-rootfs/ubuntu/root/setup2.sh
proot-distro login ubuntu -- ./setup2.sh

echo Creating convenience scripts
echo "#!/bin/sh" > smalipatcher-shell
echo "proot-distro login ubuntu" >> smalipatcher-shell
chmod a+x smalipatcher-shell
echo "#!/bin/sh" > smalipatcher
echo "proot-distro login ubuntu -- ./smalipatcher.sh \"\$@\"" >> smalipatcher
chmod a+x smalipatcher
figlet Completed
