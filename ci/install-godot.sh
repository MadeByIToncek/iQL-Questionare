#!/bin/bash

if [ ! -d /tmp/engine-godot ]; then
    sudo apt-get install jq unzip -y

    curl https://api.github.com/repos/godotengine/godot/releases/latest > /tmp/engine-ver

    cd /tmp/
    url=$(cat /tmp/engine-ver|jq '.assets[] | select (.name | endswith("_mono_linux_x86_64.zip"))' | jq '.browser_download_url')

    echo "$url"
    echo "$url"|xargs wget -q -O "godot.zip" -c {}

    unzip /tmp/godot.zip
    mv /tmp/Godot_* /tmp/engine-godot

    rm /tmp/engine-ver
    rm /tmp/godot.zip
fi

if [ ! -d /tmp/templates.tpz ]; then
    sudo apt-get install jq unzip -y

    curl https://api.github.com/repos/godotengine/godot/releases/latest > /tmp/templates-ver

    cd /tmp/
    url=$(cat /tmp/templates-ver|jq '.assets[] | select (.name | endswith("_mono_export_templates.tpz"))' | jq '.browser_download_url')

    echo "$url"
    echo "$url"|xargs wget -q -O "templates.tpz" -c {}

fi

exit 0