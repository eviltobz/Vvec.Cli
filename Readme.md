# Vvec.Cli

This is my very opinionated library for writing CLI apps.

It wraps the MS System.Commandline stuff to work in a way that I find more pleasing. It has a bunch of functionality for writing to and reading from the console, including simple ways to add some colour formatting. It dabbles with source generators for doing some startup stuff more efficiently that you could with reflection, and has some simple config handling too. It's not exactly production-quality code, and due to the heavy UI interaction there's not great test coverage.

The Vvec.Prj project uses it to create a simple command line tool for navigating a project folder that contains several git repos.
