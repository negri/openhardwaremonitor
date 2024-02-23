# DANGER! Do not use this project! ☢️

To read CPU information such as temperature and power consumption, it is necessary to issue very specific commands to the CPU.
This is done through a **driver**, which is a piece of software that has high privileges on the machine.

This project utilizes OpenHardwareMonitorLib, a .NET DLL, to gain access to these CPU internals.
Embedded within this DLL is the WinRing0_1_2_0 driver (available in both x86 and x64 versions), which is installed when the DLL is run with administrative privileges.
Despite their outdated digital signatures, these embedded drivers are still allowed to load on Windows 11, according to Microsoft's policies.

However, this driver contains a [significant security vulnerability](https://medium.com/@matterpreter/cve-2020-14979-local-privilege-escalation-in-evga-precisionx1-cf63c6b95896), 
which permits any process running on the machine, regardless of security credentials, to read and write to any part of the memory through the driver.
Consequently, a compromised process could escalate its privileges, effectively taking over the machine.

A fix is available, but it requires the driver to be digitally re-signed. Currently, a digital signature must be an EV (Extended Verification), which is costly,
demands corporate backing, and may entail legal liabilities if anything goes awry. Alternatively, one could disable the requirement for digitally signed drivers on Windows,
but this approach introduces its own set of risks. Further details can be found [here](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/issues/984).

It is unfortunate that Windows does not natively expose CPU sensors, such as temperature probes, voltages, and power, through an API.

Therefore, **do not use this project**. Consider it discontinued unless a company undertakes to fix the code, publishes it, and signs the driver.

