#!/bin/bash
if [[ "$OSTYPE" == "msys" ]]; then
  echo -e "\e[37m[Building executable...]\e[2m"
	dotnet publish -c Release -r win10-x64 -p:PublishSingleFile=True --self-contained True
	echo -e "\e[0;37m[Moving executable here...]\e[2m"
	find . -wholename "*Release/*/publish/computorv1.exe" -exec mv -t . {} \;
	echo -e "\e[1;92m[Build complete!]\e[0m"
elif [[ "$OSTYPE" == "darwin"* ]]; then
  echo -e "\e[37m[Building executable...]\e[2m"
	dotnet publish -c Release -r osx-x64 -p:PublishSingleFile=True --self-contained True
	echo -e "\e[0;37m[Moving executable here...]\e[2m"
	find . -wholename "*publish\computorv1" -exec mv -t . {} \;
	echo -e "\e[37m[Setting executable rights...]\e[2m"
	chmod +x computorv1
	echo -e "\e[0;1;92m[Build complete!]\e[0m"
else
	echo "Can't recognize OSTYPE. Try to build manually!"
fi
