## Safety Warning
This plugin functions by using player Content IDs to query local databases, and if that fails, makes a request to the game servers with that content ID. First it requests the Character Card information of the player, but if the card is not created or private will then request "friend" information of the player (this does not require the player to be a friend).

Manipulation of packets is the easiest detection vector Square Enix has in order to categorise players into those who use plugins. Although there has been no evidence that this results in a ban, it may be obvious the player is doing something unexpected (requesting friend information of a non-friend).

## Description
This plugin allows a user to see what players are already in the PF without joining. It uses the PlayerTrack's database, its own database, Adventurer Plate packets and Friend Info packets to relate Content IDs of players with their respective name and world disabling the requirement of fetching and posting data to and from an external database.

## Motive
PFRadar is a closed-source and heavily obfuscated Dalamud plugin that gets player information from a Chinese VPS to relate the retrieved content_ids from party finder packets to player names and worlds. Although the plugin states the data is anonymised and only data required is retrieved, it is a threat to the security and privacy of the user. Even with reverse engineering techniques, understanding the underlying mechanism of PFRadar is very difficult. 

This plugin allows for the same functionality as PFRadar, however can be slower due to sourcing data from different points depending on conditions (first searches locally, then checks game packets).

```plaintext
https://raw.githubusercontent.com/Ryan-RH/MyDalamudPlugins/main/pluginmaster.json