spawncamping-wallhack
=====================

> A sample on how a gaming backend on Windows Azure cloud look like. 

This gaming backend consists of two services, a 'lobby service' and the actual 'game server'. To join a game, clients connect via a custom TCP protocol to the lobby service, where matchmaking etc. happens. The client 'waits in the lobby' by keeping the TCP connection to the lobby service open until there is a set of players who are supposed to play together. The lobby service initiates the creation of a game server process on one of the game server machines, and instructs the players to re-connect to the fresh game server process. The game server process then exists for the duration of a game, usually quarter of an hour or so. When gameplay is finished, the game server process gets shut down, and clients can re-connect to the lobby service to await another game. 



```
import foo;
```
