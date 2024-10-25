#!/bin/bash

while read -ra line; do
	line=$(echo $line | tr -d '\r')
	echo "$line/build.sh"
	cd $line
	./build.sh || exit 1
	cd ..
	echo ""
done < "./mods.list"

