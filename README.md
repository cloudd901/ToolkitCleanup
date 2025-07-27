 # ToolkitCleanup
![Icon](https://github.com/cloudd901/ToolkitCleanup/blob/master/ToolkitCleanup/logo.ico)

Toolkit Cleanup script was made as part of a Computer Toolkit application. The script itself is standalone and can be used on any Windows 7, 8, 10, or 11 device.

 - Designed to scour all corners of multi-user workstations.
 - Can compare dates and remove old account profiles.
 - Includes Command-Line parameters for automation.

Command-Line parameters:

    -?
    -Computer:<computername>
    -User:<username>
    -Pass:<password>
    -Clean:<cleanType>
    <CleanType>:0 = Main Menu Choices,
    <CleanType>:1 = Full Auto Clean
    <CleanType>:2 = Full Prompt Clean
    <CleanType>:3 = Old Profile Removal w/Prompt
    <CleanType>:4 = PC Temp Cleanup
    <CleanType>:5 = User Temp Cleanup
    <CleanType>:6 = PC & User Temp Cleanup
    <CleanType>:7 = Check & Repair Profile Structure

    Example 1: Cleanup all old profiles from current computer.
    ToolkitCleanup.exe -Clean:1

    Example 2: Cleanup PC Temp files from remote computer.
    ToolkitCleanup.exe -Computer:computername -Clean:4 -User:myname -Pass:mypass

Removes files from the following folders:

    "C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\tmp\*"
    "C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\blob_storage\*"
    "C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\Cache\*"
    "C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\IndexedDB\*"
    "C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\GPUCache\*"
    "C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\databases\*"
    "C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Software Center\.cache\*"
    "C:\Users\<USERNAME>\AppData\Local\Temp\*"
    "C:\Users\<USERNAME>\AppData\Local\Cisco\Unified Communications\Jabber\CSF\cef_cache\*"
    "C:\Users\<USERNAME>\AppData\Local\Cisco\Unified Communications\Jabber\CSF\Photo Cache\*"
    "C:\Users\<USERNAME>\AppData\Local\Cisco\Unified Communications\Jabber\CSF\IMP Cache\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Caches\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Explorer\iconcache*.*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Explorer\thumbcache*.*"
    "C:\Users\<USERNAME>\AppData\Local\IconCache.db"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Temporary Internet\*" //Added for pre 8.0
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Cookies\*" //Added for pre 8.0
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\IECompatCache\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\IECompatUaCache\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\IEDownloadHistory\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\INetCookies\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\INetCache\*"
    "C:\Users\<USERNAME>\AppData\Local\Google\Chrome\User Data\Default\Cache\*"
    "C:\Users\<USERNAME>\AppData\Local\Google\Chrome\User Data\Default\Cookies\*"
    "C:\Users\<USERNAME>\AppData\Local\Google\Chrome\User Data\Default\History\*"
    "C:\Users\<USERNAME>\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\AC\*"
    "C:\Users\<USERNAME>\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\LocalCache\*"
    "C:\Users\<USERNAME>\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\AppData\User\Default\CacheStorage\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Edge\User Data\Default\Cache\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Edge\User Data\Default\Cookies\*"
    "C:\Users\<USERNAME>\AppData\Local\Microsoft\Edge\User Data\Default\History\*"
    "C:\Users\<USERNAME>\AppData\Local\Chromium\User Data\Default\Cache\*"
    "C:\Users\<USERNAME>\AppData\Local\Chromium\User Data\Default\GPUCache\*"
    "C:\Users\<USERNAME>\AppData\Local\Chromium\User Data\ShaderCache\*"

    "C:\temp\*"
    "C:\Windows\Downloaded Program Files\*"
    "C:\Windows\LiveKernelReports\*"
    "C:\Windows\Prefetch\*.pf"
    "C:\Windows\Temp\*"
    "C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache\*"
    "C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Logs\*"
    "C:\JavaCache\*"
    "C:\Program Files (x86)\Google\Temp\*"
    "C:\Program Files (x86)\Google\Update\Download\*"
    "C:\Program Files (x86)\Google\UpdateD\Download\*"
    "C:\ProgramData\USOShared\Logs\*"
    "C:\ProgramData\Microsoft\Windows\Caches\*"
    "C:\Windows\SoftwareDistribution\Download\*"
    "C:\Windows\SoftwareDistribution\EventCache.v2\*"
    "C:\Windows\SoftwareDistribution\PostRebootEventCache.V2\*"
