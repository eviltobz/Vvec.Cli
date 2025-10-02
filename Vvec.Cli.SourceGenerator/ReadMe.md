## Examine code syntax in VS
View > Other Windows > Syntax Visualiser
This is installed from the Compiler SDK. 


## Debugging
* Add this element to the PropertyGroup in the csproj file:
		`<IsRoslynComponent>true</IsRoslynComponent>`
* Open properties for SourceGen project in Solution Explorer
* Go to Debug > Open debug launch profiles UI
* Delete existing profile and add a new Roslyn Component one
* Point it to the project using the SourceGen to debug with
* Set Generator as startup project
  * Maybe tweak launchSettings.json to taste, such as profile name
* Debuggerise
