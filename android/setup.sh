#!/bin/sh
cd ~ || exit
# Install dependencies
pkg update -y
pkg install proot-distro figlet wget git make clang

figlet Setting up env
proot-distro install ubuntu-20.04
wget -q https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/setup2.sh?inline=false -O $PREFIX/var/lib/proot-distro/installed-rootfs/ubuntu-20.04/root/setup2.sh
chmod a+x $PREFIX/var/lib/proot-distro/installed-rootfs/ubuntu-20.04/root/setup2.sh
proot-distro login ubuntu-20.04 -- ./setup2.sh

echo Creating convenience scripts
echo "#!/bin/sh" > smalipatcher-shell
echo "proot-distro login ubuntu-20.04" >> smalipatcher-shell
chmod a+x smalipatcher-shell
echo "#!/bin/sh" > smalipatcher
echo "proot-distro login ubuntu-20.04 -- ./smalipatcher.sh \"\$@\"" >> smalipatcher
chmod a+x smalipatcher
figlet Completed
