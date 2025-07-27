#!/bin/bash

sudo apt-get install jq unzip -y

if [ ! -d /tmp/engine-godot ]; then

    curl https://api.github.com/repos/godotengine/godot/releases/latest > /tmp/engine-ver

    cd /tmp/
    url=$(cat /tmp/engine-ver|jq '.assets[] | select (.name | endswith("_mono_linux_x86_64.zip"))' | jq '.browser_download_url')

    echo "$url"
    echo "$url"|xargs wget -q -O "godot.zip" -c {}

    unzip /tmp/godot.zip
    mv /tmp/Godot_* /tmp/engine-godot
    mv /tmp/engine-godot/Godot_* /tmp/engine-godot/godot

    rm /tmp/engine-ver
    rm /tmp/godot.zip
fi

if [ ! -d /tmp/templates ]; then

    curl https://api.github.com/repos/godotengine/godot/releases/latest > /tmp/templates-ver

    cd /tmp/
    url=$(cat /tmp/templates-ver|jq '.assets[] | select (.name | endswith("_mono_export_templates.tpz"))' | jq '.browser_download_url')

    echo "$url"
    echo "$url"|xargs wget -q -O "templates.tpz" -c {}

    unzip /tmp/templates.tpz
    
    rm /tmp/templates-ver
    rm /tmp/templates.tpz
fi

exit 0