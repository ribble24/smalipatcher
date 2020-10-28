#!/bin/sh
cd ~
# Install dependencies
pkg update -y
pkg install proot-distro figlet wget

figlet Setting up env
proot-distro install ubuntu
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