# Ambinity

[![Join the chat at https://gitter.im/Ambinity/community](https://badges.gitter.im/Ambinity/community.svg)](https://gitter.im/Ambinity/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

![Ambino Application logo](adrilight/zoe.ico)

> An Application to control All RGB LED products and OpenRGB Support Product with Ambilight and effect feature

## What does it do?

* independently controll led zones ( screen LED, Desk LED, Case LED) with single output or multiple output devices (RGB HUB)
* Create your own color and music reaction style (unlimited possibility)

## Things to do
* Bring new matrix LED control to the application, now you can controll n Fans or n LED strips (each has 16 individual LED) as 16xn matrix
* Separately add new devices and control interface for each device according to the device name
* Convert all Effects to matrix UI and send RGB data (3 bytes for each LEDs), then All devices no longer need to decode the serial data on it's own
Please look at the schematic below
* Using Avalonia and FluentAvalonia to make project cross-platform

  
|  "Ambinity" (Ri,Gi,Bi)     | ===========> ( "RGB Controller Device"  [Leds[i]= Light(R,G,B)]) ======> LED Strip   
                


## Thanks
* Thanks goes to [jasonpong](https://github.com/jasonpang) for his [sample code for the Desktop Duplication API](https://github.com/jasonpang/desktop-duplication-net)
* Thanks to Serigo to his Awesome shader Library https://github.com/Sergio0694/ComputeSharp
* Thanks to https://github.com/diogotr7/OpenRGB.NET for making awesome OpenRGB support on C#
* Thanks to HandyControl https://github.com/HandyOrg/HandyControl for such a beautiful WPF UI Library

## Changelog

* Change only commit when Ambino launch a new product
* Minor change is not commited a change until a new product release ( bugs)

### Upcomming Realease [October 2024]
* New Canvas Lighting Mode
* Sync All Device include OpenRGB device
* Auto Detect Ambino Basic Rev2 and Ambino Desk
* Minor bugs fix
