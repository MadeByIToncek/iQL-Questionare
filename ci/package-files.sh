#!/bin/bash

sudo apt-get install p7zip-full -y

mkdir results
7z a ./results/questionare.7z ./build/Questionare.exe
7z a ./results/questionare.zip ./build/Questionare.exe 