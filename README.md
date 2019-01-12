# Cronical

* Current (stable) release version: 1.2
* Master branch version: 1.3 (beta)

.NET-based cron daemon. Can replace Windows Services and Scheduled Tasks, typically for running service-like processes as part of an application suite - or just by itself.

## Introduction

Unix systems (Linux) have since long had excellent support for background tasks, scheduled tasks and worker processes. crond has since long been a staple in the Unix world for scheduling tasks, allowing scripts and programs to be executed using a very simple definition. It's quiet, effective, and very reliable.

Windows have similar features. Scheduled Tasks allows you to schedule jobs, and Windows Services allows you to create programs that run regardless of user logins. However, programs need to be written especially for the Services model; they are not usually easily run from the command line; and they can be tricky to debug. Similarly, Scheduled Tasks is relatively complex, and I have personally found that jobs sometimes stop running, or don't run at all - with little or no warning.

Thus, Cronical was born - an attempt to create an efficient, flexible, and realiable cron service for Windows; without relying on external libraries or complex deployment or configuration, create a way to schedule activities, and run services, in a very lightweight and yet reliable manner.

## Deployment

Cronical runs best as a service - it's a fire-and-forget kind of program that really just works. (It can also run from the command line, if need be.)

To deploy Cronical, make sure you have .NET 4.7.2 installed (okay, so there's one dependency, fine) and then copy Cronical.exe and Cronical.exe.config to wherever you need it. Then adapt the example cronical.dat file to what jobs you need to run.

Start

    cronical.exe --install

to install the service, and then start it up using Windows Services, or simply

    net start cronical

You may optionally specify an alternative service name and title, which enables you to install several instances of Cronical on the same system. The -c parameter can be used to specify a particular configuration file to use for that service instance.

    cronical.exe --install --service-name Cronical1 --service-title "Cronical 1" -c d:\cron1.dat
    cronical.exe --install --service-name Cronical2 --service-title "Cronical 2" -c d:\cron2.dat
    net start cronical1
    net start cronical2

## Configuration File (cron.dat)

Please see the enclosed file cronical.dat.example for information about how Cronical can be configured. It's pretty well annotated. Also, a cursory review of the crond manpage for Linux systems might help you understand.

Some options that you might want to consider:

* Setting **RunMissedJobs** to true will cause Cronical to write the last execution time to the registry, and when it restarts, it will check to see if it missed any jobs while it was turned off.
* **Timeout** will ensure that jobs will not run longer than a certain number of seconds (3600 is one hour). After that, they will be terminated.
* **Home** sets the home folder for the next job(s); causing them to start in a specific folder. By default, cronical sets the home directory to where the executable was found, but Home= will override this. To reset Cronical to the default operation, simply specify Home= without a particular path.
  
Please note that each job has its own configuration; any directive such as Home= or MailTo= can occur several times in the configuration and will affect the jobs following that directive (until the next directive overrides it).

Any change to the cronical.dat file will be automatically and quickly detected by Cronical and it will reload and reparse the file.

Text starting with # is used for comments and may occur anywhere in the file. To use # in an actual command, escape it with a backslash, e.g. "\#".

## Running services

Cronical is also capable of running services - that is, starting jobs and keeping them running, watching them so they don't disappear (and restarting them if they stop).

    @service d:\services\queue-handler.exe
    
...will cause Cronical to attempt to start the program d:\services\queue-handler.exe; and will keep watching it to make sure that it doesn't disappear. If it does, Cronical will notice this immediately (configurable) and attempt to restart it.

When shutting down, or terminating a service that's no longer needed (due to a configuration file change, for instance), Cronical will use several methods to try to shut down the service, including sending WM_CLOSE and WM_QUIT messages to it, and injecting a CTRL-BREAK into the console process window. If this fails, the service will be terminated forcibly.

## Logging and Output

Cronical, by default, logs to a file named cronical-YYYY-MM.log in the same directory as cronical. This can be changed in the cronical.exe.config file that follows the binary file.

    <appSettings>
      <add key="LogDebug" value="false"/>
      <add key="LogPath" value="."/>
      <add key="LogRetention" value="3"/>
    </appSettings>

Setting LogDebug to true will cause Cronical to log debug output, which may help you track down why certain jobs might not be running; LogPath will set a different path to log to, and LogRetention specifies the number of months to keep log files around before deleting them.

You can also run Cronical in console mode, along with debug output, to see directly what it's doing.

    cronical --console --debug

This will cause it to run directly, without installing as a service, and display all of its output on the screen.

## Is it stable?

Yes, it's in production on servers and easily running hundreds of jobs daily without breaking a sweat; it has in many cases replaced all of our Scheduled Tasks.

## Can it be extended?

We're currently looking into the possibly of allowing plugins to load jobs from other sources (databases), and/or getting notifications when jobs are executed.

## Where does it come from?

It was originally built by [Ciceronen Telecom AB](http://www.ciceronen.com/) in Sweden, and then released as open source under an Apache License.

## Why the name "Cronical"?

We think it's funny.
