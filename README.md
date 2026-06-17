# Gravity Simulator
 
A 2D physics simulation built in C# with Windows Forms. Sixteen balls bounce inside a circular container under gravity, with full elastic collision response between balls and against the container wall.
 
## Screenshot
 
<img src="screenshot.png" width="100%">
## The interesting part
 
The window itself is part of the simulation. I track the window's position every frame, derive its velocity, then its acceleration, and apply the inverse of that acceleration to every ball as a pseudo force. Drag the window fast in one direction and the balls react like the table under them just got pulled. Same physics you feel in a car taking a sharp turn, just simulated instead of felt.
 
## How it works
 
* Gravity pulls every ball down each tick.
* Window movement is measured with a Stopwatch and converted into acceleration, clamped so a sudden window snap does not blow up the simulation.
* Ball vs ball collisions use radius based mass (area scaling) and resolve with impulse along the collision normal.
* Ball vs container collisions reflect velocity off the container normal with a restitution coefficient of 0.85.
## Running it
 
Open the project in Visual Studio or run it with the .NET CLI:
 
```
dotnet run
```
 
Requires .NET with Windows Forms support. Windows only.
 
## Possible next steps
 
* Variable gravity direction.
* Mouse drag to fling balls.
* Trail rendering for velocity visualization.
 
