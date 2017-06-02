#!/bin/sh

#This script updates this repo to allow the repo to be pushed to github and sets up upstream and forked urls.
dr="`dirname \"$0\"`"
dr="`( cd \"$dr\" && pwd )`"

read -p "Please enter your github username:" githubUser

	echo "Configuring Git Extensions repo urls."

		echo "Removing remotes"
		git remote remove origin
		git remote remove upstream
		echo "Adding remotes"
		git remote add origin "https://github.com/$githubUser/gitextensions.git"
		git remote set-url --push origin "git@github.com:$githubUser/gitextensions.git"
		git remote add upstream https://github.com/gitextensions/gitextensions.git
		git remote set-url --push upstream https://github.com/gitextensions/gitextensions.git
	if [ -e "GitExtensionsDoc/.git" ]; then
		echo "Configuring Git Extensions Doc repo urls."
		cd GitExtensionsDoc/
		echo "Removing remotes"
		git remote remove origin
		git remote remove upstream
		echo "Adding remotes"
		git remote add origin "https://github.com/$githubUser/GitExtensionsDoc.git"
		git remote set-url --push origin "git@github.com:$githubUser/GitExtensionsDoc.git"
		git remote add upstream https://github.com/gitextensions/GitExtensionsDoc.git
		git remote set-url --push upstream https://github.com/gitextensions/GitExtensionsDoc.git
        	fi
	cd  $dr
	if [ -e "GitExtensionsTest/.git" ]; then
		echo "Configuring Git Extensions Test repo urls."
		cd /GitExtensionsTest/
		echo "Removing remotes"
		git remote remove origin
		git remote remove upstream
		echo "Adding remotes"
		git remote add origin "https://github.com/$githubUser/gitextensionstest.git"
		git remote set-url --push origin "git@github.com:$githubUser/gitextensionstest.git"
		git remote add upstream "https://github.com/gitextensions/gitextensionstest.git"
		git remote set-url --push upstream "https://github.com/gitextensions/gitextensionstest.git"
	fi
	cd  $dr
	if [ -e "Externals/Git.hub/.git" ]; then
		echo "Configuring Git Extensions Git.Hub repo urls."
		cd Externals/Git.hub/
		echo "Removing remotes"
		git remote remove origin
		git remote remove upstream
		echo "Adding remotes"
		git remote add origin "https://github.com/$githubUser/Git.hub.git"
		git remote set-url --push origin "git@github.com:$githubUser/Git.hub.git"
		git remote add upstream "https://github.com/gitextensions/Git.hub.git"
	fi
    cd  $dr
	if [ -e "Externals/NBug/.git" ]; then
		echo "Configuring Git Extensions NBug repo urls."
		cd Externals/NBug/
		echo "Removing remotes"
		git remote remove origin
		git remote remove upstream
		echo "Adding remotes"
		git remote add origin "https://github.com/$githubUser/NBug.git"
		git remote set-url --push origin "git@github.com:$githubUser/NBug.git"
		git remote add upstream "https://github.com/gitextensions/NBug.git"
	fi

cd $dr
git submodule foreach --recursive git remote -v update