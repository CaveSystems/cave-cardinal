# cave-cardinal
Windows service that allows to run any console program as slave processes.

There is a crontab like part that can run programs or scripts on a timely basis and always on slaves, that will be immediately restarted upon exit.

## Configuration

Place the cave-cardinal.ini into the programs directory. 

Sample configuration:

```ini
[Service]
# Service section. This part contains settings for the windows service.
# Be aware that you should not change these settings after --install.

Name = cave-cardinal
# Indicates the name used by the system to identify this service. 

Title = CaveSystems Cardinal Service
# Indicates the friendly name that identifies the service to the user.

Description = Master service allowing other programs to be run as service.
# The description of the service.

StartMode = Automatic
# Indicates the start mode of the service.

DelayedAutoStart = true
# Indicates whether the service should be delayed from starting until other automatically started services are running.

Account = LocalSystem
# Specifies a service's security context, which defines its logon type.
# LocalService   : An account that acts as a non-privileged user on the local computer, and presents anonymous credentials to any remote server.
# NetworkService : An account that provides extensive local privileges, and presents the computer's credentials to any remote server.
# LocalSystem    : An account, used by the service control manager, that has extensive privileges on the local computer and acts as the computer on the network.
# User           : An account defined by a specific user on the network. Specifying User for the Account member causes the system to prompt for a valid user name and password when the service is installed, unless you set values for both the Username and Password properties.

Username = 
# User account under which the service application will run.
# This setting is only supported with Account = User and it may cause a security risk to set this property.

Password =
# Password associated with the user account under which the service application runs.
# This setting is only supported with Account = User and it may cause a security risk to set this property.

[Slaves]
# Section defining the always on slave processes.
# Add names for your services here. Each name defines the name of a section to load.
# Names are not case sensitive.

Test1
Test2

[Slave:Test1]
Filename = %comspec%
# Filename to start for the slave process.

Arguments = /c timeout /t 10
# Commandline arguments for the slave process.

WorkingDirectory = %windir%
# Working directory the slave is started at.

Timeout = 
# Timeout when waiting for slave exit or output (stderr, stdout). After exceeding the timeout the slave will be killed.
# Leave this empty to avoid timeout.

[slave:test2]
filename = c:\windows\system32\cmd.exe
arguments = /c timeout /t 11
workingdirectory = 
timeout = 

[Cron]
# Cron like section to start programs with time definition
#
# @yearly (or @annually): Run once a year  - equals: 0 0 1 1 *
# @monthly:               Run once a month - equals: 0 0 1 * *
# @weekly:                Run once a week  - equals: 0 0 * * 0
# @daily (or @midnight):  Run once a day   - equals: 0 0 * * *
# @hourly:                Run once an hour - equals: 0 * * * *
#
# .---------------- minute (0 - 59)
# |   .------------- hour (0 - 23)
# |   |   .---------- day of month (1 - 31)
# |   |   |   .------- month (1 - 12) OR jan,feb,mar,apr ... 
# |   |   |   |  .----- day of week (0 - 6) (Sunday=0 or 7)  OR sun,mon,tue,wed,thu,fri,sat
# |   |   |   |  |
# *   *   *   *  *  command to be executed
#
# example:
# 10  */5 *   *  mon  %comspec% /c echo "This is run every monday at 0:10, 5:10, 10:10, 15:10, 20:10"
*/2 * * * * %comspec% /c echo "This is run every even minute"
```

## Getting started
Do test your configuration and debug output of your processes you can run the service as commandline program.

```cmd
cave-cardinal.exe --run [--verbose] [--debug] [--logfile=file.log]
```

## (Un-)Installing/starting/stopping the service

It is very important that you do not change the the ini section [Service] when the service is already installed, since the registered service name cannot be changed after installation.

**To rename the service, uninstall it, update the configuration and then install it again.**

The service can be installed, started, stopped and uninstalled at the command line by using commandline switches:

```cmd
cave-cardinal.exe --install [--verbose] [--debug] [--logfile=file.log]
```

```cmd
cave-cardinal.exe --uninstall [--verbose] [--debug] [--logfile=file.log]
```

```cmd
cave-cardinal.exe --start [--verbose] [--debug] [--logfile=file.log]
```

```cmd
cave-cardinal.exe --stop [--verbose] [--debug] [--logfile=file.log]
```
