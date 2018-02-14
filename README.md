# SmartProber

Small command line application that pulls info from HDDs and SSDs, outputs raw smart values such as reall, pending, power-on hrs, along with proper exit code, for use on RMM dashboards. (Eg. Solarwinds RMM, formally MAX Focus, but should be compatible with many others.) Supports only Windows based workstations and servers Read below for more info. Screenshot of what this script returns when using Solarwinds:

####For update information and discussion, join the Invise Labs Discord: https://discord.gg/gK7NQ7h

####Follow me on Twitter: https://twitter.com/MikeLierman

### How it Works — READ FIRST BEFORE DOWNLOADING
Most RMM solutions only allow you to upload scripts, not .exe files. The pre-made Windows based scripts for monitoring and logging HDD Smart info and sending out alerts, has never worked for me, and has failed to flag dead drives. This is a solution that works, always, without fail. It's pretty dang cool to be able to see hdd info power-on hours, and bad sector count, etc, right from your dashboard. This info is even viewable using the RMM mobile app.

GETTING STARTED
1. Download the latest release (ready folder). https://github.com/MikeLierman/SmartProber/releases. Inside you will see 2 files. SmartProber.exe, and a batch file. 
2. Upload SmartProber.exe to your web server. Services like Dropbox or Mega will not work because you do not have direct DL access.
3. Edit the batch file and point the URL to yours.
4. Test it. Move the batch file to an empty folder. Open Admin CMD, cd to batch file, and execute. Script will check if the SmartProber.exe binary already exists, it it does, it's executed, if not, it's downloaded. Default save/run directory is C:\IT. This can be changed.
5. SmartProber.exe will populate all HDDs and SSDs and all of their associated values, including power-on hours, power cnt, bad sections, pending sectors, etc. After which the script will return an exit code used by your RMM dashboard to determine PASS or FAIL on the "check." If you've done everything correctly, in command prompt you will see a list of HDDs or SSDs and numbers associated for each. If you do not see this, you messed up, go back to step 1.
6. After verifying that you understand how the script functions, go ahead and upload JUST THE BATCH FILE to your RMM dashboard script manager. Deploy it to several machines as a test before deploying to every connected agent.

### Download
https://github.com/MikeLierman/SmartProber/releases

### Important Notes
* While nearly perfect and with the same accuracy as CrystalDisk, the script isn't perfect. Let me know if you run into issues.
* The percentage number is a rough idea of wear level on the drive, not an indicator of failure. Device will show fail even if just 1 bad sector is detected. 1 can spread to thousands in a matter of hours, days, weeks, and there is no way to predict this.
* If a drive is reported as a FAIL, exercise exterme caution! Neither I, or my company, Invise Labs, takes any repsonsibility for your actions or inactions. Always have proper backup systems in place to protect you and your clients from disaster. 

### Known Bugs
* ?

### Planned Features
* ?

### About Us
Check our site http://invi.se/labs for annoucements and other projects. We code scripts and programs to make our lives as IT Professionals easier. 

