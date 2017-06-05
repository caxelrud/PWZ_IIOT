# iot-samples/FieldGateway
This file includes notes on building the Field Gateway for various architectures and operating systems. 

## Build Notes:
* [Linux](https://github.com/Azure/azure-iot-gateway-sdk/blob/master/doc/devbox_setup.md#set-up-a-linux-development-environment)
* [Windows & Windows IoT Core](https://github.com/Azure/azure-iot-gateway-sdk/blob/master/doc/devbox_setup.md#setting-up-a-windows-development-environment)
* [Raspbian](#raspbian-image-for-raspberrypi)

### Raspbian Image for RaspberryPi
The following notes walk you thorugh building a base Raspbian image for exploring the Field Gateway SDK.  This content is taken partially from [IoT Hub Getting Started for Linux Guide](https://github.com/Azure/azure-iot-gateway-sdk/blob/master/doc/devbox_setup.md#set-up-a-linux-development-environment) and from blog posts by [Jon Gallant](http://blog.jongallant.com/2016/09/raspberry-pi-nodejs-base-image.html).  These notes are not indented to be exhaustive.

* Format card with Windows Disk Management
* Flash Rasbian with Win32DiskImager
* Insert card into RaspberryPI and Powerup 
* Connect to Internet (wired or wireless)
* Install Raspbian Updates
	- `sudo apt update`
	- `sudo apt full-upgrade`
* Enable Remote Access 
	- `sudo apt install -y xrdp`
	- `sudo apt install -y samba`
* Raspberry Main Menu -> Preferences -> RaspberryPi Configuration 
	- Expand Filesystem
	- Change password (`password1`)
	- Change hostname (`raspberrypi-iot`)
	- -> Interfaces
		- Enable all interfaces
	- -> Localization
		- Locale
		- Timezone
		- Keyboard 
* Setup Windows File Share
	- `sudo leafpad /etc/samba/smb.conf &`
	- Add to file
		[PiShare]
		  comment=Raspi Share
		  path=/home/pi/
		  browseable=Yes
		  writeable=Yes
		  only guest=No
		  create mask=0777
		  directory mask=0777
		  public=no
	- sudo smbpasswd -a pi (`password1')
* [Map Network Drive](https://technet.microsoft.com/en-us/library/hh849829.aspx) from development machine
* Install Node.js
	- `curl -sL https://deb.nodesource.com/setup_6.x | sudo -E bash -`
	- `sudo apt install -y nodejs`
* Co-nfigure npm to run without sudo
	-curl -O https://raw.githubusercontent.com/glenpike/npm-g_nosudo/master/npm-g-nosudo.sh
	-chmod +x npm-g-nosudo.sh
	-./npm-g-nosudo.sh
	-source ~/.bashrc 
* In-stall Required IoT Packages
	-`sudo apt-get install libcurl4-openssl-dev cmake libssl-dev uuid-dev valgrind libglib2.0-dev`
	-`y`
* Get Field Gateway repo
	- From home directory `~` -> Create directory /code `mkdir code`
	- `cd code`
	- `git clone --recursive https://github.com/Azure/azure-iot-gateway-sdk.git`
	- `git submodule update --init --recursive 
* Build Node Bindings
	- `cd /tools`
	- `./build_nodejs.sh`
* Add Environment Variables
	- `export NODE_LIB="/home/pi/code/azure-iot-gateway-sdk/build_nodejs/dist/lib"`
	- `export NODE_INCLUDE="/home/pi/code/azure-iot-gateway-sdk/build_nodejs/dist/inc"`
	- `export IOTHUB_CONNECTION_STRING="HostName={hostname}.azure-devices.net;SharedAccessKeyName={keyname};SharedAccessKey={key}"`
	- `export IOTHUB_EVENTHUB_CONNECTION_STRING="Endpoint=sb://{iot-eventhub-name}.servicebus.windows.net/;SharedAccessKeyName={keyname};SharedAccessKey={key}"`
	- `export IOTHUB_EVENTHUB_CONSUMER_GROUP="\$Default"`
	- `export IOTHUB_PARTITION_COUNT=4`
* Build Field Gateway with Nodejs Bindings 
	- `./build.sh --enable-nodejs-binding --skip-unittests --skip-e2e-tests`
* Reboot RaspberryPi