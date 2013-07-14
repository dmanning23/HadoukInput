HadoukInput
===========

HadoukInput is a library for sanitizing controller input and pattern matching.

This is written in CSharp, and uses the XNA/Monogame libraries.  It is currently tested in Windows and Ouya, but would be easy to use on any platform supported by XNA and MonoGame.

## features of this library

* abstracts left/right input into forward/back, depending on which way the character is facing.
* combines keystrokes like "punch + up" into a single keystroke "PunchUp" (just like Smash Brothers input)
* combines left thumbstick and dpad into one input stream.
* some simple networking stuff for sending controller states over a socket
* Keeps track of when buttons are pressed, if they are being held, and when they are released.
* Given an xml file with a list of moves, will parse controller input looking for patterns that match.  For example, this xml file would load a move list with a single Hadouken move:

```xml
<?xml version="1.0"?>
<XnaContent>
	<Asset Type="HadoukInput.MoveListXML">
		<moves>
			<Item type="SPFSettings.MoveXML">
				<name>Hadouken</name>
				<keystrokes>
					<Item Type="string">Down</Item>
					<Item Type="string">Forward</Item>
					<Item Type="string">A</Item>
				</keystrokes>
			</Item>
		</moves>
	</Asset>
</XnaContent>

```

## Some notes

This library puts the thumbsticks in a square gate, and sanitizes all thumbstick and dpad input to up, down, forward, and back.  If you still need to get thumbstick directions, it supports several methods of scrubbing thumbstick input:
* Axial: Normalized Tile-based (4-way) movement. The Axial Dead Zone actually works well here since it snaps analog input to the only four input vectors that are actually relevant.
* Radial: normalized direciont-centric movement. radial works well here since it is a very small area in the center of the stick within which input is ignored.
* ScaledRadial: as you push the stick away from the center, the gradient value changes smoothly while the dead zone is still preserved.
* Powercurve: like scaled radial, but the small is smaller.  works good for sneaking games etc. where you want a definite analog feel 
https://blogs.msdn.com/b/shawnhar/archive/2007/03/30/massaging-thumbsticks.aspx?Redirected=true

Input is buffered for a split second so it can be combined into single keystrokes, so button+direction presses don't have to be exact

The pattern matching is very forgiving, something like "down, back, forward, A-button" will still throw a hadouken.  It does this because it will also insert extra input like "DownRelease", "ForwardRelease", or might combine forward + A-button into "AForward".  It's better to be lenient anyway, "Be strict in what you send, but generous in what you receive."

Note: This lib uses a couple of submodules, so make sure to run "git submodule init; git submodule update" after cloning.

If you'd like to see an example of how to use this library, check out a few different samples:

https://github.com/dmanning23/RawControllerWrapperSample

https://github.com/dmanning23/ControllerWrapperTest

https://github.com/dmanning23/HadoukInputSample

## License

MIT