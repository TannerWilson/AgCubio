Author: Tanner Wilson
Author: Joseph Despain

***RUNNING NOTE*** 

	If the view connects to the server but doesn't seem to draw anything, please rebuild both the server and the view and 
	then run both by navigating to them in the file explorer and running their application files. This should fix the issue.

	For the best and most reliable results, only run from the application files intead of from VS. 

	**********************************
			DATABASE PS9
	**********************************

	*NOTES*

	- There are some default constant values in the Database to allow for testing of functionality without having to play the game.

	- We do not have a primary key in our Player_Info Table. This is due to the fact that the There is the possibility of UID's and 
	player names to be dupilacted. We chose not to have the GameID as the primary due to the fact that each time that we start the server 
	is created the GameID is started at 0. So the GameID's will be unique if the server is never stopped and restarted.

	- Also, we changed the sever code so that viruses no longer work and are treated as normal food.



	**********************************************
				SERVER PROJECT PS8
	**********************************************

*****THINGS NOT WORKING*****

	1. Merging: Players do not merge back together after splitting, but do split correctly. Players also can overlap eachother, 
	but do not eat eachother.

	2. Viruses do not de-spawn nor do they spawn new viruses upon a player colliding with them. We can have them despawn, but 
	this only works without them splitting (to switch between these two states, see below), so we chose to keep the split instead. 
	In extent, hitting the virusses too much within	a short time may cause the game to crash.
		 
	HOW TO FIX/CHANGE: comment out the lines: 
							   string virus = VirusSplit(CubeItem);
                               UpdatedCubes.Add(virus);
	in the update method inside the server, which is found where we are checking for collisions with food. This will alow you to "turn off" 
	the viruses in the game and they will be treated exactly the same as food and will be deleted when the player touches them.

	3. We do not have the ability to change the world constants through an XML document, but the constants do exist, so they can be manipulated
	just not as conviently.

	4. We do not have any unit tests. However, the game and server does work reliably well, so we used that as a "test" for our program.

	

****NOTES****

- When players split, they are propelled a "magic push button" programemd value and often it is too small for the user to see the jump

- Food Implemntation: Every time the server world is updated, we loop through every player connected and look for any and all collisions
and if this happens to be a collision with a food, the mass is grown by 3 and the food is deleted

- Player collisions are handled in the same manner as the food.

- The world is always a set size of 1000x1000 and there is a max food amount of 5000, with 5 viruses. Player speeds and attrition rates vary by size.

- Virus: A virus acts similarly to a bomb that never is erased from the game. When a player hits a virus, they simply split in the same manner
as they would if the "space" key was pressed, making the player easier to be eaten by other players.
