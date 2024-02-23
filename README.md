# DANGER! Do not use this project! ☢️

To read CPU information like temperature and power consuption it's necessary to issue very special commands to the CPU. The way to do it is trought a **driver**, a piece of software that has high privileges on the machine.

This project uses the OpenHardwareMonitorLib, a .Net DLL to get access to these CPU internals. This DLL has, embbedded on it, the driver WinRing0_1_2_0 (there are a x86 and x64 version), and install it when it runs with administrative privileges. 
Those embbeded drivers havea an old digital signature and, even on Windows 11, Microsoft allows it to load.

The problem is that this driver has a [nasty security bug](https://medium.com/@matterpreter/cve-2020-14979-local-privilege-escalation-in-evga-precisionx1-cf63c6b95896), 
one that will allow any proccess running on your machine, under whatever security credentials, to read and write, through the driver, to any part of the memory. And so a proccess, if exploited, can give itself high privileges, and at this point it's not your machine anymore.

There are a fix, but it will require that the driver be digitally signed again, and nowadays the signature must be an EV (Extended Verification) that is expensive, requires a company, and my attach legal consequences if something goes wrong. Or that someone disables the
requirement for digitally signed drivers on windows, and that open a new can of worms... You can read more [here](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/issues/984).

It's a shame that Windows don't nativally, thought an API, exposes sensors that already exists on the CPUs, like temperature probes, voltages and power.

So, **don't use this project**. Consider it killed unless some company fixes the code, publishs it, and signs a driver to it.
