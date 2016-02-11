# Cronical

.NET-based cron daemon. Can replace Windows Services and Scheduled Tasks, typically for running service-like processes as part of an application suite - or just by itself.

## Introduction

Unix systems (Linux) have since long had excellent support for background tasks, scheduled tasks and worker processes. crond has since long been a staple in the Unix world
for scheduling tasks, allowing scripts and programs to be executed using a very simple definition. It's quiet, effective, and very reliable.

Windows have similar features. Scheduled Tasks allows you to schedule jobs, and Windows Services allows you to create programs that run regardless of user logins. However,
programs need to be written especially for the Services model; they are not usually easily run the command line; and they can be tricky to debug. Similarly, Scheduled Tasks
is relatively complex, and I have personally found that jobs sometimes stop running, or don't run at all - with little or no warning.

Thus, Cronical was born - an attempt to create an efficient, flexible, and realiable cron service for Windows; without relying on external libraries or complex deployment
or configuration, create a way to schedule activities, and run services, in a very lightweight and yet reliable manner.

## Deployment

Cronical can run as a service - it's what it does best. It can also run from the command line if need be, although due to some issues with the Windows console model, it
means that it might have trouble sending CTRl-BREAK signals to child processes, and may have to kill them using Process.Kill. Running as a service is best, but not required.

To deploy Cronical, make sure you have .NET 4.5 installed (okay, so there's one dependency, fine) and then copy Cronical.exe and Cronical.exe.config to wherever you need it.
Then adapt the example cronical.dat file to what jobs you need to run.

Start

    cronical.exe --install

to install the service, and then start it up using Windows Services, or simply

    net start cronical

## Configuration

Please see the enclosed file cronical.dat.example for information about how Cronical can be configured. It's pretty well annotated. Also, a cursory review of the crond
manpage for Linux systems might help you understand.

## Why the name "Cronical"?

I think it's funny.
