# WOWAntiKeylogger
 A stylized system tray minimizable ant-keylogger with a test keylogger built in.

![Main Form](/wowantikeylogger.gif)

A long time ago in an IDE far far away...like maybe Visual Studio 2010 and it was roughly early 2011, I decided I'd create my own anti-keylogger. I used to play World of Warcraft and there was the occasional news blip about someone losing their account through various means. A common exploit at the time was a mod/plugin that would run a keylogger in the background. 

This app is pretty simple. There's a simplistic anti-keylogger and an actual keylogger for testing. The animated GIF pretty much covers the settings. It minimizes to the system tray and stays there in the background. I'm not sure if this would interfere with key combinations in other software. That's a big reason why I never published it until now. I didn't notice problems but I'm not a big key-combo kind of guy. I didn't want random weird support issues. At least with this repo being public you can fork it and modify it as you see fit. If nothing else, the GlobalKeyboardHook class ended up being pretty clean with good comments for people less familiar with PInvoke.