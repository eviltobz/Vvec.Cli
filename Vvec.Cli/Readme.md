# Features to work on

***NOTE***
Remember about the syntax visualiser in Studio & that I should be able to debug the source generators now.

* Config
	* I've got some basic config set up, and plans for a few more validated types, but it might be good to add in some collection type stuff. For Prj I'm thinking that we'll want to have collections of key/value pairs to support the known folder stuff...
	* Maybe have a way to validate config fields in commands that use them to keep it consistent. When you run a command, it can specify which args it'll need to be good...
	* **Manual editing of config to broken json TOTALLY borks startup** can't even start a new manual edit. Need to catch that and offer a manual edit option.
* SourceGenning IoC
	* This looks very in-depth. We have the current CLI app source to look at, but if we need to register classes from other libraries, we'll need to mix source genning with reflection, and it'll be a palaver. 
	* If we try it though, why should we register all the classes/interfaces that we need manually? We explicitly add the commands, and from them we can work out classes & interfaces that we need. SourceGen could then instantly know the concrete classes, and could look to see if there is a single implementation of any interfaces or abstract classes (or sub-classes that implement the concretes that we've found)
	  * If there's only one option, just use it. 
      * If there are multiple options and we've not done an explicit registration, then throw a compiler error saying that an explicit registration is required for the type.
* Command types
	* Currently I've done all multiple sub-command stuff, but for prj I'd want to have no args fire off the default command, but still be able to do the config subcommand if that's chosen...
		* I don't think that there is the concept of a "default" command in System.CommandLine, so maybe if only a single command is registered, the config should be set as an extra arg thingy, eg. prj --config key value or somefink...
		* Or, maybe we could do a quick inspect of the args[] and decide what gets registered based on that...
		* Could have a different Register method for subcommand vs single command. Either based on the method name, or have another type alongside ISubCommand...

* Help Output
	* The Usage section displays the full app name, this should be overridable in the EntryPoint ctor or some method on it at least.

* Debuggy validation stuff
	* I've got a placeholder in the Initialiser for verifying that all registered dependencies can be resolved, but I still need to implement it. Maybe as an extra Vvec.Cli specific switch on the help screen?


* Any source-genning stuff
	* Add a generation count comment to genned files & see how much they get regenned & if I need to be more cache friendly with them (see the Pluralsight source-gen code for reference - chapter 7 - using the semantic model - Understand how the cache works / Implement Equals on the model)
