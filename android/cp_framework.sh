#!/bin/sh
rm -rf adb
mkdir adb
cp /system/build.prop adb/
cp -R /system/framework adb/
cp /system/system/build.prop adb/
cp -R /system/system/framework adb/