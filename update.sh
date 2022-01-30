#!/bin/bash

# git
git pull
git submodule update --init --recursive

# dotnet
dotnet build -c Release

