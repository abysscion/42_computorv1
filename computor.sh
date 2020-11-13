#!/bin/bash
if [[ "$OSTYPE" == "msys" ]]; then
  ./computorv1.exe "${@:1}" 
elif [[ "$OSTYPE" == "darwin"* ]]; then
  ./computorv1 "${@:1}"
else
	echo "Can't recognize OSTYPE. Try to run manually!"
fi
