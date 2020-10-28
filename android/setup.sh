#!/bin/sh
cd ~
# Create user-facing scripts
echo "#!/bin/sh" > smalipatcher-shell
echo "proot-distro login ubuntu" >> smalipatcher-shell
chmod a+x smalipatcher-shell
echo "#!/bin/sh" > smalipatcher
echo "proot-distro login ubuntu -- ./smalipatcher.sh \"\$@\"" >> smalipatcher
chmod a+x smalipatcher
# Install dependencies
pkg update -y
pkg install proot-distro
# Set up proot
proot-distro install ubuntu
wget -q https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/setup2.sh?inline=false -O $PREFIX/var/lib/proot-distro/installed-rootfs/ubuntu/root/setup2.sh
chmod a+x $PREFIX/var/lib/proot-distro/installed-rootfs/ubuntu/root/setup2.sh
proot-distro login ubuntu -- ./setup2.sh