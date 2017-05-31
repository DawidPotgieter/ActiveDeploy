## Why
The aim is not to produce a setup package like an msi, but rather to encapsulate the steps needed to deploy the different components of a (usually) business solution.  How many times have you spent hours to produce detailed deployment instructions for someone else to follow, simply to have them miss a step and then have to spend hours to fix.

Instead, why not build the steps into a logical workflow - which would in most cases simply be sequential steps, and then have the documentation read "Press the 'Start' button".  Active aims to make your life just that tad easier, and it does if you spend a bit of time upfront.

## What
The project consists of 3 main binaries, a builder (workflow editor), the activities assembly and a workflow execution engine, all based on WF4.  It's built specifically to easily add more activities (you just add a class basically, and away you go).  You can also use active as a lightweight build engine if you don't have TFS/some other automated build engine - more on that later.

## What tasks are currently available?
This list grows as needed...

>![](https://github.com/DawidPotgieter/ActiveDeploy/blob/master/docs/Home_Activities.PNG) 
* CopyFolder (Copy folder and optionally exclude files)
* CreateEventLog (Create/Ensure existence of event log and source)
* CreateFolder
* CreateSimpleScheduledTask (Windows Task Scheduler)
* CreateSqlDbUser (Add SQL server login to database)
* CreateSqlLogin (Create SQL server windows/sql login)
* CreateUser (Create local/domain user account)
* DeleteFilesAndFolders (Allows deleting of files and folders with a filter)
* EncryptConfigSection (Encrypt a section of the app/web.config file)
* ExecuteSqlNonQuery (Execute any SQL on a SQL Server instance that does not return data)
* ExecuteXaml (Allows partitioning and re-use of workflow parts)
* InvokeProcess (Invoke a process - either command line, shellexecute (e.g open url) or separate uncontrolled window)
* MessageBox
* MSBuild (Only supports projects atm, not solutions)
* MSBuildCmd (A simple MSBuild.exe command line wrapper)
* MSDeploy - Requires Web deploy to be installed on target machine
* ReadAppConnectionString (Read connection string from active.run.exe)
* ReadAppSetting (Read app setting from active.run.exe)
* ReadRegistryValue (Read a value from the registry)
* RunScheduledTask (Windows Task Scheduler)
* SetFolderPermissions
* SetServiceStartupMode
* SimpleSendMail
* SqlPackage (Database Project deploy for SSDT) - Requires SqlPackage.exe to be available on execution machine ([http://msdn.microsoft.com/en-us/library/hh550080(v=VS.103).aspx](http://msdn.microsoft.com/en-us/library/hh550080(v=VS.103).aspx))
* StartService
* StopService
* UnZip
* Vsdbcmd (Database Project deploy for VS2010 prior to SSDT) - Requires Vsdbcmd.exe to be available on execution machine
* WriteExceptionTrace (Writes a full trace of an exception to activity console)
* WriteLine (Writes a line of text to the runtime console)
* Zip (Add files/folders to create Zip archive)


## That looks like PowerShell?
You would indeed be doing similar things if you used PowerShell - more powerful but less user friendly.  However, I've found that most developers would rather write documentation than try to write powershell scripts (yikes).  The GUI builder that you can use on the deployment environments with only .NET 4 installed helps a lot.  Plus it's quite simple to add tasks to do what you want it to.

![](https://github.com/DawidPotgieter/ActiveDeploy/blob/master/docs/Home_ss1.PNG)
