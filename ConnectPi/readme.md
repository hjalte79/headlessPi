# headless pi
this is a tool that helps to setup a connection to a raspberry pi that will bu used headelass (without a screen). There are 2 main functions. 

## Setting up the sd-card
When a rasbian image is written to a sd-card and used for starting a raspberry pi for the first time. Rasbian will look for a couple of configuration files. This files can be used to start a wifi connection and enable ssh without a loging in to linux. this will only work at the first start-up.
With those files it's also possible to setup a OTG connection but that has not been implemented
## Finding and connecting to the raspberry pi.
This tool will send a ping requst to all 255 ip-adresses in a range. for example it will ping all adresses from 192.168.2.1 till 192.168.2.255. all ip adress that respond will be listed with there name (if it's known) 
# Use
### Setup
First have a look at the last tab (Setup). 
 - fillin the path to you coppy of putty. If you leave this blank the putty button wont work.
 - Fillin the user name that is used to login at the raspberry pi. Putty will use this name to log in. if you leaf this blank you will need to fill in it evry login into linux.
 - fill in the iprange. this are the first 3 numbers from your local ip network that your pi is connetced to. 
### Setup sd-card
...
### Network search
...
### Installation
Copy the files 
 - headlessPi.exe
 - config.xml

to a directory.


# Todos

 - finish this file
 - enable OTG
 - chech if it's possible ot change a wifi afther first start
 - ...


