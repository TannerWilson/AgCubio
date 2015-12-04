Author: Tanner Wilson
Author: Joseph Despain

***RUNNING NOTE***: 
	If the view connects to the server but doesn't seem to draw anything, please rebuild both the server and the view and 
	then run both by navigating to them in the file explorer and running their application files. This should fix the issue

	For the best and most reliable results, only run from the application files intead from VS. 

*****THINGS NOT WORKING*****
	1. Merging: Players do not merge back together after splitting, but do split correctly. Players also can overlap eachother, 
	but do not eat eachother.

	2. Viruses do not de-spawn nor do they spawn new viruses upon a player colliding with them. We can have them despawn, but 
	this only works without the splitting, so we chose to keep the split instead. In extent, hitting the virusses too much within
	a short time may cause the game to crash.

	3. We do not have the ability to change the world constants through an XML document, but the constants do exist, so they can be manipulated
	jsut not as conviently.

	4. We do not have any unit tests. However, the game and server does work reliably well, so we used that as a "test" for our program.

	

****NOTES****
- When players split, they are propelled a "magic push button" programemd value and often it is too small for the user to see the jump

- Food Implemntation: Every time the server world is updated, we loop through every player connected and look for any and all collisions
and if this happens to be a collision with a food, the mass is grown by 3 and the food is deleted

- Player collisions are handled in the same manner as the food.

- The world is alwas a set size of 1000x1000 and there is a max food amount of 5000, with 5 viruses.

- Virus: A virus acts similarly to a bomb that never is erased from the game. When a player hits a virus, they simply split in the same manner
as they would if the "space" key was pressed, making them easier to be eaten by other players.
