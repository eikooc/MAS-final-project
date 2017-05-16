/*******************************************************\
|             AI and MAS: Competition Server            |
|                        README                         |
\*******************************************************/

	The competition server requires two arguments:
        The -c argument is the same as for server.jar; the command used to invoke the client.
        The -d argument takes a path to a folder with competition levels.
    
    The competition server runs the given client on all levels in the given directory and produces an "output.out" file with the results.
    Rename the "output.out" file to "<group_name>.out" where <group_name> is the name of your group as submitted during group registration.
    Do no modify the output file other than renaming it.
	
	Assuming "cserver.jar", "WinnerClient.class", and a directory "competition_levels/" is in the current directory, the competition server can be run with:
        $ java -jar cserver.jar -d competition_levels/ -c "java WinnerClient"
