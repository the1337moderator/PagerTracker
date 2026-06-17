Pager Tracker
==============

This app shows pager buttons in a web browser.

When someone clicks a pager button, the button moves to Active and the checkout date/time is shown in the middle list.

When someone clicks the Active button again, it moves back to Available and the checkout line is removed.


How to Build
============

Run this script:

  .\Build-PagerTracker.ps1

When it finishes, the finished app files will be in the publish folder:

  publish\PagerTracker.exe
  publish\PagerTracker.json


How to Run Without Installing
=============================

Run this:

  .\publish\PagerTracker.exe

The app will try to use port 80 first.

If port 80 is not available, it will try ports 5000 through 5099.

The app will print the address it is using, like this:

  PagerTracker is running at http://localhost:80


How to Run on a Specific Port
=============================

Run the EXE with the port number after it.

Example:

  .\publish\PagerTracker.exe 8080

Then open:

  http://localhost:8080


How to Install as a Windows Service
===================================

First build the app:

  .\Build-PagerTracker.ps1

Then run this as Administrator:

  .\Install-PagerTracker.ps1

This copies the app to:

  C:\Program Files\Pager Tracker

It also creates and starts a Windows service named:

  Pager Tracker


How to Uninstall
================

Run this as Administrator:

  .\Install-PagerTracker.ps1 -Uninstall

This stops and removes the Windows service.

It also deletes this folder:

  C:\Program Files\Pager Tracker


How to Change the Buttons
=========================

The button list is in this file:

  source\PagerTracker.json

It looks like this:

  [
    "1",
    "2",
    "3",
    "5",
    "7",
    "8",
    "9",
    "10",
    "11"
  ]

Each line in the list becomes one button.

To add a pager, add a new line in the list.

To remove a pager, delete its line from the list.

After changing the file, run the build script again.

If the app is installed as a Windows service, run the install script again too.

The installed copy of this file is here:

  C:\Program Files\Pager Tracker\PagerTracker.json

You can also change that installed copy directly, then restart the Windows service.


Opening the App from Another Computer
=====================================

If the app is running on a computer with a network address, other computers on the same network can open it in a browser.

Example:

  http://192.168.1.50

If the app is using a custom port, include the port number.

Example:

  http://192.168.1.50:8080

Windows Firewall may need to allow the app or the port.
